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
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// The Container that hosts one ore more AMQP connections that share a given
   /// AMQP container Id.
   /// </summary>
   public interface IClient : IDisposable
   {
      /// <summary>
      /// Create a new IClient instance using the Proton default implementation and configured
      /// with a set of default client options.
      /// </summary>
      /// <returns>a new client instance using the default Proton implementation.</returns>
      static IClient Create()
      {
         return null;
      }

      /// <summary>
      /// Creates a new IClient instance using the Proton default implementation which has been
      /// configured using the provided client options.
      /// </summary>
      /// <param name="options">The options to use to configure the new client instance.</param>
      /// <returns>a new client instance using the default Proton implementation.</returns>
      static IClient Create(ClientOptions options)
      {
         return null;
      }

      /// <summary>
      /// Returns the fixed AMQP container Id value this connection was created with.
      /// </summary>
      string ContainerId { get; }

      /// <summary>
      /// Initiaites an asynchronous close of all the connections created from this client
      /// container.  The returned Task allows the caller to wait for the close to complete
      /// or check in periodically to see if the operation has finished.
      /// </summary>
      /// <returns>A task that aggregates the wait for all connections to close.</returns>
      Task<IClient> Close();

      /// <summary>
      /// Creates a new connection to the designated remote host on the provided port. The connection
      /// is configured using default connection options.
      /// </summary>
      /// <param name="host">The remote host this connection should connect to</param>
      /// <param name="port">The port on the remote host where the connection is established</param>
      /// <returns>A new connection that connects to the given host and port</returns>
      IConnection Connect(string host, int port);

      /// <summary>
      /// Creates a new connection to the designated remote host on the provided port. The connection
      /// is configured using the provided connection options.
      /// </summary>
      /// <param name="host">The remote host this connection should connect to</param>
      /// <param name="port">The port on the remote host where the connection is established</param>
      /// <param name="options">The connection options to use to configure the connection</param>
      /// <returns>A new connection that connects to the given host and port</returns>
      IConnection Connect(string host, int port, ConnectionOptions options);

      /// <summary>
      /// Creates a new connection to the designated remote host on the default AMQP port. The connection
      /// is configured using default connection options.
      /// </summary>
      /// <param name="host">The remote host this connection should connect to</param>
      /// <returns>A new connection that connects to the given host</returns>
      IConnection Connect(string host);

      /// <summary>
      /// Creates a new connection to the designated remote host on the default AMQP port. The connection
      /// is configured using the provided connection options.
      /// </summary>
      /// <param name="host">The remote host this connection should connect to</param>
      /// <param name="options">The connection options to use to configure the connection</param>
      /// <returns>A new connection that connects to the given host</returns>
      IConnection Connect(string host, ConnectionOptions options);

   }
}