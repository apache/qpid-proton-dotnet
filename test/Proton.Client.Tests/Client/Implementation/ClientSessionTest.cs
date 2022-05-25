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
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Client.TestSupport;
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientSessionTest : ClientBaseTestFixture
   {
      [Test]
      public void TestSessionOpenTimeoutWhenNoRemoteBeginArrivesTimeout()
      {
         DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(true);
      }

      [Test]
      public void TestSessionOpenTimeoutWhenNoRemoteBeginArrivesNoTimeout()
      {
         DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(false);
      }

      private void DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 75;
            ISession session = connection.OpenSession(options);

            try
            {
               if (timeout)
               {
                  session.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.OpenTask.Wait();
               }

               Assert.Fail("Session Open should timeout when no Begin response and complete future with error.");
            }
            catch (Exception error)
            {
               logger.LogInformation("Session open failed with error: ", error.Message);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionOpenWaitWithTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionOpenWaitCanceledWhenConnectionDrops(true);
      }

      [Test]
      public void TestSessionOpenWaitWithNoTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionOpenWaitCanceledWhenConnectionDrops(false);
      }

      private void DoTestSessionOpenWaitCanceledWhenConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();

            try
            {
               if (timeout)
               {
                  session.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.OpenTask.Wait();
               }

               Assert.Fail("Session Open should wait should abort when connection drops.");
            }
            catch (Exception error)
            {
               logger.LogInformation("Session open failed with error: ", error.Message);
               Assert.IsTrue(error.InnerException is ClientIOException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionCloseTimeoutWhenNoRemoteEndArrivesTimeout()
      {
         DoTestSessionCloseTimeoutWhenNoRemoteEndArrives(true);
      }

      [Test]
      public void TestSessionCloseTimeoutWhenNoRemoteEndArrivesNoTimeout()
      {
         DoTestSessionCloseTimeoutWhenNoRemoteEndArrives(false);
      }

      private void DoTestSessionCloseTimeoutWhenNoRemoteEndArrives(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.CloseTimeout = 75;
            ISession session = connection.OpenSession(options).OpenTask.GetAwaiter().GetResult();

            try
            {
               if (timeout)
               {
                  session.CloseAsync().Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.CloseAsync().Wait();
               }

               Assert.Fail("Close should throw an error if the Session end doesn't arrive in time");
            }
            catch (Exception error)
            {
               logger.LogInformation("Session close failed with error: ", error);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionCloseWaitWithTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionCloseWaitCanceledWhenConnectionDrops(true);
      }

      [Test]
      public void TestSessionCloseWaitWithNoTimeoutCanceledWhenConnectionDrops()
      {
         DoTestSessionCloseWaitCanceledWhenConnectionDrops(false);
      }

      private void DoTestSessionCloseWaitCanceledWhenConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.CloseTimeout = 75;
            ISession session = connection.OpenSession(options).OpenTask.GetAwaiter().GetResult();

            try
            {
               if (timeout)
               {
                  session.CloseAsync().Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  session.CloseAsync().Wait();
               }
            }
            catch (Exception)
            {
               Assert.Fail("Session Close should complete when parent connection drops.");
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionCloseGetsResponseWithErrorDoesNotThrowTimedGet()
      {
         DoTestSessionCloseGetsResponseWithErrorThrows(true);
      }

      [Test]
      public void TestConnectionCloseGetsResponseWithErrorDoesNotThrowInfiniteGet()
      {
         DoTestSessionCloseGetsResponseWithErrorThrows(false);
      }

      protected void DoTestSessionCloseGetsResponseWithErrorThrows(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd().Respond().WithErrorCondition(AmqpError.INTERNAL_ERROR.ToString(), "Something odd happened.");
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession().OpenTask.GetAwaiter().GetResult();

            if (timeout)
            {
               session.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            }
            else
            {
               session.CloseAsync().Wait();
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionGetRemotePropertiesWaitsForRemoteBegin()
      {
         TryReadSessionRemoteProperties(true);
      }

      [Test]
      public void TestSessionGetRemotePropertiesFailsAfterOpenTimeout()
      {
         TryReadSessionRemoteProperties(false);
      }

      private void TryReadSessionRemoteProperties(bool beginResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 120;
            ISession session = connection.OpenSession(options);

            peer.WaitForScriptToComplete();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            expectedProperties.Add("TEST", "test-property");

            if (beginResponse)
            {
               peer.ExpectEnd().Respond();
               peer.RespondToLastBegin().WithProperties(expectedProperties).Later(10);
            }
            else
            {
               peer.ExpectEnd();
            }

            if (beginResponse)
            {
               Assert.IsNotNull(session.Properties, "Remote should have responded with a remote properties value");
               Assert.AreEqual(expectedProperties, session.Properties);
            }
            else
            {
               try
               {
                  _ = session.Properties;
                  Assert.Fail("Should failed to get remote state due to no begin response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex.Message);
               }
            }

            try
            {
               session.Close();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and end was sent");
            }

            peer.ExpectClose().Respond();
            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionGetRemoteOfferedCapabilitiesWaitsForRemoteBegin()
      {
         TryReadSessionRemoteOfferedCapabilities(true);
      }

      [Test]
      public void TestSessionGetRemoteOfferedCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadSessionRemoteOfferedCapabilities(false);
      }

      private void TryReadSessionRemoteOfferedCapabilities(bool beginResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            _ = connection.OpenTask.Result;
            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 125;
            ISession session = connection.OpenSession(options);

            peer.WaitForScriptToComplete();

            if (beginResponse)
            {
               peer.ExpectEnd().Respond();
               peer.RespondToLastBegin().WithOfferedCapabilities("transactions").Later(10);
            }
            else
            {
               peer.ExpectEnd();
            }

            if (beginResponse)
            {
               Assert.IsNotNull(session.OfferedCapabilities, "Remote should have responded with a remote offered Capabilities value");
               Assert.AreEqual(1, session.OfferedCapabilities.Count);
               Assert.AreEqual("transactions", session.OfferedCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = session.OfferedCapabilities;
                  Assert.Fail("Should failed to get remote state due to no begin response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               session.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex.Message);
               Assert.Fail("Should not fail to close when connection not closed and end was sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionGetRemoteDesiredCapabilitiesWaitsForRemoteBegin()
      {
         tryReadSessionRemoteDesiredCapabilities(true);
      }

      [Test]
      public void TestSessionGetRemoteDesiredCapabilitiesFailsAfterOpenTimeout()
      {
         tryReadSessionRemoteDesiredCapabilities(false);
      }

      private void tryReadSessionRemoteDesiredCapabilities(bool beginResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            SessionOptions options = new SessionOptions();
            options.OpenTimeout = 125;
            ISession session = connection.OpenSession(options);

            peer.WaitForScriptToComplete();

            if (beginResponse)
            {
               peer.ExpectEnd().Respond();
               peer.RespondToLastBegin().WithDesiredCapabilities("Error-Free").Later(10);
            }
            else
            {
               peer.ExpectEnd();
            }

            if (beginResponse)
            {
               Assert.IsNotNull(session.DesiredCapabilities, "Remote should have responded with a remote desired Capabilities value");
               Assert.AreEqual(1, session.DesiredCapabilities.Count);
               Assert.AreEqual("Error-Free", session.DesiredCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = session.DesiredCapabilities;
                  Assert.Fail("Should failed to get remote state due to no begin response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex);
               }
            }

            try
            {
               session.CloseAsync().Wait();
            }
            catch (Exception ex)
            {
               logger.LogDebug("Caught unexpected exception from close call", ex);
               Assert.Fail("Should not fail to close when connection not closed and end sent");
            }

            peer.ExpectClose().Respond();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestQuickOpenCloseWhenNoBeginResponseFailsFastOnOpenTimeout()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.ExpectEnd();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ConnectionOptions options = new ConnectionOptions();
            options.CloseTimeout = (long)TimeSpan.FromHours(1).TotalMilliseconds;

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            connection.OpenTask.Wait();

            SessionOptions sessionOptions = new SessionOptions();
            sessionOptions.OpenTimeout = 125;

            try
            {
               connection.OpenSession(sessionOptions).CloseAsync().Wait();
            }
            catch (Exception)
            {
               Assert.Fail("Should not fail when waiting on close with quick open timeout");
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseWithErrorConditionSync()
      {
         DoTestCloseWithErrorCondition(true);
      }

      [Test]
      public void TestCloseWithErrorConditionAsync()
      {
         DoTestCloseWithErrorCondition(false);
      }

      private void DoTestCloseWithErrorCondition(bool sync)
      {
         string condition = "amqp:precondition-failed";
         string description = "something bad happened.";

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd().WithError(condition, description).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();

            session.OpenTask.Wait();

            Assert.AreEqual(session.Client, container);

            if (sync)
            {
               session.Close(IErrorCondition.Create(condition, description, null));
            }
            else
            {
               session.CloseAsync(IErrorCondition.Create(condition, description, null));
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCannotCreateResourcesFromClosedSession()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            ISession session = connection.OpenSession();

            session.OpenTask.Result.Close();

            Assert.Throws<ClientIllegalStateException>(() => session.OpenReceiver("test"));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenReceiver("test", new ReceiverOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDurableReceiver("test", "test"));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDurableReceiver("test", "test", new ReceiverOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDynamicReceiver());
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDynamicReceiver(new ReceiverOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenDynamicReceiver(new ReceiverOptions(), new Dictionary<string, object>()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenSender("test"));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenSender("test", new SenderOptions()));
            Assert.Throws<ClientIllegalStateException>(() => session.OpenAnonymousSender());
            Assert.Throws<ClientIllegalStateException>(() => session.OpenAnonymousSender(new SenderOptions()));

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverFromDefaultSessionReturnsSameReceiverForQueuedDeliveries()
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
            ConnectionOptions connOptions = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.FirstAvailable
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, connOptions);

            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 0,
               AutoAccept = false
            };
            IReceiver receiver = connection.OpenReceiver("test-receiver", options);
            receiver.AddCredit(10);

            Wait.WaitFor(() => receiver.QueuedDeliveries == 10);

            peer.WaitForScriptToComplete();

            for (int i = 0; i < 10; ++i)
            {
               IReceiver nextReceiver = connection.NextReceiver();
               Assert.AreSame(receiver, nextReceiver);
               IDelivery delivery = nextReceiver.Receive();
               Assert.IsNotNull(delivery);
            }

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverTimesOut()
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

            _ = connection.OpenReceiver("test-receiver").OpenTask.Result;

            peer.WaitForScriptToComplete();

            Assert.IsNull(connection.NextReceiver(TimeSpan.FromMilliseconds(10)));

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverReturnsAllReceiversEventually()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);

            ReceiverOptions options = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };

            _ = connection.OpenReceiver("test-receiver1", options).OpenTask.Result;
            _ = connection.OpenReceiver("test-receiver2", options).OpenTask.Result;
            _ = connection.OpenReceiver("test-receiver3", options).OpenTask.Result;

            peer.WaitForScriptToComplete();

            IReceiver receiver1 = connection.NextReceiver(NextReceiverPolicy.FirstAvailable);
            Assert.IsNotNull(receiver1.Receive());
            IReceiver receiver2 = connection.NextReceiver(NextReceiverPolicy.FirstAvailable);
            Assert.IsNotNull(receiver2.Receive());
            IReceiver receiver3 = connection.NextReceiver(NextReceiverPolicy.FirstAvailable);
            Assert.IsNotNull(receiver3.Receive());

            Assert.AreNotSame(receiver1, receiver2);
            Assert.AreNotSame(receiver1, receiver3);
            Assert.AreNotSame(receiver2, receiver3);

            peer.WaitForScriptToComplete();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionOptionsConfiguresLargestBacklogNextReceiverPolicy()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(3)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.LargestBacklog
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = connection.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = connection.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = connection.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Wait.WaitFor(() => receiver1.QueuedDeliveries == 1);
            Wait.WaitFor(() => receiver2.QueuedDeliveries == 2);
            Wait.WaitFor(() => receiver3.QueuedDeliveries == 1);

            IReceiver next = connection.NextReceiver();
            Assert.AreSame(next, receiver2);

            peer.WaitForScriptToComplete();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionOptionsConfiguresLargestBacklogNextReceiverPolicy()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(3)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.Random
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            SessionOptions sessionOptions = new SessionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.LargestBacklog
            };
            ISession session = connection.OpenSession(sessionOptions);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = session.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = session.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = session.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Wait.WaitFor(() => receiver1.QueuedDeliveries == 1);
            Wait.WaitFor(() => receiver2.QueuedDeliveries == 2);
            Wait.WaitFor(() => receiver3.QueuedDeliveries == 1);

            IReceiver next = session.NextReceiver();
            Assert.AreSame(next, receiver2);

            peer.WaitForScriptToComplete();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestUserSpecifiedNextReceiverPolicyOverridesConfiguration()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(3)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.Random
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            SessionOptions sessionOptions = new SessionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.SmallestBacklog
            };
            ISession session = connection.OpenSession(sessionOptions);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = session.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = session.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = session.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Wait.WaitFor(() => receiver1.QueuedDeliveries == 1);
            Wait.WaitFor(() => receiver2.QueuedDeliveries == 2);
            Wait.WaitFor(() => receiver3.QueuedDeliveries == 1);

            IReceiver next = session.NextReceiver(NextReceiverPolicy.LargestBacklog);
            Assert.AreSame(next, receiver2);

            peer.WaitForScriptToComplete();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionOptionsConfiguresSmallestBacklogNextReceiverPolicy()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(3)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(4)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(5)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.Random
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            SessionOptions sessionOptions = new SessionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.SmallestBacklog
            };
            ISession session = connection.OpenSession(sessionOptions);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = session.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = session.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = session.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Wait.WaitFor(() => receiver1.QueuedDeliveries == 3);
            Wait.WaitFor(() => receiver2.QueuedDeliveries == 2);
            Wait.WaitFor(() => receiver3.QueuedDeliveries == 1);

            IReceiver next = session.NextReceiver();
            Assert.AreSame(next, receiver3);

            peer.WaitForScriptToComplete();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverCompletesAfterDeliveryArrivesRoundRobin()
      {
         DoTestNextReceiverCompletesAfterDeliveryArrives(NextReceiverPolicy.RoundRobin);
      }

      [Test]
      public void TestNextReceiverCompletesAfterDeliveryArrivesRandom()
      {
         DoTestNextReceiverCompletesAfterDeliveryArrives(NextReceiverPolicy.Random);
      }

      [Test]
      public void TestNextReceiverCompletesAfterDeliveryArrivesLargestBacklog()
      {
         DoTestNextReceiverCompletesAfterDeliveryArrives(NextReceiverPolicy.LargestBacklog);
      }

      [Test]
      public void TestNextReceiverCompletesAfterDeliveryArrivesSmallestBacklog()
      {
         DoTestNextReceiverCompletesAfterDeliveryArrives(NextReceiverPolicy.SmallestBacklog);
      }

      [Test]
      public void TestNextReceiverCompletesAfterDeliveryArrivesFirstAvailable()
      {
         DoTestNextReceiverCompletesAfterDeliveryArrives(NextReceiverPolicy.FirstAvailable);
      }

      public void DoTestNextReceiverCompletesAfterDeliveryArrives(NextReceiverPolicy policy)
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.Start();

            CountdownEvent done = new CountdownEvent(1);

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = policy
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            _ = connection.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Task.Run(() =>
            {
               try
               {
                  IReceiver receiver = connection.NextReceiver();
                  IDelivery delivery = receiver.Receive();
                  logger.LogInformation("Next receiver returned delivery with body: {0}", delivery.Message().Body);
                  done.Signal();
               }
               catch (Exception e)
               {
                  logger.LogDebug("Failed in next receiver task: {0}", e);
               }
            });

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Later(15);

            peer.WaitForScriptToComplete();

            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(10)));

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverThrowsAfterSessionClosedRoundRobin()
      {
         DoTestNextReceiverThrowsAfterSessionClosed(NextReceiverPolicy.RoundRobin);
      }

      [Test]
      public void TestNextReceiverThrowsAfterSessionClosedRandom()
      {
         DoTestNextReceiverThrowsAfterSessionClosed(NextReceiverPolicy.Random);
      }

      [Test]
      public void TestNextReceiverThrowsAfterSessionClosedLargestBacklog()
      {
         DoTestNextReceiverThrowsAfterSessionClosed(NextReceiverPolicy.LargestBacklog);
      }

      [Test]
      public void TestNextReceiverThrowsAfterSessionClosedSmallestBacklog()
      {
         DoTestNextReceiverThrowsAfterSessionClosed(NextReceiverPolicy.SmallestBacklog);
      }

      [Test]
      public void TestNextReceiverThrowsAfterSessionClosedFirstAvailable()
      {
         DoTestNextReceiverThrowsAfterSessionClosed(NextReceiverPolicy.FirstAvailable);
      }

      public void DoTestNextReceiverThrowsAfterSessionClosed(NextReceiverPolicy policy)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.Start();

            CountdownEvent started = new CountdownEvent(1);
            CountdownEvent done = new CountdownEvent(1);
            Exception error = null;

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();

            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = policy
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            ISession session = connection.OpenSession().OpenTask.Result;

            Task.Run(() =>
            {
               try
               {
                  started.Signal();
                  session.NextReceiver();
               }
               catch (ClientException e)
               {
                  error = e;
               }
               finally
               {
                  done.Signal();
               }
            });

            peer.WaitForScriptToComplete();

            Assert.IsTrue(started.Wait(TimeSpan.FromSeconds(10)));

            peer.ExpectEnd().Respond();

            session.CloseAsync();

            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(error is ClientIllegalStateException);

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverCompletesWhenCalledBeforeReceiverCreateRoundRobin()
      {
         DoTestNextReceiverCompletesWhenCalledBeforeReceiverCreate(NextReceiverPolicy.RoundRobin);
      }

      [Test]
      public void TestNextReceiverCompletesWhenCalledBeforeReceiverCreateRandom()
      {
         DoTestNextReceiverCompletesWhenCalledBeforeReceiverCreate(NextReceiverPolicy.Random);
      }

      [Test]
      public void TestNextReceiverCompletesWhenCalledBeforeReceiverCreateLargestBacklog()
      {
         DoTestNextReceiverCompletesWhenCalledBeforeReceiverCreate(NextReceiverPolicy.LargestBacklog);
      }

      [Test]
      public void TestNextReceiverCompletesWhenCalledBeforeReceiverCreateSmallestBacklog()
      {
         DoTestNextReceiverCompletesWhenCalledBeforeReceiverCreate(NextReceiverPolicy.SmallestBacklog);
      }

      [Test]
      public void TestNextReceiverCompletesWhenCalledBeforeReceiverCreateFirstAvailable()
      {
         DoTestNextReceiverCompletesWhenCalledBeforeReceiverCreate(NextReceiverPolicy.FirstAvailable);
      }

      public void DoTestNextReceiverCompletesWhenCalledBeforeReceiverCreate(NextReceiverPolicy policy)
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.Start();

            CountdownEvent started = new CountdownEvent(1);
            CountdownEvent done = new CountdownEvent(1);

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = policy
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            Task.Run(() =>
            {
               try
               {
                  started.Signal();
                  IReceiver receiver = connection.NextReceiver();
                  IDelivery delivery = receiver.Receive();
                  logger.LogInformation("Next receiver returned delivery with body: {0}", delivery.Message().Body);
                  done.Signal();
               }
               catch (Exception e)
               {
                  logger.LogDebug("Failed in next receiver task: {0}", e);
               }
            });

            Assert.IsTrue(started.Wait(TimeSpan.FromSeconds(10)));

            _ = connection.OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue().AfterDelay(10);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            _ = connection.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(10)));

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverRoundRobinReturnsNextReceiverAfterLast()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.Random
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            SessionOptions sessionOptions = new SessionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.RoundRobin
            };
            ISession session = connection.OpenSession(sessionOptions);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = session.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = session.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = session.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Wait.WaitFor(() => receiver2.QueuedDeliveries == 2);
            Wait.WaitFor(() => receiver3.QueuedDeliveries == 1);

            Assert.AreEqual(0, receiver1.QueuedDeliveries);

            IReceiver next = session.NextReceiver();
            Assert.AreSame(next, receiver2);
            next = session.NextReceiver();
            Assert.AreSame(next, receiver3);

            peer.WaitForScriptToComplete();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverRoundRobinPolicyWrapsAround()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.Random
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            SessionOptions sessionOptions = new SessionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.RoundRobin
            };
            ISession session = connection.OpenSession(sessionOptions);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = session.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = session.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = session.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Wait.WaitFor(() => receiver2.QueuedDeliveries == 2);
            Wait.WaitFor(() => receiver3.QueuedDeliveries == 1);

            Assert.AreEqual(0, receiver1.QueuedDeliveries);

            IReceiver next = session.NextReceiver();
            Assert.AreSame(next, receiver2);
            next = session.NextReceiver();
            Assert.AreSame(next, receiver3);

            peer.WaitForScriptToComplete();

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(3)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();

            Wait.WaitFor(() => receiver1.QueuedDeliveries == 1);

            next = session.NextReceiver();
            Assert.AreSame(next, receiver1);

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNextReceiverRoundRobinPolicyRestartsWhenLastReceiverClosed()
      {
         byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(1)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(2)
                                 .WithDeliveryId(2)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.Random
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            SessionOptions sessionOptions = new SessionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.RoundRobin
            };
            ISession session = connection.OpenSession(sessionOptions);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = session.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = session.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = session.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();

            Wait.WaitFor(() => receiver2.QueuedDeliveries == 2);
            Wait.WaitFor(() => receiver3.QueuedDeliveries == 1);

            Assert.AreEqual(0, receiver1.QueuedDeliveries);

            IReceiver next = session.NextReceiver();
            Assert.AreSame(next, receiver2);
            next.Close();

            peer.WaitForScriptToComplete();

            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(3)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();

            Wait.WaitFor(() => receiver1.QueuedDeliveries == 1);

            next = session.NextReceiver();
            Assert.AreSame(next, receiver1);

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

    [Test]
    public void TestNextReceiverRoundRobinPolicySkipsEmptyReceivers()  {
        byte[] payload = CreateEncodedMessage(new AmqpValue("Hello World"));

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.ExpectAttach().OfReceiver().Respond();
            peer.ExpectFlow().WithLinkCredit(10);
            peer.RemoteTransfer().WithHandle(0)
                                 .WithDeliveryId(0)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.RemoteTransfer().WithHandle(3)
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.Random
            };
            IConnection connection = container.Connect(remoteAddress, remotePort, options);
            SessionOptions sessionOptions = new SessionOptions()
            {
               DefaultNextReceiverPolicy = NextReceiverPolicy.RoundRobin
            };
            ISession session = connection.OpenSession(sessionOptions);

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };
            IReceiver receiver1 = session.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;
            IReceiver receiver2 = session.OpenReceiver("test-receiver2", receiverOptions).OpenTask.Result;
            IReceiver receiver3 = session.OpenReceiver("test-receiver3", receiverOptions).OpenTask.Result;
            IReceiver receiver4 = session.OpenReceiver("test-receiver4", receiverOptions).OpenTask.Result;

            peer.WaitForScriptToComplete();

            Wait.WaitFor(() => receiver1.QueuedDeliveries == 1);
            Wait.WaitFor(() => receiver4.QueuedDeliveries == 1);

            Assert.AreEqual(0, receiver2.QueuedDeliveries);
            Assert.AreEqual(0, receiver3.QueuedDeliveries);

            IReceiver next = session.NextReceiver();
            Assert.AreSame(next, receiver1);
            next = session.NextReceiver();
            Assert.AreSame(next, receiver4);

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            connection.Close();

            peer.WaitForScriptToComplete();
        }
    }

   }
}