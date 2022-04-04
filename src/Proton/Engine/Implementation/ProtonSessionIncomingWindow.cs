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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Tracks the incoming window and provides management of that window in relation to receiver links.
   /// <para>
   /// The incoming window decreases as Transfer frames arrive and is replenished when the user reads
   /// the bytes received in the accumulated payload of a delivery. The window is expanded by sending
   /// a Flow frame to the remote with an updated incoming window value at configured intervals based
   /// on reads from the pending deliveries.
   /// </summary>
   public sealed class ProtonSessionIncomingWindow
   {
      private static readonly uint DEFAULT_WINDOW_SIZE = int.MaxValue; // biggest legal value

      private readonly ProtonSession session;
      private readonly ProtonEngine engine;

      // User configured incoming capacity for the session used to compute the incoming window
      private uint incomingCapacity = 0;

      // Computed incoming window based on the incoming capacity minus bytes not yet read from deliveries.
      private uint incomingWindow = 0;

      // Tracks the next expected incoming transfer ID from the remote
      private uint nextIncomingId = 0;

      // Tracks the most recent delivery Id for validation against the next incoming delivery
      private uint? lastDeliveryid;

      private uint maxFrameSize;
      private uint incomingBytes;

      private readonly SplayedDictionary<uint, ProtonIncomingDelivery> unsettled =
         new SplayedDictionary<uint, ProtonIncomingDelivery>();

      public ProtonSessionIncomingWindow(ProtonSession session)
      {
         this.session = session;
         this.engine = (ProtonEngine)session.Connection.Engine;
         this.maxFrameSize = session.Connection.MaxFrameSize;
      }

      public uint IncomingCapacity
      {
         get => incomingCapacity;
         set => incomingCapacity = value;
      }

      public uint RemainingIncomingCapacity
      {
         get
         {
            if (incomingCapacity == 0 || maxFrameSize == uint.MaxValue)
            {
               return DEFAULT_WINDOW_SIZE;
            }
            else
            {
               return incomingCapacity - incomingBytes;
            }
         }
      }

      /// <summary>
      /// Initialize the session level window values on the outbound Begin for the parent
      /// session. The begin will be decorated with an incoming window value that matches
      /// the configured state of the session.
      /// </summary>
      /// <param name="begin"></param>
      /// <returns>The begin performative for further processing</returns>
      internal Begin ConfigureOutbound(Begin begin)
      {
         // Update as it might have changed if session created before connection open() called.
         maxFrameSize = session.Connection.MaxFrameSize;

         begin.IncomingWindow = UpdateIncomingWindow();

         return begin;
      }

      /// <summary>
      /// Update the session level window values based on remote information.
      /// </summary>
      /// <param name="begin"></param>
      /// <returns>The begin performative for further processing</returns>
      internal Begin HandleBegin(Begin begin)
      {
         if (begin.HasNextOutgoingId())
         {
            this.nextIncomingId = begin.NextOutgoingId;
         }

         return begin;
      }

      /// <summary>
      /// Update the session window state based on an incoming Flow performative
      /// </summary>
      /// <param name="flow"></param>
      /// <returns>The Flow performative for further processing</returns>
      internal Flow HandleFlow(Flow flow)
      {
         return flow;
      }

      /// <summary>
      /// Update the session window state based on an incoming Transfer performative
      /// </summary>
      /// <param name="link">Link instance that was the target of the transfer</param>
      /// <param name="transfer">The Transfer performative that was read</param>
      /// <param name="payload">The payload that arrived with the Transfer</param>
      /// <returns></returns>
      internal Transfer HandleTransfer(IProtonLink link, Transfer transfer, IProtonBuffer payload)
      {
         incomingBytes += payload != null ? (uint)payload.ReadableBytes : 0;
         incomingWindow--;
         nextIncomingId++;

         link.RemoteTransfer(transfer, payload, out ProtonIncomingDelivery delivery);

         if (!delivery.IsRemotelySettled && delivery.IsFirstTransfer)
         {
            unsettled.Add(delivery.DeliveryId, delivery);
         }

         return transfer;
      }

      /// <summary>
      /// Update the state of any received Transfers that are indicated in the disposition
      /// with the state information conveyed therein.
      /// </summary>
      /// <param name="disposition"></param>
      /// <returns></returns>
      internal Disposition HandleDisposition(Disposition disposition)
      {
         uint first = disposition.First;

         if (disposition.HasLast() && disposition.Last != first)
         {
            HandleRangedDisposition(disposition);
         }
         else
         {
            if (unsettled.TryGetValue(first, out ProtonIncomingDelivery delivery))
            {
               if (delivery.IsSettled)
               {
                  unsettled.Remove(first);
               }

               ((IProtonLink)delivery.Link).RemoteDisposition(disposition, delivery);
            }
         }

         return disposition;
      }

      private void HandleRangedDisposition(Disposition disposition)
      {
         uint first = disposition.First;
         uint last = disposition.Last;
         bool settled = disposition.Settled;

         uint index = first;

         do
         {
            if (unsettled.TryGetValue(index, out ProtonIncomingDelivery delivery))
            {
               if (settled)
               {
                  unsettled.Remove(index);
               }

               ((IProtonLink)delivery.Link).RemoteDisposition(disposition, delivery);
            }
         }
         while (index++ != last);
      }

      internal uint UpdateIncomingWindow()
      {
         // TODO - need to revisit this logic and decide on sane cutoff for capacity restriction.
         if (incomingCapacity <= 0 || maxFrameSize == uint.MaxValue)
         {
            incomingWindow = DEFAULT_WINDOW_SIZE;
         }
         else
         {
            incomingWindow = (incomingCapacity - incomingBytes) / maxFrameSize;
         }

         return incomingWindow;
      }

      internal void WriteFlow(ProtonReceiver link)
      {
         UpdateIncomingWindow();
         session.WriteFlow(link);
      }

      #region Access to some internal state useful for tests

      public uint IncomingBytes => incomingBytes;

      public uint NextIncomingId => nextIncomingId;

      public uint IncomingWindow => incomingWindow;

      #endregion

      #region Handle sender link actions in the session window context

      private readonly Disposition cachedDisposition = new Disposition();

      internal void ProcessDisposition(ProtonReceiver receiver, ProtonIncomingDelivery delivery)
      {
         if (!delivery.IsRemotelySettled)
         {
            // Would only be tracked if not already remotely settled.
            if (delivery.IsSettled)
            {
               unsettled.Remove(delivery.DeliveryId);
            }

            cachedDisposition.Reset();
            cachedDisposition.First = delivery.DeliveryId;
            cachedDisposition.Role = Role.Receiver;
            cachedDisposition.Settled = delivery.IsSettled;
            cachedDisposition.State = delivery.State;

            engine.FireWrite(cachedDisposition, session.LocalChannel);
         }
      }

      internal void DeliveryRead(ProtonIncomingDelivery delivery, uint bytesRead)
      {
         incomingBytes -= bytesRead;
         if (incomingWindow == 0)
         {
            WriteFlow(delivery.Link);
         }
      }

      internal void ValidateNextDeliveryId(uint deliveryId)
      {
         uint previousId = lastDeliveryid ?? deliveryId - 1;
         if (++previousId != deliveryId)
         {
            session.Connection.Engine.EngineFailed(
                  new ProtocolViolationException("Expected delivery-id " + previousId + ", got " + deliveryId));
         }

         lastDeliveryid = deliveryId;
      }

      #endregion
   }
}