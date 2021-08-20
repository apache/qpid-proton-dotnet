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
   /// Proton Receiver link implementation which manages the receipt of new deliveries
   /// and credit associated with the link. Delivery outcomes and settlement states can
   /// also be managed from the receiver link.
   /// </summary>
   public sealed class ProtonReceiver : ProtonLink<IReceiver>, IReceiver
   {
      private Action<IIncomingDelivery> deliveryReadEventHandler = null;
      private Action<IIncomingDelivery> deliveryAbortedEventHandler = null;
      private Action<IIncomingDelivery> deliveryUpdatedEventHandler = null;
      private Action<IReceiver> linkCreditUpdatedHandler = null;

      private readonly ProtonSessionIncomingWindow sessionWindow;
      private readonly uint? currentDeliveryId;
      private readonly SplayedDictionary<uint, ProtonIncomingDelivery> unsettled =
         new SplayedDictionary<uint, ProtonIncomingDelivery>();

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
            foreach(uint deliveryId in toRemove)
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

      protected override void HandleDecorateOfOutgoingFlow(Flow flow)
      {
         throw new NotImplementedException();
      }

      protected override void HandleRemoteAttach(Attach attach)
      {
         throw new NotImplementedException();
      }

      protected override void HandleRemoteDetach(Detach detach)
      {
         throw new NotImplementedException();
      }

      protected override void HandleRemoteDisposition(Disposition disposition, ProtonOutgoingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      protected override void HandleRemoteDisposition(Disposition disposition, ProtonIncomingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      protected override void HandleRemoteFlow(Flow flow)
      {
         throw new NotImplementedException();
      }

      protected override void HandleRemoteTransfer(Transfer transfer, IProtonBuffer payload, out ProtonIncomingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      internal override IReceiver Self()
      {
         throw new NotImplementedException();
      }

      internal void DeliveryRead(ProtonIncomingDelivery protonIncomingDelivery, long bytesRead)
      {
         throw new NotImplementedException();
      }

      internal void Disposition(ProtonIncomingDelivery protonIncomingDelivery)
      {
         throw new NotImplementedException();
      }
   }
}