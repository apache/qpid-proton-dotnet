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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Types;

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
            ISession session = connection.OpenSession().OpenTask.Result;
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
         DoTestSendCompletesWhenCreditEventuallyOffered(false);
      }

      [Test]
      public void TestSendAsyncCompletesWhenCreditEventuallyOffered()
      {
         DoTestSendCompletesWhenCreditEventuallyOffered(true);
      }

      private void DoTestSendCompletesWhenCreditEventuallyOffered(bool sendAsync)
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
               if (sendAsync)
               {
                  Assert.DoesNotThrowAsync(async () => await sender.SendAsync(message));
               }
               else
               {
                  sender.Send(message);
               }
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
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            if (addDeliveryAnnotations)
            {
               payloadMatcher.DeliveryAnnotationsMatcher = daMatcher;
            }
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithPayload(payloadMatcher);
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

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload();
            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            ITracker tracker = sender.Send(message);

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

      [Test]
      public void TestTrySendWhenNoCreditAvailable()
      {
         DoTestTrySendWhenNoCreditAvailable(false);
      }

      [Test]
      public void TestTrySendAsyncWhenNoCreditAvailable()
      {
         DoTestTrySendWhenNoCreditAvailable(true);
      }

      private void DoTestTrySendWhenNoCreditAvailable(bool sendAsync)
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

            if (sendAsync)
            {
               Assert.IsNull(sender.TrySendAsync(message).GetAwaiter().GetResult());
            }
            else
            {
               Assert.IsNull(sender.TrySend(message));
            }

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
         DoTestSentMessageGetsAutoSettledAfterRemoteSettles(false, false);
      }

      [Test]
      public void TestTrySendAutoSettlesOnceRemoteSettles()
      {
         DoTestSentMessageGetsAutoSettledAfterRemoteSettles(true, false);
      }

      [Test]
      public void TestSendAsyncAutoSettlesOnceRemoteSettles()
      {
         DoTestSentMessageGetsAutoSettledAfterRemoteSettles(false, true);
      }

      [Test]
      public void TestTrySendAsyncAutoSettlesOnceRemoteSettles()
      {
         DoTestSentMessageGetsAutoSettledAfterRemoteSettles(true, true);
      }

      private void DoTestSentMessageGetsAutoSettledAfterRemoteSettles(bool trySend, bool useAsync)
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

            ITracker tracker = null;
            if (trySend)
            {
               if (useAsync)
               {
                  tracker = sender.TrySend(message);
               }
               else
               {
                  Assert.DoesNotThrowAsync(async () => tracker = await sender.TrySendAsync(message));
               }
            }
            else
            {
               if (useAsync)
               {
                  tracker = sender.Send(message);
               }
               else
               {
                  Assert.DoesNotThrowAsync(async () => tracker = await sender.SendAsync(message));
               }
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
               logger.LogInformation("Caught expected error: {0}", unsupported.Message);
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
               logger.LogInformation("Caught expected error: {0}", unsupported.Message);
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
               OpenTimeout = attachResponse ? 5000 : 150
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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               sender.CloseAsync().Wait();
            }
            catch (AggregateException ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
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
               OpenTimeout = attachResponse ? 5000 : 75
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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               sender.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
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
               OpenTimeout = attachResponse ? 5000 : 150
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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               sender.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
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

      [Test]
      public void TestSendMultipleMessages()
      {
         uint CREDIT = 20;

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithDeliveryCount(0).WithLinkCredit(CREDIT).Queue();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            // This ensures that the flow to sender is processed before we try-send
            IReceiver receiver = session.OpenReceiver("test-queue", new ReceiverOptions()
            {
               CreditWindow = 0
            }
            ).OpenTask.Result;

            peer.WaitForScriptToComplete();

            List<ITracker> sentMessages = new List<ITracker>();

            for (uint i = 0; i < CREDIT; ++i)
            {
               peer.ExpectTransfer().WithDeliveryId(i)
                                    .WithNonNullPayload()
                                    .WithSettled(false)
                                    .Respond()
                                    .WithSettled(true)
                                    .WithState().Accepted();
            }
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            for (uint i = 0; i < CREDIT; ++i)
            {
               ITracker tracker = sender.Send(message);
               sentMessages.Add(tracker);
               tracker.SettlementTask.Wait();
            }

            Assert.AreEqual(CREDIT, sentMessages.Count);

            sender.CloseAsync();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendMessagesWithLargerBytePayload()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithIncomingWindow(1)
                             .WithDeliveryCount(0)
                             .WithNextIncomingId(1)
                             .WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false)
                                 .Respond()
                                 .WithSettled(true).WithState().Accepted();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISender sender = connection.OpenSender("test-queue").OpenTask.Result;

            byte[] payload = new byte[1024];
            Array.Fill(payload, (byte)1);

            ITracker tracker = sender.Send(IMessage<byte[]>.Create(payload));

            peer.WaitForScriptToComplete();

            tracker.SettlementTask.Wait();

            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendBlockedForCreditFailsWhenLinkRemotelyClosed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteDetach().WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Link was deleted").AfterDelay(25).Queue();
            peer.ExpectDetach();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            IMessage<string> message = IMessage<string>.Create("Hello World");

            try
            {
               logger.LogInformation("About to send and block until the remote error occurs");
               sender.Send(message);
               Assert.Fail("Send should have timed out.");
            }
            catch (ClientResourceRemotelyClosedException)
            {
               // Expected send to throw indicating that the remote closed the link
               logger.LogInformation("Sender threw excepted exception when remote detached");
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendBlockedForCreditFailsWhenSessionRemotelyClosed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteEnd().WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Session was deleted").AfterDelay(25).Queue();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            IMessage<string> message = IMessage<string>.Create("Hello World");

            try
            {
               sender.Send(message);
               Assert.Fail("Send should have timed out.");
            }
            catch (ClientResourceRemotelyClosedException)
            {
               // Expected send to throw indicating that the remote closed the session
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendBlockedForCreditFailsWhenConnectionRemotelyClosed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteClose().WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Connection was deleted").AfterDelay(25).Queue();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            IMessage<string> message = IMessage<string>.Create("Hello World");

            try
            {
               sender.Send(message);
               Assert.Fail("Send should have failed when Connection remotely closed.");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               // Expected send to throw indicating that the remote closed the connection
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendBlockedForCreditFailsWhenConnectionDrops()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.DropAfterLastHandler(25);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            IMessage<string> message = IMessage<string>.Create("Hello World");

            try
            {
               sender.Send(message);
               Assert.Fail("Send should have timed out.");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               // Expected send to throw indicating that the remote closed unexpectedly
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendAfterConnectionDropsThrowsConnectionRemotelyClosedError()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            peer.DropAfterLastHandler(25);
            peer.Start();

            CountdownEvent dropped = new CountdownEvent(1);

            ConnectionOptions options = new ConnectionOptions();
            options.DisconnectedHandler = (connection, eventArg) =>
            {
               dropped.Signal();
            };

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test").OpenTask.Result;

            Assert.IsTrue(dropped.Wait(TimeSpan.FromSeconds(10)));

            IMessage<string> message = IMessage<string>.Create("Hello World");

            try
            {
               sender.Send(message);
               Assert.Fail("Send should fail with remotely closed error after remote drops");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               // Expected
            }

            try
            {
               sender.TrySend(message);
               Assert.Fail("trySend should fail with remotely closed error after remote drops");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               // Expected
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAwaitSettlementFutureFailedAfterConnectionDropped()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectTransfer();
            peer.DropAfterLastHandler();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test").OpenTask.Result;

            IMessage<string> message = IMessage<string>.Create("test-message");

            ITracker tracker = null;
            try
            {
               tracker = sender.Send(message);
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               Assert.Fail("Send not should fail with remotely closed error after remote drops");
            }

            // Connection should be dropped at this point and next call should test that after
            // the drop the future has been completed
            peer.WaitForScriptToComplete();

            try
            {
               tracker.SettlementTask.Wait();
               Assert.Fail("Wait for settlement should fail with remotely closed error after remote drops");
            }
            catch (Exception exe)
            {
               Assert.IsTrue(exe.InnerException is ClientConnectionRemotelyClosedException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAwaitSettlementFailedOnConnectionDropped()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectTransfer();
            peer.DropAfterLastHandler(30);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test");

            IMessage<string> message = IMessage<string>.Create("test-message");

            ITracker tracker = null;
            try
            {
               tracker = sender.Send(message);
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               Assert.Fail("Send should not fail with remotely closed error after remote drops");
            }

            // Most of the time this should await before connection drops testing that
            // the drop completes waiting callers.
            try
            {
               tracker.AwaitSettlement();
               Assert.Fail("Wait for settlement should fail with remotely closed error after remote drops");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestBlockedSendThrowsConnectionRemotelyClosedError()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            peer.DropAfterLastHandler(25);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test");

            IMessage<string> message = IMessage<string>.Create("test-message");

            try
            {
               sender.Send(message);
               Assert.Fail("Send should fail with remotely closed error after remote drops");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               // Expected
            }

            connection.CloseAsync().Wait();

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
            peer.ExpectBegin().WithNextOutgoingId(0).Respond();
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
            ISender sender = connection.OpenSender("test-queue");

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
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true).WithFrameSize(1024);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(1).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true).WithFrameSize(1024);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(2).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true).WithFrameSize(1024);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(3).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true).WithFrameSize(1024);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(4).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Accept();

            // Grant the credit to start meeting the above expectations
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(0).WithLinkCredit(10).Now();

            peer.WaitForScriptToComplete();

            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAutoFlushDuringWriteWithRollingIncomingWindowUpdates()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().WithNextOutgoingId(0).Respond();
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
            ISender sender = connection.OpenSender("test-queue");

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

            // Credit should will be refilling as transfers arrive vs being exhausted on each
            // incoming transfer and the send awaiting more credit.
            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(2).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(3).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithNextIncomingId(4).WithLinkCredit(10).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Accept();

            // Grant the credit to start meeting the above expectations
            peer.RemoteFlow().WithIncomingWindow(2).WithNextIncomingId(0).WithLinkCredit(10).Now();

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConcurrentSendOnlyBlocksForInitialSendInProgress()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
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
            ISender sender = connection.OpenSender("test-queue").OpenTask.Result;
            // Ensure that sender gets its flow before the sends are triggered.
            connection.OpenReceiver("test-queue").OpenTask.Wait();

            byte[] payload = new byte[1024];
            Array.Fill(payload, (byte)1);

            CountdownEvent sendsCompleted = new CountdownEvent(2);

            // One should block on the send waiting for the others send to finish
            // otherwise they should not care about concurrency of sends.

            bool sendFailed = false;
            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 1 is preparing to fire:");
                  ITracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
                  tracker.AwaitSettlement(TimeSpan.FromSeconds(10));
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 1 failed with error: {0}", e);
                  sendFailed = true;
               }

               sendsCompleted.Signal();
            });

            Task.Run(() =>
            {
               try
               {
                  logger.LogInformation("Test send 2 is preparing to fire:");
                  ITracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
                  tracker.AwaitSettlement(TimeSpan.FromSeconds(10));
               }
               catch (Exception e)
               {
                  logger.LogInformation("Test send 2 failed with error: {0}", e);
                  sendFailed = true;
               }

               sendsCompleted.Signal();
            });

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            Assert.IsTrue(sendsCompleted.Wait(TimeSpan.FromSeconds(20)));
            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConcurrentSendBlocksBehindSendWaitingForCredit()
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
            ISender sender = connection.OpenSender("test-queue").OpenTask.Result;

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
                  ITracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
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
                  ITracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
                  tracker.AwaitSettlement(TimeSpan.FromSeconds(50));
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

            Assert.IsTrue(send2Completed.Wait(TimeSpan.FromSeconds(15)));

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConcurrentSendWaitingOnSplitFramedSendToCompleteIsSentAfterCreditUpdated()
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
            ISender sender = connection.OpenSender("test-queue").OpenTask.Result;

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
                  ITracker tracker = sender.Send(IMessage<byte[]>.Create(payload));
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
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(0).WithNextIncomingId(2).WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(1).WithNextIncomingId(3).WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(true);
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(1).WithNextIncomingId(4).WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();

            // This is the initial flow that trigger the above expect and flow chain.
            peer.RemoteFlow().WithIncomingWindow(1).WithDeliveryCount(0).WithNextIncomingId(1).WithLinkCredit(1).Now();

            Assert.IsTrue(send2Completed.Wait(TimeSpan.FromSeconds(20)));

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            Assert.IsFalse(sendFailed);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateSenderWithDefaultSourceAndTargetOptions()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender()
                               .WithSource().WithAddress(Test.Driver.Matchers.Is.NotNullValue())
                                            .WithDistributionMode(Test.Driver.Matchers.Is.NullValue())
                                            .WithDefaultTimeout()
                                            .WithDurable(TerminusDurability.None)
                                            .WithExpiryPolicy(TerminusExpiryPolicy.LinkDetach)
                                            .WithDefaultOutcome(Test.Driver.Matchers.Is.NullValue())
                                            .WithCapabilities(Test.Driver.Matchers.Is.NullValue())
                                            .WithFilter(Test.Driver.Matchers.Is.NullValue())
                                            .WithOutcomes("amqp:accepted:list", "amqp:rejected:list", "amqp:released:list", "amqp:modified:list")
                                            .Also()
                               .WithTarget().WithAddress("test-queue")
                                            .WithCapabilities(Test.Driver.Matchers.Is.NullValue())
                                            .WithDurable(Test.Driver.Matchers.Is.NullValue())
                                            .WithExpiryPolicy(Test.Driver.Matchers.Is.NullValue())
                                            .WithDefaultTimeout()
                                            .WithDynamic(Test.Driver.Matchers.Matches.AnyOf(
                                               Test.Driver.Matchers.Is.NullValue(),
                                               Test.Driver.Matchers.Is.EqualTo(false)))
                                            .WithDynamicNodeProperties(Test.Driver.Matchers.Is.NullValue())
                               .And().Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test-queue").OpenTask.Result;

            sender.Close();
            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateSenderWithUserConfiguredSourceAndTargetOptions()
      {
         IDictionary<string, object> filtersToObject = new Dictionary<string, object>();
         filtersToObject.Add("x-opt-filter", "a = b");

         IDictionary<string, object> filters = new Dictionary<string, object>();
         filters.Add("x-opt-filter", "a = b");

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender()
                               .WithSource().WithAddress(Test.Driver.Matchers.Is.NotNullValue())
                                            .WithDistributionMode("copy")
                                            .WithTimeout(128)
                                            .WithDurable(TerminusDurability.UnsettledState)
                                            .WithExpiryPolicy(TerminusExpiryPolicy.ConnectionClose)
                                            .WithDefaultOutcome(new Released())
                                            .WithCapabilities("QUEUE")
                                            .WithFilter(filtersToObject)
                                            .WithOutcomes("amqp:accepted:list", "amqp:rejected:list")
                                            .Also()
                               .WithTarget().WithAddress("test-queue")
                                            .WithCapabilities("QUEUE")
                                            .WithDurable(TerminusDurability.Configuration)
                                            .WithExpiryPolicy(TerminusExpiryPolicy.SessionEnd)
                                            .WithTimeout(42)
                                            .WithDynamic(Test.Driver.Matchers.Matches.AnyOf(
                                                Test.Driver.Matchers.Is.NullValue(),
                                                Test.Driver.Matchers.Is.EqualTo(false)))
                                            .WithDynamicNodeProperties(Test.Driver.Matchers.Is.NullValue())
                               .And().Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            SenderOptions senderOptions = new SenderOptions();

            senderOptions.SourceOptions.Capabilities = new string[] { "QUEUE" };
            senderOptions.SourceOptions.DistributionMode = DistributionMode.Copy;
            senderOptions.SourceOptions.Timeout = 128;
            senderOptions.SourceOptions.DurabilityMode = DurabilityMode.UnsettledState;
            senderOptions.SourceOptions.ExpiryPolicy = ExpiryPolicy.ConnectionClose;
            senderOptions.SourceOptions.DefaultOutcome = ClientReleased.Instance;
            senderOptions.SourceOptions.Filters = filters;
            senderOptions.SourceOptions.Outcomes =
               new DeliveryStateType[] { DeliveryStateType.Accepted, DeliveryStateType.Rejected };

            senderOptions.TargetOptions.Capabilities = new string[] { "QUEUE" };
            senderOptions.TargetOptions.DurabilityMode = DurabilityMode.Configuration;
            senderOptions.TargetOptions.ExpiryPolicy = ExpiryPolicy.SessionClose;
            senderOptions.TargetOptions.Timeout = 42;

            ISender sender = session.OpenSender("test-queue", senderOptions).OpenTask.Result;

            sender.Close();
            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestWaitForAcceptedReturnsOnRemoteAcceptance()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Accepted();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISender sender = connection.OpenSender("test-queue").OpenTask.Result;
            ITracker tracker = sender.Send(IMessage<string>.Create("Hello World"));

            tracker.AwaitAccepted();

            Assert.IsTrue(tracker.RemoteSettled);
            Assert.IsTrue(tracker.RemoteState.IsAccepted);

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestWaitForAcceptanceFailsIfRemoteSendsRejected()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true).WithState().Rejected();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISender sender = connection.OpenSender("test-queue").OpenTask.Result;
            ITracker tracker = sender.Send(IMessage<string>.Create("Hello World"));

            try
            {
               tracker.AwaitAccepted(TimeSpan.FromSeconds(10));
               Assert.Fail("Should not succeed since remote sent something other than Accepted");
            }
            catch (ClientDeliveryStateException)
            {
               // Expected
            }

            Assert.IsTrue(tracker.RemoteSettled);
            Assert.IsFalse(tracker.RemoteState.IsAccepted);

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestWaitForAcceptanceFailsIfRemoteSendsNoDisposition()
      {
         DoTestWaitForAcceptanceFailsIfRemoteSendsNoDisposition(false);
      }

      [Test]
      public void TestWaitForAcceptanceFailsIfRemoteSendsNoDispositionAsyncSend()
      {
         DoTestWaitForAcceptanceFailsIfRemoteSendsNoDisposition(true);
      }

      private void DoTestWaitForAcceptanceFailsIfRemoteSendsNoDisposition(bool sendAsync)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithLinkCredit(2).Queue();
            peer.ExpectTransfer().WithNonNullPayload().WithMore(false).Respond().WithSettled(true);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISender sender = connection.OpenSender("test-queue").OpenTask.Result;

            ITracker tracker;

            if (sendAsync)
            {
               tracker = sender.Send(IMessage<string>.Create("Hello World"));
            }
            else
            {
               tracker = sender.SendAsync(IMessage<string>.Create("Hello World")).GetAwaiter().GetResult();
            }

            try
            {
               tracker.AwaitAccepted(TimeSpan.FromSeconds(10));
               Assert.Fail("Should not succeed since remote sent something other than Accepted");
            }
            catch (ClientDeliveryStateException)
            {
               // Expected
            }

            Assert.IsTrue(tracker.RemoteSettled);
            Assert.IsNull(tracker.RemoteState);

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            sender.Close();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSenderLinkNameOptionAppliedWhenSet()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithName("custom-link-name").Respond();
            peer.ExpectDetach().Respond();
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
               LinkName = "custom-link-name"
            };
            ISender sender = session.OpenSender("test-queue", senderOptions);

            sender.OpenTask.Wait();
            sender.Close();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestInspectRemoteSourceMatchesValuesSent()
      {
         IDictionary<string, object> remoteFilters = new Dictionary<string, object>();
         remoteFilters.Add("filter-1", "value1");

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond().WithSource().WithOutcomes("Accepted", "Released")
                                                                 .WithCapabilities("Queue")
                                                                 .WithDistributionMode("COPY")
                                                                 .WithDynamic(false)
                                                                 .WithExpiryPolicy(TerminusExpiryPolicy.SessionEnd)
                                                                 .WithDurability(TerminusDurability.UnsettledState)
                                                                 .WithDefaultOutcome(new Test.Driver.Codec.Messaging.Released())
                                                                 .WithTimeout(int.MaxValue)
                                                                 .WithFilterMap(remoteFilters)
                                                                 .WithAddress("test-queue");
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISender sender = connection.OpenSender("test-queue");

            ISource remoteSource = sender.Source;

            Assert.IsTrue(remoteSource.Outcomes.Contains(DeliveryStateType.Accepted));
            Assert.IsTrue(remoteSource.Capabilities.Contains("Queue"));
            Assert.AreEqual("test-queue", remoteSource.Address);
            Assert.IsFalse(remoteSource.Dynamic);
            Assert.IsNull(remoteSource.DynamicNodeProperties);
            Assert.AreEqual(DistributionMode.Copy, remoteSource.DistributionMode);
            Assert.AreEqual(ClientReleased.Instance, remoteSource.DefaultOutcome);
            Assert.AreEqual(int.MaxValue, remoteSource.Timeout);
            Assert.AreEqual(DurabilityMode.UnsettledState, remoteSource.DurabilityMode);
            Assert.AreEqual(ExpiryPolicy.SessionClose, remoteSource.ExpiryPolicy);
            Assert.AreEqual(remoteFilters, remoteSource.Filters);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestInspectRemoteTargetMatchesValuesSent()
      {
         IDictionary<string, object> remoteFilters = new Dictionary<string, object>();
         remoteFilters.Add("filter-1", "value1");

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond().WithTarget().WithCapabilities("Queue")
                                                                 .WithDynamic(false)
                                                                 .WithExpiryPolicy(TerminusExpiryPolicy.SessionEnd)
                                                                 .WithDurability(TerminusDurability.UnsettledState)
                                                                 .WithTimeout(int.MaxValue)
                                                                 .WithAddress("test-queue");
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISender sender = connection.OpenSender("test-queue");

            ITarget remoteTarget = sender.Target;

            Assert.IsTrue(remoteTarget.Capabilities.Contains("Queue"));
            Assert.AreEqual("test-queue", remoteTarget.Address);
            Assert.IsFalse(remoteTarget.Dynamic);
            Assert.AreEqual(int.MaxValue, remoteTarget.Timeout);
            Assert.AreEqual(DurabilityMode.UnsettledState, remoteTarget.DurabilityMode);
            Assert.AreEqual(ExpiryPolicy.SessionClose, remoteTarget.ExpiryPolicy);

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
            ISession session = connection.OpenSession().OpenTask.Result;

            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtLeastOnce,
               AutoSettle = true,
               DeliveryTagGeneratorSupplier = CustomTagGenerator
            };
            ISender sender = session.OpenSender("test-tags", options).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 1, 1, 1 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 2, 2, 2 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 3, 3, 3 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");
            ITracker tracker1 = sender.Send(message);
            ITracker tracker2 = sender.Send(message);
            ITracker tracker3 = sender.Send(message);

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
            ISession session = connection.OpenSession().OpenTask.Result;
            SenderOptions options = new SenderOptions()
            {
               DeliveryMode = DeliveryMode.AtLeastOnce,
               AutoSettle = true,
               DeliveryTagGeneratorSupplier = CustomNullTagGenerator
            };

            try
            {
               _ = session.OpenSender("test-tags", options).OpenTask.Result;
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