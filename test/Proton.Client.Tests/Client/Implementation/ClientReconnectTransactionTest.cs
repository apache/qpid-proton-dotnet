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
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.TestSupport;
using Apache.Qpid.Proton.Test.Driver;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectTransactionTest : ClientBaseTestFixture
   {
      [Test]
      public void TestDeclareTransactionAfterConnectionDropsAndReconnects()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.DropAfterLastHandler();
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
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            ISession session = connection.OpenSession().OpenTask.Result;

            firstPeer.WaitForScriptToComplete();

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectCoordinatorAttach().Respond();
            secondPeer.RemoteFlow().WithLinkCredit(2).Queue();
            secondPeer.ExpectDeclare().Accept(txnId);
            secondPeer.ExpectClose().Respond();

            try
            {
               session.BeginTransaction();
            }
            catch (ClientException cliEx)
            {
               logger.LogWarning("Caught unexpected error from test: {0}", cliEx);
               Assert.Fail("Should not have failed to declare transaction");
            }

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestTransactionInDoubtAfterReconnect()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectCoordinatorAttach().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(2).Queue();
            firstPeer.ExpectDeclare().Accept(txnId);
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.ExpectTransfer().WithNonNullPayload();
            firstPeer.DropAfterLastHandler();
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
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();

            ISender sender = session.OpenSender("test").OpenTask.Result;
            sender.Send(IMessage<string>.Create("Hello"));

            firstPeer.WaitForScriptToComplete();

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectClose().Respond();

            try
            {
               session.CommitTransaction();
               Assert.Fail("Should have failed to declare transaction");
            }
            catch (ClientTransactionRolledBackException cliEx)
            {
               logger.LogInformation("Caught expected error from test: {0}", cliEx);
            }

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSendInTransactionIsNoOpAfterReconnect()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectCoordinatorAttach().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(2).Queue();
            firstPeer.ExpectDeclare().Accept(txnId);
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(1).Queue();
            firstPeer.ExpectTransfer().WithNonNullPayload();
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
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
            ISession session = connection.OpenSession().OpenTask.Result;

            session.BeginTransaction();

            ISender sender = session.OpenSender("test").OpenTask.Result;
            sender.Send(IMessage<string>.Create("Hello"));

            firstPeer.WaitForScriptToComplete();

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectClose().Respond();

            sender.Send(IMessage<string>.Create("Hello Again"));

            try
            {
               session.CommitTransaction();
               Assert.Fail("Should have failed to declare transaction");
            }
            catch (ClientTransactionRolledBackException cliEx)
            {
               logger.LogInformation("Caught expected error from test: {0}", cliEx.Message);
            }

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestNewTransactionCanBeCreatedAfterOldInstanceRolledBackByReconnect()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
            firstPeer.ExpectCoordinatorAttach().Respond();
            firstPeer.RemoteFlow().WithLinkCredit(2).Queue();
            firstPeer.ExpectDeclare().Accept(txnId);
            firstPeer.DropAfterLastHandler(5);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectAttach().OfSender().WithTarget().WithAddress("test").And().Respond();
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
            ISession session = connection.OpenSession().OpenTask.Result;
            ISender sender = session.OpenSender("test").OpenTask.Result;

            session.BeginTransaction();

            firstPeer.WaitForScriptToComplete();

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectCoordinatorAttach().Respond();
            secondPeer.RemoteFlow().WithLinkCredit(2).Queue();
            secondPeer.ExpectDeclare().Accept(txnId);
            secondPeer.ExpectTransfer().WithHandle(0)
                                      .WithNonNullPayload()
                                      .WithState().Transactional().WithTxnId(txnId).And()
                                      .Respond()
                                      .WithState().Transactional().WithTxnId(txnId).WithAccepted().And()
                                      .WithSettled(true);
            secondPeer.ExpectDischarge().WithFail(false).WithTxnId(txnId).Accept();
            secondPeer.ExpectEnd().Respond();
            secondPeer.ExpectClose().Respond();

            try
            {
               session.CommitTransaction();
               Assert.Fail("Should have failed to declare transaction");
            }
            catch (ClientTransactionRolledBackException cliEx)
            {
               logger.LogInformation("Caught expected error from test: {0}", cliEx.Message);
            }

            session.BeginTransaction();

            ITracker tracker = sender.Send(IMessage<string>.Create("test-message"));

            Assert.IsNotNull(tracker);
            Assert.IsNotNull(tracker.SettlementTask.Result);
            Assert.AreEqual(tracker.RemoteState.Type, DeliveryStateType.Transactional);
            Assert.IsNotNull(tracker.State);
            Assert.AreEqual(tracker.State.Type, DeliveryStateType.Transactional,
                "Delivery inside transaction should have Transactional state: " + tracker.State.Type);
            Wait.AssertTrue("Delivery in transaction should be locally settled after response", () => tracker.Settled);

            try
            {
               session.CommitTransaction();
            }
            catch (ClientException cliEx)
            {
               logger.LogInformation("Caught unexpected error from test: {0}", cliEx.Message);
               Assert.Fail("Should not have failed to declare transaction");
            }

            session.Close();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

   }
}