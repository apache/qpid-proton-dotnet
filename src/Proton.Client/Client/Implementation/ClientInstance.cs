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
using Apache.Qpid.Proton.Client.Threading;
using Apache.Qpid.Proton.Client.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   // TODO
   public class ClientInstance : IClient
   {
      private static readonly IdGenerator CONTAINER_ID_GENERATOR = new IdGenerator();

      private readonly ClientOptions options;
      private readonly ConnectionOptions defaultConnectionOptions = new ConnectionOptions();
      private readonly IDictionary<string, ClientConnection> connections = new Dictionary<string, ClientConnection>();
      private readonly string clientUniqueId = CONTAINER_ID_GENERATOR.GenerateId();
      private readonly AtomicInteger CONNECTION_COUNTER = new AtomicInteger();
      private readonly AtomicBoolean closed = new AtomicBoolean();

      public string ContainerId => throw new System.NotImplementedException();

      internal ClientInstance(ClientOptions options)
      {
         this.options = (ClientOptions)(options?.Clone() ?? new ClientOptions());
      }

      public void Close()
      {
         // TODO Try and determine a real exception to return other than the aggregated one.
         CloseAsync().Wait();
      }

      public Task<IClient> CloseAsync()
      {
         if (closed.CompareAndSet(false, true))
         {
            lock (connections)
            {

            }
         }

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

      public IConnection Connect(string host, int port, ConnectionOptions options = null)
      {
         lock (connections)
         {
            CheckClosed();
            return AddConnection(new ClientConnection(this, host, port, new ConnectionOptions(options))).Connect();
         }
      }

      public IConnection Connect(string host, ConnectionOptions options = null)
      {
         lock (connections)
         {
            CheckClosed();
            return AddConnection(new ClientConnection(this, host, -1, new ConnectionOptions(options))).Connect();
         }
      }

      private void CheckClosed()
      {
         if (closed)
         {
            throw new InvalidOperationException("Cannot create new connections, the Client has been closed.");
         }
      }

      private ClientConnection AddConnection(ClientConnection connection)
      {
         connections.Add(connection.ConnectionId, connection);
         return connection;
      }

      internal string NextConnectionId()
      {
         return clientUniqueId + ":" + CONNECTION_COUNTER.IncrementAndGet();
      }
   }
}