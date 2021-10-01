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
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Proton Sender link implementation which manages the state of the Sender end
   /// of an attached link and provides resources for sending deliveries and managing
   /// the state of sent deliveries.
   /// </summary>
   public sealed class ProtonSender : ProtonLink<ISender>, ISender
   {
      private readonly ProtonSessionOutgoingWindow sessionWindow;
      private readonly LinkedSplayedDictionary<uint, ProtonOutgoingDelivery> unsettled =
         new LinkedSplayedDictionary<uint, ProtonOutgoingDelivery>();

      private Action<IOutgoingDelivery> deliveryUpdatedEventHandler = null;

      private uint? currentDeliveryId = null;
      private bool sendable;
      private IDeliveryTagGenerator autoTagGenerator;
      private IOutgoingDelivery current;

      public ProtonSender(ProtonSession session, string name, ProtonLinkCreditState creditState) : base(session, name, creditState)
      {
         this.sessionWindow = session.OutgoingWindow;
      }

      #region Proton Sender API Implementation

      public override Role Role => Role.Sender;

      public override uint Credit => CreditState.Credit;

      public bool IsSendable => sendable && sessionWindow.IsSendable;

      public override bool IsDraining => CreditState.IsDrain;

      public ISender Drained()
      {
         CheckLinkOperable("Cannot report link drained.");

         ProtonLinkCreditState state = CreditState;

         if (state.IsDrain && state.HasCredit)
         {
            uint drained = state.Credit;

            state.ClearCredit();
            state.IncrementDeliveryCount(drained);

            session.WriteFlow(this);

            state.ClearDrain();
         }

         return this;
      }

      public ISender Disposition(Predicate<IOutgoingDelivery> filter, IDeliveryState state, bool settle)
      {
         CheckLinkOperable("Cannot apply disposition");
         if (filter == null)
         {
            throw new ArgumentNullException("Supplied filter cannot be null");
         };

         IList<uint> toRemove = new List<uint>(); ;
         foreach (KeyValuePair<uint, ProtonOutgoingDelivery> entry in unsettled)
         {
            if (filter.Invoke(entry.Value))
            {
               if (state != null)
               {
                  entry.Value.LocalState(state);
               }
               if (settle)
               {
                  entry.Value.LocallySettled();
                  toRemove.Add(entry.Key);
               }

               sessionWindow.ProcessDisposition(this, entry.Value);
            }
         }

         if (toRemove.Count > 0)
         {
            foreach (uint key in toRemove)
            {
               unsettled.Remove(key);
            }
         }

         return this;
      }

      public ISender Settle(Predicate<IOutgoingDelivery> filter)
      {
         Disposition(filter, null, true);
         return this;
      }

      public IOutgoingDelivery Current => current;

      public IOutgoingDelivery Next()
      {
         CheckLinkOperable("Cannot update next delivery");

         if (current != null)
         {
            throw new InvalidOperationException("Current delivery is not complete and cannot be advanced.");
         }
         else
         {
            current = new ProtonOutgoingDelivery(this);
            if (autoTagGenerator != null)
            {
               current.DeliveryTag = autoTagGenerator.NextTag();
            }
         }

         return current;
      }

      public bool HasUnsettled => unsettled.Count > 0;

      public IEnumerable<IOutgoingDelivery> Unsettled => new List<IOutgoingDelivery>(unsettled.Values);

      public IDeliveryTagGenerator DeliveryTagGenerator
      {
         get => autoTagGenerator;
         set => autoTagGenerator = value;
      }

      public ISender DeliveryStateUpdatedHandler(Action<IOutgoingDelivery> handler)
      {
         deliveryUpdatedEventHandler = handler;
         return this;
      }

      #endregion

      #region Proton Link event and state change handlers

      protected override void HandleRemoteAttach(Attach attach)
      {
      }

      protected override void HandleRemoteDetach(Detach detach)
      {
      }

      protected override void HandleRemoteDisposition(Disposition disposition, ProtonIncomingDelivery delivery)
      {
         throw new InvalidOperationException("Sender link should never handle dispositions for incoming deliveries");
      }

      protected override void HandleRemoteDisposition(Disposition disposition, ProtonOutgoingDelivery delivery)
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
            FireDeliveryStateUpdated(delivery);
         }
      }

      protected override void HandleRemoteFlow(Flow flow)
      {
         ProtonLinkCreditState creditState = CreditState;

         creditState.RemoteFlow(flow);

         uint existingDeliveryCount = creditState.DeliveryCount;
         // int casts are expected, credit is a uint and delivery-count is really a uint sequence which wraps, so we
         // just use the truncation and overflows.  Receivers flow might not have any delivery-count, as sender initializes
         // on attach! We initialize to 0 so we can just ignore that.
         uint remoteDeliveryCount = flow.DeliveryCount;
         uint newDeliveryCountLimit = remoteDeliveryCount + flow.LinkCredit;

         uint effectiveCredit = newDeliveryCountLimit - existingDeliveryCount;
         if (effectiveCredit > 0)
         {
            creditState.UpdateCredit(effectiveCredit);
         }
         else
         {
            creditState.UpdateCredit(0);
         }

         if (IsLocallyOpen)
         {
            sendable = Credit > 0 && sessionWindow.IsSendable;

            FireCreditStateUpdated();
         }
      }

      protected override void HandleRemoteTransfer(Transfer transfer, IProtonBuffer payload, out ProtonIncomingDelivery delivery)
      {
         throw new InvalidOperationException("Sender end cannot process incoming transfers");
      }

      protected override void HandleSessionCreditStateUpdates(in ProtonSessionOutgoingWindow window)
      {
         bool previousSendable = sendable;

         sendable = Credit > 0 && sessionWindow.IsSendable;

         if (previousSendable != sendable)
         {
            FireCreditStateUpdated();
         }
      }

      protected override void HandleSessionCreditStateUpdates(in ProtonSessionIncomingWindow window)
      {
      }

      protected override void HandleDecorateOfOutgoingFlow(Flow flow)
      {
         flow.LinkCredit = Credit;
         flow.Handle = Handle;
         flow.DeliveryCount = CreditState.DeliveryCount;
         flow.Drain = IsDraining;
      }

      protected override void TransitionedToLocallyOpened()
      {
         localAttach.InitialDeliveryCount = currentDeliveryId ?? 0;
         sendable = Credit > 0 && sessionWindow.IsSendable;
      }

      protected override void TransitionedToLocallyDetached()
      {
         sendable = false;
      }

      protected override void TransitionedToLocallyClosed()
      {
         sendable = false;
      }

      protected override void TransitionToRemotelyOpenedState()
      {
         sendable = false;
      }

      protected override void TransitionToRemotelyDetached()
      {
         sendable = false;
      }

      protected override void TransitionToRemotelyClosed()
      {
         sendable = false;
      }

      protected override void TransitionToParentLocallyClosed()
      {
         sendable = false;
      }

      protected override void TransitionToParentRemotelyClosed()
      {
         sendable = false;
      }

      #endregion

      #region Internal Proton Sender API

      internal override ProtonSender Self() => this;

      internal bool HasDeliveryStateUpdateHandler => deliveryUpdatedEventHandler != null;

      internal void FireDeliveryStateUpdated(ProtonOutgoingDelivery delivery)
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

      internal override ISender FireRemoteOpen()
      {
         if (HasOpenHandler)
         {
            base.FireRemoteOpen();
         }
         else if (session.HasSenderOpenEventHandler)
         {
            session.FireRemoteSenderOpened((ISender)this);
         }
         else if (connection.HasSenderOpenEventHandler)
         {
            connection.FireRemoteSenderOpened((ISender)this);
         }
         else
         {
            // TODO LOG.info("Sender opened but no event handler registered to inform: {}", this);
         }

         return this;
      }

      internal void Send(ProtonOutgoingDelivery delivery, IProtonBuffer buffer, bool complete)
      {
         CheckLinkOperable("Cannot send when link has become inoperable");

         if (IsSendable)
         {
            if (currentDeliveryId == null)
            {
               currentDeliveryId = sessionWindow.ClaimNextDeliveryId;

               delivery.DeliveryId = (uint)currentDeliveryId;
            }

            if (!delivery.IsSettled && delivery.TransferCount == 0)
            {
               unsettled.Add(delivery.DeliveryId, delivery);
            }

            try
            {
               sendable = sessionWindow.ProcessSend(this, delivery, buffer, complete) && Credit > 0;
            }
            finally
            {
               if (complete && (buffer == null || !buffer.IsReadable))
               {
                  delivery.MarkComplete();
                  currentDeliveryId = null;
                  current = null;
                  CreditState.IncrementDeliveryCount();
                  CreditState.DecrementCredit();

                  if (Credit == 0)
                  {
                     sendable = false;
                     CreditState.ClearDrain();
                  }
               }
            }
         }
      }

      internal void Abort(ProtonOutgoingDelivery delivery)
      {
         CheckLinkOperable("Cannot abort Transfer");

         try
         {
            if (delivery.TransferCount > 0)
            {
               sessionWindow.ProcessAbort(this, delivery);
            }
         }
         finally
         {
            unsettled.Remove(delivery.DeliveryId);
            currentDeliveryId = null;
            current = null;
         }
      }

      internal void Disposition(ProtonOutgoingDelivery delivery)
      {
         if (!delivery.IsRemotelySettled)
         {
            CheckLinkOperable("Cannot set a disposition");
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
            }
         }
      }

      #endregion
   }
}