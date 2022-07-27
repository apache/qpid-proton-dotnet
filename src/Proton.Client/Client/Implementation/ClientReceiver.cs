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
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Utilities;
using System.Linq;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Client receiver implementation which provides a wrapper around the proton
   /// receiver link and processes incoming deliveries with options for queueing
   /// with a credit window.
   /// </summary>
   public class ClientReceiver : ClientReceiverLinkType<IReceiver>, IReceiver
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientReceiver>();

      private readonly IDeque<TaskCompletionSource<IDelivery>> receiveRequests =
         new ArrayDeque<TaskCompletionSource<IDelivery>>();
      private readonly IDeque<IIncomingDelivery> prefetch = new ArrayDeque<IIncomingDelivery>();

      internal ClientReceiver(ClientSession session, ReceiverOptions options, string receiverId, Engine.IReceiver receiver)
       : base(session, options, receiverId, receiver)
      {
      }

      public IDelivery TryReceive()
      {
         return Receive(TimeSpan.Zero);
      }

      public IDelivery Receive()
      {
         return Receive(TimeSpan.MaxValue);
      }

      public IDelivery Receive(TimeSpan timeout)
      {
         return ReceiveAsync(timeout).ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<IDelivery> TryReceiveAsync()
      {
         return ReceiveAsync(TimeSpan.Zero);
      }

      public Task<IDelivery> ReceiveAsync()
      {
         return ReceiveAsync(TimeSpan.MaxValue);
      }

      public Task<IDelivery> ReceiveAsync(TimeSpan timeout)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IDelivery> receive = new();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(receive))
            {
               // Check prefetch for an available message
               if (!prefetch.TryDequeue(out IIncomingDelivery delivery))
               {
                  if (timeout == TimeSpan.Zero)
                  {
                     receive.TrySetResult(null);
                  }
                  else
                  {
                     if (timeout != TimeSpan.MaxValue)
                     {
                        session.Schedule(() =>
                        {
                           if (!receive.Task.IsCompleted)
                           {
                              receiveRequests.Remove(receive);
                              receive.TrySetResult(null);
                           }
                        }, timeout);
                     }

                     receiveRequests.Enqueue(receive);
                  }
               }
               else
               {
                  if (options.AutoAccept)
                  {
                     AsyncApplyDisposition(delivery, Accepted.Instance, options.AutoSettle);
                  }
                  else
                  {
                     AsyncReplenishCreditIfNeeded();
                  }

                  receive.TrySetResult(new ClientDelivery(this, delivery));
               }
            }
         });

         return receive.Task;
      }

      #region Internal Receiver API

      internal ClientReceiver Open()
      {
         protonLink.LocalOpenHandler(HandleLocalOpen)
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

      internal Task<IDelivery> DispositionAsync(ClientDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         CheckClosedOrFailed();
         AsyncApplyDisposition(delivery.ProtonDelivery, state, settle);
         return Task.FromResult((IDelivery)delivery);
      }

      internal ReceiverOptions ReceiverOptions => options;

      protected override IReceiver Self => this;

      protected override void ReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            uint currentCredit = protonLink.Credit;
            if (currentCredit <= creditWindow * 0.5)
            {
               uint potentialPrefetch = currentCredit + (uint)prefetch.Count;

               if (potentialPrefetch <= creditWindow * 0.7)
               {
                  uint additionalCredit = creditWindow - potentialPrefetch;

                  LOG.Trace("Receiver granting additional credit: {0}", additionalCredit);
                  try
                  {
                     protonLink.AddCredit(additionalCredit);
                  }
                  catch (Exception ex)
                  {
                     LOG.Debug("Error caught during credit top-up", ex);
                  }
               }
            }
         }
      }

      #endregion

      #region Private Receiver Implementation

      private void ImmediateLinkShutdown(ClientException failureCause)
      {
         if (this.failureCause == null)
         {
            this.failureCause = failureCause;
         }

         try
         {
            if (protonLink.IsRemotelyDetached)
            {
               protonLink.Detach();
            }
            else
            {
               protonLink.Close();
            }
         }
         catch (Exception)
         {
         }

         if (receiveRequests.TryDequeue(out TaskCompletionSource<IDelivery> entry))
         {
            if (failureCause != null)
            {
               entry.TrySetException(failureCause);
            }
            else
            {
               entry.TrySetException(new ClientResourceRemotelyClosedException("The stream receiver was closed"));
            }
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
         // If not yet remotely closed we only wait for a remote close if the engine isn't
         // already failed and we have successfully opened the sender without a timeout.
         if (!receiver.Engine.IsShutdown && failureCause == null && receiver.IsRemotelyOpen)
         {
            long timeout = options.CloseTimeout;

            if (timeout > 0)
            {
               session.ScheduleRequestTimeout(closeFuture, timeout, () =>
                  new ClientOperationTimedOutException("receiver close timed out waiting for remote to respond"));
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
               remoteTarget = new ClientRemoteTarget((Target)receiver.RemoteTerminus);
            }

            ReplenishCreditIfNeeded();

            _ = openFuture.TrySetResult(this);

            LOG.Trace("Receiver opened successfully: {0}", receiverId);
         }
         else
         {
            LOG.Debug("Receiver opened but remote signalled close is pending: {0}", receiverId);
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

      private void HandleEngineShutdown(IEngine engine)
      {
         if (!IsDynamic && !session.ProtonSession.Engine.IsShutdown)
         {
            uint previousCredit = (uint)(protonLink.Credit +
               protonLink.Unsettled.Count(delivery => delivery.LinkedResource == null));

            if (drainingFuture != null)
            {
               drainingFuture.TrySetResult(this);
            }

            protonLink.LocalCloseHandler(null);
            protonLink.LocalDetachHandler(null);
            protonLink.Close();
            protonLink = ClientReceiverBuilder.RecreateReceiver(session, protonLink, options);
            protonLink.LinkedResource = this;
            protonLink.AddCredit(previousCredit);

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
         LOG.Trace("Delivery data was received: {0}", delivery);

         if (delivery.DefaultDeliveryState == null)
         {
            delivery.DefaultDeliveryState = Released.Instance;
         }

         if (!delivery.IsPartial)
         {
            // Either there is a waiter or we enqueue this into the prefetch buffer far
            // later receive calls. If there was a waiter we must either auto accept or
            // check for credit window expansion to complete the async operation.
            LOG.Trace("{0} has incoming Message(s).", this);
            if (receiveRequests.TryDequeue(out TaskCompletionSource<IDelivery> entry))
            {
               entry.TrySetResult(new ClientDelivery(this, delivery));
               if (options.AutoAccept)
               {
                  AsyncApplyDisposition(delivery, Accepted.Instance, options.AutoSettle);
               }
               else
               {
                  AsyncReplenishCreditIfNeeded();
               }
            }
            else
            {
               prefetch.Enqueue(delivery);
               // Allow the prefetch to fill even in the case there is a session window
               // that would otherwise prevent new incoming bytes.
               delivery.ClaimAvailableBytes();
            }
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
         LOG.Trace("Delivery data was aborted: {0}", delivery);
         delivery.Settle();
         ReplenishCreditIfNeeded();
      }

      private void HandleDeliveryStateRemotelyUpdated(IIncomingDelivery delivery)
      {
         LOG.Trace("Delivery remote state was updated: {0}", delivery);
      }

      private void HandleReceiverCreditUpdated(Engine.IReceiver receiver)
      {
         LOG.Trace("Receiver credit update by remote: {0}", receiver);

         if (drainingFuture != null)
         {
            if (receiver.Credit == 0)
            {
               drainingFuture.TrySetResult(this);
            }
         }
      }

      #endregion
   }
}