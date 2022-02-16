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

using System.IO;
using System.Threading;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Messaging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectStreamReceiverTest : ClientBaseTestFixture
   {
      [Test]
      public void TestStreamReceiverRecoversAndDeliveryReceived()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfReceiver().Respond();
            firstPeer.ExpectFlow();
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfReceiver().Respond();
            secondPeer.ExpectFlow();
            secondPeer.RemoteTransfer().WithHandle(0)
                                       .WithDeliveryId(0)
                                       .WithDeliveryTag(new byte[] { 1 })
                                       .WithMore(false)
                                       .WithMessageFormat(0)
                                       .WithPayload(payload).Queue();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");
            IStreamDelivery delivery = receiver.Receive();

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            secondPeer.ExpectDetach().Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotReceiveFromStreamStartedBeforeReconnection()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfReceiver().Respond();
            firstPeer.ExpectFlow();
            firstPeer.RemoteTransfer().WithHandle(0)
                                      .WithDeliveryId(0)
                                      .WithDeliveryTag(new byte[] { 1 })
                                      .WithMore(true)
                                      .WithMessageFormat(0)
                                      .WithPayload(payload).Queue();
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfReceiver().Respond();
            secondPeer.ExpectFlow();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");
            IStreamDelivery delivery = receiver.Receive();

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();

            Assert.IsNotNull(delivery);
            Assert.IsFalse(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            Assert.Throws<IOException>(() => delivery.RawInputStream.ReadByte());

            secondPeer.ExpectDetach().Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }
   }
}