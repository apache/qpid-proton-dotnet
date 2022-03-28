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
using System.Collections.Generic;
using System.Linq;
using Apache.Qpid.Proton.Client.TestSupport;
using Apache.Qpid.Proton.Types.Messaging;
using System.IO;
using Apache.Qpid.Proton.Utilities;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Buffer;

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
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = false
            };
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue", options).OpenTask.Result;

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
            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();

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
            peer.ExpectDisposition().WithSettled(true);

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

      [Test]
      public void TestStreamDeliveryRawInputStreamWithCompleteDeliveryReadByte()
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
            peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload.Length, stream.Length);
            byte[] deliveryBytes = new byte[payload.Length];
            for (int i = 0; i < payload.Length; ++i)
            {
               deliveryBytes[i] = (byte)stream.ReadByte();
            }

            Assert.AreEqual(payload, deliveryBytes);
            Assert.AreEqual(0, stream.Length - stream.Position);
            Assert.AreEqual(-1, stream.ReadByte());

            stream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamBehaviorAfterStreamClosed()
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
            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            stream.Close();

            byte[] scratch = new byte[10];

            Assert.Throws<IOException>(() => _ = stream.Length);
            Assert.Throws<IOException>(() => stream.ReadByte());
            Assert.Throws<IOException>(() => stream.Read(scratch));
            Assert.Throws<IOException>(() => stream.Read(scratch, 0, scratch.Length));

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamWithCompleteDeliveryReadBytes()
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
            peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload.Length, stream.Length);
            byte[] deliveryBytes = new byte[payload.Length];
            stream.Read(deliveryBytes);

            Assert.AreEqual(payload, deliveryBytes);
            Assert.AreEqual(0, stream.Length - stream.Position);
            Assert.AreEqual(-1, stream.ReadByte());

            stream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamWithInCompleteDeliveryReadBytes()
      {
         byte[] payload1 = CreateEncodedMessage(new Data(new byte[] { 0, 1, 2, 3, 4, 5 }));
         byte[] payload2 = CreateEncodedMessage(new Data(new byte[] { 6, 7, 8, 9, 0, 1 }));

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
                                 .WithPayload(payload1).Queue();
            peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
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
            Assert.IsFalse(delivery.Aborted);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload1.Length, stream.Length);
            byte[] deliveryBytes1 = new byte[payload1.Length];
            stream.Read(deliveryBytes1);

            Assert.AreEqual(payload1, deliveryBytes1);
            Assert.AreEqual(0, stream.Length - stream.Position);

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithPayload(payload2).Later(50);

            // Should block until more data arrives.
            byte[] deliveryBytes2 = new byte[payload2.Length];
            stream.Read(deliveryBytes2);

            Assert.AreEqual(payload2, deliveryBytes2);
            Assert.AreEqual(0, stream.Length - stream.Position);

            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            stream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamReadBytesSignalsEOFOnEmptyCompleteTransfer()
      {
         byte[] payload1 = CreateEncodedMessage(new Data(new byte[] { 0, 1, 2, 3, 4, 5 }));

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
                                 .WithPayload(payload1).Queue();
            peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
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
            Assert.IsFalse(delivery.Aborted);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload1.Length, stream.Length);
            byte[] deliveryBytes1 = new byte[payload1.Length];
            stream.Read(deliveryBytes1);

            Assert.AreEqual(payload1, deliveryBytes1);
            Assert.AreEqual(0, stream.Length - stream.Position);

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .Later(50);

            // Should block until more data arrives.
            byte[] deliveryBytes2 = new byte[payload1.Length];
            Assert.AreEqual(0, stream.Read(deliveryBytes2));
            Assert.AreEqual(0, stream.Length - stream.Position);

            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            stream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamWithInCompleteDeliverySkipBytes()
      {
         byte[] payload1 = CreateEncodedMessage(new Data(new byte[] { 0, 1, 2, 3, 4, 5 }));
         byte[] payload2 = CreateEncodedMessage(new Data(new byte[] { 6, 7, 8, 9, 0, 1 }));

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
                                 .WithPayload(payload1).Queue();
            peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
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
            Assert.IsFalse(delivery.Aborted);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload1.Length, stream.Length);
            stream.Read(new byte[payload1.Length]);
            Assert.AreEqual(0, stream.Length - stream.Position);

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithPayload(payload2).Later(50);

            // Should block until more data arrives.
            stream.Read(new byte[payload1.Length]);
            Assert.AreEqual(0, stream.Length - stream.Position);

            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            stream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamReadOpensSessionWindowForAdditionalInAdd()
      {
         byte[] body1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] body2 = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] payload1 = CreateEncodedMessage(new Data(body1));
         byte[] payload2 = CreateEncodedMessage(new Data(body2));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1000).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 1000
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = 2000
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            Stream rawStream = delivery.RawInputStream;
            Assert.IsNotNull(rawStream);

            // An initial frame has arrived but more than that is requested so the first chuck is pulled
            // from the incoming delivery and the session window opens which allows the second chunk to
            // arrive and again the session window will be opened as that chunk is moved to the reader's
            // buffer for return from the read request.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();
            peer.ExpectFlow().WithDeliveryCount(1).WithIncomingWindow(1).WithLinkCredit(9);
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);

            byte[] combinedPayloads = new byte[payload1.Length + payload2.Length];
            rawStream.Read(combinedPayloads);

            Assert.IsTrue(Statics.SequenceEquals(payload1, 0, payload1.Length, combinedPayloads, 0, payload1.Length));
            Assert.IsTrue(Statics.SequenceEquals(payload2, 0, payload2.Length, combinedPayloads, payload1.Length, payload1.Length + payload2.Length));

            rawStream.Close();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.OpenTask.Wait();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamBlockedReadBytesAborted()
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
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsFalse(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload.Length, stream.Length);
            byte[] deliveryBytes = new byte[payload.Length * 2];

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithAborted(true)
                                 .WithMessageFormat(0).Later(50);

            try
            {
               stream.Read(deliveryBytes);
               Assert.Fail("Delivery should have been aborted while waiting for more data.");
            }
            catch (IOException ioe)
            {
               logger.LogInformation("Stream read threw Ex with inner type: {0}", ioe.InnerException.GetType());
               Assert.IsTrue(ioe.InnerException is ClientDeliveryAbortedException);
            }

            stream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamClosedWithoutReadsConsumesTransfers()
      {
         byte[] body1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] body2 = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] payload1 = CreateEncodedMessage(new Data(body1));
         byte[] payload2 = CreateEncodedMessage(new Data(body2));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1000).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 1000
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = 2000,
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            Stream rawStream = delivery.RawInputStream;
            Assert.IsNotNull(rawStream);

            // An initial frame has arrived but no reads have been performed and then if closed
            // the delivery will be consumed to allow the session window to be opened and prevent
            // a stall due to an un-consumed delivery.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();
            peer.ExpectFlow().WithDeliveryCount(1).WithIncomingWindow(1).WithLinkCredit(9);

            rawStream.Close();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.OpenTask.Wait();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryRawInputStreamClosedWithoutReadsAllowsUserDisposition()
      {
         byte[] body1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] body2 = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] payload1 = CreateEncodedMessage(new Data(body1));
         byte[] payload2 = CreateEncodedMessage(new Data(body2));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1000).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 1000
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = 2000,
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            Stream rawStream = delivery.RawInputStream;
            Assert.IsNotNull(rawStream);

            // An initial frame has arrived but no reads have been performed and then if closed
            // the delivery will be consumed to allow the session window to be opened and prevent
            // a stall due to an un-consumed delivery.  The stream delivery will not auto accept
            // or auto settle the delivery as the user closed early which should indicate they
            // are rejecting the message otherwise it is a programming error on their part.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();
            peer.ExpectFlow().WithDeliveryCount(1).WithIncomingWindow(1).WithLinkCredit(9);

            rawStream.Close();

            peer.WaitForScriptToComplete();
            peer.ExpectDisposition().WithState().Rejected("invalid-format", "decode error").WithSettled(true);
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            delivery.Disposition(IDeliveryState.Rejected("invalid-format", "decode error"), true);

            receiver.OpenTask.Wait();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryUserAppliedDispositionBeforeStreamRead()
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
            peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            delivery.Disposition(IDeliveryState.Accepted(), true);

            peer.WaitForScriptToComplete();

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload.Length, stream.Length);
            byte[] deliveryBytes = new byte[payload.Length];
            for (int i = 0; i < payload.Length; ++i)
            {
               deliveryBytes[i] = (byte)stream.ReadByte();
            }

            Assert.AreEqual(payload, deliveryBytes);
            Assert.AreEqual(0, stream.Length - stream.Position);
            Assert.AreEqual(-1, stream.ReadByte());

            stream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageWithHeaderOnly()
      {
         byte[] payload = CreateEncodedMessage(new Header() { Durable = true });

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
            peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);
            Header header = message.Header;
            Assert.IsNotNull(header);

            Assert.AreSame(receiver, message.Receiver);
            Assert.AreSame(delivery, message.Delivery);

            Assert.IsNull(message.Properties);
            Assert.IsNull(message.Annotations);
            Assert.IsNull(message.ApplicationProperties);
            Assert.IsNull(message.Footer);
            Assert.IsTrue(message.Completed);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadHeaderFromStreamMessageWithoutHeaderSection()
      {
         IDictionary<Symbol, object> annotationsMap = new Dictionary<Symbol, object>();
         annotationsMap.Add(Symbol.Lookup("test-1"), Guid.NewGuid());
         annotationsMap.Add(Symbol.Lookup("test-2"), Guid.NewGuid());
         annotationsMap.Add(Symbol.Lookup("test-3"), Guid.NewGuid());

         byte[] payload = CreateEncodedMessage(new MessageAnnotations(annotationsMap));

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
            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);
            Header header = message.Header;
            Assert.IsNull(header);
            MessageAnnotations annotations = message.Annotations;
            Assert.IsNotNull(annotations);
            Assert.AreEqual(annotationsMap, annotations.Value);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestTryReadSectionBeyondWhatIsEncodedIntoMessage()
      {
         IDictionary<Symbol, object> annotationsMap = new Dictionary<Symbol, object>();
         annotationsMap.Add(Symbol.Lookup("test-1"), Guid.NewGuid());
         annotationsMap.Add(Symbol.Lookup("test-2"), Guid.NewGuid());
         annotationsMap.Add(Symbol.Lookup("test-3"), Guid.NewGuid());

         byte[] payload = CreateEncodedMessage(new Header(), new MessageAnnotations(annotationsMap));

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Properties properties = message.Properties;
            Assert.IsNull(properties);
            Header header = message.Header;
            Assert.IsNotNull(header);
            MessageAnnotations annotations = message.Annotations;
            Assert.IsNotNull(annotations);
            Assert.AreEqual(annotationsMap, annotations.Value);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadBytesFromBodyInputStreamUsingReadByteAPI()
      {
         byte[] body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] payload = CreateEncodedMessage(new Data(body));

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            Assert.IsNull(message.Header);
            Assert.IsNull(message.Annotations);
            Assert.IsNull(message.Properties);
            Assert.IsNull(delivery.Annotations);

            byte[] receivedBody = new byte[body.Length];
            for (int i = 0; i < body.Length; ++i)
            {
               receivedBody[i] = (byte)bodyStream.ReadByte();
            }
            Assert.AreEqual(body, receivedBody);
            Assert.AreEqual(-1, bodyStream.ReadByte());
            Assert.IsNull(message.Footer);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadBytesFromInputStreamUsingReadByteWithSingleByteSplitTransfers()
      {
         TestReadBytesFromBodyInputStreamWithSplitSingleByteTransfers(1);
      }

      [Test]
      public void TestReadBytesFromInputStreamUsingSingleReadBytesWithSingleByteSplitTransfers()
      {
         TestReadBytesFromBodyInputStreamWithSplitSingleByteTransfers(2);
      }

      [Test]
      public void TestReadBytesArrayFromInputStreamUsingSingleReadBytesWithSingleByteSplitTransfers()
      {
         TestReadBytesFromBodyInputStreamWithSplitSingleByteTransfers(3);
      }

      private void TestReadBytesFromBodyInputStreamWithSplitSingleByteTransfers(int option)
      {
         byte[] body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] payload = CreateEncodedMessage(new Data(body));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            for (int i = 0; i < payload.Length; ++i)
            {
               peer.RemoteTransfer().WithHandle(0)
                                    .WithDeliveryId(0)
                                    .WithDeliveryTag(new byte[] { 1 })
                                    .WithMore(true)
                                    .WithMessageFormat(0)
                                    .WithPayload(new byte[] { payload[i] }).AfterDelay(6 + i * 10).Queue();
            }
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithMessageFormat(0).AfterDelay(1000).Queue();
            peer.ExpectDisposition().WithFirst(0).WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();
            Stream bodyStream = message.Body;

            byte[] receivedBody = new byte[body.Length];

            if (option == 1)
            {
               for (int i = 0; i < body.Length; ++i)
               {
                  receivedBody[i] = (byte)bodyStream.ReadByte();
               }
               Assert.AreEqual(body, receivedBody);
            }
            else if (option == 2)
            {
               Assert.AreEqual(body.Length, bodyStream.Read(receivedBody));
               Assert.AreEqual(body, receivedBody);
            }
            else if (option == 3)
            {
               Assert.AreEqual(body.Length, bodyStream.Read(receivedBody, 0, receivedBody.Length));
               Assert.AreEqual(body, receivedBody);
            }
            else
            {
               Assert.Fail("Unknown test option");
            }

            bodyStream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamReceiverSessionCannotCreateNewResources()
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

            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenReceiver("test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenReceiver("test", new ReceiverOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenDurableReceiver("test", "test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenDurableReceiver("test", "test", new ReceiverOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenDynamicReceiver());
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenDynamicReceiver(null, new Dictionary<string, object>()));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenDynamicReceiver(new ReceiverOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenDynamicReceiver(new ReceiverOptions(), new Dictionary<string, object>()));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenSender("test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenSender("test", new SenderOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenAnonymousSender());
            Assert.Throws<ClientUnsupportedOperationException>(() => receiver.Session.OpenAnonymousSender(new SenderOptions()));

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
      public void TestReadByteArrayPayloadInChunksFromSingleTransferMessage()
      {
         TestReadPayloadInChunksFromLargerMessage(false);
      }

      [Test]
      public void TestReadBytesWithArgsPayloadInChunksFromSingleTransferMessage()
      {
         TestReadPayloadInChunksFromLargerMessage(true);
      }

      private void TestReadPayloadInChunksFromLargerMessage(bool readWithArgs)
      {
         byte[] body = new byte[100];
         Random random = new Random(Environment.TickCount);
         random.NextBytes(body);
         byte[] payload = CreateEncodedMessage(new Data(body));

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);
            Assert.AreEqual(0, delivery.MessageFormat);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            Assert.Throws<ClientUnsupportedOperationException>(() => message.MessageFormat = 1);
            Assert.IsNull(message.Header);
            Assert.IsNull(message.Annotations);
            Assert.IsNull(message.Properties);
            Assert.IsNull(delivery.Annotations);

            byte[] aggregateBody = new byte[body.Length];
            byte[] receivedBody = new byte[10];

            for (int i = 0; i < body.Length; i += 10)
            {
               if (readWithArgs)
               {
                  bodyStream.Read(receivedBody, 0, receivedBody.Length);
               }
               else
               {
                  bodyStream.Read(receivedBody);
               }

               Array.Copy(receivedBody, 0, aggregateBody, i, receivedBody.Length);
            }

            Assert.AreEqual(body, aggregateBody);
            Assert.AreEqual(0, bodyStream.Read(receivedBody, 0, receivedBody.Length));
            Assert.AreEqual(0, bodyStream.Read(receivedBody));
            Assert.AreEqual(-1, bodyStream.ReadByte());
            Assert.IsNull(message.Footer);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamReceiverMessageThrowsOnAnyMessageModificationAPI()
      {
         byte[] body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] payload = CreateEncodedMessage(new Data(body));

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();

            Assert.Throws<ClientUnsupportedOperationException>(() => message.Header = new Header());
            Assert.Throws<ClientUnsupportedOperationException>(() => message.Properties = new Properties());
            Assert.Throws<ClientUnsupportedOperationException>(() => message.ApplicationProperties = new ApplicationProperties());
            Assert.Throws<ClientUnsupportedOperationException>(() => message.Annotations = new MessageAnnotations());
            Assert.Throws<ClientUnsupportedOperationException>(() => message.Footer = new Footer());

            Assert.Throws<ClientUnsupportedOperationException>(() => message.MessageFormat = 1);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.Durable = true);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.Priority = (byte)4);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.TimeToLive = 128);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.FirstAcquirer = false);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.DeliveryCount = 10);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.MessageId = 10);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.CorrelationId = 10);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.UserId = new byte[] { 1 });
            Assert.Throws<ClientUnsupportedOperationException>(() => message.To = "test");
            Assert.Throws<ClientUnsupportedOperationException>(() => message.Subject = "test");
            Assert.Throws<ClientUnsupportedOperationException>(() => message.ReplyTo = "test");
            Assert.Throws<ClientUnsupportedOperationException>(() => message.ContentType = "test");
            Assert.Throws<ClientUnsupportedOperationException>(() => message.ContentEncoding = "test");
            Assert.Throws<ClientUnsupportedOperationException>(() => message.AbsoluteExpiryTime = 10);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.CreationTime = 10);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.GroupId = "test");
            Assert.Throws<ClientUnsupportedOperationException>(() => message.GroupSequence = 10);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.ReplyToGroupId = "test");

            Assert.Throws<ClientUnsupportedOperationException>(() => message.SetAnnotation("test", 1));
            Assert.Throws<ClientUnsupportedOperationException>(() => message.RemoveAnnotation("test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => message.SetProperty("test", 1));
            Assert.Throws<ClientUnsupportedOperationException>(() => message.RemoveProperty("test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => message.SetFooter("test", 1));
            Assert.Throws<ClientUnsupportedOperationException>(() => message.RemoveFooter("test"));

            Assert.Throws<ClientUnsupportedOperationException>(() => message.Body = Stream.Null);
            Assert.Throws<ClientUnsupportedOperationException>(() => message.AddBodySection(new AmqpValue("test")));
            Assert.Throws<ClientUnsupportedOperationException>(() => message.SetBodySections(new List<ISection>()));
            Assert.Throws<ClientUnsupportedOperationException>(() => _ = message.GetBodySections());
            Assert.Throws<ClientUnsupportedOperationException>(() => message.ClearBodySections());
            Assert.Throws<ClientUnsupportedOperationException>(() => message.ForEachBodySection((section) => { }));
            Assert.Throws<ClientUnsupportedOperationException>(() => message.Encode(new Dictionary<string, object>()));

            Stream bodyStream = message.Body;

            byte[] received = new byte[body.Length];
            Assert.IsNotNull(bodyStream.Read(received));
            bodyStream.Close();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSkipPayloadInChunksFromSingleTransferMessage()
      {
         byte[] body = new byte[100];
         Random random = new Random(Environment.TickCount);
         random.NextBytes(body);
         byte[] payload = CreateEncodedMessage(new Data(body));

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            Assert.IsNull(message.Header);
            Assert.IsNull(message.Annotations);
            Assert.IsNull(message.Properties);
            Assert.IsNull(delivery.Annotations);

            int skipSize = 10;
            byte[] scratchBuffer = new byte[skipSize];

            for (int i = 0; i < body.Length; i += skipSize)
            {
               Assert.AreNotEqual(0, bodyStream.Read(scratchBuffer));
            }

            Assert.AreEqual(0, bodyStream.Read(scratchBuffer, 0, scratchBuffer.Length));
            Assert.AreEqual(0, bodyStream.Read(scratchBuffer));
            Assert.AreEqual(-1, bodyStream.ReadByte());
            Assert.IsNull(message.Footer);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadByteArrayPayloadInChunksFromMultipleTransfersMessage()
      {
         TestReadPayloadInChunksFromLargerMultiTransferMessage(false);
      }

      [Test]
      public void TestReadBytesWithArgsPayloadInChunksFromMultipleTransferMessage()
      {
         TestReadPayloadInChunksFromLargerMultiTransferMessage(true);
      }

      private void TestReadPayloadInChunksFromLargerMultiTransferMessage(bool readWithArgs)
      {
         int seed = Environment.TickCount;
         Random random = new Random(seed);
         int numChunks = 4;
         int chunkSize = 30;

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            for (int i = 0; i < numChunks; ++i)
            {
               byte[] chunk = new byte[chunkSize];
               random.NextBytes(chunk);
               peer.RemoteTransfer().WithHandle(0)
                                    .WithDeliveryId(0)
                                    .WithDeliveryTag(new byte[] { 1 })
                                    .WithMore(true)
                                    .WithMessageFormat(0)
                                    .WithPayload(CreateEncodedMessage(new Data(chunk))).Queue();
            }
            peer.RemoteTransfer().WithHandle(0).WithMore(false).Queue();
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            Assert.IsNull(message.Header);
            Assert.IsNull(message.Annotations);
            Assert.IsNull(message.Properties);
            Assert.IsNull(delivery.Annotations);

            byte[] readChunk = new byte[chunkSize];
            byte[] receivedBody = new byte[3];

            random = new Random(seed);

            int totalBytesRead = 0;

            for (int i = 0; i < numChunks; ++i)
            {
               for (int j = 0; j < readChunk.Length; j += receivedBody.Length)
               {
                  int bytesRead = 0;
                  if (readWithArgs)
                  {
                     bytesRead = bodyStream.Read(receivedBody, 0, receivedBody.Length);
                  }
                  else
                  {
                     bytesRead = bodyStream.Read(receivedBody);
                  }

                  totalBytesRead += bytesRead;

                  Array.Copy(receivedBody, 0, readChunk, j, bytesRead);
               }

               byte[] chunk = new byte[chunkSize];
               random.NextBytes(chunk);
               Assert.AreEqual(chunk, readChunk);
            }

            Assert.AreEqual(chunkSize * numChunks, totalBytesRead);
            Assert.AreEqual(0, bodyStream.Read(receivedBody, 0, receivedBody.Length));
            Assert.AreEqual(0, bodyStream.Read(receivedBody));
            Assert.AreEqual(-1, bodyStream.ReadByte());
            Assert.IsNull(message.Footer);

            bodyStream.Close();

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
      public void TestReadPayloadFromSplitFrameTransferWithBufferLargerThanTotalPayload()
      {
         int seed = Environment.TickCount;
         Random random = new Random(seed);
         int numChunks = 4;
         int chunkSize = 30;

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            for (int i = 0; i < numChunks; ++i)
            {
               byte[] chunk = new byte[chunkSize];
               random.NextBytes(chunk);
               peer.RemoteTransfer().WithHandle(0)
                                    .WithDeliveryId(0)
                                    .WithDeliveryTag(new byte[] { 1 })
                                    .WithMore(true)
                                    .WithMessageFormat(0)
                                    .WithPayload(CreateEncodedMessage(new Data(chunk))).Queue();
            }
            peer.RemoteTransfer().WithHandle(0).WithMore(false).Queue();
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = (IStreamReceiver)connection.OpenStreamReceiver("test-queue").OpenTask.Result;
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            Assert.IsNull(message.Header);
            Assert.IsNull(message.Annotations);
            Assert.IsNull(message.Properties);
            Assert.IsNull(delivery.Annotations);

            byte[] receivedBody = new byte[(chunkSize * numChunks) + 100];
            Array.Fill(receivedBody, (byte)0);
            int totalBytesRead = bodyStream.Read(receivedBody);

            Assert.AreEqual(chunkSize * numChunks, totalBytesRead);
            Assert.AreEqual(0, bodyStream.Read(receivedBody, 0, receivedBody.Length));
            Assert.AreEqual(0, bodyStream.Read(receivedBody));
            Assert.AreEqual(-1, bodyStream.ReadByte());
            Assert.IsNull(message.Footer);

            // Regenerate what should have been sent plus empty trailing section to
            // check that the read doesn't write anything into the area we gave beyond
            // what was expected payload size.
            random = new Random(seed);
            byte[] regeneratedPayload = new byte[numChunks * chunkSize + 100];
            Array.Fill(regeneratedPayload, (byte)0);
            for (int i = 0; i < numChunks; ++i)
            {
               byte[] chunk = new byte[chunkSize];
               random.NextBytes(chunk);
               Array.Copy(chunk, 0, regeneratedPayload, chunkSize * i, chunkSize);
            }

            Assert.AreEqual(regeneratedPayload, receivedBody);

            bodyStream.Close();

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
      public void TestStreamReadOpensSessionWindowForAdditionalInput()
      {
         byte[] body1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] body2 = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] payload1 = CreateEncodedMessage(new Data(body1));
         byte[] payload2 = CreateEncodedMessage(new Data(body2));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1000).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 1000
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = 2000
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            // Creating the input stream instance should read the first chunk of data from the incoming
            // delivery which should result in a new credit being available to expand the session window.
            // An additional transfer should be placed into the delivery buffer but not yet read since
            // the user hasn't read anything.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            // Once the read of all data completes the session window should be opened and the
            // stream should mark the delivery as accepted and settled since we are in auto settle
            // mode and there is nothing more to read.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(1).WithIncomingWindow(1).WithLinkCredit(9);

            byte[] combinedPayloads = new byte[body1.Length + body2.Length];
            bodyStream.Read(combinedPayloads);

            Assert.IsTrue(Statics.SequenceEquals(body1, 0, body1.Length, combinedPayloads, 0, body1.Length));
            Assert.IsTrue(Statics.SequenceEquals(body2, 0, body2.Length, combinedPayloads, body1.Length, body1.Length + body2.Length));

            bodyStream.Close();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.OpenTask.Wait();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamReadOpensSessionWindowForAdditionalInputAndGrantsCreditOnClose()
      {
         byte[] body1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] body2 = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] payload1 = CreateEncodedMessage(new Data(body1));
         byte[] payload2 = CreateEncodedMessage(new Data(body2));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1000).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(1);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 1000
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = 2000,
               CreditWindow = 1
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            // Creating the input stream instance should read the first chunk of data from the incoming
            // delivery which should result in a new credit being available to expand the session window.
            // An additional transfer should be placed into the delivery buffer but not yet read since
            // the user hasn't read anything.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(1);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.ExpectFlow().WithDeliveryCount(1).WithIncomingWindow(0).WithLinkCredit(1);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            // Once the read of all data completes the session window should be opened .
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(1).WithIncomingWindow(1).WithLinkCredit(1);

            byte[] combinedPayloads = new byte[body1.Length + body2.Length];
            bodyStream.Read(combinedPayloads);

            Assert.IsTrue(Statics.SequenceEquals(body1, 0, body1.Length, combinedPayloads, 0, body1.Length));
            Assert.IsTrue(Statics.SequenceEquals(body2, 0, body2.Length, combinedPayloads, body1.Length, body1.Length + body2.Length));

            bodyStream.Close();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.OpenTask.Wait();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamReadOfAllPayloadConsumesTrailingFooterOnClose()
      {
         byte[] body1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] body2 = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] payload1 = CreateEncodedMessage(new Data(body1));
         byte[] payload2 = CreateEncodedMessage(new Data(body2));
         Footer footers = new Footer(new Dictionary<Symbol, object>());
         footers.Value.Add(Symbol.Lookup("footer-key"), "test");
         byte[] payload3 = CreateEncodedMessage(footers);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1000).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 1000
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = 2000,
               AutoAccept = true
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            // Creating the input stream instance should read the first chunk of data from the incoming
            // delivery which should result in a new credit being available to expand the session window.
            // An additional transfer should be placed into the delivery buffer but not yet read since
            // the user hasn't read anything.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            // Once the read of all data completes the session window should be opened and the
            // stream should mark the delivery as accepted and settled since we are in auto settle
            // mode and there is nothing more to read.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload3).Queue();
            peer.ExpectFlow().WithDeliveryCount(1).WithIncomingWindow(1).WithLinkCredit(9);
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);

            byte[] combinedPayloads = new byte[body1.Length + body2.Length];
            bodyStream.Read(combinedPayloads);

            Assert.IsTrue(Statics.SequenceEquals(body1, 0, body1.Length, combinedPayloads, 0, body1.Length));
            Assert.IsTrue(Statics.SequenceEquals(body2, 0, body2.Length, combinedPayloads, body1.Length, body1.Length + body2.Length));

            bodyStream.Close();

            Footer footer = message.Footer;
            Assert.IsNotNull(footer);
            Assert.IsFalse(footer.Value.Count == 0);
            Assert.IsTrue(footer.Value.ContainsKey(Symbol.Lookup("footer-key")));

            Assert.IsTrue(message.HasFooters);
            Assert.IsTrue(message.HasFooter("footer-key"));
            message.ForEachFooter((key, value) =>
            {
               Assert.AreEqual(key, "footer-key");
               Assert.AreEqual(value, "test");
            });

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.OpenTask.Wait();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadBytesFromBodyInputStreamWithinTransactedSession()
      {
         byte[] body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] payload = CreateEncodedMessage(new Data(body));
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

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
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");

            peer.WaitForScriptToComplete();
            peer.ExpectCoordinatorAttach().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectDeclare().Accept(txnId);
            peer.ExpectDisposition().WithSettled(true).WithState().Transactional().WithTxnId(txnId).WithAccepted();
            peer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();

            receiver.Session.BeginTransaction();

            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            Assert.IsNull(message.Header);
            Assert.IsNull(message.Annotations);
            Assert.IsNull(message.Properties);
            Assert.IsNull(delivery.Annotations);

            byte[] receivedBody = new byte[body.Length];
            for (int i = 0; i < body.Length; ++i)
            {
               receivedBody[i] = (byte)bodyStream.ReadByte();
            }
            Assert.AreEqual(body, receivedBody);
            Assert.AreEqual(-1, bodyStream.ReadByte());

            receiver.Session.CommitTransaction();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryHandlesInvalidHeaderEncoding()
      {
         byte[] payload = CreateInvalidHeaderEncoding();

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
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", options);

            peer.WaitForScriptToComplete();
            peer.ExpectDisposition().WithState().Rejected("decode-error", "failed reading message header");

            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();

            Assert.Throws<ClientException>(() => _ = message.Header);
            Assert.Throws<ClientException>(() => _ = message.Body);

            delivery.Reject("decode-error", "failed reading message header");

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryHandlesInvalidDeliveryAnnotationsEncoding()
      {
         byte[] payload = CreateInvalidDeliveryAnnotationsEncoding();

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
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", options);

            peer.WaitForScriptToComplete();
            peer.ExpectDisposition().WithState().Rejected("decode-error", "failed reading message header");

            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();

            Assert.Throws<ClientException>(() => _ = delivery.Annotations);
            Assert.Throws<ClientException>(() => _ = message.Body);

            delivery.Reject("decode-error", "failed reading message header");

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryHandlesInvalidMessageAnnotationsEncoding()
      {
         byte[] payload = CreateInvalidMessageAnnotationsEncoding();

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
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", options);

            peer.WaitForScriptToComplete();
            peer.ExpectDisposition().WithState().Rejected("decode-error", "failed reading message header");

            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();

            Assert.Throws<ClientException>(() => _ = message.Annotations);
            Assert.Throws<ClientException>(() => _ = message.Body);

            delivery.Reject("decode-error", "failed reading message header");

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryHandlesInvalidPropertiesEncoding()
      {
         byte[] payload = CreateInvalidPropertiesEncoding();

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
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", options);

            peer.WaitForScriptToComplete();
            peer.ExpectDisposition().WithState().Rejected("decode-error", "failed reading message header");

            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();

            Assert.Throws<ClientException>(() => _ = message.Properties);
            Assert.Throws<ClientException>(() => _ = message.Body);

            delivery.Reject("decode-error", "failed reading message header");

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryHandlesInvalidApplicationPropertiesEncoding()
      {
         byte[] payload = CreateInvalidApplicationPropertiesEncoding();

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
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", options);

            peer.WaitForScriptToComplete();
            peer.ExpectDisposition().WithState().Rejected("decode-error", "failed reading message header");

            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();

            Assert.Throws<ClientException>(() => _ = message.ApplicationProperties);
            Assert.Throws<ClientException>(() => _ = message.Body);

            delivery.Reject("decode-error", "failed reading message header");

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamDeliveryHandlesInvalidHeaderEncodingDuringBodyStreamOpen()
      {
         byte[] payload = CreateInvalidHeaderEncoding();

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
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = false
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", options);

            peer.WaitForScriptToComplete();
            peer.ExpectDisposition().WithState().Rejected("decode-error", "failed reading message header");

            IStreamDelivery delivery = receiver.Receive();

            IStreamReceiverMessage message = delivery.Message();

            Assert.Throws<ClientException>(() => _ = message.Body);

            delivery.Reject("decode-error", "failed reading message header");

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionDropsDuringStreamedBodyRead()
      {
         byte[] body1 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] body2 = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] payload1 = CreateEncodedMessage(new Data(body1));
         byte[] payload2 = CreateEncodedMessage(new Data(body2));

         CountdownEvent disconnected = new CountdownEvent(1);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1000).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(1);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 1000
            };
            connectionOptions.DisconnectedHandler = (conn, eventArgs) => disconnected.Signal();
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               ReadBufferSize = 2000,
               CreditWindow = 1
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();

            // Creating the input stream instance should read the first chunk of data from the incoming
            // delivery which should result in a new credit being available to expand the session window.
            // An additional transfer should be placed into the delivery buffer but not yet read since
            // the user hasn't read anything.
            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDeliveryCount(0).WithIncomingWindow(1).WithLinkCredit(1);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();
            peer.DropAfterLastHandler();

            Stream bodyStream = message.Body;
            Assert.IsNotNull(bodyStream);

            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(10)));

            byte[] readPayload = new byte[body1.Length + body2.Length];

            try
            {
               bodyStream.Read(readPayload);
               Assert.Fail("Should not be able to read from closed connection stream");
            }
            catch (IOException)
            {
               // Connection should be down now.
            }

            bodyStream.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestFrameSizeViolationWhileWaitingForIncomingStreamReceiverContent()
      {
         byte[] overFrameSizeLimitFrameHeader = new byte[] { 0x00, (byte)0xA0, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00 };

         byte[] body = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         byte[] payload = CreateEncodedMessage(new Data(body));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(65535).Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(1);
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
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               MaxFrameSize = 65535
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamReceiverOptions streamOptions = new StreamReceiverOptions()
            {
               CreditWindow = 1
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue", streamOptions);
            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();
            Stream stream = message.Body;

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();
            peer.RemoteBytes().WithBytes(overFrameSizeLimitFrameHeader).Later(10);

            byte[] bytesToRead = new byte[body.Length * 2];

            try
            {
               stream.Read(bytesToRead);
               Assert.Fail("Should throw an error indicating issue with read of payload");
            }
            catch (IOException)
            {
               // Expected
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamReceiverTryReadAmqpSequenceBytes()
      {
         List<string> stringList = new List<string>();
         stringList.Add("Hello World");
         byte[] payload = CreateEncodedMessage(new AmqpSequence(stringList));

         DoTestStreamReceiverReadsNonDataSectionBody(payload);
      }

      [Test]
      public void TestStreamReceiverTryReadAmqpValueBytes()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         DoTestStreamReceiverReadsNonDataSectionBody(payload);
      }

      private void DoTestStreamReceiverReadsNonDataSectionBody(byte[] payload)
      {
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
                                 .WithSettled(true)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");

            peer.WaitForScriptToComplete();

            IStreamDelivery delivery = receiver.Receive();
            IStreamReceiverMessage message = delivery.Message();
            try
            {
               _ = message.Body;
               Assert.Fail("Should not return a stream since we cannot read this type");
            }
            catch (ClientException)
            {
               // Expected
            }

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadMessageHeaderFromStreamReceiverMessage()
      {
         Header header = new Header();

         header.DeliveryCount = uint.MaxValue;
         header.Durable = true;
         header.FirstAcquirer = false;
         header.Priority = (byte)255;
         header.TimeToLive = int.MaxValue;

         byte[] payload = CreateEncodedMessage(header);

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Header readHeader = message.Header;
            Assert.IsNotNull(readHeader);
            Assert.IsNull(message.Body);

            Assert.AreEqual(int.MaxValue, message.TimeToLive);
            Assert.AreEqual(true, message.Durable);
            Assert.AreEqual(false, message.FirstAcquirer);
            Assert.AreEqual((byte)255, message.Priority);
            Assert.AreEqual(uint.MaxValue, message.DeliveryCount);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadMessagePropertiesFromStreamReceiverMessage()
      {
         Properties properties = new Properties();

         properties.AbsoluteExpiryTime = int.MaxValue;
         properties.ContentEncoding = "utf8";
         properties.ContentType = "text/plain";
         properties.CorrelationId = Guid.NewGuid();
         properties.CreationTime = ushort.MaxValue;
         properties.GroupId = "Group";
         properties.GroupSequence = uint.MaxValue;
         properties.MessageId = Guid.NewGuid();
         properties.ReplyTo = "replyTo";
         properties.ReplyToGroupId = "group-1";
         properties.Subject = "test";
         properties.To = "queue";
         properties.UserId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 5, 6, 9 });

         byte[] payload = CreateEncodedMessage(new Header(), properties);

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Assert.IsFalse(message.HasProperties);
            Assert.IsFalse(message.HasFooters);
            Assert.IsFalse(message.HasAnnotations);

            Properties readProperties = message.Properties;
            Assert.IsNotNull(readProperties);
            Header header = message.Header;
            Assert.IsNotNull(header);
            Assert.IsNull(message.Body);

            Assert.AreEqual(int.MaxValue, message.AbsoluteExpiryTime);
            Assert.AreEqual("utf8", message.ContentEncoding);
            Assert.AreEqual("text/plain", message.ContentType);
            Assert.AreEqual(properties.CorrelationId, message.CorrelationId);
            Assert.AreEqual(ushort.MaxValue, message.CreationTime);
            Assert.AreEqual(uint.MaxValue, message.GroupSequence);
            Assert.AreEqual(properties.MessageId, message.MessageId);
            Assert.AreEqual("replyTo", message.ReplyTo);
            Assert.AreEqual("group-1", message.ReplyToGroupId);
            Assert.AreEqual("test", message.Subject);
            Assert.AreEqual("queue", message.To);
            Assert.AreEqual(new byte[] { 0, 1, 5, 6, 9 }, message.UserId);

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReadApplicationPropertiesStreamReceiverMessage()
      {
         IDictionary<string, object> propertiesMap = new Dictionary<string, object>();
         ApplicationProperties appProperties = new ApplicationProperties(propertiesMap);

         propertiesMap.Add("property1", uint.MaxValue);
         propertiesMap.Add("property2", 1u);
         propertiesMap.Add("property3", 0u);

         byte[] payload = CreateEncodedMessage(appProperties);

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
            peer.ExpectDisposition().WithFirst(0).WithState().Accepted().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-queue");
            IStreamDelivery delivery = receiver.Receive();

            Assert.IsNotNull(delivery);
            Assert.IsTrue(delivery.Completed);
            Assert.IsFalse(delivery.Aborted);

            IStreamReceiverMessage message = delivery.Message();
            Assert.IsNotNull(message);

            Assert.IsTrue(message.HasProperties);
            Assert.IsFalse(message.HasFooters);
            Assert.IsFalse(message.HasAnnotations);

            Assert.IsFalse(message.HasProperty("property"));
            Assert.AreEqual(uint.MaxValue, message.GetProperty("property1"));
            Assert.AreEqual(1, message.GetProperty("property2"));
            Assert.AreEqual(0u, message.GetProperty("property3"));

            message.ForEachProperty((key, value) =>
            {
               Assert.IsTrue(propertiesMap.ContainsKey(key));
               Assert.AreEqual(value, propertiesMap[key]);
            });

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenDrainTimeoutExceeded()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectFlow().WithDrain(true);
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions receiverOptions = new StreamReceiverOptions()
            {
               DrainTimeout = 15
            };
            IReceiver receiver = connection.OpenStreamReceiver("test-queue", receiverOptions).OpenTask.Result;

            try
            {
               receiver.DrainAsync().Wait();
               Assert.Fail("Drain call should fail timeout exceeded.");
            }
            catch (Exception cliEx)
            {
               logger.LogInformation("Receiver threw error on drain call", cliEx);
               Assert.IsTrue(cliEx.InnerException is ClientOperationTimedOutException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenConnectionDrainTimeoutExceeded()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectFlow().WithDrain(true);
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions()
            {
               DrainTimeout = 20
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            IReceiver receiver = connection.OpenStreamReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.DrainAsync().Wait();
               Assert.Fail("Drain call should fail timeout exceeded.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call", cliEx);
               Assert.IsTrue(cliEx.InnerException is ClientOperationTimedOutException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainCompletesWhenReceiverHasNoCredit()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IReceiver receiver = connection.OpenStreamReceiver("test-queue", new StreamReceiverOptions()
            {
               CreditWindow = 0
            });
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();

            receiver.DrainAsync().Wait(TimeSpan.FromSeconds(10));

            // Close things down
            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete(); ;
         }
      }

      [Test]
      public void TestDrainAdditionalDrainCallThrowsWhenReceiverStillDraining()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectFlow().WithDrain(true);
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions receiverOptions = new StreamReceiverOptions();
            IReceiver receiver = connection.OpenStreamReceiver("test-queue", receiverOptions).OpenTask.Result;

            receiver.DrainAsync();

            try
            {
               receiver.DrainAsync().Wait();
               Assert.Fail("Drain call should fail since already draining.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call", cliEx);
               Assert.IsTrue(cliEx.InnerException is ClientIllegalStateException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetRemotePropertiesWaitsForRemoteAttach()
      {
         TryReadReceiverRemoteProperties(true);
      }

      [Test]
      public void TestReceiverGetRemotePropertiesFailsAfterOpenTimeout()
      {
         TryReadReceiverRemoteProperties(false);
      }

      private void TryReadReceiverRemoteProperties(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               OpenTimeout = 150
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-receiver", options);

            peer.WaitForScriptToComplete();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            expectedProperties.Add("TEST", "test-property");

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.ExpectEnd().Respond();
               peer.RespondToLastAttach().WithPropertiesMap(expectedProperties).Later(10);
            }
            else
            {
               peer.ExpectDetach();
               peer.ExpectEnd();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(receiver.Properties, "Remote should have responded with a remote properties value");
               Assert.AreEqual(expectedProperties, receiver.Properties);
            }
            else
            {
               try
               {
                  _ = receiver.Properties;
                  Assert.Fail("Should failed to get remote state due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               receiver.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetRemoteOfferedCapabilitiesWaitsForRemoteAttach()
      {
         TryReadReceiverRemoteOfferedCapabilities(true);
      }

      [Test]
      public void TestReceiverGetRemoteOfferedCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadReceiverRemoteOfferedCapabilities(false);
      }

      private void TryReadReceiverRemoteOfferedCapabilities(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               OpenTimeout = 150
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-receiver", options);

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.ExpectEnd().Respond();
               peer.RespondToLastAttach().WithOfferedCapabilities("QUEUE").Later(10);
            }
            else
            {
               peer.ExpectDetach();
               peer.ExpectEnd();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(receiver.OfferedCapabilities, "Remote should have responded with a remote offered Capabilities value");
               Assert.AreEqual(1, receiver.OfferedCapabilities.Count());
               Assert.AreEqual("QUEUE", receiver.OfferedCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = receiver.OfferedCapabilities;
                  Assert.Fail("Should failed to get remote state due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               receiver.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetRemoteDesiredCapabilitiesWaitsForRemoteAttach()
      {
         tryReadReceiverRemoteDesiredCapabilities(true);
      }

      [Test]
      public void TestReceiverGetRemoteDesiredCapabilitiesFailsAfterOpenTimeout()
      {
         tryReadReceiverRemoteDesiredCapabilities(false);
      }

      private void tryReadReceiverRemoteDesiredCapabilities(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               OpenTimeout = 150
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-receiver", options);

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.ExpectEnd().Respond();
               peer.RespondToLastAttach().WithDesiredCapabilities("Error-Free").Later(10);
            }
            else
            {
               peer.ExpectDetach();
               peer.ExpectEnd();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(receiver.DesiredCapabilities, "Remote should have responded with a remote desired Capabilities value");
               Assert.AreEqual(1, receiver.DesiredCapabilities.Count());
               Assert.AreEqual("Error-Free", receiver.DesiredCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = receiver.DesiredCapabilities;
                  Assert.Fail("Should failed to get remote state due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               receiver.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetTargetWaitsForRemoteAttach()
      {
         tryReadReceiverTarget(true);
      }

      [Test]
      public void TestReceiverGetTargetFailsAfterOpenTimeout()
      {
         tryReadReceiverTarget(false);
      }

      private void tryReadReceiverTarget(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               OpenTimeout = 150
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-receiver", options);

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.ExpectEnd().Respond();
               peer.RespondToLastAttach().Later(10);
            }
            else
            {
               peer.ExpectDetach();
               peer.ExpectEnd();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(receiver.Target, "Remote should have responded with a Target value");
            }
            else
            {
               try
               {
                  _ = receiver.Target;
                  Assert.Fail("Should failed to get remote source due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               receiver.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetSourceWaitsForRemoteAttach()
      {
         tryReadReceiverSource(true);
      }

      [Test]
      public void TestReceiverGetSourceFailsAfterOpenTimeout()
      {
         tryReadReceiverSource(false);
      }

      private void tryReadReceiverSource(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               OpenTimeout = 150
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-receiver", options);

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.ExpectEnd().Respond();
               peer.RespondToLastAttach().Later(10);
            }
            else
            {
               peer.ExpectDetach();
               peer.ExpectEnd();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(receiver.Source, "Remote should have responded with a Source value");
               Assert.AreEqual("test-receiver", receiver.Source.Address);
            }
            else
            {
               try
               {
                  _ = receiver.Source;
                  Assert.Fail("Should failed to get remote source due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               receiver.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverCreditReplenishedAfterSyncReceiveAutoAccept()
      {
         DoTestReceiverCreditReplenishedAfterSyncReceive(true);
      }

      [Test]
      public void TestReceiverCreditReplenishedAfterSyncReceiveManualAccept()
      {
         DoTestReceiverCreditReplenishedAfterSyncReceive(false);
      }

      public void DoTestReceiverCreditReplenishedAfterSyncReceive(bool autoAccept)
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            for (uint i = 0; i < 10; ++i)
            {
               peer.RemoteTransfer().WithDeliveryId(i)
                                    .WithMore(false)
                                    .WithMessageFormat(0)
                                    .WithPayload(payload).Queue();
            }
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamReceiverOptions options = new StreamReceiverOptions()
            {
               AutoAccept = autoAccept,
               CreditWindow = 10
            };
            IStreamReceiver receiver = connection.OpenStreamReceiver("test-receiver", options);

            peer.WaitForScriptToComplete();
            if (autoAccept)
            {
               peer.ExpectDisposition().WithFirst(0);
               peer.ExpectDisposition().WithFirst(1);
            }

            // Consume messages 1 and 2 which should not provoke credit replenishment
            // as there are still 8 outstanding which is above the 70% mark
            Assert.IsNotNull(receiver.Receive()); // #1
            Assert.IsNotNull(receiver.Receive()); // #2

            peer.WaitForScriptToComplete();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().Respond();
            if (autoAccept)
            {
               peer.ExpectDisposition().WithFirst(2);
            }
            peer.ExpectFlow().WithLinkCredit(3);

            connection.OpenSender("test").OpenTask.Result.Close();

            // Now consume message 3 which will trip the replenish barrier and the
            // credit should be updated to reflect that we still have 7 queued
            Assert.IsNotNull(receiver.Receive());  // #3

            peer.WaitForScriptToComplete();
            if (autoAccept)
            {
               peer.ExpectDisposition().WithFirst(3);
               peer.ExpectDisposition().WithFirst(4);
            }

            // Consume messages 4 and 5 which should not provoke credit replenishment
            // as there are still 5 outstanding plus the credit we sent last time
            // which is above the 70% mark
            Assert.IsNotNull(receiver.Receive()); // #4
            Assert.IsNotNull(receiver.Receive()); // #5

            peer.WaitForScriptToComplete();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().Respond();
            if (autoAccept)
            {
               peer.ExpectDisposition().WithFirst(5);
            }
            peer.ExpectFlow().WithLinkCredit(6);

            connection.OpenSender("test").OpenTask.Result.Close();

            // Consume number 6 which means we only have 4 outstanding plus the three
            // that we sent last time we flowed which is 70% of possible prefetch so
            // we should flow to top off credit which would be 6 since we have four
            // still pending
            Assert.IsNotNull(receiver.Receive()); // #6

            peer.WaitForScriptToComplete();
            if (autoAccept)
            {
               peer.ExpectDisposition().WithFirst(6);
               peer.ExpectDisposition().WithFirst(7);
            }

            // Consume deliveries 7 and 8 which should not flow as we should be
            // above the threshold of 70% since we would now have 2 outstanding
            // and 6 credits on the link
            Assert.IsNotNull(receiver.Receive()); // #7
            Assert.IsNotNull(receiver.Receive()); // #8

            peer.WaitForScriptToComplete();
            if (autoAccept)
            {
               peer.ExpectDisposition().WithFirst(8);
               peer.ExpectDisposition().WithFirst(9);
            }

            // Now consume 9 and 10 but we still shouldn't flow more credit because
            // the link credit is above the 50% mark for overall credit windowing.
            Assert.IsNotNull(receiver.Receive()); // #9
            Assert.IsNotNull(receiver.Receive()); // #10

            peer.WaitForScriptToComplete();

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}