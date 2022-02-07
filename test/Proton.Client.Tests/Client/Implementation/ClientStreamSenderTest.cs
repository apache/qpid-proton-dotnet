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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Client.Exceptions;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientStreamSenderTest : ClientBaseTestFixture
   {
      [Test]
      public void TestSendWhenCreditIsAvailable()
      {
         DoTestSendWhenCreditIsAvailable(false, false);
      }

      [Test]
      public void TestTrySendWhenCreditIsAvailable()
      {
         DoTestSendWhenCreditIsAvailable(true, false);
      }

      [Test]
      public void TestSendWhenCreditIsAvailableWithDeliveryAnnotations()
      {
         DoTestSendWhenCreditIsAvailable(false, true);
      }

      [Test]
      public void TestTrySendWhenCreditIsAvailableWithDeliveryAnnotations()
      {
         DoTestSendWhenCreditIsAvailable(true, true);
      }

      private void DoTestSendWhenCreditIsAvailable(bool trySend, bool addDeliveryAnnotations)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithDeliveryCount(0)
                             .WithLinkCredit(10)
                             .WithIncomingWindow(1024)
                             .WithOutgoingWindow(10)
                             .WithNextIncomingId(0)
                             .WithNextOutgoingId(1).Queue();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            sender.OpenTask.Wait();

            // This ensures that the flow to sender is processed before we try-send
            IReceiver receiver = connection.OpenReceiver("test-queue", new ReceiverOptions()
            {
               CreditWindow = 0
            });
            receiver.OpenTask.Wait();

            IDictionary<string, object> deliveryAnnotations = new Dictionary<string, object>();
            deliveryAnnotations.Add("da1", 1);
            deliveryAnnotations.Add("da2", 2);
            deliveryAnnotations.Add("da3", 3);
            DeliveryAnnotationsMatcher daMatcher = new DeliveryAnnotationsMatcher(true);
            daMatcher.WithEntry("da1", Test.Driver.Matchers.Is.EqualTo(1));
            daMatcher.WithEntry("da2", Test.Driver.Matchers.Is.EqualTo(2));
            daMatcher.WithEntry("da3", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            if (addDeliveryAnnotations)
            {
               payloadMatcher.DeliveryAnnotationsMatcher = daMatcher;
            }
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            ITracker tracker;
            if (trySend)
            {
               if (addDeliveryAnnotations)
               {
                  tracker = sender.TrySend(message, deliveryAnnotations);
               }
               else
               {
                  tracker = sender.TrySend(message);
               }
            }
            else
            {
               if (addDeliveryAnnotations)
               {
                  tracker = sender.Send(message, deliveryAnnotations);
               }
               else
               {
                  tracker = sender.Send(message);
               }
            }

            Assert.IsNotNull(tracker);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenStreamSenderWithLinCapabilities()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender()
                               .WithTarget().WithCapabilities("queue").And()
                               .Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamSenderOptions senderOptions = new StreamSenderOptions();
            senderOptions.TargetOptions.Capabilities = new string[] { "queue" };
            IStreamSender sender = connection.OpenStreamSender("test-queue", senderOptions);

            sender.OpenTask.Wait();
            sender.Close();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenStreamSenderAppliesDefaultSessionOutgoingWindow()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender()
                               .WithTarget().WithCapabilities("queue").And()
                               .Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamSenderOptions senderOptions = new StreamSenderOptions();
            senderOptions.TargetOptions.Capabilities = new string[] { "queue" };
            ClientStreamSender sender = (ClientStreamSender)connection.OpenStreamSender("test-queue", senderOptions);

            Assert.AreEqual(StreamSenderOptions.DEFAULT_PENDING_WRITES_BUFFER_SIZE,
                            sender.ProtonSender.Session.OutgoingCapacity);

            sender.OpenTask.Wait();
            sender.Close();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenStreamSenderAppliesConfiguredSessionOutgoingWindow()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender()
                               .WithTarget().WithCapabilities("queue").And()
                               .Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            uint PENDING_WRITES_BUFFER_SIZE = StreamSenderOptions.DEFAULT_PENDING_WRITES_BUFFER_SIZE / 2;

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamSenderOptions senderOptions = new StreamSenderOptions()
            {
               PendingWriteBufferSize = PENDING_WRITES_BUFFER_SIZE
            };
            senderOptions.TargetOptions.Capabilities = new string[] { "queue" };
            ClientStreamSender sender = (ClientStreamSender)connection.OpenStreamSender("test-queue", senderOptions);

            Assert.AreEqual(PENDING_WRITES_BUFFER_SIZE, sender.ProtonSender.Session.OutgoingCapacity);

            sender.OpenTask.Wait();
            sender.Close();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Stream sender message not fully implemented yet")]
      [Test]
      public void TestSendCustomMessageWithMultipleAmqpValueSections()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectBegin().Respond(); // Hidden session for stream sender
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.ExpectAttach().Respond();  // Open a receiver to ensure sender link has processed
            peer.ExpectFlow();              // the inbound flow frame we sent previously before send.
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;

            StreamSenderOptions options = new StreamSenderOptions();
            options.DeliveryMode = DeliveryMode.AtMostOnce;
            options.WriteBufferSize = uint.MaxValue;

            IStreamSender sender = connection.OpenStreamSender("test-qos", options);

            // Create a custom message format send context and ensure that no early buffer writes take place
            IStreamSenderMessage message = sender.BeginMessage();

            Assert.AreEqual(sender, message.Sender);
            Assert.IsNull(message.Tracker);

            Assert.AreEqual(Header.DEFAULT_PRIORITY, message.Priority);
            Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT, message.DeliveryCount);
            Assert.AreEqual(Header.DEFAULT_FIRST_ACQUIRER, message.FirstAcquirer);
            Assert.AreEqual(Header.DEFAULT_TIME_TO_LIVE, message.TimeToLive);
            Assert.AreEqual(Header.DEFAULT_DURABILITY, message.Durable);

            message.MessageFormat = 17;

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            // Note: This is a specification violation but could be used by other message formats
            //       and we don't attempt to enforce at the Send Context what users write
            AmqpValueMatcher bodyMatcher1 = new AmqpValueMatcher("one", true);
            AmqpValueMatcher bodyMatcher2 = new AmqpValueMatcher("two", true);
            AmqpValueMatcher bodyMatcher3 = new AmqpValueMatcher("three", false);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.AddMessageContentMatcher(bodyMatcher1);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher2);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher3);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithMore(false).WithMessageFormat(17).WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            message.Header = header;
            message.AddBodySection(new AmqpValue("one"));
            message.AddBodySection(new AmqpValue("two"));
            message.AddBodySection(new AmqpValue("three"));

            message.Complete();

            Assert.IsNotNull(message.Tracker);
            Assert.AreEqual(17, message.MessageFormat);
            Assert.IsTrue(message.Tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.Settled);
            Assert.Throws<ClientIllegalStateException>(() => message.AddBodySection(new AmqpValue("three")));
            Assert.Throws<ClientIllegalStateException>(() => _ = message.Body);
            Assert.Throws<ClientIllegalStateException>(() => message.RawOutputStream());
            Assert.Throws<ClientIllegalStateException>(() => message.Abort());

            sender.Close();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}