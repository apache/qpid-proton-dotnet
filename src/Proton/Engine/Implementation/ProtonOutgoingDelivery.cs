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
   /// Outgoing delivery implementation that manges the state and payload
   /// writes for this delivery.
   /// </summary>
   public sealed class ProtonOutgoingDelivery : IOutgoingDelivery
   {
      private readonly ProtonSender link;

      private uint deliveryId = 0;

      private IDeliveryTag deliveryTag;

      private bool complete;
      private uint messageFormat;
      private bool aborted;
      private uint transferCount;

      private IDeliveryState localState;
      private bool locallySettled;

      private IDeliveryState remoteState;
      private bool remotelySettled;

      private ProtonAttachments attachments;
      private Object linkedResource;

      private Action<IOutgoingDelivery> deliveryUpdatedEventHandler = null;

      /// <summary>
      /// Creates the outgoing delivery instance and assigns it to the given link.
      /// </summary>
      /// <param name="link">The parent link</param>
      public ProtonOutgoingDelivery(ProtonSender link)
      {
         this.link = link;
      }

      #region Accessors for the properties of an outgoing delivery

      public ISender Sender => link;

      public IAttachments Attachments => attachments ??= new ProtonAttachments();

      public object LinkedResource
      {
         get => linkedResource;
         set => linkedResource = value;
      }

      public IDeliveryTag DeliveryTag
      {
         get => deliveryTag;
         set
         {
            if (transferCount > 0)
            {
               throw new InvalidOperationException("Cannot change delivery tag once Delivery has sent Transfer frames");
            }

            if (this.deliveryTag != null)
            {
               this.deliveryTag.Release();
               this.deliveryTag = null;
            }

            this.deliveryTag = value;
         }
      }

      public uint MessageFormat
      {
         get => messageFormat;
         set
         {
            if (transferCount > 0 && messageFormat != value)
            {
               throw new InvalidOperationException("Cannot change the message format once Delivery has sent Transfer frames");
            }

            messageFormat = value;
         }
      }

      public byte[] DeliveryTagBytes
      {
         get => deliveryTag.TagBytes;
         set
         {
            if (transferCount > 0)
            {
               throw new InvalidOperationException("Cannot change delivery tag once Delivery has sent Transfer frames");
            }

            if (this.deliveryTag != null)
            {
               this.deliveryTag.Release();
               this.deliveryTag = null;
            }

            this.deliveryTag = new DeliveryTag(value);
         }
      }

      public bool IsPartial => !complete && !aborted;

      public bool IsAborted => aborted;

      public IDeliveryState State => localState;

      public IDeliveryState RemoteState
      {
         get => remoteState;
         internal set => remoteState = value;
      }

      public bool IsSettled => locallySettled;

      public bool IsRemotelySettled => remotelySettled;

      public uint TransferCount => transferCount;

      public override string ToString()
      {
         return "ProtonOutgoingDelivery: { " + deliveryId + ", " + deliveryTag + " }";
      }

      #endregion

      #region Delivery Write and State Management APIs

      public IOutgoingDelivery Disposition(IDeliveryState state)
      {
         return Disposition(state, false);
      }

      public IOutgoingDelivery Disposition(IDeliveryState state, bool settled)
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

         IDeliveryState oldState = localState;

         this.locallySettled = settled;
         this.localState = state;

         // If no transfers initiated yet we just store the state and transmit in the first transfer
         // and if no work actually requested we don't emit a useless frame.  After complete send we
         // must send a disposition instead for this transfer until it is settled.
         if (complete && (oldState != localState || settled))
         {
            try
            {
               link.Disposition(this);
            }
            finally
            {
               TryRetireDeliveryTag();
            }
         }

         return this;
      }

      public IOutgoingDelivery Settle()
      {
         return Disposition(localState, true);
      }

      public IOutgoingDelivery Abort()
      {
         CheckComplete();

         // Cannot abort when nothing has been sent so far.
         if (!aborted)
         {
            locallySettled = true;
            aborted = true;
            try
            {
               link.Abort(this);
            }
            finally
            {
               TryRetireDeliveryTag();
            }
         }

         return this;
      }

      public IOutgoingDelivery StreamBytes(IProtonBuffer buffer)
      {
         return StreamBytes(buffer, false);
      }

      public IOutgoingDelivery StreamBytes(IProtonBuffer buffer, bool complete)
      {
         CheckCompleteOrAborted();
         try
         {
            link.Send(this, buffer, complete);
         }
         finally
         {
            TryRetireDeliveryTag();
         }
         return this;
      }

      public IOutgoingDelivery WriteBytes(IProtonBuffer buffer)
      {
         CheckCompleteOrAborted();
         try
         {
            link.Send(this, buffer, true);
         }
         finally
         {
            TryRetireDeliveryTag();
         }
         return this;
      }

      public IOutgoingDelivery DeliveryStateUpdatedHandler(Action<IOutgoingDelivery> handler)
      {
         this.deliveryUpdatedEventHandler = handler;
         return this;
      }

      #endregion

      #region Internal Proton Outgoing Delivery State APIs

      internal bool HasDeliveryStateUpdatedHandler => deliveryUpdatedEventHandler != null;

      internal void FireDeliveryStateUpdated()
      {
         deliveryUpdatedEventHandler?.Invoke(this);
      }

      internal ProtonSender Link => link;

      internal uint DeliveryId
      {
         get => deliveryId;
         set => deliveryId = value;
      }

      internal ProtonOutgoingDelivery AfterTransferWritten()
      {
         transferCount++;
         return this;
      }

      internal ProtonOutgoingDelivery LocallySettled()
      {
         this.locallySettled = true;
         return this;
      }

      internal ProtonOutgoingDelivery RemotelySettled()
      {
         this.remotelySettled = true;
         return this;
      }

      internal ProtonOutgoingDelivery LocalState(IDeliveryState localState)
      {
         this.localState = localState;
         return this;
      }

      internal ProtonOutgoingDelivery MarkComplete()
      {
         this.complete = true;
         return this;
      }

      #endregion

      #region Private Delivery APIs

      private void TryRetireDeliveryTag()
      {
         if (deliveryTag != null && IsSettled)
         {
            deliveryTag.Release();
         }
      }

      private void CheckComplete()
      {
         if (complete)
         {
            throw new InvalidOperationException("Cannot write to a delivery already marked as complete.");
         }
      }

      private void CheckCompleteOrAborted()
      {
         if (complete || aborted)
         {
            throw new InvalidOperationException("Cannot write to a delivery already marked as complete or has been aborted.");
         }
      }

      #endregion
   }
}