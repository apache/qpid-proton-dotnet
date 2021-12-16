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
using System.Net;
using System.Security.Principal;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Concurrent;

namespace Apache.Qpid.Proton.Client.Transport
{
   /// <summary>
   /// Base transport interface which defines the API of a wire
   /// level IO transport used by the client.
   /// </summary>
   public interface ITransport
   {
      /// <summary>
      /// Returns the event loop that this transport is registed against,
      /// the event loop should never have its lifetime linked to a transport
      /// as the client connection will use a single event loop for the
      /// duration of its lifetime.
      /// </summary>
      IEventLoop EventLoop { get; }

      /// <summary>
      /// Returns the originally provided host address this transport was given in the
      /// connect method. Returns null if connect was not yet called.
      /// </summary>
      string Host { get; }

      /// <summary>
      /// Returns the this transport was given or a default that was used in the
      /// connect method. Returns -1 if connect was not yet called.
      /// </summary>
      int Port { get; }

      /// <summary>
      /// Returns the endpoint that his transport connects to.
      /// </summary>
      EndPoint EndPoint { get; }

      /// <summary>
      /// Returns a local principal that was created following successful SSL
      /// handshaking with the remote. Before a handshake or in the case of non-SSL
      /// transport types this method returns null.
      /// </summary>
      IPrincipal LocalPrincipal { get; }

      /// <summary>
      /// Initiates an orderly close of the transport.
      /// </summary>
      void Close();

      /// <summary>
      /// Initiates the IO level connect that will trigger IO events
      /// in the transport event loop based on the outcome.
      /// </summary>
      /// <returns>This transport instance</returns>
      ITransport Connect(string address, int port);

      /// <summary>
      /// Queues the given buffer for write using this transport and
      /// registers a completion action that will be triggered when
      /// the write is actually performed.
      /// </summary>
      /// <param name="buffer">The buffer to write</param>
      /// <param name="writeCompleteAction">optional action to be performed</param>
      /// <returns>This transport instance</returns>
      ITransport Write(IProtonBuffer buffer, Action writeCompleteAction);

      /// <summary>
      /// Configures the read handler used to process incoming bytes that
      /// are read by this transport. The handler is always invoked within
      /// the registered event loop.
      /// </summary>
      /// <param name="readHandler">Handler that is invoked on read</param>
      /// <returns>This transport instance</returns>
      ITransport TransportReadHandler(Action<ITransport, IProtonBuffer> readHandler);

      /// <summary>
      /// Configures the async connected handler that is called when a transport
      /// creates a successful connection to the remote. The handler is always
      /// invoked within the registered event loop.
      /// </summary>
      /// <param name="connectedHandler">Handler that is invoked on connect</param>
      /// <returns>This transport instance</returns>
      ITransport TransportConnectedHandler(Action<ITransport> connectedHandler);

      /// <summary>
      /// Configures the async connected handler that is called when a transport
      /// fails to connect to a remote. The handler is always invoked within the
      /// registered event loop.
      /// </summary>
      /// <param name="connectedHandler">Handler that is invoked on connect failure</param>
      /// <returns>This transport instance</returns>
      ITransport TransportConnectFailedHandler(Action<ITransport, Exception> connectFailedHandler);

      /// <summary>
      /// Configures the async disconnected handler that is called when a transport
      /// experiences a loss of connectivity with the remote. The handler is always
      /// invoked within the registered event loop.
      /// </summary>
      /// <param name="connectedHandler">Handler that is invoked on disconnect</param>
      /// <returns>This transport instance</returns>
      ITransport TransportDisconnectedHandler(Action<ITransport> disconnectedHandler);

   }
}