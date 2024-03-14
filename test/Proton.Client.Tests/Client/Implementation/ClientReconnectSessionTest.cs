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
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Messaging;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectSessionTest : ClientBaseTestFixture
   {
      [Test]
      public void TestOpenedSessionRecoveredAfterConnectionDropped()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.DropAfterLastHandler(5);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
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
            ISession session = connection.OpenSession().OpenTask.Result;

            firstPeer.WaitForScriptToComplete();

            secondPeer.WaitForScriptToComplete();

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionCreationRecoversAfterDropWithNoBeginResponse()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin();
            firstPeer.DropAfterLastHandler(20);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfReceiver().Respond();
            secondPeer.ExpectFlow();
            secondPeer.ExpectClose().Respond();
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

            firstPeer.WaitForScriptToComplete();

            IReceiver receiver = session.OpenTask.Result.OpenReceiver("queue").OpenTask.Result;

            Assert.IsNull(receiver.TryReceive());

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestMultipleSessionCreationRecoversAfterDropWithNoBeginResponse()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer intermediatePeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer finalPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectBegin();
            firstPeer.DropAfterLastHandler(20);
            firstPeer.Start();

            intermediatePeer.ExpectSASLAnonymousConnect();
            intermediatePeer.ExpectOpen().Respond();
            intermediatePeer.ExpectBegin().Respond();
            intermediatePeer.ExpectBegin();
            intermediatePeer.DropAfterLastHandler();
            intermediatePeer.Start();

            finalPeer.ExpectSASLAnonymousConnect();
            finalPeer.ExpectOpen().Respond();
            finalPeer.ExpectBegin().Respond();
            finalPeer.ExpectBegin().Respond();
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
            options.IdleTimeout = (uint)TimeSpan.FromSeconds(5).Milliseconds;
            options.ReconnectOptions.AddReconnectLocation(intermediateAddress, intermediatePort);
            options.ReconnectOptions.AddReconnectLocation(finalAddress, finalPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            ISession session1 = connection.OpenSession();
            ISession session2 = connection.OpenSession();

            firstPeer.WaitForScriptToComplete();
            intermediatePeer.WaitForScriptToComplete();

            // Await both being open before doing work to make the outcome predictable
            session1.OpenTask.Wait();
            session2.OpenTask.Wait();

            finalPeer.WaitForScriptToComplete();
            finalPeer.ExpectAttach().OfReceiver().Respond();
            finalPeer.ExpectFlow();
            finalPeer.ExpectAttach().OfReceiver().Respond();
            finalPeer.ExpectFlow();
            finalPeer.ExpectClose().Respond();

            IReceiver receiver1 = session1.OpenReceiver("queue").OpenTask.Result;
            IReceiver receiver2 = session2.OpenReceiver("queue").OpenTask.Result;

            Assert.IsNull(receiver1.TryReceive());
            Assert.IsNull(receiver2.TryReceive());

            connection.Close();

            finalPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionOpenTimeoutWhenNoRemoteBeginArrivesTimeoutWithReconnection()
      {
         DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(true);
      }

      [Test]
      public void TestSessionOpenTimeoutWhenNoRemoteBeginArrivesNoTimeoutWithReconnection()
      {
         DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(false);
      }

      /*
       * Tests that session open timeout is preserved across reconnection boundaries
       */
      private void DoTestSessionOpenTimeoutWhenNoRemoteBeginArrives(bool timeout)
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer finalPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin();
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            finalPeer.ExpectSASLAnonymousConnect();
            finalPeer.ExpectOpen().Respond();
            finalPeer.ExpectBegin().Optional();  // Might not arrive if timed out already
            finalPeer.ExpectEnd().Optional();
            finalPeer.ExpectClose().Respond();
            finalPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string finalAddress = finalPeer.ServerAddress;
            int finalPort = finalPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, final peer listening on: {0}:{1}", finalAddress, finalPort);

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(finalAddress, finalPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            ISession session = connection.OpenSession(new SessionOptions()
            {
               OpenTimeout = 500
            });

            try
            {
               if (timeout)
               {
                  session.OpenTask.Wait(TimeSpan.FromSeconds(1));
               }
               else
               {
                  session.OpenTask.Wait();
               }

               Assert.Fail("Session Open should timeout when no Begin response and complete future with error.");
            }
            catch (Exception error)
            {
               logger.LogInformation("Session open failed with error: {0}", error.Message);
            }

            connection.Close();

            firstPeer.WaitForScriptToComplete();
            finalPeer.WaitForScriptToComplete();
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

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer finalPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfReceiver().Respond();
            firstPeer.ExpectFlow().WithLinkCredit(10);
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            finalPeer.ExpectSASLAnonymousConnect();
            finalPeer.ExpectOpen().Respond();
            finalPeer.ExpectBegin().Respond();
            finalPeer.ExpectAttach().OfReceiver().Respond();
            finalPeer.ExpectFlow().WithLinkCredit(10);
            finalPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string finalAddress = finalPeer.ServerAddress;
            int finalPort = finalPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, final peer listening on: {0}:{1}", finalAddress, finalPort);

            CountdownEvent done = new CountdownEvent(1);

            ConnectionOptions options = new ConnectionOptions()
            {
               DefaultNextReceiverPolicy = policy,
            };
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(finalAddress, finalPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

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
                  logger.LogDebug("Exception in next receiver task: {0}", e.Message);
               }
            });

            ReceiverOptions receiverOptions = new ReceiverOptions()
            {
               CreditWindow = 10,
               AutoAccept = false
            };

            _ = connection.OpenReceiver("test-receiver1", receiverOptions).OpenTask.Result;

            firstPeer.WaitForScriptToComplete();
            finalPeer.WaitForScriptToComplete();

            finalPeer.RemoteTransfer().WithHandle(0)
                                      .WithDeliveryId(0)
                                      .WithMore(false)
                                      .WithMessageFormat(0)
                                      .WithPayload(payload).Later(15);

            finalPeer.WaitForScriptToComplete();

            Assert.IsTrue(done.Wait(TimeSpan.FromSeconds(10)));

            finalPeer.WaitForScriptToComplete();
            finalPeer.ExpectClose().Respond();

            connection.Close();

            finalPeer.WaitForScriptToComplete();
         }
      }
   }
}