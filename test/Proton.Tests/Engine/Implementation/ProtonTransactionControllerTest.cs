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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonTransactionControllerTest : ProtonEngineTestSupport
   {
      private Symbol[] DEFAULT_OUTCOMES = new Symbol[] { Accepted.DescriptorSymbol,
                                                         Rejected.DescriptorSymbol,
                                                         Released.DescriptorSymbol,
                                                         Modified.DescriptorSymbol };

      private String[] DEFAULT_OUTCOMES_STRINGS = new String[] { Accepted.DescriptorSymbol.ToString(),
                                                                 Rejected.DescriptorSymbol.ToString(),
                                                                 Released.DescriptorSymbol.ToString(),
                                                                 Modified.DescriptorSymbol.ToString() };

      [Test]
      public void TestTransactionControllerDeclaresTransaction()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString())
                            .And().Respond();
         peer.RemoteFlow().WithLinkCredit(1).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         Assert.AreSame(session, txnController.Session);

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         bool openedWithCoordinatorTarget = false;
         txnController.OpenHandler((result) =>
         {
            if (result.RemoteCoordinator is Coordinator)
            {
               openedWithCoordinatorTarget = true;
            }
         });

         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IProtonBuffer declaredTxnId = null;
         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
         });

         txnController.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare().Accept(TXN_ID);

         Assert.IsTrue(openedWithCoordinatorTarget);

         Assert.IsNotNull(txnController.Declare());

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(TXN_ID), declaredTxnId);

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionControllerSignalsWhenParentSessionClosed()
      {
         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(1).Queue();
         peer.ExpectDeclare().Accept(TXN_ID);
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         bool openedWithCoordinatorTarget = false;
         txnController.OpenHandler((result) =>
         {
            if (result.RemoteCoordinator is Coordinator)
            {
               openedWithCoordinatorTarget = true;
            }
         });

         IProtonBuffer declaredTxnId = null;
         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
         });

         bool parentEndpointClosed = false;
         txnController.ParentEndpointClosedHandler((controller) =>
         {
            parentEndpointClosed = true;
         });

         txnController.Open();
         txnController.Declare();

         Assert.IsTrue(openedWithCoordinatorTarget);

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(parentEndpointClosed);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionControllerSignalsWhenParentConnectionClosed()
      {
         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(1).Queue();
         peer.ExpectDeclare().Accept(TXN_ID);
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         bool openedWithCoordinatorTarget = false;
         txnController.OpenHandler((result) =>
         {
            if (result.RemoteCoordinator is Coordinator)
            {
               openedWithCoordinatorTarget = true;
            }
         });

         IProtonBuffer declaredTxnId = null;
         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
         });

         bool parentEndpointClosed = false;
         txnController.ParentEndpointClosedHandler((controller) =>
         {
            parentEndpointClosed = true;
         });

         txnController.Open();
         txnController.Declare();

         Assert.IsTrue(openedWithCoordinatorTarget);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(parentEndpointClosed);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionControllerSignalsWhenEngineShutdown()
      {
         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(1).Queue();
         peer.ExpectDeclare().Accept(TXN_ID);

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         bool openedWithCoordinatorTarget = false;
         txnController.OpenHandler((result) =>
         {
            if (result.RemoteCoordinator is Coordinator)
            {
               openedWithCoordinatorTarget = true;
            }
         });

         IProtonBuffer declaredTxnId = null;
         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
         });

         bool engineShutdown = false;
         txnController.EngineShutdownHandler((theEngine) =>
         {
            engineShutdown = true;
         });

         txnController.Open();
         txnController.Declare();

         Assert.IsTrue(openedWithCoordinatorTarget);

         engine.Shutdown();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(engineShutdown);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionControllerDoesNotSignalsWhenParentConnectionClosedIfAlreadyClosed()
      {
         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(1).Queue();
         peer.ExpectDeclare().Accept(TXN_ID);
         peer.ExpectDetach().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         bool openedWithCoordinatorTarget = false;
         txnController.OpenHandler((result) =>
         {
            if (result.RemoteCoordinator is Coordinator)
            {
               openedWithCoordinatorTarget = true;
            }
         });

         IProtonBuffer declaredTxnId = null;
         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
         });

         bool parentEndpointClosed = false;
         txnController.ParentEndpointClosedHandler((controller) =>
         {
            parentEndpointClosed = true;
         });

         txnController.Open();
         txnController.Declare();
         txnController.Close();

         Assert.IsTrue(openedWithCoordinatorTarget);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsFalse(parentEndpointClosed);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionControllerBeginCommitBeginRollback()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         byte[] TXN_ID1 = new byte[] { 1, 2, 3, 4 };
         byte[] TXN_ID2 = new byte[] { 2, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(4).Queue();
         peer.ExpectDeclare().Accept(TXN_ID1);
         peer.ExpectDischarge().WithFail(false).WithTxnId(TXN_ID1).Accept();
         peer.ExpectDeclare().Accept(TXN_ID2);
         peer.ExpectDischarge().WithFail(true).WithTxnId(TXN_ID2).Accept();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;
         txnController.Open();

         Assert.IsTrue(txnController.IsLocallyOpen);
         Assert.IsTrue(txnController.IsRemotelyOpen);
         Assert.IsFalse(txnController.IsLocallyClosed);
         Assert.IsFalse(txnController.IsRemotelyClosed);

         ITransaction<ITransactionController> txn1 = txnController.NewTransaction();
         ITransaction<ITransactionController> txn2 = txnController.NewTransaction();

         // Begin / Commit
         txnController.Declare(txn1);
         txnController.Discharge(txn1, false);

         // Begin / Rollback
         txnController.Declare(txn2);
         txnController.Discharge(txn2, true);

         txnController.Close();

         Assert.IsFalse(txnController.IsLocallyOpen);
         Assert.IsFalse(txnController.IsRemotelyOpen);
         Assert.IsTrue(txnController.IsLocallyClosed);
         Assert.IsTrue(txnController.IsRemotelyClosed);

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionControllerDeclareAndDischargeOneTransactionDirect()
      {
         DoTestTransactionControllerDeclareAndDischargeOneTransaction(false);
      }

      [Test]
      public void TestTransactionControllerDeclareAndDischargeOneTransactionInDirect()
      {
         DoTestTransactionControllerDeclareAndDischargeOneTransaction(true);
      }

      private void DoTestTransactionControllerDeclareAndDischargeOneTransaction(bool useNewTransactionAPI)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IProtonBuffer declaredTxnId = null;
         IProtonBuffer dischargedTxnId = null;

         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
            if (useNewTransactionAPI)
            {
               Assert.AreEqual(txnController, (ITransactionController)result.LinkedResource);
            }
            else
            {
               Assert.IsNull(result.LinkedResource);
            }
         });
         txnController.DischargedHandler((result) =>
         {
            dischargedTxnId = result.TxnId.Copy();
            if (useNewTransactionAPI)
            {
               Assert.AreEqual(txnController, (ITransactionController)result.LinkedResource);
            }
            else
            {
               Assert.IsNull(result.LinkedResource);
            }
         });

         txnController.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare().Accept(TXN_ID);

         ITransaction<ITransactionController> txn;
         if (useNewTransactionAPI)
         {
            txn = txnController.NewTransaction();
            txn.LinkedResource = txnController;
            Assert.AreEqual(TransactionState.Idle, txn.State);
            txnController.Declare(txn);
         }
         else
         {
            txn = txnController.Declare();
            Assert.IsNotNull(txn.Attachments);
            Assert.AreSame(txn.Attachments, txn.Attachments);
         }

         Assert.IsNotNull(txn);

         peer.WaitForScriptToComplete();
         peer.ExpectDischarge().WithTxnId(TXN_ID).WithFail(false).Accept();

         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(TXN_ID), declaredTxnId);

         txnController.Discharge(txn, false);

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(TXN_ID), dischargedTxnId);
         Assert.IsFalse(txn.IsFailed);

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionDeclareRejected()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         bool declareFailure = false;
         ITransaction<ITransactionController> failedTxn = null;

         ErrorCondition failureError =
            new ErrorCondition(AmqpError.INTERNAL_ERROR, "Cannot Declare Transaction at this time");

         txnController.DeclareFailedHandler((result) =>
         {
            declareFailure = true;
            failedTxn = result;
         });

         txnController.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare().Reject(AmqpError.INTERNAL_ERROR.ToString(), "Cannot Declare Transaction at this time");

         ITransaction<ITransactionController> txn = txnController.Declare();

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(declareFailure);
         Assert.AreSame(txn, failedTxn);
         Assert.AreEqual(TransactionState.DeclareFailed, txn.State);
         Assert.AreEqual(failureError, txn.Error);
         Assert.AreEqual(0, CountElements(txnController.Transactions));

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionDischargeRejected()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         bool dischargeFailure = false;
         ITransaction<ITransactionController> failedTxn = null;
         ErrorCondition failureError =
            new ErrorCondition(TransactionError.TRANSACTION_TIMEOUT, "Transaction timed out");

         txnController.DischargeFailedHandler((result) =>
         {
            dischargeFailure = true;
            failedTxn = result;
         });

         txnController.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare().Accept();

         ITransaction<ITransactionController> txn = txnController.Declare();

         peer.WaitForScriptToComplete();
         peer.ExpectDischarge().Reject(TransactionError.TRANSACTION_TIMEOUT.ToString(), "Transaction timed out");

         txnController.Discharge(txn, false);

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(dischargeFailure);
         Assert.AreSame(txn, failedTxn);
         Assert.AreEqual(TransactionState.DischargeFailed, txn.State);
         Assert.AreEqual(failureError, txn.Error);
         Assert.AreEqual(0, CountElements(txnController.Transactions));

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotDeclareTransactionFromOneControllerInAnother()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();

         ITransactionController txnController1 = session.Coordinator("test-coordinator-1");
         ITransactionController txnController2 = session.Coordinator("test-coordinator-2");

         txnController1.Source = source;
         txnController1.Coordinator = coordinator;
         txnController1.Open();

         txnController2.Source = source;
         txnController2.Coordinator = coordinator;
         txnController2.Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(txnController1.HasCapacity);
         Assert.IsTrue(txnController2.HasCapacity);

         ITransaction<ITransactionController> txn1 = txnController1.NewTransaction();
         ITransaction<ITransactionController> txn2 = txnController2.NewTransaction();

         try
         {
            txnController1.Declare(txn2);
            Assert.Fail("Should not be able to declare a transaction with TXN created from another controller");
         }
         catch (ArgumentException)
         {
            // Expected
         }

         try
         {
            txnController2.Declare(txn1);
            Assert.Fail("Should not be able to declare a transaction with TXN created from another controller");
         }
         catch (ArgumentException)
         {
            // Expected
         }

         Assert.AreEqual(1, CountElements(txnController1.Transactions));
         Assert.AreEqual(1, CountElements(txnController2.Transactions));

         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         txnController1.Close();
         txnController2.Close();

         session.Close();
         connection.Close();

         // Never discharged so they remain in the controller now
         Assert.AreEqual(1, CountElements(txnController1.Transactions));
         Assert.AreEqual(1, CountElements(txnController2.Transactions));

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotDischargeTransactionFromOneControllerInAnother()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();

         ITransactionController txnController1 = session.Coordinator("test-coordinator-1");
         ITransactionController txnController2 = session.Coordinator("test-coordinator-2");

         txnController1.Source = source;
         txnController1.Coordinator = coordinator;
         txnController1.Open();

         txnController2.Source = source;
         txnController2.Coordinator = coordinator;
         txnController2.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare().Accept();
         peer.ExpectDeclare().Accept();

         Assert.IsTrue(txnController1.HasCapacity);
         Assert.IsTrue(txnController2.HasCapacity);

         ITransaction<ITransactionController> txn1 = txnController1.Declare();
         ITransaction<ITransactionController> txn2 = txnController2.Declare();

         peer.WaitForScriptToComplete();

         try
         {
            txnController1.Discharge(txn2, false);
            Assert.Fail("Should not be able to discharge a transaction with TXN created from another controller");
         }
         catch (ArgumentException)
         {
            // Expected
         }

         try
         {
            txnController2.Discharge(txn1, false);
            Assert.Fail("Should not be able to discharge a transaction with TXN created from another controller");
         }
         catch (ArgumentException)
         {
            // Expected
         }

         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         txnController1.Close();
         txnController2.Close();

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSendMessageInsideOfTransaction()
      {
         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };
         byte[] payloadBuffer = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(payloadBuffer);

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithLinkCredit(1).Queue();
         peer.ExpectCoordinatorAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();
         peer.ExpectDeclare().Accept(TXN_ID);

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test").Open();

         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;
         txnController.Open();

         ITransaction<ITransactionController> txn = txnController.Declare();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithHandle(0)
                              .WithNonNullPayload()
                              .WithState().Transactional().WithTxnId(TXN_ID).And()
                              .Respond()
                              .WithState().Transactional().WithTxnId(TXN_ID).WithAccepted().And()
                              .WithSettled(true);
         peer.ExpectDischarge().WithFail(false).WithTxnId(TXN_ID).Accept();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(sender.IsSendable);

         IOutgoingDelivery delivery = sender.Next();

         TransactionalState txnState = new TransactionalState();
         txnState.TxnId = ProtonByteBufferAllocator.Instance.Wrap(TXN_ID);

         delivery.Disposition(txnState, false);
         delivery.WriteBytes(payload);

         Assert.AreEqual(1, CountElements(txnController.Transactions));

         txnController.Discharge(txn, false);

         Assert.AreEqual(0, CountElements(txnController.Transactions));

         Assert.IsNotNull(delivery);
         Assert.IsNotNull(delivery.RemoteState);
         Assert.AreEqual(delivery.RemoteState.Type, DeliveryStateType.Transactional);
         Assert.IsNotNull(delivery.State);
         Assert.AreEqual(delivery.State.Type, DeliveryStateType.Transactional);
         Assert.IsFalse(delivery.IsSettled);

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
      }

      [Test]
      public void TestCommitTransactionAfterConnectionDropsFollowingTxnDeclared()
      {
         DischargeTransactionAfterConnectionDropsFollowingTxnDeclared(true);
      }

      [Test]
      public void TestRollbackTransactionAfterConnectionDropsFollowingTxnDeclared()
      {
         DischargeTransactionAfterConnectionDropsFollowingTxnDeclared(false);
      }

      public void DischargeTransactionAfterConnectionDropsFollowingTxnDeclared(bool commit)
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectCoordinatorAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();
         peer.ExpectDeclare().Accept(txnId);
         peer.DropAfterLastHandler();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;
         txnController.Open();

         ITransaction<ITransactionController> txn = txnController.NewTransaction();

         txnController.AddCapacityAvailableHandler(controller =>
         {
            controller.Declare(txn);
         });

         peer.WaitForScriptToComplete();

         // The write that are triggered here should fail and throw an exception

         try
         {
            if (commit)
            {
               txnController.Discharge(txn, false);
            }
            else
            {
               txnController.Discharge(txn, true);
            }

            Assert.Fail("Should have failed to discharge transaction");
         }
         catch (EngineFailedException)
         {
            // Expected error as a simulated IO disconnect was requested
            // TODO LOG.info("Caught expected EngineFailedException on write of discharge", ex);
         }

         peer.WaitForScriptToComplete();
      }

      [Test]
      public void TestTransactionControllerSignalsHandlerWhenCreditAvailableDirect()
      {
         DoTestTransactionControllerSignalsHandlerWhenCreditAvailable(false);
      }

      [Test]
      public void TestTransactionControllerSignalsHandlerWhenCreditAvailableInDirect()
      {
         DoTestTransactionControllerSignalsHandlerWhenCreditAvailable(true);
      }

      private void DoTestTransactionControllerSignalsHandlerWhenCreditAvailable(bool useNewTransactionAPI)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IProtonBuffer declaredTxnId = null;
         IProtonBuffer dischargedTxnId = null;

         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
         });
         txnController.DischargedHandler((result) =>
         {
            dischargedTxnId = result.TxnId.Copy();
         });

         txnController.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare().Accept(TXN_ID);

         ITransaction<ITransactionController> txn = null;

         if (useNewTransactionAPI)
         {
            txn = txnController.NewTransaction();
            try
            {
               txnController.Declare(txn);
               Assert.Fail("Should not be able to declare as there is no link credit to do so.");
            }
            catch (InvalidOperationException)
            {
            }

            txnController.AddCapacityAvailableHandler((controller) =>
            {
               txnController.Declare(txn);
            });
         }
         else
         {
            try
            {
               txnController.Declare();
               Assert.Fail("Should not be able to declare as there is no link credit to do so.");
            }
            catch (InvalidOperationException)
            {
            }

            txnController.AddCapacityAvailableHandler((controller) => {
               txn = txnController.Declare();
            });
         }

         peer.RemoteFlow().WithNextIncomingId(1).WithDeliveryCount(0).WithLinkCredit(1).Now();
         peer.WaitForScriptToComplete();
         peer.ExpectDischarge().WithTxnId(TXN_ID).WithFail(false).Accept();

         Assert.IsNotNull(txn);
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(TXN_ID) , declaredTxnId);

         try
         {
            txnController.Discharge(txn, false);
            Assert.Fail("Should not be able to discharge as there is no link credit to do so.");
         }
         catch (InvalidOperationException)
         {
         }

         txnController.AddCapacityAvailableHandler((controller) =>
         {
            txnController.Discharge(txn, false);
         });

         peer.RemoteFlow().WithNextIncomingId(2).WithDeliveryCount(1).WithLinkCredit(1).Now();
         peer.WaitForScriptToComplete();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(TXN_ID) , dischargedTxnId);

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCapacityAvailableHandlersAreQueuedAndNotifiedWhenCreditGranted()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         byte[] TXN_ID = new byte[] { 1, 2, 3, 4 };

         IProtonBuffer declaredTxnId = null;
         IProtonBuffer dischargedTxnId = null;

         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId=result.TxnId.Copy();
         });
         txnController.DischargedHandler((result) =>
         {
            dischargedTxnId=result.TxnId.Copy();
         });

         txnController.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare().Accept(TXN_ID);

         ITransaction<ITransactionController> txn = null;

         txnController.AddCapacityAvailableHandler((controller) =>
         {
            txn = txnController.Declare();
         });

         txnController.AddCapacityAvailableHandler((controller) =>
         {
            txnController.Discharge(txn, false);
         });

         peer.RemoteFlow().WithNextIncomingId(1).WithDeliveryCount(0).WithLinkCredit(1).Now();
         peer.WaitForScriptToComplete();

         Assert.IsTrue(txn.IsDeclared);

         peer.ExpectDischarge().WithTxnId(TXN_ID).WithFail(false).Accept();

         Assert.IsNotNull(txn);
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(TXN_ID) , declaredTxnId);

         peer.RemoteFlow().WithNextIncomingId(2).WithDeliveryCount(1).WithLinkCredit(1).Now();
         peer.WaitForScriptToComplete();
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(txn.IsDischarged);
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(TXN_ID) , dischargedTxnId);

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionControllerDeclareIsIdempotent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(3).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         IProtonBuffer declaredTxnId = null;

         txnController.DeclaredHandler((result) =>
         {
            declaredTxnId = result.TxnId.Copy();
         });

         txnController.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDeclare();

         ITransaction<ITransactionController> txn = txnController.NewTransaction();

         txnController.Declare(txn);

         Assert.IsFalse(txn.IsDeclared);  // No response yet
         Assert.IsFalse(txn.IsDischarged);

         try
         {
            txnController.Declare(txn);
            Assert.Fail("Should not be able to declare the same transaction a second time.");
         }
         catch (InvalidOperationException)
         {
         }

         try
         {
            Assert.AreEqual(txn.State, TransactionState.Declaring);
            txnController.Discharge(txn, false);
            Assert.Fail("Should not be able to discharge a transaction that is not activated by the remote.");
         }
         catch (InvalidOperationException)
         {
         }

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionDeclareRejectedWithNoHandlerRegistered()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();
         peer.ExpectDeclare().Reject(AmqpError.INTERNAL_ERROR.ToString(), "Cannot Declare Transaction at this time");
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         ErrorCondition failureError =
            new ErrorCondition(AmqpError.INTERNAL_ERROR, "Cannot Declare Transaction at this time");

         txnController.Open();

         ITransaction<ITransactionController> txn = txnController.Declare();

         Assert.IsNotNull(txn.Error);
         Assert.AreEqual(TransactionState.DeclareFailed, txn.State);
         Assert.AreEqual(failureError, txn.Error);
         Assert.AreEqual(0, CountElements(txnController.Transactions));

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTransactionDischargeRejectedWithNoHandlerRegistered()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Source source = new Source();
         source.Outcomes = DEFAULT_OUTCOMES;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString()).And().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();
         peer.ExpectDeclare().Accept();
         peer.ExpectDischarge().Reject(TransactionError.TRANSACTION_TIMEOUT.ToString(), "Transaction timed out");
         peer.ExpectDetach().WithClosed(true).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ITransactionController txnController = session.Coordinator("test-coordinator");

         txnController.Source = source;
         txnController.Coordinator = coordinator;

         txnController.Open();

         ITransaction<ITransactionController> txn = txnController.Declare();
         ErrorCondition failureError =
            new ErrorCondition(TransactionError.TRANSACTION_TIMEOUT, "Transaction timed out");

         txnController.Discharge(txn, false);

         Assert.IsNotNull(txn.Error);
         Assert.AreEqual(TransactionState.DischargeFailed, txn.State);
         Assert.AreEqual(failureError, txn.Error);
         Assert.AreEqual(0, CountElements(txnController.Transactions));
         Assert.IsTrue(txn.IsFailed);

         txnController.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }
   }
}