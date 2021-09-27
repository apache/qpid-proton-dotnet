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
using System.IO;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;
using Is = Apache.Qpid.Proton.Test.Driver.Matchers.Is;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonReceiverTest : ProtonEngineTestSupport
   {
      public static readonly Symbol[] SUPPORTED_OUTCOMES = new Symbol[] { Accepted.DescriptorSymbol,
                                                                          Rejected.DescriptorSymbol,
                                                                          Released.DescriptorSymbol,
                                                                          Modified.DescriptorSymbol };

      [Test]
      public void TestLocalLinkStateCannotBeChangedAfterOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

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

         IReceiver receiver = session.Receiver("test");

         receiver.Properties = new Dictionary<Symbol, object>();

         receiver.Open();

         try
         {
            receiver.Properties = new Dictionary<Symbol, object>();
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            receiver.DesiredCapabilities = new Symbol[] { AmqpError.DECODE_ERROR };
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            receiver.OfferedCapabilities = new Symbol[] { AmqpError.DECODE_ERROR };
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            receiver.SenderSettleMode = SenderSettleMode.Mixed;
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            receiver.Source = new Source();
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            receiver.Target = new Target();
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         try
         {
            receiver.MaxMessageSize = 0u;
            Assert.Fail("Cannot alter local link initial state data after sender opened.");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         receiver.Detach();
         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverEmitsOpenAndCloseEvents()
      {
         DoTestReceiverEmitsEvents(false);
      }

      [Test]
      public void TestReceiverEmitsOpenAndDetachEvents()
      {
         DoTestReceiverEmitsEvents(true);
      }

      private void DoTestReceiverEmitsEvents(bool detach)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool receiverLocalOpen = false;
         bool receiverLocalClose = false;
         bool receiverLocalDetach = false;
         bool receiverRemoteOpen = false;
         bool receiverRemoteClose = false;
         bool receiverRemoteDetach = false;

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

         IReceiver receiver = session.Receiver("test");
         receiver.LocalOpenHandler((result) => receiverLocalOpen = true)
                 .LocalCloseHandler((result) => receiverLocalClose = true)
                 .LocalDetachHandler((result) => receiverLocalDetach = true)
                 .OpenHandler((result) => receiverRemoteOpen = true)
                 .DetachHandler((result) => receiverRemoteDetach = true)
                 .CloseHandler((result) => receiverRemoteClose = true);

         receiver.Open();

         if (detach)
         {
            receiver.Detach();
         }
         else
         {
            receiver.Close();
         }

         Assert.IsTrue(receiverLocalOpen, "Receiver should have reported local open");
         Assert.IsTrue(receiverRemoteOpen, "Receiver should have reported remote open");

         if (detach)
         {
            Assert.IsFalse(receiverLocalClose, "Receiver should not have reported local close");
            Assert.IsTrue(receiverLocalDetach, "Receiver should have reported local detach");
            Assert.IsFalse(receiverRemoteClose, "Receiver should not have reported remote close");
            Assert.IsTrue(receiverRemoteDetach, "Receiver should have reported remote close");
         }
         else
         {
            Assert.IsTrue(receiverLocalClose, "Receiver should have reported local close");
            Assert.IsFalse(receiverLocalDetach, "Receiver should not have reported local detach");
            Assert.IsTrue(receiverRemoteClose, "Receiver should have reported remote close");
            Assert.IsFalse(receiverRemoteDetach, "Receiver should not have reported remote close");
         }

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverRoutesDetachEventToCloseHandlerIfNoneSet()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool receiverLocalOpen = false;
         bool receiverLocalClose = false;
         bool receiverRemoteOpen = false;
         bool receiverRemoteClose = false;

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

         IReceiver receiver = session.Receiver("test");
         receiver.LocalOpenHandler((result) => receiverLocalOpen = true)
                 .LocalCloseHandler((result) => receiverLocalClose = true)
                 .OpenHandler((result) => receiverRemoteOpen = true)
                 .CloseHandler((result) => receiverRemoteClose = true);

         receiver.Open();
         receiver.Detach();

         Assert.IsTrue(receiverLocalOpen, "Receiver should have reported local open");
         Assert.IsTrue(receiverRemoteOpen, "Receiver should have reported remote open");
         Assert.IsTrue(receiverLocalClose, "Receiver should have reported local detach");
         Assert.IsTrue(receiverRemoteClose, "Receiver should have reported remote detach");

         session.Close();

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

         IReceiver receiver = session.Receiver("test");
         receiver.Open();
         receiver.EngineShutdownHandler((result) => engineShutdown = true);

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

            receiver.Close();
         }

         if (remotelyClosed && !locallyClosed)
         {
            peer.RemoteDetach().Now();
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
      public void TestReceiverOpenWithNoSenderOrReceiverSettleModes()
      {
         DoTestOpenReceiverWithConfiguredSenderAndReceiverSettlementModes(null, null);
      }

      [Test]
      public void TestReceiverOpenWithSettledAndFirst()
      {
         DoTestOpenReceiverWithConfiguredSenderAndReceiverSettlementModes(SenderSettleMode.Settled, ReceiverSettleMode.First);
      }

      [Test]
      public void TestReceiverOpenWithUnsettledAndSecond()
      {
         DoTestOpenReceiverWithConfiguredSenderAndReceiverSettlementModes(SenderSettleMode.Unsettled, ReceiverSettleMode.Second);
      }

      private void DoTestOpenReceiverWithConfiguredSenderAndReceiverSettlementModes(SenderSettleMode? senderMode, ReceiverSettleMode? receiverMode)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithSndSettleMode(((byte?)senderMode))
                            .WithRcvSettleMode(((byte?)receiverMode))
                            .Respond()
                            .WithSndSettleMode(((byte?)senderMode))
                            .WithRcvSettleMode(((byte?)receiverMode));

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("test");
         if (senderMode.HasValue)
         {
            receiver.SenderSettleMode = (SenderSettleMode)senderMode;
         }
         if (receiverMode.HasValue)
         {
            receiver.ReceiverSettleMode = (ReceiverSettleMode)receiverMode;
         }
         receiver.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().Respond();

         if (senderMode != null)
         {
            Assert.AreEqual(senderMode, receiver.SenderSettleMode);
         }
         else
         {
            Assert.AreEqual(SenderSettleMode.Mixed, receiver.SenderSettleMode);
         }

         if (receiverMode != null)
         {
            Assert.AreEqual(receiverMode, receiver.ReceiverSettleMode);
         }
         else
         {
            Assert.AreEqual(ReceiverSettleMode.First, receiver.ReceiverSettleMode);
         }

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCreateReceiverAndInspectRemoteEndpoint()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfReceiver()
                            .WithSource(Is.NotNullValue())
                            .WithTarget(Is.NotNullValue())
                            .Respond();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         Modified defaultOutcome = new Modified();
         defaultOutcome.DeliveryFailed = true;
         String sourceAddress = Guid.NewGuid().ToString() + ":1";

         Source source = new Source();
         source.Address = sourceAddress;
         source.Outcomes = SUPPORTED_OUTCOMES;
         source.DefaultOutcome = defaultOutcome;

         IReceiver receiver = session.Receiver("test");
         receiver.Source = source;
         receiver.Target = new Target();
         receiver.Open();

         Assert.IsTrue(receiver.RemoteState.Equals(LinkState.Active));
         Assert.IsNotNull(receiver.RemoteSource);
         Assert.IsNotNull(receiver.RemoteTerminus);
         Assert.AreEqual(SUPPORTED_OUTCOMES, receiver.RemoteSource.Outcomes);
         Assert.IsTrue(receiver.RemoteSource.DefaultOutcome is Modified);
         Assert.AreEqual(sourceAddress, receiver.RemoteSource.Address);

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

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
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.ExpectDetach().WithClosed(close).Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         Assert.IsTrue(receiver.IsReceiver);
         Assert.IsFalse(receiver.IsSender);

         if (close)
         {
            receiver.Close();
         }
         else
         {
            receiver.Detach();
         }

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverOpenAndCloseAreIdempotent()
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
         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         // Should not emit another attach frame
         receiver.Open();

         receiver.Close();

         // Should not emit another detach frame
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineEmitsAttachAfterLocalReceiverOpened()
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
         IReceiver receiver = session.Receiver("test");
         receiver.Open();
         receiver.Close();

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
         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverFireOpenedEventAfterRemoteAttachArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();

         bool receiverRemotelyOpened = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.OpenHandler((result) => receiverRemotelyOpened = true);
         receiver.Open();

         Assert.IsTrue(receiverRemotelyOpened, "Receiver remote opened event did not fire");

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverFireClosedEventAfterRemoteDetachArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();

         bool receiverRemotelyOpened = false;
         bool receiverRemotelyClosed = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.OpenHandler((result) =>
         {
            receiverRemotelyOpened = true;
         });
         receiver.CloseHandler((result) =>
         {
            receiverRemotelyClosed = true;
         });
         receiver.Open();

         Assert.IsTrue(receiverRemotelyOpened, "Receiver remote opened event did not fire");

         receiver.Close();

         Assert.IsTrue(receiverRemotelyClosed, "Receiver remote closed event did not fire");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestRemotelyCloseReceiverAndOpenNewReceiverImmediatelyAfterWithNewLinkName()
      {
         DoTestRemotelyTerminateLinkAndThenCreateNewLink(true, false);
      }

      [Test]
      public void TestRemotelyDetachReceiverAndOpenNewReceiverImmediatelyAfterWithNewLinkName()
      {
         DoTestRemotelyTerminateLinkAndThenCreateNewLink(false, false);
      }

      [Test]
      public void TestRemotelyCloseReceiverAndOpenNewReceiverImmediatelyAfterWithSameLinkName()
      {
         DoTestRemotelyTerminateLinkAndThenCreateNewLink(true, true);
      }

      [Test]
      public void TestRemotelyDetachReceiverAndOpenNewReceiverImmediatelyAfterWithSameLinkName()
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
         peer.ExpectAttach().WithHandle(0).OfReceiver().Respond();
         peer.RemoteDetach().WithClosed(close).Queue();

         bool receiverRemotelyOpened = false;
         bool receiverRemotelyClosed = false;
         bool receiverRemotelyDetached = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver(firstLinkName);
         receiver.OpenHandler((result) => receiverRemotelyOpened = true);
         receiver.CloseHandler((result) => receiverRemotelyClosed = true);
         receiver.DetachHandler((result) => receiverRemotelyDetached = true);
         receiver.Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(receiverRemotelyOpened, "Receiver remote opened event did not fire");

         if (close)
         {
            Assert.IsTrue(receiverRemotelyClosed, "Receiver remote closed event did not fire");
            Assert.IsFalse(receiverRemotelyDetached, "Receiver remote detached event fired");
         }
         else
         {
            Assert.IsFalse(receiverRemotelyClosed, "Receiver remote closed event fired");
            Assert.IsTrue(receiverRemotelyDetached, "Receiver remote closed event did not fire");
         }

         peer.ExpectDetach().WithClosed(close);
         if (close)
         {
            receiver.Close();
         }
         else
         {
            receiver.Detach();
         }

         peer.WaitForScriptToComplete();
         peer.ExpectAttach().WithHandle(0).OfReceiver().Respond();
         peer.ExpectDetach().WithClosed(close).Respond();

         // Reset trackers
         receiverRemotelyOpened = false;
         receiverRemotelyClosed = false;
         receiverRemotelyDetached = false;

         receiver = session.Receiver(secondLinkName);
         receiver.OpenHandler((result) => receiverRemotelyOpened = true);
         receiver.CloseHandler((result) => receiverRemotelyClosed = true);
         receiver.DetachHandler((result) => receiverRemotelyDetached = true);
         receiver.Open();

         if (close)
         {
            receiver.Close();
         }
         else
         {
            receiver.Detach();
         }

         peer.WaitForScriptToComplete();

         Assert.IsTrue(receiverRemotelyOpened, "Receiver remote opened event did not fire");

         if (close)
         {
            Assert.IsTrue(receiverRemotelyClosed, "Receiver remote closed event did not fire");
            Assert.IsFalse(receiverRemotelyDetached, "Receiver remote detached event fired");
         }
         else
         {
            Assert.IsFalse(receiverRemotelyClosed, "Receiver remote closed event fired");
            Assert.IsTrue(receiverRemotelyDetached, "Receiver remote closed event did not fire");
         }

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverFireOpenedEventAfterRemoteAttachArrivesWithNullTarget()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond().WithNullSource();
         peer.ExpectDetach().Respond();

         bool receiverRemotelyOpened = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Source = new Source();
         receiver.Target = new Target();
         receiver.OpenHandler((result) =>
         {
            receiverRemotelyOpened = true;
         });
         receiver.Open();

         Assert.IsTrue(receiverRemotelyOpened, "Receiver remote opened event did not fire");
         Assert.IsNull(receiver.RemoteSource);

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenAndCloseMultipleReceivers()
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

         IReceiver receiver1 = session.Receiver("receiver-1");
         receiver1.Open();
         IReceiver receiver2 = session.Receiver("receiver-2");
         receiver2.Open();

         // Close in reverse order
         receiver2.Close();
         receiver1.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionSignalsRemoteReceiverOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.RemoteAttach().WithName("sender")
                            .WithHandle(0)
                            .OfSender()
                            .WithInitialDeliveryCount(0).Queue();
         peer.ExpectAttach();
         peer.ExpectDetach().Respond();

         bool receiverRemotelyOpened = false;
         IReceiver receiver = null;

         IConnection connection = engine.Start();

         connection.ReceiverOpenedHandler((result) =>
         {
            receiverRemotelyOpened = true;
            receiver = result;
         });

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         Assert.IsTrue(receiverRemotelyOpened, "Receiver remote opened event did not fire");

         receiver.Open();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotOpenReceiverAfterSessionClosed()
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

         IReceiver receiver = session.Receiver("test");

         session.Close();

         try
         {
            receiver.Open();
            Assert.Fail("Should not be able to open a link from a closed session.");
         }
         catch (InvalidOperationException)
         {
         }

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotOpenReceiverAfterSessionRemotelyClosed()
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
         IReceiver receiver = session.Receiver("test");
         session.Open();

         try
         {
            receiver.Open();
            Assert.Fail("Should not be able to open a link from a remotely closed session.");
         }
         catch (InvalidOperationException)
         {
         }

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenReceiverBeforeOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Create the connection but don't open, then open a session and a receiver and
         // the session begin and receiver attach shouldn't go out until the connection
         // is opened locally.
         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("receiver");
         receiver.Open();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("receiver").OfReceiver().Respond();

         // Now open the connection, expect the Open, Begin, and Attach frames
         connection.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenReceiverBeforeOpenSession()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         // Create the connection and open it, then create a session and a receiver
         // and observe that the receiver doesn't send its attach until the session
         // is opened.
         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         IReceiver receiver = session.Receiver("receiver");
         receiver.Open();

         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("receiver").OfReceiver().Respond();

         // Now open the session, expect the Begin, and Attach frames
         session.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDetachAfterEndSent()
      {
         DoTestReceiverCloseOrDetachAfterEndSent(false);
      }

      [Test]
      public void TestReceiverCloseAfterEndSent()
      {
         DoTestReceiverCloseOrDetachAfterEndSent(true);
      }

      public void DoTestReceiverCloseOrDetachAfterEndSent(bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("receiver").OfReceiver().Respond();
         peer.ExpectEnd().Respond();

         // Create the connection and open it, then crate a session and a receiver
         // and observe that the receiver doesn't send its detach if the session has
         // already been closed.
         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("receiver");
         receiver.Open();

         // Cause an End frame to be sent
         session.Close();

         // The sender should not emit an end as the session was closed which implicitly
         // detached the link.
         if (close)
         {
            receiver.Close();
         }
         else
         {
            receiver.Detach();
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDetachAfterCloseSent()
      {
         DoTestReceiverClosedOrDetachedAfterCloseSent(false);
      }

      [Test]
      public void TestReceiverCloseAfterCloseSent()
      {
         DoTestReceiverClosedOrDetachedAfterCloseSent(true);
      }

      public void DoTestReceiverClosedOrDetachedAfterCloseSent(bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("receiver").OfReceiver().Respond();
         peer.ExpectClose().Respond();

         // Create the connection and open it, then create a session and a receiver
         // and observe that the receiver doesn't send its detach if the connection has
         // already been closed.
         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("receiver");
         receiver.Open();

         // Cause an Close frame to be sent
         connection.Close();

         // The receiver should not emit an detach as the connection was closed which implicitly
         // detached the link.
         if (close)
         {
            receiver.Close();
         }
         else
         {
            receiver.Detach();
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsFlowWhenCreditSet()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond().WithNextOutgoingId(42);
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100).WithNextIncomingId(42);
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();
         receiver.AddCredit(100);
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsFlowWithNoIncomingIdWhenRemoteBeginHasNotArrivedYet()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin();
         peer.ExpectAttach();
         peer.ExpectFlow().WithLinkCredit(100).WithNextIncomingId(Is.NullValue());

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open();

         receiver.AddCredit(100);

         bool opened = false;
         receiver.OpenHandler((self) =>
         {
            opened = true;
         });

         peer.WaitForScriptToComplete();
         peer.RespondToLastBegin().WithNextOutgoingId(42).Now();
         peer.RespondToLastAttach().Now();
         peer.ExpectFlow().WithLinkCredit(101).WithNextIncomingId(42);
         peer.ExpectDetach().Respond();

         Assert.IsTrue(opened);

         receiver.AddCredit(1);

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsFlowAfterOpenedWhenCreditSetBeforeOpened()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100);
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.AddCredit(100);
         receiver.Open();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsFlowAfterConnectionOpenFinallySent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");

         // Create and open all resources except don't open the connection and then
         // we will observe that the receiver flow doesn't fire until it has sent its
         // attach following the session send its Begin.
         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.AddCredit(1);
         receiver.Open();

         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);

         connection.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverOmitsFlowAfterConnectionOpenFinallySentWhenAfterDetached()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Create and open all resources except don't open the connection and then
         // we will observe that the receiver flow doesn't fire since the link was
         // detached prior to being able to send any state updates.
         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.AddCredit(1);
         receiver.Open();
         receiver.Detach();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().Respond();

         connection.Open();

         peer.WaitForScriptToComplete();

         Assert.AreEqual(0, receiver.Credit);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDrainAllOutstanding()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         uint creditWindow = 100;

         // Add some credit, verify not draining
         IMatcher notDrainingMatcher = Matches.AnyOf(Is.EqualTo(false), Is.NullValue());
         peer.ExpectFlow().WithDrain(notDrainingMatcher).WithLinkCredit(creditWindow).WithDeliveryCount(0);
         receiver.AddCredit(creditWindow);

         peer.WaitForScriptToComplete();

         // Check that calling drain sends flow, and calls handler on response draining all credit
         bool handlerCalled = false;
         receiver.CreditStateUpdateHandler((x) => handlerCalled = true);

         peer.ExpectFlow().WithDrain(true).WithLinkCredit(creditWindow).WithDeliveryCount(0)
                          .Respond()
                          .WithDrain(true).WithLinkCredit(0).WithDeliveryCount(creditWindow);

         receiver.Drain();

         peer.WaitForScriptToComplete();
         Assert.IsTrue(handlerCalled, "Handler was not called");

         peer.ExpectDetach().Respond();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }
   }
}
