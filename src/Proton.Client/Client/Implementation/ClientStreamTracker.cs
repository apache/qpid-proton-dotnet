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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientStreamTracker : IStreamTracker
   {
      private readonly ClientStreamSender sender;
      private readonly IOutgoingDelivery delivery;

      private volatile bool remotelySettled;
      private volatile IDeliveryState remoteDeliveryState;

      internal ClientStreamTracker(ClientStreamSender sender, IOutgoingDelivery delivery)
      {
         this.sender = sender;
         this.delivery = delivery;
         this.delivery.DeliveryStateUpdatedHandler(ProcessDeliveryUpdated);
         // TODO this.remoteSettlementFuture = sender.session().getFutureFactory().createFuture();
      }

      public IStreamSender Sender => sender;

      public bool Settled => delivery.IsSettled;

      public IDeliveryState State => delivery.State?.ToClientDeliveryState();

      public bool RemoteSettled => remotelySettled;

      public IDeliveryState RemoteState => remoteDeliveryState;

      public Task<ITracker> SettlementTask => throw new NotImplementedException();

      public IStreamTracker Disposition(IDeliveryState state, bool settle)
      {
         try
         {
            sender.Disposition(delivery, state?.AsProtonType(), settle);
         }
         finally
         {
            if (settle)
            {
               // TODO remoteSettlementFuture.complete(this);
            }
         }

         return this;
      }

      public IStreamTracker Settle()
      {
         try
         {
            sender.Disposition(delivery, null, true);
         }
         finally
         {
            // TODO remoteSettlementFuture.complete(this);
         }

         return this;
      }

      public IStreamTracker AwaitAccepted()
      {
         throw new NotImplementedException();
      }

      public IStreamTracker AwaitAccepted(TimeSpan timeout)
      {
         throw new NotImplementedException();
      }

      public IStreamTracker AwaitSettlement()
      {
         throw new NotImplementedException();
      }

      public IStreamTracker AwaitSettlement(TimeSpan timeout)
      {
         throw new NotImplementedException();
      }

      #region Private tracker APIs

      private void ProcessDeliveryUpdated(IOutgoingDelivery delivery)
      {
         remotelySettled = delivery.IsRemotelySettled;
         remoteDeliveryState = delivery.RemoteState?.ToClientDeliveryState();

         if (delivery.IsRemotelySettled)
         {
            // TODO remoteSettlementFuture.complete(this);
         }

         if (sender.Options.AutoSettle && delivery.IsRemotelySettled)
         {
            delivery.Settle();
         }
      }

      #endregion
   }
}