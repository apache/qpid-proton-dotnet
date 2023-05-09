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
using System.IO;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Transport;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Creates a proxy layer between a transport instance and an engine instance which
   /// isolates concerns of mapping the events from a transport to a given engine instance
   /// and prevents possible misdirected event routing during reconnects etc where the
   /// active transport and engine will switch.
   /// </summary>
   public sealed class ClientTransportProxy
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientTransportProxy>();

      private static readonly string EngineTransportProxyBindingKey = "Transport-Proxy-Key";

      private readonly IEngine engine;
      private readonly ITransport transport;

      public ClientTransportProxy(IEngine engine, ITransport transport)
      {
         this.engine = engine;
         this.engine.Connection.Attachments.Set(EngineTransportProxyBindingKey, this);

         this.transport = transport;
         this.transport.TransportConnectedHandler(HandleTransportConnected);
         this.transport.TransportConnectFailedHandler(HandleTransportConnectFailed);
         this.transport.TransportDisconnectedHandler(HandleTransportDisconnected);
         this.transport.TransportReadHandler(HandleTransportRead);
      }

      public ITransport Transport => transport;

      public IEngine Engine => engine;

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
               new IOException(string.Format("Connection to remote {0}:{1} failed.", transport.Host, transport.Port)));
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
   }
}