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

using System.Threading;
using Apache.Qpid.Proton.Test.Driver;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using System;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Types.Transport;
using System.Collections.Generic;
using System.Linq;
using Apache.Qpid.Proton.Client.TestSupport;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientStreamReceiverTest : ClientBaseTestFixture
   {
      [Test]
      public void TestCreateReceiverAndClose()
      {
         DoTestCreateReceiverAndCloseOrDetachLink(true);
      }

      [Test]
      public void TestCreateReceiverAndDetach()
      {
         DoTestCreateReceiverAndCloseOrDetachLink(false);
      }

      private void DoTestCreateReceiverAndCloseOrDetachLink(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");

            receiver.OpenTask.Wait();

            Assert.AreSame(container, receiver.Client);
            Assert.AreSame(connection, receiver.Connection);

            if (close)
            {
               receiver.CloseAsync().Wait();
            }
            else
            {
               receiver.DetachAsync().Wait();
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateReceiverAndCloseSync()
      {
         DoTestCreateReceiverAndCloseOrDetachSyncLink(true);
      }

      [Test]
      public void TestCreateReceiverAndDetachSync()
      {
         DoTestCreateReceiverAndCloseOrDetachSyncLink(false);
      }

      private void DoTestCreateReceiverAndCloseOrDetachSyncLink(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            if (close)
            {
               receiver.Close();
            }
            else
            {
               receiver.Detach();
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateReceiverAndCloseWithErrorSync()
      {
         DoTestCreateReceiverAndCloseOrDetachWithErrorSync(true);
      }

      [Test]
      public void TestCreateReceiverAndDetachWithErrorSync()
      {
         DoTestCreateReceiverAndCloseOrDetachWithErrorSync(false);
      }

      private void DoTestCreateReceiverAndCloseOrDetachWithErrorSync(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().WithError("amqp-resource-deleted", "an error message").WithClosed(close).Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            if (close)
            {
               receiver.Close(IErrorCondition.Create("amqp-resource-deleted", "an error message", null));
            }
            else
            {
               receiver.Detach(IErrorCondition.Create("amqp-resource-deleted", "an error message", null));
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateReceiverAndCloseWithErrorAsync()
      {
         DoTestCreateReceiverAndCloseOrDetachWithErrorAsync(true);
      }

      [Test]
      public void TestCreateReceiverAndDetachWithErrorAsync()
      {
         DoTestCreateReceiverAndCloseOrDetachWithErrorAsync(false);
      }

      private void DoTestCreateReceiverAndCloseOrDetachWithErrorAsync(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().WithError("amqp-resource-deleted", "an error message").WithClosed(close).Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            if (close)
            {
               receiver.CloseAsync(IErrorCondition.Create("amqp-resource-deleted", "an error message", null)).Wait();
            }
            else
            {
               receiver.DetachAsync(IErrorCondition.Create("amqp-resource-deleted", "an error message", null)).Wait();
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamReceiverConfiguresSessionCapacity_1()
      {
         // Read buffer is always halved by connection when creating new session for the stream
         DoTestStreamReceiverSessionCapacity(100_000, 200_000, 1);
      }

      [Test]
      public void TestStreamReceiverConfiguresSessionCapacity_2()
      {
         // Read buffer is always halved by connection when creating new session for the stream
         DoTestStreamReceiverSessionCapacity(100_000, 400_000, 2);
      }

      [Test]
      public void TestStreamReceiverConfiguresSessionCapacity_3()
      {
         // Read buffer is always halved by connection when creating new session for the stream
         DoTestStreamReceiverSessionCapacity(100_000, 600_000, 3);
      }

      [Test]
      public void TestStreamReceiverConfiguresSessionCapacityIdenticalToMaxFrameSize()
      {
         // Read buffer is always halved by connection when creating new session for the stream
         // unless it falls at the max frame size value which means only one is possible, in this
         // case the user configured session window the same as than max frame size so only one
         // frame is possible.
         DoTestStreamReceiverSessionCapacity(100_000, 100_000, 1);
      }

      [Test]
      public void TestStreamReceiverConfiguresSessionCapacityLowerThanMaxFrameSize()
      {
         // Read buffer is always halved by connection when creating new session for the stream
         // unless it falls at the max frame size value which means only one is possible, in this
         // case the user configured session window lower than max frame size and the client auto
         // adjusts that to one frame.
         DoTestStreamReceiverSessionCapacity(100_000, 50_000, 1);
      }

      private void DoTestStreamReceiverSessionCapacity(uint maxFrameSize, uint readBufferSize, uint expectedSessionWindow)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(maxFrameSize).Respond();
            peer.ExpectBegin().WithIncomingWindow(expectedSessionWindow).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(expectedSessionWindow);
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = maxFrameSize
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = readBufferSize
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);

            receiver.OpenTask.Wait();

            receiver.CloseAsync().Wait();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenStreamReceiverWithLinCapabilities()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithCapabilities("queue")
                               .WithDistributionMode(Test.Driver.Matchers.Is.NullValue())
                               .And().Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions receiverOptions = new StreamReceiverOptions();
            receiverOptions.SourceOptions.Capabilities = new String[] { "queue" };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", receiverOptions);

            receiver.OpenTask.Wait();

            Assert.AreSame(container, receiver.Client);
            Assert.AreSame(connection, receiver.Connection);

            receiver.Close();

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateStreamDeliveryWithoutAnyIncomingDeliveryPresent()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive(TimeSpan.FromMilliseconds(5));

            Assert.IsNull(delivery);

            peer.WaitForScriptToComplete();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamReceiverAwaitTimedCanBePerformedMultipleTimes()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();

            Assert.IsNull(receiver.Receive(TimeSpan.FromMilliseconds(5)));
            Assert.IsNull(receiver.Receive(TimeSpan.FromMilliseconds(5)));
            Assert.IsNull(receiver.Receive(TimeSpan.FromMilliseconds(5)));

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveFailsWhenLinkRemotelyClosed()
      {
         DoTestReceiveFailsWhenLinkRemotelyClose(false);
      }

      [Test]
      public void TestTimedReceiveFailsWhenLinkRemotelyClosed()
      {
         DoTestReceiveFailsWhenLinkRemotelyClose(true);
      }

      private void DoTestReceiveFailsWhenLinkRemotelyClose(bool timed)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.RemoteDetach().Later(50);

            if (timed)
            {
               Assert.Throws<ClientLinkRemotelyClosedException>(() => receiver.Receive(TimeSpan.FromHours(1)));
            }
            else
            {
               Assert.Throws<ClientLinkRemotelyClosedException>(() => receiver.Receive());
            }

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryUsesUnsettledDeliveryOnOpen()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteDisposition().FromSender()
                                    .WithFirst(0)
                                    .WithSettled(true)
                                    .WithState().Accepted().Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();

            IStreamDelivery delivery = receiver.Receive();

            Wait.AssertTrue("Should eventually be remotely settled", () => delivery.RemoteSettled);
            Wait.AssertTrue(() => { return delivery.RemoteState == IDeliveryState.Accepted(); });

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryReceiveWithTransferAlreadyComplete()
      {
         DoTestStreamDeliveryReceiveWithTransferAlreadyComplete(false);
      }

      [Test]
      public void TestStreamDeliveryTryReceiveWithTransferAlreadyComplete()
      {
         DoTestStreamDeliveryReceiveWithTransferAlreadyComplete(true);
      }

      private void DoTestStreamDeliveryReceiveWithTransferAlreadyComplete(bool tryReceive)
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();

            // Ensures that stream receiver has the delivery in its queue.
            connection.OpenSender("test-sender").OpenTask.Wait();

            IStreamDelivery delivery;

            if (tryReceive)
            {
               delivery = receiver.TryReceive();
            }
            else
            {
               delivery = receiver.Receive();
            }

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryReceivedWhileTransferIsIncomplete()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsFalse(delivery.Completed);

            peer.WaitForScriptToComplete();

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithNullDeliveryTag()
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();

            Wait.AssertTrue("Should eventually be marked as completed", () => delivery.Completed);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

   }
}