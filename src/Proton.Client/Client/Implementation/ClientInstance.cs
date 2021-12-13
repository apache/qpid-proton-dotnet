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
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Client.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// The client type servers as a container of connections and provides a
   /// means of closing all open connection in a single operation which can
   /// be performed synchronously or provides a Task type that allows a caller
   /// to be notified once all connections have been closed.
   /// </summary>
   public class ClientInstance : IClient
   {
      private static readonly IdGenerator CONTAINER_ID_GENERATOR = new IdGenerator();

      private readonly ClientOptions options;
      private readonly ConnectionOptions defaultConnectionOptions = new ConnectionOptions();
      private readonly IDictionary<string, ClientConnection> connections = new Dictionary<string, ClientConnection>();
      private readonly string clientUniqueId = CONTAINER_ID_GENERATOR.GenerateId();
      private readonly AtomicInteger CONNECTION_COUNTER = new AtomicInteger();
      private readonly AtomicBoolean closed = new AtomicBoolean();

      private readonly Lazy<TaskCompletionSource<IClient>> closeTask = new Lazy<TaskCompletionSource<IClient>>();

      public string ContainerId => throw new System.NotImplementedException();

      internal ClientInstance(ClientOptions options)
      {
         this.options = (ClientOptions)(options?.Clone() ?? new ClientOptions());
      }

      public void Close()
      {
         try
         {
            CloseAsync().GetAwaiter().GetResult();
         }
         catch (Exception)
         {
            // Ignore exceptions as we are closed regardless.
         }
      }

      public Task<IClient> CloseAsync()
      {
         if (closed.CompareAndSet(false, true))
         {
            lock (connections)
            {
               foreach (KeyValuePair<string, ClientConnection> connection in connections)
               {
                  _ = connection.Value.CloseAsync();
               }
            }
         }

         return closeTask.Value.Task;
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
         lock (connections)
         {
            connections.Add(connection.ConnectionId, connection);
         }

         return connection;
      }

      internal void UnregisterClosedConnection(ClientConnection connection)
      {
         lock (connections)
         {
            connections.Remove(connection.ConnectionId);
            if (closed && connections.Count == 0)
            {
               closeTask.Value.SetResult(this);
            }
         }
      }

      internal string NextConnectionId()
      {
         return clientUniqueId + ":" + CONNECTION_COUNTER.IncrementAndGet();
      }
   }
}