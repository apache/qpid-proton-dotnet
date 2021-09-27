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
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
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
      [Ignore("Fails due to error that causes link to detach itself.")]
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
   }
}
