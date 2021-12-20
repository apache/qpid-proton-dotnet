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
using System.Security.Principal;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Client.Utilities;
using Apache.Qpid.Proton.Engine.Sasl.Client;
using Apache.Qpid.Proton.Client.Transport;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Engine.Exceptions;
using System.IO;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// The client connection class manages a single connection to a remote AMQP
   /// peer and handles connection errors and reconnection operations if those
   /// are enabled.
   /// </summary>
   public class ClientConnection : IConnection
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientConnection>();

      private const int UNLIMITED = -1;
      private const int UNDEFINED = -1;

      private readonly ClientInstance client;
      private readonly ConnectionOptions options;
      private readonly ClientSessionBuilder sessionBuilder;
      private readonly string connectionId;
      private readonly ReconnectLocationPool reconnectPool = new ReconnectLocationPool();
      private readonly AtomicBoolean closed = new AtomicBoolean();
      private readonly ClientConnectionCapabilities capabilities = new ClientConnectionCapabilities();
      private readonly TaskCompletionSource<IConnection> openFuture = new TaskCompletionSource<IConnection>();
      private readonly TaskCompletionSource<IConnection> closeFuture = new TaskCompletionSource<IConnection>();
      private readonly IOContext ioContext;

      private Engine.IEngine engine;
      private Engine.IConnection protonConnection;
      private ITransport transport;
      private AtomicReference<Exception> failureCause = new AtomicReference<Exception>();
      private ClientSession connectionSession;
      private ClientSender connectionSender;
      private long totalConnections;
      private long reconnectAttempts;
      private long nextReconnectDelay = -1;

      internal ClientConnection(ClientInstance client, string host, int port, ConnectionOptions options)
      {
         this.client = client;
         this.options = options;
         this.connectionId = client.NextConnectionId();
         this.sessionBuilder = new ClientSessionBuilder(this);
         this.ioContext = new IOContext(options.TransportOptions, options.SslOptions);

         reconnectPool.Add(new ReconnectLocation(host, port));
         reconnectPool.AddAll(options.ReconnectOptions.ReconnectLocations);
      }

      #region Connection Properties Access APIs

      public IClient Client => client;

      public Task<IConnection> OpenTask => openFuture.Task;

      public IReadOnlyDictionary<string, object> Properties
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringKeyedMap(protonConnection.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonConnection.RemoteDesiredCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonConnection.RemoteDesiredCapabilities);
         }
      }

      #endregion

      public void Close(IErrorCondition error = null)
      {
         try
         {
            DoCloseAsync(error).GetAwaiter().GetResult();
         }
         catch (Exception)
         {
            // Ignore any exception as we are closed regardless
         }
      }

      public Task<IConnection> CloseAsync(IErrorCondition error = null)
      {
         return DoCloseAsync(error);
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

      public ISender DefaultSender()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISender> defaultSender = new TaskCompletionSource<ISender>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               defaultSender.TrySetResult(LazyCreateConnectionSender());
            }
            catch (Exception error)
            {
               defaultSender.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, defaultSender).Task.GetAwaiter().GetResult();
      }

      public ISession DefaultSession()
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISession> defaultSession = new TaskCompletionSource<ISession>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               defaultSession.TrySetResult(LazyCreateConnectionSession());
            }
            catch (Exception error)
            {
               defaultSession.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, defaultSession).Task.GetAwaiter().GetResult();
      }

      public ISession OpenSession(SessionOptions options = null)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISession> createSession = new TaskCompletionSource<ISession>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               createSession.TrySetResult(sessionBuilder.Session(options).Open());
            }
            catch (Exception error)
            {
               createSession.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createSession).Task.GetAwaiter().GetResult();
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         Objects.RequireNonNull(address, "Cannot create a receiver with a null address");
         TaskCompletionSource<IReceiver> createReceiver = new TaskCompletionSource<IReceiver>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               createReceiver.TrySetResult(LazyCreateConnectionSession().InternalOpenDurableReceiver(address, subscriptionName, options));
            }
            catch (Exception error)
            {
               createReceiver.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createReceiver).Task.GetAwaiter().GetResult();
      }

      public IReceiver OpenDynamicReceiver(IDictionary<string, object> dynamicNodeProperties = null)
      {
         return OpenDynamicReceiver(null, dynamicNodeProperties);
      }

      public IReceiver OpenDynamicReceiver(ReceiverOptions options, IDictionary<string, object> dynamicNodeProperties = null)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IReceiver> createReceiver = new TaskCompletionSource<IReceiver>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               createReceiver.TrySetResult(LazyCreateConnectionSession().InternalOpenDynamicReceiver(dynamicNodeProperties, options));
            }
            catch (Exception error)
            {
               createReceiver.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createReceiver).Task.GetAwaiter().GetResult();
      }

      public IReceiver OpenReceiver(string address, ReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         Objects.RequireNonNull(address, "Cannot create a receiver with a null address");
         TaskCompletionSource<IReceiver> createReceiver = new TaskCompletionSource<IReceiver>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               createReceiver.TrySetResult(LazyCreateConnectionSession().InternalOpenReceiver(address, options));
            }
            catch (Exception error)
            {
               createReceiver.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createReceiver).Task.GetAwaiter().GetResult();
      }

      public ISender OpenAnonymousSender(SenderOptions options = null)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<ISender> createSender = new TaskCompletionSource<ISender>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               createSender.TrySetResult(LazyCreateConnectionSession().InternalOpenAnonymousSender(options));
            }
            catch (Exception error)
            {
               createSender.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createSender).Task.GetAwaiter().GetResult();
      }

      public ISender OpenSender(string address, SenderOptions options = null)
      {
         CheckClosedOrFailed();
         Objects.RequireNonNull(address, "Cannot create a sender with a null address");
         TaskCompletionSource<ISender> createSender = new TaskCompletionSource<ISender>();

         Execute(() =>
         {
            try
            {
               CheckClosedOrFailed();
               createSender.TrySetResult(LazyCreateConnectionSession().InternalOpenSender(address, options));
            }
            catch (Exception error)
            {
               createSender.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createSender).Task.GetAwaiter().GetResult();
      }

      public IStreamReceiver OpenStreamReceiver(string address, StreamReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         TaskCompletionSource<IStreamReceiver> createReceiver = new TaskCompletionSource<IStreamReceiver>();

         Execute(() =>
         {
            try
            {
               uint sessionCapacity = StreamReceiverOptions.DEFAULT_READ_BUFFER_SIZE;
               if (options != null)
               {
                  sessionCapacity = options.ReadBufferSize / 2;
               }

               // Session capacity cannot be smaller than one frame size so we adjust to the lower bound
               sessionCapacity = Math.Max(sessionCapacity, protonConnection.MaxFrameSize);

               CheckClosedOrFailed();
               SessionOptions sessionOptions = new SessionOptions(sessionBuilder.DefaultSessionOptions);
               sessionOptions.IncomingCapacity = sessionCapacity;
               ClientStreamSession session = (ClientStreamSession)sessionBuilder.StreamSession(sessionOptions).Open();
               createReceiver.TrySetResult(session.InternalOpenStreamReceiver(address, options));
            }
            catch (Exception error)
            {
               createReceiver.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createReceiver).Task.GetAwaiter().GetResult();
      }

      public IStreamSender OpenStreamSender(string address, StreamSenderOptions options = null)
      {
         CheckClosedOrFailed();
         Objects.RequireNonNull(address, "Cannot create a sender with a null address");
         TaskCompletionSource<IStreamSender> createSender = new TaskCompletionSource<IStreamSender>();

         Execute(() =>
         {
            try
            {
               uint sessionCapacity = StreamSenderOptions.DEFAULT_PENDING_WRITES_BUFFER_SIZE;
               if (options != null)
               {
                  sessionCapacity = options.PendingWriteBufferSize;
               }

               // Session capacity cannot be smaller than one frame size so we adjust to the lower bound
               sessionCapacity = Math.Max(sessionCapacity, protonConnection.MaxFrameSize);

               CheckClosedOrFailed();
               SessionOptions sessionOptions = new SessionOptions(sessionBuilder.DefaultSessionOptions);
               sessionOptions.OutgoingCapacity = sessionCapacity;

               ClientStreamSession session = (ClientStreamSession)sessionBuilder.StreamSession(sessionOptions).Open();
               createSender.TrySetResult(session.InternalOpenStreamSender(address, options));
            }
            catch (Exception error)
            {
               createSender.TrySetException(ClientExceptionSupport.CreateNonFatalOrPassthrough(error));
            }
         });

         return Request(this, createSender).Task.GetAwaiter().GetResult();
      }

      public ITracker Send<T>(IMessage<T> message)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public override string ToString()
      {
         return "ClientConnection:[" + ConnectionId + "]";
      }

      #region Internal Connection API

      internal string ConnectionId => connectionId;

      internal bool IsClosed => closed;

      internal bool HasOpened => openFuture.Task.IsCompleted;

      internal Engine.IEngine ProtonEngine => protonConnection.Engine;

      internal Engine.IConnection ProtonConnection => protonConnection;

      internal ConnectionOptions Options => options;

      internal ClientConnection Connect()
      {
         try
         {
            ReconnectLocation remoteLocation = reconnectPool.Next.Value;

            // Initial configuration validation happens here, if this step fails then the
            // user most likely configured something incorrect or that violates some constraint
            // like an invalid SASL mechanism etc.
            InitializeProtonResources(remoteLocation);
            ScheduleReconnect(remoteLocation);

            return this;
         }
         catch (Exception ex)
         {
            closed.Set(true);
            failureCause.CompareAndSet(null, ClientExceptionSupport.CreateOrPassthroughFatal(ex));
            _ = openFuture.TrySetException(failureCause);
            _ = closeFuture.TrySetResult(this);
            ioContext.Shutdown();

            throw failureCause;
         }
      }

      internal void CheckClosedOrFailed()
      {
         if (closed)
         {
            throw new ClientIllegalStateException("The Connection was explicitly closed", failureCause);
         }
         else if (failureCause.Get() != null)
         {
            throw failureCause;
         }
      }

      internal bool IsAnonymousRelaySupported => capabilities.AnonymousRelaySupported;

      internal void CheckAnonymousRelaySupported()
      {
         if (!capabilities.AnonymousRelaySupported)
         {
            throw new ClientUnsupportedOperationException("Anonymous relay support not available from this connection");
         }
      }

      internal void Execute(Action action)
      {
         ioContext.EventLoop.Execute(action);
      }

      internal void Schedule(Action action, TimeSpan delay)
      {
         // TODO: Either add scheduling to event loop or handle timeouts here somehow
         Task.Delay(delay).ContinueWith((t) => Execute(action));
      }

      internal TaskCompletionSource<T> Request<T>(Object requestor, TaskCompletionSource<T> request)
      {
         // TODO : Wait for completion or fail later via tracking map
         return request;
      }

      #endregion

      #region Proton Engine and Connection event handlers

      private void HandleLocalOpen(Engine.IConnection connection)
      {
         // TODO connection.TickAuto(Scheduler);

         if (options.OpenTimeout > 0)
         {
            Schedule(() =>
            {
               if (!openFuture.Task.IsCompleted)
               {
                  // Ensure a close write is attempted and then force failure regardless
                  // as we don't expect the remote to respond given it hasn't done so yet.
                  try
                  {
                     connection.Close();
                  }
                  catch (Exception) { }

                  connection.Engine.EngineFailed(new ClientOperationTimedOutException(
                      "Connection Open timed out waiting for remote to open"));
               }
            }, TimeSpan.FromMilliseconds(options.OpenTimeout));
         }
      }

      private void HandleLocalClose(Engine.IConnection connection)
      {
         if (connection.IsRemotelyClosed)
         {
            ClientException failureCause;

            if (engine.Connection.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(connection.RemoteErrorCondition);
            }
            else
            {
               failureCause = new ClientConnectionRemotelyClosedException("Unknown error led to connection disconnect");
            }

            try
            {
               connection.Engine.EngineFailed(failureCause);
            }
            catch (Exception)
            {
            }
         }
         else if (!engine.IsShutdown || !engine.IsFailed)
         {
            // Ensure engine gets shut down and future completed if remote doesn't respond.
            Schedule(() =>
            {
               try
               {
                  connection.Engine.Shutdown();
               }
               catch (Exception)
               {
               }
            }, TimeSpan.FromMilliseconds(options.CloseTimeout));
         }
      }

      private void HandleRemoteOpen(Engine.IConnection connection)
      {
         ConnectionEstablished();
         capabilities.DetermineCapabilities(connection);

         if (totalConnections == 1)
         {
            LOG.Info("Connection {0} connected to server: {1}", ConnectionId, transport.EndPoint);
            SubmitConnectionEvent(options.ConnectedHandler, transport.Host, transport.Port);
         }
         else
         {
            LOG.Info("Connection {0} reconnected to server: {1}", ConnectionId, transport.EndPoint);
            SubmitConnectionEvent(options.ReconnectedHandler, transport.Host, transport.Port);
         }

         openFuture.TrySetResult(this);
      }

      private void HandleRemoteClose(Engine.IConnection connection)
      {
         // When the connection is already locally closed this implies the application requested
         // a close of this connection so this is normal, if not then the remote is closing for
         // some reason and we should react as if the connection has failed which we will determine
         // in the local close handler based on state.
         if (connection.IsLocallyClosed)
         {
            try
            {
               connection.Engine.Shutdown();
            }
            catch (Exception ignore)
            {
               LOG.Debug("Unexpected exception thrown from engine shutdown: {0}", ignore.Message);
            }
         }
         else
         {
            try
            {
               connection.Close();
            }
            catch (Exception)
            {
               // Engine handlers will ensure we close down if not already locally closed.
            }
         }
      }

      private void HandleEngineOutput(IProtonBuffer buffer, Action ioComplete)
      {
         bool autoFlush = false;

         if (autoFlush)
         {
            //transport.WriteAndFlush(buffer, ioComplete);
         }
         else
         {
            transport.Write(buffer, ioComplete);
         }
      }

      /// <summary>
      /// Handle normal engine shutdown which should only happen when the connection is closed
      /// by the user, all other cases should lead to engine failed event first which will deal
      /// with reconnect cases and avoid this event unless reconnect cannot proceed.
      /// </summary>
      /// <param name="engine"></param>
      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         // Only handle this on normal shutdown failure will perform its own controlled shutdown
         // and or reconnection logic which this method should avoid interfering with.
         if (engine.FailureCause == null)
         {
            try
            {
               protonConnection.Close();
            }
            catch (Exception)
            {
            }

            try
            {
               transport.Close();
            }
            catch (Exception)
            {
            }

            client.UnregisterClosedConnection(this);

            _ = openFuture.TrySetResult(this);
            _ = closeFuture.TrySetResult(this);
         }
      }

      private void HandleEngineFailure(Engine.IEngine engine)
      {
         ClientIOException failureCause;

         if (engine.Connection.RemoteErrorCondition != null)
         {
            failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(engine.Connection.RemoteErrorCondition);
         }
         else if (engine.FailureCause != null)
         {
            failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(engine.FailureCause);
         }
         else
         {
            failureCause = new ClientConnectionRemotelyClosedException("Unknown error led to connection disconnect");
         }

         LOG.Trace("Engine reports failure with error: {}", failureCause.Message);

         if (IsReconnectAllowed(failureCause))
         {
            LOG.Info("Connection {0} interrupted to server: {1}", ConnectionId, transport.EndPoint);
            // TODO SubmitDisconnectionEvent(options.interruptedHandler(), transport.getHost(), transport.getPort(), failureCause);

            // Initial configuration validation happens here, if this step fails then the
            // user most likely configured something incorrect or that violates some constraint
            // like an invalid SASL mechanism etc.
            try
            {
               ReconnectLocation remoteLocation = reconnectPool.Next.Value;

               InitializeProtonResources(remoteLocation);
               ScheduleReconnect(remoteLocation);
            }
            catch (ClientException initError)
            {
               FailConnection(ClientExceptionSupport.CreateOrPassthroughFatal(initError));
            }
            finally
            {
               engine.Shutdown();
            }
         }
         else
         {
            FailConnection(failureCause);
         }
      }

      #endregion

      #region private connection utility methods

      private Task<IConnection> DoCloseAsync(IErrorCondition error)
      {
         if (closed.CompareAndSet(false, true))
         {
            try
            {
               ioContext.EventLoop.Execute(() =>
               {
                  LOG.Trace("Close requested for connection: {0}", this);

                  if (protonConnection.IsLocallyOpen)
                  {
                     protonConnection.ErrorCondition = ClientErrorCondition.AsProtonErrorCondition(error);

                     try
                     {
                        protonConnection.Close();
                     }
                     catch (Exception)
                     {
                        // Engine error handler will kick in if the write of Close fails
                     }
                  }
                  else
                  {
                     engine.Shutdown();
                  }
               });
            }
            catch (RejectedExecutionException rje)
            {
               LOG.Trace("Close task rejected from the event loop", rje);
            }
            finally
            {
               try
               {
                  // TODO: Blocking here isn't ideal but for now we want to await
                  ///      the remote sending the close performative back to us
                  ///      before dropping the connection. We should probably schedule
                  ///      a task that closes the connection and completes the close
                  ///      future if the remote hasn't responded by then.
                  closeFuture.Task.GetAwaiter().GetResult();
               }
               catch (Exception)
               {
                  // Ignore error as we are closed regardless
               }
               finally
               {
                  try
                  {
                     transport.Close();
                  }
                  catch (Exception) { }

                  ioContext.Shutdown();
               }
            }
         }

         return closeFuture.Task;
      }

      private void InitializeProtonResources(ReconnectLocation location)
      {
         if (options.SaslOptions.SaslEnabled)
         {
            engine = Engine.IEngineFactory.Proton.CreateEngine();
         }
         else
         {
            engine = Engine.IEngineFactory.Proton.CreateNonSaslEngine();
         }

         if (options.TraceFrames)
         {
            engine.Configuration.TraceFrames = true;
            if (!engine.Configuration.TraceFrames)
            {
               LOG.Warn("Connection {0} frame tracing was enabled but protocol engine does not support it", ConnectionId);
            }
         }

         engine.OutputHandler(HandleEngineOutput)
               .ShutdownHandler(HandleEngineShutdown)
               .ErrorHandler(HandleEngineFailure);

         protonConnection = engine.Connection;

         if (client.ContainerId != null)
         {
            protonConnection.ContainerId = client.ContainerId;
         }
         else
         {
            protonConnection.ContainerId = connectionId;
         }

         protonConnection.LinkedResource = this;
         protonConnection.ChannelMax = options.ChannelMax;
         protonConnection.MaxFrameSize = options.MaxFrameSize;
         protonConnection.Hostname = location.Host;
         protonConnection.IdleTimeout = (uint)options.IdleTimeout;
         protonConnection.OfferedCapabilities = ClientConversionSupport.ToSymbolArray(options.OfferedCapabilities);
         protonConnection.DesiredCapabilities = ClientConversionSupport.ToSymbolArray(options.DesiredCapabilities);
         protonConnection.Properties = ClientConversionSupport.ToSymbolKeyedMap(options.Properties);
         protonConnection.LocalOpenHandler(HandleLocalOpen)
                         .LocalCloseHandler(HandleLocalClose)
                         .OpenHandler(HandleRemoteOpen)
                         .CloseHandler(HandleRemoteClose);

         if (options.SaslOptions.SaslEnabled)
         {
            SaslMechanismSelector mechSelector = new SaslMechanismSelector(
               ClientConversionSupport.ToSymbolSet(options.SaslOptions.AllowedMechanisms));

            engine.SaslDriver.Client().Authenticator =
               new SaslAuthenticator(mechSelector, new ClientSaslCredentialsProvider(this));
         }
      }

      private ClientSession LazyCreateConnectionSession()
      {
         return connectionSession ?? (connectionSession = sessionBuilder.Session().Open());
      }

      private ClientSender LazyCreateConnectionSender()
      {
         if (connectionSender == null)
         {
            if (openFuture.Task.IsCompleted)
            {
               CheckAnonymousRelaySupported();
            }

            connectionSender = LazyCreateConnectionSession().InternalOpenAnonymousSender();
            connectionSender.SenderRemotelyClosedHandler(sender =>
            {
               try
               {
                  sender.CloseAsync();
               }
               catch (Exception) { }

               // Clear the old closed sender, a lazy create needs to construct a new sender.
               connectionSender = null;
            });
         }

         return connectionSender;
      }

      private void WaitForOpenToComplete()
      {
         if (!openFuture.Task.IsCompleted || openFuture.Task.IsFaulted)
         {
            try
            {
               openFuture.Task.GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
               throw failureCause.Value ?? ClientExceptionSupport.CreateNonFatalOrPassthrough(e);
            }
         }
      }

      private void FailConnection(ClientIOException cause)
      {
         failureCause.CompareAndSet(null, cause);

         try
         {
            protonConnection.Close();
         }
         catch (Exception) { }

         try
         {
            engine.Shutdown();
         }
         catch (Exception) { }

         _ = openFuture.TrySetException(failureCause);
         _ = closeFuture.TrySetResult(this);

         LOG.Warn("Connection {0} has failed due to: {1}", ConnectionId, failureCause != null ?
                  failureCause.Get().GetType().Name + " -> " + failureCause.Get().Message : "No failure details provided.");

         SubmitDisconnectionEvent(options.DisconnectedHandler, transport.Host, transport.Port, cause);
      }

      #endregion

      #region Client reconnection support API

      private void AttemptConnection(ReconnectLocation location)
      {
         try
         {
            reconnectAttempts++;
            transport = ioContext.NewTransport();
            LOG.Trace("Connection {0} Attempting connection to remote {1}", ConnectionId, location.Host, location.Port);

            transport.TransportConnectedHandler(HandleTransportConnected);
            transport.TransportConnectFailedHandler(HandleTransportConnectFailed);
            transport.TransportDisconnectedHandler(HandleTransportDisconnected);
            transport.TransportReadHandler(HandleTransportRead);

            transport.Connect(location.Host, location.Port);
         }
         catch (Exception error)
         {
            engine.EngineFailed(ClientExceptionSupport.CreateOrPassthroughFatal(error));
         }
      }

      private void HandleTransportConnected(ITransport transport)
      {
         // Trigger the AMQP header and Open performative exchange on connect
         engine.Start().Open();
      }

      private void HandleTransportConnectFailed(ITransport transport, Exception error)
      {
         if (!engine.IsShutdown)
         {
            LOG.Debug("Transport reports connect attempt failed: {0}", transport);
            engine.EngineFailed(
               new IOException(string.Format("Connection to remote {0} failed.", transport.EndPoint)));
         }
      }

      private void HandleTransportDisconnected(ITransport transport)
      {
         if (!engine.IsShutdown)
         {
            LOG.Debug("Transport reports connection dropped: {0}", transport);
            engine.EngineFailed(
               new IOException(string.Format("Connection to remote {0} dropped.", transport.EndPoint)));
         }
      }

      private void HandleTransportRead(ITransport transport, IProtonBuffer buffer)
      {
         try
         {
            do
            {
               engine.Ingest(buffer);
            }
            while (buffer.IsReadable && engine.IsWritable);
            // TODO - How do we handle case of not all data read ?
         }
         catch (EngineStateException e)
         {
            LOG.Warn("Caught problem during incoming data processing: {0}", e.Message, e);
            engine.EngineFailed(ClientExceptionSupport.CreateOrPassthroughFatal(e));
         }
      }

      private void ScheduleReconnect(ReconnectLocation location)
      {
         // Warn of ongoing connection attempts if configured.
         int warnInterval = options.ReconnectOptions.WarnAfterReconnectAttempts;
         if (reconnectAttempts > 0 && warnInterval > 0 && (reconnectAttempts % warnInterval) == 0)
         {
            LOG.Warn("Connection {0}: Failed to connect after: {1} attempt(s) continuing to retry.", ConnectionId, reconnectAttempts);
         }

         // If no connection recovery required then we have never fully connected to a remote
         // so we proceed down the connect with one immediate connection attempt and then follow
         // on delayed attempts based on configuration.
         if (totalConnections == 0)
         {
            if (reconnectAttempts == 0)
            {
               LOG.Trace("Initial connect attempt will be performed immediately");
               ioContext.EventLoop.Execute(() => AttemptConnection(location));
            }
            else
            {
               long delay = NextReconnectDelay();
               LOG.Trace("Next connect attempt will be in {0} milliseconds", delay);
               // TODO: Executor scheduling would handle connection close, this will
               //       try and run this even after a close.
               Task.Delay((int)delay).ContinueWith(
                  (t) => ioContext.EventLoop.Execute(() => AttemptConnection(location)));
            }
         }
         else if (reconnectAttempts == 0)
         {
            LOG.Trace("Initial reconnect attempt will be performed immediately");
            ioContext.EventLoop.Execute(() => AttemptConnection(location));
         }
         else
         {
            long delay = NextReconnectDelay();
            LOG.Trace("Next reconnect attempt will be in {0} milliseconds", delay);
            // TODO: Executor scheduling would handle connection close, this will
            //       try and run this even after a close.
            Task.Delay((int)delay).ContinueWith(
               (t) => ioContext.EventLoop.Execute(() => AttemptConnection(location)));
         }
      }

      private void ConnectionEstablished()
      {
         // After each successful connection is made, update stats to
         // prepare for the next eventual failure.
         totalConnections++;
         nextReconnectDelay = -1;
         reconnectAttempts = 0;
      }

      private bool IsLimitExceeded()
      {
         int reconnectLimit = ReconnectAttemptLimit();
         if (reconnectLimit != UNLIMITED && reconnectAttempts >= reconnectLimit)
         {
            return true;
         }

         return false;
      }

      private bool IsReconnectAllowed(ClientException cause)
      {
         if (options.ReconnectOptions.ReconnectEnabled && !IsClosed)
         {
            // If a connection attempts fail due to Security errors then we abort
            // reconnection as there is a configuration issue and we want to avoid
            // a spinning reconnect cycle that can never complete.
            if (IsStoppageCause(cause))
            {
               return false;
            }

            return !IsLimitExceeded();
         }
         else
         {
            return false;
         }
      }

      private bool IsStoppageCause(ClientException cause)
      {
         if (cause is ClientConnectionSecuritySaslException saslFailure)
         {
            return !saslFailure.IsTemporaryFailure;
         }
         else if (cause is ClientConnectionSecurityException)
         {
            return true;
         }
         else
         {
            return false;
         }
      }

      private int ReconnectAttemptLimit()
      {
         int maxReconnectValue = options.ReconnectOptions.MaxReconnectAttempts;
         if (totalConnections == 0 && options.ReconnectOptions.MaxInitialConnectionAttempts != UNDEFINED)
         {
            // If this is the first connection attempt and a specific startup retry limit
            // is configured then use it, otherwise use the main reconnect limit
            maxReconnectValue = options.ReconnectOptions.MaxInitialConnectionAttempts;
         }

         return maxReconnectValue;
      }

      private long NextReconnectDelay()
      {
         if (nextReconnectDelay == UNDEFINED)
         {
            nextReconnectDelay = options.ReconnectOptions.ReconnectDelay;
         }

         if (options.ReconnectOptions.UseReconnectBackOff && reconnectAttempts > 1)
         {
            // Exponential increment of reconnect delay.
            nextReconnectDelay = (long)(nextReconnectDelay * options.ReconnectOptions.ReconnectBackOffMultiplier);
            if (nextReconnectDelay > options.ReconnectOptions.MaxReconnectDelay)
            {
               nextReconnectDelay = options.ReconnectOptions.MaxReconnectDelay;
            }
         }

         return nextReconnectDelay;
      }

      private void SubmitConnectionEvent(Action<IConnection, ConnectionEvent> handler, string host, int port)
      {
         if (handler != null)
         {
            try
            {
               Task.Factory.StartNew(() => handler(this, new ConnectionEvent(host, port)));
            }
            catch (Exception ex)
            {
               LOG.Trace("Error thrown while attempting to submit event notification ", ex);
            }
         }
      }

      private void SubmitDisconnectionEvent(Action<IConnection, DisconnectionEvent> handler, string host, int port, ClientIOException cause)
      {
         if (handler != null)
         {
            try
            {
               Task.Factory.StartNew(() => handler(this, new DisconnectionEvent(host, port, cause)));
            }
            catch (Exception ex)
            {
               LOG.Trace("Error thrown while attempting to submit event notification ", ex);
            }
         }
      }

      #endregion

      #region Connection defined SASL Credentials provider

      private class ClientSaslCredentialsProvider : ISaslCredentialsProvider
      {
         private ClientConnection connection;
         private ConnectionOptions options;

         public ClientSaslCredentialsProvider(ClientConnection connection)
         {
            this.connection = connection;
            this.options = connection.options;
         }

         public string VHost => options.VirtualHost;

         public string Username => options.User;

         public string Password => options.Password;

         public IPrincipal LocalPrincipal => connection.transport?.LocalPrincipal;

      }

      #endregion
   }
}