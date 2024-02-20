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
using Apache.Qpid.Proton.Utilities;
using Apache.Qpid.Proton.Types.Messaging;
using System.IO;
using Apache.Qpid.Proton.Types;

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

            Task<IReceiver> draining = receiver.DrainAsync();
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

            Task<IReceiver> draining = receiver.DrainAsync();
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

            receiver.DrainAsync();

            try
            {
               receiver.DrainAsync().Wait();
               Assert.Fail("Drain call should fail since already draining.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
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

            Task<IReceiver> draining = receiver.DrainAsync();
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

      [Test]
      public void TestCreateDynamicReceiver()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithDynamic(true).WithAddress((string)null)
                               .And().Respond()
                               .WithSource().WithDynamic(true).WithAddress("test-dynamic-node");
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenDynamicReceiver().OpenTask.Result;

            Assert.IsNotNull(receiver.Address, "Remote should have assigned the address for the dynamic receiver");
            Assert.AreEqual("test-dynamic-node", receiver.Address);

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateDynamicReceiverWthNodeProperties()
      {
         IDictionary<string, object> nodeProperties = new Dictionary<string, object>();
         nodeProperties.Add("test-property-1", "one");
         nodeProperties.Add("test-property-2", "two");
         nodeProperties.Add("test-property-3", "three");

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource()
                                   .WithDynamic(true)
                                   .WithAddress((String)null)
                                   .WithDynamicNodeProperties(nodeProperties)
                               .And().Respond()
                               .WithSource()
                                   .WithDynamic(true)
                                   .WithAddress("test-dynamic-node")
                                   .WithDynamicNodeProperties(nodeProperties);
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenDynamicReceiver(null, nodeProperties).OpenTask.Result;

            Assert.IsNotNull(receiver.Address, "Remote should have assigned the address for the dynamic receiver");
            Assert.AreEqual("test-dynamic-node", receiver.Address);

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateDynamicReceiverWithNoCreditWindow()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithDynamic(true).WithAddress((String)null)
                               .And().Respond()
                               .WithSource().WithDynamic(true).WithAddress("test-dynamic-node");
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 0
            };
            IReceiver receiver = session.OpenDynamicReceiver(receiverOptions).OpenTask.Result;

            // Perform another round trip operation to ensure we see that no flow frame was
            // sent by the receiver
            session.OpenSender("test");

            Assert.IsNotNull(receiver.Address, "Remote should have assigned the address for the dynamic receiver");
            Assert.AreEqual("test-dynamic-node", receiver.Address);

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDynamicReceiverAddressWaitsForRemoteAttach()
      {
         tryReadDynamicReceiverAddress(true);
      }

      [Test]
      public void TestDynamicReceiverAddressFailsAfterOpenTimeout()
      {
         tryReadDynamicReceiverAddress(false);
      }

      private void tryReadDynamicReceiverAddress(bool attachResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithDynamic(true).WithAddress((String)null);
            peer.ExpectFlow();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               OpenTimeout = 100
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenDynamicReceiver();

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.RespondToLastAttach().WithSource().WithAddress("test-dynamic-node").And().Later(10);
            }
            else
            {
               peer.ExpectDetach();
            }

            if (attachResponse)
            {
               Assert.IsNotNull(receiver.Address, "Remote should have assigned the address for the dynamic receiver");
               Assert.AreEqual("test-dynamic-node", receiver.Address);
            }
            else
            {
               try
               {
                  _ = receiver.Address;
                  Assert.Fail("Should failed to get address due to no attach response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from address call: {0}", ex.Message);
               }
            }

            try
            {
               receiver.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateReceiverWithQoSOfAtMostOnce()
      {
         DoTestCreateReceiverWithConfiguredQoS(DeliveryMode.AtMostOnce);
      }

      [Test]
      public void TestCreateReceiverWithQoSOfAtLeastOnce()
      {
         DoTestCreateReceiverWithConfiguredQoS(DeliveryMode.AtLeastOnce);
      }

      private void DoTestCreateReceiverWithConfiguredQoS(DeliveryMode qos)
      {
         byte sndMode = (byte)(qos == DeliveryMode.AtMostOnce ? SenderSettleMode.Settled : SenderSettleMode.Unsettled);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSndSettleMode(sndMode)
                               .WithRcvSettleMode((byte)ReceiverSettleMode.First)
                               .Respond()
                               .WithSndSettleMode(sndMode)
                               .WithRcvSettleMode((byte?)ReceiverSettleMode.First);
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
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
               DeliveryMode = qos
            };
            IReceiver receiver = session.OpenReceiver("test-qos", options).OpenTask.Result;

            Assert.AreEqual("test-qos", receiver.Address);

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetSourceWaitsForRemoteAttach()
      {
         TryReadReceiverSource(true);
      }

      [Test]
      public void TestReceiverGetSourceFailsAfterOpenTimeout()
      {
         TryReadReceiverSource(false);
      }

      private void TryReadReceiverSource(bool attachResponse)
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
            ConnectionOptions options = new ConnectionOptions()
            {
               OpenTimeout = attachResponse ? 5000 : 100
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;

            IReceiver receiver = session.OpenReceiver("test-receiver");

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.RespondToLastAttach().Later(10);
            }
            else
            {
               peer.ExpectDetach();
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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               receiver.CloseAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetTargetWaitsForRemoteAttach()
      {
         TryReadReceiverTarget(true);
      }

      [Test]
      public void TestReceiverGetTargetFailsAfterOpenTimeout()
      {
         TryReadReceiverTarget(false);
      }

      private void TryReadReceiverTarget(bool attachResponse)
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
            ConnectionOptions options = new ConnectionOptions()
            {
               OpenTimeout = attachResponse ? 5000 : 150
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenReceiver("test-receiver");

            peer.WaitForScriptToComplete();

            if (attachResponse)
            {
               peer.ExpectDetach().Respond();
               peer.RespondToLastAttach().Later(10);
            }
            else
            {
               peer.ExpectDetach();
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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               receiver.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait();

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
            IConnection connection = container.Connect(remoteAddress, remotePort).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            ReceiverOptions options = new ReceiverOptions()
            {
               OpenTimeout = attachResponse ? 5000 : 150
            };
            IReceiver receiver = session.OpenReceiver("test-receiver", options);

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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               receiver.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait();

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
            ConnectionOptions options = new ConnectionOptions()
            {
               OpenTimeout = attachResponse ? 5000 : 150
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenReceiver("test-receiver");

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
               Assert.IsNotNull(receiver.OfferedCapabilities, "Remote should have responded with a remote offered Capabilities value");
               Assert.AreEqual(1, receiver.OfferedCapabilities.Count);
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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               receiver.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverGetRemoteDesiredCapabilitiesWaitsForRemoteAttach()
      {
         TryReadReceiverRemoteDesiredCapabilities(true);
      }

      [Test]
      public void TestReceiverGetRemoteDesiredCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadReceiverRemoteDesiredCapabilities(false);
      }

      private void TryReadReceiverRemoteDesiredCapabilities(bool attachResponse)
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
            ConnectionOptions options = new ConnectionOptions()
            {
               OpenTimeout = attachResponse ? 5000 : 150
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options).OpenTask.Result;
            ISession session = connection.OpenSession().OpenTask.Result;
            IReceiver receiver = session.OpenReceiver("test-receiver");

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
               Assert.IsNotNull(receiver.DesiredCapabilities, "Remote should have responded with a remote desired Capabilities value");
               Assert.AreEqual(1, receiver.DesiredCapabilities.Count);
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
                  logger.LogDebug("Caught expected exception from blocking call: {0}", ex.Message);
               }
            }

            try
            {
               receiver.CloseAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call: {0}", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and detach sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestBlockingReceiveCancelledWhenReceiverClosed()
      {
         DoTestBlockingReceiveCancelledWhenReceiverClosedOrDetached(true);
      }

      [Test]
      public void TestBlockingReceiveCancelledWhenReceiverDetached()
      {
         DoTestBlockingReceiveCancelledWhenReceiverClosedOrDetached(false);
      }

      public void DoTestBlockingReceiveCancelledWhenReceiverClosedOrDetached(bool close)
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
            peer.ExpectFlow().WithLinkCredit(10);
            peer.Execute(() =>
            {
               if (close)
               {
                  receiver.CloseAsync();
               }
               else
               {
                  receiver.DetachAsync();
               }
            }).Queue();
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.ExpectClose().Respond();

            receiver.AddCredit(10);

            try
            {
               receiver.Receive();
               Assert.Fail("Should throw to indicate that receiver was closed");
            }
            catch (ClientException)
            {
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestBlockingReceiveCancelledWhenReceiverRemotelyClosed()
      {
         DoTestBlockingReceiveCancelledWhenReceiverRemotelyClosedOrDetached(true);
      }

      [Test]
      public void TestBlockingReceiveCancelledWhenReceiverRemotelyDetached()
      {
         DoTestBlockingReceiveCancelledWhenReceiverRemotelyClosedOrDetached(false);
      }

      public void DoTestBlockingReceiveCancelledWhenReceiverRemotelyClosedOrDetached(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteDetach().WithClosed(close)
                               .WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Address was manually deleted")
                               .AfterDelay(10).Queue();
            peer.ExpectDetach().WithClosed(close);
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.Receive();
               Assert.Fail("Client should throw to indicate remote closed the receiver forcibly.");
            }
            catch (ClientIllegalStateException)
            {
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseReceiverWithErrorCondition()
      {
         DoTestCloseOrDetachWithErrorCondition(true);
      }

      [Test]
      public void TestDetachReceiverWithErrorCondition()
      {
         DoTestCloseOrDetachWithErrorCondition(false);
      }

      public void DoTestCloseOrDetachWithErrorCondition(bool close)
      {
         string condition = "amqp:link:detach-forced";
         string description = "something bad happened.";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().WithClosed(close).WithError(condition, description).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            if (close)
            {
               receiver.CloseAsync(IErrorCondition.Create(condition, description, null));
            }
            else
            {
               receiver.DetachAsync(IErrorCondition.Create(condition, description, null));
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenReceiverWithLinCapabilities()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithCapabilities("queue").And()
                               .Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ReceiverOptions receiverOptions = new ReceiverOptions();
            receiverOptions.SourceOptions.Capabilities = new string[] { "queue" };
            IReceiver receiver = session.OpenReceiver("test-queue", receiverOptions);

            receiver.OpenTask.Wait();

            receiver.Close();

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveMessageInSplitTransferFrames()
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
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

            byte[] slice1 = Statics.CopyOfRange(payload, 0, 2);
            byte[] slice2 = Statics.CopyOfRange(payload, 2, 4);
            byte[] slice3 = Statics.CopyOfRange(payload, 4, payload.Length);

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(slice1).Now();

            Assert.IsNull(receiver.TryReceive());

            peer.RemoteTransfer().WithHandle(0)
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(slice2).Now();

            Assert.IsNull(receiver.TryReceive());

            peer.RemoteTransfer().WithHandle(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(slice3).Now();

            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            IMessage<object> received = delivery.Message();
            Assert.IsNotNull(received);
            Assert.IsTrue(received.Body is String);
            String value = (String)received.Body;
            Assert.AreEqual("Hello World", value);

            delivery.Accept();
            receiver.CloseAsync();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverHandlesAbortedSplitFrameTransfer()
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
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();

            Assert.IsNull(receiver.Receive(TimeSpan.FromMilliseconds(20)));

            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithMore(false)
                                 .WithAborted(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();

            Assert.IsNull(receiver.Receive(TimeSpan.FromMilliseconds(20)));

            receiver.CloseAsync();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverAddCreditOnAbortedTransferWhenNeeded()
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
            ISession session = connection.OpenSession();
            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 1
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();

            Assert.IsNull(receiver.TryReceive());

            peer.ExpectFlow();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(1)
                                 .WithMessageFormat(0)
                                 .WithMore(false)
                                 .WithPayload(payload).Queue();
            peer.ExpectDisposition().WithSettled(true).WithState().Accepted();
            peer.ExpectFlow();

            // // Send aborted transfer to complete first transfer and allow next to commence.
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithAborted(true)
                                 .WithMessageFormat(0)
                                 .Now();

            IDelivery delivery = receiver.Receive();
            Assert.IsNotNull(delivery);
            IMessage<object> received = delivery.Message();
            Assert.IsNotNull(received);
            Assert.IsTrue(received.Body is string);
            string value = (string)received.Body;
            Assert.AreEqual("Hello World", value);

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            receiver.CloseAsync();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverHandlesAbortedSplitFrameTransferAndReplenishesCredit()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
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
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 1
            };
            IReceiver receiver = session.OpenReceiver("test-queue", options).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Assert.IsNull(receiver.Receive(TimeSpan.FromMilliseconds(15)));

            // Credit window is one and next transfer signals aborted so receiver should
            // top-up the credit window to allow more transfers to arrive.
            peer.ExpectFlow().WithLinkCredit(1);

            // Abort the delivery which should result in a credit top-up.
            peer.RemoteTransfer().WithHandle(0)
                                 .WithMore(false)
                                 .WithAborted(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();

            Assert.IsNull(receiver.Receive(TimeSpan.FromMilliseconds(15)));

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            receiver.CloseAsync();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveCallFailsWhenReceiverPreviouslyClosed()
      {
         DoTestReceiveCallFailsWhenReceiverDetachedOrClosed(true);
      }

      [Test]
      public void TestReceiveCallFailsWhenReceiverPreviouslyDetached()
      {
         DoTestReceiveCallFailsWhenReceiverDetachedOrClosed(false);
      }

      private void DoTestReceiveCallFailsWhenReceiverDetachedOrClosed(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            if (close)
            {
               receiver.CloseAsync();
            }
            else
            {
               receiver.DetachAsync();
            }

            peer.WaitForScriptToComplete();

            try
            {
               receiver.Receive();
               Assert.Fail("Receive call should fail when link closed or detached.");
            }
            catch (ClientIllegalStateException cliEx)
            {
               logger.LogDebug("Receiver threw error on receive call: {0}", cliEx.Message);
            }

            peer.ExpectClose().Respond();

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveBlockedForMessageFailsWhenConnectionRemotelyClosed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteClose().WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Connection was deleted")
                              .AfterDelay(25).Queue();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions();
            options.TransportOptions.TraceBytes = false;
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.Receive();
               Assert.Fail("Receive should have failed when Connection remotely closed.");
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
      public void TestTimedReceiveBlockedForMessageFailsWhenConnectionRemotelyClosed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteClose().WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Connection was deleted")
                              .AfterDelay(50).Queue();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.Receive(TimeSpan.FromSeconds(10));
               Assert.Fail("Receive should have failed when Connection remotely closed.");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
               // Expected send to throw indicating that the remote closed the connection
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveTimedCallFailsWhenReceiverClosed()
      {
         DoTestReceiveTimedCallFailsWhenReceiverDetachedOrClosed(true);
      }

      [Test]
      public void TestReceiveTimedCallFailsWhenReceiverDetached()
      {
         DoTestReceiveTimedCallFailsWhenReceiverDetachedOrClosed(false);
      }

      private void DoTestReceiveTimedCallFailsWhenReceiverDetachedOrClosed(bool close)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            if (close)
            {
               receiver.CloseAsync();
            }
            else
            {
               receiver.DetachAsync();
            }

            peer.WaitForScriptToComplete();

            try
            {
               receiver.Receive(TimeSpan.FromSeconds(10));
               Assert.Fail("Receive call should fail when link closed or detached.");
            }
            catch (ClientIllegalStateException cliEx)
            {
               logger.LogDebug("Receiver threw error on receive call: {0}", cliEx.Message);
            }

            peer.ExpectClose().Respond();

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenReceiverClosed()
      {
         DoTestDrainFutureSignalsFailureWhenReceiverClosedOrDetached(true);
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenReceiverDetached()
      {
         DoTestDrainFutureSignalsFailureWhenReceiverClosedOrDetached(false);
      }

      private void DoTestDrainFutureSignalsFailureWhenReceiverClosedOrDetached(bool close)
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
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDrain(true).WithLinkCredit(10);
            peer.Execute(() =>
            {
               if (close)
               {
                  receiver.CloseAsync();
               }
               else
               {
                  receiver.DetachAsync();
               }
            }).Queue();
            peer.ExpectDetach().WithClosed(close).Respond();
            peer.ExpectClose().Respond();

            try
            {
               receiver.DrainAsync().Wait(TimeSpan.FromSeconds(10));
               Assert.Fail("Drain call should fail when link closed or detached.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
               Assert.IsTrue(cliEx.InnerException is ClientException);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenReceiverRemotelyClosed()
      {
         DoTestDrainFutureSignalsFailureWhenReceiverRemotelyClosedOrDetached(true);
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenReceiverRemotelyDetached()
      {
         DoTestDrainFutureSignalsFailureWhenReceiverRemotelyClosedOrDetached(false);
      }

      private void DoTestDrainFutureSignalsFailureWhenReceiverRemotelyClosedOrDetached(bool close)
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
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectFlow().WithDrain(true).WithLinkCredit(10);
            peer.RemoteDetach().WithClosed(close)
                               .WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Address was manually deleted").Queue();
            peer.ExpectDetach().WithClosed(close);
            peer.ExpectClose().Respond();

            try
            {
               receiver.DrainAsync().Wait(TimeSpan.FromSeconds(10));
               Assert.Fail("Drain call should fail when link closed or detached.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
               Assert.IsTrue(cliEx.InnerException is ClientException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenSessionRemotelyClosed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectFlow().WithDrain(true);
            peer.RemoteEnd().WithErrorCondition(AmqpError.RESOURCE_DELETED.ToString(), "Session was closed")
                            .AfterDelay(5).Queue();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.DrainAsync().Wait(TimeSpan.FromSeconds(10));
               Assert.Fail("Drain call should fail when session closed by remote.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
               Assert.IsTrue(cliEx.InnerException is ClientException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenConnectionDrops()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow();
            peer.ExpectFlow().WithDrain(true);
            peer.DropAfterLastHandler();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.DrainAsync().Wait(TimeSpan.FromSeconds(10));
               Assert.Fail("Drain call should fail when the connection drops.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
               Assert.IsTrue(cliEx.InnerException is ClientException);
            }

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
            ISession session = connection.OpenSession();
            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               DrainTimeout = 15
            };
            IReceiver receiver = session.OpenReceiver("test-queue", receiverOptions).OpenTask.Result;

            try
            {
               receiver.DrainAsync().Wait();
               Assert.Fail("Drain call should fail timeout exceeded.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
               Assert.IsTrue(cliEx.InnerException is ClientOperationTimedOutException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDrainFutureSignalsFailureWhenSessionDrainTimeoutExceeded()
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
            SessionOptions sessionOptions = new SessionOptions()
            {
               DrainTimeout = 15
            };
            ISession session = connection.OpenSession(sessionOptions);
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.DrainAsync().Wait();
               Assert.Fail("Drain call should fail timeout exceeded.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
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
               DrainTimeout = 15
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connectionOptions);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.DrainAsync().Wait();
               Assert.Fail("Drain call should fail timeout exceeded.");
            }
            catch (Exception cliEx)
            {
               logger.LogDebug("Receiver threw error on drain call: {0}", cliEx.Message);
               Assert.IsTrue(cliEx.InnerException is ClientOperationTimedOutException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestBlockedReceiveThrowsConnectionRemotelyClosedError()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().WithSource().WithAddress("test-queue").And().Respond();
            peer.ExpectFlow();
            peer.DropAfterLastHandler(25);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            try
            {
               receiver.Receive();
               Assert.Fail("Receive should fail with remotely closed error after remote drops");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDeliveryRefusesRawStreamAfterMessage()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithSettled(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(10));
            Assert.IsNotNull(delivery);

            IMessage<object> message = delivery.Message();
            Assert.IsNotNull(message);

            try
            {
               _ = delivery.RawInputStream;
               Assert.Fail("Should not be able to use the input stream once message is requested");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDeliveryRefusesRawStreamAfterAnnotations()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithSettled(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(5));
            Assert.IsNotNull(delivery);
            Assert.IsNull(delivery.Annotations);

            try
            {
               _ = delivery.RawInputStream;
               Assert.Fail("Should not be able to use the input stream once message is requested");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestDeliveryRefusesMessageDecodeOnceRawInputStreamIsRequested()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithSettled(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(5));
            Assert.IsNotNull(delivery);
            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload.Length, stream.Length);
            byte[] bytesRead = new byte[payload.Length];
            Assert.AreEqual(payload.Length, stream.Read(bytesRead));
            Assert.AreEqual(payload, bytesRead);

            try
            {
               _ = delivery.Message();
               Assert.Fail("Should not be able to use the message API once raw stream is requested");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            try
            {
               _ = delivery.Annotations;
               Assert.Fail("Should not be able to use the annotations API once raw stream is requested");
            }
            catch (ClientIllegalStateException)
            {
               // Expected
            }

            receiver.CloseAsync();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiveDeliveryWithMultipleDataSections()
      {
         Data section1 = new Data(new byte[] { 0, 1, 2, 3 });
         Data section2 = new Data(new byte[] { 0, 1, 2, 3 });
         Data section3 = new Data(new byte[] { 0, 1, 2, 3 });

         byte[] payload = CreateEncodedMessage(section1, section2, section3);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithSettled(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(5));
            Assert.IsNotNull(delivery);

            IAdvancedMessage<object> message = delivery.Message().ToAdvancedMessage();
            Assert.IsNotNull(message);

            Assert.AreEqual(3, message.GetBodySections().Count());
            List<ISection> section = new List<ISection>(message.GetBodySections());
            Assert.AreEqual(section1, section[0]);
            Assert.AreEqual(section2, section[1]);
            Assert.AreEqual(section3, section[2]);

            receiver.CloseAsync();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionWindowExpandedAsIncomingFramesArrive()
      {
         byte[] payload1 = new byte[255];
         byte[] payload2 = new byte[255];

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
            peer.ExpectBegin().WithIncomingWindow(1).Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(true)
                                 .WithSettled(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload1).Queue();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithSettled(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Queue();
            peer.ExpectFlow().WithIncomingWindow(1).WithLinkCredit(9);
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
            SessionOptions sessionOptions = new SessionOptions()
            {
               IncomingCapacity = 1024
            };
            ISession session = connection.OpenSession(sessionOptions);
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            IDelivery delivery = receiver.Receive(TimeSpan.FromSeconds(5));
            Assert.IsNotNull(delivery);

            Stream stream = delivery.RawInputStream;
            Assert.IsNotNull(stream);

            Assert.AreEqual(payload1.Length + payload2.Length, stream.Length);
            byte[] bytesRead = new byte[payload1.Length + payload2.Length];
            Assert.AreEqual(payload1.Length + payload2.Length, stream.Read(bytesRead));

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotReadFromStreamDeliveredBeforeConnectionDrop()
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
            peer.DropAfterLastHandler();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IReceiver receiver = connection.OpenReceiver("test-queue");
            IDelivery delivery = receiver.Receive();

            peer.WaitForScriptToComplete();

            Assert.IsNotNull(delivery);

            // Data already read so it will be already available for read.
            Assert.AreNotEqual(-1, delivery.RawInputStream.ReadByte());

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateReceiverWithDefaultSourceAndTargetOptions()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithAddress("test-queue")
                                            .WithDistributionMode(Test.Driver.Matchers.Is.NullValue())
                                            .WithDefaultTimeout()
                                            .WithDurable(Test.Driver.Codec.Messaging.TerminusDurability.None)
                                            .WithExpiryPolicy(Test.Driver.Codec.Messaging.TerminusExpiryPolicy.LinkDetach)
                                            .WithDefaultOutcome(new Test.Driver.Codec.Messaging.Modified() { DeliveryFailed = true })
                                            .WithCapabilities(Test.Driver.Matchers.Is.NullValue())
                                            .WithFilter(Test.Driver.Matchers.Is.NullValue())
                                            .WithOutcomes("amqp:accepted:list", "amqp:rejected:list", "amqp:released:list", "amqp:modified:list")
                                            .Also()
                               .WithTarget().WithAddress(Test.Driver.Matchers.Is.NotNullValue())
                                            .WithCapabilities(Test.Driver.Matchers.Is.NullValue())
                                            .WithDurable(Test.Driver.Matchers.Is.NullValue())
                                            .WithExpiryPolicy(Test.Driver.Matchers.Is.NullValue())
                                            .WithDefaultTimeout()
                                            .WithDynamic(Test.Driver.Matchers.Matches.AnyOf(
                                                               Test.Driver.Matchers.Is.NullValue(),
                                                               Test.Driver.Matchers.Is.EqualTo(false)))
                                            .WithDynamicNodeProperties(Test.Driver.Matchers.Is.NullValue())
                               .And().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
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
            IReceiver receiver = session.OpenReceiver("test-queue").OpenTask.Result;

            receiver.Close();
            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateReceiverWithUserConfiguredSourceAndTargetOptions()
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
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithAddress("test-queue")
                                            .WithDistributionMode("copy")
                                            .WithTimeout(128)
                                            .WithDurable(Test.Driver.Codec.Messaging.TerminusDurability.UnsettledState)
                                            .WithExpiryPolicy(Test.Driver.Codec.Messaging.TerminusExpiryPolicy.ConnectionClose)
                                            .WithDefaultOutcome(new Test.Driver.Codec.Messaging.Released())
                                            .WithCapabilities("QUEUE")
                                            .WithFilter(filtersToObject)
                                            .WithOutcomes("amqp:accepted:list", "amqp:rejected:list")
                                            .Also()
                               .WithTarget().WithAddress(Test.Driver.Matchers.Is.NotNullValue())
                                            .WithCapabilities("QUEUE")
                                            .WithDurable(Test.Driver.Codec.Messaging.TerminusDurability.Configuration)
                                            .WithExpiryPolicy(Test.Driver.Codec.Messaging.TerminusExpiryPolicy.SessionEnd)
                                            .WithTimeout(42)
                                            .WithDynamic(Test.Driver.Matchers.Matches.AnyOf(
                                                               Test.Driver.Matchers.Is.NullValue(),
                                                               Test.Driver.Matchers.Is.EqualTo(false)))
                                            .WithDynamicNodeProperties(Test.Driver.Matchers.Is.NullValue())
                               .And().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
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

            ReceiverOptions receiverOptions = new ReceiverOptions();

            receiverOptions.SourceOptions.Capabilities = new string[] { "QUEUE" };
            receiverOptions.SourceOptions.DistributionMode = DistributionMode.Copy;
            receiverOptions.SourceOptions.Timeout = 128;
            receiverOptions.SourceOptions.DurabilityMode = DurabilityMode.UnsettledState;
            receiverOptions.SourceOptions.ExpiryPolicy = ExpiryPolicy.ConnectionClose;
            receiverOptions.SourceOptions.DefaultOutcome = ClientReleased.Instance;
            receiverOptions.SourceOptions.Filters = filters;
            receiverOptions.SourceOptions.Outcomes =
               new DeliveryStateType[] { DeliveryStateType.Accepted, DeliveryStateType.Rejected };

            receiverOptions.TargetOptions.Capabilities = new string[] { "QUEUE" };
            receiverOptions.TargetOptions.DurabilityMode = DurabilityMode.Configuration;
            receiverOptions.TargetOptions.ExpiryPolicy = ExpiryPolicy.SessionClose;
            receiverOptions.TargetOptions.Timeout = 42;

            IReceiver receiver = session.OpenReceiver("test-queue", receiverOptions).OpenTask.Result;

            receiver.Close();
            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenDurableReceiver()
      {
         String address = "test-topic";
         String subscriptionName = "mySubscriptionName";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithName(subscriptionName)
                               .WithSource()
                                   .WithAddress(address)
                                   .WithDurable(Test.Driver.Codec.Messaging.TerminusDurability.UnsettledState)
                                   .WithExpiryPolicy(Test.Driver.Codec.Messaging.TerminusExpiryPolicy.Never)
                                   .WithDistributionMode("copy")
                               .And().Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenDurableReceiver(address, subscriptionName).OpenTask.Result;

            receiver.CloseAsync();

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

            ReceiverOptions options = new ReceiverOptions()
            {
               AutoAccept = autoAccept,
               CreditWindow = 10
            };

            IReceiver receiver = connection.OpenReceiver("test-receiver", options);

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

      [Test]
      public void TestDeliveryReadWithLongTimeoutValue()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithDeliveryTag(new byte[] { 1 })
                                 .WithMore(false)
                                 .WithSettled(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).AfterDelay(20).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();
            IReceiver receiver = session.OpenReceiver("test-queue");

            Assert.Throws<ArgumentOutOfRangeException>(() => receiver.Receive(TimeSpan.FromDays(50)));

            IDelivery delivery = receiver.Receive(TimeSpan.FromDays(49));
            Assert.IsNotNull(delivery);

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();

            receiver.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      private class AmqpJmsSelectorType : IDescribedType
      {
         private string selector;

         public object Descriptor => 0x0000468C00000004UL;

         public object Described => selector;

         public AmqpJmsSelectorType(string selector)
         {
            this.selector = selector;
         }

         public override string ToString()
         {
            return "AmqpJmsSelectorType{" + selector + "}";
         }
      }

      private class PeerJmsSelectorType : Apache.Qpid.Proton.Test.Driver.Codec.Primitives.UnknownDescribedType
      {
         public PeerJmsSelectorType(string selector) : base(0x0000468C00000004UL, selector)
         {
         }
      }

      [Test]
      public void TestCreateReceiverWithUserConfiguredSourceWithJMSStyleSelector()
      {
         IDescribedType clientJmsSelector = new AmqpJmsSelectorType("myProperty=42");
         IDictionary<string, object> filters = new Dictionary<string, object>();
         filters.Add("jms-selector", clientJmsSelector);

         PeerJmsSelectorType peerJmsSelector = new PeerJmsSelectorType("myProperty=42");
         IDictionary<string, object> filtersAtPeer = new Dictionary<string, object>();
         filtersAtPeer.Add("jms-selector", peerJmsSelector);

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource().WithAddress("test-queue")
                                            .WithDistributionMode("copy")
                                            .WithTimeout(128)
                                            .WithDurable(Test.Driver.Codec.Messaging.TerminusDurability.UnsettledState)
                                            .WithExpiryPolicy(Test.Driver.Codec.Messaging.TerminusExpiryPolicy.ConnectionClose)
                                            .WithDefaultOutcome(new Test.Driver.Codec.Messaging.Released())
                                            .WithCapabilities("QUEUE")
                                            .WithFilter(filtersAtPeer)
                                            .WithOutcomes("amqp:accepted:list", "amqp:rejected:list")
                                            .Also()
                               .WithTarget().WithAddress(Test.Driver.Matchers.Is.NotNullValue())
                                            .WithCapabilities("QUEUE")
                                            .WithDurable(Test.Driver.Codec.Messaging.TerminusDurability.Configuration)
                                            .WithExpiryPolicy(Test.Driver.Codec.Messaging.TerminusExpiryPolicy.SessionEnd)
                                            .WithTimeout(42)
                                            .WithDynamic(Test.Driver.Matchers.Matches.AnyOf(
                                                         Test.Driver.Matchers.Is.NullValue(),
                                                         Test.Driver.Matchers.Is.EqualTo(false)))
                                            .WithDynamicNodeProperties(Test.Driver.Matchers.Is.NullValue())
                               .And().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
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
            ReceiverOptions receiverOptions = new ReceiverOptions();

            receiverOptions.SourceOptions.Capabilities = new string[] { "QUEUE" };
            receiverOptions.SourceOptions.DistributionMode = DistributionMode.Copy;
            receiverOptions.SourceOptions.Timeout = 128;
            receiverOptions.SourceOptions.DurabilityMode = DurabilityMode.UnsettledState;
            receiverOptions.SourceOptions.ExpiryPolicy = ExpiryPolicy.ConnectionClose;
            receiverOptions.SourceOptions.DefaultOutcome = ClientReleased.Instance;
            receiverOptions.SourceOptions.Filters = filters;
            receiverOptions.SourceOptions.Outcomes =
               new DeliveryStateType[] { DeliveryStateType.Accepted, DeliveryStateType.Rejected };

            receiverOptions.TargetOptions.Capabilities = new string[] { "QUEUE" };
            receiverOptions.TargetOptions.DurabilityMode = DurabilityMode.Configuration;
            receiverOptions.TargetOptions.ExpiryPolicy = ExpiryPolicy.SessionClose;
            receiverOptions.TargetOptions.Timeout = 42;

            IReceiver receiver = session.OpenReceiver("test-queue", receiverOptions).OpenTask.GetAwaiter().GetResult();

            receiver.Close();
            session.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}