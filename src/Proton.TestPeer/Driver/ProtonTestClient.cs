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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Network;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// A TCP based test driver client that will attempt one connection and then
   /// proceeds to run the configured test script actions and apply scripted
   /// expectations to incoming AMQP frames.
   /// </summary>
   public sealed class ProtonTestClient : ProtonTestPeer
   {
      private readonly AMQPTestDriver driver;
      private readonly ProtonTestClientOptions options;
      private readonly PeerTcpClient client;

      private PeerTcpTransport transport;

      private readonly ILoggerFactory loggerFactory;
      private readonly ILogger<ProtonTestClient> logger;

      public ProtonTestClient(in ILoggerFactory loggerFactory = null) : this(new ProtonTestClientOptions(), loggerFactory)
      {
      }

      public ProtonTestClient(ProtonTestClientOptions options, in ILoggerFactory loggerFactory = null)
      {
         this.options = options;
         this.loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
         this.logger = loggerFactory.CreateLogger<ProtonTestClient>();
         this.client = new PeerTcpClient(options, this.loggerFactory);
         this.driver = new AMQPTestDriver(PeerName, ProcessDriverOutput, ProcessDriverAssertion, loggerFactory);
      }

      public void Connect(IPEndPoint endpoint)
      {
         CheckClosed();
         client.Connect(endpoint);
      }

      public void Connect(string address, int port)
      {
         CheckClosed();

         transport = client.Connect(address, port);
         transport.TransportConnectedHandler(HandleClientConnected);
         transport.TransportConnectFailedHandler(HandleClientConnectFailed);
         transport.TransportDisconnectedHandler(HandleClientDisconnected);
         transport.TransportReadHandler(HandleClientRead);
         transport.Start();
      }

      public void Close()
      {
         Dispose();
      }

      public override AMQPTestDriver Driver => driver;

      protected override string PeerName => "Client";

      protected override void ProcessCloseRequest()
      {
         transport?.Close();
      }

      protected override void ProcessConnectionEstablished()
      {
         logger.LogTrace("AMQP Client connected to remote.");
         driver.HandleConnectedEstablished();
      }

      protected override void ProcessDriverOutput(Stream output)
      {
         logger.LogTrace("AMQP Client Channel writing: {0} bytes", output.Length);
         transport.Write(output);
      }

      private void ProcessDriverAssertion(Exception error)
      {
         logger.LogTrace("AMQP Test Client Closing due to error: {0}", error.Message);
         Close();
      }

      #region TCP Client handler methods

      private void HandleClientConnected(PeerTcpTransport transport)
      {
         ProcessConnectionEstablished();
      }

      private void HandleClientConnectFailed(PeerTcpTransport transport, Exception exception)
      {
         driver.SignalFailure(exception);
      }

      private void HandleClientDisconnected(PeerTcpTransport transport)
      {
         logger.LogTrace("Client connection failed before channel closed");
         driver.SignalFailure(new IOException("Connection to remote dropped"));
      }

      private void HandleClientRead(PeerTcpTransport transport, byte[] buffer)
      {
         logger.LogTrace("AMQP Test Client Channel processing: {0} bytes", buffer.Length);
         driver.Ingest(new MemoryStream(buffer));
      }

      #endregion
   }
}