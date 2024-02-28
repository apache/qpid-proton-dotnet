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
using System;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Client.Utilities;
using System.Collections;

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
            peer.ExpectTransfer().WithMessageFormat(0).WithPayload(payloadMatcher).Accept();
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

      [Test]
      public void TestSendMessageWithMultipleSectionsPopulated()
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
            DeliveryAnnotationsMatcher daMatcher = new DeliveryAnnotationsMatcher(true);
            daMatcher.WithEntry("da1", Test.Driver.Matchers.Is.EqualTo(1));
            daMatcher.WithEntry("da2", Test.Driver.Matchers.Is.EqualTo(2));
            daMatcher.WithEntry("da3", Test.Driver.Matchers.Is.EqualTo(3));
            MessageAnnotationsMatcher maMatcher = new MessageAnnotationsMatcher(true);
            maMatcher.WithEntry("ma1", Test.Driver.Matchers.Is.EqualTo(1));
            maMatcher.WithEntry("ma2", Test.Driver.Matchers.Is.EqualTo(2));
            maMatcher.WithEntry("ma3", Test.Driver.Matchers.Is.EqualTo(3));
            ApplicationPropertiesMatcher apMatcher = new ApplicationPropertiesMatcher(true);
            apMatcher.WithEntry("ap1", Test.Driver.Matchers.Is.EqualTo(1));
            apMatcher.WithEntry("ap2", Test.Driver.Matchers.Is.EqualTo(2));
            apMatcher.WithEntry("ap3", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World", true);
            FooterMatcher footerMatcher = new FooterMatcher(false);
            footerMatcher.WithEntry("f1", Test.Driver.Matchers.Is.EqualTo(1));
            footerMatcher.WithEntry("f2", Test.Driver.Matchers.Is.EqualTo(2));
            footerMatcher.WithEntry("f3", Test.Driver.Matchers.Is.EqualTo(3));
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.DeliveryAnnotationsMatcher = daMatcher;
            payloadMatcher.MessageAnnotationsMatcher = maMatcher;
            payloadMatcher.PropertiesMatcher = propertiesMatcher;
            payloadMatcher.ApplicationPropertiesMatcher = apMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;
            payloadMatcher.FooterMatcher = footerMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            // Populate delivery annotations
            IDictionary<string, object> deliveryAnnotations = new Dictionary<string, object>();
            deliveryAnnotations.Add("da1", 1);
            deliveryAnnotations.Add("da2", 2);
            deliveryAnnotations.Add("da3", 3);

            IMessage<string> message = IMessage<string>.Create("Hello World");

            // Populate all Header values
            message.Durable = true;
            message.Priority = (byte)1;
            message.TimeToLive = 65535;
            message.FirstAcquirer = true;
            message.DeliveryCount = 2;
            // Populate message annotations
            message.SetAnnotation("ma1", 1);
            message.SetAnnotation("ma2", 2);
            message.SetAnnotation("ma3", 3);
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
            // Populate message application properties
            message.SetProperty("ap1", 1);
            message.SetProperty("ap2", 2);
            message.SetProperty("ap3", 3);
            // Populate message footers
            message.SetFooter("f1", 1);
            message.SetFooter("f2", 2);
            message.SetFooter("f3", 3);

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
      public void TestSendMessageWithUUIDPayloadArrivesWithAMQPValueBodySetFromEmpty()
      {
         DoTestSendMessageWithUUIDPayloadArrivesWithAMQPValueBody(true);
      }

      [Test]
      public void TestSendMessageWithUUIDPayloadArrivesWithAMQPValueBodyPopulateOnCreate()
      {
         DoTestSendMessageWithUUIDPayloadArrivesWithAMQPValueBody(false);
      }

      private void DoTestSendMessageWithUUIDPayloadArrivesWithAMQPValueBody(bool useSetter)
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

            Guid payload = Guid.NewGuid();

            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<Guid> message;
            if (useSetter)
            {
               message = IMessage<Guid>.Create();
               message.Body = payload;
            }
            else
            {
               message = IMessage<Guid>.Create(payload);
            }

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
      public void TestSendMessageWithByteArrayPayloadArrivesWithDataSectionSetFromEmpty()
      {
         DoTestSendMessageWithByteArrayPayloadArrivesWithDataSection(true);
      }

      [Test]
      public void TestSendMessageWithByteArrayPayloadArrivesWithDataSectionPopulateOnCreate()
      {
         DoTestSendMessageWithByteArrayPayloadArrivesWithDataSection(false);
      }

      private void DoTestSendMessageWithByteArrayPayloadArrivesWithDataSection(bool useSetter)
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

            byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

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

            DataMatcher bodyMatcher = new DataMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<byte[]> message;
            if (useSetter)
            {
               message = IMessage<byte[]>.Create();
               message.Body = payload;
            }
            else
            {
               message = IMessage<byte[]>.Create(payload);
            }

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
      public void TestSendMessageWithListPayloadArrivesWithAMQPSequenceBodySetFromEmpty()
      {
         DoTestSendMessageWithListPayloadArrivesWithAMQPSequenceBody(true);
      }

      [Test]
      public void TestSendMessageWithListPayloadArrivesWithAMQPSequenceBodyPopulateOnCreate()
      {
         DoTestSendMessageWithListPayloadArrivesWithAMQPSequenceBody(false);
      }

      private void DoTestSendMessageWithListPayloadArrivesWithAMQPSequenceBody(bool useSetter)
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

            List<Guid> payload = new List<Guid>();
            payload.Add(Guid.NewGuid());
            payload.Add(Guid.NewGuid());
            payload.Add(Guid.NewGuid());

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

            AmqpSequenceMatcher bodyMatcher = new AmqpSequenceMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<IList> message;
            if (useSetter)
            {
               message = IMessage<IList>.Create();
               message.Body = payload;
            }
            else
            {
               message = IMessage<IList>.Create((IList) payload);
            }

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
      public void TestSendMessageWithMapPayloadArrivesWithAMQPValueBodySetFromEmpty()
      {
         DoTestSendMessageWithMapPayloadArrivesWithAMQPValueBody(true);
      }

      [Test]
      public void TestSendMessageWithMapPayloadArrivesWithAMQPValueBodyPopulateOnCreate()
      {
         DoTestSendMessageWithMapPayloadArrivesWithAMQPValueBody(false);
      }

      private void DoTestSendMessageWithMapPayloadArrivesWithAMQPValueBody(bool useSetter)
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

            Dictionary<string, Guid> payload = new Dictionary<string, Guid>();
            payload.Add("1", Guid.NewGuid());
            payload.Add("2", Guid.NewGuid());
            payload.Add("3", Guid.NewGuid());

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

            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<IDictionary<string, Guid>> message;
            if (useSetter)
            {
               message = IMessage<IDictionary<string, Guid>>.Create();
               message.Body = payload;
            }
            else
            {
               message = IMessage<IDictionary<string, Guid>>.Create(payload);
            }

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
      public void TestConvertMessageToAdvancedAndSendAMQPHeader()
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

            HeaderMatcher headerMatcher = new HeaderMatcher(false);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create();
            IAdvancedMessage<string> advanced = message.ToAdvancedMessage();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            advanced.Header = header;

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
      public void TestSendOfExternalMessageWithoutAdvancedConversionSupport()
      {
         DoTestSendOfExternalMessage(false, false);
      }

      [Test]
      public void TestSendOfExternalMessageWithAdvancedConversionSupport()
      {
         DoTestSendOfExternalMessage(true, false);
      }

      [Test]
      public void TestTrySendOfExternalMessageWithoutAdvancedConversionSupport()
      {
         DoTestSendOfExternalMessage(false, true);
      }

      [Test]
      public void TestTrySendOfExternalMessageWithAdvancedConversionSupport()
      {
         DoTestSendOfExternalMessage(true, true);
      }

      private void DoTestSendOfExternalMessage(bool allowAdvancedConversion, bool trySend)
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
            DeliveryAnnotationsMatcher daMatcher = new DeliveryAnnotationsMatcher(true);
            daMatcher.WithEntry("da1", Test.Driver.Matchers.Is.EqualTo(1));
            daMatcher.WithEntry("da2", Test.Driver.Matchers.Is.EqualTo(2));
            daMatcher.WithEntry("da3", Test.Driver.Matchers.Is.EqualTo(3));
            MessageAnnotationsMatcher maMatcher = new MessageAnnotationsMatcher(true);
            maMatcher.WithEntry("ma1", Test.Driver.Matchers.Is.EqualTo(1));
            maMatcher.WithEntry("ma2", Test.Driver.Matchers.Is.EqualTo(2));
            maMatcher.WithEntry("ma3", Test.Driver.Matchers.Is.EqualTo(3));
            ApplicationPropertiesMatcher apMatcher = new ApplicationPropertiesMatcher(true);
            apMatcher.WithEntry("ap1", Test.Driver.Matchers.Is.EqualTo(1));
            apMatcher.WithEntry("ap2", Test.Driver.Matchers.Is.EqualTo(2));
            apMatcher.WithEntry("ap3", Test.Driver.Matchers.Is.EqualTo(3));
            FooterMatcher footerMatcher = new FooterMatcher(false);
            footerMatcher.WithEntry("f1", Test.Driver.Matchers.Is.EqualTo(1));
            footerMatcher.WithEntry("f2", Test.Driver.Matchers.Is.EqualTo(2));
            footerMatcher.WithEntry("f3", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World", true);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.DeliveryAnnotationsMatcher = daMatcher;
            payloadMatcher.MessageAnnotationsMatcher = maMatcher;
            payloadMatcher.PropertiesMatcher = propertiesMatcher;
            payloadMatcher.ApplicationPropertiesMatcher = apMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;
            payloadMatcher.FooterMatcher = footerMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            // Populate delivery annotations
            IDictionary<string, object> deliveryAnnotations = new Dictionary<string, object>();
            deliveryAnnotations.Add("da1", 1);
            deliveryAnnotations.Add("da2", 2);
            deliveryAnnotations.Add("da3", 3);

            IMessage<string> message = new ExternalMessage<string>(allowAdvancedConversion);

            message.Body = "Hello World";
            // Populate all Header values
            message.Durable = true;
            message.Priority = (byte)1;
            message.TimeToLive = 65535;
            message.FirstAcquirer = true;
            message.DeliveryCount = 2;
            // Populate message annotations
            message.SetAnnotation("ma1", 1);
            message.SetAnnotation("ma2", 2);
            message.SetAnnotation("ma3", 3);
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
            // Populate message application properties
            message.SetProperty("ap1", 1);
            message.SetProperty("ap2", 2);
            message.SetProperty("ap3", 3);
            // Populate message footers
            message.SetFooter("f1", 1);
            message.SetFooter("f2", 2);
            message.SetFooter("f3", 3);

            // Check preconditions that should affect the send operation
            if (allowAdvancedConversion)
            {
               Assert.IsNotNull(message.ToAdvancedMessage());
            }
            else
            {
               Assert.Throws<NotSupportedException>(() => message.ToAdvancedMessage());
            }

            ITracker tracker;
            if (trySend)
            {
               tracker = sender.TrySend(message, deliveryAnnotations);
            }
            else
            {
               tracker = sender.Send(message, deliveryAnnotations);
            }

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessageWithMultipleAmqpValueSections()
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

            // Note: This is a specification violation but could be used by other message formats
            //       and we don't attempt to enforce at the AdvancedMessage API level what users do.
            AmqpValueMatcher bodyMatcher1 = new AmqpValueMatcher("one", true);
            AmqpValueMatcher bodyMatcher2 = new AmqpValueMatcher("two", true);
            AmqpValueMatcher bodyMatcher3 = new AmqpValueMatcher("three", false);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.AddMessageContentMatcher(bodyMatcher1);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher2);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher3);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithMessageFormat(17).WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IAdvancedMessage<string> message = IAdvancedMessage<string>.Create();

            message.MessageFormat = 17;
            message.AddBodySection(new AmqpValue("one"));
            message.AddBodySection(new AmqpValue("two"));
            message.AddBodySection(new AmqpValue("three"));

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
      public void TestSendMessageWithMultipleAmqpSequenceSections()
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

            List<string> list1 = new List<string>();
            list1.Add("1");
            List<string> list2 = new List<string>();
            list2.Add("21");
            list2.Add("22");
            List<string> list3 = new List<string>();
            list3.Add("31");
            list3.Add("32");
            list3.Add("33");

            AmqpSequenceMatcher bodyMatcher1 = new AmqpSequenceMatcher(list1, true);
            AmqpSequenceMatcher bodyMatcher2 = new AmqpSequenceMatcher(list2, true);
            AmqpSequenceMatcher bodyMatcher3 = new AmqpSequenceMatcher(list3, false);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.AddMessageContentMatcher(bodyMatcher1);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher2);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher3);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IAdvancedMessage<List<string>> message = IAdvancedMessage<List<string>>.Create();

            message.AddBodySection(new AmqpSequence(list1));
            message.AddBodySection(new AmqpSequence(list2));
            message.AddBodySection(new AmqpSequence(list3));

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
      public void TestSendMessageWithMultipleDataSections()
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

            byte[] buffer1 = new byte[] { 1 };
            byte[] buffer2 = new byte[] { 1, 2 };
            byte[] buffer3 = new byte[] { 1, 2, 3 };

            DataMatcher bodyMatcher1 = new DataMatcher(buffer1, true);
            DataMatcher bodyMatcher2 = new DataMatcher(buffer2, true);
            DataMatcher bodyMatcher3 = new DataMatcher(buffer3, false);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.AddMessageContentMatcher(bodyMatcher1);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher2);
            payloadMatcher.AddMessageContentMatcher(bodyMatcher3);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IAdvancedMessage<byte[]> message = IAdvancedMessage<byte[]>.Create();

            message.AddBodySection(new Data(buffer1));
            message.AddBodySection(new Data(buffer2));
            message.AddBodySection(new Data(buffer3));

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