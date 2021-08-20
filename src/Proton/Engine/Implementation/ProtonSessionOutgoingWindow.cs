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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Holds Session level credit window information for outgoing transfers from a
   /// Session. The window is constrained by the remote incoming capacity restrictions
   /// or if present outgoing restrictions on pending transfers.
   /// </summary>
   public sealed class ProtonSessionOutgoingWindow
   {
      private readonly ProtonSession session;
      private readonly ProtonEngine engine;
      private readonly ushort localChannel;

      // This is used for the delivery-id actually stamped in each transfer frame of a given message delivery.
      private uint outgoingDeliveryId = 0;

      // Conceptual outgoing Transfer ID value.
      private uint nextOutgoingId = 0;

      // Track outgoing windowing state information in order to stop outgoing writes if the high
      // water mark is hit and restart later once the low water mark is hit.  When outgoing capacity
      // is at the default -1 value then no real limit is applied.  If set to zero no writes are allowed.
      private uint outgoingCapacity = uint.MaxValue;
      private uint outgoingWindowHighWaterMark = int.MaxValue;
      private uint outgoingWindowLowWaterMark = int.MaxValue / 2;
      private uint pendingOutgoingWrites;
      private bool writeable;

      private uint remoteIncomingWindow;
      private uint remoteNextIncomingId = 0;

      private readonly SplayedDictionary<uint, ProtonOutgoingDelivery> unsettled =
        new SplayedDictionary<uint, ProtonOutgoingDelivery>();

      public ProtonSessionOutgoingWindow(ProtonSession session)
      {
         this.session = session;
         this.engine = (ProtonEngine)session.Connection.Engine;
         this.localChannel = session.LocalChannel;
      }

      internal Begin ConfigureOutbound(Begin begin)
      {
         begin.NextOutgoingId = NextOutgoingId;
         begin.OutgoingWindow = OutgoingWindow;

         UpdateOutgoingWindowState();

         return begin;
      }

      /// <summary>
      /// Reads and then increments the outgoing id value leaving it ready for
      /// the next writer to claim.
      /// </summary>
      internal uint ClaimNextDeliveryId => outgoingDeliveryId++;

      internal uint OutgoingCapacity
      {
         get => outgoingCapacity;
         set
         {
            this.outgoingCapacity = value;
            UpdateOutgoingWindowState();
         }
      }

      internal uint RemainingOutgoingCapacity
      {
         get
         {
            // If set to lower value after some writes are pending this calculation could exceed 2GB which we don't
            // want so we ensure it never exceeds that. Then limit the max value to max positive integer value and
            // hold there as it being more than that is a fairly pointless value to try and convey.
            uint allowedWrites = outgoingWindowHighWaterMark - pendingOutgoingWrites;
            uint remaining = allowedWrites * engine.OutboundMaxFrameSize;

            if (outgoingCapacity > Int32.MaxValue || remaining > Int32.MaxValue)
            {
               return Int32.MaxValue;
            }
            else
            {
               return remaining;
            }
         }
      }

      /// <summary>
      /// Convery the writability at this point in time
      /// </summary>
      internal bool IsSendable => writeable;

      #region Private Credit Windowing APIs

      private void UpdateOutgoingWindowState()
      {
         bool oldWritable = writeable;
         uint maxFrameSize = engine.OutboundMaxFrameSize;

         // TODO : How to disable this
         if (outgoingCapacity == 0)
         {
            // At a setting of zero outgoing writes is manually disabled until elevated again to > 0
            outgoingWindowHighWaterMark = outgoingWindowLowWaterMark = 0;
            writeable = false;
         }
         else if (outgoingCapacity > 0)
         {
            // The local end is writable here if the current pending writes count is below the low water
            // mark and also if there is remote incoming window to allow more write.
            outgoingWindowHighWaterMark = Math.Max(1, outgoingCapacity / maxFrameSize);
            outgoingWindowLowWaterMark = outgoingWindowHighWaterMark / 2;
            writeable = pendingOutgoingWrites <= outgoingWindowLowWaterMark && remoteIncomingWindow > 0;
         }
         else
         {
            // User disabled outgoing windowing so reset state to reflect that we are not
            // enforcing any limit from now on, at least not any sane limit.
            outgoingWindowHighWaterMark = Int32.MaxValue;
            outgoingWindowLowWaterMark = Int32.MaxValue / 2;
            writeable = remoteIncomingWindow > 0;
         }

         if (!oldWritable && writeable)
         {
            IEnumerable<ISender> senders = session.Senders;
            foreach (ISender sender in senders)
            {
               ((IProtonLink)sender).HandleSessionCreditStateUpdate(this);
               if (!writeable)
               {
                  break;
               }
            }
         }
      }

      private void HandleOutgoingFrameWriteComplete()
      {
         pendingOutgoingWrites = Math.Max(0, --pendingOutgoingWrites);

         if (!writeable && (writeable = pendingOutgoingWrites <= outgoingWindowLowWaterMark && remoteIncomingWindow > 0))
         {
            IEnumerable<ISender> senders = session.Senders;
            foreach (ISender sender in senders)
            {
               ((IProtonLink)sender).HandleSessionCreditStateUpdate(this);
               if (!writeable)
               {
                  break;
               }
            }
         }
      }

      #endregion

      #region Handle incoming AMQP Performatives

      /// <summary>
      /// Update the session level window values based on remote information.
      /// </summary>
      /// <param name="begin">The remote performative</param>
      /// <returns>The provided remote performative</returns>
      internal Begin HandleBegin(Begin begin)
      {
         remoteIncomingWindow = begin.IncomingWindow;
         return begin;
      }

      /// <summary>
      /// Update the session window state based on an incoming Flow performative
      /// </summary>
      /// <param name="flow">The remote performative</param>
      /// <returns>The remote performative</returns>
      internal Flow HandleFlow(Flow flow)
      {
         if (flow.HasNextIncomingId())
         {
            remoteNextIncomingId = flow.NextIncomingId;
            remoteIncomingWindow = (remoteNextIncomingId + flow.IncomingWindow) - nextOutgoingId;
         }
         else
         {
            remoteIncomingWindow = flow.IncomingWindow;
         }

         writeable = remoteIncomingWindow > 0 && pendingOutgoingWrites <= outgoingWindowLowWaterMark;

         return flow;
      }

      /// <summary>
      /// Update the session window state based on an outgoing Transfer performative
      /// </summary>
      /// <param name="flow">The remote performative</param>
      /// <returns>The remote performative</returns>
      internal Transfer HandleTransfer(Transfer transfer, IProtonBuffer payload)
      {
         return transfer;
      }

      /// <summary>
      /// Update the session window state based on an incoming Disposition performative
      /// </summary>
      /// <param name="flow">The remote performative</param>
      /// <returns>The remote performative</returns>
      internal Disposition HandleDisposition(Disposition disposition)
      {
         uint first = disposition.First;

         if (disposition.HasLast() && disposition.Last != first)
         {
            HandleRangedDisposition(disposition);
         }
         else
         {
            ProtonOutgoingDelivery delivery;

            if (unsettled.TryGetValue(first, out delivery))
            {
               if (disposition.Settled)
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
         ProtonOutgoingDelivery delivery;

         do
         {
            if (unsettled.TryGetValue(first, out delivery))
            {
               if (disposition.Settled)
               {
                  unsettled.Remove(first);
               }
               ((IProtonLink)delivery.Link).RemoteDisposition(disposition, delivery);
            }
         }
         while (index++ != last);
      }

      #endregion

      #region Handlers for Sender link actions that occurs in the window context

      private readonly Disposition cachedDisposition = new Disposition();
      private readonly Transfer cachedTransfer = new Transfer();

      private void HandlePayloadToLargeRequiresSplitFrames(IPerformative performative)
      {
         cachedTransfer.More = true;
      }

      internal bool ProcessSend(ProtonSender sender, ProtonOutgoingDelivery delivery, IProtonBuffer payload, bool complete)
      {
         // For a transfer that hasn't completed but has no bytes in the final transfer write we want
         // to allow a transfer to go out with the more flag as false.

         if (!delivery.IsSettled)
         {
            unsettled.Add(delivery.DeliveryId, delivery);
         }

         try
         {
            cachedTransfer.DeliveryId = delivery.DeliveryId;
            if (delivery.MessageFormat != 0)
            {
               cachedTransfer.MessageFormat = delivery.MessageFormat;
            }
            else
            {
               cachedTransfer.ClearMessageFormat();
            }
            cachedTransfer.Handle = sender.Handle;
            cachedTransfer.Settled = delivery.IsSettled;
            cachedTransfer.DeliveryState = delivery.State;

            do
            {
               // Update session window tracking for each transfer that ends up being sent.
               ++nextOutgoingId;
               ++pendingOutgoingWrites;
               --remoteIncomingWindow;

               writeable = pendingOutgoingWrites < outgoingWindowHighWaterMark && remoteIncomingWindow > 0;

               // Only the first transfer requires the delivery tag, afterwards we can omit it for efficiency.
               if (delivery.TransferCount == 0)
               {
                  cachedTransfer.DeliveryTag = delivery.DeliveryTag;
               }
               else
               {
                  cachedTransfer.DeliveryTag = (IDeliveryTag)null;
               }
               cachedTransfer.More = !complete;

               OutgoingAmqpEnvelope frame = engine.Wrap(cachedTransfer, localChannel, payload);

               frame.PayloadToLargeHandler = HandlePayloadToLargeRequiresSplitFrames;
               frame.FrameWriteCompletionHandler = HandleOutgoingFrameWriteComplete;

               engine.FireWrite(frame);

               delivery.AfterTransferWritten();
            }
            while (payload != null && payload.IsReadable && IsSendable);
         }
         finally
         {
            cachedTransfer.Reset();
         }

         return IsSendable;
      }

      internal void ProcessDisposition(ProtonSender sender, ProtonOutgoingDelivery delivery)
      {
         // Would only be tracked if not already remotely settled.
         if (delivery.IsSettled && !delivery.IsRemotelySettled)
         {
            unsettled.Remove(delivery.DeliveryId);
         }

         if (!delivery.IsRemotelySettled)
         {
            cachedDisposition.First = delivery.DeliveryId;
            cachedDisposition.Role = Role.Sender;
            cachedDisposition.Settled = delivery.IsSettled;
            cachedDisposition.State = delivery.State;

            try
            {
               engine.FireWrite(cachedDisposition, session.LocalChannel);
            }
            finally
            {
               cachedDisposition.Reset();
            }
         }
      }

      internal void ProcessAbort(ProtonSender sender, ProtonOutgoingDelivery delivery)
      {
         cachedTransfer.DeliveryId = delivery.DeliveryId;
         cachedTransfer.DeliveryTag = delivery.DeliveryTag;
         cachedTransfer.Settled = true;
         cachedTransfer.Aborted = true;
         cachedTransfer.Handle = sender.Handle;

         // Ensure we don't track the aborted delivery any longer.
         unsettled.Remove(delivery.DeliveryId);

         try
         {
            engine.FireWrite(cachedTransfer, session.LocalChannel);
         }
         finally
         {
            cachedTransfer.Reset();
         }
      }

      #endregion

      #region Access to internal state useful for tests

      internal uint NextOutgoingId => nextOutgoingId;

      internal uint OutgoingWindow => Int32.MaxValue;

      internal uint RemoteNextIncomingId => remoteNextIncomingId;

      internal uint RemoteIncomingWindow => remoteIncomingWindow;

      #endregion
   }
}