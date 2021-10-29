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
using System.Threading;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.Threading;
using Apache.Qpid.Proton.Client.Utilities;
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Client receiver implementation which provides a wrapper around the proton
   /// receiver link and processes incoming deliveries with options for queueing
   /// with a credit window.
   /// </summary>
   public class ClientReceiver : IReceiver
   {
      private readonly ReceiverOptions options;
      private readonly ClientSession session;
      private readonly string receiverId;
      private readonly FifoDeliveryQueue<ClientDelivery> messageQueue;
      private readonly AtomicBoolean closed = new AtomicBoolean();
      private ClientException failureCause;

      private Engine.IReceiver protonReceiver;

      private volatile ISource remoteSource;
      private volatile ITarget remoteTarget;

      internal ClientReceiver(ClientSession session, ReceiverOptions options, String receiverId, Engine.IReceiver receiver)
      {
         this.options = options;
         this.session = session;
         this.receiverId = receiverId;
         this.protonReceiver = receiver;
         this.protonReceiver.LinkedResource = this;

         if (options.CreditWindow > 0)
         {
            protonReceiver.AddCredit(options.CreditWindow);
         }

         messageQueue = new FifoDeliveryQueue<ClientDelivery>();
         messageQueue.Start();
      }

      public IClient Client => session.Client;

      public IConnection Connection => session.Connection;

      public ISession Session => session;

      public Task<IReceiver> OpenTask => throw new NotImplementedException();

      public string Address
      {
         get
         {
            if (IsDynamic)
            {
               WaitForOpenToComplete();
               return protonReceiver.RemoteSource?.Address;
            }
            else
            {
               return protonReceiver.Source?.Address;
            }
         }
      }

      public ISource Source
      {
         get
         {
            WaitForOpenToComplete();
            return remoteSource;
         }
      }

      public ITarget Target
      {
         get
         {
            WaitForOpenToComplete();
            return remoteTarget;
         }
      }

      public IReadOnlyDictionary<string, object> Properties
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringKeyedMap(protonReceiver.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonReceiver.OfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonReceiver.DesiredCapabilities);
         }
      }

      public int QueuedDeliveries => messageQueue.Count;

      public IReceiver AddCredit(int credit)
      {
         throw new NotImplementedException();
      }

      public void Close(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> CloseAsync(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public void Detach(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public Task<ISender> DetachAsync(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> Drain()
      {
         throw new NotImplementedException();
      }

      public IDelivery Receive()
      {
         return Receive(TimeSpan.MaxValue);
      }

      public IDelivery Receive(TimeSpan timeout)
      {
         CheckClosedOrFailed();

         try
         {
            ClientDelivery delivery = messageQueue.Dequeue(timeout);
            if (delivery != null)
            {
               if (options.AutoAccept)
               {
                  delivery.Disposition(ClientAccepted.Instance, options.AutoSettle);
               }
               else
               {
                  AsyncReplenishCreditIfNeeded();
               }

               return delivery;
            }

            CheckClosedOrFailed();

            return null;
         }
         catch (ThreadInterruptedException e)
         {
            throw new ClientException("Receive wait interrupted", e);
         }
      }

      public IDelivery TryReceive()
      {
         CheckClosedOrFailed();

         IDelivery delivery = messageQueue.DequeueNoWait();
         if (delivery != null)
         {
            if (options.AutoAccept)
            {
               delivery.Disposition(ClientAccepted.Instance, options.AutoSettle);
            }
            else
            {
               AsyncReplenishCreditIfNeeded();
            }
         }
         else
         {
            CheckClosedOrFailed();
         }

         return delivery;
      }

      #region Internal Receiver API

      internal ClientReceiver Open()
      {
         protonReceiver.LocalOpenHandler(HandleLocalOpen)
                       .LocalCloseHandler(HandleLocalCloseOrDetach)
                       .LocalDetachHandler(HandleLocalCloseOrDetach)
                       .OpenHandler(HandleRemoteOpen)
                       .CloseHandler(HandleRemoteCloseOrDetach)
                       .DetachHandler(HandleRemoteCloseOrDetach)
                       .ParentEndpointClosedHandler(HandleParentEndpointClosed)
                       .DeliveryStateUpdatedHandler(HandleDeliveryStateRemotelyUpdated)
                       .DeliveryReadHandler(HandleDeliveryReceived)
                       .DeliveryAbortedHandler(HandleDeliveryAborted)
                       .CreditStateUpdateHandler(HandleReceiverCreditUpdated)
                       .EngineShutdownHandler(HandleEngineShutdown)
                       .Open();

         return this;
      }

      internal void Disposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         // TODO CheckClosedOrFailed();
         // asyncApplyDisposition(delivery, state, settle);
      }

      internal String ReceiverId => receiverId;

      internal bool IsClosed => closed;

      internal bool IsDynamic => protonReceiver.Source?.Dynamic ?? false;

      #endregion

      #region Private Receiver Implementation

      private void AsyncApplyDisposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         // TODO
         //   executor.execute(() -> {
         //       session.getTransactionContext().disposition(delivery, state, settle);
         //       ReplenishCreditIfNeeded();
         //   });
      }

      private void ReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            uint currentCredit = protonReceiver.Credit;
            if (currentCredit <= creditWindow * 0.5)
            {
               uint potentialPrefetch = currentCredit + (uint)messageQueue.Count;

               if (potentialPrefetch <= creditWindow * 0.7)
               {
                  uint additionalCredit = creditWindow - potentialPrefetch;

                  // TODO LOG.trace("Consumer granting additional credit: {}", additionalCredit);
                  try
                  {
                     protonReceiver.AddCredit(additionalCredit);
                  }
                  catch (Exception)
                  {
                     // TODO LOG.debug("Error caught during credit top-up", ex);
                  }
               }
            }
         }
      }

      private void AsyncReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            // TODO executor.execute(() -> replenishCreditIfNeeded());
         }
      }

      private void CheckClosedOrFailed()
      {
         if (IsClosed)
         {
            throw new ClientIllegalStateException("The Receiver was explicitly closed", failureCause);
         }
         else if (failureCause != null)
         {
            throw failureCause;
         }
      }

      private void WaitForOpenToComplete()
      {
         // TODO
      }

      #endregion

      #region Proton Receiver lifecycle event handlers

      private void HandleLocalOpen(Engine.IReceiver receiver)
      {
         throw new NotImplementedException();
      }

      private void HandleLocalCloseOrDetach(Engine.IReceiver receiver)
      {
         throw new NotImplementedException();
      }

      private void HandleRemoteOpen(Engine.IReceiver receiver)
      {
         throw new NotImplementedException();
      }

      private void HandleRemoteCloseOrDetach(Engine.IReceiver receiver)
      {
         throw new NotImplementedException();
      }

      private void HandleParentEndpointClosed(Engine.IReceiver receiver)
      {
         throw new NotImplementedException();
      }

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         throw new NotImplementedException();
      }

      private void HandleDeliveryReceived(IIncomingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      private void HandleDeliveryAborted(IIncomingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      private void HandleDeliveryStateRemotelyUpdated(IIncomingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      private void HandleReceiverCreditUpdated(Engine.IReceiver receiver)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}