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
using Apache.Qpid.Proton.Client.Concurrent;
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
      private readonly TaskCompletionSource<IReceiver> openFuture = new TaskCompletionSource<IReceiver>();
      private readonly TaskCompletionSource<IReceiver> closeFuture = new TaskCompletionSource<IReceiver>();

      private TaskCompletionSource<IReceiver> drainingFuture;

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

      public Task<IReceiver> OpenTask => openFuture.Task;

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
         try
         {
            CloseAsync(error).Wait();
         }
         catch (Exception)
         {
         }
      }

      public Task<IReceiver> CloseAsync(IErrorCondition error = null)
      {
         return DoCloseOrDetach(true, error);
      }

      public void Detach(IErrorCondition error = null)
      {
         try
         {
            DetachAsync(error).Wait();
         }
         catch (Exception)
         {
         }
      }

      public Task<IReceiver> DetachAsync(IErrorCondition error = null)
      {
         return DoCloseOrDetach(false, error);
      }

      public void Dispose()
      {
         try
         {
            Close();
         }
         catch (Exception)
         {
         }
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

      private Task<IReceiver> DoCloseOrDetach(bool close, IErrorCondition error)
      {
         if (closed.CompareAndSet(false, true))
         {
            // Already closed by failure or shutdown so no need to
            if (!closeFuture.Task.IsCompleted)
            {
               session.Execute(() =>
               {
                  if (protonReceiver.IsLocallyOpen)
                  {
                     try
                     {
                        protonReceiver.ErrorCondition = ClientErrorCondition.AsProtonErrorCondition(error);
                        if (close)
                        {
                           protonReceiver.Close();
                        }
                        else
                        {
                           protonReceiver.Detach();
                        }
                     }
                     catch (Exception)
                     {
                        // The engine event handlers will deal with errors
                     }
                  }
               });
            }
         }

         return closeFuture.Task;
      }

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
         if (!openFuture.Task.IsCompleted || openFuture.Task.IsFaulted)
         {
            try
            {
               openFuture.Task.Wait();
            }
            catch (Exception e)
            {
               throw failureCause ?? ClientExceptionSupport.CreateNonFatalOrPassthrough(e);
            }
         }
      }

      private void ImmediateLinkShutdown(ClientException failureCause)
      {
         if (this.failureCause == null)
         {
            this.failureCause = failureCause;
         }

         try
         {
            if (protonReceiver.IsRemotelyDetached)
            {
               protonReceiver.Detach();
            }
            else
            {
               protonReceiver.Close();
            }
         }
         catch (Exception)
         {
         }

         if (failureCause != null)
         {
            openFuture.TrySetException(failureCause);
            if (drainingFuture != null)
            {
               drainingFuture.TrySetException(failureCause);
            }
         }
         else
         {
            openFuture.TrySetResult(this);
            if (drainingFuture != null)
            {
               drainingFuture.TrySetException(new ClientResourceRemotelyClosedException("The Receiver has been closed"));
            }
         }

         // TODO
         // if (drainingTimeout != null)
         // {
         //    drainingTimeout.cancel(false);
         //    drainingTimeout = null;
         // }

         closeFuture.TrySetResult(this);
      }

      #endregion

      #region Proton Receiver lifecycle event handlers

      private void HandleLocalOpen(Engine.IReceiver receiver)
      {
         if (options.OpenTimeout > 0)
         {
            session.Schedule(() =>
            {
               if (!openFuture.Task.IsCompleted)
               {
                  ImmediateLinkShutdown(
                     new ClientOperationTimedOutException("Receiver open timed out waiting for remote to respond"));
               }
            }, TimeSpan.FromMilliseconds(options.OpenTimeout));
         }
      }

      private void HandleLocalCloseOrDetach(Engine.IReceiver receiver)
      {
         messageQueue.Stop();  // Ensure blocked receivers are all unblocked.

         // If not yet remotely closed we only wait for a remote close if the engine isn't
         // already failed and we have successfully opened the sender without a timeout.
         if (!receiver.Engine.IsShutdown && failureCause == null && receiver.IsRemotelyOpen)
         {
            long timeout = options.CloseTimeout;

            if (timeout > 0)
            {
               // TODO session.ScheduleRequestTimeout(closeFuture, timeout, ()->
               //    new ClientOperationTimedOutException("receiver close timed out waiting for remote to respond");
            }
         }
         else
         {
            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleRemoteOpen(Engine.IReceiver receiver)
      {
         // Check for deferred close pending and hold completion if so
         if (receiver.RemoteSource != null)
         {
            remoteSource = new ClientRemoteSource(receiver.RemoteSource);

            if (receiver.RemoteTerminus != null)
            {
               remoteTarget = new ClientRemoteTarget((Types.Messaging.Target)receiver.RemoteTerminus);
            }

            ReplenishCreditIfNeeded();

            _ = openFuture.TrySetResult(this);

            // TODO
            //LOG.trace("Receiver opened successfully: {}", receiverId);
         }
         else
         {
            // TODO LOG.debug("Receiver opened but remote signalled close is pending: {}", receiverId);
         }
      }

      private void HandleRemoteCloseOrDetach(Engine.IReceiver receiver)
      {
         if (receiver.IsLocallyOpen)
         {
            ImmediateLinkShutdown(ClientExceptionSupport.ConvertToLinkClosedException(
                receiver.RemoteErrorCondition, "Receiver remotely closed without explanation from the remote"));
         }
         else
         {
            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleParentEndpointClosed(Engine.IReceiver receiver)
      {
         // Don't react if engine was shutdown and parent closed as a result instead wait to get the
         // shutdown notification and respond to that change.
         if (receiver.Engine.IsRunning)
         {
            ClientException failureCause;

            if (receiver.Connection.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(receiver.Connection.RemoteErrorCondition);
            }
            else if (receiver.Session.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToSessionClosedException(receiver.Session.RemoteErrorCondition);
            }
            else if (receiver.Engine.FailureCause != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(receiver.Engine.FailureCause);
            }
            else if (!IsClosed)
            {
               failureCause = new ClientResourceRemotelyClosedException("Remote closed without a specific error condition");
            }
            else
            {
               failureCause = null;
            }

            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         if (!IsDynamic && !session.ProtonSession.Engine.IsShutdown)
         {
            uint previousCredit = protonReceiver.Credit + (uint)messageQueue.Count;

            messageQueue.Clear();  // Prefetched messages should be discarded.

            if (drainingFuture != null)
            {
               drainingFuture.TrySetResult(this);
               // if (drainingTimeout != null)
               // {
               //    drainingTimeout.cancel(false);
               //    drainingTimeout = null;
               // }
            }

            protonReceiver.LocalCloseHandler(null);
            protonReceiver.LocalDetachHandler(null);
            protonReceiver.Close();
            protonReceiver = ClientReceiverBuilder.RecreateReceiver(session, protonReceiver, options);
            protonReceiver.LinkedResource = this;
            protonReceiver.AddCredit(previousCredit);

            Open();
         }
         else
         {
            Engine.IConnection connection = engine.Connection;

            ClientException failureCause;

            if (connection.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(connection.RemoteErrorCondition);
            }
            else if (engine.FailureCause != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(engine.FailureCause);
            }
            else if (!IsClosed)
            {
               failureCause = new ClientConnectionRemotelyClosedException("Remote closed without a specific error condition");
            }
            else
            {
               failureCause = null;
            }

            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleDeliveryReceived(IIncomingDelivery delivery)
      {
         // TODO LOG.trace("Delivery data was received: {}", delivery);

         if (delivery.DefaultDeliveryState == null)
         {
            delivery.DefaultDeliveryState = Types.Messaging.Released.Instance;
         }

         if (!delivery.IsPartial)
         {
            // TODO LOG.trace("{} has incoming Message(s).", this);
            messageQueue.Enqueue(new ClientDelivery(this, delivery));
         }
         else
         {
            // The receiver doesn't return a delivery until it has been
            // completely received, and to ensure that happens we need to
            // claim the partially received bytes so that the session window
            // can be reopened if need be and more delivery portions can
            // then arrive.
            delivery.ClaimAvailableBytes();
         }
      }

      private void HandleDeliveryAborted(IIncomingDelivery delivery)
      {
         // TODO LOG.trace("Delivery data was aborted: {}", delivery);
         delivery.Settle();
         ReplenishCreditIfNeeded();
      }

      private void HandleDeliveryStateRemotelyUpdated(IIncomingDelivery delivery)
      {
         // TODO LOG.trace("Delivery remote state was updated: {}", delivery);
      }

      private void HandleReceiverCreditUpdated(Engine.IReceiver receiver)
      {
         // TODO LOG.trace("Receiver credit update by remote: {}", receiver);

         if (drainingFuture != null)
         {
            if (receiver.Credit == 0)
            {
               drainingFuture.TrySetResult(this);
               // TODO
               // if (drainingTimeout != null)
               // {
               //    drainingTimeout.cancel(false);
               //    drainingTimeout = null;
               // }
            }
         }
      }

      #endregion
   }
}