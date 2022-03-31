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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Utilities;
using System.Linq;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Implements the streaming message receiver which allows for reading of large
   /// messages in smaller chunks. The API allows for multiple calls to receiver but
   /// any call that happens after a large message receives begins will be blocked
   /// until the previous large messsage is fully read and the next arrives.
   /// </summary>
   public sealed class ClientStreamReceiver : ClientLinkType<IStreamReceiver, Engine.IReceiver>, IStreamReceiver
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamReceiver>();

      private readonly StreamReceiverOptions options;
      private readonly string receiverId;

      private readonly IDeque<TaskCompletionSource<IStreamDelivery>> receiveRequests =
         new ArrayDeque<TaskCompletionSource<IStreamDelivery>>();

      private TaskCompletionSource<IStreamReceiver> drainingFuture;

      internal ClientStreamReceiver(ClientSession session, StreamReceiverOptions options, string receiverId, Engine.IReceiver receiver)
       : base(session, receiver)
      {
         this.options = options;
         this.receiverId = receiverId;
         this.protonLink.LinkedResource = this;

         if (options.CreditWindow > 0)
         {
            protonLink.AddCredit(options.CreditWindow);
         }
      }

      public int QueuedDeliveries => protonLink.Unsettled.Count(delivery => delivery.LinkedResource == null);

      public IStreamReceiver AddCredit(uint credit)
      {
         return (IStreamReceiver)AddCreditAsync(credit).ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<IStreamReceiver> AddCreditAsync(uint credit)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IStreamReceiver> creditAdded = new TaskCompletionSource<IStreamReceiver>();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(creditAdded))
            {
               if (options.CreditWindow != 0)
               {
                  creditAdded.TrySetException(new ClientIllegalStateException("Cannot add credit when a credit window has been configured"));
               }
               else if (protonLink.IsDraining)
               {
                  creditAdded.TrySetException(new ClientIllegalStateException("Cannot add credit while a drain is pending"));
               }
               else
               {
                  try
                  {
                     protonLink.AddCredit(credit);
                     creditAdded.TrySetResult(this);
                  }
                  catch (Exception ex)
                  {
                     creditAdded.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(ex));
                  }
               }
            }
         });

         return creditAdded.Task;
      }

      public IStreamReceiver Drain()
      {
         return (IStreamReceiver)DrainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<IStreamReceiver> DrainAsync()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IStreamReceiver> drainComplete = new TaskCompletionSource<IStreamReceiver>();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(drainComplete))
            {
               if (protonLink.IsDraining)
               {
                  drainComplete.TrySetException(new ClientIllegalStateException("Stream Receiver is already draining"));
                  return;
               }

               try
               {
                  if (protonLink.Drain())
                  {
                     drainingFuture = drainComplete;
                     session.ScheduleRequestTimeout(drainingFuture, options.DrainTimeout,
                         () => new ClientOperationTimedOutException("Timed out waiting for remote to respond to drain request"));
                  }
                  else
                  {
                     drainComplete.TrySetResult(this);
                  }
               }
               catch (Exception ex)
               {
                  drainComplete.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(ex));
               }
            }
         });

         return drainComplete.Task;
      }

      public IStreamDelivery Receive()
      {
         return Receive(TimeSpan.MaxValue);
      }

      public IStreamDelivery TryReceive()
      {
         return Receive(TimeSpan.Zero);
      }

      public IStreamDelivery Receive(TimeSpan timeout)
      {
         return ReceiveAsync(timeout).ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<IStreamDelivery> ReceiveAsync()
      {
         return ReceiveAsync(TimeSpan.MaxValue);
      }

      public Task<IStreamDelivery> TryReceiveAsync()
      {
         return ReceiveAsync(TimeSpan.Zero);
      }

      public Task<IStreamDelivery> ReceiveAsync(TimeSpan timeout)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IStreamDelivery> receive = new TaskCompletionSource<IStreamDelivery>();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(receive))
            {
               IIncomingDelivery delivery = null;

               // Scan all unsettled deliveries in the link and check if any are still
               // lacking a linked stream delivery instance which indicates they have
               // not been made part of a receive yet.
               foreach (IIncomingDelivery candidate in protonLink.Unsettled)
               {
                  if (candidate.LinkedResource == null)
                  {
                     delivery = candidate;
                     break;
                  }
               }

               if (delivery == null)
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
                           receiveRequests.Remove(receive);
                           receive.TrySetResult(null);
                        }, timeout);
                     }

                     receiveRequests.Enqueue(receive);
                  }
               }
               else
               {
                  receive.TrySetResult(new ClientStreamDelivery(this, delivery));
                  AsyncReplenishCreditIfNeeded();
               }
            }
         });

         return receive.Task;
      }

      #region Internal Receiver API

      internal ClientStreamReceiver Open()
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

      internal Task<IStreamDelivery> DispositionAsync(ClientStreamDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         CheckClosedOrFailed();
         AsyncApplyDisposition(delivery.ProtonDelivery, state, settle);
         return Task.FromResult((IStreamDelivery)delivery);
      }

      internal String ReceiverId => receiverId;

      internal Exception FailureCause => failureCause;

      internal StreamReceiverOptions ReceiverOptions => options;

      #endregion

      #region Private Receiver Implementation

      private void AsyncApplyDisposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         session.Execute(() =>
         {
            session.TransactionContext.Disposition(delivery, state, settle);
            ReplenishCreditIfNeeded();
         });
      }

      private void AsyncReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            session.Execute(ReplenishCreditIfNeeded);
         }
      }

      private void ReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            uint currentCredit = protonLink.Credit;
            if (currentCredit <= creditWindow * 0.5)
            {
               uint potentialPrefetch = (uint)(currentCredit +
                  protonLink.Unsettled.Count((delivery) => delivery.LinkedResource == null));

               if (potentialPrefetch <= creditWindow * 0.7)
               {
                  uint additionalCredit = creditWindow - potentialPrefetch;

                  LOG.Trace("Consumer granting additional credit: {0}", additionalCredit);
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
         finally
         {
            session.CloseAsync();
         }

         if (receiveRequests.TryDequeue(out TaskCompletionSource<IStreamDelivery> entry))
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

         foreach (IIncomingDelivery delivery in protonLink.Unsettled)
         {
            if (delivery.LinkedResource is ClientStreamDelivery streamDelivery)
            {
               streamDelivery.HandleReceiverClosed(this);
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
               remoteTarget = new ClientRemoteTarget((Types.Messaging.Target)receiver.RemoteTerminus);
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

      private void HandleDeliveryStateRemotelyUpdated(IIncomingDelivery delivery)
      {
         LOG.Trace("Delivery remote state was updated: {0}", delivery);
      }

      private void HandleDeliveryReceived(IIncomingDelivery delivery)
      {
         LOG.Trace("Delivery data was received: {0}", delivery);

         if (delivery.DefaultDeliveryState == null)
         {
            delivery.DefaultDeliveryState = Types.Messaging.Released.Instance;
         }

         if (delivery.LinkedResource == null)
         {
            if (receiveRequests.TryDequeue(out TaskCompletionSource<IStreamDelivery> entry))
            {
               entry.TrySetResult(new ClientStreamDelivery(this, delivery));
               AsyncReplenishCreditIfNeeded();
            }
         }
      }

      private void HandleDeliveryAborted(IIncomingDelivery delivery)
      {
         LOG.Trace("Delivery data was aborted: {0}", delivery);
         delivery.Settle();
         ReplenishCreditIfNeeded();
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

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         if (!IsDynamic && !session.ProtonSession.Engine.IsShutdown)
         {
            uint previousCredit = (uint)(protonLink.Credit + protonLink.Unsettled.Count);

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

      #endregion
   }
}