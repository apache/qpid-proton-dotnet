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
using Apache.Qpid.Proton.Client.Utilities;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Client session that wraps a proton session instance and provides the higher level
   /// clint API for managing links and creating session scoped transaction instances.
   /// </summary>
   public class ClientSession : ISession
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientSession>();

      private static readonly IClientTransactionContext NoOpTransactionContext = new ClientNoOpTransactionContext();

      private const long INFINITE = -1;

      private readonly SessionOptions options;
      private readonly ClientConnection connection;
      private readonly string sessionId;
      private readonly AtomicBoolean closed = new AtomicBoolean();
      private readonly ClientSenderBuilder senderBuilder;
      private readonly ClientReceiverBuilder receiverBuilder;
      private readonly TaskCompletionSource<ISession> openFuture = new TaskCompletionSource<ISession>();
      private readonly TaskCompletionSource<ISession> closeFuture = new TaskCompletionSource<ISession>();

      private IClientTransactionContext txnContext = NoOpTransactionContext;
      private Engine.ISession protonSession;
      private ClientException failureCause;

      public ClientSession(ClientConnection connection, SessionOptions options, string sessionId, Engine.ISession session)
      {
         this.options = new SessionOptions(options);
         this.connection = connection;
         this.protonSession = session;
         this.sessionId = sessionId;
         this.senderBuilder = new ClientSenderBuilder(this);
         this.receiverBuilder = new ClientReceiverBuilder(this);
      }

      public IClient Client => connection.Client;

      public IConnection Connection => connection;

      public Task<ISession> OpenTask => openFuture.Task;

      public IReadOnlyDictionary<string, object> Properties
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringKeyedMap(protonSession.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSession.RemoteOfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSession.RemoteDesiredCapabilities);
         }
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

      public Task<ISession> CloseAsync(IErrorCondition error = null)
      {
         return DoClose(error);
      }

      public virtual IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         Objects.RequireNonNull(address, "Cannot create a durable receiver with a null address");
         Objects.RequireNonNull(address, "Cannot create a durable receiver with a null subscription name");

         TaskCompletionSource<IReceiver> createReceiver = new TaskCompletionSource<IReceiver>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               _ = createReceiver.TrySetResult(InternalOpenDurableReceiver(address, subscriptionName, options));
            }
            catch (Exception error)
            {
               _ = createReceiver.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, createReceiver).Task.Result;
      }

      public virtual IReceiver OpenDynamicReceiver(ReceiverOptions options = null, IDictionary<string, object> dynamicNodeProperties = null)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IReceiver> createReceiver = new TaskCompletionSource<IReceiver>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               _ = createReceiver.TrySetResult(InternalOpenDynamicReceiver(dynamicNodeProperties, options));
            }
            catch (Exception error)
            {
               _ = createReceiver.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, createReceiver).Task.Result;
      }

      public virtual IReceiver OpenReceiver(string address, ReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         Objects.RequireNonNull(address, "Cannot create a receiver with a null address");

         TaskCompletionSource<IReceiver> createReceiver = new TaskCompletionSource<IReceiver>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               _ = createReceiver.TrySetResult(InternalOpenReceiver(address, options));
            }
            catch (Exception error)
            {
               _ = createReceiver.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, createReceiver).Task.Result;
      }

      public virtual ISender OpenAnonymousSender(SenderOptions options = null)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISender> createSender = new TaskCompletionSource<ISender>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               _ = createSender.TrySetResult(InternalOpenAnonymousSender(options));
            }
            catch (Exception error)
            {
               _ = createSender.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, createSender).Task.Result;
      }

      public virtual ISender OpenSender(string address, SenderOptions options = null)
      {
         CheckClosedOrFailed();
         Objects.RequireNonNull(address, "Cannot create a sender with a null address");

         TaskCompletionSource<ISender> createSender = new TaskCompletionSource<ISender>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               _ = createSender.TrySetResult(InternalOpenSender(address, options));
            }
            catch (Exception error)
            {
               _ = createSender.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, createSender).Task.Result;
      }

      public ISession BeginTransaction()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISession> beginFuture = new TaskCompletionSource<ISession>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               if (txnContext == NoOpTransactionContext)
               {
                  txnContext = new ClientLocalTransactionContext(this);
               }
               txnContext.Begin(beginFuture);
            }
            catch (Exception error)
            {
               _ = beginFuture.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, beginFuture).Task.Result;
      }

      public ISession CommitTransaction()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISession> commitFuture = new TaskCompletionSource<ISession>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               txnContext.Commit(commitFuture, false);
            }
            catch (Exception error)
            {
               _ = commitFuture.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, commitFuture).Task.Result;
      }

      public ISession RollbackTransaction()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISession> rollbackFuture = new TaskCompletionSource<ISession>();

         connection.Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               txnContext.Rollback(rollbackFuture, false);
            }
            catch (Exception error)
            {
               _ = rollbackFuture.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return connection.Request(this, rollbackFuture).Task.GetAwaiter().GetResult();
      }

      #region Internal client session API

      internal void CheckClosedOrFailed()
      {
         if (IsClosed)
         {
            throw new ClientIllegalStateException("The Session was explicitly closed", failureCause);
         }
         else if (failureCause != null)
         {
            throw failureCause;
         }
      }

      internal string SessionId => sessionId;

      internal Engine.ISession ProtonSession => protonSession;

      internal bool IsClosed => closed;

      internal SessionOptions Options => options;

      internal void Execute(Action action) => connection.Execute(action);

      internal void Schedule(Action action, TimeSpan delay) => connection.Schedule(action, delay);

      internal ClientSession Open()
      {
         protonSession.LocalOpenHandler(HandleLocalOpen)
                      .LocalCloseHandler(HandleLocalClose)
                      .OpenHandler(HandleRemoteOpen)
                      .CloseHandler(HandleRemoteClose)
                      .EngineShutdownHandler(HandleEngineShutdown);

         try
         {
            protonSession.Open();
         }
         catch (Exception)
         {
            // Connection is responding to all engine failed errors
         }

         return this;
      }

      internal void ScheduleRequestTimeout<T>(TaskCompletionSource<T> request, long timeout, Func<ClientException> errorSupplier)
      {
         if (timeout != INFINITE)
         {
            connection.Schedule(() =>
               _ = request.TrySetException(errorSupplier.Invoke()), TimeSpan.FromMilliseconds(timeout));
         }
         else
         {
            // TODO return null;
         }
      }

      internal TaskCompletionSource<T> Request<T>(Object requestor, TaskCompletionSource<T> request)
      {
         return connection.Request(requestor, request);
      }

      internal ClientReceiver InternalOpenReceiver(string address, ReceiverOptions receiverOptions)
      {
         return receiverBuilder.Receiver(address, receiverOptions).Open();
      }

      internal ClientStreamReceiver InternalOpenStreamReceiver(string address, StreamReceiverOptions receiverOptions)
      {
         return receiverBuilder.StreamReceiver(address, receiverOptions).Open();
      }

      internal ClientReceiver InternalOpenDurableReceiver(string address, string subscriptionName, ReceiverOptions receiverOptions)
      {
         return receiverBuilder.DurableReceiver(address, subscriptionName, receiverOptions).Open();
      }

      internal ClientReceiver InternalOpenDynamicReceiver(IDictionary<string, object> dynamicNodeProperties, ReceiverOptions receiverOptions)
      {
         return receiverBuilder.DynamicReceiver(dynamicNodeProperties, receiverOptions).Open();
      }

      internal ClientSender InternalOpenSender(string address, SenderOptions senderOptions)
      {
         return senderBuilder.Sender(address, senderOptions).Open();
      }

      internal ClientSender InternalOpenAnonymousSender(SenderOptions senderOptions = null)
      {
         // When the connection is opened we are ok to check that the anonymous relay is supported
         // and open the sender if so, otherwise we need to wait.
         if (connection.HasOpened)
         {
            connection.CheckAnonymousRelaySupported();
            return senderBuilder.AnonymousSender(senderOptions).Open();
         }
         else
         {
            return senderBuilder.AnonymousSender(senderOptions);
         }
      }

      internal ClientStreamSender InternalOpenStreamSender(string address, StreamSenderOptions senderOptions)
      {
         return senderBuilder.StreamSender(address, senderOptions).Open();
      }

      #endregion

      #region Session private API

      private Task<ISession> DoClose(IErrorCondition error)
      {
         if (closed.CompareAndSet(false, true))
         {
            // Already closed by failure or shutdown so no need to
            if (!closeFuture.Task.IsCompleted)
            {
               connection.Execute(() =>
               {
                  if (protonSession.IsLocallyOpen)
                  {
                     try
                     {
                        protonSession.ErrorCondition = ClientErrorCondition.AsProtonErrorCondition(error);
                        protonSession.Close();
                     }
                     catch (Exception)
                     {
                        // Allow engine error handler to deal with this
                     }
                  }
               });
            }
         }

         return closeFuture.Task;
      }

      private Engine.ISession ConfigureSession(Engine.ISession protonSession)
      {
         protonSession.LinkedResource = this;
         protonSession.OfferedCapabilities = ClientConversionSupport.ToSymbolArray(options.OfferedCapabilities);
         protonSession.DesiredCapabilities = ClientConversionSupport.ToSymbolArray(options.DesiredCapabilities);
         protonSession.Properties = ClientConversionSupport.ToSymbolKeyedMap(options.Properties);

         return protonSession;
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

      private void ImmediateSessionShutdown(ClientException failureCause)
      {
         if (this.failureCause == null)
         {
            this.failureCause = failureCause;
         }

         try
         {
            protonSession.Close();
         }
         catch (Exception)
         {
         }

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

      #endregion

      #region Session Proton event handling API

      private void HandleLocalOpen(Engine.ISession session)
      {
         if (options.OpenTimeout > 0)
         {
            connection.Schedule(() =>
            {
               if (!openFuture.Task.IsCompleted)
               {
                  ImmediateSessionShutdown(
                     new ClientOperationTimedOutException("Session open timed out waiting for remote to respond"));
               }
            }, TimeSpan.FromMilliseconds(options.OpenTimeout));
         }
      }

      private void HandleLocalClose(Engine.ISession session)
      {
         // If not yet remotely closed we only wait for a remote close if the engine isn't
         // already failed and we have successfully opened the session without a timeout.
         if (session.IsRemotelyOpen && failureCause == null && !session.Engine.IsShutdown)
         {
            long timeout = options.CloseTimeout;

            if (timeout > 0)
            {
               ScheduleRequestTimeout(closeFuture, timeout, () =>
                   new ClientOperationTimedOutException("Session close timed out waiting for remote to respond"));
            }
         }
         else
         {
            ImmediateSessionShutdown(failureCause);
         }
      }

      private void HandleRemoteOpen(Engine.ISession session)
      {
         openFuture.SetResult(this);
         LOG.Trace("Session:{0} opened successfully.", SessionId);

         foreach (Engine.ISender sender in protonSession.Senders)
         {
            if (!sender.IsLocallyOpen)
            {
               // TODO Client Stream sender not accounted for yet
               ClientSender clientSender = (ClientSender)sender.LinkedResource;
               if (connection.IsAnonymousRelaySupported)
               {
                  clientSender.Open();
               }
               else
               {
                  clientSender.HandleUpdateAnonymousRelayNotSupported();
               }
            }
         }
      }

      private void HandleRemoteClose(Engine.ISession session)
      {
         if (session.IsLocallyOpen)
         {
            ImmediateSessionShutdown(ClientExceptionSupport.ConvertToSessionClosedException(session.RemoteErrorCondition));
         }
         else
         {
            ImmediateSessionShutdown(failureCause);
         }
      }

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         // If the connection has an engine that is running then it is going to attempt
         // reconnection and we want to recover by creating a new Session that will be
         // opened once the remote has been recovered.
         if (!connection.ProtonEngine.IsShutdown)
         {
            // No local close processing needed but we should try and let the session
            // clean up any resources it can by closing it.
            protonSession.LocalCloseHandler(null);
            protonSession.Close();
            protonSession = ConfigureSession(ClientSessionBuilder.RecreateSession(connection, protonSession, options));

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

            ImmediateSessionShutdown(failureCause);
         }
      }

      #endregion
   }
}