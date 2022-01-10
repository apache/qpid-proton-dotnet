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
using Apache.Qpid.Proton.Test.Driver.Matchers;
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReceiverTest : ClientBaseTestFixture
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
            peer.ExpectAttach().OfReceiver().WithSource().WithDistributionMode(Test.Driver.Matchers.Is.NullValue()).And().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
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

            IReceiver receiver = session.OpenReceiver("test-queue");
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            Assert.AreSame(container, receiver.Client);
            Assert.AreSame(connection, receiver.Connection);
            Assert.AreSame(session, receiver.Session);

            if (close)
            {
               receiver.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            }
            else
            {
               receiver.DetachAsync().Wait(TimeSpan.FromSeconds(10));
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateReceiverAndCloseSync()
      {
         DoTestCreateReceiverAndCloseOrDetachSync(true);
      }

      [Test]
      public void TestCreateReceiverAndDetachSync()
      {
         DoTestCreateReceiverAndCloseOrDetachSync(false);
      }

      private void DoTestCreateReceiverAndCloseOrDetachSync(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
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

            IReceiver receiver = session.OpenReceiver("test-queue");
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            if (close)
            {
               receiver.Close();
            }
            else
            {
               receiver.Detach();
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

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

            IReceiver receiver = session.OpenReceiver("test-queue");
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            if (close)
            {
               receiver.Close(IErrorCondition.Create("amqp-resource-deleted", "an error message", null));
            }
            else
            {
               receiver.Detach(IErrorCondition.Create("amqp-resource-deleted", "an error message", null));
            }

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverOpenRejectedByRemote()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().Respond().WithNullSource();
            peer.ExpectFlow();
            peer.RemoteDetach().WithErrorCondition(AmqpError.UNAUTHORIZED_ACCESS.ToString(), "Cannot read from this address").Queue();
            peer.ExpectDetach();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenReceiver("test-queue");

            try
            {
               receiver.OpenTask.Wait();
               Assert.Fail("Open of receiver should fail due to remote indicating pending close.");
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
            receiver.CloseAsync().Wait();

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete(TimeSpan.FromSeconds(10));
         }
      }

      [Test]
      public void TestOpenReceiverTimesOutWhenNoAttachResponseReceivedTimeout()
      {
         DoTestOpenReceiverTimesOutWhenNoAttachResponseReceived(true);
      }

      [Test]
      public void TestOpenReceiverTimesOutWhenNoAttachResponseReceivedNoTimeout()
      {
         DoTestOpenReceiverTimesOutWhenNoAttachResponseReceived(false);
      }

      private void DoTestOpenReceiverTimesOutWhenNoAttachResponseReceived(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.ExpectFlow();
            peer.ExpectDetach();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ReceiverOptions options = new ReceiverOptions()
            {
               OpenTimeout = 10
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options);

            try
            {
               if (timeout)
               {
                  receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  receiver.OpenTask.Wait();
               }

               Assert.Fail("Should not complete the open future without an error");
            }
            catch (Exception exe)
            {
               Exception cause = exe.InnerException;
               Assert.IsTrue(cause is ClientOperationTimedOutException);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenReceiverWaitWithTimeoutFailsWhenConnectionDrops()
      {
         DoTestOpenReceiverWaitFailsWhenConnectionDrops(true);
      }

      [Test]
      public void TestOpenReceiverWaitWithNoTimeoutFailsWhenConnectionDrops()
      {
         DoTestOpenReceiverWaitFailsWhenConnectionDrops(false);
      }

      private void DoTestOpenReceiverWaitFailsWhenConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.ExpectFlow();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenReceiver("test-queue");

            try
            {
               if (timeout)
               {
                  receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  receiver.OpenTask.Wait();
               }

               Assert.Fail("Should not complete the open future without an error");
            }
            catch (Exception exe)
            {
               Exception cause = exe.InnerException;
               Assert.IsTrue(cause is ClientIOException);
            }

            connection.CloseAsync().GetAwaiter().GetResult();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseReceiverTimesOutWhenNoCloseResponseReceivedTimeout()
      {
         DoTestCloseOrDetachReceiverTimesOutWhenNoCloseResponseReceived(true, true);
      }

      [Test]
      public void TestCloseReceiverTimesOutWhenNoCloseResponseReceivedNoTimeout()
      {
         DoTestCloseOrDetachReceiverTimesOutWhenNoCloseResponseReceived(true, false);
      }

      [Test]
      public void TestDetachReceiverTimesOutWhenNoCloseResponseReceivedTimeout()
      {
         DoTestCloseOrDetachReceiverTimesOutWhenNoCloseResponseReceived(false, true);
      }

      [Test]
      public void TestDetachReceiverTimesOutWhenNoCloseResponseReceivedNoTimeout()
      {
         DoTestCloseOrDetachReceiverTimesOutWhenNoCloseResponseReceived(false, false);
      }

      private void DoTestCloseOrDetachReceiverTimesOutWhenNoCloseResponseReceived(bool close, bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectDetach();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               CloseTimeout = 5
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenReceiver("test-queue");
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            try
            {
               if (close)
               {
                  if (timeout)
                  {
                     receiver.CloseAsync().Wait(TimeSpan.FromSeconds(10));
                  }
                  else
                  {
                     receiver.CloseAsync().Wait();
                  }
               }
               else
               {
                  if (timeout)
                  {
                     receiver.DetachAsync().Wait(TimeSpan.FromSeconds(10));
                  }
                  else
                  {
                     receiver.DetachAsync().Wait();
                  }
               }

               Assert.Fail("Should not complete the close or detach future without an error");
            }
            catch (Exception exe)
            {
               Exception cause = exe.InnerException;
               Assert.IsTrue(cause is ClientOperationTimedOutException);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverDrainAllOutstanding()
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
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 0
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();

            // Add some credit, verify not draining
            uint credit = 7;
            peer.ExpectFlow().WithDrain(Matches.AnyOf(Test.Driver.Matchers.Is.EqualTo(false),
                                                      Test.Driver.Matchers.Is.NullValue()))
                             .WithLinkCredit(credit).WithDeliveryCount(0);

            receiver.AddCredit(credit);

            peer.WaitForScriptToComplete();

            // Drain all the credit
            peer.ExpectFlow().WithDrain(true).WithLinkCredit(credit).WithDeliveryCount(0)
                             .Respond()
                             .WithDrain(true).WithLinkCredit(0).WithDeliveryCount(credit);

            Task<IReceiver> draining = receiver.Drain();
            draining.Wait(TimeSpan.FromSeconds(5));

            // Close things down
            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete(TimeSpan.FromSeconds(10));
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
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 0
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Task<IReceiver> draining = receiver.Drain();
            draining.Wait(TimeSpan.FromSeconds(5));

            // Close things down
            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete(TimeSpan.FromSeconds(10));
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
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            receiver.Drain();

            try
            {
               receiver.Drain().Wait();
               Assert.Fail("Drain call should fail since already draining.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call", cliEx);
               Assert.IsTrue(cliEx.InnerException is ClientIllegalStateException);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAddCreditFailsWhileDrainPending()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond().WithInitialDeliveryCount(20);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 0
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();

            // Add some credit, verify not draining
            uint credit = 7;
            peer.ExpectFlow().WithDrain(Matches.AnyOf(Test.Driver.Matchers.Is.EqualTo(false),
                                                      Test.Driver.Matchers.Is.NullValue()))
                             .WithLinkCredit(credit);

            // Ensure we get the attach response with the initial delivery count.
            receiver.OpenTask.Result.AddCredit(credit);

            peer.WaitForScriptToComplete();

            // Drain all the credit
            peer.ExpectFlow().WithDrain(true).WithLinkCredit(credit).WithDeliveryCount(20);
            peer.ExpectClose().Respond();

            Task<IReceiver> draining = receiver.Drain();
            Assert.IsFalse(draining.IsCompleted);

            try
            {
               receiver.AddCredit(1);
               Assert.Fail("Should not allow add credit when drain is pending");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAddCreditFailsWhenCreditWindowEnabled()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 10 // Explicitly set a credit window to unsure behavior is consistent.
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            try
            {
               receiver.AddCredit(1);
               Assert.Fail("Should not allow add credit when credit window configured");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}