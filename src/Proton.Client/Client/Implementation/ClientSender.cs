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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Sender implementation that send complete messages on a remote link.
   /// </summary>
   public sealed class ClientSender : ISender
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientSender>();

      private readonly AtomicBoolean closed = new AtomicBoolean();
      private ClientException failureCause;

      private readonly IDeque<ClientOutgoingEnvelope> blocked = new ArrayDeque<ClientOutgoingEnvelope>();
      private readonly SenderOptions options;
      private readonly ClientSession session;
      private readonly string senderId;
      private readonly bool sendsSettled;
      private readonly TaskCompletionSource<ISender> openFuture = new TaskCompletionSource<ISender>();
      private readonly TaskCompletionSource<ISender> closeFuture = new TaskCompletionSource<ISender>();

      private Engine.ISender protonSender;
      private Action<ClientSender> senderRemotelyClosedHandler;

      private volatile ISource remoteSource;
      private volatile ITarget remoteTarget;

      internal ClientSender(ClientSession session, SenderOptions options, string senderId, Engine.ISender protonSender)
      {
         this.options = new SenderOptions(options);
         this.session = session;
         this.senderId = senderId;
         this.protonSender = protonSender;
         this.protonSender.LinkedResource = this;
         this.sendsSettled = protonSender.SenderSettleMode == Types.Transport.SenderSettleMode.Settled;
      }

      public IClient Client => session.Client;

      public IConnection Connection => session.Connection;

      public ISession Session => session;

      public Task<ISender> OpenTask => openFuture.Task;

      public string Address
      {
         get
         {
            if (IsDynamic)
            {
               WaitForOpenToComplete();
               return (protonSender.RemoteTerminus as ITarget)?.Address;
            }
            else
            {
               return protonSender.Target?.Address;
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
            return ClientConversionSupport.ToStringKeyedMap(protonSender.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSender.OfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSender.DesiredCapabilities);
         }
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

      public Task<ISender> CloseAsync(IErrorCondition error = null)
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

      public Task<ISender> DetachAsync(IErrorCondition error = null)
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

      public ITracker Send<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         return DoSendMessage(ClientMessageSupport.ConvertMessage(message), deliveryAnnotations, true);
      }

      public ITracker TrySend<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         return DoSendMessage(ClientMessageSupport.ConvertMessage(message), deliveryAnnotations, false);
      }

      #region Internal Sender API

      internal string SenderId => senderId;

      internal bool IsClosed => closed;

      internal bool IsDynamic => protonSender.Target?.Dynamic ?? false;

      internal bool IsAnonymous => protonSender.Target.Address == null;

      internal bool IsSendingSettled => sendsSettled;

      internal Engine.ISender ProtonSender => protonSender;

      internal SenderOptions Options => options;

      internal ITracker CreateTracker(IOutgoingDelivery delivery)
      {
         return new ClientTracker(this, delivery);
      }

      internal ITracker CreateNoOpTracker()
      {
         return new ClientNoOpTracker(this);
      }

      internal ClientSender Open()
      {
         protonSender.LocalOpenHandler(HandleLocalOpen)
                     .LocalCloseHandler(HandleLocalCloseOrDetach)
                     .LocalDetachHandler(HandleLocalCloseOrDetach)
                     .OpenHandler(HandleRemoteOpen)
                     .CloseHandler(HandleRemoteCloseOrDetach)
                     .DetachHandler(HandleRemoteCloseOrDetach)
                     .ParentEndpointClosedHandler(HandleParentEndpointClosed)
                     .CreditStateUpdateHandler(HandleCreditStateUpdated)
                     .EngineShutdownHandler(HandleEngineShutdown)
                     .Open();

         return this;
      }

      internal void Disposition(IOutgoingDelivery delivery, Types.Transport.IDeliveryState state, bool settled)
      {
         CheckClosedOrFailed();
         session.Execute(() =>
         {
            delivery.Disposition(state, settled);
         });
      }

      internal ClientSender SenderRemotelyClosedHandler(Action<ClientSender> handler)
      {
         this.senderRemotelyClosedHandler = handler;
         return this;
      }

      internal void HandleUpdateAnonymousRelayNotSupported()
      {
         if (IsAnonymous && protonSender.LinkState == LinkState.Idle)
         {
            ImmediateLinkShutdown(new ClientUnsupportedOperationException(
               "Anonymous relay support not available from this connection"));
         }
      }

      #endregion

      #region Private Sender Implementation

      private ITracker DoSendMessage<T>(IAdvancedMessage<T> message, IDictionary<string, object> deliveryAnnotations, bool waitForCredit)
      {
         TaskCompletionSource<ITracker> operation = new TaskCompletionSource<ITracker>();

         IProtonBuffer buffer = message.Encode(deliveryAnnotations);

         session.Execute(() =>
         {
            if (NotClosedOrFailed(operation))
            {
               try
               {
                  ClientOutgoingEnvelope envelope = new ClientOutgoingEnvelope(this, message.MessageFormat, buffer, operation);

                  if (protonSender.IsSendable && protonSender.Current == null)
                  {
                     session.TransactionContext.Send(envelope, null, protonSender.SenderSettleMode == SenderSettleMode.Settled);
                  }
                  else if (waitForCredit)
                  {
                     AddToTailOfBlockedQueue(envelope);
                  }
                  else
                  {
                     operation.TrySetResult(null);
                  }
               }
               catch (Exception error)
               {
                  operation.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
               }
            }
         });

         return session.Request(this, operation).Task.GetAwaiter().GetResult();
      }

      internal void AddToTailOfBlockedQueue(ClientOutgoingEnvelope send)
      {
         // TODO Need a cancellation token to tell the scheduled timeout it was cancelled.
         // if (options.SendTimeout > 0 && send.SendTimeout == null)
         // {
         //    send.SendTimeout(executor.schedule(()-> {
         //       send.failed(send.createSendTimedOutException());
         //    }, options.SendTimeout(), TimeUnit.MILLISECONDS));
         // }
         if (options.SendTimeout > 0)
         {
            session.Schedule(() =>
            {
               send.Failed(send.CreateSendTimedOutException());
            },
            TimeSpan.FromMilliseconds(options.SendTimeout));
         }

         blocked.EnqueueBack(send);
      }

      internal void AddToHeadOfBlockedQueue(ClientOutgoingEnvelope send)
      {
         // TODO
         // if (options.SendTimeout > 0 && send.SendTimeout == null)
         // {
         //    send.sendTimeout(executor.schedule(()-> {
         //       send.failed(send.createSendTimedOutException());
         //    }, options.sendTimeout(), TimeUnit.MILLISECONDS));
         // }

         blocked.EnqueueFront(send);
      }

      private Task<ISender> DoCloseOrDetach(bool close, IErrorCondition error)
      {
         if (closed.CompareAndSet(false, true))
         {
            // Already closed by failure or shutdown so no need to
            if (!closeFuture.Task.IsCompleted)
            {
               session.Execute(() =>
               {
                  if (protonSender.IsLocallyOpen)
                  {
                     try
                     {
                        protonSender.ErrorCondition = ClientErrorCondition.AsProtonErrorCondition(error);
                        if (close)
                        {
                           protonSender.Close();
                        }
                        else
                        {
                           protonSender.Detach();
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

      private void CheckClosedOrFailed()
      {
         if (IsClosed)
         {
            throw new ClientIllegalStateException("The Sender was explicitly closed", failureCause);
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
            request.TrySetException(new ClientIllegalStateException("The Sender was explicitly closed", failureCause));
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
            if (protonSender.IsRemotelyDetached)
            {
               protonSender.Detach();
            }
            else
            {
               protonSender.Close();
            }
         }
         catch (Exception)
         {
            // Ignore
         }

         FailPendingUnsettledAndBlockedSends(
            failureCause ?? new ClientResourceRemotelyClosedException("The sender link has closed"));

         if (failureCause != null)
         {
            _ = openFuture.TrySetException(failureCause);
         }
         else
         {
            _ = openFuture.TrySetResult(this);
         }

         _ = closeFuture.TrySetResult(this);
      }

      private void FailPendingUnsettledAndBlockedSends(ClientException cause)
      {
         // Cancel all settlement futures for in-flight sends passing an appropriate error to the future
         foreach (IOutgoingDelivery delivery in protonSender.Unsettled)
         {
            try
            {
               ((ClientTracker)delivery.LinkedResource).FailSettlementTask(cause);
            }
            catch (Exception)
            {
            }
         }

         // TODO
         // Cancel all blocked sends passing an appropriate error to the future
         //   blocked.removeIf((held) -> {
         //       held.failed(cause);
         //       return true;
         //   });
      }

      #endregion

      #region Proton Sender lifecycle envent handlers

      private void HandleLocalOpen(Engine.ISender sender)
      {
         if (options.OpenTimeout > 0)
         {
            session.Schedule(() =>
            {
               if (!openFuture.Task.IsCompleted)
               {
                  ImmediateLinkShutdown(new ClientOperationTimedOutException("Sender open timed out waiting for remote to respond"));
               }
            }, TimeSpan.FromMilliseconds(options.OpenTimeout));
         }
      }

      private void HandleLocalCloseOrDetach(Engine.ISender sender)
      {
         // If not yet remotely closed we only wait for a remote close if the engine isn't
         // already failed and we have successfully opened the sender without a timeout.
         if (!sender.Engine.IsShutdown && failureCause == null && sender.IsRemotelyOpen)
         {
            if (options.CloseTimeout > 0)
            {
               session.ScheduleRequestTimeout(closeFuture, options.CloseTimeout, () =>
                  new ClientOperationTimedOutException("Sender close timed out waiting for remote to respond"));
            }
         }
         else
         {
            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleRemoteOpen(Engine.ISender sender)
      {
         // Check for deferred close pending and hold completion if so
         if (sender.RemoteTerminus != null)
         {
            remoteSource = new ClientRemoteSource(sender.RemoteSource);

            if (sender.RemoteTerminus != null)
            {
               remoteTarget = new ClientRemoteTarget(sender.RemoteTerminus as Types.Messaging.Target);
            }

            _ = openFuture.TrySetResult(this);
            LOG.Trace("Sender opened successfully");
         }
         else
         {
            LOG.Debug("Sender {0} opened but remote signalled close is pending: ", sender);
         }
      }

      private void HandleRemoteCloseOrDetach(Engine.ISender sender)
      {
         if (sender.IsLocallyOpen)
         {
            try
            {
               senderRemotelyClosedHandler.Invoke(this);
            }
            catch (Exception) { }

            ImmediateLinkShutdown(ClientExceptionSupport.ConvertToLinkClosedException(
                sender.RemoteErrorCondition, "Sender remotely closed without explanation from the remote"));
         }
         else
         {
            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleParentEndpointClosed(Engine.ISender sender)
      {
         // Don't react if engine was shutdown and parent closed as a result instead wait to get the
         // shutdown notification and respond to that change.
         if (sender.Engine.IsRunning)
         {
            ClientException failureCause;

            if (sender.Connection.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(sender.Connection.RemoteErrorCondition);
            }
            else if (sender.Session.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToSessionClosedException(sender.Session.RemoteErrorCondition);
            }
            else if (sender.Engine.FailureCause != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(sender.Engine.FailureCause);
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

      private void HandleCreditStateUpdated(Engine.ISender sender)
      {
         if (!blocked.IsEmpty)
         {
            while (sender.IsSendable && !blocked.IsEmpty)
            {
               ClientOutgoingEnvelope held = blocked.Peek();
               if (held.Delivery == protonSender.Current)
               {
                  LOG.Trace("Dispatching previously held send");
                  try
                  {
                     // We don't currently allow a sender to define any outcome so we pass null for
                     // now, however a transaction context will apply its TransactionalState outcome
                     // and would wrap anything we passed in the future.
                     session.TransactionContext.Send(held, null, IsSendingSettled);
                  }
                  catch (Exception error)
                  {
                     held.Failed(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
                  }
                  finally
                  {
                     blocked.Dequeue();
                  }
               }
               else
               {
                  break;
               }
            }
         }

         if (sender.IsDraining && sender.Current == null && blocked.IsEmpty)
         {
            sender.Drained();
         }
      }

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         if (!IsDynamic && !session.ProtonSession.Engine.IsShutdown)
         {
            protonSender.LocalCloseHandler(null);
            protonSender.LocalDetachHandler(null);
            protonSender.Close();
            if (protonSender.HasUnsettled)
            {
               FailPendingUnsettledAndBlockedSends(
                   new ClientConnectionRemotelyClosedException("Connection failed and send result is unknown"));
            }
            protonSender = ClientSenderBuilder.RecreateSender(session, protonSender, options);
            protonSender.LinkedResource = this;

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