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
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Implements the stream sender using a stateful current outgoing message that prevents
   /// any sends other than to the current message until the current is completed.
   /// </summary>
   public sealed class ClientStreamSender : IStreamSender
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamSender>();

      private readonly AtomicBoolean closed = new AtomicBoolean();
      private ClientException failureCause;

      private readonly IDeque<ClientOutgoingEnvelope> blocked = new ArrayDeque<ClientOutgoingEnvelope>();
      private readonly ClientSession session;
      private readonly string senderId;
      private readonly bool sendsSettled;
      private readonly TaskCompletionSource<IStreamSender> openFuture = new TaskCompletionSource<IStreamSender>();
      private readonly TaskCompletionSource<IStreamSender> closeFuture = new TaskCompletionSource<IStreamSender>();

      private Engine.ISender protonSender;
      private Action<IStreamSender> senderRemotelyClosedHandler;

      private volatile ISource remoteSource;
      private volatile ITarget remoteTarget;

      private readonly StreamSenderOptions options;

      internal ClientStreamSender(ClientSession session, StreamSenderOptions options, string senderId, Engine.ISender protonSender)
      {
         this.options = new StreamSenderOptions(options);
         this.session = session;
         this.senderId = senderId;
         this.protonSender = protonSender;
         this.protonSender.LinkedResource = this;
         this.sendsSettled = protonSender.SenderSettleMode == Types.Transport.SenderSettleMode.Settled;
      }

      internal StreamSenderOptions Options => options;

      public IClient Client => session.Client;

      public IConnection Connection => session.Connection;

      public ISession Session => session;

      public Task<IStreamSender> OpenTask => openFuture.Task;

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
            return ClientConversionSupport.ToStringArray(protonSender.RemoteOfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSender.RemoteDesiredCapabilities);
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

      public Task<IStreamSender> CloseAsync(IErrorCondition error = null)
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

      public Task<IStreamSender> DetachAsync(IErrorCondition error = null)
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

      public IStreamTracker Send<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         return DoSendMessageAsync(
            ClientMessageSupport.ConvertMessage(message), deliveryAnnotations, true).ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public IStreamTracker TrySend<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         return DoSendMessageAsync(
            ClientMessageSupport.ConvertMessage(message), deliveryAnnotations, false).ConfigureAwait(false).GetAwaiter().GetResult();
      }

      public Task<IStreamTracker> SendAsync<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         return DoSendMessageAsync(ClientMessageSupport.ConvertMessage(message), deliveryAnnotations, true);
      }

      public Task<IStreamTracker> TrySendAsync<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         return DoSendMessageAsync(ClientMessageSupport.ConvertMessage(message), deliveryAnnotations, false);
      }

      public IStreamSenderMessage BeginMessage(IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         DeliveryAnnotations annotations = null;
         TaskCompletionSource<IStreamSenderMessage> request = new TaskCompletionSource<IStreamSenderMessage>();

         if (deliveryAnnotations != null)
         {
            annotations = new DeliveryAnnotations(ClientConversionSupport.ToSymbolKeyedMap(deliveryAnnotations));
         }

         ClientSession.Execute(() =>
         {
            if (ProtonSender.Current != null)
            {
               request.TrySetException(new ClientIllegalStateException(
                   "Cannot initiate a new streaming send until the previous one is complete"));
            }
            else
            {
               // Grab the next delivery and hold for stream writes, no other sends
               // can occur while we hold the delivery.
               IOutgoingDelivery streamDelivery = ProtonSender.Next();
               ClientStreamTracker streamTracker = new ClientStreamTracker(this, streamDelivery);

               streamDelivery.LinkedResource = streamTracker;

               request.TrySetResult(new ClientStreamSenderMessage(this, streamTracker, annotations));
            }
         });

         return request.Task.ConfigureAwait(false).GetAwaiter().GetResult();
      }

      internal ClientSession ClientSession => session;

      internal string SenderId => senderId;

      internal bool IsClosed => closed;

      internal bool IsDynamic => protonSender.Target?.Dynamic ?? false;

      internal bool IsAnonymous => protonSender.Target.Address == null;

      internal bool IsSendingSettled => sendsSettled;

      internal Engine.ISender ProtonSender => protonSender;

      internal ClientStreamSender Open()
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

      internal void DispositionAsync(IOutgoingDelivery delivery, Types.Transport.IDeliveryState state, bool settled)
      {
         CheckClosedOrFailed();
         session.Execute(() =>
         {
            delivery.Disposition(state, settled);
         });
      }

      internal ClientStreamSender SenderRemotelyClosedHandler(Action<IStreamSender> handler)
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

      internal IStreamTracker DoStreamMessage(ClientStreamSenderMessage context, IProtonBuffer buffer, uint messageFormat)
      {
         CheckClosedOrFailed();

         TaskCompletionSource<IStreamTracker> request = new TaskCompletionSource<IStreamTracker>();
         ClientOutgoingEnvelope envelope = new ClientOutgoingEnvelope(
            this, context.ProtonDelivery, messageFormat, buffer, context.Completed, request);

         ClientSession.Execute(() =>
         {
            if (NotClosedOrFailed(request, context.ProtonDelivery.Sender))
            {
               try
               {
                  if (ProtonSender.IsSendable)
                  {
                     envelope.Send(ClientSession.TransactionContext, null, IsSendingSettled);
                  }
                  else
                  {
                     AddToHeadOfBlockedQueue(envelope);
                  }
               }
               catch (Exception error)
               {
                  request.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
               }
            }
         });

         return (IStreamTracker)request.Task.ConfigureAwait(false).GetAwaiter().GetResult();
      }

      internal void Abort(IOutgoingDelivery protonDelivery, ClientStreamTracker tracker)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IStreamTracker> request = new TaskCompletionSource<IStreamTracker>();
         request.Task.ContinueWith((tracker) =>
         {
            ClientSession.Execute(() => HandleCreditStateUpdated(ProtonSender));
         });

         ClientSession.Execute(() =>
         {
            if (protonDelivery.TransferCount == 0)
            {
               protonDelivery.Abort();
               request.TrySetResult(tracker);
            }
            else
            {
               ClientOutgoingEnvelope envelope = new ClientOutgoingEnvelope(this, protonDelivery, protonDelivery.MessageFormat, null, false, request);

               try
               {
                  if (ProtonSender.IsSendable && (ProtonSender.Current == null || ProtonSender.Current == protonDelivery))
                  {
                     envelope.Abort();
                  }
                  else
                  {
                     if (ProtonSender.Current == protonDelivery)
                     {
                        AddToHeadOfBlockedQueue(envelope);
                     }
                     else
                     {
                        AddToTailOfBlockedQueue(envelope);
                     }
                  }
               }
               catch (Exception error)
               {
                  request.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
               }
            }
         });

         request.Task.ConfigureAwait(false).GetAwaiter().GetResult();
      }

      internal void Complete(IOutgoingDelivery protonDelivery, ClientStreamTracker tracker)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IStreamTracker> request = new TaskCompletionSource<IStreamTracker>();
         request.Task.ContinueWith((tracker) =>
         {
            ClientSession.Execute(() => HandleCreditStateUpdated(ProtonSender));
         });

         ClientSession.Execute(() =>
         {
            ClientOutgoingEnvelope envelope = new ClientOutgoingEnvelope(this, protonDelivery, protonDelivery.MessageFormat, null, true, request);
            try
            {
               if (ProtonSender.IsSendable && (ProtonSender.Current == null || ProtonSender.Current == protonDelivery))
               {
                  envelope.Complete();
               }
               else
               {
                  if (ProtonSender.Current == protonDelivery)
                  {
                     AddToHeadOfBlockedQueue(envelope);
                  }
                  else
                  {
                     AddToTailOfBlockedQueue(envelope);
                  }
               }
            }
            catch (Exception error)
            {
               request.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         request.Task.ConfigureAwait(false).GetAwaiter().GetResult();
      }

      #region Private sender implementations

      private void AddToTailOfBlockedQueue(ClientOutgoingEnvelope send)
      {
         if (Options.SendTimeout > 0)
         {
            session.Schedule(() =>
            {
               send.Failed(send.CreateSendTimedOutException());
            },
            TimeSpan.FromMilliseconds(Options.SendTimeout));
         }

         blocked.EnqueueBack(send);
      }

      private void AddToHeadOfBlockedQueue(ClientOutgoingEnvelope send)
      {
         blocked.EnqueueFront(send);
      }

      private IStreamTracker CreateTracker(IOutgoingDelivery delivery)
      {
         return new ClientStreamTracker(this, delivery);
      }

      private IStreamTracker CreateNoOpTracker()
      {
         return new ClientNoOpStreamTracker(this);
      }

      private Task<IStreamTracker> DoSendMessageAsync<T>(IAdvancedMessage<T> message, IDictionary<string, object> deliveryAnnotations, bool waitForCredit)
      {
         TaskCompletionSource<IStreamTracker> operation = new TaskCompletionSource<IStreamTracker>();

         IProtonBuffer buffer = message.Encode(deliveryAnnotations);

         ClientSession.Execute(() =>
         {
            if (NotClosedOrFailed(operation))
            {
               try
               {
                  ClientOutgoingEnvelope envelope =
                     new ClientOutgoingEnvelope(this, null, message.MessageFormat, buffer, true, operation);

                  if (ProtonSender.IsSendable && ProtonSender.Current == null)
                  {
                     envelope.Send(ClientSession.TransactionContext, null, sendsSettled);
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

         return operation.Task;
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
         return NotClosedOrFailed(request, ProtonSender);
      }

      private bool NotClosedOrFailed<T>(TaskCompletionSource<T> request, Engine.ISender sender)
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
         else if (sender.IsLocallyClosedOrDetached)
         {
            if (sender.Connection.RemoteErrorCondition != null)
            {
               request.TrySetException(ClientExceptionSupport.ConvertToConnectionClosedException(sender.Connection.RemoteErrorCondition));
            }
            else if (sender.Session.RemoteErrorCondition != null)
            {
               request.TrySetException(ClientExceptionSupport.ConvertToSessionClosedException(sender.Session.RemoteErrorCondition));
            }
            else if (sender.Engine.FailureCause != null)
            {
               request.TrySetException(ClientExceptionSupport.ConvertToConnectionClosedException(sender.Engine.FailureCause));
            }
            else
            {
               request.TrySetException(new ClientIllegalStateException("Sender closed without a specific error condition"));
            }
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

      private Task<IStreamSender> DoCloseOrDetach(bool close, IErrorCondition error)
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
         finally
         {
            // If the parent of this sender is a stream session than this sender owns it
            // and must close it when it closes itself to ensure that the resources are
            // cleaned up on the remote for the session.
            if (session is ClientStreamSession)
            {
               session.CloseAsync();
            }
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

         foreach (ClientOutgoingEnvelope envelope in blocked)
         {
            envelope.Failed(cause);
         }

         blocked.Clear();
      }

      #endregion

      #region Proton Sender lifecycle envent handlers

      private void HandleLocalOpen(Engine.ISender sender)
      {
         if (Options.OpenTimeout > 0)
         {
            session.Schedule(() =>
            {
               if (!openFuture.Task.IsCompleted)
               {
                  ImmediateLinkShutdown(new ClientOperationTimedOutException("Sender open timed out waiting for remote to respond"));
               }
            }, TimeSpan.FromMilliseconds(Options.OpenTimeout));
         }
      }

      private void HandleLocalCloseOrDetach(Engine.ISender sender)
      {
         // If not yet remotely closed we only wait for a remote close if the engine isn't
         // already failed and we have successfully opened the sender without a timeout.
         if (!sender.Engine.IsShutdown && failureCause == null && sender.IsRemotelyOpen)
         {
            if (Options.CloseTimeout > 0)
            {
               session.ScheduleRequestTimeout(closeFuture, Options.CloseTimeout, () =>
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

      internal void HandleCreditStateUpdated(Engine.ISender sender)
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
                     held.Send(session.TransactionContext, null, IsSendingSettled);
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
            protonSender = ClientSenderBuilder.RecreateSender(session, protonSender, Options);
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

      #region

      private sealed class ClientOutgoingEnvelope
      {
         private readonly IProtonBuffer payload;
         private readonly TaskCompletionSource<IStreamTracker> request;
         private readonly ClientStreamSender sender;
         private readonly uint messageFormat;
         private readonly bool complete;

         private IOutgoingDelivery delivery;

         /// <summary>
         /// Create a new In-flight Send instance for a complete message send. No further
         /// sends can occur after the send completes however if the send cannot be completed
         /// due to session or link credit issues the send will be requeued at the sender for
         /// retry when the credit is updated by the remote.
         /// </summary>
         /// <param name="sender">The originating sender of the wrapped message payload</param>
         /// <param name="delivery">The proton delivery that is linked to this outgoing write</param>
         /// <param name="messageFormat">The AMQP message format to encode the transfer</param>
         /// <param name="payload">The encoded message bytes</param>
         /// <param name="complete">Is the delivery considered complete as of this transmission</param>
         /// <param name="request">The request that is linked to this send event</param>
         public ClientOutgoingEnvelope(ClientStreamSender sender, IOutgoingDelivery delivery, uint messageFormat, IProtonBuffer payload, bool complete, TaskCompletionSource<IStreamTracker> request)
         {
            this.messageFormat = messageFormat;
            this.delivery = delivery;
            this.payload = payload;
            this.request = request;
            this.sender = sender;
            this.complete = complete;
         }

         /// <summary>
         /// Indicates if the delivery contained within this envelope is aborted. This does
         /// not transmit the actual aborted status to the remote, the sender must transmit
         /// the contents of the envelope in order to convery the abort.
         /// </summary>
         public bool Aborted => delivery?.IsAborted ?? false;

         /// <summary>
         /// The client sender instance that this envelope is linked to
         /// </summary>
         public ClientStreamSender Sender => sender;

         /// <summary>
         /// The encoded message payload this envelope wraps.
         /// </summary>
         public IProtonBuffer Payload => payload;

         /// <summary>
         /// Returns the proton outgoing delivery object that is contained in this envelope
         /// which can be null if the payload to be sent has not yet had a transmit attempt
         /// due to lack of any credit on the link or in the session window.
         /// </summary>
         public IOutgoingDelivery Delivery => delivery;

         public void Send(IClientTransactionContext context, Types.Transport.IDeliveryState state, bool settled)
         {
            context.Send(Transmit, state, settled, Discard);
         }

         public void Abort()
         {
            delivery.Abort();
            Transmit(delivery.State, delivery.IsSettled);
         }

         public void Complete()
         {
            Transmit(delivery.State, delivery.IsSettled);
         }

         /// <summary>
         /// Performs a send of some or all of the message payload on this outgoing delivery
         /// or possibly an abort if the delivery has already begun streaming and has since
         /// been tagged as aborted.
         /// </summary>
         /// <param name="state">The delivery state to apply</param>
         /// <param name="settled">The settlement value to apply</param>
         private void Transmit(Types.Transport.IDeliveryState state, bool settled)
         {
            if (delivery == null)
            {
               delivery = sender.ProtonSender.Next();
               delivery.LinkedResource = sender.CreateTracker(delivery);
            }

            if (delivery.TransferCount == 0)
            {
               delivery.MessageFormat = messageFormat;
               delivery.Disposition(state, settled);
            }

            // We must check if the delivery was fully written and then complete the send operation otherwise
            // if the session capacity limited the amount of payload data we need to hold the completion until
            // the session capacity is refilled and we can fully write the remaining message payload. This
            // area could use some enhancement to allow control of write and flush when dealing with delivery
            // modes that have low assurance versus those that are strict.
            if (Aborted)
            {
               delivery.Abort();
               Succeeded();
            }
            else
            {
               delivery.StreamBytes(payload, complete);
               if (payload != null && payload.IsReadable)
               {
                  sender.AddToHeadOfBlockedQueue(this);
               }
               else
               {
                  Succeeded();
               }
            }
         }

         private void Discard()
         {
            if (delivery != null)
            {
               ClientStreamTracker tracker = (ClientStreamTracker)delivery.LinkedResource;
               if (tracker != null)
               {
                  tracker.CompleteSettlementTask();
               }
               request.TrySetResult(tracker);
            }
            else
            {
               request.TrySetResult(sender.CreateNoOpTracker());
            }
         }

         public ClientOutgoingEnvelope Failed(ClientException exception)
         {
            request.TrySetException(exception);
            return this;
         }

         public ClientOutgoingEnvelope Succeeded()
         {
            request.TrySetResult((IStreamTracker)delivery.LinkedResource);
            return this;
         }

         public ClientException CreateSendTimedOutException()
         {
            return new ClientSendTimedOutException("Timed out waiting for credit to send");
         }
      }

      #endregion
   }
}