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
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Implements the stream sender using a stateful current outgoing message that prevents
   /// any sends other than to the current message until the current is completed.
   /// </summary>
   public sealed class ClientStreamSender : ClientLinkType<IStreamSender, Engine.ISender>, IStreamSender
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamSender>();

      private readonly IDeque<ClientOutgoingEnvelope> blocked = new ArrayDeque<ClientOutgoingEnvelope>();
      private readonly string senderId;
      private readonly bool sendsSettled;

      private Action<IStreamSender> senderRemotelyClosedHandler;

      private readonly StreamSenderOptions options;

      internal ClientStreamSender(ClientSession session, StreamSenderOptions options, string senderId, Engine.ISender protonSender)
       : base(session, protonSender)
      {
         this.options = new StreamSenderOptions(options);
         this.senderId = senderId;
         this.protonLink.LinkedResource = this;
         this.sendsSettled = protonSender.SenderSettleMode == Types.Transport.SenderSettleMode.Settled;
      }

      internal StreamSenderOptions Options => options;

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
         TaskCompletionSource<IStreamSenderMessage> request = new();

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
               ClientStreamTracker streamTracker = new(this, streamDelivery);

               streamDelivery.LinkedResource = streamTracker;

               request.TrySetResult(new ClientStreamSenderMessage(this, streamTracker, annotations));
            }
         });

         return request.Task.ConfigureAwait(false).GetAwaiter().GetResult();
      }

      internal string SenderId => senderId;

      internal bool IsAnonymous => protonLink.Target.Address == null;

      internal bool IsSendingSettled => sendsSettled;

      internal Engine.ISender ProtonSender => protonLink;

      internal ClientStreamSender Open()
      {
         protonLink.LocalOpenHandler(HandleLocalOpen)
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
         if (IsAnonymous && protonLink.LinkState == LinkState.Idle)
         {
            ImmediateLinkShutdown(new ClientUnsupportedOperationException(
               "Anonymous relay support not available from this connection"));
         }
      }

      internal IStreamTracker DoStreamMessage(ClientStreamSenderMessage context, IProtonBuffer buffer, uint messageFormat)
      {
         CheckClosedOrFailed();

         TaskCompletionSource<IStreamTracker> request = new();
         ClientOutgoingEnvelope envelope = new(
            this, context.ProtonDelivery, messageFormat, buffer, context.Completed, request);

         ClientSession.Execute(() =>
         {
            if (NotClosedOrFailed(request, context.ProtonDelivery.Sender))
            {
               try
               {
                  if (ProtonSender.IsSendable)
                  {
                     ClientSession.TransactionContext.Send(envelope, null, IsSendingSettled);
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

         return request.Task.ConfigureAwait(false).GetAwaiter().GetResult();
      }

      internal void Abort(IOutgoingDelivery protonDelivery, ClientStreamTracker tracker)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IStreamTracker> request = new();
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
               ClientOutgoingEnvelope envelope = new(this, protonDelivery, protonDelivery.MessageFormat, null, false, request);

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
         TaskCompletionSource<IStreamTracker> request = new();
         request.Task.ContinueWith((tracker) =>
         {
            ClientSession.Execute(() => HandleCreditStateUpdated(ProtonSender));
         });

         ClientSession.Execute(() =>
         {
            ClientOutgoingEnvelope envelope = new(this, protonDelivery, protonDelivery.MessageFormat, null, true, request);
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
               if (!send.Request.IsCompleted)
               {
                  send.Failed(send.CreateSendTimedOutException());
               }
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
         TaskCompletionSource<IStreamTracker> operation = new();

         IProtonBuffer buffer = message.Encode(deliveryAnnotations);

         ClientSession.Execute(() =>
         {
            if (NotClosedOrFailed(operation))
            {
               try
               {
                  ClientOutgoingEnvelope envelope =
                     new(this, null, message.MessageFormat, buffer, true, operation);

                  if (ProtonSender.IsSendable && ProtonSender.Current == null)
                  {
                     session.TransactionContext.Send(envelope, null, IsSendingSettled);
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
         foreach (IOutgoingDelivery delivery in protonLink.Unsettled)
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

      #region Proton Sender lifecycle event handlers

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
               remoteTarget = new ClientRemoteTarget(sender.RemoteTerminus as Target);
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
               if (held.Delivery == protonLink.Current)
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

      private void HandleEngineShutdown(IEngine engine)
      {
         if (!IsDynamic && !session.ProtonSession.Engine.IsShutdown)
         {
            protonLink.LocalCloseHandler(null);
            protonLink.LocalDetachHandler(null);
            protonLink.Close();
            if (protonLink.HasUnsettled)
            {
               FailPendingUnsettledAndBlockedSends(
                   new ClientConnectionRemotelyClosedException("Connection failed and send result is unknown"));
            }
            protonLink = ClientSenderBuilder.RecreateSender(session, protonLink, Options);
            protonLink.LinkedResource = this;

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

      private sealed class ClientOutgoingEnvelope : ISendable
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
         /// the contents of the envelope in order to convey the abort.
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
         /// Gets the task that tracks the completion of this send operation.
         /// </summary>
         public Task Request => request.Task;

         /// <summary>
         /// Returns the proton outgoing delivery object that is contained in this envelope
         /// which can be null if the payload to be sent has not yet had a transmit attempt
         /// due to lack of any credit on the link or in the session window.
         /// </summary>
         public IOutgoingDelivery Delivery => delivery;

         public void Abort()
         {
            delivery.Abort();
            Send(delivery.State, delivery.IsSettled);
         }

         public void Complete()
         {
            Send(delivery.State, delivery.IsSettled);
         }

         /// <summary>
         /// Performs a send of some or all of the message payload on this outgoing delivery
         /// or possibly an abort if the delivery has already begun streaming and has since
         /// been tagged as aborted.
         /// </summary>
         /// <param name="state">The delivery state to apply</param>
         /// <param name="settled">The settlement value to apply</param>
         public void Send(Types.Transport.IDeliveryState state, bool settled)
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

         public void Discard()
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