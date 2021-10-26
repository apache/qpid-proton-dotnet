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
using Apache.Qpid.Proton.Client.Threading;
using Apache.Qpid.Proton.Client.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   // TODO
   public class ClientConnection : IConnection
   {
      private readonly ClientInstance client;
      private readonly ConnectionOptions options;
      private readonly ClientSessionBuilder sessionBuilder;
      private readonly string connectionId;
      private readonly ReconnectLocationPool reconnectPool = new ReconnectLocationPool();
      private readonly AtomicBoolean closed = new AtomicBoolean();

      private Engine.IEngine engine;
      private Engine.IConnection protonConnection;
      private Exception failureCause;
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

      public void Close()
      {
         if (closed.CompareAndSet(false, true))
         {

         }
         throw new System.NotImplementedException();
      }

      public void Close(IErrorCondition error)
      {
         throw new System.NotImplementedException();
      }

      public Task<IConnection> CloseAsync()
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
         finally
         {
            System.GC.SuppressFinalize(this);
         }
      }

      public Task<IConnection> CloseAsync(IErrorCondition error)
      {
         throw new System.NotImplementedException();
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

      public ISession OpenSession()
      {
         return OpenSession(null);
      }

      public ISession OpenSession(SessionOptions options)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public ISender OpenAnonymousSender()
      {
         return OpenAnonymousSender(null);
      }

      public ISender OpenAnonymousSender(SenderOptions options)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName)
      {
         return OpenDurableReceiver(address, subscriptionName, null);
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver()
      {
         return OpenDynamicReceiver(null, null);
      }

      public IReceiver OpenDynamicReceiver(ReceiverOptions options)
      {
         return OpenDynamicReceiver(null, options);
      }

      public IReceiver OpenDynamicReceiver(IDictionary<string, object> dynamicNodeProperties, ReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IReceiver OpenReceiver(string address)
      {
         return OpenReceiver(address, null);
      }

      public IReceiver OpenReceiver(string address, ReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public ISender OpenSender(string address)
      {
         return OpenSender(address, null);
      }

      public ISender OpenSender(string address, SenderOptions options)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IStreamReceiver OpenStreamReceiver(string address)
      {
         return OpenStreamReceiver(address, null);
      }

      public IStreamReceiver OpenStreamReceiver(string address, StreamReceiverOptions options)
      {
         CheckClosedOrFailed();
         throw new System.NotImplementedException();
      }

      public IStreamSender OpenStreamSender(string address)
      {
         return OpenStreamSender(address, null);
      }

      public IStreamSender OpenStreamSender(string address, StreamSenderOptions options)
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

      internal Engine.IConnection ProtonConnection => protonConnection;

      internal ConnectionOptions Options => options;

      internal ClientConnection Connect()
      {
         throw new NotImplementedException();
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

      #endregion

      #region private connection utility methods

      private void WaitForOpenToComplete()
      {
         // TODO
      }

      #endregion
   }
}