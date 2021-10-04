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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;
using Is = Apache.Qpid.Proton.Test.Driver.Matchers.Is;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonTransactionManagerTest : ProtonEngineTestSupport
   {
      private String[] DEFAULT_OUTCOMES_STRINGS = new String[] { Accepted.DescriptorSymbol.ToString(),
                                                                 Rejected.DescriptorSymbol.ToString(),
                                                                 Released.DescriptorSymbol.ToString(),
                                                                 Modified.DescriptorSymbol.ToString() };

      [Test]
      public void TestRemoteCoordinatorSenderSignalsTransactionManagerFromSessionWhenEnabled()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .WithRole(Role.Sender.ToBoolean())
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);
         Assert.AreSame(transactionManager.Session, session);

         ITransactionManager manager = transactionManager;

         Assert.IsFalse(manager.IsLocallyClosed);
         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsTrue(manager.IsLocallyClosed);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCloseRemotelyInitiatedTxnManagerWithErrorCondition()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .WithRole(Role.Sender.ToBoolean())
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ErrorCondition condition = new ErrorCondition(AmqpError.NOT_IMPLEMENTED, "Transactions are not supported");
         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource(Is.NullValue())
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectDetach().WithError(condition.Condition.ToString(), condition.Description).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);
         Assert.AreSame(transactionManager.Session, session);

         ITransactionManager manager = transactionManager;

         Assert.IsFalse(manager.IsLocallyClosed);
         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = null;
         manager.Open();
         manager.ErrorCondition = condition;
         manager.Close();

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNotNull(manager.ErrorCondition);
         Assert.IsTrue(manager.IsLocallyClosed);
         Assert.IsTrue(manager.IsRemotelyClosed);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionManagerAlertedIfParentSessionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .WithRole(Role.Sender.ToBoolean())
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);
         Assert.AreSame(transactionManager.Session, session);

         bool parentClosed = false;

         ITransactionManager manager = transactionManager;
         manager.ParentEndpointClosedHandler((txnMgr) => parentClosed = true);

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();

         session.Close();

         Assert.IsTrue(parentClosed);

         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionManagerAlertedIfParentConnectionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .WithRole(Role.Sender.ToBoolean())
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectClose().Respond();

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);
         Assert.AreSame(transactionManager.Session, session);

         bool parentClosed = false;

         ITransactionManager manager = transactionManager;
         manager.ParentEndpointClosedHandler((txnMgr) => parentClosed = true);

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();

         Assert.IsNotNull(manager.Source);
         Assert.IsNotNull(manager.Coordinator);

         manager.Open();

         connection.Close();

         Assert.IsTrue(parentClosed);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionManagerAlertedIfEngineShutdown()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .WithRole(Role.Sender.ToBoolean())
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);
         Assert.AreSame(transactionManager.Session, session);

         bool engineShutdown = false;

         ITransactionManager manager = transactionManager;
         manager.EngineShutdownHandler((theEngine) => engineShutdown = true);

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();

         engine.Shutdown();

         Assert.IsTrue(engineShutdown);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestRemoteCoordinatorSenderSignalsTransactionManagerFromConnectionWhenEnabled()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .WithRole(Role.Sender.ToBoolean())
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         connection.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         ITransactionManager manager = transactionManager;

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionManagerSignalsTxnDeclarationAndDischargeSucceeds()
      {
         DoTestTransactionManagerSignalsTxnDeclarationAndDischarge(false);
      }

      [Test]
      public void TestTransactionManagerSignalsTxnDeclarationAndDischargeFailed()
      {
         DoTestTransactionManagerSignalsTxnDeclarationAndDischarge(true);
      }

      private void DoTestTransactionManagerSignalsTxnDeclarationAndDischarge(bool txnFailed)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         bool txnRolledBack = false;
         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithLinkCredit(2);

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         ITransactionManager manager = transactionManager;

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();
         manager.DeclareHandler(declared =>
         {
            manager.Declared(declared, ProtonByteBufferAllocator.Instance.Wrap(TXN_ID));
         });
         manager.DischargeHandler(discharged =>
         {
            txnRolledBack = discharged.DischargeState.Equals(DischargeState.Rollback);
            manager.Discharged(discharged);
         });
         manager.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.RemoteDischarge().WithTxnId(TXN_ID)
                               .WithFail(txnFailed)
                               .WithDeliveryId(1)
                               .WithDeliveryTag(new byte[] { 1 }).Queue();
         peer.ExpectDisposition().WithState().Accepted();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         if (txnFailed)
         {
            Assert.IsTrue(txnRolledBack);
         }
         else
         {
            Assert.IsFalse(txnRolledBack);
         }
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionManagerSignalsTxnDeclarationFailed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithLinkCredit(1);

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         ITransactionManager manager = transactionManager;
         ErrorCondition failureError =
            new ErrorCondition(TransactionError.TRANSACTION_TIMEOUT, "Transaction timed out");

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();
         manager.DeclareHandler(declared =>
         {
            manager.DeclareFailed(declared, failureError);
         });
         manager.AddCredit(1);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Rejected(failureError.Condition.ToString(), failureError.Description);
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionManagerSignalsTxnDischargeFailed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithLinkCredit(2);

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         ITransactionManager manager = transactionManager;
         ErrorCondition failureError =
            new ErrorCondition(TransactionError.TRANSACTION_TIMEOUT, "Transaction timed out");

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();
         manager.DeclareHandler(declared =>
         {
            manager.Declared(declared, ProtonByteBufferAllocator.Instance.Wrap(TXN_ID));
         });
         manager.DischargeHandler(discharged =>
         {
            manager.DischargeFailed(discharged, failureError);
         });
         manager.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.RemoteDischarge().WithTxnId(TXN_ID).WithFail(false).WithDeliveryId(1).WithDeliveryTag(new byte[] { 1 }).Queue();
         peer.ExpectDisposition().WithState().Rejected(failureError.Condition.ToString(), failureError.Description);
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         Assert.AreEqual(0, manager.Credit);

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestManagerChecksDeclaredArgumentsForSomeCorrectness()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithLinkCredit(1);

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         ITransactionManager manager = transactionManager;
         ITransaction<ITransactionManager> txn = null;

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();
         manager.DeclareHandler(declared =>
         {
            txn = declared;
         });
         manager.AddCredit(1);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         Assert.IsNotNull(txn);

         Assert.Throws<ArgumentException>(() => manager.Declared(txn, ProtonByteBufferAllocator.Instance.Wrap(new byte[0])));
         Assert.Throws<NullReferenceException>(() => manager.Declared(txn, (byte[])null));
         Assert.Throws<ArgumentException>(() => manager.Declared(txn, new byte[0]));

         manager.Declared(txn, ProtonByteBufferAllocator.Instance.Wrap(TXN_ID));

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestManagerIgnoresAbortedTransfers()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithLinkCredit(1);

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         ITransactionManager manager = transactionManager;
         ITransaction<ITransactionManager> txn = null;
         int declareCounter = 0;

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();
         manager.DeclareHandler(declared =>
         {
            declareCounter++;
            txn = declared;
         });
         manager.AddCredit(1);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames aborting first attempt and then getting it right.
         peer.RemoteDeclare().WithMore(true).WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();
         peer.RemoteTransfer().WithDeliveryId(0).WithAborted(true).Now();
         peer.RemoteDeclare().WithDeliveryId(1).WithDeliveryTag(new byte[] { 1 }).Now();

         Assert.IsNotNull(txn);
         Assert.AreEqual(1, declareCounter);

         manager.Declared(txn, ProtonByteBufferAllocator.Instance.Wrap(TXN_ID));

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSignalDeclaredFromAnotherTransactionManager()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link-1")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();
         peer.RemoteAttach().WithName("TXN-Link-2")
                            .WithHandle(1)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager1 = null;
         ITransactionManager transactionManager2 = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            if (transactionManager1 == null)
            {
               transactionManager1 = manager;
            }
            else
            {
               transactionManager2 = manager;
            }
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(0)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(0).WithLinkCredit(2);
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(1)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(1).WithLinkCredit(2);

         Assert.IsNotNull(transactionManager1);
         Assert.IsNotNull(transactionManager1.RemoteCoordinator);
         Assert.IsNotNull(transactionManager2);
         Assert.IsNotNull(transactionManager2.RemoteCoordinator);

         ITransactionManager manager1 = transactionManager1;
         ITransactionManager manager2 = transactionManager2;

         ITransaction<ITransactionManager> txn = null;

         manager1.Coordinator = manager1.RemoteCoordinator.Copy();
         manager1.Source = manager1.RemoteSource.Copy();
         manager1.Open();
         manager1.DeclareHandler(declared =>
         {
            txn = declared;
         });
         manager1.DischargeHandler(discharged =>
         {
            manager1.Discharged(discharged);
         });
         manager1.AddCredit(2);

         // Put number two into a valid state as well.
         manager2.Coordinator = manager1.RemoteCoordinator.Copy();
         manager2.Source = manager1.RemoteSource.Copy();
         manager2.Open();
         manager2.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.RemoteDischarge().WithHandle(0).WithTxnId(TXN_ID).WithFail(false).WithDeliveryId(1).WithDeliveryTag(new byte[] { 1 }).Queue();
         peer.ExpectDisposition().WithState().Accepted();
         peer.ExpectDetach().WithHandle(0).Respond();
         peer.ExpectDetach().WithHandle(1).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithHandle(0).WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         Assert.IsNotNull(txn);

         Assert.Throws<ArgumentException>(() => manager2.Declared(txn, ProtonByteBufferAllocator.Instance.Wrap(TXN_ID)));

         manager1.Declared(txn, ProtonByteBufferAllocator.Instance.Wrap(TXN_ID));

         manager1.Close();
         manager2.Close();

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSignalDeclareFailedFromAnotherTransactionManager()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link-1")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();
         peer.RemoteAttach().WithName("TXN-Link-2")
                            .WithHandle(1)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager1 = null;
         ITransactionManager transactionManager2 = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            if (transactionManager1 == null)
            {
               transactionManager1 = manager;
            }
            else
            {
               transactionManager2 = manager;
            }
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(0)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(0).WithLinkCredit(2);
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(1)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(1).WithLinkCredit(2);

         Assert.IsNotNull(transactionManager1);
         Assert.IsNotNull(transactionManager1.RemoteCoordinator);
         Assert.IsNotNull(transactionManager2);
         Assert.IsNotNull(transactionManager2.RemoteCoordinator);

         ITransactionManager manager1 = transactionManager1;
         ITransactionManager manager2 = transactionManager2;

         ErrorCondition failureError =
            new ErrorCondition(TransactionError.UNKNOWN_ID, "Transaction unknown for some reason");

         ITransaction<ITransactionManager> txn = null;

         manager1.Coordinator = manager1.RemoteCoordinator.Copy();
         manager1.Source = manager1.RemoteSource.Copy();
         manager1.Open();
         manager1.DeclareHandler(declared =>
         {
            txn = declared;
         });
         manager1.AddCredit(2);

         // Put number two into a valid state as well.
         manager2.Coordinator = manager1.RemoteCoordinator.Copy();
         manager2.Source = manager1.RemoteSource.Copy();
         manager2.Open();
         manager2.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Rejected(failureError.Condition.ToString(), failureError.Description);
         peer.ExpectDetach().WithHandle(0).Respond();
         peer.ExpectDetach().WithHandle(1).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithHandle(0).WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         Assert.IsNotNull(txn);

         Assert.Throws<ArgumentException>(() => manager2.DeclareFailed(txn, failureError));

         manager1.DeclareFailed(txn, failureError);

         manager1.Close();
         manager2.Close();

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSignalDischargedFromAnotherTransactionManager()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link-1")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();
         peer.RemoteAttach().WithName("TXN-Link-2")
                            .WithHandle(1)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager1 = null;
         ITransactionManager transactionManager2 = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            if (transactionManager1 == null)
            {
               transactionManager1 = manager;
            }
            else
            {
               transactionManager2 = manager;
            }
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(0)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(0).WithLinkCredit(2);
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(1)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(1).WithLinkCredit(2);

         Assert.IsNotNull(transactionManager1);
         Assert.IsNotNull(transactionManager1.RemoteCoordinator);
         Assert.IsNotNull(transactionManager2);
         Assert.IsNotNull(transactionManager2.RemoteCoordinator);

         ITransactionManager manager1 = transactionManager1;
         ITransactionManager manager2 = transactionManager2;

         ITransaction<ITransactionManager> txn = null;

         manager1.Coordinator = manager1.RemoteCoordinator.Copy();
         manager1.Source = manager1.RemoteSource.Copy();
         manager1.Open();
         manager1.DeclareHandler(declared =>
         {
            txn = declared;
            manager1.Declared(declared, TXN_ID);
         });
         manager1.AddCredit(2);

         // Put number two into a valid state as well.
         manager2.Coordinator = manager1.RemoteCoordinator.Copy();
         manager2.Source = manager1.RemoteSource.Copy();
         manager2.Open();
         manager2.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.RemoteDischarge().WithHandle(0).WithTxnId(TXN_ID).WithFail(false).WithDeliveryId(1).WithDeliveryTag(new byte[] { 1 }).Queue();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithHandle(0).WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         Assert.IsNotNull(txn);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Accepted();
         peer.ExpectDetach().WithHandle(0).Respond();
         peer.ExpectDetach().WithHandle(1).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.Throws<ArgumentException>(() => manager2.Discharged(txn));

         manager1.Discharged(txn);

         manager1.Close();
         manager2.Close();

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSignalDischargeFailedFromAnotherTransactionManager()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link-1")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();
         peer.RemoteAttach().WithName("TXN-Link-2")
                            .WithHandle(1)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager1 = null;
         ITransactionManager transactionManager2 = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            if (transactionManager1 == null)
            {
               transactionManager1 = manager;
            }
            else
            {
               transactionManager2 = manager;
            }
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(0)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(0).WithLinkCredit(2);
         peer.ExpectAttach().OfReceiver()
                            .WithHandle(1)
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithHandle(1).WithLinkCredit(2);

         Assert.IsNotNull(transactionManager1);
         Assert.IsNotNull(transactionManager1.RemoteCoordinator);
         Assert.IsNotNull(transactionManager2);
         Assert.IsNotNull(transactionManager2.RemoteCoordinator);

         ITransactionManager manager1 = transactionManager1;
         ITransactionManager manager2 = transactionManager2;

         ErrorCondition failureError =
            new ErrorCondition(TransactionError.UNKNOWN_ID, "Transaction unknown for some reason");

         ITransaction<ITransactionManager> txn = null;

         manager1.Coordinator = manager1.RemoteCoordinator.Copy();
         manager1.Source = manager1.RemoteSource.Copy();
         manager1.Open();
         manager1.DeclareHandler(declared =>
         {
            txn = declared;
            manager1.Declared(declared, TXN_ID);
         });
         manager1.AddCredit(2);

         // Put number two into a valid state as well.
         manager2.Coordinator = manager1.RemoteCoordinator.Copy();
         manager2.Source = manager1.RemoteSource.Copy();
         manager2.Open();
         manager2.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.RemoteDischarge().WithHandle(0).WithTxnId(TXN_ID).WithFail(false).WithDeliveryId(1).WithDeliveryTag(new byte[] { 1 }).Queue();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithHandle(0).WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         Assert.IsNotNull(txn);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Rejected(failureError.Condition.ToString(), failureError.Description);
         peer.ExpectDetach().WithHandle(0).Respond();
         peer.ExpectDetach().WithHandle(1).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.Throws<ArgumentException>(() => manager2.DischargeFailed(txn, failureError));

         manager1.DischargeFailed(txn, failureError);

         manager1.Close();
         manager2.Close();

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionManagerRejectsAttemptedDischargeOfUnknownTxnId()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] TXN_ID = new byte[] { 0, 1, 2, 3 };
         byte[] TXN_ID_UNKNOWN = new byte[] { 3, 2, 1, 0 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithLinkCredit(2);

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         ITransactionManager manager = transactionManager;
         ErrorCondition failureError =
            new ErrorCondition(TransactionError.UNKNOWN_ID, "Transaction Manager is not tracking the given transaction ID.");

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();
         manager.DeclareHandler(declared =>
         {
            manager.Declared(declared, ProtonByteBufferAllocator.Instance.Wrap(TXN_ID));
         });
         manager.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithState().Declared(TXN_ID);
         peer.RemoteDischarge().WithTxnId(TXN_ID_UNKNOWN).WithFail(false).WithDeliveryId(1).WithDeliveryTag(new byte[] { 1 }).Queue();
         peer.ExpectDisposition().WithState().Rejected(failureError.Condition.ToString(), failureError.Description);
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Starts the flow of Transaction frames
         peer.RemoteDeclare().WithDeliveryId(0).WithDeliveryTag(new byte[] { 0 }).Now();

         Assert.AreEqual(0, manager.Credit);

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineFailedIfNonTxnRelatedTransferArrivesAtCoordinator()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = CreateEncodedMessage(new AmqpValue("test"));

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("TXN-Link")
                            .WithHandle(0)
                            .OfSender()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithInitialDeliveryCount(0)
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();

         ITransactionManager transactionManager = null;
         session.TransactionManagerOpenedHandler(manager =>
         {
            transactionManager = manager;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectFlow().WithLinkCredit(2);

         Assert.IsNotNull(transactionManager);
         Assert.IsNotNull(transactionManager.RemoteCoordinator);

         ITransactionManager manager = transactionManager;

         Assert.AreEqual(TxnCapability.LOCAL_TXN, manager.RemoteCoordinator.Capabilities[0]);

         manager.Coordinator = manager.RemoteCoordinator.Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();
         manager.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectClose().WithError(Is.NotNullValue());
         // Send the invalid Transfer to trigger engine shutdown
         peer.RemoteTransfer().WithDeliveryTag(new byte[] { 0 })
                              .WithPayload(payload)
                              .WithMore(false)
                              .Now();

         Assert.IsTrue(engine.IsFailed);

         // The transfer write should trigger an error back into the peer which we can ignore.
         peer.WaitForScriptToCompleteIgnoreErrors();
         Assert.IsNotNull(failure);
      }
   }
}