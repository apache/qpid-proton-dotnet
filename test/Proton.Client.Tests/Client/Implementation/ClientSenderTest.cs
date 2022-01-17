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
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientSenderTest : ClientBaseTestFixture
   {
      [Test]
      public void TestCreateSenderAndClose()
      {
         DoTestCreateSenderAndCloseOrDetach(true);
      }

      [Test]
      public void TestCreateSenderAndDetach()
      {
         DoTestCreateSenderAndCloseOrDetach(false);
      }

      private void DoTestCreateSenderAndCloseOrDetach(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISession session = connection.OpenSession();
            session.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISender sender = session.OpenSender("test-queue");
            sender.OpenTask.Wait(TimeSpan.FromSeconds(10));

            if (close)
            {
               sender.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            }
            else
            {
               sender.DetachAsync().Wait(TimeSpan.FromSeconds(10));
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateSenderAndCloseSync()
      {
         DoTestCreateSenderAndCloseOrDetachSync(true);
      }

      [Test]
      public void TestCreateSenderAndDetachSync()
      {
         DoTestCreateSenderAndCloseOrDetachSync(false);
      }

      private void DoTestCreateSenderAndCloseOrDetachSync(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISession session = connection.OpenSession();
            session.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISender sender = session.OpenSender("test-queue");
            sender.OpenTask.Wait(TimeSpan.FromSeconds(10));

            if (close)
            {
               sender.Close();
            }
            else
            {
               sender.Detach();
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateSenderAndCloseWithErrorSync()
      {
         DoTestCreateSenderAndCloseOrDetachWithErrorSync(true);
      }

      [Test]
      public void TestCreateSenderAndDetachWithErrorSync()
      {
         DoTestCreateSenderAndCloseOrDetachWithErrorSync(false);
      }

      private void DoTestCreateSenderAndCloseOrDetachWithErrorSync(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().WithError("amqp-resource-deleted", "an error message").WithClosed(close).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISession session = connection.OpenSession();
            session.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISender sender = session.OpenSender("test-queue");
            sender.OpenTask.Wait(TimeSpan.FromSeconds(10));

            if (close)
            {
               sender.Close(IErrorCondition.Create("amqp-resource-deleted", "an error message", null));
            }
            else
            {
               sender.Detach(IErrorCondition.Create("amqp-resource-deleted", "an error message", null));
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSenderOpenRejectedByRemote()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().Respond().WithNullTarget();
            peer.RemoteDetach().WithErrorCondition(AmqpError.UNAUTHORIZED_ACCESS.ToString(), "Cannot read from this address").Queue();
            peer.ExpectDetach();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISession session = connection.OpenSession();
            session.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISender sender = session.OpenSender("test-queue");

            try
            {
               sender.OpenTask.Wait(TimeSpan.FromSeconds(20));
               Assert.Fail("Open of sender should fail due to remote indicating pending close.");
            }
            catch (Exception exe)
            {
               Assert.IsNotNull(exe.InnerException);
               Assert.IsTrue(exe.InnerException is ClientLinkRemotelyClosedException);
               ClientLinkRemotelyClosedException linkClosed = (ClientLinkRemotelyClosedException)exe.InnerException;
               Assert.IsNotNull(linkClosed.Error);
               Assert.AreEqual(AmqpError.UNAUTHORIZED_ACCESS.ToString(), linkClosed.Error.Condition);
            }

            peer.WaitForScriptToComplete();

            // Should not result in any close being sent now, already closed.
            sender.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete(TimeSpan.FromSeconds(10));
         }
      }

      [Test]
      public void TestRemotelyCloseSenderLinkWithRedirect()
      {
         string redirectVhost = "vhost";
         string redirectNetworkHost = "localhost";
         string redirectAddress = "redirect-queue";
         int redirectPort = 5677;
         string redirectScheme = "wss";
         string redirectPath = "/websockets";

         // Tell the test peer to close the connection when executing its last handler
         IDictionary<string, object> errorInfo = new Dictionary<string, object>();
         errorInfo.Add(ClientConstants.OPEN_HOSTNAME.ToString(), redirectVhost);
         errorInfo.Add(ClientConstants.NETWORK_HOST.ToString(), redirectNetworkHost);
         errorInfo.Add(ClientConstants.PORT.ToString(), redirectPort);
         errorInfo.Add(ClientConstants.SCHEME.ToString(), redirectScheme);
         errorInfo.Add(ClientConstants.PATH.ToString(), redirectPath);
         errorInfo.Add(ClientConstants.ADDRESS.ToString(), redirectAddress);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond().WithNullTarget();
            peer.RemoteDetach().WithClosed(true)
                               .WithErrorCondition(LinkError.REDIRECT.ToString(), "Not accepting links here", errorInfo).Queue();
            peer.ExpectDetach();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue");

            try
            {
               sender.OpenTask.Wait();
               Assert.Fail("Should not be able to create sender since the remote is redirecting.");
            }
            catch (Exception ex)
            {
               logger.LogDebug("Received expected exception from sender open: {0}", ex.Message);
               Exception cause = ex.InnerException;
               Assert.IsTrue(cause is ClientLinkRedirectedException);

               ClientLinkRedirectedException linkRedirect = (ClientLinkRedirectedException)ex.InnerException;

               Assert.AreEqual(redirectVhost, linkRedirect.Hostname);
               Assert.AreEqual(redirectNetworkHost, linkRedirect.NetworkHostname);
               Assert.AreEqual(redirectPort, linkRedirect.Port);
               Assert.AreEqual(redirectScheme, linkRedirect.Scheme);
               Assert.AreEqual(redirectPath, linkRedirect.Path);
               Assert.AreEqual(redirectAddress, linkRedirect.Address);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenSenderTimesOutWhenNoAttachResponseReceivedTimeout()
      {
         DoTestOpenSenderTimesOutWhenNoAttachResponseReceived(true);
      }

      [Test]
      public void TestOpenSenderTimesOutWhenNoAttachResponseReceivedNoTimeout()
      {
         DoTestOpenSenderTimesOutWhenNoAttachResponseReceived(false);
      }

      private void DoTestOpenSenderTimesOutWhenNoAttachResponseReceived(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender();
            peer.ExpectDetach();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            SenderOptions options = new SenderOptions();
            options.OpenTimeout = 10;
            ISender sender = session.OpenSender("test-queue", options);

            try
            {
               if (timeout)
               {
                  sender.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  sender.OpenTask.Wait();
               }

               Assert.Fail("Should not complete the open future without an error");
            }
            catch (Exception exe)
            {
               Exception cause = exe.InnerException;
               Assert.IsTrue(cause is ClientOperationTimedOutException);
            }

            logger.LogInformation("Closing connection after waiting for sender open");

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenSenderWaitWithTimeoutFailsWhenConnectionDrops()
      {
         DoTestOpenSenderWaitFailsWhenConnectionDrops(true);
      }

      [Test]
      public void TestOpenSenderWaitWithNoTimeoutFailsWhenConnectionDrops()
      {
         DoTestOpenSenderWaitFailsWhenConnectionDrops(false);
      }

      private void DoTestOpenSenderWaitFailsWhenConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue");

            Thread.Sleep(10); // Allow some time for attach to get written

            try
            {
               if (timeout)
               {
                  sender.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  sender.OpenTask.Wait();
               }

               Assert.Fail("Should not complete the open future without an error");
            }
            catch (Exception exe)
            {
               Exception cause = exe.InnerException;
               Assert.IsTrue(cause is ClientConnectionRemotelyClosedException);
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseSenderTimesOutWhenNoCloseResponseReceivedTimeout()
      {
         DoTestCloseOrDetachSenderTimesOutWhenNoCloseResponseReceived(true, true);
      }

      [Test]
      public void TestCloseSenderTimesOutWhenNoCloseResponseReceivedNoTimeout()
      {
         DoTestCloseOrDetachSenderTimesOutWhenNoCloseResponseReceived(true, false);
      }

      [Test]
      public void TestDetachSenderTimesOutWhenNoCloseResponseReceivedTimeout()
      {
         DoTestCloseOrDetachSenderTimesOutWhenNoCloseResponseReceived(false, true);
      }

      [Test]
      public void TestDetachSenderTimesOutWhenNoCloseResponseReceivedNoTimeout()
      {
         DoTestCloseOrDetachSenderTimesOutWhenNoCloseResponseReceived(false, false);
      }

      private void DoTestCloseOrDetachSenderTimesOutWhenNoCloseResponseReceived(bool close, bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               CloseTimeout = 10
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            try
            {
               if (close)
               {
                  if (timeout)
                  {
                     sender.CloseAsync().Wait(TimeSpan.FromSeconds(10));
                  }
                  else
                  {
                     sender.CloseAsync().Wait();
                  }
               }
               else
               {
                  if (timeout)
                  {
                     sender.DetachAsync().Wait(TimeSpan.FromSeconds(10));
                  }
                  else
                  {
                     sender.DetachAsync().Wait();
                  }
               }

               Assert.Fail("Should not complete the close or detach future without an error");
            }
            catch (Exception exe)
            {
               Exception cause = exe.InnerException;
               Assert.IsTrue(cause is ClientOperationTimedOutException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendTimesOutWhenNoCreditIssued()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               SendTimeout = 10
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            IMessage<string> message = IMessage<string>.Create("Hello World");
            try
            {
               sender.Send(message);
               Assert.Fail("Should throw a send timed out exception");
            }
            catch (ClientSendTimedOutException)
            {
               // Expected error, ignore
            }

            sender.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendCompletesWhenCreditEventuallyOffered()
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
               SendTimeout = 2000
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();

            // Expect a transfer but only after the flow which is delayed to allow the
            // client time to block on credit.
            peer.ExpectTransfer().WithNonNullPayload();
            peer.RemoteFlow().WithDeliveryCount(0)
                             .WithLinkCredit(1)
                             .WithIncomingWindow(1024)
                             .WithOutgoingWindow(10)
                             .WithNextIncomingId(0)
                             .WithNextOutgoingId(1).Later(30);
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");
            try
            {
               logger.LogDebug("Attempting send with sender: {}", sender);
               sender.Send(message);
            }
            catch (ClientSendTimedOutException)
            {
               Assert.Fail("Should not throw a send timed out exception");
            }

            sender.CloseAsync();

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Test peer needs a transfer composite payload matcher")]
      [Test]
      public void TestSendWhenCreditIsAvailable()
      {
         DoTestSendWhenCreditIsAvailable(false, false);
      }

      [Ignore("Test peer needs a transfer composite payload matcher")]
      [Test]
      public void TestTrySendWhenCreditIsAvailable()
      {
         DoTestSendWhenCreditIsAvailable(true, false);
      }

      [Ignore("Test peer needs a transfer composite payload matcher")]
      [Test]
      public void TestSendWhenCreditIsAvailableWithDeliveryAnnotations()
      {
         DoTestSendWhenCreditIsAvailable(false, true);
      }

      [Ignore("Test peer needs a transfer composite payload matcher")]
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
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            // This ensures that the flow to sender is processed before we try-send
            IReceiver receiver = session.OpenReceiver("test-queue", new ReceiverOptions()
            {
               CreditWindow = 0
            }
            ).OpenTask.Result;

            IDictionary<string, object> deliveryAnnotations = new Dictionary<string, object>();
            deliveryAnnotations.Add("da1", 1);
            deliveryAnnotations.Add("da2", 2);
            deliveryAnnotations.Add("da3", 3);
            DeliveryAnnotationsMatcher daMatcher = new DeliveryAnnotationsMatcher(true);
            daMatcher.WithEntry("da1", Test.Driver.Matchers.Is.EqualTo(1));
            daMatcher.WithEntry("da2", Test.Driver.Matchers.Is.EqualTo(2));
            daMatcher.WithEntry("da3", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            //TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            //if (addDeliveryAnnotations)
            //{
            //   payloadMatcher.DeliveryAnnotationsMatcher = daMatcher);
            //}
            //payloadMatcher.MessageContentMatcher = bodyMatcher);

            peer.WaitForScriptToComplete();
            //peer.ExpectTransfer().WithPayload(payloadMatcher);
            peer.ExpectDetach().Respond();
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
      public void TestTrySendWhenNoCreditAvailable()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               SendTimeout = 1
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            IMessage<string> message = IMessage<string>.Create("Hello World");
            Assert.IsNull(sender.TrySend(message));

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateSenderWithQoSOfAtMostOnce()
      {
         DoTestCreateSenderWithConfiguredQoS(DeliveryMode.AtMostOnce);
      }

      [Test]
      public void TestCreateSenderWithQoSOfAtLeastOnce()
      {
         DoTestCreateSenderWithConfiguredQoS(DeliveryMode.AtLeastOnce);
      }

      private void DoTestCreateSenderWithConfiguredQoS(DeliveryMode qos)
      {
         byte sndMode = (byte)(qos == DeliveryMode.AtMostOnce ? SenderSettleMode.Settled : SenderSettleMode.Unsettled);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender()
                               .WithSndSettleMode(sndMode)
                               .WithRcvSettleMode((byte)ReceiverSettleMode.First)
                               .Respond()
                               .WithSndSettleMode(sndMode)
                               .WithRcvSettleMode((byte?)ReceiverSettleMode.Second);
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = qos
            };
            ISender sender = session.OpenSender("test-queue", options).OpenTask.Result;

            Assert.AreEqual("test-queue", sender.Address);

            sender.CloseAsync().Wait();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendAutoSettlesOnceRemoteSettles()
      {
         DoTestSentMessageGetsAutoSettledAfterRemoteSettles(false);
      }

      [Test]
      public void TestTrySendAutoSettlesOnceRemoteSettles()
      {
         DoTestSentMessageGetsAutoSettledAfterRemoteSettles(true);
      }

      private void DoTestSentMessageGetsAutoSettledAfterRemoteSettles(bool trySend)
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
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            // This ensures that the flow to sender is processed before we try-send
            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 0
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .Respond()
                                 .WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            ITracker tracker;
            if (trySend)
            {
               tracker = sender.TrySend(message);
            }
            else
            {
               tracker = sender.Send(message);
            }

            Assert.IsNotNull(tracker);
            Assert.IsNotNull(tracker.SettlementTask.Result);
            Assert.AreEqual(tracker.RemoteState.Type, DeliveryStateType.Accepted);

            sender.CloseAsync();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendDoesNotAutoSettlesOnceRemoteSettlesIfAutoSettleOff()
      {
         DoTestSentMessageNotAutoSettledAfterRemoteSettles(false);
      }

      [Test]
      public void TestTrySendDoesNotAutoSettlesOnceRemoteSettlesIfAutoSettleOff()
      {
         DoTestSentMessageNotAutoSettledAfterRemoteSettles(true);
      }

      private void DoTestSentMessageNotAutoSettledAfterRemoteSettles(bool trySend)
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
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession();
            SenderOptions options = new SenderOptions()
            {
               AutoSettle = false
            };
            ISender sender = session.OpenSender("test-queue", options).OpenTask.Result;

            // This ensures that the flow to sender is processed before we try-send
            ReceiverOptions rcvOptions = new ReceiverOptions()
            {
               CreditWindow = 0
            };
            IReceiver receiver = session.OpenReceiver("test-queue", rcvOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .Respond()
                                 .WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            ITracker tracker;
            if (trySend)
            {
               tracker = sender.TrySend(message);
            }
            else
            {
               tracker = sender.Send(message);
            }

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(tracker.RemoteState.Type, DeliveryStateType.Accepted);
            Assert.IsNull(tracker.State);
            Assert.IsFalse(tracker.Settled);

            sender.CloseAsync();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSenderSendingSettledCompletesTrackerAcknowledgeFuture()
      {
         DoTestSenderSendingSettledCompletesTrackerAcknowledgeFuture(false);
      }

      [Test]
      public void TestSenderTrySendingSettledCompletesTrackerAcknowledgeFuture()
      {
         DoTestSenderSendingSettledCompletesTrackerAcknowledgeFuture(true);
      }

      private void DoTestSenderSendingSettledCompletesTrackerAcknowledgeFuture(bool trySend)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender()
                               .WithSenderSettleModeSettled()
                               .WithReceiverSettlesFirst()
                               .Respond()
                               .WithSenderSettleModeSettled()
                               .WithReceiverSettlesFirst();
            peer.RemoteFlow().WithLinkCredit(10).Queue();
            peer.ExpectAttach().Respond();  // Open a receiver to ensure sender link has processed
            peer.ExpectFlow();              // the inbound flow frame we sent previously before send.
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession();
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender sender = session.OpenSender("test-queue", options).OpenTask.Result;

            Assert.AreEqual("test-queue", sender.Address);
            session.OpenReceiver("dummy").OpenTask.Wait();

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");
            ITracker tracker;
            if (trySend)
            {
               tracker = sender.TrySend(message);
            }
            else
            {
               tracker = sender.Send(message);
            }

            Assert.IsNotNull(tracker);
            Assert.IsTrue(tracker.SettlementTask.IsCompleted);
            Assert.IsTrue(tracker.SettlementTask.Result.Settled);

            sender.CloseAsync().Wait();
            connection.CloseAsync().Wait(TimeSpan.FromSeconds(5));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSenderIncrementsTransferTagOnEachSend()
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
            ISession session = connection.OpenSession();
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtLeastOnce,
               AutoSettle = false
            };
            ISender sender = session.OpenSender("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 0 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 1 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 2 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");
            ITracker tracker1 = sender.Send(message);
            ITracker tracker2 = sender.Send(message);
            ITracker tracker3 = sender.Send(message);

            Assert.IsNotNull(tracker1);
            Assert.IsFalse(tracker1.SettlementTask.Result.Settled);
            Assert.IsNotNull(tracker2);
            Assert.IsFalse(tracker2.SettlementTask.Result.Settled);
            Assert.IsNotNull(tracker3);
            Assert.IsFalse(tracker3.SettlementTask.Result.Settled);

            sender.CloseAsync();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSenderSendsSettledInAtLeastOnceMode()
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
            ISession session = connection.OpenSession();
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce,
               AutoSettle = false
            };
            ISender sender = session.OpenSender("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { }).WithSettled(true);
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { }).WithSettled(true);
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { }).WithSettled(true);
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");
            ITracker tracker1 = sender.Send(message);
            ITracker tracker2 = sender.Send(message);
            ITracker tracker3 = sender.Send(message);

            Assert.IsNotNull(tracker1);
            Assert.IsTrue(tracker1.SettlementTask.Result.Settled);
            Assert.IsNotNull(tracker2);
            Assert.IsTrue(tracker2.SettlementTask.Result.Settled);
            Assert.IsNotNull(tracker3);
            Assert.IsTrue(tracker3.SettlementTask.Result.Settled);

            sender.CloseAsync().Wait();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateAnonymousSenderWhenRemoteDoesNotOfferSupportForIt()
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
            ISession session = connection.OpenSession().OpenTask.Result;

            try
            {
               session.OpenAnonymousSender();
               Assert.Fail("Should not be able to open an anonymous sender when remote does not offer anonymous relay");
            }
            catch (ClientUnsupportedOperationException unsupported)
            {
               logger.LogInformation("Caught expected error: ", unsupported);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateAnonymousSenderBeforeKnowingRemoteDoesNotOfferSupportForIt()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender anonymousSender = session.OpenAnonymousSender();

            IMessage<string> message = IMessage<string>.Create("Hello World");
            message.To = "my-queue";

            peer.WaitForScriptToComplete();
            peer.RemoteOpen().Now();
            peer.RespondToLastBegin().Now();
            peer.ExpectClose().Respond();

            try
            {
               anonymousSender.Send(message);
               Assert.Fail("Should not be able to open an anonymous sender when remote does not offer anonymous relay");
            }
            catch (ClientUnsupportedOperationException unsupported)
            {
               logger.LogInformation("Caught expected error: ", unsupported);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateAnonymousSenderAppliesOptions()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond().WithOfferedCapabilities("ANONYMOUS-RELAY");
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithSenderSettleModeSettled()
                                          .WithReceiverSettlesFirst()
                                          .WithTarget().WithAddress(Test.Driver.Matchers.Is.NullValue())
                                          .And().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            SenderOptions senderOptions = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtMostOnce
            };
            ISender anonymousSender = session.OpenAnonymousSender(senderOptions);

            _ = anonymousSender.OpenTask.Result;

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAnonymousSenderOpenHeldUntilConnectionOpenedAndSupportConfirmed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenAnonymousSender();

            peer.WaitForScriptToComplete();

            // This should happen after we inject the held open and attach
            peer.ExpectAttach().OfSender().WithTarget().WithAddress(Test.Driver.Matchers.Is.NullValue()).And().Respond();
            peer.ExpectClose().Respond();

            // Inject held responses to get the ball rolling again
            peer.RemoteOpen().WithOfferedCapabilities("ANONYMOUS-RELAY").Now();
            peer.RespondToLastBegin().Now();

            try
            {
               sender.OpenTask.Wait();
            }
            catch (Exception ex)
            {
               Assert.Fail("Open of Sender failed waiting for response: " + ex.InnerException);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSenderGetRemotePropertiesWaitsForRemoteAttach()
      {
         TryReadSenderRemoteProperties(true);
      }

      [Test]
      public void TestSenderGetRemotePropertiesFailsAfterOpenTimeout()
      {
         TryReadSenderRemoteProperties(false);
      }

      private void TryReadSenderRemoteProperties(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            SenderOptions options = new SenderOptions()
            {
               OpenTimeout = 75
            };
            ISender sender = session.OpenSender("test-sender", options);

            peer.WaitForScriptToComplete();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            expectedProperties.Add("TEST", "test-property");

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.RespondToLastAttach().WithPropertiesMap(expectedProperties).Later(10);
            }
            else
            {
               peer.ExpectDetach();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(sender.Properties, "Remote should have responded with a remote properties value");
               Assert.AreEqual(expectedProperties, sender.Properties);
            }
            else
            {
               try
               {
                  _ = sender.Properties;
                  Assert.Fail("Should failed to get remote state due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               sender.CloseAsync().Wait();
            }
            catch (AggregateException ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent.");
            }

            logger.LogDebug("*** Test read remote properties ***");

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestGetRemoteOfferedCapabilitiesWaitsForRemoteAttach()
      {
         TryReadRemoteOfferedCapabilities(true);
      }

      [Test]
      public void TestGetRemoteOfferedCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadRemoteOfferedCapabilities(false);
      }

      private void TryReadRemoteOfferedCapabilities(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               OpenTimeout = 75
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-sender");

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.RespondToLastAttach().WithOfferedCapabilities("QUEUE").Later(10);
            }
            else
            {
               peer.ExpectDetach();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(sender.OfferedCapabilities, "Remote should have responded with a remote offered Capabilities value");
               Assert.AreEqual(1, sender.OfferedCapabilities.Count);
               Assert.AreEqual("QUEUE", sender.OfferedCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = sender.OfferedCapabilities;
                  Assert.Fail("Should failed to get remote state due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               sender.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent.");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestGetRemoteDesiredCapabilitiesWaitsForRemoteAttach()
      {
         TryReadRemoteDesiredCapabilities(true);
      }

      [Test]
      public void TestGetRemoteDesiredCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadRemoteDesiredCapabilities(false);
      }

      private void TryReadRemoteDesiredCapabilities(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               OpenTimeout = 75
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-sender");

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.RespondToLastAttach().WithDesiredCapabilities("Error-Free").Later(10);
            }
            else
            {
               peer.ExpectDetach();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(sender.DesiredCapabilities, "Remote should have responded with a remote desired Capabilities value");
               Assert.AreEqual(1, sender.DesiredCapabilities.Count);
               Assert.AreEqual("Error-Free", sender.DesiredCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = sender.DesiredCapabilities;
                  Assert.Fail("Should failed to get remote state due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               sender.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and detach sent.");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenSenderWithLinCapabilities()
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
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            SenderOptions senderOptions = new SenderOptions();
            senderOptions.TargetOptions.Capabilities = new string[] { "queue" };
            ISender sender = session.OpenSender("test-queue", senderOptions);

            sender.OpenTask.Wait();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseSenderWithErrorCondition()
      {
         DoTestCloseOrDetachWithErrorCondition(true);
      }

      [Test]
      public void TestDetachSenderWithErrorCondition()
      {
         DoTestCloseOrDetachWithErrorCondition(false);
      }

      public void DoTestCloseOrDetachWithErrorCondition(bool close)
      {
         String condition = "amqp:link:detach-forced";
         String description = "something bad happened.";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().WithClosed(close).WithError(condition, description).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            if (close)
            {
               sender.CloseAsync(IErrorCondition.Create(condition, description, null));
            }
            else
            {
               sender.DetachAsync(IErrorCondition.Create(condition, description, null));
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

   }
}