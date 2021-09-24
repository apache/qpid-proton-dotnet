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
      [Ignore("Test driver issue with null sender settle mode values")]
      public void TestSenderOpenWithSettledAndFirst()
      {
         DoTestOpenSenderWithConfiguredSenderAndReceiverSettlementModes(SenderSettleMode.Settled, ReceiverSettleMode.First);
      }

      [Test]
      [Ignore("Test driver issue with null sender settle mode values")]
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
         peer.ExpectAttach().WithSndSettleMode(((byte?)senderMode))
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

   }
}
