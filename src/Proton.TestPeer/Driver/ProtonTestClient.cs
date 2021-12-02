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
   /// A TCP based test driver client that will attempt one connection and then
   /// proceeds to run the configured test script actions and apply scripted
   /// expectations to incoming AMQP frames.
   /// </summary>
   public sealed class ProtonTestClient : ProtonTestPeer
   {
      private readonly AMQPTestDriver driver;
      private readonly ProtonTestClientOptions options;
      private readonly PeerTcpClient client;

      public ProtonTestClient(in ILoggerFactory loggerFactory = null) : this(new ProtonTestClientOptions(), loggerFactory)
      {
      }

      public ProtonTestClient(ProtonTestClientOptions options, in ILoggerFactory loggerFactory = null)
      {
         this.options = options;
         this.client = new PeerTcpClient();
         this.client.TransportConnectedHandler(HandleClientConnected);
         this.client.TransportConnectFailedHandler(HandleClientConnectFailed);
         this.client.TransportDisconnectedHandler(HandleClientDisconnected);
         this.client.TransportReadHandler(HandleClientRead);
         this.driver = new AMQPTestDriver(PeerName, ProcessDriverOutput, ProcessDriverAssertion, loggerFactory);
      }

      public void Connect(string addres, int port)
      {
         CheckClosed();
         client.Connect(addres, port);
      }

      public void Close()
      {
         Dispose();
      }

      public override AMQPTestDriver Driver => driver;

      protected override string PeerName => "Client";

      protected override void ProcessCloseRequest()
      {
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
         client?.Close();
      }

      #region TCP Client handler methods

      private void HandleClientConnected(PeerTcpClient client)
      {
         ProcessConnectionEstablished();
      }

      private void HandleClientConnectFailed(PeerTcpClient client, Exception exception)
      {
         driver.SignalFailure(exception);
      }

      private void HandleClientDisconnected(PeerTcpClient client)
      {
         driver.SignalFailure(new IOException("Client connection failed."));
      }

      private void HandleClientRead(PeerTcpClient client, byte[] buffer)
      {
         driver.Ingest(new MemoryStream(buffer));
      }

      #endregion
   }
}