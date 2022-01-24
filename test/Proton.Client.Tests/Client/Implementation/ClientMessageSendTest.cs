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
using NUnit.Framework;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver;
using Microsoft.Extensions.Logging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;
using System.Text;
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientMessageSendTest : ClientBaseTestFixture
   {
      [Test]
      public void TestSendMessageWithHeaderValuesPopulated()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
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
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender sender = session.OpenSender("test-qos", options);

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            // Populate all Header values
            message.Durable = true;
            message.Priority = (byte)1;
            message.TimeToLive = 65535;
            message.FirstAcquirer = true;
            message.DeliveryCount = 2;

            ITracker tracker = sender.Send(message);

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Issue in test peer decoding ulong types")]
      [Test]
      public void TestSendMessageWithPropertiesValuesPopulated()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
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
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender sender = session.OpenSender("test-qos", options);

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            PropertiesMatcher propertiesMatcher = new PropertiesMatcher(true);
            propertiesMatcher.WithMessageId("ID:12345");
            propertiesMatcher.WithUserId(Encoding.UTF8.GetBytes("user"));
            propertiesMatcher.WithTo("the-management");
            propertiesMatcher.WithSubject("amqp");
            propertiesMatcher.WithReplyTo("the-minions");
            propertiesMatcher.WithCorrelationId("abc");
            propertiesMatcher.WithContentEncoding("application/json");
            propertiesMatcher.WithContentType("gzip");
            propertiesMatcher.WithAbsoluteExpiryTime(123);
            propertiesMatcher.WithCreationTime(1);
            propertiesMatcher.WithGroupId("disgruntled");
            propertiesMatcher.WithGroupSequence(8192);
            propertiesMatcher.WithReplyToGroupId("/dev/null");
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.PropertiesMatcher = propertiesMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            // Populate all Properties values
            message.MessageId = "ID:12345";
            message.UserId = Encoding.UTF8.GetBytes("user");
            message.To = "the-management";
            message.Subject = "amqp";
            message.ReplyTo = "the-minions";
            message.CorrelationId = "abc";
            message.ContentEncoding = "application/json";
            message.ContentType = "gzip";
            message.AbsoluteExpiryTime = 123;
            message.CreationTime = 1;
            message.GroupId = "disgruntled";
            message.GroupSequence = 8192;
            message.ReplyToGroupId = "/dev/null";

            ITracker tracker = sender.Send(message);

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessageWithDeliveryAnnotationsPopulated()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
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
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender sender = session.OpenSender("test-qos", options);

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            DeliveryAnnotationsMatcher daMatcher = new DeliveryAnnotationsMatcher(true);
            daMatcher.WithEntry("one", Test.Driver.Matchers.Is.EqualTo(1));
            daMatcher.WithEntry("two", Test.Driver.Matchers.Is.EqualTo(2));
            daMatcher.WithEntry("three", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.DeliveryAnnotationsMatcher = daMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            // Populate delivery annotations
            IDictionary<string, object> deliveryAnnotations = new Dictionary<string, object>();
            deliveryAnnotations.Add("one", 1);
            deliveryAnnotations.Add("two", 2);
            deliveryAnnotations.Add("three", 3);

            ITracker tracker = sender.Send(message, deliveryAnnotations);

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessageWithMessageAnnotationsPopulated()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
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
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender sender = session.OpenSender("test-qos", options);

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            MessageAnnotationsMatcher maMatcher = new MessageAnnotationsMatcher(true);
            maMatcher.WithEntry("one", Test.Driver.Matchers.Is.EqualTo(1));
            maMatcher.WithEntry("two", Test.Driver.Matchers.Is.EqualTo(2));
            maMatcher.WithEntry("three", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageAnnotationsMatcher = maMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            // Populate message annotations
            message.SetAnnotation("one", 1);
            message.SetAnnotation("two", 2);
            message.SetAnnotation("three", 3);

            ITracker tracker = sender.Send(message);

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessageWithApplicationPropertiesPopulated()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
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
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender sender = session.OpenSender("test-qos", options);

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            ApplicationPropertiesMatcher apMatcher = new ApplicationPropertiesMatcher(true);
            apMatcher.WithEntry("one", Test.Driver.Matchers.Is.EqualTo(1));
            apMatcher.WithEntry("two", Test.Driver.Matchers.Is.EqualTo(2));
            apMatcher.WithEntry("three", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.ApplicationPropertiesMatcher = apMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            // Populate message application properties
            message.SetProperty("one", 1);
            message.SetProperty("two", 2);
            message.SetProperty("three", 3);

            ITracker tracker = sender.Send(message);

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessageWithFootersPopulated()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
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
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender sender = session.OpenSender("test-qos", options);

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            FooterMatcher footerMatcher = new FooterMatcher(false);
            footerMatcher.WithEntry("f1", Test.Driver.Matchers.Is.EqualTo(1));
            footerMatcher.WithEntry("f2", Test.Driver.Matchers.Is.EqualTo(2));
            footerMatcher.WithEntry("f3", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World", true);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = bodyMatcher;
            payloadMatcher.FooterMatcher = footerMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            // Populate message footers
            message.SetFooter("f1", 1);
            message.SetFooter("f2", 2);
            message.SetFooter("f3", 3);

            ITracker tracker = sender.Send(message);

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

   }
}