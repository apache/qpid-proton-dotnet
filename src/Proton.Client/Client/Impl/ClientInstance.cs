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
using System.Threading;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Utilities;

namespace Apache.Qpid.Proton.Client.Impl
{
   // TODO
   public class ClientInstance : IClient
   {
      private static readonly IdGenerator CONTAINER_ID_GENERATOR = new IdGenerator();

      private readonly ClientOptions options;
      private readonly ConnectionOptions defaultConnectionOptions = new ConnectionOptions();
      private readonly IDictionary<string, ClientConnection> connections = new Dictionary<string, ClientConnection>();
      private readonly string clientUniqueId = CONTAINER_ID_GENERATOR.GenerateId();

      private int connectionCounter;
      private bool disposedValue;

      public string ContainerId => throw new System.NotImplementedException();

      internal ClientInstance(ClientOptions options)
      {
         this.options = options;
      }

      public Task<IClient> Close()
      {
         lock(connections)
         {

         }

         throw new System.NotImplementedException();
      }

      public void Dispose()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         System.GC.SuppressFinalize(this);
      }

      public IConnection Connect(string host, int port)
      {
         lock(connections)
         {
            CheckClosed();
            return AddConnection(new ClientConnection(this, host, port, defaultConnectionOptions)).Connect();
         }
      }

      public IConnection Connect(string host, int port, ConnectionOptions options)
      {
         lock(connections)
         {
            CheckClosed();
            return AddConnection(new ClientConnection(this, host, port, new ConnectionOptions(options))).Connect();
         }
      }

      public IConnection Connect(string host)
      {
         lock(connections)
         {
            CheckClosed();
            return AddConnection(new ClientConnection(this, host, -1, defaultConnectionOptions)).Connect();
         }
      }

      public IConnection Connect(string host, ConnectionOptions options)
      {
         lock(connections)
         {
            CheckClosed();
            return AddConnection(new ClientConnection(this, host, -1, new ConnectionOptions(options))).Connect();
         }
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
         }
      }

      private void CheckClosed()
      {
         if (disposedValue)
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
         return clientUniqueId + ":" + Interlocked.Increment(ref connectionCounter);
      }
   }
}