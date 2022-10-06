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
using System.Threading;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Transport;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectSenderTest : ClientBaseTestFixture
   {
      [Test]
      public void TestOpenedSenderRecoveredAfterConnectionDropped()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.DropAfterLastHandler(5);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
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
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test");

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectDetach().WithClosed(true).Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            sender.Close();
            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestInFlightSendFailedAfterConnectionDroppedAndNotResent()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.ExpectTransfer().WithNonNullPayload();
            firstPeer.DropAfterLastHandler(15);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
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
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test");

            AtomicReference<ITracker> tracker = new AtomicReference<ITracker>();
            AtomicReference<ClientException> error = new AtomicReference<ClientException>();
            CountdownEvent latch = new CountdownEvent(1);

            Task.Run(() =>
            {
               try
               {
                  tracker.Set(sender.Send(IMessage<string>.Create("Hello")));
               }
               catch (ClientException e)
               {
                  error.Set(e);
               }
               finally
               {
                  latch.Signal();
               }
            });

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectDetach().WithClosed(true).Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(5)), "Should have failed previously sent message");
            Assert.IsNotNull(tracker.Get());
            Assert.IsNull(error.Get());
            Assert.Throws<ClientConnectionRemotelyClosedException>(() => tracker.Get().AwaitSettlement());

            sender.Close();
            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendBlockedOnCreditGetsSentAfterReconnectAndCreditGranted()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.DropAfterLastHandler(15);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
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
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test");

            AtomicReference<ITracker> tracker = new AtomicReference<ITracker>();
            AtomicReference<Exception> sendError = new AtomicReference<Exception>();
            CountdownEvent latch = new CountdownEvent(1);

            Task.Run(() =>
            {
               try
               {
                  tracker.Set(sender.Send(IMessage<string>.Create("Hello")));
               }
               catch (ClientException e)
               {
                  sendError.Set(e);
               }
               finally
               {
                  latch.Signal();
               }
            });

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectTransfer().WithNonNullPayload()
                                      .Respond()
                                      .WithSettled(true).WithState().Accepted();
            secondPeer.ExpectDetach().WithClosed(true).Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            // Grant credit now and await expected message send.
            secondPeer.RemoteFlow().WithDeliveryCount(0)
                                  .WithLinkCredit(10)
                                  .WithIncomingWindow(10)
                                  .WithOutgoingWindow(10)
                                  .WithNextIncomingId(0)
                                  .WithNextOutgoingId(1).Now();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)), "Should have sent blocked message");
            Assert.IsNull(sendError.Get());
            Assert.IsNotNull(tracker.Get());

            ITracker send = tracker.Get();
            Assert.AreSame(tracker.Get(), send.AwaitSettlement(TimeSpan.FromSeconds(10)));
            Assert.IsTrue(send.RemoteSettled);
            Assert.AreEqual(send.RemoteState, ClientAccepted.Instance);

            sender.Close();
            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAwaitSettlementOnSendFiredBeforeConnectionDrops()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.ExpectTransfer().WithNonNullPayload();
            firstPeer.DropAfterLastHandler(15);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
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
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test");
            ITracker tracker = sender.Send(IMessage<string>.Create("Hello"));

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectDetach().WithClosed(true).Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            try
            {
               tracker.AwaitSettlement(TimeSpan.FromSeconds(10));
               Assert.Fail("Should not be able to successfully await settlement");
            }
            catch (ClientConnectionRemotelyClosedException) { }

            Assert.IsFalse(tracker.RemoteSettled);
            Assert.IsNull(tracker.RemoteState);

            sender.Close();
            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestMultipleSenderCreationRecoversAfterDropWithNoAttachResponse()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer intermediatePeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer finalPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().Respond();
            firstPeer.ExpectAttach().OfSender();
            firstPeer.DropAfterLastHandler(20);
            firstPeer.Start();

            intermediatePeer.ExpectSASLAnonymousConnect();
            intermediatePeer.ExpectOpen().Respond();
            intermediatePeer.ExpectBegin().Respond();
            intermediatePeer.ExpectAttach().OfSender();
            intermediatePeer.ExpectAttach().OfSender();
            intermediatePeer.DropAfterLastHandler();
            intermediatePeer.Start();

            finalPeer.ExpectSASLAnonymousConnect();
            finalPeer.ExpectOpen().Respond();
            finalPeer.ExpectBegin().Respond();
            finalPeer.ExpectAttach().OfSender().Respond();
            finalPeer.ExpectAttach().OfSender().Respond();
            finalPeer.ExpectClose().Respond();
            finalPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string intermediateAddress = intermediatePeer.ServerAddress;
            int intermediatePort = intermediatePeer.ServerPort;
            string finalAddress = finalPeer.ServerAddress;
            int finalPort = finalPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, intermediate peer listening on: {0}:{1}", intermediateAddress, intermediatePort);
            logger.LogInformation("Test started, final peer listening on: {0}:{1}", finalAddress, finalPort);

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(intermediateAddress, intermediatePort);
            options.ReconnectOptions.AddReconnectLocation(finalAddress, finalPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            ISession session = connection.OpenSession();

            ISender sender1 = session.OpenSender("queue-1");
            ISender sender2 = session.OpenSender("queue-2");

            firstPeer.WaitForScriptToComplete();
            intermediatePeer.WaitForScriptToComplete();

            // Await both being open before doing work to make the outcome predictable
            sender1.OpenTask.Wait();
            sender2.OpenTask.Wait();

            Assert.IsNull(sender1.TrySend(IMessage<string>.Create("test")));
            Assert.IsNull(sender2.TrySend(IMessage<string>.Create("test")));

            connection.Close();

            finalPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestInFlightSendFailedAfterConnectionForcedCloseAndNotResent()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.ExpectTransfer().WithNonNullPayload();
            firstPeer.RemoteClose()
                     .WithErrorCondition(ConnectionError.CONNECTION_FORCED.ToString(), "Forced disconnect").Queue().AfterDelay(20);
            firstPeer.ExpectClose();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
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
            ISession session = connection.OpenSession();
            ISender sender = session.OpenSender("test");

            AtomicReference<ITracker> tracker = new();
            AtomicReference<ClientException> error = new();
            CountdownEvent latch = new CountdownEvent(1);

            Task.Run(() =>
            {
               try
               {
                  tracker.Set(sender.Send(IMessage<string>.Create("Hello")));
               }
               catch (ClientException e)
               {
                  error.Set(e);
               }
               finally
               {
                  latch.Signal();
               }
            });

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectDetach().WithClosed(true).Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            Assert.IsTrue(latch.Wait(TimeSpan.FromSeconds(10)), "Should have failed previously sent message");
            Assert.IsNull(error.Get());
            Assert.IsNotNull(tracker.Get());
            Assert.Throws<ClientConnectionRemotelyClosedException>(() => tracker.Get().AwaitSettlement());

            sender.Close();
            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }
   }
}