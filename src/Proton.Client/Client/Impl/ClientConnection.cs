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
using Apache.Qpid.Proton.Client.Utilities;

namespace Apache.Qpid.Proton.Client.Impl
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
      private Engine.IConnection connection;
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

      public IDictionary<string, object> Properties => throw new System.NotImplementedException();

      public ICollection<string> OfferedCapabilities => throw new System.NotImplementedException();

      public ICollection<string> DesiredCapabilities => throw new System.NotImplementedException();

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
         throw new System.NotImplementedException();
      }

      public ISession DefaultSession()
      {
         throw new System.NotImplementedException();
      }

      public ISender OpenAnonymousSender()
      {
         throw new System.NotImplementedException();
      }

      public ISender OpenAnonymousSender(SenderOptions options)
      {
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName)
      {
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options)
      {
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver()
      {
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver(ReceiverOptions options)
      {
         throw new System.NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver(IDictionary<string, object> dynamicNodeProperties, ReceiverOptions options)
      {
         throw new System.NotImplementedException();
      }

      public IReceiver OpenReceiver(string address)
      {
         throw new System.NotImplementedException();
      }

      public IReceiver OpenReceiver(string address, ReceiverOptions options)
      {
         throw new System.NotImplementedException();
      }

      public ISender OpenSender(string address)
      {
         throw new System.NotImplementedException();
      }

      public ISender OpenSender(string address, SenderOptions options)
      {
         throw new System.NotImplementedException();
      }

      public ISession OpenSession()
      {
         throw new System.NotImplementedException();
      }

      public ISession OpenSession(SessionOptions options)
      {
         throw new System.NotImplementedException();
      }

      public IStreamReceiver OpenStreamReceiver(string address)
      {
         throw new System.NotImplementedException();
      }

      public IStreamReceiver OpenStreamReceiver(string address, StreamReceiverOptions options)
      {
         throw new System.NotImplementedException();
      }

      public IStreamSender OpenStreamSender(string address)
      {
         throw new System.NotImplementedException();
      }

      public IStreamSender OpenStreamSender(string address, StreamSenderOptions options)
      {
         throw new System.NotImplementedException();
      }

      public ITracker Send<T>(IMessage<T> message)
      {
         throw new System.NotImplementedException();
      }

      #region Internal Connection API

      internal string ConnectionId => throw new NotImplementedException();

      internal Engine.IConnection ProtonConnection => connection;

      internal ConnectionOptions Options => options;

      internal ClientConnection Connect()
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}