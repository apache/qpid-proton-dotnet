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
using Apache.Qpid.Proton.Client.Threading;
using Apache.Qpid.Proton.Client.Utilities;
using Apache.Qpid.Proton.Engine.Sasl.Client;

namespace Apache.Qpid.Proton.Client.Implementation
{
   // TODO
   public class ClientConnection : IConnection
   {
      private const int UNLIMITED = -1;
      private const int UNDEFINED = -1;

      private readonly ClientInstance client;
      private readonly ConnectionOptions options;
      private readonly ClientSessionBuilder sessionBuilder;
      private readonly string connectionId;
      private readonly ReconnectLocationPool reconnectPool = new ReconnectLocationPool();
      private readonly AtomicBoolean closed = new AtomicBoolean();
      private readonly ClientConnectionCapabilities capabilities = new ClientConnectionCapabilities();

      private Engine.IEngine engine;
      private Engine.IConnection protonConnection;
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

         reconnectPool.Add(new ReconnectLocation(host, port));
         reconnectPool.AddAll(options.ReconnectOptions.ReconnectLocations);
      }

      #region Connection Properties Access APIs

      public IClient Client => client;

      public Task<IConnection> OpenTask => throw new System.NotImplementedException();

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
         throw new System.NotImplementedException();
      }

      public Task<IConnection> CloseAsync(IErrorCondition error = null)
      {
         throw new System.NotImplementedException();
      }

      public void Dispose()
      {
         try
         {
            Close();
         }
         catch (Exception)
         {
            // TODO Log something helpful
         }
      }

      public ISender DefaultSender()
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public ISession DefaultSession()
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public ISession OpenSession(SessionOptions options = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public ISender OpenAnonymousSender(SenderOptions options = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver()
      {
         return OpenDynamicReceiver(null, null);
      }

      public IReceiver OpenDynamicReceiver(ReceiverOptions options = null, IDictionary<string, object> dynamicNodeProperties = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IReceiver OpenReceiver(string address, ReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public ISender OpenSender(string address, SenderOptions options = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IStreamReceiver OpenStreamReceiver(string address, StreamReceiverOptions options = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IStreamSender OpenStreamSender(string address, StreamSenderOptions options = null)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
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

      internal bool HasOpened => false; // TODO

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
            // TODO openFuture.failed(failureCause);
            // TODO closeFuture.complete(this);
            // TODO ioContext.shutdown();

            throw failureCause;
         }
      }

      internal void CheckClosedOrFailed()
      {
         if (closed)
         {
            throw new ClientIllegalStateException("The Connection was explicitly closed", failureCause);
         }
         else if (failureCause != null)
         {
            throw failureCause;
         }
      }

      internal void CheckAnonymousRelaySupported()
      {
         if (!capabilities.AnonymousRelaySupported)
         {
            throw new ClientUnsupportedOperationException("Anonymous relay support not available from this connection");
         }
      }

      #endregion

      #region Proton Engine and Connection event handlers

      private void HandleEngineOutput(IProtonBuffer buffer, Action ioComplete)
      {
         // TODO
      }

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         // TODO
      }

      private void HandleEngineFailure(Engine.IEngine engine)
      {
         // TODO
      }

      private void HandleLocalOpen(Engine.IConnection connection)
      {
         // TODO
      }

      private void HandleLocalClose(Engine.IConnection connection)
      {
         // TODO
      }

      private void HandleRemoteOpen(Engine.IConnection connection)
      {
         // TODO
      }

      private void HandleRemoteClose(Engine.IConnection connection)
      {
         // TODO
      }

      #endregion

      #region private connection utility methods

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
               // TODO LOG.warn("Connection {} frame tracing was enabled but protocol engine does not support it", getId());
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

      private void WaitForOpenToComplete()
      {
         // TODO
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

         // TODO

         // openFuture.failed(failureCause);
         // closeFuture.complete(this);

         // LOG.warn("Connection {} has failed due to: {}", ConnectionId, failureCause != null ?
         //          failureCause.GetType().Name + " -> " + failureCause.Message : "No failure details provided.");

         // SubmitDisconnectionEvent(options.DisconnectedHandler, transport.Host, transport.Port, failureCause);
      }

      #endregion

      #region Client reconnection support API

      private void AttemptConnection(ReconnectLocation location)
      {
         try
         {
            reconnectAttempts++;
            // TODO
            //transport = ioContext.newTransport();
            //LOG.trace("Connection {} Attempting connection to remote {}:{}", getId(), location.getHost(), location.getPort());
            // transport.connect(location.getHost(), location.getPort(), new ClientTransportListener(engine));
         }
         catch (Exception error)
         {
            engine.EngineFailed(ClientExceptionSupport.CreateOrPassthroughFatal(error));
         }
      }

      private void ScheduleReconnect(ReconnectLocation location)
      {
         // Warn of ongoing connection attempts if configured.
         int warnInterval = options.ReconnectOptions.WarnAfterReconnectAttempts;
         if (reconnectAttempts > 0 && warnInterval > 0 && (reconnectAttempts % warnInterval) == 0)
         {
            // TODO LOG.warn("Connection {}: Failed to connect after: {} attempt(s) continuing to retry.", getId(), reconnectAttempts);
         }

         // If no connection recovery required then we have never fully connected to a remote
         // so we proceed down the connect with one immediate connection attempt and then follow
         // on delayed attempts based on configuration.
         if (totalConnections == 0)
         {
            if (reconnectAttempts == 0)
            {
               // TODO LOG.trace("Initial connect attempt will be performed immediately");
               // executor.execute(()->attemptConnection(location));
            }
            else
            {
               long delay = NextReconnectDelay();
               // TODO LOG.trace("Next connect attempt will be in {} milliseconds", delay);
               // executor.schedule(()->attemptConnection(location), delay, TimeUnit.MILLISECONDS);
            }
         }
         else if (reconnectAttempts == 0)
         {
            // TODO LOG.trace("Initial reconnect attempt will be performed immediately");
            // executor.execute(()->attemptConnection(location));
         }
         else
         {
            long delay = NextReconnectDelay();
            // TODO LOG.trace("Next reconnect attempt will be in {} milliseconds", delay);
            // executor.schedule(()->attemptConnection(location), delay, TimeUnit.MILLISECONDS);
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

         public IPrincipal LocalPrincipal => throw new NotImplementedException(); // TODO

      }

      #endregion
   }
}