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
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Types;

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

            IStreamTracker tracker;
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

      [Test]
      public void TestClearBodySectionsIsNoOpForStreamSenderMessage()
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

            message.MessageFormat = 17;

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            AmqpValueMatcher bodyMatcher1 = new AmqpValueMatcher("one", true);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.AddMessageContentMatcher(bodyMatcher1);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithMore(false).WithMessageFormat(17).WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            message.AddBodySection(new AmqpValue("one"));
            message.ClearBodySections();
            message.ForEachBodySection((section) =>
            {
               // No sections retained so this should never run.
               throw new InvalidOperationException();
            });

            Assert.IsNotNull(message.GetBodySections());
            Assert.IsTrue(message.GetBodySections().Count() == 0);

            message.Complete();

            Assert.IsTrue(message.Tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.Settled);
            Assert.Throws<ClientIllegalStateException>(() => _ = message.Body);
            Assert.Throws<ClientIllegalStateException>(() => message.RawOutputStream());

            sender.Close();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestMessageFormatCannotBeModifiedAfterBodyWritesStart()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond(); // Hidden session for stream sender
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;

            IStreamSender sender = connection.OpenStreamSender("test-qos");
            IStreamSenderMessage message = sender.BeginMessage();

            sender.OpenTask.Wait();

            message.Durable = true;
            message.MessageFormat = 17;

            _ = message.Body;  // Alters message state to exclude future message format changes

            try
            {
               message.MessageFormat = 16;
               Assert.Fail("Should not be able to modify message format after body writes started");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }
            catch (Exception unexpected)
            {
               Assert.Fail("Failed test due to message format set throwing unexpected error: " + unexpected);
            }

            message.Abort();

            Assert.Throws<ClientIllegalStateException>(() => message.Complete());

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotCreateNewStreamingMessageWhileCurrentInstanceIsIncomplete()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond(); // Hidden session for stream sender
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;

            IStreamSender sender = (IStreamSender)connection.OpenStreamSender("test-qos").OpenTask.Result;
            IStreamSenderMessage message = sender.BeginMessage();

            try
            {
               sender.BeginMessage();
               Assert.Fail("Should not be able create a new streaming sender message before last one is completed.");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            message.Abort();

            Assert.Throws<ClientIllegalStateException>(() => message.Complete());

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotAssignAnOutputStreamToTheMessageBody()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond(); // Hidden session for stream sender
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;

            IStreamSender sender = (IStreamSender)connection.OpenStreamSender("test-qos").OpenTask.Result;
            IStreamSenderMessage message = sender.BeginMessage();

            try
            {
               message.Body = new MemoryStream();
               Assert.Fail("Should not be able assign an output stream to the message body");
            }
            catch (ClientUnsupportedOperationException)
            {
               // Expected
            }

            message.Abort();

            Assert.Throws<ClientIllegalStateException>(() => message.Complete());

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotModifyMessagePreambleAfterWritesHaveStarted()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond(); // Hidden session for stream sender
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;

            IStreamSender sender = (IStreamSender)connection.OpenStreamSender("test-qos").OpenTask.Result;
            IStreamSenderMessage message = sender.BeginMessage();

            message.Durable = true;
            message.MessageId = "test";
            message.SetAnnotation("key", "value");
            message.SetProperty("key", "value");
            _ = message.Body;

            try
            {
               message.Durable = false;
               Assert.Fail("Should not be able to modify message preamble after body writes started");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            try
            {
               message.MessageId = "test1";
               Assert.Fail("Should not be able to modify message preamble after body writes started");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            try
            {
               message.SetAnnotation("key1", "value");
               Assert.Fail("Should not be able to modify message preamble after body writes started");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            try
            {
               message.SetProperty("key", "value");
               Assert.Fail("Should not be able to modify message preamble after body writes started");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            message.Abort();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateStream()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithMore(false).WithNullPayload();
            peer.ExpectDetach().WithClosed(true).Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-qos");
            IStreamSenderMessage tracker = sender.BeginMessage();

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = tracker.GetBodyStream(options);

            Assert.IsNotNull(stream);

            sender.OpenTask.Wait();

            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOutputStreamOptionsEnforcesValidBodySizeValues()
      {
         OutputStreamOptions options = new OutputStreamOptions();

         options.BodyLength = 1024;
         options.BodyLength = int.MaxValue;

         Assert.Throws<ArgumentOutOfRangeException>(() => options.BodyLength = -1);
      }

      [Test]
      public void TestFlushWithSetNonBodySectionsThenClose()
      {
         DoTestNonBodySectionWrittenWhenNoWritesToStream(true);
      }

      [Test]
      public void TestCloseWithSetNonBodySections()
      {
         DoTestNonBodySectionWrittenWhenNoWritesToStream(false);
      }

      private void DoTestNonBodySectionWrittenWhenNoWritesToStream(bool flushBeforeClose)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            message.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = message.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;

            peer.WaitForScriptToComplete();
            if (flushBeforeClose)
            {
               peer.ExpectTransfer().WithMore(true).WithPayload(payloadMatcher);
               peer.ExpectTransfer().WithMore(false).WithNullPayload()
                                    .Respond()
                                    .WithSettled(true).WithState().Accepted();
            }
            else
            {
               peer.ExpectTransfer().WithMore(false).WithPayload(payloadMatcher)
                                    .Respond()
                                    .WithSettled(true).WithState().Accepted();
            }
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // Once flush is called than anything in the buffer is written regardless of
            // there being any actual stream writes.  Default close action is to complete
            // the delivery.
            if (flushBeforeClose)
            {
               stream.Flush();
            }
            stream.Close();

            message.Tracker.AwaitSettlement(TimeSpan.FromSeconds(5));

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestFlushAfterFirstWriteEncodesAMQPHeaderAndMessageBuffer()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            message.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = message.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher dataMatcher = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.MessageContentMatcher = dataMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithMore(true).WithPayload(payloadMatcher);
            peer.ExpectTransfer().WithMore(false).WithNullPayload();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // Stream won't output until some body bytes are written since the buffer was not
            // filled by the header write.  Then the close will complete the stream message.
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAutoFlushAfterSingleWriteExceedsConfiguredBufferLimit()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue", new StreamSenderOptions()
            {
               WriteBufferSize = 512
            });
            IStreamSenderMessage tracker = sender.BeginMessage();

            byte[] payload = new byte[512];
            Array.Fill(payload, (byte)16);

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            tracker.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = tracker.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher dataMatcher = new DataMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.MessageContentMatcher = dataMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(true);

            // Stream won't output until some body bytes are written.
            stream.Write(payload);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNullPayload().WithMore(false).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAutoFlushDuringWriteThatExceedConfiguredBufferLimit()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue", new StreamSenderOptions()
            {
               WriteBufferSize = 256
            });
            IStreamSenderMessage tracker = sender.BeginMessage();

            byte[] payload1 = new byte[256];
            Array.Fill(payload1, (byte)1);
            byte[] payload2 = new byte[256];
            Array.Fill(payload2, (byte)2);
            byte[] payload3 = new byte[256];
            Array.Fill(payload3, (byte)3);
            byte[] payload4 = new byte[256];
            Array.Fill(payload4, (byte)4);

            byte[] payload = new byte[1024];
            Array.Copy(payload1, 0, payload, 0, 256);
            Array.Copy(payload2, 0, payload, 256, 256);
            Array.Copy(payload3, 0, payload, 512, 256);
            Array.Copy(payload4, 0, payload, 768, 256);

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            tracker.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = tracker.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher dataMatcher1 = new DataMatcher(payload1);
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.HeaderMatcher = headerMatcher;
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            DataMatcher dataMatcher2 = new DataMatcher(payload2);
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            DataMatcher dataMatcher3 = new DataMatcher(payload3);
            TransferPayloadCompositeMatcher payloadMatcher3 = new TransferPayloadCompositeMatcher();
            payloadMatcher3.MessageContentMatcher = dataMatcher3;

            DataMatcher dataMatcher4 = new DataMatcher(payload4);
            TransferPayloadCompositeMatcher payloadMatcher4 = new TransferPayloadCompositeMatcher();
            payloadMatcher4.MessageContentMatcher = dataMatcher4;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            peer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            peer.ExpectTransfer().WithPayload(payloadMatcher3).WithMore(true);
            peer.ExpectTransfer().WithPayload(payloadMatcher4).WithMore(true);

            // Stream won't output until some body bytes are written.
            stream.Write(payload);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNullPayload().WithMore(false).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAutoFlushDuringWriteThatExceedConfiguredBufferLimitSessionCreditLimitOnTransfer()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue", new StreamSenderOptions()
            {
               WriteBufferSize = 256
            });
            IStreamSenderMessage tracker = sender.BeginMessage();

            byte[] payload1 = new byte[256];
            Array.Fill(payload1, (byte)1);
            byte[] payload2 = new byte[256];
            Array.Fill(payload2, (byte)2);
            byte[] payload3 = new byte[256];
            Array.Fill(payload3, (byte)3);
            byte[] payload4 = new byte[256];
            Array.Fill(payload4, (byte)4);

            byte[] payload = new byte[1024];
            Array.Copy(payload1, 0, payload, 0, 256);
            Array.Copy(payload2, 0, payload, 256, 256);
            Array.Copy(payload3, 0, payload, 512, 256);
            Array.Copy(payload4, 0, payload, 768, 256);

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            tracker.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = tracker.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher dataMatcher1 = new DataMatcher(payload1);
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.HeaderMatcher = headerMatcher;
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            DataMatcher dataMatcher2 = new DataMatcher(payload2);
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            DataMatcher dataMatcher3 = new DataMatcher(payload3);
            TransferPayloadCompositeMatcher payloadMatcher3 = new TransferPayloadCompositeMatcher();
            payloadMatcher3.MessageContentMatcher = dataMatcher3;

            DataMatcher dataMatcher4 = new DataMatcher(payload4);
            TransferPayloadCompositeMatcher payloadMatcher4 = new TransferPayloadCompositeMatcher();
            payloadMatcher4.MessageContentMatcher = dataMatcher4;

            CountdownEvent sendComplete = new CountdownEvent(1);
            bool sendFailed = false;
            // Stream won't output until some body bytes are written.
            Task.Run(() =>
            {
               try
               {
                  stream.Write(payload);
               }
               catch (IOException e)
               {
                  logger.LogInformation("send failed with error: {0}", e.Message);
                  sendFailed = true;
               }
               finally
               {
                  sendComplete.Signal();
               }
            });

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(2).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(3).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithPayload(payloadMatcher3).WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(4).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithPayload(payloadMatcher4).WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(5).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNullPayload().WithMore(false).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // Initiate the above script of transfers and flows
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(1).WithLinkCredit(10).Now();

            Assert.IsTrue(sendComplete.Wait(TimeSpan.FromSeconds(10)));

            stream.Close();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseAfterSingleWriteEncodesAndCompletesTransferWhenNoStreamSizeConfigured()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage tracker = sender.BeginMessage();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            tracker.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = tracker.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher dataMatcher = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.MessageContentMatcher = dataMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(false).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // Stream won't output until some body bytes are written.
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestFlushAfterSecondWriteDoesNotEncodeAMQPHeaderFromConfiguration()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage tracker = sender.BeginMessage();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.TimeToLive = 65535;
            header.FirstAcquirer = true;
            header.DeliveryCount = 2;

            tracker.Header = header;

            OutputStreamOptions options = new OutputStreamOptions();
            Stream stream = tracker.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher dataMatcher1 = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.HeaderMatcher = headerMatcher;
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            // Second flush expectation
            DataMatcher dataMatcher2 = new DataMatcher(new byte[] { 4, 5, 6, 7 });
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            peer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            peer.ExpectTransfer().WithNullPayload().WithMore(false).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // Stream won't output until some body bytes are written.
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();

            // Next write should only be a single Data section
            stream.Write(new byte[] { 4, 5, 6, 7 });
            stream.Flush();

            // Final Transfer that completes the Delivery
            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestIncompleteStreamClosureCausesTransferAbort()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage tracker = sender.BeginMessage();

            byte[] payload = new byte[] { 0, 1, 2, 3 };

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.DeliveryCount = 1;

            tracker.Header = header;

            OutputStreamOptions options = new OutputStreamOptions()
            {
               BodyLength = 8192
            };
            Stream stream = tracker.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithDeliveryCount(1);
            PartialDataSectionMatcher partialDataMatcher = new PartialDataSectionMatcher(8192, payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.MessageContentMatcher = partialDataMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher);
            peer.ExpectTransfer().WithAborted(true).WithNullPayload();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            stream.Write(payload);
            stream.Flush();

            // Stream should abort the send now since the configured size wasn't sent.
            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestIncompleteStreamClosureWithNoWritesAbortsTransfer()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.DeliveryCount = 1;

            message.Header = header;

            OutputStreamOptions options = new OutputStreamOptions()
            {
               BodyLength = 8192,
               CompleteSendOnClose = false
            };
            Stream stream = message.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithDeliveryCount(1);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // This should abort the transfer as we might have triggered output upon create when the
            // preamble was written.
            stream.Close();

            Assert.IsTrue(message.Aborted);

            // Should have no affect.
            message.Abort();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCompleteStreamClosureCausesTransferCompleted()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(3).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage tracker = sender.BeginMessage();

            byte[] payload1 = new byte[] { 0, 1, 2, 3, 4, 5 };
            byte[] payload2 = new byte[] { 6, 7, 8, 9, 10, 11, 12, 13, 14 };
            byte[] payload3 = new byte[] { 15 };

            int payloadSize = payload1.Length + payload2.Length + payload3.Length;

            // Populate all Header values
            Header header = new Header();
            header.Durable = true;
            header.Priority = (byte)1;
            header.DeliveryCount = 1;

            tracker.Header = header;

            // Populate message application properties
            tracker.SetProperty("ap1", 1);
            tracker.SetProperty("ap2", 2);
            tracker.SetProperty("ap3", 3);

            OutputStreamOptions options = new OutputStreamOptions()
            {
               BodyLength = payloadSize
            };
            Stream stream = tracker.GetBodyStream(options);

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithDeliveryCount(1);
            ApplicationPropertiesMatcher apMatcher = new ApplicationPropertiesMatcher(true);
            apMatcher.WithEntry("ap1", Test.Driver.Matchers.Is.EqualTo(1));
            apMatcher.WithEntry("ap2", Test.Driver.Matchers.Is.EqualTo(2));
            apMatcher.WithEntry("ap3", Test.Driver.Matchers.Is.EqualTo(3));
            PartialDataSectionMatcher partialDataMatcher = new PartialDataSectionMatcher(payloadSize, payload1);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.MessageContentMatcher = partialDataMatcher;
            payloadMatcher.ApplicationPropertiesMatcher = apMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher);

            stream.Write(payload1);
            stream.Flush();

            peer.WaitForScriptToComplete();
            partialDataMatcher = new PartialDataSectionMatcher(payload2);
            payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = partialDataMatcher;
            peer.ExpectTransfer().WithMore(true).WithPayload(partialDataMatcher);

            stream.Write(payload2);
            stream.Flush();

            peer.WaitForScriptToComplete();
            partialDataMatcher = new PartialDataSectionMatcher(payload3);
            payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = partialDataMatcher;
            peer.ExpectTransfer().WithMore(false).WithPayload(partialDataMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            stream.Write(payload3);
            stream.Flush();

            // Stream should already be completed so no additional frames should be written.
            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestRawOutputStreamFromMessageWritesUnmodifiedBytes()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            Stream stream = message.RawOutputStream();

            // Only one writer at a time can exist
            Assert.Throws<ClientIllegalStateException>(() => message.RawOutputStream());
            Assert.Throws<ClientIllegalStateException>(() => message.GetBodyStream());

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithMore(true).WithPayload(new byte[] { 0, 1, 2, 3 });
            peer.ExpectTransfer().WithMore(false).WithNullPayload();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamSenderMessageWithDeliveryAnnotations()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;

            // Populate delivery annotations
            IDictionary<string, object> deliveryAnnotations = new Dictionary<string, object>();
            deliveryAnnotations.Add("da1", 1);
            deliveryAnnotations.Add("da2", 2);
            deliveryAnnotations.Add("da3", 3);

            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage(deliveryAnnotations);

            byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

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
            DataMatcher bodyMatcher = new DataMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.DeliveryAnnotationsMatcher = daMatcher;
            payloadMatcher.MessageAnnotationsMatcher = maMatcher;
            payloadMatcher.PropertiesMatcher = propertiesMatcher;
            payloadMatcher.ApplicationPropertiesMatcher = apMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(false).Accept();

            // Populate all Header values
            message.Durable = true;
            Assert.AreEqual(true, message.Durable);
            message.Priority = (byte)1;
            Assert.AreEqual(1, message.Priority);
            message.TimeToLive = 65535;
            Assert.AreEqual(65535, message.TimeToLive);
            message.FirstAcquirer = true;
            Assert.IsTrue(message.FirstAcquirer);
            message.DeliveryCount = 2;
            Assert.AreEqual(2, message.DeliveryCount);
            // Populate message annotations
            Assert.IsFalse(message.HasAnnotations);
            Assert.IsFalse(message.HasAnnotation("ma1"));
            message.SetAnnotation("ma1", 1);
            Assert.IsTrue(message.HasAnnotation("ma1"));
            Assert.AreEqual(1, message.GetAnnotation("ma1"));
            message.SetAnnotation("ma2", 2);
            Assert.AreEqual(2, message.GetAnnotation("ma2"));
            message.SetAnnotation("ma3", 3);
            Assert.AreEqual(3, message.GetAnnotation("ma3"));
            Assert.IsTrue(message.HasAnnotations);
            // Populate all Properties values
            message.MessageId = "ID:12345";
            Assert.AreEqual("ID:12345", message.MessageId);
            message.UserId = Encoding.UTF8.GetBytes("user");
            Assert.AreEqual(Encoding.UTF8.GetBytes("user"), message.UserId);
            message.To = "the-management";
            Assert.AreEqual("the-management", message.To);
            message.Subject = "amqp";
            Assert.AreEqual("amqp", message.Subject);
            message.ReplyTo = "the-minions";
            Assert.AreEqual("the-minions", message.ReplyTo);
            message.CorrelationId = "abc";
            Assert.AreEqual("abc", message.CorrelationId);
            message.ContentEncoding = "application/json";
            Assert.AreEqual("application/json", message.ContentEncoding);
            message.ContentType = "gzip";
            Assert.AreEqual("gzip", message.ContentType);
            message.AbsoluteExpiryTime = 123;
            Assert.AreEqual(123, message.AbsoluteExpiryTime);
            message.CreationTime = 1;
            Assert.AreEqual(1, message.CreationTime);
            message.GroupId = "disgruntled";
            Assert.AreEqual("disgruntled", message.GroupId);
            message.GroupSequence = 8192;
            Assert.AreEqual(8192, message.GroupSequence);
            message.ReplyToGroupId = "/dev/null";
            Assert.AreEqual("/dev/null", message.ReplyToGroupId);
            // Populate message application properties
            Assert.IsFalse(message.HasProperties);
            Assert.IsFalse(message.HasProperty("ma1"));
            message.SetProperty("ap1", 1);
            Assert.AreEqual(1, message.GetProperty("ap1"));
            Assert.IsTrue(message.HasProperty("ap1"));
            message.SetProperty("ap2", 2);
            Assert.IsTrue(message.HasProperty("ap2"));
            Assert.AreEqual(2, message.GetProperty("ap2"));
            message.SetProperty("ap3", 3);
            Assert.IsTrue(message.HasProperty("ap3"));
            Assert.AreEqual(3, message.GetProperty("ap3"));
            Assert.IsTrue(message.HasProperties);

            Stream stream = message.Body;

            stream.Write(payload);
            stream.Close();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsNotNull(message.Tracker);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.RemoteSettled);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamSenderWritesFooterAfterStreamClosed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

            // First frame should include only the bits up to the body
            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            ApplicationPropertiesMatcher apMatcher = new ApplicationPropertiesMatcher(true);
            apMatcher.WithEntry("ap1", Test.Driver.Matchers.Is.EqualTo(1));
            apMatcher.WithEntry("ap2", Test.Driver.Matchers.Is.EqualTo(2));
            apMatcher.WithEntry("ap3", Test.Driver.Matchers.Is.EqualTo(3));
            FooterMatcher footerMatcher = new FooterMatcher(false);
            footerMatcher.WithEntry("f1", Test.Driver.Matchers.Is.EqualTo(1));
            footerMatcher.WithEntry("f2", Test.Driver.Matchers.Is.EqualTo(2));
            footerMatcher.WithEntry("f3", Test.Driver.Matchers.Is.EqualTo(3));
            DataMatcher bodyMatcher = new DataMatcher(payload, true);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.ApplicationPropertiesMatcher = apMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;
            payloadMatcher.FooterMatcher = footerMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(false).Accept();

            // Populate all Header values
            message.Durable = true;
            message.Priority = (byte)1;
            message.TimeToLive = 65535;
            message.FirstAcquirer = true;
            message.DeliveryCount = 2;
            // Populate message application properties
            message.SetProperty("ap1", 1);
            message.SetProperty("ap2", 2);
            message.SetProperty("ap3", 3);
            // Populate message footers
            Assert.IsFalse(message.HasFooters);
            Assert.IsFalse(message.HasFooter("f1"));
            message.SetFooter("f1", 1);
            message.SetFooter("f2", 2);
            message.SetFooter("f3", 3);
            Assert.IsTrue(message.HasFooter("f1"));
            Assert.IsTrue(message.HasFooters);

            OutputStreamOptions bodyOptions = new OutputStreamOptions()
            {
               CompleteSendOnClose = true
            };
            Stream stream = message.GetBodyStream(bodyOptions);

            Assert.Throws<ClientUnsupportedOperationException>(() => message.Encode(new Dictionary<string, object>()));

            stream.Write(payload);
            stream.Close();

            Assert.Throws<ClientIllegalStateException>(() => message.Footer = new Footer());

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsNotNull(message.Tracker);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.RemoteSettled);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamSenderWritesFooterAfterMessageCompleted()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

            // First frame should include only the bits up to the body
            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            ApplicationPropertiesMatcher apMatcher = new ApplicationPropertiesMatcher(true);
            apMatcher.WithEntry("ap1", Test.Driver.Matchers.Is.EqualTo(1));
            apMatcher.WithEntry("ap2", Test.Driver.Matchers.Is.EqualTo(2));
            apMatcher.WithEntry("ap3", Test.Driver.Matchers.Is.EqualTo(3));
            DataMatcher bodyMatcher = new DataMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.ApplicationPropertiesMatcher = apMatcher;
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            // Second Frame should contains the appended footers
            FooterMatcher footerMatcher = new FooterMatcher(false);
            footerMatcher.WithEntry("f1", Test.Driver.Matchers.Is.EqualTo(1));
            footerMatcher.WithEntry("f2", Test.Driver.Matchers.Is.EqualTo(2));
            footerMatcher.WithEntry("f3", Test.Driver.Matchers.Is.EqualTo(3));
            TransferPayloadCompositeMatcher payloadFooterMatcher = new TransferPayloadCompositeMatcher();
            payloadFooterMatcher.FooterMatcher = footerMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(true);
            peer.ExpectTransfer().WithPayload(payloadFooterMatcher).WithMore(false).Accept();

            // Populate all Header values
            message.Durable = true;
            message.Priority = (byte)1;
            message.TimeToLive = 65535;
            message.FirstAcquirer = true;
            message.DeliveryCount = 2;
            // Populate message application properties
            message.SetProperty("ap1", 1);
            message.SetProperty("ap2", 2);
            message.SetProperty("ap3", 3);

            OutputStreamOptions bodyOptions = new OutputStreamOptions()
            {
               CompleteSendOnClose = false
            };
            Stream stream = message.GetBodyStream(bodyOptions);

            stream.Write(payload);
            stream.Close();

            // Populate message footers
            message.SetFooter("f1", 1);
            message.SetFooter("f2", 2);
            message.SetFooter("f3", 3);

            message.Complete();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsNotNull(message.Tracker);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.RemoteSettled);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAutoFlushDuringMessageSendThatExceedConfiguredBufferLimitSessionCreditLimitOnTransfer()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               MaxFrameSize = 1024
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            IStreamSender sender = connection.OpenStreamSender("test-queue");

            byte[] payload = new byte[4800];
            Array.Fill(payload, (byte)1);

            bool sendFailed = false;
            Task.Run(() =>
            {
               try
               {
                  sender.Send(IMessage<byte[]>.Create(payload));
               }
               catch (Exception e)
               {
                  logger.LogInformation("send failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            peer.WaitForScriptToComplete();
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(1).WithLinkCredit(10).Now();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(2).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(3).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(4).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(5).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Accept();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConcurrentMessageSendOnlyBlocksForInitialSendInProgress()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = (IStreamSender)connection.OpenStreamSender("test-queue").OpenTask.Result;

            // Ensure that sender gets its flow before the sends are triggered.
            connection.OpenReceiver("test-queue").OpenTask.Wait();

            byte[] payload = new byte[1024];
            Array.Fill(payload, (byte)1);

            // One should block on the send waiting for the others send to finish
            // otherwise they should not care about concurrency of sends.

            bool sendFailed = false;
            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 1 is preparing to fire:");
                  IStreamTracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
                  tracker.AwaitSettlement(TimeSpan.FromSeconds(10));
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 1 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 2 is preparing to fire:");
                  IStreamTracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
                  tracker.AwaitSettlement(TimeSpan.FromSeconds(10));
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 2 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConcurrentMessageSendsBlocksBehindSendWaitingForCredit()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");

            byte[] payload = new byte[1024];
            Array.Fill(payload, (byte)1);

            CountdownEvent send1Started = new CountdownEvent(1);
            CountdownEvent send2Completed = new CountdownEvent(1);

            bool sendFailed = false;
            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 1 is preparing to fire:");
                  Task.Run(() => send1Started.Signal());
                  sender.Send(IMessage<byte[]>.Create(payload));
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 1 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            Task.Run(() =>
            {
               try
               {
                  Assert.IsTrue(send1Started.Wait(TimeSpan.FromSeconds(10)));
                  logger.LogInformation("Test send 2 is preparing to fire:");
                  IStreamTracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
                  tracker.AwaitSettlement(TimeSpan.FromSeconds(10));
                  send2Completed.Signal();
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 2 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            peer.WaitForScriptToComplete();
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(0).WithNextIncomingId(1).WithLinkCredit(1).Now();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(1).WithNextIncomingId(2).WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();

            Assert.IsTrue(send2Completed.Wait(TimeSpan.FromSeconds(10)));

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConcurrentMessageSendWaitingOnSplitFramedSendToCompleteIsSentAfterCreditUpdated()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               MaxFrameSize = 1024
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            IStreamSender sender = connection.OpenStreamSender("test-queue");

            byte[] payload = new byte[1536];
            Array.Fill(payload, (byte)1);

            CountdownEvent send1Started = new CountdownEvent(1);
            CountdownEvent send2Completed = new CountdownEvent(1);

            bool sendFailed = false;
            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 1 is preparing to fire:");
                  Task.Run(() => send1Started.Signal());
                  sender.Send(IMessage<byte[]>.Create(payload));
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 1 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            Task.Run(() =>
            {
               try
               {
                  Assert.IsTrue(send1Started.Wait(TimeSpan.FromSeconds(10)));
                  logger.LogInformation("Test send 2 is preparing to fire:");
                  IStreamTracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
                  tracker.AwaitSettlement(TimeSpan.FromSeconds(10));
                  send2Completed.Signal();
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 2 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            peer.WaitForScriptToComplete(TimeSpan.FromSeconds(15));
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(0).WithNextIncomingId(1).WithLinkCredit(1).Now();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(0).WithNextIncomingId(2).WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(1).WithNextIncomingId(3).WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(1).WithNextIncomingId(4).WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();

            Assert.IsTrue(send2Completed.Wait(TimeSpan.FromSeconds(10)));

            peer.WaitForScriptToComplete(TimeSpan.FromSeconds(15));
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete(TimeSpan.FromSeconds(15));
         }
      }

      [Test]
      public void TestMessageSendWhileStreamSendIsOpenShouldBlock()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            byte[] payload = new byte[1536];
            Array.Fill(payload, (byte)1);

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();
            OutputStreamOptions options = new OutputStreamOptions()
            {
               BodyLength = 8192,
               CompleteSendOnClose = false
            };
            Stream stream = message.GetBodyStream(options);

            CountdownEvent sendStarted = new CountdownEvent(1);
            CountdownEvent sendCompleted = new CountdownEvent(1);
            bool sendFailed = false;

            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 1 is preparing to fire:");
                  sendStarted.Signal();
                  sender.Send(IMessage<byte[]>.Create(payload));
                  sendCompleted.Signal();
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 1 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
            });

            DataMatcher bodyMatcher = new DataMatcher(payload);
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            Assert.IsTrue(sendStarted.Wait(TimeSpan.FromSeconds(10)));

            // This should abort the streamed send as we provided a size for the body.
            stream.Close();
            Assert.IsTrue(message.Aborted);
            Assert.IsTrue(sendCompleted.Wait(TimeSpan.FromSeconds(10)));
            Assert.Throws<ClientIllegalStateException>(() => message.RawOutputStream());
            Assert.Throws<ClientIllegalStateException>(() => _ = message.Body);
            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamSenderSessionCannotCreateNewResources()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");

            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenReceiver("test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenReceiver("test", new ReceiverOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenDurableReceiver("test", "test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenDurableReceiver("test", "test", new ReceiverOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenDynamicReceiver());
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenDynamicReceiver(new ReceiverOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenDynamicReceiver(new ReceiverOptions(), new Dictionary<string, object>()));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenSender("test"));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenSender("test", new SenderOptions()));
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenAnonymousSender());
            Assert.Throws<ClientUnsupportedOperationException>(() => sender.Session.OpenAnonymousSender(new SenderOptions()));

            peer.WaitForScriptToComplete();

            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageWaitingOnCreditWritesWhileCompleteSendWaitsInQueue()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage tracker = sender.BeginMessage();
            Stream stream = tracker.Body;

            byte[] payload1 = new byte[256];
            Array.Fill(payload1, (byte)1);
            byte[] payload2 = new byte[256];
            Array.Fill(payload2, (byte)2);
            byte[] payload3 = new byte[256];
            Array.Fill(payload3, (byte)3);

            DataMatcher dataMatcher1 = new DataMatcher(payload1);
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            DataMatcher dataMatcher2 = new DataMatcher(payload2);
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            DataMatcher dataMatcher3 = new DataMatcher(payload3);
            TransferPayloadCompositeMatcher payloadMatcher3 = new TransferPayloadCompositeMatcher();
            payloadMatcher3.MessageContentMatcher = dataMatcher3;

            bool sendFailed = false;
            CountdownEvent streamSend1Complete = new CountdownEvent(1);
            // Stream won't output until some body bytes are written.
            Task.Run(() =>
            {
               try
               {
                  stream.Write(payload1);
                  stream.Flush();
               }
               catch (IOException e)
               {
                  logger.LogInformation("send failed with error: {0}", e.Message);
                  sendFailed = true;
               }
               finally
               {
                  streamSend1Complete.Signal();
               }
            });

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            // Now trigger the next send by granting credit for payload 1
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(1).WithLinkCredit(10).Now();

            Assert.IsTrue(streamSend1Complete.Wait(TimeSpan.FromSeconds(5)), "Stream sender completed first send");
            Assert.IsFalse(sendFailed);

            CountdownEvent sendStarted = new CountdownEvent(1);
            CountdownEvent sendCompleted = new CountdownEvent(1);

            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 1 is preparing to fire:");
                  sendStarted.Signal();
                  sender.Send(IMessage<byte[]>.Create(payload3));
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 1 failed with error: {0}", e.Message);
                  sendFailed = true;
               }
               finally
               {
                  sendCompleted.Signal();
               }
            });

            Assert.IsTrue(sendStarted.Wait(TimeSpan.FromSeconds(10)));

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            // Queue a flow that will allow send by granting credit for payload 3 via sender.send
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(3).WithLinkCredit(10).Queue();
            // Now trigger the next send by granting credit for payload 2
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(2).WithLinkCredit(10).Now();

            stream.Write(payload2);
            stream.Flush();

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNullPayload().WithMore(false).Accept();
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(4).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithPayload(payloadMatcher3).WithMore(false);
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            stream.Close();

            Assert.IsTrue(sendCompleted.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestWriteToCreditLimitFramesOfMessagePayloadOneBytePerWrite()
      {
         uint WRITE_COUNT = 10;

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithIncomingWindow(WRITE_COUNT).WithNextIncomingId(0).WithLinkCredit(WRITE_COUNT).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage tracker = sender.BeginMessage();
            Stream stream = tracker.Body;

            peer.WaitForScriptToComplete();

            byte[][] payloads = new byte[WRITE_COUNT][];
            for (int i = 0; i < WRITE_COUNT; ++i)
            {
               payloads[i] = new byte[256];
               Array.Fill(payloads[i], (byte)(i + 1));
            }

            for (int i = 0; i < WRITE_COUNT; ++i)
            {
               DataMatcher dataMatcher = new DataMatcher(payloads[i]);
               TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
               payloadMatcher.MessageContentMatcher = dataMatcher;

               peer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(true);
            }

            for (int i = 0; i < WRITE_COUNT; ++i)
            {
               foreach (byte value in payloads[i])
               {
                  stream.WriteByte(value);
               }
               stream.Flush();
            }

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNullPayload().WithMore(false).Accept();

            // grant one more credit for the complete to arrive.
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(WRITE_COUNT).WithLinkCredit(1).Later(10);

            stream.Close();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestWriteToCreditLimitFramesOfMessagePayload()
      {
         uint WRITE_COUNT = 10;

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithIncomingWindow(WRITE_COUNT).WithNextIncomingId(0).WithLinkCredit(WRITE_COUNT).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage tracker = sender.BeginMessage();
            Stream stream = tracker.Body;

            peer.WaitForScriptToComplete();

            byte[][] payloads = new byte[WRITE_COUNT][];
            for (int i = 0; i < WRITE_COUNT; ++i)
            {
               payloads[i] = new byte[256];
               Array.Fill(payloads[i], (byte)(i + 1));
            }

            for (int i = 0; i < WRITE_COUNT; ++i)
            {
               DataMatcher dataMatcher = new DataMatcher(payloads[i]);
               TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
               payloadMatcher.MessageContentMatcher = dataMatcher;

               peer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(true);
            }

            for (int i = 0; i < WRITE_COUNT; ++i)
            {
               stream.Write(payloads[i]);
               stream.Flush();
            }

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNullPayload().WithMore(false).Accept();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            // grant one more credit for the complete to arrive.
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(WRITE_COUNT).WithLinkCredit(1).Now();

            stream.Close();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageFlushFailsAfterConnectionDropped()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            CountdownEvent disconnected = new CountdownEvent(1);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions();
            options.DisconnectedHandler = (connection, eventArgs) =>
            {
               disconnected.Signal();
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            Stream stream = message.Body;

            DataMatcher dataMatcher1 = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            DataMatcher dataMatcher2 = new DataMatcher(new byte[] { 4, 5, 6, 7 });
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            peer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            peer.DropAfterLastHandler();

            // Write two then after connection drops the message should fail on future writes
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Write(new byte[] { 4, 5, 6, 7 });
            stream.Flush();

            peer.WaitForScriptToComplete();

            // Next write should fail as connection should have dropped.
            stream.Write(new byte[] { 8, 9, 10, 11 });

            logger.LogInformation("Waiting for connection to report it was disconnected");
            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(5)));

            try
            {
               stream.Flush();
               Assert.Fail("Should not be able to flush after connection drop");
            }
            catch (IOException ioe)
            {
               Assert.IsTrue(ioe.InnerException is ClientException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageCloseThatFlushesFailsAfterConnectionDropped()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            CountdownEvent disconnected = new CountdownEvent(1);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions();
            options.DisconnectedHandler = (conn, ev) => disconnected.Signal();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            Stream stream = message.Body;

            DataMatcher dataMatcher1 = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            DataMatcher dataMatcher2 = new DataMatcher(new byte[] { 4, 5, 6, 7 });
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            peer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            peer.DropAfterLastHandler();

            // Write two then after connection drops the message should fail on future writes
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Write(new byte[] { 4, 5, 6, 7 });
            stream.Flush();

            peer.WaitForScriptToComplete();

            // Next write should fail as connection should have dropped.
            stream.Write(new byte[] { 8, 9, 10, 11 });

            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(10)));

            try
            {
               stream.Close();
               Assert.Fail("Should not be able to close after connection drop");
            }
            catch (IOException ioe)
            {
               Assert.IsTrue(ioe.InnerException is ClientException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageWriteThatFlushesFailsAfterConnectionDropped()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.DropAfterLastHandler();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            CountdownEvent disconnected = new CountdownEvent(1);
            IClient container = IClient.Create();
            ConnectionOptions connectionOptions = new ConnectionOptions();
            connectionOptions.DisconnectedHandler = (connection, eventArgs) => disconnected.Signal();
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            StreamSenderOptions options = new StreamSenderOptions()
            {
               WriteBufferSize = 1024
            };
            IStreamSender sender = connection.OpenStreamSender("test-queue", options);
            IStreamSenderMessage message = sender.BeginMessage();

            byte[] payload = new byte[65535];
            Array.Fill(payload, (byte)65);
            OutputStreamOptions streamOptions = new OutputStreamOptions()
            {
               BodyLength = 65535
            };
            Stream stream = message.GetBodyStream(streamOptions);

            peer.WaitForScriptToComplete();

            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(10)));

            try
            {
               stream.Write(payload);
               Assert.Fail("Should not be able to write section after connection drop");
            }
            catch (IOException ioe)
            {
               Assert.IsTrue(ioe.InnerException is ClientException);
            }

            connection.CloseAsync().GetAwaiter().GetResult();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageSendFromByteArrayInputStreamWithoutBodySizeSet()
      {
         DoTestStreamMessageSendFromByteArrayInputStream(false);
      }

      [Test]
      public void TestStreamMessageSendFromByteArrayInputStreamWithBodySizeSet()
      {
         DoTestStreamMessageSendFromByteArrayInputStream(false);
      }

      private void DoTestStreamMessageSendFromByteArrayInputStream(bool setBodySize)
      {
         Random random = new Random(Environment.TickCount);
         byte[] array = new byte[4096];
         MemoryStream bytesIn = new MemoryStream(array);

         // Populate the array with something other than zeros.
         random.NextBytes(array);

         CompositingDataSectionMatcher matcher = new CompositingDataSectionMatcher(array);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(100).Queue();
            for (int i = 0; i < (array.Length / 1023); ++i)
            {
               peer.ExpectTransfer().WithDeliveryId(0)
                                    .WithMore(true)
                                    .WithPayload(matcher);
            }
            // A small number of trailing bytes will be transmitted in the frame.
            peer.ExpectTransfer().WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithPayload(matcher);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            StreamSenderOptions options = new StreamSenderOptions()
            {
               WriteBufferSize = 1023
            };
            IStreamSender sender = connection.OpenStreamSender("test-queue", options);
            IStreamSenderMessage tracker = sender.BeginMessage();

            Stream stream;

            if (setBodySize)
            {
               stream = tracker.GetBodyStream(new OutputStreamOptions()
               {
                  BodyLength = array.Length
               });
            }
            else
            {
               stream = tracker.Body;
            }

            try
            {
               bytesIn.WriteTo(stream);
            }
            finally
            {
               // Ensure any trailing bytes get written and transfer marked as done.
               stream.Close();
            }

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestBatchAddBodySectionsWritesEach()
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
            options.WriteBufferSize = int.MaxValue;

            IStreamSender sender = connection.OpenStreamSender("test-qos", options);

            // Create a custom message format send context and ensure that no early buffer writes take place
            IStreamSenderMessage message = sender.BeginMessage();

            Assert.AreEqual(Header.DEFAULT_PRIORITY, message.Priority);
            Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT, message.DeliveryCount);
            Assert.AreEqual(Header.DEFAULT_FIRST_ACQUIRER, message.FirstAcquirer);
            Assert.AreEqual(Header.DEFAULT_TIME_TO_LIVE, message.TimeToLive);
            Assert.AreEqual(Header.DEFAULT_DURABILITY, message.Durable);

            // Gates send on remote flow having been sent and received
            session.OpenReceiver("dummy").OpenTask.Wait();

            HeaderMatcher headerMatcher = new HeaderMatcher(true);
            headerMatcher.WithDurable(true);
            headerMatcher.WithPriority((byte)1);
            headerMatcher.WithTtl(65535);
            headerMatcher.WithFirstAcquirer(true);
            headerMatcher.WithDeliveryCount(2);
            DataMatcher data1Matcher = new DataMatcher(new byte[] { 0, 1, 2, 3 }, true);
            DataMatcher data2Matcher = new DataMatcher(new byte[] { 4, 5, 6, 7 }, true);
            DataMatcher data3Matcher = new DataMatcher(new byte[] { 8, 9, 0, 1 });
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.HeaderMatcher = headerMatcher;
            payloadMatcher.AddMessageContentMatcher(data1Matcher);
            payloadMatcher.AddMessageContentMatcher(data2Matcher);
            payloadMatcher.AddMessageContentMatcher(data3Matcher);

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithMore(false).WithPayload(payloadMatcher).Accept();
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

            IList<ISection> sections = new List<ISection>();
            sections.Add(new Data(new byte[] { 0, 1, 2, 3 }));
            sections.Add(new Data(new byte[] { 4, 5, 6, 7 }));
            sections.Add(new Data(new byte[] { 8, 9, 0, 1 }));

            message.Header = header;
            message.SetBodySections(sections);

            message.Complete();

            Assert.IsNotNull(message.Tracker);
            Assert.IsTrue(message.Tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(message.Tracker.SettlementTask.Result.Settled);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendAndApplyDisposition()
      {
         DoTestSendAndApplyDisposition(false);
      }

      [Test]
      public void TestSendAndApplyDispositionAsync()
      {
         DoTestSendAndApplyDisposition(true);
      }

      private void DoTestSendAndApplyDisposition(bool dispositionAsync)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectBegin().Respond(); // Hidden session for stream sender
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithDeliveryCount(0)
                             .WithLinkCredit(10)
                             .WithIncomingWindow(1024)
                             .WithOutgoingWindow(10)
                             .WithNextIncomingId(0)
                             .WithNextOutgoingId(1).Queue();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IStreamSender sender = (IStreamSender)connection.OpenStreamSender("test-queue").OpenTask.Result;

            // This ensures that the flow to sender is processed before we try-send
            IReceiver receiver = session.OpenReceiver("test-queue", new ReceiverOptions()
            {
               CreditWindow = 0
            }
            ).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload();
            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            IStreamTracker tracker = sender.Send(message);

            if (dispositionAsync)
            {
               Assert.DoesNotThrowAsync(async () => await tracker.DispositionAsync(IDeliveryState.Accepted(), true));
            }
            else
            {
               tracker.Disposition(IDeliveryState.Accepted(), true);
            }

            Assert.IsNotNull(tracker);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      private static IDeliveryTagGenerator CustomTagGenerator()
      {
         return new CustomDeliveryTagGenerator();
      }

      private static IDeliveryTagGenerator CustomNullTagGenerator()
      {
         return null;
      }

      private class CustomDeliveryTagGenerator : IDeliveryTagGenerator
      {
         private int count = 1;

         IDeliveryTag IDeliveryTagGenerator.NextTag()
         {
            switch (count++)
            {
               case 1:
                  return new DeliveryTag(new byte[] { 1, 1, 1 });
               case 2:
                  return new DeliveryTag(new byte[] { 2, 2, 2 });
               case 3:
                  return new DeliveryTag(new byte[] { 3, 3, 3 });
               default:
                  throw new InvalidOperationException("Only supports creating three tags");
            }
         }
      }

      [Test]
      public void TestSenderUsesCustomDeliveryTagGeneratorConfiguration()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;

            StreamSenderOptions options = new StreamSenderOptions()
            {
               DeliveryMode = DeliveryMode.AtLeastOnce,
               AutoSettle = true,
               DeliveryTagGeneratorSupplier = CustomTagGenerator
            };
            IStreamSender sender = connection.OpenStreamSender("test-tags", options).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 1, 1, 1 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 2, 2, 2 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 3, 3, 3 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");
            IStreamTracker tracker1 = sender.Send(message);
            IStreamTracker tracker2 = sender.Send(message);
            IStreamTracker tracker3 = sender.Send(message);

            Assert.IsNotNull(tracker1);
            Assert.IsNotNull(tracker1.SettlementTask.Result);
            Assert.IsNotNull(tracker2);
            Assert.IsNotNull(tracker2.SettlementTask.Result);
            Assert.IsNotNull(tracker3);
            Assert.IsNotNull(tracker3.SettlementTask.Result);

            sender.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotCreateSenderWhenTagGeneratorReturnsNull()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            StreamSenderOptions options = new StreamSenderOptions()
            {
               DeliveryMode = DeliveryMode.AtLeastOnce,
               AutoSettle = true,
               DeliveryTagGeneratorSupplier = CustomNullTagGenerator
            };

            try
            {
               _ = connection.OpenStreamSender("test-tags", options).OpenTask.Result;
               Assert.Fail("Should not create a sender if the tag generator is not supplied");
            }
            catch (ClientException)
            {
               // Expected
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }
   }
}