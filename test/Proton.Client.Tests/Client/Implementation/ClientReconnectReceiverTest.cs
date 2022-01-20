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
using System.Threading;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Messaging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectReceiverTest : ClientBaseTestFixture
   {
      [Test]
      public void TestOpenedReceiverRecoveredAfterConnectionDroppedCreditWindow()
      {
         DoTestOpenedReceiverRecoveredAfterConnectionDropped(false);
      }

      [Test]
      public void TestOpenedReceiverRecoveredAfterConnectionDroppedFixedCreditGrant()
      {
         DoTestOpenedReceiverRecoveredAfterConnectionDropped(true);
      }

      private void DoTestOpenedReceiverRecoveredAfterConnectionDropped(bool fixedCredit)
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            const uint FIXED_CREDIT = 25;
            const uint CREDIT_WINDOW = 15;

            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfReceiver().WithSource().WithAddress("test").And().Respond();
            if (fixedCredit)
            {
               firstPeer.ExpectFlow().WithLinkCredit(FIXED_CREDIT);
            }
            else
            {
               firstPeer.ExpectFlow().WithLinkCredit(CREDIT_WINDOW);
            }
            firstPeer.DropAfterLastHandler(5);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfReceiver().WithSource().WithAddress("test").And().Respond();
            if (fixedCredit)
            {
               secondPeer.ExpectFlow().WithLinkCredit(FIXED_CREDIT);
            }
            else
            {
               secondPeer.ExpectFlow().WithLinkCredit(CREDIT_WINDOW);
            }
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
            ISession session = connection.OpenSession();
            ReceiverOptions receiverOptions = new ReceiverOptions();
            if (fixedCredit)
            {
               receiverOptions.CreditWindow = 0;
            }
            else
            {
               receiverOptions.CreditWindow = CREDIT_WINDOW;
            }

            IReceiver receiver = session.OpenReceiver("test", receiverOptions);
            if (fixedCredit)
            {
               receiver.AddCredit(FIXED_CREDIT);
            }

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectDetach().WithClosed(true).Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            receiver.Close();
            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDynamicReceiverLinkNotRecovered()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfReceiver()
                                    .WithSource().WithDynamic(true).WithAddress((String)null)
                                    .And().Respond()
                                    .WithSource().WithDynamic(true).WithAddress("test-dynamic-node");
            firstPeer.DropAfterLastHandler(5);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
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
            ISession session = connection.OpenSession();
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 0
            };
            IReceiver receiver = session.OpenDynamicReceiver(receiverOptions);

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            try
            {
               receiver.Drain();
               Assert.Fail("Should not be able to drain as dynamic receiver not recovered");
            }
            catch (ClientConnectionRemotelyClosedException ex)
            {
               logger.LogTrace("Error caught: ", ex);
            }

            receiver.Close();
            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDispositionFromDeliveryReceivedBeforeDisconnectIsNoOp()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfReceiver().Respond();
            firstPeer.ExpectFlow().WithLinkCredit(10);
            firstPeer.RemoteTransfer().WithHandle(0)
                                      .WithDeliveryId(0)
                                      .WithDeliveryTag(new byte[] { 1 })
                                      .WithMore(false)
                                      .WithSettled(true)
                                      .WithMessageFormat(0)
                                      .WithPayload(payload).Queue();
            firstPeer.DropAfterLastHandler(100);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfReceiver().Respond();
            secondPeer.ExpectFlow().WithLinkCredit(9);
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
            ISession session = connection.OpenSession();
            ReceiverOptions rcvOpts = new ReceiverOptions()
            {
               AutoAccept = false
            };
            IReceiver receiver = session.OpenReceiver("test-queue", rcvOpts);
            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(10));

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectDetach().Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            Assert.IsNotNull(delivery);

            delivery.Accept();

            receiver.Close();
            session.Close();
            connection.Close();

            Assert.IsNotNull(delivery);
         }
      }
   }
}