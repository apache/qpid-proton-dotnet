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
using Apache.Qpid.Proton.Test.Driver.Network;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// A TCP based test driver server that accepts one connection and then
   /// proceeds to run the configured test script actions and apply scripted
   /// expectations to incoming AMQP frames.
   /// </summary>
   public sealed class ProtonTestServer : ProtonTestPeer
   {
      private readonly AMQPTestDriver driver;
      private readonly ProtonTestServerOptions options;
      private readonly PeerTcpServer server;

      private PeerTcpClient client;

      private ILogger<ProtonTestServer> logger;

      public ProtonTestServer(in ILoggerFactory loggerFactory = null) : this(new ProtonTestServerOptions(), loggerFactory)
      {
      }

      public ProtonTestServer(ProtonTestServerOptions options, in ILoggerFactory loggerFactory = null)
      {
         this.options = options;
         this.server = new PeerTcpServer();
         this.server.ClientConnectedHandler(HandleClientConnected);
         this.server.ServerFailedHandler(HandlerServerStartFailed);
         this.driver = new AMQPTestDriver(PeerName, ProcessDriverOutput, ProcessDriverAssertion, loggerFactory);
         this.logger = loggerFactory?.CreateLogger<ProtonTestServer>();
      }

      public string ServerAddress => server.ListeningOnAddress;

      public int ServerPort => server.ListeningOnPort;

      public void Start()
      {
         CheckClosed();
         server.Start();
      }

      public void Close()
      {
         base.Dispose();
      }

      public override AMQPTestDriver Driver => driver;

      protected override string PeerName => "Server";

      #region Server handlers for AMQP test driver events and server socket events

      protected override void ProcessCloseRequest()
      {
         server.Stop();
         client?.Close();
      }

      protected override void ProcessConnectionEstablished()
      {
         driver.HandleConnectedEstablished();
      }

      protected override void ProcessDriverOutput(Stream output)
      {
         client.Write(output);
      }

      private void ProcessDriverAssertion(Exception error)
      {
         Close();
      }

      #endregion

      #region TCP Server event handlers

      private void HandleClientConnected(PeerTcpClient client)
      {
         // Configure the client
         client.TransportDisconnectedHandler(DisconnectedHandler);
         client.TransportReadHandler(ClientReadHandler);

         this.client = client;
         this.client.Start();

         ProcessConnectionEstablished();
      }

      private void HandlerServerStartFailed(PeerTcpServer server)
      {
         driver.SignalFailure(new IOException("Server failed to start"));
      }

      #endregion

      #region Connected TCP Client event handlers

      private void DisconnectedHandler(PeerTcpClient client)
      {
         if (!closed)
         {
            logger.LogInformation("The client connection has dropped.");
         }
         Close();
      }

      private void ClientReadHandler(PeerTcpClient client, byte[] buffer)
      {
         if (!closed)
         {
            driver.Ingest(new MemoryStream(buffer));
         }
      }

      #endregion
   }
}