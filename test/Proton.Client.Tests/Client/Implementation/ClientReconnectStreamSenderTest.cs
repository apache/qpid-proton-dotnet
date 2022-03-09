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
using System.Threading;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectStreamSenderTest : ClientBaseTestFixture
   {
      [Test]
      public void TestStreamMessageFlushFailsAfterConnectionDropped()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().Respond();
            secondPeer.RemoteFlow().WithLinkCredit(1).Queue();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            ConnectionOptions options = new ConnectionOptions();
            options.IdleTimeout = 5000;
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            Stream stream = message.Body;

            DataMatcher dataMatcher1 = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            DataMatcher dataMatcher2 = new DataMatcher(new byte[] { 4, 5, 6, 7 });
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            firstPeer.WaitForScriptToComplete();
            firstPeer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            firstPeer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            firstPeer.DropAfterLastHandler();

            // Write two then after connection drops the message should fail on future writes
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Write(new byte[] { 4, 5, 6, 7 });
            stream.Flush();

            firstPeer.WaitForScriptToComplete(TimeSpan.FromSeconds(5));
            // Reconnection should have occurred now and we should not be able to flush data from
            // the stream as its initial sender instance was closed on disconnect.
            secondPeer.WaitForScriptToComplete(TimeSpan.FromSeconds(5));
            secondPeer.ExpectClose().Respond();

            // Next write should fail as connection should have dropped.
            stream.Write(new byte[] { 8, 9, 10, 11 });

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

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageCloseThatFlushesFailsAfterConnectionDropped()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().Respond();
            secondPeer.RemoteFlow().WithLinkCredit(1).Queue();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            ConnectionOptions options = new ConnectionOptions();
            options.IdleTimeout = 5000;
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            Stream stream = message.Body;

            DataMatcher dataMatcher1 = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher1 = new TransferPayloadCompositeMatcher();
            payloadMatcher1.MessageContentMatcher = dataMatcher1;

            DataMatcher dataMatcher2 = new DataMatcher(new byte[] { 4, 5, 6, 7 });
            TransferPayloadCompositeMatcher payloadMatcher2 = new TransferPayloadCompositeMatcher();
            payloadMatcher2.MessageContentMatcher = dataMatcher2;

            firstPeer.WaitForScriptToComplete();
            firstPeer.ExpectTransfer().WithPayload(payloadMatcher1).WithMore(true);
            firstPeer.ExpectTransfer().WithPayload(payloadMatcher2).WithMore(true);
            firstPeer.DropAfterLastHandler();

            // Write two then after connection drops the message should fail on future writes
            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Write(new byte[] { 4, 5, 6, 7 });
            stream.Flush();

            firstPeer.WaitForScriptToComplete();

            // Reconnection should have occurred now and we should not be able to flush data from
            // the stream as its initial sender instance was closed on disconnect.
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectClose().Respond();

            // Next write should fail as connection should have dropped.
            stream.Write(new byte[] { 8, 9, 10, 11 });

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

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageWriteThatFlushesFailsAfterConnectionDropped()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().Respond();
            secondPeer.RemoteFlow().WithLinkCredit(1).Queue();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            ConnectionOptions options = new ConnectionOptions();
            options.MaxFrameSize = 32_768;
            options.IdleTimeout = 5_000;
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            IStreamSenderMessage message = sender.BeginMessage();

            byte[] payload = new byte[65536];
            Array.Fill(payload, (byte)65);
            OutputStreamOptions streamOptions = new OutputStreamOptions()
            {
               BodyLength = payload.Length
            };
            Stream stream = message.GetBodyStream(streamOptions);

            firstPeer.WaitForScriptToComplete();

            // Reconnection should have occurred now and we should not be able to flush data from
            // the stream as its initial sender instance was closed on disconnect.
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectClose().Respond();

            try
            {
               stream.Write(payload);
               Assert.Fail("Should not be able to write section after connection drop");
            }
            catch (IOException ioe)
            {
               Assert.IsTrue(ioe.InnerException is ClientException);
            }

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamSenderRecoveredAfterReconnectCanCreateAndStreamBytes()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().Respond();
            secondPeer.RemoteFlow().WithLinkCredit(1).Queue();
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
            IStreamSender sender = connection.OpenStreamSender("test-queue");

            firstPeer.WaitForScriptToComplete();

            // After reconnection a new stream sender message should be properly created
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectTransfer().WithMore(true).WithPayload(new byte[] { 0, 1, 2, 3 });
            secondPeer.ExpectTransfer().WithMore(false).WithNullPayload();
            secondPeer.ExpectDetach().Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            IStreamSenderMessage message = sender.BeginMessage();
            Stream stream = message.RawOutputStream();

            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();
            stream.Close();

            sender.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestStreamMessageWriteThatFlushesFailsAfterConnectionDroppedAndReconnected()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            DataMatcher dataMatcher = new DataMatcher(new byte[] { 0, 1, 2, 3 });
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            payloadMatcher.MessageContentMatcher = dataMatcher;

            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.ExpectTransfer().WithPayload(payloadMatcher).WithMore(true);
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().Respond();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            ConnectionOptions options = new ConnectionOptions();
            options.MaxFrameSize = 32768;
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            StreamSenderOptions senderOptions = new StreamSenderOptions()
            {
               SendTimeout = 1000
            };
            IStreamSender sender = connection.OpenStreamSender("test-queue", senderOptions);
            IStreamSenderMessage message = sender.BeginMessage();
            Stream stream = message.Body;

            stream.Write(new byte[] { 0, 1, 2, 3 });
            stream.Flush();

            firstPeer.WaitForScriptToComplete();

            // Reconnection should have occurred now and we should not be able to flush data
            // from the stream as its initial sender instance was closed on disconnect.
            secondPeer.WaitForScriptToComplete();

            // Ensure that idle processing happens in case send blocks so we can see the
            // send timed out exception
            secondPeer.RemoteEmptyFrame().Later(5000);
            secondPeer.RemoteEmptyFrame().Later(10000);
            secondPeer.RemoteEmptyFrame().Later(15000);
            secondPeer.RemoteEmptyFrame().Later(20000); // Test timeout kicks in now
            secondPeer.ExpectClose().Respond();

            byte[] payload = new byte[1024];
            Array.Fill(payload, (byte)65);

            try
            {
               stream.Write(payload);
               stream.Flush();
               Assert.Fail("Should not be able to write section after connection drop");
            }
            catch (IOException ioe)
            {
               Assert.IsFalse(ioe.InnerException is ClientSendTimedOutException);
               Assert.IsTrue(ioe.InnerException is ClientConnectionRemotelyClosedException);
            }

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }
   }
}