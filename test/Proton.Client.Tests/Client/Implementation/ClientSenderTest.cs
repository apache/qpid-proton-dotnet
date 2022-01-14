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

      [Ignore("Sends not yet working from client sender")]
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
   }
}