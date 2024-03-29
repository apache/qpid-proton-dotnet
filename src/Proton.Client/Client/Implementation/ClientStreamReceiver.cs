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
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Implements the streaming message receiver which allows for reading of large
   /// messages in smaller chunks. The API allows for multiple calls to receiver but
   /// any call that happens after a large message receives begins will be blocked
   /// until the previous large message is fully read and the next arrives.
   /// </summary>
   public sealed class ClientStreamReceiver : ClientReceiverLinkType<IStreamReceiver>, IStreamReceiver
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamReceiver>();

      private readonly IDeque<TaskCompletionSource<IStreamDelivery>> receiveRequests =
         new ArrayDeque<TaskCompletionSource<IStreamDelivery>>();

      internal ClientStreamReceiver(ClientSession session, StreamReceiverOptions options, string receiverId, Engine.IReceiver receiver)
       : base(session, options, receiverId, receiver)
      {
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
         TaskCompletionSource<IStreamDelivery> receive = new();

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
                        if (timeout.TotalMilliseconds > uint.MaxValue)
                        {
                           receive.TrySetException(new ArgumentOutOfRangeException(
                              "Receive timeout must convert to a value less than UInt32.MaxValue Milliseconds"));
                        }

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

      internal Exception FailureCause => failureCause;

      internal StreamReceiverOptions ReceiverOptions => (StreamReceiverOptions)options;

      protected override IStreamReceiver Self => this;

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
         // This handle is only for the case that the parent session was remotely or locally
         // closed. In all other cases we want to allow natural engine shutdown handling to
         // trigger shutdown as we can check there if the parent is reconnecting or not.
         if (receiver.Engine.IsRunning && !receiver.Connection.IsLocallyClosed)
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

      private void HandleEngineShutdown(IEngine engine)
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