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
using System.Text;
using System.Text.Json;
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
         string sourceAddress = Guid.NewGuid().ToString() + ":1";

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

         string firstLinkName = "test-link-1";
         string secondLinkName = sameLinkName ? firstLinkName : "test-link-2";

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

      [Test]
      public void TestReceiverDrainWithNoCreditResultInNoOutAdd()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().Respond();

         receiver.Drain();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDrainAllowsOnlyOnePendingDrain()
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

         peer.ExpectFlow().WithDrain(true).WithLinkCredit(creditWindow).WithDeliveryCount(0);

         receiver.Drain();

         Assert.Throws<InvalidOperationException>(() => receiver.Drain());

         peer.WaitForScriptToComplete();

         peer.ExpectDetach().Respond();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDrainWithCreditsAllowsOnlyOnePendingDrain()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open();

         uint creditWindow = 100;

         peer.WaitForScriptToComplete();

         peer.ExpectFlow().WithDrain(true).WithLinkCredit(creditWindow).WithDeliveryCount(0);

         receiver.Drain(creditWindow);

         Assert.Throws<InvalidOperationException>(() => receiver.Drain(creditWindow));

         peer.WaitForScriptToComplete();

         peer.ExpectDetach().Respond();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverThrowsOnAddCreditAfterConnectionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();
         connection.Close();

         try
         {
            receiver.AddCredit(100);
            Assert.Fail("Should not be able to add credit after connection was closed");
         }
         catch (InvalidOperationException)
         {
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverThrowsOnAddCreditAfterSessionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();
         session.Close();

         try
         {
            receiver.AddCredit(100);
            Assert.Fail("Should not be able to add credit after session was closed");
         }
         catch (InvalidOperationException)
         {
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDispatchesIncomingDelivery()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         receiver.DeliveryReadHandler((delivery) =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;
         });
         receiver.Open();
         receiver.AddCredit(100);
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Deliver should not be partial");

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsDispositionForTransfer()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Accepted();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;

            delivery.Disposition(Accepted.Instance, true);
         });

         receiver.Open();
         receiver.AddCredit(100);

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Deliver should not be partial");
         Assert.IsFalse(receiver.HasUnsettled);

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsDispositionOnlyOnceForTransfer()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Accepted();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         receiver.DeliveryReadHandler((delivery) =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;

            delivery.Disposition(Accepted.Instance, true);
         });
         receiver.Open();
         receiver.AddCredit(100);

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Deliver should not be partial");

         // Already settled so this should trigger error
         try
         {
            receivedDelivery.Disposition(Released.Instance, true);
            Assert.Fail("Should not be able to set a second disposition");
         }
         catch (InvalidOperationException)
         {
            // Expected that we can't settle twice.
         }

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsUpdatedDispositionsForTransferBeforeSettlement()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(false)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Released();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;

            delivery.Disposition(Accepted.Instance, false);
         });
         receiver.Open();
         receiver.AddCredit(100);

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Deliver should not be partial");
         Assert.IsTrue(receiver.HasUnsettled);

         // Second disposition should be sent as we didn't settle previously.
         receivedDelivery.Disposition(Released.Instance, true);

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverSendsUpdatedDispositionsForTransferBeforeSettlementThenSettles()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(false)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(false)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Released();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Released();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;

            delivery.Disposition(Accepted.Instance);
         });
         receiver.Open();
         receiver.AddCredit(100);

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Deliver should not be partial");
         Assert.IsTrue(receiver.HasUnsettled);

         // Second disposition should be sent as we didn't settle previously.
         receivedDelivery.Disposition(Released.Instance);
         receivedDelivery.Settle();

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestDispositionNoAllowedAfterCloseSent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectClose();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;
         });

         receiver.Open();
         receiver.AddCredit(1);

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Deliver should not be partial");

         connection.Close();

         try
         {
            receivedDelivery.Disposition(Released.Instance);
            Assert.Fail("Should not be able to set a disposition after the connection was closed");
         }
         catch (InvalidOperationException) { }

         try
         {
            receivedDelivery.Disposition(Released.Instance, true);
            Assert.Fail("Should not be able to update a disposition after the connection was closed");
         }
         catch (InvalidOperationException) { }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverReportsDeliveryUpdatedOnDisposition()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(100);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.RemoteDisposition().WithSettled(true)
                                 .WithRole(Role.Sender.ToBoolean())
                                 .WithState().Accepted()
                                 .WithFirst(0).Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;
         });

         bool deliveryUpdatedAndSettled = false;
         IIncomingDelivery updatedDelivery = null;
         receiver.DeliveryStateUpdatedHandler(delivery =>
         {
            if (delivery.IsRemotelySettled)
            {
               deliveryUpdatedAndSettled = true;
            }

            updatedDelivery = delivery;
         });

         receiver.Open();
         receiver.AddCredit(100);
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsTrue(deliveryUpdatedAndSettled, "Delivery should have been updated to settled");
         Assert.IsFalse(receivedDelivery.IsPartial, "Delivery should not be partial");
         Assert.IsFalse(updatedDelivery.IsPartial, "Delivery should not be partial");
         Assert.AreSame(receivedDelivery, updatedDelivery, "Delivery should be same object as first received");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverReportsDeliveryUpdatedOnDispositionForMultipleTransfers()
      {
         DoTestReceiverReportsDeliveryUpdatedOnDispositionForMultipleTransfers(0);
      }

      [Test]
      public void TestReceiverReportsDeliveryUpdatedOnDispositionForMultipleTransfersDeliveryIdOverflows()
      {
         DoTestReceiverReportsDeliveryUpdatedOnDispositionForMultipleTransfers(int.MaxValue);
      }

      private void DoTestReceiverReportsDeliveryUpdatedOnDispositionForMultipleTransfers(uint firstDeliveryId)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);
         peer.RemoteTransfer().WithDeliveryId(firstDeliveryId)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.RemoteTransfer().WithDeliveryId(firstDeliveryId + 1)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.RemoteDisposition().WithSettled(true)
                                 .WithRole(Role.Sender.ToBoolean())
                                 .WithState().Accepted()
                                 .WithFirst(firstDeliveryId)
                                 .WithLast(firstDeliveryId + 1).Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;
         int dispositionCounter = 0;

         List<IIncomingDelivery> deliveries = new List<IIncomingDelivery>();

         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryCounter++;
         });

         receiver.DeliveryStateUpdatedHandler(delivery =>
         {
            if (delivery.IsRemotelySettled)
            {
               dispositionCounter++;
               deliveries.Add(delivery);
            }
         });

         receiver.Open();
         receiver.AddCredit(2);
         receiver.Close();

         Assert.AreEqual(2, deliveryCounter, "Not all deliveries arrived");
         Assert.AreEqual(2, deliveries.Count, "Not all deliveries received dispositions");

         byte deliveryTag = 0;

         foreach (IIncomingDelivery delivery in deliveries)
         {
            Assert.AreEqual(deliveryTag++, delivery.DeliveryTag.TagBuffer.GetByte(0), "Delivery not updated in correct order");
            Assert.IsTrue(delivery.IsRemotelySettled, "Delivery should be marked as remotely settled");
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverReportsDeliveryUpdatedNextFrameForMultiFrameTransfer()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         string text = "test-string-for-split-frame-delivery";
         byte[] encoded = Encoding.UTF8.GetBytes(text);
         byte[] first = new byte[encoded.Length / 2];
         byte[] second = new byte[encoded.Length - first.Length];

         Array.Copy(encoded, 0, first, 0, first.Length);
         Array.Copy(encoded, first.Length, second, 0, second.Length);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithBody().WithData(first).Also().Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithBody().WithData(second).Also().Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         int deliverReads = 0;

         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;
            deliverReads++;
         });

         receiver.Open();
         receiver.AddCredit(2);

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Delivery should not be partial");
         Assert.AreEqual(2, deliverReads, "Deliver should have been read twice for two transfers");
         Assert.AreSame(receivedDelivery, receivedDelivery, "Delivery should be same object as first received");

         IProtonBuffer payload = receivedDelivery.ReadAll();

         Assert.IsNotNull(payload);

         // We are cheating a bit here as this isn't how the encoding would normally work.
         Data section1 = decoder.ReadObject<Data>(payload, decoderState);
         Data section2 = decoder.ReadObject<Data>(payload, decoderState);

         IProtonBuffer combined = ProtonByteBufferAllocator.Instance.Allocate(encoded.Length);

         combined.WriteBytes(section1.Value);
         combined.WriteBytes(section2.Value);

         Assert.AreEqual(text, combined.ToString(Encoding.UTF8), "Encoded and Decoded strings don't match");

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverReportsUpdateWhenLastFrameOfMultiFrameTransferHasNoPayload()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         string text = "test-string-for-split-frame-delivery";
         byte[] encoded = Encoding.UTF8.GetBytes(text);
         byte[] first = new byte[encoded.Length / 2];
         byte[] second = new byte[encoded.Length - first.Length];

         Array.Copy(encoded, 0, first, 0, first.Length);
         Array.Copy(encoded, first.Length, second, 0, second.Length);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithBody().WithData(first).Also().Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithBody().WithData(second).Also().Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         bool deliveryArrived = false;
         IIncomingDelivery receivedDelivery = null;
         int deliverReads = 0;

         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryArrived = true;
            receivedDelivery = delivery;
            deliverReads++;
         });

         receiver.Open();
         receiver.AddCredit(1);

         Assert.IsTrue(deliveryArrived, "Delivery did not arrive at the receiver");
         Assert.IsFalse(receivedDelivery.IsPartial, "Delivery should not be partial");
         Assert.AreEqual(4, deliverReads, "Deliver should have been read twice for two transfers");
         Assert.AreSame(receivedDelivery, receivedDelivery, "Delivery should be same object as first received");

         IProtonBuffer payload = receivedDelivery.ReadAll();

         Assert.IsNotNull(payload);

         // We are cheating a bit here as this isn't how the encoding would normally work.
         Data section1 = decoder.ReadObject<Data>(payload, decoderState);
         Data section2 = decoder.ReadObject<Data>(payload, decoderState);

         IProtonBuffer data1 = section1.Buffer;
         IProtonBuffer data2 = section2.Buffer;

         IProtonBuffer combined = ProtonByteBufferAllocator.Instance.Allocate(encoded.Length);

         combined.WriteBytes(data1);
         combined.WriteBytes(data2);

         Assert.AreEqual(text, combined.ToString(Encoding.UTF8), "Encoded and Decoded strings don't match");

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestMultiplexMultiFrameDeliveriesOnSingleSessionIncoming()
      {
         DoMultiplexMultiFrameDeliveryOnSingleSessionIncomingTestImpl(true);
      }

      [Test]
      public void TestMultiplexMultiFrameDeliveryOnSingleSessionIncoming()
      {
         DoMultiplexMultiFrameDeliveryOnSingleSessionIncomingTestImpl(false);
      }

      private void DoMultiplexMultiFrameDeliveryOnSingleSessionIncomingTestImpl(bool bothDeliveriesMultiFrame)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("receiver-1").Respond();
         peer.ExpectAttach().WithHandle(1).WithName("receiver-2").Respond();
         peer.ExpectFlow().WithHandle(0).WithLinkCredit(5);
         peer.ExpectFlow().WithHandle(1).WithLinkCredit(5);

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver1 = session.Receiver("receiver-1");
         IReceiver receiver2 = session.Receiver("receiver-2");

         string delivery1LinkedResource = "Delivery1";
         string delivery2LinkedResource = "Delivery2";

         IIncomingDelivery receivedDelivery1 = null;
         IIncomingDelivery receivedDelivery2 = null;

         bool delivery1Updated = false;
         bool delivery2Updated = false;

         string deliveryTag1 = "tag1";
         string deliveryTag2 = "tag2";

         byte[] payload1 = new byte[] { 1, 1 };
         byte[] payload2 = new byte[] { 2, 2 };

         // Receiver 1 handlers for delivery processing.
         receiver1.DeliveryReadHandler(delivery =>
         {
            receivedDelivery1 = delivery;
            delivery.LinkedResource = delivery1LinkedResource;
         });
         receiver1.DeliveryStateUpdatedHandler(delivery =>
         {
            delivery1Updated = true;
            Assert.AreEqual(delivery1LinkedResource, delivery.LinkedResource);
            string autoCasted = (string)delivery.LinkedResource;
            Assert.AreEqual(delivery1LinkedResource, autoCasted);
         });

         // Receiver 2 handlers for delivery processing.
         receiver2.DeliveryReadHandler(delivery =>
         {
            receivedDelivery2 = delivery;
            delivery.LinkedResource = delivery2LinkedResource;
         });
         receiver2.DeliveryStateUpdatedHandler(delivery =>
         {
            delivery2Updated = true;
            Assert.AreEqual(delivery2LinkedResource, delivery.LinkedResource);
            string autoCasted = (string)delivery.LinkedResource;
            Assert.AreEqual(delivery2LinkedResource, autoCasted);
         });

         receiver1.Open();
         receiver2.Open();

         receiver1.AddCredit(5);
         receiver2.AddCredit(5);

         Assert.IsNull(receivedDelivery1, "Should not have any delivery data yet on receiver 1");
         Assert.IsNull(receivedDelivery2, "Should not have any delivery date yet on receiver 2");

         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithHandle(0)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag1))
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(payload1).Now();
         peer.RemoteTransfer().WithDeliveryId(1)
                              .WithHandle(1)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag2))
                              .WithMore(bothDeliveriesMultiFrame)
                              .WithMessageFormat(0)
                              .WithPayload(payload2).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have a delivery event on receiver 1");
         Assert.IsNotNull(receivedDelivery2, "Should have a delivery event on receiver 2");

         Assert.IsTrue(receivedDelivery1.IsPartial, "Delivery on Receiver 1 Should not be complete");
         if (bothDeliveriesMultiFrame)
         {
            Assert.IsTrue(receivedDelivery2.IsPartial, "Delivery on Receiver 2 Should be complete");
         }
         else
         {
            Assert.IsFalse(receivedDelivery2.IsPartial, "Delivery on Receiver 2 Should not be complete");
         }

         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithHandle(0)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag1))
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload1).Now();
         if (bothDeliveriesMultiFrame)
         {
            peer.RemoteTransfer().WithDeliveryId(1)
                                 .WithHandle(1)
                                 .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag2))
                                 .WithMore(false)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload2).Now();
         }

         Assert.IsFalse(receivedDelivery1.IsPartial, "Delivery on Receiver 1 Should be complete");
         Assert.IsFalse(receivedDelivery2.IsPartial, "Delivery on Receiver 2 Should be complete");

         Assert.IsFalse(delivery1Updated);
         Assert.IsFalse(delivery2Updated);

         peer.ExpectDisposition().WithFirst(1)
                                 .WithSettled(true)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithRole(Role.Receiver.ToBoolean())
                                 .WithState().Accepted();

         Assert.AreEqual(Encoding.UTF8.GetBytes(deliveryTag1), receivedDelivery1.DeliveryTag.TagBytes);
         Assert.AreEqual(Encoding.UTF8.GetBytes(deliveryTag2), receivedDelivery2.DeliveryTag.TagBytes);

         IProtonBuffer payloadBuffer1 = receivedDelivery1.ReadAll();
         IProtonBuffer payloadBuffer2 = receivedDelivery2.ReadAll();

         Assert.AreEqual(payload1.Length * 2, payloadBuffer1.ReadableBytes, "Received 1 payload size is wrong");
         Assert.AreEqual(payload2.Length * (bothDeliveriesMultiFrame ? 2 : 1), payloadBuffer2.ReadableBytes, "Received 2 payload size is wrong");

         receivedDelivery2.Disposition(Accepted.Instance, true);
         receivedDelivery1.Disposition(Accepted.Instance, true);

         Assert.IsFalse(delivery1Updated);
         Assert.IsFalse(delivery2Updated);

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDeliveryIdTrackingHandlesAbortedDelivery()
      {
         // Check aborted=true, more=false, settled=true.
         DoTestReceiverDeliveryIdTrackingHandlesAbortedDelivery(false, true);
         // Check aborted=true, more=false, settled=unset(false)
         // Aborted overrides settled not being set.
         DoTestReceiverDeliveryIdTrackingHandlesAbortedDelivery(false, null);
         // Check aborted=true, more=false, settled=false
         // Aborted overrides settled being explicitly false.
         DoTestReceiverDeliveryIdTrackingHandlesAbortedDelivery(false, false);
         // Check aborted=true, more=true, settled=true
         // Aborted overrides the more=true.
         DoTestReceiverDeliveryIdTrackingHandlesAbortedDelivery(true, true);
         // Check aborted=true, more=true, settled=unset(false)
         // Aborted overrides the more=true, and settled being unset.
         DoTestReceiverDeliveryIdTrackingHandlesAbortedDelivery(true, null);
         // Check aborted=true, more=true, settled=false
         // Aborted overrides the more=true, and settled explicitly false.
         DoTestReceiverDeliveryIdTrackingHandlesAbortedDelivery(true, false);
      }

      private void DoTestReceiverDeliveryIdTrackingHandlesAbortedDelivery(bool setMoreOnAbortedTransfer, bool? setSettledOnAbortedTransfer)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("receiver");
         receiver.AddCredit(2);

         IIncomingDelivery receivedDelivery = null;
         IIncomingDelivery abortedDelivery = null;
         int deliveryCounter = 0;
         bool deliveryUpdated = false;
         byte[] payload = new byte[] { 1 };

         // Receiver 1 handlers for delivery processing.
         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryCounter++;
            if (delivery.IsAborted)
            {
               abortedDelivery = delivery;
            }
            else
            {
               receivedDelivery = delivery;
            }
         });
         receiver.DeliveryStateUpdatedHandler(delivery =>
         {
            deliveryUpdated = true;
         });

         receiver.Open();

         Assert.IsNull(receivedDelivery, "Should not have any delivery data yet on receiver 1");
         Assert.AreEqual(0, deliveryCounter, "Should not have any delivery data yet on receiver 1");
         Assert.IsFalse(deliveryUpdated, "Should not have any delivery data yet on receiver 1");

         // First chunk indicates more to come.
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Now();

         Assert.IsNotNull(receivedDelivery, "Should have delivery data on receiver");
         Assert.AreEqual(1, deliveryCounter, "Should have delivery data on receiver");
         Assert.IsFalse(deliveryUpdated, "Should not have any delivery updates yet on receiver");

         // Second chunk indicates more to come as a twist but also signals aborted.
         if (setSettledOnAbortedTransfer.HasValue)
         {
            peer.RemoteTransfer().WithDeliveryId(0)
                                 .WithSettled((bool)setSettledOnAbortedTransfer)
                                 .WithMore(setMoreOnAbortedTransfer)
                                 .WithAborted(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();
         }
         else
         {
            peer.RemoteTransfer().WithDeliveryId(0)
                                 .WithMore(setMoreOnAbortedTransfer)
                                 .WithAborted(true)
                                 .WithMessageFormat(0)
                                 .WithPayload(payload).Now();
         }

         Assert.IsNotNull(receivedDelivery, "Should have delivery data on receiver");
         Assert.AreEqual(2, deliveryCounter, "Should have delivery data on receiver");
         Assert.IsFalse(deliveryUpdated, "Should not have a delivery updates on receiver");
         Assert.IsTrue(receivedDelivery.IsAborted, "Should now show that delivery is aborted");
         Assert.IsTrue(receivedDelivery.IsRemotelySettled, "Should now show that delivery is remotely settled");
         Assert.IsNull(receivedDelivery.ReadAll(), "Aborted Delivery should discard read bytes");

         // Another delivery now which should arrive just fine, no further frames on this one.
         peer.RemoteTransfer().WithDeliveryId(1)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Now();

         Assert.IsNotNull(abortedDelivery, "Should have one aborted delivery");
         Assert.IsNotNull(receivedDelivery, "Should have delivery data on receiver");
         Assert.AreNotSame(abortedDelivery, receivedDelivery, "Should have a  non-aborted delivery");
         Assert.AreEqual(3, deliveryCounter, "Should have delivery data on receiver");
         Assert.IsFalse(deliveryUpdated, "Should not have a delivery updates on receiver");
         Assert.IsFalse(receivedDelivery.IsAborted, "Should now show that delivery is not aborted");
         Assert.AreEqual(2, receivedDelivery.DeliveryTag.TagBuffer.GetByte(0), "Should have delivery tagged as two");

         // Test that delivery count updates correctly on next flow
         peer.ExpectFlow().WithLinkCredit(10).WithDeliveryCount(2);

         receiver.AddCredit(10);

         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestAbortedTransferRemovedFromUnsettledListOnceSettledRemoteSettles()
      {
         DoTestAbortedTransferRemovedFromUnsettledListOnceSettled(true);
      }

      [Test]
      public void TestAbortedTransferRemovedFromUnsettledListOnceSettledRemoteDoesNotSettle()
      {
         DoTestAbortedTransferRemovedFromUnsettledListOnceSettled(false);
      }

      private void DoTestAbortedTransferRemovedFromUnsettledListOnceSettled(bool remoteSettled)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("receiver");
         receiver.AddCredit(1);

         IIncomingDelivery abortedDelivery = null;
         byte[] payload = new byte[] { 1 };

         // Receiver 1 handlers for delivery processing.
         receiver.DeliveryReadHandler(delivery =>
         {
            if (delivery.IsAborted)
            {
               abortedDelivery = delivery;
            }
         });

         receiver.Open();

         // Send one chunk then abort to check that local side can settle and clear
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Now();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithSettled(remoteSettled)
                              .WithMore(false)
                              .WithAborted(true)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Now();

         Assert.IsNotNull(abortedDelivery, "should have one aborted delivery");

         Assert.IsTrue(receiver.HasUnsettled);
         Assert.AreEqual(1, CountElements<IIncomingDelivery>(receiver.Unsettled));
         abortedDelivery.Settle();
         Assert.IsFalse(receiver.HasUnsettled);
         Assert.AreEqual(0, CountElements<IIncomingDelivery>(receiver.Unsettled));

         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestDeliveryWithIdOmittedOnContinuationTransfers()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).WithName("receiver-1").Respond();
         peer.ExpectAttach().WithHandle(1).WithName("receiver-2").Respond();
         peer.ExpectFlow().WithHandle(0).WithLinkCredit(5);
         peer.ExpectFlow().WithHandle(1).WithLinkCredit(5);

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver1 = session.Receiver("receiver-1");
         IReceiver receiver2 = session.Receiver("receiver-2");

         IIncomingDelivery receivedDelivery1 = null;
         IIncomingDelivery receivedDelivery2 = null;

         int receiver1Transfers = 0;
         int receiver2Transfers = 0;

         bool delivery1Updated = false;
         bool delivery2Updated = false;

         string deliveryTag1 = "tag1";
         string deliveryTag2 = "tag2";

         // Receiver 1 handlers for delivery processing.
         receiver1.DeliveryReadHandler(delivery =>
         {
            receivedDelivery1 = delivery;
            receiver1Transfers++;
         });
         receiver1.DeliveryStateUpdatedHandler(delivery =>
         {
            delivery1Updated = true;
            receiver1Transfers++;
         });

         // Receiver 2 handlers for delivery processing.
         receiver2.DeliveryReadHandler(delivery =>
         {
            receivedDelivery2 = delivery;
            receiver2Transfers++;
         });
         receiver2.DeliveryStateUpdatedHandler(delivery =>
         {
            delivery2Updated = true;
            receiver2Transfers++;
         });

         receiver1.Open();
         receiver2.Open();

         receiver1.AddCredit(5);
         receiver2.AddCredit(5);

         Assert.IsNull(receivedDelivery1, "Should not have any delivery data yet on receiver 1");
         Assert.IsNull(receivedDelivery2, "Should not have any delivery date yet on receiver 2");
         Assert.AreEqual(0, receiver1Transfers, "Receiver 1 should not have any transfers yet");
         Assert.AreEqual(0, receiver2Transfers, "Receiver 2 should not have any transfers yet");

         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithHandle(0)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag1))
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 1 }).Now();
         peer.RemoteTransfer().WithDeliveryId(1)
                              .WithHandle(1)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag2))
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 10 }).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have a delivery event on receiver 1");
         Assert.IsNotNull(receivedDelivery2, "Should have a delivery event on receiver 2");
         Assert.AreEqual(1, receiver1Transfers, "Receiver 1 should have 1 transfers");
         Assert.AreEqual(1, receiver2Transfers, "Receiver 2 should have 1 transfers");
         Assert.AreNotSame(receivedDelivery1, receivedDelivery2);

         peer.RemoteTransfer().WithHandle(1)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag2))
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 11 }).Now();
         peer.RemoteTransfer().WithHandle(0)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag1))
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 2 }).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have a delivery event on receiver 1");
         Assert.IsNotNull(receivedDelivery2, "Should have a delivery event on receiver 2");
         Assert.AreEqual(2, receiver1Transfers, "Receiver 1 should have 2 transfers");
         Assert.AreEqual(2, receiver2Transfers, "Receiver 2 should have 2 transfers");
         Assert.AreNotSame(receivedDelivery1, receivedDelivery2);

         peer.RemoteTransfer().WithHandle(0)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag1))
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 3 }).Now();
         peer.RemoteTransfer().WithHandle(1)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag2))
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 12 }).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have a delivery event on receiver 1");
         Assert.IsNotNull(receivedDelivery2, "Should have a delivery event on receiver 2");
         Assert.AreEqual(3, receiver1Transfers, "Receiver 1 should have 3 transfers");
         Assert.AreEqual(3, receiver2Transfers, "Receiver 2 should have 3 transfers");
         Assert.AreNotSame(receivedDelivery1, receivedDelivery2);

         peer.RemoteTransfer().WithHandle(1)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag2))
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 13 }).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have a delivery event on receiver 1");
         Assert.IsNotNull(receivedDelivery2, "Should have a delivery event on receiver 2");
         Assert.AreEqual(3, receiver1Transfers, "Receiver 1 should have 3 transfers");
         Assert.AreEqual(4, receiver2Transfers, "Receiver 2 should have 4 transfers");
         Assert.AreNotSame(receivedDelivery1, receivedDelivery2);
         Assert.IsFalse(receivedDelivery1.IsPartial, "Delivery on Receiver 1 Should be complete");
         Assert.IsFalse(receivedDelivery2.IsPartial, "Delivery on Receiver 2 Should be complete");
         Assert.IsFalse(delivery1Updated);
         Assert.IsFalse(delivery2Updated);

         Assert.AreEqual(Encoding.UTF8.GetBytes(deliveryTag1), receivedDelivery1.DeliveryTag.TagBytes);
         Assert.AreEqual(Encoding.UTF8.GetBytes(deliveryTag2), receivedDelivery2.DeliveryTag.TagBytes);

         IProtonBuffer delivery1Buffer = receivedDelivery1.ReadAll();
         IProtonBuffer delivery2Buffer = receivedDelivery2.ReadAll();

         for (int i = 1; i < 4; ++i)
         {
            Assert.AreEqual(i, delivery1Buffer.ReadByte());
         }

         for (int i = 10; i < 14; ++i)
         {
            Assert.AreEqual(i, delivery2Buffer.ReadByte());
         }

         Assert.IsNull(receivedDelivery1.ReadAll());
         Assert.IsNull(receivedDelivery2.ReadAll());

         peer.ExpectDetach().WithHandle(0).Respond();
         peer.ExpectDetach().WithHandle(1).Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         receiver1.Close();
         receiver2.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestDeliveryIdThresholdsAndWraps()
      {
         // Check start from 0
         DoDeliveryIdThresholdsWrapsTestImpl(0u, 1u, 2u);
         // Check run up to max-int (interesting boundary for underlying impl)
         DoDeliveryIdThresholdsWrapsTestImpl(int.MaxValue - 2, int.MaxValue - 1, int.MaxValue);
         // Check crossing from signed range value into unsigned range value (interesting boundary for underlying impl)
         DoDeliveryIdThresholdsWrapsTestImpl(int.MaxValue, (uint.MaxValue / 2) + 1, (uint.MaxValue / 2) + 2);
         // Check run up to max-uint
         DoDeliveryIdThresholdsWrapsTestImpl(uint.MaxValue - 2, uint.MaxValue - 1, uint.MaxValue);
         // Check wrapping from max unsigned value back to min(/0).
         DoDeliveryIdThresholdsWrapsTestImpl(uint.MaxValue, 0u, 1u);
      }

      private void DoDeliveryIdThresholdsWrapsTestImpl(uint deliveryId1, uint deliveryId2, uint deliveryId3)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond().WithNextOutgoingId(deliveryId1);
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(5);

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("receiver");

         IIncomingDelivery receivedDelivery1 = null;
         IIncomingDelivery receivedDelivery2 = null;
         IIncomingDelivery receivedDelivery3 = null;

         int deliveryCounter = 0;

         string deliveryTag1 = "tag1";
         string deliveryTag2 = "tag2";
         string deliveryTag3 = "tag3";

         // Receiver handlers for delivery processing.
         receiver.DeliveryReadHandler(delivery =>
         {
            switch (deliveryCounter)
            {
               case 0:
                  receivedDelivery1 = delivery;
                  break;
               case 1:
                  receivedDelivery2 = delivery;
                  break;
               case 2:
                  receivedDelivery3 = delivery;
                  break;
               default:
                  break;
            }
            deliveryCounter++;
         });
         receiver.DeliveryStateUpdatedHandler(delivery =>
         {
            deliveryCounter++;
         });

         receiver.Open();
         receiver.AddCredit(5);

         Assert.IsNull(receivedDelivery1, "Should not have received delivery 1");
         Assert.IsNull(receivedDelivery2, "Should not have received delivery 2");
         Assert.IsNull(receivedDelivery3, "Should not have received delivery 3");
         Assert.AreEqual(0, deliveryCounter, "Receiver should not have any deliveries yet");

         peer.RemoteTransfer().WithDeliveryId(deliveryId1)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag1))
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 1 }).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have received delivery 1");
         Assert.IsNull(receivedDelivery2, "Should not have received delivery 2");
         Assert.IsNull(receivedDelivery3, "Should not have received delivery 3");
         Assert.AreEqual(1, deliveryCounter, "Receiver should have 1 deliveries now");

         peer.RemoteTransfer().WithDeliveryId(deliveryId2)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag2))
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 2 }).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have received delivery 1");
         Assert.IsNotNull(receivedDelivery2, "Should have received delivery 2");
         Assert.IsNull(receivedDelivery3, "Should not have received delivery 3");
         Assert.AreEqual(2, deliveryCounter, "Receiver should have 2 deliveries now");

         peer.RemoteTransfer().WithDeliveryId(deliveryId3)
                              .WithDeliveryTag(Encoding.UTF8.GetBytes(deliveryTag3))
                              .WithMessageFormat(0)
                              .WithPayload(new byte[] { 3 }).Now();

         Assert.IsNotNull(receivedDelivery1, "Should have received delivery 1");
         Assert.IsNotNull(receivedDelivery2, "Should have received delivery 2");
         Assert.IsNotNull(receivedDelivery3, "Should have received delivery 3");
         Assert.AreEqual(3, deliveryCounter, "Receiver should have 3 deliveries now");

         Assert.AreNotSame(receivedDelivery1, receivedDelivery2, "delivery duplicate detected");
         Assert.AreNotSame(receivedDelivery2, receivedDelivery3, "delivery duplicate detected");
         Assert.AreNotSame(receivedDelivery1, receivedDelivery3, "delivery duplicate detected");

         // Verify deliveries arrived with expected payload
         Assert.AreEqual(Encoding.UTF8.GetBytes(deliveryTag1), receivedDelivery1.DeliveryTag.TagBytes);
         Assert.AreEqual(Encoding.UTF8.GetBytes(deliveryTag2), receivedDelivery2.DeliveryTag.TagBytes);
         Assert.AreEqual(Encoding.UTF8.GetBytes(deliveryTag3), receivedDelivery3.DeliveryTag.TagBytes);

         IProtonBuffer delivery1Buffer = receivedDelivery1.ReadAll();
         IProtonBuffer delivery2Buffer = receivedDelivery2.ReadAll();
         IProtonBuffer delivery3Buffer = receivedDelivery3.ReadAll();

         Assert.AreEqual(1, delivery1Buffer.ReadByte(), "Delivery 1 payload not as expected");
         Assert.AreEqual(2, delivery2Buffer.ReadByte(), "Delivery 2 payload not as expected");
         Assert.AreEqual(3, delivery3Buffer.ReadByte(), "Delivery 3 payload not as expected");

         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverFlowSentAfterAttachWrittenWhenCreditPrefilled()
      {
         DoTestReceiverFlowSentAfterAttachWritten(true);
      }

      [Test]
      public void TestReceiverFlowSentAfterAttachWrittenWhenCreditAddedBeforeAttachResponse()
      {
         DoTestReceiverFlowSentAfterAttachWritten(false);
      }

      private void DoTestReceiverFlowSentAfterAttachWritten(bool creditBeforeOpen)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();

         IReceiver receiver = session.Receiver("receiver");

         if (creditBeforeOpen)
         {
            // Add credit before open, no frame should be written until opened.
            receiver.AddCredit(5);
         }

         // Expect attach but don't respond to observe that flow is sent regardless.
         peer.WaitForScriptToComplete();
         peer.ExpectAttach();
         peer.ExpectFlow().WithLinkCredit(5).WithDeliveryCount(Is.NullValue());

         receiver.Open();

         if (!creditBeforeOpen)
         {
            // Add credit after open, frame should be written regardless of no attach response
            receiver.AddCredit(5);
         }

         peer.RespondToLastAttach().Now();
         peer.WaitForScriptToComplete();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         receiver.Detach();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverHandlesDeferredOpenAndBeginAttachResponses()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool receiverRemotelyOpened = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();
         peer.ExpectBegin();
         peer.ExpectAttach().OfReceiver()
                            .WithSource().WithDynamic(true)
                            .WithAddress((string)null);

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("receiver-1");
         receiver.Source = new Source();
         receiver.Source.Dynamic = true;
         receiver.Source.Address = null;
         receiver.OpenHandler((result) => receiverRemotelyOpened = true).Open();

         peer.WaitForScriptToComplete();

         // This should happen after we inject the held open and attach
         peer.ExpectClose().Respond();

         // Inject held responses to get the ball rolling again
         peer.RemoteOpen().WithOfferedCapabilities("ANONYMOUS_RELAY").Now();
         peer.RespondToLastBegin().Now();
         peer.RespondToLastAttach().Now();

         Assert.IsTrue(receiverRemotelyOpened, "Receiver remote opened event did not fire");
         Assert.IsNotNull(receiver.RemoteSource.Address);

         connection.Close();

         peer.WaitForScriptToComplete();
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenAndBeginWrittenAndResponseAttachWrittenAndResponse()
      {
         TestCloseAfterShutdownNoOutputAndNoException(true, true, true, true);
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenAndBeginWrittenAndResponseAttachWrittenAndNoResponse()
      {
         TestCloseAfterShutdownNoOutputAndNoException(true, true, true, false);
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenWrittenAndResponseBeginWrittenAndNoResponse()
      {
         TestCloseAfterShutdownNoOutputAndNoException(true, true, false, false);
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenWrittenButNoResponse()
      {
         TestCloseAfterShutdownNoOutputAndNoException(true, false, false, false);
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenNotWritten()
      {
         TestCloseAfterShutdownNoOutputAndNoException(false, false, false, false);
      }

      private void TestCloseAfterShutdownNoOutputAndNoException(bool respondToHeader, bool respondToOpen, bool respondToBegin, bool respondToAttach)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         if (respondToHeader)
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            if (respondToOpen)
            {
               peer.ExpectOpen().Respond();
               if (respondToBegin)
               {
                  peer.ExpectBegin().Respond();
                  if (respondToAttach)
                  {
                     peer.ExpectAttach().Respond();
                  }
                  else
                  {
                     peer.ExpectAttach();
                  }
               }
               else
               {
                  peer.ExpectBegin();
                  peer.ExpectAttach();
               }
            }
            else
            {
               peer.ExpectOpen();
               peer.ExpectBegin();
               peer.ExpectAttach();
            }
         }
         else
         {
            peer.ExpectAMQPHeader();
         }

         IConnection connection = engine.Start();
         connection.Open();

         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         engine.Shutdown();

         // Should clean up and not throw as we knowingly shutdown engine operations.
         receiver.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenAndBeginWrittenAndResponseAttachWrittenAndResponse()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(true, true, true, true);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenAndBeginWrittenAndResponseAttachWrittenAndNoResponse()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(true, true, true, false);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenWrittenAndResponseBeginWrittenAndNoResponse()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(true, true, true, false);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenWrittenButNoResponse()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(true, false, false, false);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenNotWritten()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(false, false, false, false);
      }

      private void TestCloseAfterEngineFailedThrowsAndNoOutputWritten(bool respondToHeader, bool respondToOpen, bool respondToBegin, bool respondToAttach)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         if (respondToHeader)
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            if (respondToOpen)
            {
               peer.ExpectOpen().Respond();
               if (respondToBegin)
               {
                  peer.ExpectBegin().Respond();
                  if (respondToAttach)
                  {
                     peer.ExpectAttach().Respond();
                  }
                  else
                  {
                     peer.ExpectAttach();
                  }
               }
               else
               {
                  peer.ExpectBegin();
                  peer.ExpectAttach();
               }
               peer.ExpectClose();
            }
            else
            {
               peer.ExpectOpen();
               peer.ExpectBegin();
               peer.ExpectAttach();
               peer.ExpectClose();
            }
         }
         else
         {
            peer.ExpectAMQPHeader();
         }

         IConnection connection = engine.Start();
         connection.Open();

         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         engine.EngineFailed(new IOException());

         try
         {
            receiver.Close();
            Assert.Fail("Should throw exception indicating engine is in a failed state.");
         }
         catch (EngineFailedException) { }

         try
         {
            session.Close();
            Assert.Fail("Should throw exception indicating engine is in a failed state.");
         }
         catch (EngineFailedException) { }

         try
         {
            connection.Close();
            Assert.Fail("Should throw exception indicating engine is in a failed state.");
         }
         catch (EngineFailedException) { }

         engine.Shutdown();  // Explicit shutdown now allows local close to complete

         // Should clean up and not throw as we knowingly shutdown engine operations.
         receiver.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestCloseReceiverWithErrorCondition()
      {
         DoTestCloseOrDetachWithErrorCondition(true);
      }

      [Test]
      public void TestDetachReceiverWithErrorCondition()
      {
         DoTestCloseOrDetachWithErrorCondition(false);
      }

      public void DoTestCloseOrDetachWithErrorCondition(bool close)
      {
         string condition = "amqp:link:detach-forced";
         string description = "something bad happened.";

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectDetach().WithClosed(close)
                            .WithError(condition, description)
                            .Respond();
         peer.ExpectClose();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.Open();

         IReceiver receiver = session.Receiver("receiver-1");
         receiver.Open();
         receiver.ErrorCondition = new ErrorCondition(Symbol.Lookup(condition), description);

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
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterReceiverLocallyClosed()
      {
         DoTestReceiverAddCreditFailsWhenLinkIsNotOperable(true, false, false);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterReceiverLocallyDetached()
      {
         DoTestReceiverAddCreditFailsWhenLinkIsNotOperable(true, false, true);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterReceiverRemotelyClosed()
      {
         DoTestReceiverAddCreditFailsWhenLinkIsNotOperable(false, true, false);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterReceiverRemotelyDetached()
      {
         DoTestReceiverAddCreditFailsWhenLinkIsNotOperable(false, true, true);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterReceiverFullyClosed()
      {
         DoTestReceiverAddCreditFailsWhenLinkIsNotOperable(true, true, false);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterReceiverFullyDetached()
      {
         DoTestReceiverAddCreditFailsWhenLinkIsNotOperable(true, true, true);
      }

      private void DoTestReceiverAddCreditFailsWhenLinkIsNotOperable(bool localClose, bool RemoteClose, bool detach)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         if (localClose)
         {
            if (RemoteClose)
            {
               peer.ExpectDetach().Respond();
            }
            else
            {
               peer.ExpectDetach();
            }

            if (detach)
            {
               receiver.Detach();
            }
            else
            {
               receiver.Close();
            }
         }
         else if (RemoteClose)
         {
            peer.RemoteDetach().WithClosed(!detach).Now();
         }

         try
         {
            receiver.AddCredit(2);
            Assert.Fail("Receiver should not allow addCredit to be called");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterSessionLocallyClosed()
      {
         DoTestReceiverAddCreditFailsWhenSessionNotOperable(true, false);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterSessionRemotelyClosed()
      {
         DoTestReceiverAddCreditFailsWhenSessionNotOperable(false, true);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterSessionFullyClosed()
      {
         DoTestReceiverAddCreditFailsWhenSessionNotOperable(true, true);
      }

      private void DoTestReceiverAddCreditFailsWhenSessionNotOperable(bool localClose, bool RemoteClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         if (localClose)
         {
            if (RemoteClose)
            {
               peer.ExpectEnd().Respond();
            }
            else
            {
               peer.ExpectEnd();
            }

            session.Close();
         }
         else if (RemoteClose)
         {
            peer.RemoteEnd().Now();
         }

         try
         {
            receiver.AddCredit(2);
            Assert.Fail("Receiver should not allow addCredit to be called");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterConnectionLocallyClosed()
      {
         DoTestReceiverAddCreditFailsWhenConnectionNotOperable(true, false);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterConnectionRemotelyClosed()
      {
         DoTestReceiverAddCreditFailsWhenConnectionNotOperable(false, true);
      }

      [Test]
      public void TestReceiverAddCreditFailsAfterConnectionFullyClosed()
      {
         DoTestReceiverAddCreditFailsWhenConnectionNotOperable(true, true);
      }

      private void DoTestReceiverAddCreditFailsWhenConnectionNotOperable(bool localClose, bool RemoteClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         if (localClose)
         {
            if (RemoteClose)
            {
               peer.ExpectClose().Respond();
            }
            else
            {
               peer.ExpectClose();
            }

            connection.Close();
         }
         else if (RemoteClose)
         {
            peer.RemoteClose().Now();
         }

         try
         {
            receiver.AddCredit(2);
            Assert.Fail("Receiver should not allow addCredit to be called");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDispositionFailsAfterReceiverLocallyClosed()
      {
         DoTestReceiverDispositionFailsWhenLinkIsNotOperable(true, false, false);
      }

      [Test]
      public void TestReceiverDispositionFailsAfterReceiverLocallyDetached()
      {
         DoTestReceiverDispositionFailsWhenLinkIsNotOperable(true, false, true);
      }

      [Test]
      public void TestReceiverDispositionFailsAfterReceiverRemotelyClosed()
      {
         DoTestReceiverDispositionFailsWhenLinkIsNotOperable(false, true, false);
      }

      [Test]
      public void TestReceiverDispositionFailsAfterReceiverRemotelyDetached()
      {
         DoTestReceiverDispositionFailsWhenLinkIsNotOperable(false, true, true);
      }

      [Test]
      public void TestReceiverDispositionFailsAfterReceiverFullyClosed()
      {
         DoTestReceiverDispositionFailsWhenLinkIsNotOperable(true, true, false);
      }

      [Test]
      public void TestReceiverDispositionFailsAfterReceiverFullyDetached()
      {
         DoTestReceiverDispositionFailsWhenLinkIsNotOperable(true, true, true);
      }

      private void DoTestReceiverDispositionFailsWhenLinkIsNotOperable(bool localClose, bool RemoteClose, bool detach)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start();
         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");
         receiver.Open();

         // Should no-op with no deliveries
         receiver.Disposition(delivery => true, Accepted.Instance, true);

         if (localClose)
         {
            if (RemoteClose)
            {
               peer.ExpectDetach().Respond();
            }
            else
            {
               peer.ExpectDetach();
            }

            if (detach)
            {
               receiver.Detach();
            }
            else
            {
               receiver.Close();
            }
         }
         else if (RemoteClose)
         {
            peer.RemoteDetach().WithClosed(!detach).Now();
         }

         try
         {
            receiver.Disposition(delivery => true, Accepted.Instance, true);
            Assert.Fail("Receiver should not allow disposition to be called");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestDrainCreditAmountLessThanCurrentCreditThrowsIAE()
      {
         DoTestReceiverDrainThrowsIAEForCertainDrainAmountScenarios(10, 1);
      }

      private void DoTestReceiverDrainThrowsIAEForCertainDrainAmountScenarios(uint credit, uint drain)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();

         if (credit > 0)
         {
            peer.ExpectFlow().WithDrain(false).WithLinkCredit(credit);
         }

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open().AddCredit(credit);

         peer.WaitForScriptToComplete();

         // Check that calling drain sends flow, and calls handler on response draining all credit
         bool handlerCalled = false;
         receiver.CreditStateUpdateHandler(x =>
         {
            handlerCalled = true;
         });

         try
         {
            receiver.Drain(drain);
            Assert.Fail("Should not be able to drain given amount");
         }
         catch (ArgumentException) { }

         peer.WaitForScriptToComplete();
         Assert.IsFalse(handlerCalled, "Handler was called when no flow expected");

         peer.ExpectDetach().Respond();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestDrainRequestWithNoCreditPendingAndAmountRequestedAsZero()
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

         peer.WaitForScriptToComplete();

         // Check that calling drain sends flow, and calls handler on response draining all credit
         bool handlerCalled = false;
         receiver.CreditStateUpdateHandler(x =>
         {
            handlerCalled = true;
         });

         Assert.IsFalse(receiver.Drain(0));

         peer.WaitForScriptToComplete();
         Assert.IsFalse(handlerCalled, "Handler was not called");

         peer.ExpectDetach().Respond();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverDrainWithCreditsWhenNoCreditOutstanding()
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

         peer.WaitForScriptToComplete();

         uint drainAmount = 100;

         // Check that calling drain sends flow, and calls handler on response draining all credit
         bool handlerCalled = false;
         receiver.CreditStateUpdateHandler(x =>
         {
            handlerCalled = true;
         });

         peer.ExpectFlow().WithDrain(true).WithLinkCredit(drainAmount).WithDeliveryCount(0)
                          .Respond()
                          .WithDrain(true).WithLinkCredit(0).WithDeliveryCount(drainAmount);

         receiver.Drain(drainAmount);

         peer.WaitForScriptToComplete();
         Assert.IsTrue(handlerCalled, "Handler was not called");

         peer.ExpectDetach().Respond();
         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiveComplexEncodedAMQPMessageAndDecode()
      {
         string SERIALIZED_JAVA_OBJECT_CONTENT_TYPE = "application/x-java-serialized-object";
         string JMS_MSG_TYPE = "x-opt-jms-msg-type";

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithDrain(false).WithLinkCredit(1);

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open().AddCredit(1);

         peer.WaitForScriptToComplete();

         IIncomingDelivery received = null;
         receiver.DeliveryReadHandler(delivery =>
         {
            received = delivery;
            delivery.Disposition(Accepted.Instance, true);
         });

         Guid expectedContent = Guid.NewGuid();

         MemoryStream stream = new MemoryStream();
         string serialized = JsonSerializer.Serialize(expectedContent);
         stream.Write(Encoding.UTF8.GetBytes(serialized));
         byte[] bytes = stream.ToArray();

         peer.ExpectDisposition().WithState().Accepted().WithSettled(true);
         peer.RemoteTransfer().WithDeliveryTag(new byte[] { 0 })
                              .WithDeliveryId(0)
                              .WithProperties().WithContentType(SERIALIZED_JAVA_OBJECT_CONTENT_TYPE).Also()
                              .WithMessageAnnotations().WithAnnotation("x-opt-jms-msg-type", (byte)1).Also()
                              .WithBody().WithData(bytes).Also()
                              .Now();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(received);

         IProtonBuffer buffer = received.ReadAll();

         MessageAnnotations annotations;
         Properties properties;
         ISection body;

         try
         {
            annotations = (MessageAnnotations)decoder.ReadObject(buffer, decoderState);
            Assert.IsNotNull(annotations);
            Assert.IsTrue(annotations.Value.ContainsKey(Symbol.Lookup(JMS_MSG_TYPE)));
            decoderState.Reset();
         }
         catch (Exception ex)
         {
            Assert.Fail("Should not encounter error on decode of MessageAnnotations: " + ex);
         }

         try
         {
            properties = (Properties)decoder.ReadObject(buffer, decoderState);
            Assert.IsNotNull(properties);
            Assert.AreEqual(SERIALIZED_JAVA_OBJECT_CONTENT_TYPE, properties.ContentType);
            decoderState.Reset();
         }
         catch (Exception ex)
         {
            Assert.Fail("Should not encounter error on decode of Properties: " + ex);
         }

         try
         {
            body = (ISection)decoder.ReadObject(buffer, decoderState);
            Assert.IsNotNull(body);
            Assert.IsTrue(body is Data);
            Data payload = (Data)body;
            Assert.AreEqual(bytes.Length, payload.Buffer.ReadableBytes);
            decoderState.Reset();
         }
         catch (Exception ex)
         {
            Assert.Fail("Should not encounter error on decode of Body section: " + ex);
         }

         peer.ExpectClose().Respond();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverCreditNotClearedUntilClosedAfterRemoteClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(10);
         peer.RemoteDetach().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open().AddCredit(10);

         peer.WaitForScriptToComplete();
         peer.ExpectDetach();

         Assert.AreEqual(10, receiver.Credit);
         receiver.Close();
         Assert.AreEqual(0, receiver.Credit);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverCreditNotClearedUntilClosedAfterSessionRemoteClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(10);
         peer.RemoteEnd().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open().AddCredit(10);

         peer.WaitForScriptToComplete();
         peer.ExpectDetach();

         Assert.AreEqual(10, receiver.Credit);
         receiver.Close();
         Assert.AreEqual(0, receiver.Credit);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverCreditNotClearedUntilClosedAfterConnectionRemoteClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(10);
         peer.RemoteClose().Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open().AddCredit(10);

         peer.WaitForScriptToComplete();
         peer.ExpectDetach();

         Assert.AreEqual(10, receiver.Credit);
         receiver.Close();
         Assert.AreEqual(0, receiver.Credit);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverCreditNotClearedUntilClosedAfterEngineShutdown()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(10);

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test").Open().AddCredit(10);

         peer.WaitForScriptToComplete();

         engine.Shutdown();

         Assert.AreEqual(10, receiver.Credit);
         receiver.Close();
         Assert.AreEqual(0, receiver.Credit);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverHonorsDeliverySetEventHandlers()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithMessageFormat(0).Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.RemoteDisposition().FromSender()
                                 .WithSettled(true)
                                 .WithState().Accepted()
                                 .WithFirst(0).Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;
         int additionalDeliveryCounter = 0;
         int dispositionCounter = 0;

         List<IIncomingDelivery> deliveries = new List<IIncomingDelivery>();

         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryCounter++;
            delivery.DeliveryReadHandler((target) =>
            {
               additionalDeliveryCounter++;
            });
            delivery.DeliveryStateUpdatedHandler((target) =>
            {
               dispositionCounter++;
               deliveries.Add(delivery);
            });
         });

         receiver.DeliveryStateUpdatedHandler((delivery) =>
         {
            Assert.Fail("Should not have updated this handler.");
         });

         receiver.Open();
         receiver.AddCredit(2);
         receiver.Close();

         Assert.AreEqual(1, deliveryCounter, "Should only be one initial delivery");
         Assert.AreEqual(1, additionalDeliveryCounter, "Should be a second delivery update at the delivery handler");
         Assert.AreEqual(1, dispositionCounter, "Not all deliveries received dispositions");

         byte deliveryTag = 0;

         foreach (IIncomingDelivery delivery in deliveries)
         {
            Assert.AreEqual(deliveryTag++, delivery.DeliveryTag.TagBuffer.GetByte(0), "Delivery not updated in correct order");
            Assert.IsTrue(delivery.IsRemotelySettled, "Delivery should be marked as remotely settled");
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiverAbortedHandlerCalledWhenSet()
      {
         DoTestReceiverReadHandlerOrAbortHandlerCalled(true);
      }

      [Test]
      public void TestReceiverReadHandlerCalledForAbortWhenAbortedNotSet()
      {
         DoTestReceiverReadHandlerOrAbortHandlerCalled(false);
      }

      private void DoTestReceiverReadHandlerOrAbortHandlerCalled(bool setAbortHandler)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithMessageFormat(0).Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithAborted(true)
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;
         int deliveryAbortedInReadEventCounter = 0;
         int deliveryAbortedCounter = 0;

         receiver.DeliveryReadHandler(delivery =>
         {
            if (delivery.IsAborted)
            {
               deliveryAbortedInReadEventCounter++;
            }
            else
            {
               deliveryCounter++;
            }
         });

         if (setAbortHandler)
         {
            receiver.DeliveryAbortedHandler(delivery =>
            {
               deliveryAbortedCounter++;
            });
         }

         receiver.DeliveryStateUpdatedHandler((delivery) =>
         {
            Assert.Fail("Should not have updated this handler.");
         });

         receiver.Open();
         receiver.AddCredit(2);
         receiver.Close();

         Assert.AreEqual(1, deliveryCounter, "Should only be one initial delivery");
         if (setAbortHandler)
         {
            Assert.AreEqual(0, deliveryAbortedInReadEventCounter, "Should be no aborted delivery in read event");
            Assert.AreEqual(1, deliveryAbortedCounter, "Should only be one aborted delivery events");
         }
         else
         {
            Assert.AreEqual(1, deliveryAbortedInReadEventCounter, "Should only be no aborted delivery in read event");
            Assert.AreEqual(0, deliveryAbortedCounter, "Should be no aborted delivery events");
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestIncomingDeliveryReadEventSignaledWhenNoAbortedHandlerSet()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithMessageFormat(0).Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithAborted(true)
                              .WithMore(false)
                              .WithMessageFormat(0).Queue();
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session();
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;
         int deliveryAbortedCounter = 0;

         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryCounter++;
            delivery.DeliveryReadHandler((target) =>
            {
               if (target.IsAborted)
               {
                  deliveryAbortedCounter++;
               }
            });
         });

         receiver.DeliveryStateUpdatedHandler((delivery) =>
         {
            Assert.Fail("Should not have updated this handler.");
         });

         receiver.Open();
         receiver.AddCredit(2);
         receiver.Close();

         Assert.AreEqual(1, deliveryCounter, "Should only be one initial delivery");
         Assert.AreEqual(1, deliveryAbortedCounter, "Should only be one aborted delivery");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionWindowOpenedAfterDeliveryRead()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().WithIncomingWindow(1).Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.ExpectFlow().WithLinkCredit(2).WithIncomingWindow(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithPayload(payload)
                              .WithMessageFormat(0).Queue();
         peer.ExpectFlow().WithLinkCredit(1).WithIncomingWindow(1);
         peer.RemoteTransfer().WithDeliveryId(1)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithPayload(payload)
                              .WithMessageFormat(0).Queue();
         peer.ExpectFlow().WithLinkCredit(0).WithIncomingWindow(1);
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.IncomingCapacity = 1024;
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;

         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryCounter++;
            delivery.ReadAll();
         });

         receiver.Open();
         receiver.AddCredit(2);
         receiver.Close();

         Assert.AreEqual(2, deliveryCounter, "Should be two deliveries");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionWindowOpenedAfterDeliveryReadFromSplitFramedTransfer()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().WithIncomingWindow(1).Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.ExpectFlow().WithLinkCredit(2).WithIncomingWindow(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithPayload(payload)
                              .WithMessageFormat(0).Queue();
         peer.ExpectFlow().WithLinkCredit(2).WithIncomingWindow(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(true)
                              .WithPayload(payload)
                              .WithMessageFormat(0).Queue();
         peer.ExpectFlow().WithLinkCredit(3).WithIncomingWindow(0);
         peer.ExpectFlow().WithLinkCredit(3).WithIncomingWindow(1);
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.IncomingCapacity = 1024;
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;
         IIncomingDelivery delivery = null;

         receiver.DeliveryReadHandler(incoming =>
         {
            if (deliveryCounter++ == 0)
            {
               delivery = incoming;
               delivery.ReadAll();
            }
         });

         receiver.Open();
         receiver.AddCredit(2);

         Assert.AreEqual(2, deliveryCounter, "Should be two deliveries");
         Assert.IsTrue(delivery.Available > 0);

         receiver.AddCredit(1);

         delivery.ReadAll();

         receiver.Close();

         Assert.AreEqual(2, deliveryCounter, "Should be two deliveries");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestIncomingDeliveryTracksTransferInCount()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.ExpectFlow().WithLinkCredit(2).WithIncomingWindow(1);
         peer.ExpectDetach().Respond();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.IncomingCapacity = 1024;
         session.Open();
         IReceiver receiver = session.Receiver("test");

         IIncomingDelivery received = null;

         receiver.DeliveryReadHandler(delivery =>
         {
            received = delivery;
         });

         receiver.Open();
         receiver.AddCredit(2);

         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithPayload(payload).Now();

         Assert.IsNotNull(received);
         Assert.AreEqual(1, received.TransferCount);

         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithMore(false)
                              .WithPayload(payload).Now();

         Assert.IsNotNull(received);
         Assert.AreEqual(2, received.TransferCount);

         receiver.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSettleDeliveryAfterEngineShutdown()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IIncomingDelivery receivedDelivery = null;
         byte[] payload = new byte[] { 1 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("receiver");
         receiver.AddCredit(1);

         // Receiver 1 handlers for delivery processing.
         receiver.DeliveryReadHandler(delivery =>
         {
            receivedDelivery = delivery;
         });

         receiver.Open();

         peer.WaitForScriptToComplete();

         engine.Shutdown();

         try
         {
            receivedDelivery.Settle();
            Assert.Fail("Should not allow for settlement since engine was manually shut down");
         }
         catch (EngineShutdownException) { }

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReadAllDeliveryDataWhenSessionWindowInForceAndLinkIsClosed()
      {
         TestReadAllDeliveryDataWhenSessionWindowInForceButLinkCannotWrite(true, false, false, false);
      }

      [Test]
      public void TestReadAllDeliveryDataWhenSessionWindowInForceAndSessionIsClosed()
      {
         TestReadAllDeliveryDataWhenSessionWindowInForceButLinkCannotWrite(false, true, false, false);
      }

      [Test]
      public void TestReadAllDeliveryDataWhenSessionWindowInForceAndConnectionIsClosed()
      {
         TestReadAllDeliveryDataWhenSessionWindowInForceButLinkCannotWrite(false, false, true, false);
      }

      [Test]
      public void TestReadAllDeliveryDataWhenSessionWindowInForceAndEngineIsShutdown()
      {
         TestReadAllDeliveryDataWhenSessionWindowInForceButLinkCannotWrite(false, false, false, true);
      }

      private void TestReadAllDeliveryDataWhenSessionWindowInForceButLinkCannotWrite(bool closeLink, bool closeSession, bool closeConnection, bool shutdown)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().WithIncomingWindow(1).Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.ExpectFlow().WithLinkCredit(2).WithIncomingWindow(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithPayload(payload)
                              .WithMessageFormat(0).Queue();
         peer.ExpectFlow().WithLinkCredit(2).WithIncomingWindow(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithPayload(payload)
                              .WithMessageFormat(0).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.IncomingCapacity = 1024;
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;
         IIncomingDelivery delivery = null;

         receiver.DeliveryReadHandler(incoming =>
         {
            if (deliveryCounter++ == 0)
            {
               delivery = incoming;
               incoming.ReadAll();
            }
         });

         receiver.Open();
         receiver.AddCredit(2);

         peer.WaitForScriptToComplete();

         if (closeLink)
         {
            peer.ExpectDetach().WithClosed(true).Respond();
            receiver.Close();
         }
         if (closeSession)
         {
            peer.ExpectEnd().Respond();
            session.Close();
         }
         if (closeConnection)
         {
            peer.ExpectClose().Respond();
            connection.Close();
         }
         if (shutdown)
         {
            engine.Shutdown();
         }

         Assert.IsNotNull(delivery);
         Assert.AreEqual(2, deliveryCounter, "Should only be one initial delivery");

         delivery.ReadAll();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestWalkUnsettledAfterReceivingTransfersThatCrossSignedIntDeliveryIdRange()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 1 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond().WithNextOutgoingId(int.MaxValue);
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);
         peer.RemoteTransfer().WithDeliveryId(int.MaxValue)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.RemoteTransfer().WithDeliveryId((uint)int.MaxValue + 1)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("receiver");

         receiver.AddCredit(2);
         receiver.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithFirst(int.MaxValue)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst((uint)int.MaxValue + 1)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(receiver.HasUnsettled);
         Assert.AreEqual(2, CountElements(receiver.Unsettled));
         receiver.Disposition((delivery) => true, Accepted.Instance, true);

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestUnsettledCollectionDispositionsAfterReceivingTransfersThatCrossSignedIntDeliveryIdRange()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 1 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond().WithNextOutgoingId(int.MaxValue);
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2);
         peer.RemoteTransfer().WithDeliveryId(int.MaxValue)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.RemoteTransfer().WithDeliveryId((uint)int.MaxValue + 1)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("receiver");

         receiver.AddCredit(2);
         receiver.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithFirst(int.MaxValue)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst((uint)int.MaxValue + 1)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(receiver.HasUnsettled);
         Assert.AreEqual(2, CountElements(receiver.Unsettled));
         foreach (IIncomingDelivery delivery in receiver.Unsettled)
         {
            delivery.Disposition(Accepted.Instance, true);
         }

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestWalkUnsettledAfterReceivingTransfersThatCrossUnsignedIntDeliveryIdRange()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 1 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond().WithNextOutgoingId(uint.MaxValue);
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(3);
         peer.RemoteTransfer().WithDeliveryId(uint.MaxValue)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.RemoteTransfer().WithDeliveryId(1)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("receiver");

         receiver.AddCredit(3);
         receiver.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithFirst(uint.MaxValue)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst(1)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(receiver.HasUnsettled);
         Assert.AreEqual(3, CountElements(receiver.Unsettled));
         receiver.Disposition((delivery) => true, Accepted.Instance, true);

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestUnsettledCollectionDispositionAfterReceivingTransfersThatCrossUnsignedIntDeliveryIdRange()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 1 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond().WithNextOutgoingId(uint.MaxValue);
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(3);
         peer.RemoteTransfer().WithDeliveryId(uint.MaxValue)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.RemoteTransfer().WithDeliveryId(1)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("receiver");

         receiver.AddCredit(3);
         receiver.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDisposition().WithFirst(uint.MaxValue)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDisposition().WithFirst(1)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         Assert.IsTrue(receiver.HasUnsettled);
         Assert.AreEqual(3, CountElements(receiver.Unsettled));
         foreach (IIncomingDelivery delivery in receiver.Unsettled)
         {
            delivery.Disposition(Accepted.Instance, true);
         }

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestIncomingWindowRefilledWithBytesPreviouslyReadOnAbortedTransfer()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[256];
         Array.Fill(payload, (byte)127);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().WithIncomingWindow(2).Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(2).WithNextIncomingId(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(true)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();
         session.IncomingCapacity = connection.MaxFrameSize * 2;
         session.Open();
         IReceiver receiver = session.Receiver("test");

         int deliveryCounter = 0;
         int deliveryAbortedCounter = 0;

         receiver.DeliveryReadHandler(delivery =>
         {
            deliveryCounter++;
            if (delivery.IsAborted)
            {
               deliveryAbortedCounter++;
            }
         });

         receiver.DeliveryStateUpdatedHandler((delivery) =>
         {
            Assert.Fail("Should not have updated this handler.");
         });

         receiver.Open();
         receiver.AddCredit(2);

         peer.WaitForScriptToComplete();
         peer.ExpectFlow().WithLinkCredit(1).WithIncomingWindow(2).WithNextIncomingId(3);
         peer.ExpectDetach().Respond();

         Assert.AreEqual((connection.MaxFrameSize * 2) - payload.Length, session.RemainingIncomingCapacity);

         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithAborted(true)
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Now();

         Assert.AreEqual(connection.MaxFrameSize * 2, session.RemainingIncomingCapacity);

         receiver.Close();

         Assert.AreEqual(2, deliveryCounter, "Should have received two delivery read events");
         Assert.AreEqual(1, deliveryAbortedCounter, "Should only be one aborted delivery event");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestReceiveDeliveriesAndSendDispositionUponReceipt()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler(result => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         byte[] payload = new byte[] { 1 };

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond().WithNextOutgoingId(0);
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(3);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.ExpectDisposition().WithFirst(0)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.RemoteTransfer().WithDeliveryId(1)
                              .WithDeliveryTag(new byte[] { 2 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.ExpectDisposition().WithFirst(1)
                                 .WithSettled(true)
                                 .WithState().Accepted();
         peer.RemoteTransfer().WithDeliveryId(2)
                              .WithDeliveryTag(new byte[] { 3 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithPayload(payload).Queue();
         peer.ExpectDisposition().WithFirst(2)
                                 .WithSettled(true)
                                 .WithState().Accepted();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("receiver");
         receiver.DeliveryReadHandler((delivery) =>
         {
            delivery.Disposition(Accepted.Instance, true);
         });

         receiver.AddCredit(3);
         receiver.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         receiver.Close();
         session.Close();
         connection.Close();

         // Check post conditions and done.
         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }
   }
}
