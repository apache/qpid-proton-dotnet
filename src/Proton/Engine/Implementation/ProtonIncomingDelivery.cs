/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Proton Incoming Delivery implementation that wraps all the details of
   /// managing the read of delivery payload and management of delivery state.
   /// </summary>
   public sealed class ProtonIncomingDelivery : IIncomingDelivery
   {
      private readonly IDeliveryTag deliveryTag;
      private readonly ProtonReceiver link;
      private readonly uint deliveryId;

      private bool complete;
      private uint messageFormat;
      private bool aborted;
      private uint transferCount;
      private long claimedBytes;

      private IDeliveryState defaultDeliveryState;

      private IDeliveryState localState;
      private bool locallySettled;

      private IDeliveryState remoteState;
      private bool remotelySettled;

      private IProtonBuffer payload;
      private ProtonCompositeBuffer aggregate;

      private ProtonAttachments attachments;
      private object linkedResource;

      private Action<IIncomingDelivery> deliveryReadEventHandler = null;
      private Action<IIncomingDelivery> deliveryAbortedEventHandler = null;
      private Action<IIncomingDelivery> deliveryUpdatedEventHandler = null;

      public ProtonIncomingDelivery(ProtonReceiver link, uint deliveryId, IDeliveryTag deliveryTag)
      {
         this.deliveryId = deliveryId;
         this.deliveryTag = deliveryTag;
         this.link = link;
      }

      public IReceiver Receiver => link;

      public IAttachments Attachments => attachments != null ? attachments : attachments = new ProtonAttachments();

      public object LinkedResource
      {
         get => linkedResource;
         set => linkedResource = value;
      }

      public IDeliveryTag DeliveryTag => deliveryTag;

      public IDeliveryState State => localState;

      public IDeliveryState RemoteState
      {
         get => remoteState;
         internal set => remoteState = value;
      }

      public uint MessageFormat
      {
         get => messageFormat;
         internal set => messageFormat = value;
      }

      public IDeliveryState DefaultDeliveryState
      {
         get => defaultDeliveryState;
         set => defaultDeliveryState = value;
      }

      public bool IsPartial => !complete || aborted;

      public bool IsAborted => aborted;

      public bool IsSettled => locallySettled;

      public bool IsRemotelySettled => remotelySettled;

      public uint TransferCount => transferCount;

      public IIncomingDelivery Disposition(IDeliveryState state)
      {
         return Disposition(state, false);
      }

      public IIncomingDelivery Disposition(IDeliveryState state, bool settled)
      {
         if (locallySettled)
         {
            if ((localState != null && !localState.Equals(state)) || localState != state)
            {
               throw new InvalidOperationException("Cannot update disposition on an already settled Delivery");
            }
            else
            {
               return this;
            }
         }

         locallySettled = settled;
         localState = state;
         link.Disposition(this);

         return this;
      }

      public IIncomingDelivery Settle()
      {
         return Disposition(localState, true);
      }

      #region Access API for the Delivery payload

      public long Available => payload?.ReadableBytes ?? 0;

      public IProtonBuffer ReadAll()
      {
         IProtonBuffer result = null;
         if (payload != null)
         {
            long bytesRead = claimedBytes -= payload.ReadableBytes;
            result = payload;
            payload = null;
            aggregate = null;
            if (bytesRead < 0)
            {
               claimedBytes = 0;
               link.DeliveryRead(this, (uint)-bytesRead);
            }
         }

         return result;
      }

      public IIncomingDelivery ReadBytes(IProtonBuffer buffer)
      {
         if (payload != null)
         {
            long bytesRead = Math.Min(payload.ReadableBytes, buffer.WritableBytes);
            payload.CopyInto(payload.ReadOffset, buffer, buffer.WriteOffset, bytesRead);
            if (!payload.IsReadable)
            {
               payload = null;
               aggregate = null;
            }

            bytesRead = claimedBytes -= (uint)bytesRead;
            if (bytesRead < 0)
            {
               claimedBytes = 0;
               link.DeliveryRead(this, (uint)-bytesRead);
            }
         }

         return this;
      }

      public IIncomingDelivery ReadBytes(byte[] target, int offset, int length)
      {
         if (payload != null)
         {
            long bytesRead = Math.Min(payload.ReadableBytes, length);
            payload.CopyInto(payload.ReadOffset, target, offset, bytesRead);
            if (!payload.IsReadable)
            {
               payload = null;
               aggregate = null;
            }

            bytesRead = claimedBytes -= bytesRead;
            if (bytesRead < 0)
            {
               claimedBytes = 0;
               link.DeliveryRead(this, (uint)-bytesRead);
            }
         }

         return this;
      }

      public IIncomingDelivery ClaimAvailableBytes()
      {
         long available = Available;

         if (available > 0)
         {
            long unclaimed = available - claimedBytes;
            if (unclaimed > 0)
            {
               claimedBytes += unclaimed;
               link.DeliveryRead(this, (uint)unclaimed);
            }
         }

         return this;
      }

      #endregion

      #region Event Handler access API

      public IIncomingDelivery DeliveryAbortedHandler(Action<IIncomingDelivery> handler)
      {
         this.deliveryAbortedEventHandler = handler;
         return this;
      }

      public IIncomingDelivery DeliveryReadHandler(Action<IIncomingDelivery> handler)
      {
         this.deliveryReadEventHandler = handler;
         return this;
      }

      public IIncomingDelivery DeliveryStateUpdatedHandler(Action<IIncomingDelivery> handler)
      {
         this.deliveryUpdatedEventHandler = handler;
         return this;
      }

      #endregion

      #region Internal Delivery Management API

      internal ProtonReceiver Link => link;

      internal bool IsFirstTransfer => transferCount <= 1;

      internal uint IncrementAndGetTransferCount() => ++transferCount;

      internal uint DeliveryId => deliveryId;

      internal ProtonIncomingDelivery Aborted()
      {
         aborted = true;

         if (payload != null)
         {
            long bytesRead = payload.ReadableBytes;

            payload = null;
            aggregate = null;

            // Ensure Session no longer records these in the window metrics
            link.DeliveryRead(this, (uint)bytesRead);
         }

         return this;
      }

      internal ProtonIncomingDelivery Completed()
      {
         this.complete = true;
         return this;
      }

      internal ProtonIncomingDelivery RemotelySettled()
      {
         this.remotelySettled = true;
         return this;
      }

      internal ProtonIncomingDelivery LocallySettled()
      {
         this.locallySettled = true;
         return this;
      }

      internal ProtonIncomingDelivery LocalState(IDeliveryState localState)
      {
         this.localState = localState;
         return this;
      }

      internal ProtonIncomingDelivery AppendTransferPayload(IProtonBuffer buffer)
      {
         if (payload == null)
         {
            payload = buffer;
         }
         else if (aggregate != null)
         {
            aggregate.Append(buffer);
         }
         else
         {
            IProtonBuffer previous = payload;

            payload = aggregate = new ProtonCompositeBuffer();

            aggregate.Append(previous);
            aggregate.Append(buffer);
         }

         return this;
      }

      internal bool HasDeliveryAbortedHandler => deliveryAbortedEventHandler != null;

      internal void FireDeliveryAborted() => deliveryAbortedEventHandler?.Invoke(this);

      internal bool HasDeliveryReadHandler => deliveryReadEventHandler != null;

      internal void FireDeliveryRead() => deliveryReadEventHandler?.Invoke(this);

      internal bool HasDeliveryStateUpdatedHandler => deliveryUpdatedEventHandler != null;

      internal void FireDeliveryStateUpdated() => deliveryAbortedEventHandler?.Invoke(this);

      #endregion
   }
}