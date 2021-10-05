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
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonTransactionLinkTest : ProtonEngineTestSupport
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
      public void TestCreateDefaultCoordinatorSender()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Coordinator coordinator = new Coordinator();
         Source source = new Source();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectCoordinatorAttach().Respond();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test-coordinator");

         sender.Source = source;
         sender.Coordinator = coordinator;

         sender.Open();
         sender.Detach();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCreateCoordinatorSender()
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
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test-coordinator");

         sender.Source = source;
         sender.Coordinator = coordinator;

         bool openedWithCoordinatorTarget = false;
         sender.OpenHandler((result) =>
         {
            if (result.RemoteTerminus is Coordinator)
            {
               openedWithCoordinatorTarget = true;
            }
         });

         sender.Open();

         Assert.IsTrue(openedWithCoordinatorTarget);

         Coordinator remoteCoordinator = (Coordinator)sender.RemoteTerminus;

         Assert.AreEqual(TxnCapability.LOCAL_TXN, remoteCoordinator.Capabilities[0]);

         sender.Detach();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestRemoteCoordinatorTriggersSenderCreateWhenManagerHandlerNotSet()
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

         IReceiver transactionReceiver = null;
         session.ReceiverOpenHandler(txnReceiver =>
         {
            transactionReceiver = txnReceiver;
         });

         session.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithOutcomes(DEFAULT_OUTCOMES_STRINGS).And()
                            .WithCoordinator().WithCapabilities(TxnCapability.LOCAL_TXN.ToString());
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IReceiver manager = transactionReceiver;

         Assert.IsNotNull(transactionReceiver);
         Assert.IsNotNull(transactionReceiver.RemoteTerminus);

         Assert.AreEqual(TxnCapability.LOCAL_TXN, ((Coordinator)manager.RemoteTerminus).Capabilities[0]);

         manager.Coordinator = ((Coordinator)manager.RemoteTerminus).Copy();
         manager.Source = manager.RemoteSource.Copy();
         manager.Open();

         manager.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }
   }
}