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
   public class ClientReceiver : IReceiver
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientReceiver>();

      private readonly ReceiverOptions options;
      private readonly ClientSession session;
      private readonly string receiverId;
      private readonly AtomicBoolean closed = new AtomicBoolean();
      private readonly TaskCompletionSource<IReceiver> openFuture = new TaskCompletionSource<IReceiver>();
      private readonly TaskCompletionSource<IReceiver> closeFuture = new TaskCompletionSource<IReceiver>();

      private readonly IDeque<TaskCompletionSource<IDelivery>> receiveRequests =
         new ArrayDeque<TaskCompletionSource<IDelivery>>();

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
            return ClientConversionSupport.ToStringArray(protonReceiver.RemoteOfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonReceiver.RemoteDesiredCapabilities);
         }
      }

      public int QueuedDeliveries => protonReceiver.Unsettled.Count(delivery => delivery.LinkedResource == null);

      public IReceiver AddCredit(uint credit)
      {
         return AddCreditAsync(credit).ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<IReceiver> AddCreditAsync(uint credit)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IReceiver> creditAdded = new TaskCompletionSource<IReceiver>();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(creditAdded))
            {
               if (options.CreditWindow != 0)
               {
                  creditAdded.TrySetException(new ClientIllegalStateException("Cannot add credit when a credit window has been configured"));
               }
               else if (protonReceiver.IsDraining)
               {
                  creditAdded.TrySetException(new ClientIllegalStateException("Cannot add credit while a drain is pending"));
               }
               else
               {
                  try
                  {
                     protonReceiver.AddCredit(credit);
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

      public IReceiver Drain()
      {
         return DrainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<IReceiver> DrainAsync()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IReceiver> drainComplete = new TaskCompletionSource<IReceiver>();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(drainComplete))
            {
               if (protonReceiver.IsDraining)
               {
                  drainComplete.TrySetException(new ClientIllegalStateException("Receiver is already draining"));
                  return;
               }

               try
               {
                  if (protonReceiver.Drain())
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
         CheckClosedOrFailed();
         TaskCompletionSource<IDelivery> receive = new TaskCompletionSource<IDelivery>();

         session.Execute(() =>
         {
            if (NotClosedOrFailed(receive))
            {
               IIncomingDelivery delivery = null;

               // Scan all unsettled deliveries in the link and check if any are still
               // lacking a linked stream delivery instance which indicates they have
               // not been made part of a receive yet.
               foreach (IIncomingDelivery candidate in protonReceiver.Unsettled)
               {
                  if (candidate.LinkedResource == null && !candidate.IsPartial)
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
                  receive.TrySetResult(new ClientDelivery(this, delivery));
                  if (options.AutoAccept)
                  {
                     AsyncApplyDisposition(delivery, Accepted.Instance, options.AutoSettle);
                  }
                  else
                  {
                     AsyncReplenishCreditIfNeeded();
                  }
               }
            }
         });

         return receive.Task.ConfigureAwait(false).GetAwaiter().GetResult();
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

      internal Task<IDelivery> DispositionAsync(ClientDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         CheckClosedOrFailed();
         AsyncApplyDisposition(delivery.ProtonDelivery, state, settle);
         return Task.FromResult((IDelivery)delivery);
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
         session.Execute(() =>
         {
            session.TransactionContext.Disposition(delivery, state, settle);
            ReplenishCreditIfNeeded();
         });
      }

      private void ReplenishCreditIfNeeded()
      {
         uint creditWindow = options.CreditWindow;
         if (creditWindow > 0)
         {
            uint currentCredit = protonReceiver.Credit;
            if (currentCredit <= creditWindow * 0.5)
            {
               uint potentialPrefetch = currentCredit +
                  (uint)protonReceiver.Unsettled.Count(delivery => delivery.LinkedResource == null);

               if (potentialPrefetch <= creditWindow * 0.7)
               {
                  uint additionalCredit = creditWindow - potentialPrefetch;

                  LOG.Trace("Consumer granting additional credit: {0}", additionalCredit);
                  try
                  {
                     protonReceiver.AddCredit(additionalCredit);
                  }
                  catch (Exception ex)
                  {
                     LOG.Debug("Error caught during credit top-up", ex);
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
            session.Execute(ReplenishCreditIfNeeded);
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

      private bool NotClosedOrFailed<T>(TaskCompletionSource<T> request)
      {
         if (IsClosed)
         {
            request.TrySetException(new ClientIllegalStateException("The Receiver was explicitly closed", failureCause));
            return false;
         }
         else if (failureCause != null)
         {
            request.TrySetException(failureCause);
            return false;
         }
         else
         {
            return true;
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

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         if (!IsDynamic && !session.ProtonSession.Engine.IsShutdown)
         {
            uint previousCredit = (uint)(protonReceiver.Credit +
               protonReceiver.Unsettled.Count(delivery => delivery.LinkedResource == null));

            if (drainingFuture != null)
            {
               drainingFuture.TrySetResult(this);
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
         LOG.Trace("Delivery data was received: {0}", delivery);

         if (delivery.DefaultDeliveryState == null)
         {
            delivery.DefaultDeliveryState = Types.Messaging.Released.Instance;
         }

         if (!delivery.IsPartial)
         {
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
               // Ensure the credit window is expanded to allow the prefetch
               // to fill since we are delivering fully read messages from this
               // receiver
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