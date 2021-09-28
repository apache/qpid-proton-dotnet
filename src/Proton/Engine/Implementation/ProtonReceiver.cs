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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Proton Receiver link implementation which manages the receipt of new deliveries
   /// and credit associated with the link. Delivery outcomes and settlement states can
   /// also be managed from the receiver link.
   /// </summary>
   public sealed class ProtonReceiver : ProtonLink<IReceiver>, IReceiver
   {
      private Action<IIncomingDelivery> deliveryReadEventHandler = null;
      private Action<IIncomingDelivery> deliveryAbortedEventHandler = null;
      private Action<IIncomingDelivery> deliveryUpdatedEventHandler = null;

      private readonly ProtonSessionIncomingWindow sessionWindow;
      private readonly SplayedDictionary<uint, ProtonIncomingDelivery> unsettled =
         new SplayedDictionary<uint, ProtonIncomingDelivery>();

      private uint? currentDeliveryId;

      private IDeliveryState defaultDeliveryState;
      private ILinkCreditState drainStateSnapshot;

      public ProtonReceiver(ProtonSession session, string name, ProtonLinkCreditState creditState) : base(session, name, creditState)
      {
         sessionWindow = session.IncomingWindow;
      }

      #region Receiver API implementation

      public override uint Credit => CreditState.Credit;

      public override bool IsDraining => drainStateSnapshot != null;

      public override Role Role => Role.Receiver;

      public IDeliveryState DefaultDeliveryState
      {
         get => defaultDeliveryState;
         set => defaultDeliveryState = value;
      }

      public bool HasUnsettled => unsettled.Count > 0;

      public IEnumerable<IIncomingDelivery> Unsettled => new List<IIncomingDelivery>(unsettled.Values);

      public IReceiver AddCredit(uint amount)
      {
         CheckLinkOperable("Cannot add credit");

         if (amount > 0)
         {
            CreditState.IncrementCredit(amount);
            if (IsLocallyOpen && WasLocalAttachSent)
            {
               sessionWindow.WriteFlow(this);
            }
         }

         return this;
      }

      public bool Drain()
      {
         CheckLinkOperable("Cannot drain Receiver");

         if (drainStateSnapshot != null)
         {
            throw new InvalidOperationException("Drain attempt already outstanding");
         }

         if (Credit > 0)
         {
            drainStateSnapshot = CreditState.Snapshot();

            if (IsLocallyOpen && WasLocalAttachSent)
            {
               sessionWindow.WriteFlow(this);
            }
         }

         return IsDraining;
      }

      public bool Drain(uint credits)
      {
         CheckLinkOperable("Cannot drain Receiver");

         if (drainStateSnapshot != null)
         {
            throw new InvalidOperationException("Drain attempt already outstanding");
         }

         uint currentCredit = Credit;

         if (credits < currentCredit)
         {
            throw new ArgumentOutOfRangeException("Cannot drain partial link credit");
         }

         CreditState.IncrementCredit(credits - currentCredit);

         if (Credit > 0)
         {
            drainStateSnapshot = CreditState.Snapshot();

            if (IsLocallyOpen && WasLocalAttachSent)
            {
               sessionWindow.WriteFlow(this);
            }
         }

         return IsDraining;
      }

      public IReceiver Settle(Predicate<IIncomingDelivery> filter)
      {
         return Disposition(filter, null, true);
      }

      public IReceiver Disposition(Predicate<IIncomingDelivery> filter, IDeliveryState state, bool settle)
      {
         CheckLinkOperable("Cannot apply disposition");

         if (filter == null)
         {
            throw new ArgumentNullException("Supplied filter cannot be null");
         }

         IList<uint> toRemove = new List<uint>();

         foreach (KeyValuePair<uint, ProtonIncomingDelivery> delivery in unsettled)
         {
            if (filter.Invoke(delivery.Value))
            {
               if (state != null)
               {
                  delivery.Value.LocalState(state);
               }
               if (settle)
               {
                  delivery.Value.LocallySettled();
                  toRemove.Add(delivery.Key);
               }

               sessionWindow.ProcessDisposition(this, delivery.Value);
            }
         }

         if (toRemove.Count > 0)
         {
            foreach (uint deliveryId in toRemove)
            {
               unsettled.Remove(deliveryId);
            }
         }

         return this;
      }

      public IReceiver DeliveryAbortedHandler(Action<IIncomingDelivery> handler)
      {
         deliveryAbortedEventHandler = handler;
         return this;
      }

      public IReceiver DeliveryReadHandler(Action<IIncomingDelivery> handler)
      {
         deliveryReadEventHandler = handler;
         return this;
      }

      public IReceiver DeliveryStateUpdatedHandler(Action<IIncomingDelivery> handler)
      {
         deliveryUpdatedEventHandler = handler;
         return this;
      }

      #endregion

      protected override void HandleRemoteAttach(Attach attach)
      {
         if (!attach.HasInitialDeliveryCount())
         {
            throw new ProtocolViolationException("Sending peer attach had no initial delivery count");
         }

         CreditState.InitializeDeliveryCount(attach.InitialDeliveryCount);
      }

      protected override void HandleRemoteDetach(Detach detach)
      {
      }

      protected override void HandleRemoteFlow(Flow flow)
      {
         ProtonLinkCreditState creditState = CreditState;
         creditState.RemoteFlow(flow);

         if (flow.Drain)
         {
            creditState.UpdateDeliveryCount(flow.DeliveryCount);
            creditState.UpdateCredit(flow.LinkCredit);
            if (creditState.Credit != 0)
            {
               throw new ArgumentOutOfRangeException("Receiver read flow with drain set but credit was not zero");
            }
            else
            {
               drainStateSnapshot = null;
            }
         }

         FireCreditStateUpdated();
      }

      protected override void HandleRemoteDisposition(Disposition disposition, ProtonOutgoingDelivery delivery)
      {
         throw new InvalidOperationException("Receiver link should never handle disposition for outgoing deliveries");
      }

      protected override void HandleRemoteDisposition(Disposition disposition, ProtonIncomingDelivery delivery)
      {
         bool updated = false;

         if (disposition.State != null && !disposition.State.Equals(delivery.RemoteState))
         {
            updated = true;
            delivery.RemoteState = disposition.State;
         }

         if (disposition.Settled && !delivery.IsRemotelySettled)
         {
            updated = true;
            delivery.RemotelySettled();
         }

         if (updated)
         {
            FireDeliveryUpdated(delivery);
         }
      }

      protected override void HandleRemoteTransfer(Transfer transfer, IProtonBuffer payload, out ProtonIncomingDelivery delivery)
      {
         if (currentDeliveryId != null && (!transfer.HasDeliveryId() || currentDeliveryId.Equals(transfer.DeliveryId)))
         {
            delivery = unsettled[(uint)currentDeliveryId];
         }
         else
         {
            VerifyNewDeliveryIdSequence(transfer, currentDeliveryId);

            delivery = new ProtonIncomingDelivery(this, transfer.DeliveryId, transfer.DeliveryTag);
            delivery.MessageFormat = transfer.MessageFormat;

            unsettled.Add(transfer.DeliveryId, delivery);
            currentDeliveryId = transfer.DeliveryId;
         }

         delivery.IncrementAndGetTransferCount();

         if (transfer.HasState())
         {
            delivery.RemoteState = transfer.DeliveryState;
         }

         if (transfer.Settled || transfer.Aborted)
         {
            delivery.RemotelySettled();
         }

         if (payload != null)
         {
            delivery.AppendTransferPayload(payload);
         }

         bool done = transfer.Aborted || !transfer.More;
         if (done)
         {
            CreditState.DecrementCredit();
            CreditState.IncrementDeliveryCount();
            currentDeliveryId = null;

            if (transfer.Aborted)
            {
               delivery.Aborted();
            }
            else
            {
               delivery.Completed();
            }
         }

         if (transfer.Aborted)
         {
            FireDeliveryAborted(delivery);
         }
         else
         {
            FireDeliveryRead(delivery);
         }

         if (IsDraining && Credit == 0)
         {
            drainStateSnapshot = null;
            FireCreditStateUpdated();
         }
      }

      protected override void HandleDecorateOfOutgoingFlow(Flow flow)
      {
         flow.LinkCredit = Credit;
         flow.Handle = Handle;
         if (CreditState.IsDeliveryCountInitialized)
         {
            flow.DeliveryCount = CreditState.DeliveryCount;
         }
         flow.Drain = IsDraining;
      }

      #region Internal ProtonReceiver APIs

      internal bool HasDeliveryAbortedHandler => deliveryAbortedEventHandler != null;

      internal bool HasDeliveryReadHandler => deliveryReadEventHandler != null;

      internal bool HasDeliveryStateUpdatedHandler => deliveryUpdatedEventHandler != null;

      internal void FireDeliveryAborted(ProtonIncomingDelivery delivery)
      {
         if (delivery.HasDeliveryAbortedHandler)
         {
            delivery.FireDeliveryAborted();
         }
         else
         {
            deliveryAbortedEventHandler?.Invoke(delivery);
         }
      }

      internal void FireDeliveryRead(ProtonIncomingDelivery delivery)
      {
         if (delivery.HasDeliveryReadHandler)
         {
            delivery.FireDeliveryRead();
         }
         else
         {
            deliveryReadEventHandler?.Invoke(delivery);
         }
      }

      internal void FireDeliveryUpdated(ProtonIncomingDelivery delivery)
      {
         if (delivery.HasDeliveryStateUpdatedHandler)
         {
            delivery.FireDeliveryStateUpdated();
         }
         else
         {
            deliveryUpdatedEventHandler?.Invoke(delivery);
         }
      }

      internal override IReceiver FireRemoteOpen()
      {
         if (HasOpenHandler)
         {
            base.FireRemoteOpen();
         }
         else
         {
            if (localAttach.Target is Apache.Qpid.Proton.Types.Transactions.Coordinator)
            {
               if (session.HasTransactionManagerOpenHandler)
               {
                  session.FireRemoteTransactionManagerOpened(new ProtonTransactionManager(this));
                  return this;
               }
               else if (connection.HasTransactionManagerOpenHandler)
               {
                  connection.FireRemoteTransactionManagerOpened(new ProtonTransactionManager(this));
                  return this;
               }
            }

            if (session.HasReceiverOpenEventHandler)
            {
               session.FireRemoteReceiverOpened(this);
            }
            else if (connection.HasReceiverOpenEventHandler)
            {
               connection.FireRemoteReceiverOpened(this);
            }
            else
            {
               // TODO LOG.info("Receiver opened but no event handler registered to inform: {}", this);
            }
         }

         return this;
      }

      internal override IReceiver Self()
      {
         return this;
      }

      internal void DeliveryRead(ProtonIncomingDelivery delivery, uint bytesRead)
      {
         if (AreDeliveriesStillActive())
         {
            sessionWindow.DeliveryRead(delivery, bytesRead);
         }
      }

      internal void Disposition(ProtonIncomingDelivery delivery)
      {
         if (!delivery.IsRemotelySettled)
         {
            CheckLinkOperable("Cannot set a disposition for delivery");
         }

         try
         {
            sessionWindow.ProcessDisposition(this, delivery);
         }
         finally
         {
            if (delivery.IsSettled)
            {
               unsettled.Remove(delivery.DeliveryId);
               delivery.DeliveryTag?.Release();
            }
         }
      }

      private void VerifyNewDeliveryIdSequence(Transfer transfer, uint? currentDeliveryId)
      {
         if (!transfer.HasDeliveryId())
         {
            engine.EngineFailed(
                new ProtocolViolationException("No delivery-id specified on first Transfer of new delivery"));
         }

         sessionWindow.ValidateNextDeliveryId(transfer.DeliveryId);

         if (currentDeliveryId != null)
         {
            engine.EngineFailed(
                new ProtocolViolationException("Illegal multiplex of deliveries on same link with delivery-id " +
                                               currentDeliveryId + " and " + transfer.DeliveryId));
         }
      }

      #endregion
   }
}