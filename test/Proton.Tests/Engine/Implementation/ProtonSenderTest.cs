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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;
using Is = Apache.Qpid.Proton.Test.Driver.Matchers.Is;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonSenderTest : ProtonEngineTestSupport
   {
      [Test]
      public void TestLocalLinkStateCannotBeChangedAfterOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test");

         sender.Properties = new Dictionary<Symbol, object>();

         sender.Open();

         try
         {
            sender.Properties = new Dictionary<Symbol, object>();
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            sender.DesiredCapabilities = new Symbol[] { AmqpError.DECODE_ERROR };
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            sender.OfferedCapabilities = new Symbol[] { AmqpError.DECODE_ERROR };
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            sender.SenderSettleMode = SenderSettleMode.Mixed;
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            sender.Source = new Source();
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            sender.Target = new Target();
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            sender.MaxMessageSize = 0u;
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         sender.Detach();
         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderEmitsOpenAndCloseEvents()
      {
         DoTestSenderEmitsEvents(false);
      }

      [Test]
      public void TestSenderEmitsOpenAndDetachEvents()
      {
         DoTestSenderEmitsEvents(true);
      }

      private void DoTestSenderEmitsEvents(bool detach)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool senderLocalOpen = false;
         bool senderLocalClose = false;
         bool senderLocalDetach = false;
         bool senderRemoteOpen = false;
         bool senderRemoteClose = false;
         bool senderRemoteDetach = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("test");
         sender.LocalOpenHandler((result) => senderLocalOpen = true)
               .LocalCloseHandler((result) => senderLocalClose = true)
               .LocalDetachHandler((result) => senderLocalDetach = true)
               .OpenHandler((result) => senderRemoteOpen = true)
               .DetachHandler((result) => senderRemoteDetach = true)
               .CloseHandler((result) => senderRemoteClose = true);

         sender.Open();

         Assert.IsNull(sender.DeliveryTagGenerator);

         if (detach)
         {
            sender.Detach();
         }
         else
         {
            sender.Close();
         }

         Assert.IsTrue(senderLocalOpen, "Sender should have reported local open");
         Assert.IsTrue(senderRemoteOpen, "Sender should have reported remote open");

         if (detach)
         {
            Assert.IsFalse(senderLocalClose, "Sender should not have reported local close");
            Assert.IsTrue(senderLocalDetach, "Sender should have reported local detach");
            Assert.IsFalse(senderRemoteClose, "Sender should not have reported remote close");
            Assert.IsTrue(senderRemoteDetach, "Sender should have reported remote close");
         }
         else
         {
            Assert.IsTrue(senderLocalClose, "Sender should have reported local close");
            Assert.IsFalse(senderLocalDetach, "Sender should not have reported local detach");
            Assert.IsTrue(senderRemoteClose, "Sender should have reported remote close");
            Assert.IsFalse(senderRemoteDetach, "Sender should not have reported remote close");
         }

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderRoutesDetachEventToCloseHandlerIfNonSset()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool senderLocalOpen = false;
         bool senderLocalClose = false;
         bool senderRemoteOpen = false;
         bool senderRemoteClose = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("test");
         sender.LocalOpenHandler((result) => senderLocalOpen = true)
               .LocalCloseHandler((result) => senderLocalClose = true)
               .OpenHandler((result) => senderRemoteOpen = true)
               .CloseHandler((result) => senderRemoteClose = true);

         sender.Open();
         sender.Detach();

         Assert.IsTrue(senderLocalOpen, "Sender should have reported local open");
         Assert.IsTrue(senderRemoteOpen, "Sender should have reported remote open");
         Assert.IsTrue(senderLocalClose, "Sender should have reported local detach");
         Assert.IsTrue(senderRemoteClose, "Sender should have reported remote detach");

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderEnforcesOneActiveDeliveryAtNextAPI()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test").Open();

         Assert.IsNotNull(sender.Next);

         IOutgoingDelivery delivery;

         Assert.Throws<InvalidOperationException>(() => delivery = sender.Next);

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderReceivesParentSessionClosedEvent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool parentClosed = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("test");
         sender.ParentEndpointClosedHandler((result) => parentClosed = true);

         sender.Open();

         session.Close();

         Assert.IsTrue(parentClosed, "Sender should have reported parent session closed");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderReceivesParentConnectionClosedEvent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool parentClosed = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("test");
         sender.ParentEndpointClosedHandler((result) => parentClosed = true);

         sender.Open();

         connection.Close();

         Assert.IsTrue(parentClosed, "Sender should have reported parent connection closed");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineShutdownEventNeitherEndClosed()
      {
         DoTestEngineShutdownEvent(false, false);
      }

      [Test]
      public void TestEngineShutdownEventLocallyClosed()
      {
         DoTestEngineShutdownEvent(true, false);
      }

      [Test]
      public void TestEngineShutdownEventRemotelyClosed()
      {
         DoTestEngineShutdownEvent(false, true);
      }

      [Test]
      public void TestEngineShutdownEventBothEndsClosed()
      {
         DoTestEngineShutdownEvent(true, true);
      }

      private void DoTestEngineShutdownEvent(bool locallyClosed, bool remotelyClosed)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool engineShutdown = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start();

         connection.Open();

         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("test");
         sender.Open();
         sender.EngineShutdownHandler((result) => engineShutdown = true);

         if (locallyClosed)
         {
            if (remotelyClosed)
            {
               peer.ExpectDetach().Respond();
            }
            else
            {
               peer.ExpectDetach();
            }

            sender.Close();
         }

         if (remotelyClosed && !locallyClosed)
         {
            peer.RemoteDetach();
         }

         engine.Shutdown();

         if (locallyClosed && remotelyClosed)
         {
            Assert.IsFalse(engineShutdown, "Should not have reported engine shutdown");
         }
         else
         {
            Assert.IsTrue(engineShutdown, "Should have reported engine shutdown");
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderOpenWithNoSenderOrReceiverSettleModes()
      {
         DoTestOpenSenderWithConfiguredSenderAndReceiverSettlementModes(null, null);
      }

      [Test]
      public void TestSenderOpenWithSettledAndFirst()
      {
         DoTestOpenSenderWithConfiguredSenderAndReceiverSettlementModes(SenderSettleMode.Settled, ReceiverSettleMode.First);
      }

      [Test]
      public void TestSenderOpenWithUnsettledAndSecond()
      {
         DoTestOpenSenderWithConfiguredSenderAndReceiverSettlementModes(SenderSettleMode.Unsettled, ReceiverSettleMode.Second);
      }

      private void DoTestOpenSenderWithConfiguredSenderAndReceiverSettlementModes(SenderSettleMode? senderMode, ReceiverSettleMode? receiverMode)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender()
                            .WithSndSettleMode(((byte?)senderMode))
                            .WithRcvSettleMode(((byte?)receiverMode))
                            .Respond()
                            .WithSndSettleMode(((byte?)senderMode))
                            .WithRcvSettleMode(((byte?)receiverMode));

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender");
         if (senderMode.HasValue)
         {
            sender.SenderSettleMode = (SenderSettleMode)senderMode;
         }
         if (receiverMode.HasValue)
         {
            sender.ReceiverSettleMode = (ReceiverSettleMode)receiverMode;
         }
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().Respond();

         if (senderMode != null)
         {
            Assert.AreEqual(senderMode, sender.SenderSettleMode);
         }
         else
         {
            Assert.AreEqual(SenderSettleMode.Mixed, sender.SenderSettleMode);
         }
         if (receiverMode != null)
         {
            Assert.AreEqual(receiverMode, sender.ReceiverSettleMode);
         }
         else
         {
            Assert.AreEqual(ReceiverSettleMode.First, sender.ReceiverSettleMode);
         }

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderOpenAndCloseAreIdempotent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("test");
         sender.Open();

         // Should not emit another attach frame
         sender.Open();

         sender.Close();

         // Should not emit another detach frame
         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCreateSenderAndClose()
      {
         DoTestCreateSenderAndCloseOrDetachLink(true);
      }

      [Test]
      public void TestCreateSenderAndDetach()
      {
         DoTestCreateSenderAndCloseOrDetachLink(false);
      }

      private void DoTestCreateSenderAndCloseOrDetachLink(bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.ExpectDetach().WithClosed(close).Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("test");
         sender.Open();

         Assert.IsTrue(sender.IsSender);
         Assert.IsFalse(sender.IsReceiver);

         if (close)
         {
            sender.Close();
         }
         else
         {
            sender.Detach();
         }

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineEmitsAttachAfterLocalSenderOpened()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("test");
         sender.Open();
         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenBeginAttachBeforeRemoteResponds()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();
         peer.ExpectBegin();
         peer.ExpectAttach();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("test");
         sender.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderFireOpenedEventAfterRemoteAttachArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();

         bool senderRemotelyOpened = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("test");
         sender.OpenHandler((result) => senderRemotelyOpened = true);
         sender.Open();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderFireOpenedEventAfterRemoteAttachArrivesWithNullTarget()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond().WithNullTarget();
         peer.ExpectDetach().Respond();

         bool senderRemotelyOpened = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("test");
         sender.Source = new Source();
         sender.Target = new Target();
         sender.OpenHandler((result) => senderRemotelyOpened = true);
         sender.Open();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");
         Assert.IsNull(sender.RemoteTerminus);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenAndCloseMultipleSenders()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).Respond();
         peer.ExpectAttach().WithHandle(1).Respond();
         peer.ExpectDetach().WithHandle(1).Respond();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender1 = session.Sender("sender-1");
         sender1.Open();
         ISender sender2 = session.Sender("sender-2");
         sender2.Open();

         // Close in reverse order
         sender2.Close();
         sender1.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderFireClosedEventAfterRemoteDetachArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();

         bool senderRemotelyOpened = false;
         bool senderRemotelyClosed = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("test");
         sender.OpenHandler((result) => senderRemotelyOpened = true);
         sender.CloseHandler((result) => senderRemotelyClosed = true);
         sender.Open();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");

         sender.Close();

         Assert.IsTrue(senderRemotelyClosed, "Sender remote closed event did not fire");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderFireClosedEventAfterRemoteDetachArrivesBeforeLocalClose()
      {
         DoTestSenderFireEventAfterRemoteDetachArrivesBeforeLocalClose(true);
      }

      [Test]
      public void TestSenderFireDetachEventAfterRemoteDetachArrivesBeforeLocalClose()
      {
         DoTestSenderFireEventAfterRemoteDetachArrivesBeforeLocalClose(false);
      }

      private void DoTestSenderFireEventAfterRemoteDetachArrivesBeforeLocalClose(bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteDetach().WithClosed(close).Queue();

         bool senderRemotelyOpened = false;
         bool senderRemotelyClosed = false;
         bool senderRemotelyDetached = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("test");
         sender.OpenHandler((result) => senderRemotelyOpened = true);
         sender.CloseHandler((result) => senderRemotelyClosed = true);
         sender.DetachHandler((result) => senderRemotelyDetached = true);
         sender.Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");
         if (close)
         {
            Assert.IsTrue(senderRemotelyClosed, "Sender remote closed event did not fire");
            Assert.IsFalse(senderRemotelyDetached, "Sender remote detached event fired");
         }
         else
         {
            Assert.IsFalse(senderRemotelyClosed, "Sender remote closed event fired");
            Assert.IsTrue(senderRemotelyDetached, "Sender remote closed event did not fire");
         }

         peer.ExpectDetach().WithClosed(close);
         if (close)
         {
            sender.Close();
         }
         else
         {
            sender.Detach();
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestRemotelyCloseSenderAndOpenNewSenderImmediatelyAfterWithNewLinkName()
      {
         DoTestRemotelyTerminateLinkAndThenCreateNewLink(true, false);
      }

      [Test]
      public void TestRemotelyDetachSenderAndOpenNewSenderImmediatelyAfterWithNewLinkName()
      {
         DoTestRemotelyTerminateLinkAndThenCreateNewLink(false, false);
      }

      [Test]
      public void TestRemotelyCloseSenderAndOpenNewSenderImmediatelyAfterWithSameLinkName()
      {
         DoTestRemotelyTerminateLinkAndThenCreateNewLink(true, true);
      }

      [Test]
      public void TestRemotelyDetachSenderAndOpenNewSenderImmediatelyAfterWithSameLinkName()
      {
         DoTestRemotelyTerminateLinkAndThenCreateNewLink(false, true);
      }

      private void DoTestRemotelyTerminateLinkAndThenCreateNewLink(bool close, bool sameLinkName)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         String firstLinkName = "test-link-1";
         String secondLinkName = sameLinkName ? firstLinkName : "test-link-2";

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).OfSender().Respond();
         peer.RemoteDetach().WithClosed(close).Queue();

         bool senderRemotelyOpened = false;
         bool senderRemotelyClosed = false;
         bool senderRemotelyDetached = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender(firstLinkName);
         sender.OpenHandler((result) => senderRemotelyOpened = true);
         sender.CloseHandler((result) => senderRemotelyClosed = true);
         sender.DetachHandler((result) => senderRemotelyDetached = true);
         sender.Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");
         if (close)
         {
            Assert.IsTrue(senderRemotelyClosed, "Sender remote closed event did not fire");
            Assert.IsFalse(senderRemotelyDetached, "Sender remote detached event fired");
         }
         else
         {
            Assert.IsFalse(senderRemotelyClosed, "Sender remote closed event fired");
            Assert.IsTrue(senderRemotelyDetached, "Sender remote closed event did not fire");
         }

         peer.ExpectDetach().WithClosed(close);
         if (close)
         {
            sender.Close();
         }
         else
         {
            sender.Detach();
         }

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().WithHandle(0).OfSender().Respond();
         peer.ExpectDetach().WithClosed(close).Respond();

         // Reset trackers
         senderRemotelyOpened = false;
         senderRemotelyClosed = false;
         senderRemotelyDetached = false;

         sender = session.Sender(secondLinkName);
         sender.OpenHandler((result) => senderRemotelyOpened = true);
         sender.CloseHandler((result) => senderRemotelyClosed = true);
         sender.DetachHandler((result) => senderRemotelyDetached = true);
         sender.Open();

         if (close)
         {
            sender.Close();
         }
         else
         {
            sender.Detach();
         }

         peer.WaitForScriptToComplete();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");
         if (close)
         {
            Assert.IsTrue(senderRemotelyClosed, "Sender remote closed event did not fire");
            Assert.IsFalse(senderRemotelyDetached, "Sender remote detached event fired");
         }
         else
         {
            Assert.IsFalse(senderRemotelyClosed, "Sender remote closed event fired");
            Assert.IsTrue(senderRemotelyDetached, "Sender remote closed event did not fire");
         }

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionSignalsRemoteSenderOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().OfReceiver()
                            .WithName("receiver")
                            .WithHandle(0)
                            .WithInitialDeliveryCount(0)
                            .OnChannel(0).Queue();
         peer.ExpectAttach();
         peer.ExpectDetach().Respond();

         bool senderRemotelyOpened = false;
         ISender sender = null;

         IConnection connection = engine.Start();

         connection.SenderOpenedHandler((result) =>
         {
            senderRemotelyOpened = true;
            sender = result;
         });

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");

         sender.Open();
         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotOpenSenderAfterSessionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("test");

         session.Close();

         try
         {
            sender.Open();
            Assert.Fail("Should not be able to open a link from a closed session.");
         }
         catch (InvalidOperationException)
         {
         }

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotOpenSenderAfterSessionRemotelyClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.RemoteEnd().Queue();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         ISender sender = session.Sender("test");
         session.Open();

         try
         {
            sender.Open();
            Assert.Fail("Should not be able to open a link from a remotely closed session.");
         }
         catch (InvalidOperationException)
         {
         }

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestGetCurrentDeliveryFromSender()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).Respond();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");

         sender.Open();

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         Assert.IsFalse(delivery.IsAborted);
         Assert.IsTrue(delivery.IsPartial);
         Assert.IsFalse(delivery.IsSettled);
         Assert.IsFalse(delivery.IsRemotelySettled);

         // Always return same delivery until completed.
         Assert.AreSame(delivery, sender.Current);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderGetsCreditOnIncomingFlow()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");

         Assert.IsFalse(sender.IsSendable);

         sender.Open();

         Assert.AreEqual(10, sender.Credit);
         Assert.IsTrue(sender.IsSendable);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSendSmallPayloadWhenCreditAvailable()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payloadBuffer = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithPayload(payloadBuffer);
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(payloadBuffer);

         ISender sender = session.Sender("sender-1");

         Assert.IsFalse(sender.IsSendable);

         sender.CreditStateUpdateHandler((handler) =>
         {
            if (handler.IsSendable)
            {
               handler.Next.DeliveryTag = new DeliveryTag(new byte[] { 0 });
               handler.Current.WriteBytes(payload);
            }
         });

         sender.Open();
         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSendTransferWithNonDefaultMessageFormat()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payloadBuffer = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithMessageFormat(17).WithPayload(payloadBuffer);
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(payloadBuffer);

         ISender sender = session.Sender("sender-1");

         Assert.IsFalse(sender.IsSendable);

         sender.CreditStateUpdateHandler(handler =>
         {
            if (handler.IsSendable)
            {
               IOutgoingDelivery delivery = handler.Next;

               delivery.DeliveryTagBytes = new byte[] { 0 };
               delivery.MessageFormat = 17;
               delivery.WriteBytes(payload);
            }
         });

         sender.Open();
         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderSignalsDeliveryUpdatedOnSettledThenSettleFromLinkAPI()
      {
         DoTestSenderSignalsDeliveryUpdatedOnSettled(true);
      }

      [Test]
      public void TestSenderSignalsDeliveryUpdatedOnSettledThenSettleDelivery()
      {
         DoTestSenderSignalsDeliveryUpdatedOnSettled(false);
      }

      private void DoTestSenderSignalsDeliveryUpdatedOnSettled(bool settleFromLink)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithPayload(payload);
         peer.RemoteDisposition().WithSettled(true)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Accepted()
                                 .WithFirst(0).Queue();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");

         bool deliveryUpdatedAndSettled = false;
         IOutgoingDelivery updatedDelivery = null;
         sender.DeliveryStateUpdatedHandler(delivery =>
         {
            if (delivery.IsRemotelySettled)
            {
               deliveryUpdatedAndSettled = true;
            }

            updatedDelivery = delivery;
         });

         Assert.IsFalse(sender.IsSendable);

         sender.CreditStateUpdateHandler(handler =>
         {
            if (handler.IsSendable)
            {
               IOutgoingDelivery delivery = handler.Next;
               delivery.DeliveryTagBytes = new byte[] { 0 };
               delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
            }
         });

         sender.Open();

         Assert.IsTrue(deliveryUpdatedAndSettled, "Delivery should have been updated and state settled");
         Assert.AreEqual(Accepted.Instance, updatedDelivery.RemoteState);
         Assert.IsTrue(sender.HasUnsettled);
         int count = 0;
         foreach (IOutgoingDelivery unsettled in sender.Unsettled)
         {
            count++;
         }

         Assert.AreEqual(1, count);

         if (settleFromLink)
         {
            sender.Settle(delivery => true);
         }
         else
         {
            updatedDelivery.Settle();
         }

         Assert.IsFalse(sender.HasUnsettled);
         count = 0;
         foreach (IOutgoingDelivery unsettled in sender.Unsettled)
         {
            count++;
         }

         Assert.AreEqual(0, count);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenSenderBeforeOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Create the connection but don't open, then open a session and a sender and
         // the session begin and sender attach shouldn't go out until the connection
         // is opened locally.
         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("sender");
         sender.Open();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("sender").OfSender().Respond();

         // Now open the connection, expect the Open, Begin, and Attach frames
         connection.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenSenderBeforeOpenSession()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         // Create the connection and open it, then create a session and a sender
         // and observe that the sender doesn't send its attach until the session
         // is opened.
         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         ISender sender = session.Sender("sender");
         sender.Open();

         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("sender").OfSender().Respond();

         // Now open the session, expect the Begin, and Attach frames
         session.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderDetachAfterEndSent()
      {
         DoTestSenderClosedOrDetachedAfterEndSent(false);
      }

      [Test]
      public void TestSenderCloseAfterEndSent()
      {
         DoTestSenderClosedOrDetachedAfterEndSent(true);
      }

      public void DoTestSenderClosedOrDetachedAfterEndSent(bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("sender").WithRole(Role.Sender.ToBoolean()).Respond();
         peer.ExpectEnd().Respond();

         // Create the connection and open it, then create a session and a sender
         // and observe that the sender doesn't send its detach if the session has
         // already been closed.
         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("sender");
         sender.Open();

         // Causes the End frame to be sent
         session.Close();

         // The sender should not emit an end as the session was closed which implicitly
         // detached the link.
         if (close)
         {
            sender.Close();
         }
         else
         {
            sender.Detach();
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderDetachAfterCloseSent()
      {
         DoTestSenderClosedOrDetachedAfterCloseSent(false);
      }

      [Test]
      public void TestSenderCloseAfterCloseSent()
      {
         DoTestSenderClosedOrDetachedAfterCloseSent(true);
      }

      public void DoTestSenderClosedOrDetachedAfterCloseSent(bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("sender").OfSender().Respond();
         peer.ExpectClose().Respond();

         // Create the connection and open it, then create a session and a sender
         // and observe that the sender doesn't send its detach if the connection has
         // already been closed.
         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         ISender sender = session.Sender("sender");
         sender.Open();

         // Cause an Close frame to be sent
         connection.Close();

         // The sender should not emit an detach as the connection was closed which implicitly
         // detached the link.
         if (close)
         {
            sender.Close();
         }
         else
         {
            sender.Detach();
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestNoDispositionSentAfterDeliverySettledForSender()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 });
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });

         ISender sender = session.Sender("sender-1");

         bool deliverySentAfterSendable = false;
         IOutgoingDelivery sent = null;

         sender.CreditStateUpdateHandler(handler =>
         {
            if (handler.IsSendable)
            {
               sent = handler.Next;
               sent.DeliveryTagBytes = new byte[] { 0 };
               sent.WriteBytes(payload);
               deliverySentAfterSendable = true;
            }
         });

         sender.Open();

         Assert.IsTrue(deliverySentAfterSendable, "Delivery should have been sent after credit arrived");

         Assert.IsNull(sender.Current);

         sent.Disposition(Accepted.Instance, true);

         IOutgoingDelivery delivery2 = sender.Next;

         Assert.AreNotSame(delivery2, sent);
         delivery2.Disposition(Released.Instance, true);

         Assert.IsFalse(sender.HasUnsettled);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderCannotSendAfterConnectionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");

         Assert.IsFalse(sender.IsSendable);

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         sender.Open();

         Assert.AreEqual(10, sender.Credit);
         Assert.IsTrue(sender.IsSendable);

         connection.Close();

         Assert.IsFalse(sender.IsSendable);
         try
         {
            delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 }));
            Assert.Fail("Should not be able to write to delivery after connection closed.");
         }
         catch (InvalidOperationException)
         {
            // Should not allow writes on past delivery instances after connection closed
         }

         try
         {
            IOutgoingDelivery next = sender.Next;
            Assert.Fail("Should not be able get next after connection closed");
         }
         catch (InvalidOperationException)
         {
            // Should not allow next message after close of connection
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderCannotSendAfterSessionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");

         Assert.IsFalse(sender.IsSendable);

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         sender.Open();

         Assert.AreEqual(10, sender.Credit);
         Assert.IsTrue(sender.IsSendable);

         session.Close();

         Assert.IsFalse(sender.IsSendable);
         try
         {
            delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 }));
            Assert.Fail("Should not be able to write to delivery after session closed.");
         }
         catch (InvalidOperationException)
         {
            // Should not allow writes on past delivery instances after session closed
         }

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderWriteBytesThrowsEngineFailedAfterConnectionDropped()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.DropAfterLastHandler();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("sender-1").Open();
         IOutgoingDelivery delivery = sender.Next;

         Assert.IsNotNull(delivery);
         Assert.IsTrue(sender.IsSendable);

         try
         {
            delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 }));
            Assert.Fail("Should not be able to write to delivery afters simulated connection drop.");
         }
         catch (EngineFailedException)
         {
         }

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
      }

      [Test]
      [Ignore("Test fails with transfer sending null payload")]
      public void TestSendMultiFrameDeliveryAndSingleFrameDeliveryOnSingleSessionFromDifferentSenders()
      {
         DoMultiplexMultiFrameDeliveryOnSingleSessionOutgoingTestImpl(false);
      }

      [Test]
      [Ignore("Test fails with transfer sending null payload")]
      public void TestMultipleMultiFrameDeliveriesOnSingleSessionFromDifferentSenders()
      {
         DoMultiplexMultiFrameDeliveryOnSingleSessionOutgoingTestImpl(true);
      }

      private void DoMultiplexMultiFrameDeliveryOnSingleSessionOutgoingTestImpl(bool bothDeliveriesMultiFrame)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         uint contentLength1 = 6000;
         uint frameSizeLimit = 4000;
         uint contentLength2 = 2000;

         if (bothDeliveriesMultiFrame)
         {
            contentLength2 = 6000;
         }

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(frameSizeLimit)
                          .Respond()
                          .WithContainerId("driver")
                          .WithMaxFrameSize(frameSizeLimit);
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.ExpectAttach().OfSender().Respond();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = frameSizeLimit;
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         string linkName1 = "Sender1";
         ISender sender1 = session.Sender(linkName1);
         sender1.Open();

         string linkName2 = "Sender2";
         ISender sender2 = session.Sender(linkName2);
         sender2.Open();

         bool sender1MarkedSendable = false;
         sender1.CreditStateUpdateHandler(handler =>
         {
            sender1MarkedSendable = handler.IsSendable;
         });

         bool sender2MarkedSendable = false;
         sender2.CreditStateUpdateHandler(handler =>
         {
            sender2MarkedSendable = handler.IsSendable;
         });

         peer.RemoteFlow().WithHandle(0)
                          .WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Now();
         peer.RemoteFlow().WithHandle(1)
                          .WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Now();

         Assert.IsTrue(sender1MarkedSendable, "Sender 1 should now be sendable");
         Assert.IsTrue(sender2MarkedSendable, "Sender 2 should now be sendable");

         // Frames are not multiplexed for large deliveries as we write the full
         // writable portion out when a write is called.

         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(true)
                              .WithState().Accepted()
                              .WithDeliveryId(0)
                              .WithMore(true)
                              .WithDeliveryTag(new byte[] { 1 });
         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(true)
                              .WithState().Accepted()
                              .WithDeliveryId(0)
                              .WithMore(false)
                              .WithDeliveryTag(Is.NullValue());
         peer.ExpectTransfer().WithHandle(1)
                              .WithSettled(true)
                              .WithState().Accepted()
                              .WithDeliveryId(1)
                              .WithMore(bothDeliveriesMultiFrame)
                              .WithDeliveryTag(new byte[] { 2 });
         if (bothDeliveriesMultiFrame)
         {
            peer.ExpectTransfer().WithHandle(1)
                                 .WithSettled(true)
                                 .WithState().Accepted()
                                 .WithDeliveryId(1)
                                 .WithMore(false)
                                 .WithDeliveryTag(Is.NullValue());
         }

         IProtonBuffer messageContent1 = CreateContentBuffer(contentLength1);
         IOutgoingDelivery delivery1 = sender1.Next;
         delivery1.DeliveryTagBytes = new byte[] { 1 };
         delivery1.Disposition(Accepted.Instance, true);
         delivery1.WriteBytes(messageContent1);

         IProtonBuffer messageContent2 = CreateContentBuffer(contentLength2);
         IOutgoingDelivery delivery2 = sender2.Next;
         delivery2.DeliveryTagBytes = new byte[] { 2 };
         delivery2.Disposition(Accepted.Instance, true);
         delivery2.WriteBytes(messageContent2);

         peer.ExpectClose().Respond();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      [Ignore("Test fails due to frame decoding error on test peer")]
      public void TestMaxFrameSizeOfPeerHasEffect()
      {
         DoMaxFrameSizeTestImpl(0, 0, 5700, 1);
         DoMaxFrameSizeTestImpl(1024, 0, 5700, 6);
      }

      [Test]
      [Ignore("Test fails due to frame decoding error on test peer")]
      public void TestMaxFrameSizeOutgoingFrameSizeLimitHasEffect()
      {
         DoMaxFrameSizeTestImpl(0, 512, 5700, 12);
         DoMaxFrameSizeTestImpl(1024, 512, 5700, 12);
         DoMaxFrameSizeTestImpl(1024, 2048, 5700, 6);
      }

      void DoMaxFrameSizeTestImpl(uint remoteMaxFrameSize, uint outboundFrameSizeLimit, uint contentLength, uint expectedNumFrames)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         if (outboundFrameSizeLimit == 0)
         {
            if (remoteMaxFrameSize == 0)
            {
               peer.ExpectOpen().Respond();
            }
            else
            {
               peer.ExpectOpen().Respond().WithMaxFrameSize(remoteMaxFrameSize);
            }
         }
         else
         {
            if (remoteMaxFrameSize == 0)
            {
               peer.ExpectOpen().WithMaxFrameSize(outboundFrameSizeLimit).Respond();
            }
            else
            {
               peer.ExpectOpen().WithMaxFrameSize(outboundFrameSizeLimit)
                                .Respond()
                                .WithMaxFrameSize(remoteMaxFrameSize);
            }
         }
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();

         IConnection connection = engine.Start();
         if (outboundFrameSizeLimit != 0)
         {
            connection.MaxFrameSize = outboundFrameSizeLimit;
         }
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         string linkName = "mySender";
         ISender sender = session.Sender(linkName);
         sender.Open();

         bool senderMarkedSendable = false;
         sender.CreditStateUpdateHandler(handler =>
         {
            senderMarkedSendable = handler.IsSendable;
         });

         peer.RemoteFlow().WithHandle(0)
                          .WithDeliveryCount(0)
                          .WithLinkCredit(50)
                          .WithIncomingWindow(65535)
                          .WithOutgoingWindow(65535)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Now();

         Assert.IsTrue(senderMarkedSendable, "Sender should now be sendable");

         // This calculation isn't entirely precise, there is some added performative/frame overhead not
         // accounted for...but values are chosen to work, and verified here.
         uint frameCount;
         if (remoteMaxFrameSize == 0 && outboundFrameSizeLimit == 0)
         {
            frameCount = 1;
         }
         else if (remoteMaxFrameSize == 0 && outboundFrameSizeLimit != 0)
         {
            frameCount = (uint)Math.Ceiling((double)contentLength / (double)outboundFrameSizeLimit);
         }
         else
         {
            uint effectiveMaxFrameSize;
            if (outboundFrameSizeLimit != 0)
            {
               effectiveMaxFrameSize = Math.Min(outboundFrameSizeLimit, remoteMaxFrameSize);
            }
            else
            {
               effectiveMaxFrameSize = remoteMaxFrameSize;
            }

            frameCount = (uint)Math.Ceiling((double)contentLength / (double)effectiveMaxFrameSize);
         }

         Assert.AreEqual(expectedNumFrames, frameCount, "Unexpected number of frames calculated");

         for (int i = 1; i <= expectedNumFrames; ++i)
         {
            peer.ExpectTransfer().WithHandle(0)
                                 .WithSettled(true)
                                 .WithState().Accepted()
                                 .WithDeliveryId(0)
                                 .WithMore(i != expectedNumFrames ? true : false)
                                 .WithDeliveryTag(i == 1 ? Is.NotNullValue() : Is.NullValue())
                                 .WithNonNullPayload();
         }

         IProtonBuffer messageContent = CreateContentBuffer(contentLength);
         IOutgoingDelivery delivery = sender.Next;
         delivery.DeliveryTagBytes = new byte[] { 1 };
         delivery.Disposition(Accepted.Instance, true);
         delivery.WriteBytes(messageContent);

         peer.ExpectClose().Respond();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      [Ignore("Fails due to error from splayed tree that indicates duplicate add attempted.")]
      public void TestCompleteInProgressDeliveryWithFinalEmptyTransfer()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithMore(true)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithPayload(payload);
         peer.ExpectTransfer().WithHandle(0)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithAborted(Matches.AnyOf(Is.NullValue(), Matches.Is(false)))
                              .WithSettled(false)
                              .WithMore(Matches.AnyOf(Is.NullValue(), Matches.Is(false)))
                              .WithNullPayload();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");
         sender.Open();

         bool senderMarkedSendable = false;
         sender.CreditStateUpdateHandler(handler =>
         {
            senderMarkedSendable = sender.IsSendable;
         });

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         delivery.DeliveryTagBytes = new byte[] { 0 };
         delivery.StreamBytes(ProtonByteBufferAllocator.Instance.Wrap(payload), false);
         delivery.StreamBytes(null, true);

         Assert.IsFalse(delivery.IsAborted);
         Assert.IsFalse(delivery.IsPartial);
         Assert.IsFalse(delivery.IsSettled);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestAbortInProgressDelivery()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithMore(true)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithPayload(payload);
         peer.ExpectTransfer().WithHandle(0)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithAborted(true)
                              .WithSettled(true)
                              .WithMore(Matches.AnyOf(Is.NullValue(), Matches.Is(false)))
                              .WithNullPayload();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");
         sender.Open();

         bool senderMarkedSendable = false;
         sender.CreditStateUpdateHandler(handler =>
         {
            senderMarkedSendable = sender.IsSendable;
         });

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         delivery.DeliveryTagBytes = new byte[] { 0 };
         delivery.StreamBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         delivery.Abort();

         Assert.IsTrue(delivery.IsAborted);
         Assert.IsFalse(delivery.IsPartial);
         Assert.IsTrue(delivery.IsSettled);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestAbortAlreadyAbortedDelivery()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithMore(true)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithPayload(payload);
         peer.ExpectTransfer().WithHandle(0)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithAborted(true)
                              .WithSettled(true)
                              .WithMore(Matches.AnyOf(Is.NullValue(), Matches.Is(false)))
                              .WithNullPayload();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");
         sender.Open();

         bool senderMarkedSendable = false;
         sender.CreditStateUpdateHandler(handler =>
         {
            senderMarkedSendable = sender.IsSendable;
         });

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         delivery.DeliveryTagBytes = new byte[] { 0 };
         delivery.StreamBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         Assert.IsTrue(sender.HasUnsettled);

         delivery.Abort();

         Assert.IsTrue(delivery.IsAborted);
         Assert.IsFalse(delivery.IsPartial);
         Assert.IsTrue(delivery.IsSettled);

         // Second abort attempt should not error out or trigger additional frames
         delivery.Abort();

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestAbortOnDeliveryThatHasNoWritesIsNoOp()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");
         sender.Open();

         bool senderMarkedSendable = false;
         sender.CreditStateUpdateHandler(handler =>
         {
            senderMarkedSendable = sender.IsSendable;
         });

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         delivery.DeliveryTagBytes = new byte[] { 0 };
         delivery.Abort();

         Assert.IsNull(sender.Current);
         Assert.IsTrue(delivery.IsAborted);
         Assert.IsFalse(delivery.IsPartial);
         Assert.IsTrue(delivery.IsSettled);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestAbortOnDeliveryThatHasNoWritesIsNoOpThenSendUsingCurrent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");
         sender.Open();

         bool senderMarkedSendable = false;
         sender.CreditStateUpdateHandler(handler =>
         {
            senderMarkedSendable = sender.IsSendable;
         });

         IOutgoingDelivery delivery = sender.Next;
         Assert.IsNotNull(delivery);

         delivery.DeliveryTagBytes = new byte[] { 0 };
         delivery.Abort();

         Assert.IsNull(sender.Current);
         Assert.IsTrue(delivery.IsAborted);
         Assert.IsFalse(delivery.IsPartial);
         Assert.IsTrue(delivery.IsSettled);

         try
         {
            IOutgoingDelivery d = sender.Next;
         }
         catch (InvalidOperationException)
         {
            Assert.Fail("Should not be able to next as current was not aborted since nothing was ever written.");
         }

         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithPayload(payload);
         peer.ExpectDisposition().WithFirst(0).WithSettled(true).WithState().Accepted();
         peer.ExpectDetach().WithHandle(0).Respond();

         delivery = sender.Current;
         delivery.DeliveryTagBytes = new byte[] { 1 };
         delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         delivery.Disposition(Accepted.Instance, true);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSettleTransferWithNullDisposition()
      {
         DoTestSettleTransferWithSpecifiedOutcome(null, Is.NullValue(), true);
      }

      [Test]
      public void TestSettleTransferWithAcceptedDisposition()
      {
         IDeliveryState state = Accepted.Instance;
         AcceptedMatcher matcher = new AcceptedMatcher();
         DoTestSettleTransferWithSpecifiedOutcome(state, matcher, true);
      }

      [Test]
      public void TestUnsettledDispositionOfTransferWithAcceptedOutcome()
      {
         IDeliveryState state = Accepted.Instance;
         AcceptedMatcher matcher = new AcceptedMatcher();
         DoTestSettleTransferWithSpecifiedOutcome(state, matcher, false);
      }

      [Test]
      public void TestSettleTransferWithReleasedDisposition()
      {
         IDeliveryState state = Released.Instance;
         ReleasedMatcher matcher = new ReleasedMatcher();
         DoTestSettleTransferWithSpecifiedOutcome(state, matcher, true);
      }

      [Test]
      public void TestSettleTransferWithRejectedDisposition()
      {
         Rejected state = new Rejected();
         RejectedMatcher matcher = new RejectedMatcher();
         DoTestSettleTransferWithSpecifiedOutcome(state, matcher, true);
      }

      [Test]
      [Ignore("Fails due to test peer not handling Error Condition matching")]
      public void TestSettleTransferWithRejectedWithErrorDisposition()
      {
         Rejected state = new Rejected();
         state.Error = new ErrorCondition(AmqpError.DECODE_ERROR, "test");
         RejectedMatcher matcher = new RejectedMatcher().WithError(AmqpError.DECODE_ERROR.ToString(), "test");
         DoTestSettleTransferWithSpecifiedOutcome(state, matcher, true);
      }

      [Test]
      public void TestSettleTransferWithModifiedDisposition()
      {
         Modified state = new Modified();
         state.DeliveryFailed = true;
         state.UndeliverableHere = true;
         ModifiedMatcher matcher = new ModifiedMatcher().WithDeliveryFailed(true).WithUndeliverableHere(true);
         DoTestSettleTransferWithSpecifiedOutcome(state, matcher, true);
      }

      [Test]
      public void TestSettleTransferWithTransactionalDisposition()
      {
         TransactionalState state = new TransactionalState();
         state.TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         state.Outcome = Accepted.Instance;
         TransactionalStateMatcher matcher =
             new TransactionalStateMatcher();
         matcher.WithTxnId(new byte[] { 1 });
         matcher.WithOutcome(new AcceptedMatcher());
         DoTestSettleTransferWithSpecifiedOutcome(state, matcher, true);
      }

      private void DoTestSettleTransferWithSpecifiedOutcome(IDeliveryState state, IMatcher stateMatcher, bool settled)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 });
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(settled)
                                 .WithState(stateMatcher);
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });

         ISender sender = session.Sender("sender-1");

         bool deliverySentAfterSendable = false;
         IOutgoingDelivery sentDelivery = null;
         sender.CreditStateUpdateHandler(handler =>
         {
            sentDelivery = handler.Next;
            sentDelivery.DeliveryTagBytes = new byte[] { 0 };
            sentDelivery.WriteBytes(payload);
            deliverySentAfterSendable = sender.IsSendable;
         });

         sender.Open();

         Assert.IsTrue(deliverySentAfterSendable, "Delivery should have been sent after credit arrived");

         IOutgoingDelivery delivery = sender.Current;
         Assert.IsNull(delivery);
         sentDelivery.Disposition(state, settled);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestAttemptedSecondDispositionOnAlreadySettledDeliveryNull()
      {
         DoTestAttemptedSecondDispositionOnAlreadySettledDelivery(Accepted.Instance, null);
      }

      [Test]
      public void TestAttemptedSecondDispositionOnAlreadySettledDeliveryReleased()
      {
         DoTestAttemptedSecondDispositionOnAlreadySettledDelivery(Accepted.Instance, Released.Instance);
      }

      [Test]
      public void TestAttemptedSecondDispositionOnAlreadySettledDeliveryModified()
      {
         Modified modified = new Modified();
         modified.DeliveryFailed = true;
         DoTestAttemptedSecondDispositionOnAlreadySettledDelivery(Released.Instance, modified);
      }

      [Test]
      public void TestAttemptedSecondDispositionOnAlreadySettledDeliveryRejected()
      {
         DoTestAttemptedSecondDispositionOnAlreadySettledDelivery(Released.Instance, new Rejected());
      }

      [Test]
      public void TestAttemptedSecondDispositionOnAlreadySettledDeliveryTransactional()
      {
         TransactionalState state = new TransactionalState();
         state.Outcome = Accepted.Instance;
         DoTestAttemptedSecondDispositionOnAlreadySettledDelivery(Released.Instance, state);
      }

      private void DoTestAttemptedSecondDispositionOnAlreadySettledDelivery(IDeliveryState first, IDeliveryState second)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 });
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithState(Is.NotNullValue());
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });

         ISender sender = session.Sender("sender-1");
         IOutgoingDelivery sentDelivery = null;

         bool deliverySentAfterSendable = false;
         sender.CreditStateUpdateHandler(handler =>
         {
            sentDelivery = handler.Next;
            sentDelivery.DeliveryTagBytes = new byte[] { 0 };
            sentDelivery.WriteBytes(payload);
            deliverySentAfterSendable = sender.IsSendable;
         });

         sender.Open();

         Assert.IsTrue(deliverySentAfterSendable, "Delivery should have been sent after credit arrived");

         IOutgoingDelivery delivery = sender.Current;
         Assert.IsNull(delivery);
         sentDelivery.Disposition(first, true);

         // A second attempt at the same outcome should result in no action.
         sentDelivery.Disposition(first, true);

         try
         {
            sentDelivery.Disposition(second, true);
            Assert.Fail("Should not be able to update outcome on already settled delivery");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSettleSentDeliveryAfterRemoteSettles()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.RemoteFlow().WithDeliveryCount(0)
                          .WithLinkCredit(10)
                          .WithIncomingWindow(1024)
                          .WithOutgoingWindow(10)
                          .WithNextIncomingId(0)
                          .WithNextOutgoingId(1).Queue();
         peer.ExpectTransfer().WithHandle(0)
                              .WithSettled(false)
                              .WithState(Is.NullValue())
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .Accept();
         peer.ExpectDetach().WithHandle(0).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IProtonBuffer payload = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });

         ISender sender = session.Sender("sender-1");

         bool deliverySentAfterSendable = false;
         IOutgoingDelivery sentDelivery = null;
         sender.CreditStateUpdateHandler(handler =>
         {
            sentDelivery = handler.Next;
            sentDelivery.DeliveryTagBytes = new byte[] { 0 };
            sentDelivery.WriteBytes(payload);
            deliverySentAfterSendable = sender.IsSendable;
         });

         sender.DeliveryStateUpdatedHandler((delivery) =>
         {
            if (delivery.IsRemotelySettled)
            {
               delivery.Settle();
            }
         });

         sender.Open();

         Assert.IsTrue(deliverySentAfterSendable, "Delivery should have been sent after credit arrived");

         Assert.IsNull(sender.Current);

         Assert.IsTrue(sentDelivery.IsRemotelySettled);
         Assert.AreSame(Accepted.Instance, sentDelivery.RemoteState);
         Assert.IsNull(sentDelivery.State);
         Assert.IsTrue(sentDelivery.IsSettled);

         sender.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderHandlesDeferredOpenAndBeginAttachResponses()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool senderRemotelyOpened = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();
         peer.ExpectBegin();
         peer.ExpectAttach().OfSender()
                            .WithTarget().WithDynamic(true).WithAddress((string)null);

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         ISender sender = session.Sender("sender-1");
         Target target = new Target();
         target.Dynamic = true;
         target.Address = null;
         sender.Target = target;
         sender.OpenHandler((result) => senderRemotelyOpened = true).Open();

         peer.WaitForScriptToComplete();

         // This should happen after we inject the held open and attach
         peer.ExpectClose().Respond();

         // Inject held responses to get the ball rolling again
         peer.RemoteOpen().WithOfferedCapabilities("ANONYMOUS_RELAY").Now();
         peer.RespondToLastBegin().Now();
         peer.RespondToLastAttach().Now();

         Assert.IsTrue(senderRemotelyOpened, "Sender remote opened event did not fire");
         ITerminus terminus = sender.RemoteTerminus;
         Assert.True(terminus is Target);

         connection.Close();

         peer.WaitForScriptToComplete();
      }
   }
}
