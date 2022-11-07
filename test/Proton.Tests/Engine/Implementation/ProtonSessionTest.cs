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
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;
using Is = Apache.Qpid.Proton.Test.Driver.Matchers.Is;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonSessionTest : ProtonEngineTestSupport
   {
      [Test]
      public void TestSessionEmitsOpenAndCloseEvents()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool sessionLocalOpen = false;
         bool sessionLocalClose = false;
         bool sessionRemoteOpen = false;
         bool sessionRemoteClose = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();

         session.LocalOpenHandler((result) => sessionLocalOpen = true)
                .LocalCloseHandler((result) => sessionLocalClose = true)
                .OpenHandler((result) => sessionRemoteOpen = true)
                .CloseHandler((result) => sessionRemoteClose = true);

         session.Open();
         session.Close();

         Assert.IsTrue(sessionLocalOpen, "Session should have reported local open");
         Assert.IsTrue(sessionLocalClose, "Session should have reported local close");
         Assert.IsTrue(sessionRemoteOpen, "Session should have reported remote open");
         Assert.IsTrue(sessionRemoteClose, "Session should have reported remote close");

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
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool engineShutdown = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();

         IConnection connection = engine.Start();

         connection.Open();

         ISession session = connection.Session();
         session.Open();
         session.EngineShutdownHandler((result) => engineShutdown = true);

         if (locallyClosed)
         {
            if (remotelyClosed)
            {
               peer.ExpectEnd().Respond();
            }
            else
            {
               peer.ExpectEnd();
            }

            session.Close();
         }

         if (remotelyClosed && !locallyClosed)
         {
            peer.RemoteEnd().Now();
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
      public void TestSessionOpenAndCloseAreIdempotent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
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

         // Should not emit another begin frame
         session.Open();

         session.Close();

         // Should not emit another end frame
         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderCreateOnClosedSessionThrowsIOE()
      {
         TestLinkCreateOnClosedSessionThrowsISE(false);
      }

      [Test]
      public void TestReceiverCreateOnClosedSessionThrowsIOE()
      {
         TestLinkCreateOnClosedSessionThrowsISE(true);
      }

      private void TestLinkCreateOnClosedSessionThrowsISE(bool receiver)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();
         ISession session = connection.Session().Open().Close();

         if (receiver)
         {
            try
            {
               session.Receiver("test");
               Assert.Fail("Should not allow receiver create on closed session");
            }
            catch (InvalidOperationException)
            {
               // Expected
            }
         }
         else
         {
            try
            {
               session.Sender("test");
               Assert.Fail("Should not allow sender create on closed session");
            }
            catch (InvalidOperationException)
            {
               // Expected
            }
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenSessionBeforeOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // An opened session shouldn't write its begin until the parent connection
         // is opened and once it is the begin should be automatically written.
         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();
         peer.ExpectBegin();

         connection.Open();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineEmitsBeginAfterLocalSessionOpened()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();

         bool RemoteOpened = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();

         ISession session = connection.Session();
         session.OpenHandler((result) => RemoteOpened = true);
         session.Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(RemoteOpened);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionFiresOpenedEventAfterRemoteOpensLocallyOpenedSession()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();

         bool connectionRemotelyOpened = false;
         bool sessionRemotelyOpened = false;

         IConnection connection = engine.Start();

         connection.OpenHandler((result) => connectionRemotelyOpened = true);
         connection.Open();

         Assert.IsTrue(connectionRemotelyOpened, "Connection remote opened event did not fire");

         ISession session = connection.Session();
         session.OpenHandler((result) => sessionRemotelyOpened = true);
         session.Open();

         Assert.IsTrue(sessionRemotelyOpened, "Session remote opened event did not fire");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestNoSessionPerformativesEmittedIfConnectionOpenedAndClosedBeforeAnyRemoteResponses()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // An opened session shouldn't write its begin until the parent connection
         // is opened and once it is the begin should be automatically written.
         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();

         peer.ExpectAMQPHeader();

         connection.Open();

         peer.WaitForScriptToComplete();

         connection.Close();

         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();
         peer.RemoteHeader(AmqpHeader.GetAMQPHeader().ToArray()).Now();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenAndCloseSessionWithNullSetsOnSessionOptions()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().OnChannel(0).Respond();
         peer.ExpectEnd().OnChannel(0).Respond();
         peer.ExpectClose();

         IConnection connection = engine.Start();
         connection.Open();

         ISession session = connection.Session();
         session.Properties = null;
         session.OfferedCapabilities = (Symbol[])null;
         session.DesiredCapabilities = (Symbol[])null;
         session.ErrorCondition = null;
         session.Open();

         Assert.IsNotNull(session.Attachments);
         Assert.IsNull(session.Properties);
         Assert.IsNull(session.OfferedCapabilities);
         Assert.IsNull(session.DesiredCapabilities);
         Assert.IsNull(session.ErrorCondition);

         Assert.IsNull(session.RemoteProperties);
         Assert.IsNull(session.RemoteOfferedCapabilities);
         Assert.IsNull(session.RemoteDesiredCapabilities);
         Assert.IsNull(session.RemoteErrorCondition);

         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenAndCloseMultipleSessions()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().OnChannel(0).Respond();
         peer.ExpectBegin().OnChannel(1).Respond();
         peer.ExpectEnd().OnChannel(1).Respond();
         peer.ExpectEnd().OnChannel(0).Respond();
         peer.ExpectClose();

         IConnection connection = engine.Start();
         connection.Open();

         ISession session1 = connection.Session();
         session1.Open();
         ISession session2 = connection.Session();
         session2.Open();

         session2.Close();
         session1.Close();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineFireRemotelyOpenedSessionEventWhenRemoteBeginArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.RemoteBegin().Queue();

         bool connectionRemotelyOpened = false;
         bool sessionRemotelyOpened = false;

         ISession remoteSession = null;
         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.OpenHandler((result) => connectionRemotelyOpened = true);
         connection.SessionOpenedHandler((result) =>
         {
            remoteSession = result;
            sessionRemotelyOpened = true;
         });
         connection.Open();

         Assert.IsTrue(connectionRemotelyOpened, "Connection remote opened event did not fire");
         Assert.IsTrue(sessionRemotelyOpened, "Session remote opened event did not fire");
         Assert.IsNotNull(remoteSession, "Connection did not create a local session for remote open");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionPopulatesBeginUsingDefaults()
      {
         DoTestSessionOpenPopulatesBegin(false, false);
      }

      [Test]
      public void TestSessionPopulatesBeginWithConfiguredMaxFrameSizeButNoIncomingCapacity()
      {
         DoTestSessionOpenPopulatesBegin(true, false);
      }

      [Test]
      public void TestSessionPopulatesBeginWithConfiguredMaxFrameSizeAndIncomingCapacity()
      {
         DoTestSessionOpenPopulatesBegin(true, true);
      }

      private void DoTestSessionOpenPopulatesBegin(bool setMaxFrameSize, bool setIncomingCapacity)
      {
         uint MAX_FRAME_SIZE = 32767;
         uint SESSION_INCOMING_CAPACITY = int.MaxValue;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         uint expectedMaxFrameSize;

         if (setMaxFrameSize)
         {
            expectedMaxFrameSize = MAX_FRAME_SIZE;
         }
         else
         {
            expectedMaxFrameSize = ProtonConstants.DefaultMaxAmqpFrameSize;
         }

         uint expectedIncomingWindow = int.MaxValue;
         if (setIncomingCapacity)
         {
            expectedIncomingWindow = SESSION_INCOMING_CAPACITY / MAX_FRAME_SIZE;
         }

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(expectedMaxFrameSize).Respond().WithContainerId("driver");
         peer.ExpectBegin().WithHandleMax(Is.NullValue())
                           .WithNextOutgoingId(0)
                           .WithIncomingWindow(expectedIncomingWindow)
                           .WithOutgoingWindow(int.MaxValue)
                           .WithOfferedCapabilities(Is.NullValue())
                           .WithDesiredCapabilities(Is.NullValue())
                           .WithProperties(Is.NullValue())
                           .Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();
         if (setMaxFrameSize)
         {
            connection.MaxFrameSize = MAX_FRAME_SIZE;
         }
         connection.Open();

         ISession session = connection.Session();
         if (setIncomingCapacity)
         {
            session.IncomingCapacity = SESSION_INCOMING_CAPACITY;
         }
         session.Open();
         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionOpenFailsWhenConnectionClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectClose().Respond();

         bool connectionOpenedSignaled = false;
         bool connectionClosedSignaled = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.OpenHandler((result) => connectionOpenedSignaled = true);
         connection.CloseHandler((result) => connectionClosedSignaled = true);

         ISession session = connection.Session();
         connection.Open();
         connection.Close();

         Assert.IsTrue(connectionOpenedSignaled, "Connection remote opened event did not fire");
         Assert.IsTrue(connectionClosedSignaled, "Connection remote closed event did not fire");

         try
         {
            session.Open();
            Assert.Fail("Should not be able to open a session when its Connection was already closed");
         }
         catch (InvalidOperationException) { }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionOpenFailsWhenConnectionRemotelyClosed()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.RemoteClose().Queue();

         bool connectionOpenedSignaled = false;
         bool connectionClosedSignaled = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.OpenHandler((result) => connectionOpenedSignaled = true);
         connection.CloseHandler((result) => connectionClosedSignaled = true);

         ISession session = connection.Session();
         connection.Open();

         Assert.IsTrue(connectionOpenedSignaled, "Connection remote opened event did not fire");
         Assert.IsTrue(connectionClosedSignaled, "Connection remote closed event did not fire");

         try
         {
            session.Open();
            Assert.Fail("Should not be able to open a session when its Connection was already closed");
         }
         catch (InvalidOperationException) { }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionOpenFailsWhenWriteOfBeginFailsWithException()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.DropAfterLastHandler();

         IConnection connection = engine.Start().Open();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);
         Assert.IsTrue(connection.ConnectionState == ConnectionState.Active);
         Assert.IsTrue(connection.RemoteConnectionState == ConnectionState.Active);

         ISession session = connection.Session();

         try
         {
            session.Open();
            Assert.Fail("Should not be able to open a session when its Connection was already closed");
         }
         catch (EngineFailedException)
         {
         }

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
         Assert.IsTrue(engine.IsFailed);
         Assert.IsFalse(engine.IsShutdown);
         Assert.IsNotNull(engine.FailureCause);
      }

      [Test]
      public void TestSessionOpenNotSentUntilConnectionOpened()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectClose();

         connection.Open();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionCloseNotSentUntilConnectionOpened()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool sessionOpenedSignaled = false;

         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.OpenHandler((result) => sessionOpenedSignaled = true);
         session.Open();
         session.Close();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose();

         Assert.IsFalse(sessionOpenedSignaled, "Session opened handler should not have been called yet");

         connection.Open();

         // Session was already closed so no open event should fire.
         Assert.IsFalse(sessionOpenedSignaled, "Session opened handler should not have been called yet");

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionRemotelyClosedWithError()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();
         session.Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(session.IsLocallyOpen);
         Assert.IsFalse(session.IsLocallyClosed);
         Assert.IsTrue(session.IsRemotelyOpen);
         Assert.IsFalse(session.IsRemotelyClosed);

         peer.ExpectEnd();
         peer.ExpectClose();
         peer.RemoteEnd().WithErrorCondition(AmqpError.INTERNAL_ERROR.ToString(), "Error").Now();

         Assert.IsTrue(session.IsLocallyOpen);
         Assert.IsFalse(session.IsLocallyClosed);
         Assert.IsFalse(session.IsRemotelyOpen);
         Assert.IsTrue(session.IsRemotelyClosed);

         Assert.AreEqual(AmqpError.INTERNAL_ERROR, session.RemoteErrorCondition.Condition);
         Assert.AreEqual("Error", session.RemoteErrorCondition.Description);

         session.Close();

         Assert.IsFalse(session.IsLocallyOpen);
         Assert.IsTrue(session.IsLocallyClosed);
         Assert.IsFalse(session.IsRemotelyOpen);
         Assert.IsTrue(session.IsRemotelyClosed);

         Assert.AreEqual(AmqpError.INTERNAL_ERROR, session.RemoteErrorCondition.Condition);
         Assert.AreEqual("Error", session.RemoteErrorCondition.Description);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionCloseAfterConnectionRemotelyClosedWhenNoBeginResponseReceived()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         ISession session = connection.Session();
         session.Open();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin();
         peer.RemoteClose().WithErrorCondition(AmqpError.NOT_ALLOWED.ToString(), "Error").Queue();

         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectEnd();
         peer.ExpectClose();

         // Connection not locally closed so end still written.
         session.Close();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestHandleRemoteBeginWithInvalidRemoteChannelSet()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");

         bool remoteConnectionOpened = false;
         bool remoteSession = false;

         IConnection connection = engine.Start();

         // Default engine should start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.OpenHandler((result) => remoteConnectionOpened = true);
         connection.Open();

         connection.SessionOpenedHandler((session) => remoteSession = true);

         peer.WaitForScriptToComplete();

         // Simulate asynchronous arrival of data as we always operate on one thread in these tests.
         peer.ExpectClose().WithError(Is.NotNullValue());
         peer.RemoteBegin().WithRemoteChannel(3).Now();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(remoteConnectionOpened, "Remote connection should have occurred");
         Assert.IsFalse(remoteSession, "Should not have seen a remote session open.");

         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestCapabilitiesArePopulatedAndAccessible()
      {
         Symbol clientOfferedSymbol = Symbol.Lookup("clientOfferedCapability");
         Symbol clientDesiredSymbol = Symbol.Lookup("clientDesiredCapability");
         Symbol serverOfferedSymbol = Symbol.Lookup("serverOfferedCapability");
         Symbol serverDesiredSymbol = Symbol.Lookup("serverDesiredCapability");

         Symbol[] clientOfferedCapabilities = new Symbol[] { clientOfferedSymbol };
         Symbol[] clientDesiredCapabilities = new Symbol[] { clientDesiredSymbol };

         Symbol[] serverOfferedCapabilities = new Symbol[] { serverOfferedSymbol };
         Symbol[] serverDesiredCapabilities = new Symbol[] { serverDesiredSymbol };

         bool sessionRemotelyOpened = false;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().WithOfferedCapabilities(new String[] { clientOfferedSymbol.ToString() })
                           .WithDesiredCapabilities(new String[] { clientDesiredSymbol.ToString() })
                           .Respond()
                           .WithDesiredCapabilities(new String[] { serverDesiredSymbol.ToString() })
                           .WithOfferedCapabilities(new String[] { serverOfferedSymbol.ToString() });
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();
         connection.Open();

         ISession session = connection.Session();

         session.DesiredCapabilities = clientDesiredCapabilities;
         session.OfferedCapabilities = clientOfferedCapabilities;
         session.OpenHandler((result) => sessionRemotelyOpened = true);
         session.Open();

         Assert.IsTrue(sessionRemotelyOpened, "Session remote opened event did not fire");

         Assert.AreEqual(clientOfferedCapabilities, session.OfferedCapabilities);
         Assert.AreEqual(clientDesiredCapabilities, session.DesiredCapabilities);
         Assert.AreEqual(serverOfferedCapabilities, session.RemoteOfferedCapabilities);
         Assert.AreEqual(serverDesiredCapabilities, session.RemoteDesiredCapabilities);

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestPropertiesArePopulatedAndAccessible()
      {
         Symbol clientPropertyName = Symbol.Lookup("ClientPropertyName");
         int clientPropertyValue = 1234;
         Symbol serverPropertyName = Symbol.Lookup("ServerPropertyName");
         int serverPropertyValue = 5678;

         Dictionary<string, object> expectedClientProperties = new Dictionary<string, object>();
         expectedClientProperties.Add(clientPropertyName.ToString(), clientPropertyValue);
         Dictionary<Symbol, object> clientProperties = new Dictionary<Symbol, object>();
         clientProperties.Add(clientPropertyName, clientPropertyValue);

         Dictionary<string, object> expectedServerProperties = new Dictionary<string, object>();
         expectedServerProperties.Add(serverPropertyName.ToString(), serverPropertyValue);
         Dictionary<Symbol, object> serverProperties = new Dictionary<Symbol, object>();
         serverProperties.Add(serverPropertyName, serverPropertyValue);

         bool sessionRemotelyOpened = false;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().WithProperties(expectedClientProperties)
                           .Respond()
                           .WithProperties(expectedServerProperties);
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();
         connection.Open();

         ISession session = connection.Session();

         session.Properties = clientProperties;
         session.OpenHandler((result) => sessionRemotelyOpened = true);
         session.Open();

         Assert.IsTrue(sessionRemotelyOpened, "Session remote opened event did not fire");

         Assert.IsNotNull(session.Properties);
         Assert.IsNotNull(session.RemoteProperties);

         Assert.AreEqual(clientPropertyValue, session.Properties[clientPropertyName]);
         Assert.AreEqual(serverPropertyValue, session.RemoteProperties[serverPropertyName]);

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEmittedSessionIncomingWindowOnFirstFlowNoFrameSizeOrSessionCapacitySet()
      {
         DoSessionIncomingWindowTestImpl(false, false);
      }

      [Test]
      public void TestEmittedSessionIncomingWindowOnFirstFlowWithFrameSizeButNoSessionCapacitySet()
      {
         DoSessionIncomingWindowTestImpl(true, false);
      }

      [Test]
      public void TestEmittedSessionIncomingWindowOnFirstFlowWithNoFrameSizeButWithSessionCapacitySet()
      {
         DoSessionIncomingWindowTestImpl(false, true);
      }

      [Test]
      public void TestEmittedSessionIncomingWindowOnFirstFlowWithFrameSizeAndSessionCapacitySet()
      {
         DoSessionIncomingWindowTestImpl(true, true);
      }

      private void DoSessionIncomingWindowTestImpl(bool setFrameSize, bool setSessionCapacity)
      {
         uint TEST_MAX_FRAME_SIZE = 5 * 1024;
         uint TEST_SESSION_CAPACITY = 100 * 1024;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         uint expectedMaxFrameSize;
         if (setFrameSize)
         {
            expectedMaxFrameSize = TEST_MAX_FRAME_SIZE;
         }
         else
         {
            expectedMaxFrameSize = ProtonConstants.DefaultMaxAmqpFrameSize;
         }

         uint expectedWindowSize = 2147483647;
         if (setSessionCapacity && setFrameSize)
         {
            expectedWindowSize = TEST_SESSION_CAPACITY / TEST_MAX_FRAME_SIZE;
         }
         else if (setSessionCapacity)
         {
            expectedWindowSize = TEST_SESSION_CAPACITY / engine.Connection.MaxFrameSize;
         }

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(expectedMaxFrameSize).Respond();
         peer.ExpectBegin().WithIncomingWindow(expectedWindowSize).Respond();
         peer.ExpectAttach().Respond();

         IConnection connection = engine.Start();
         if (setFrameSize)
         {
            connection.MaxFrameSize = TEST_MAX_FRAME_SIZE;
         }
         connection.Open();

         ISession session = connection.Session();
         uint sessionCapacity = 0;
         if (setSessionCapacity)
         {
            sessionCapacity = TEST_SESSION_CAPACITY;
            session.IncomingCapacity = sessionCapacity;
         }

         // Open session and verify emitted incoming window
         session.Open();

         if (setSessionCapacity)
         {
            Assert.AreEqual(sessionCapacity, session.RemainingIncomingCapacity);
         }
         else
         {
            Assert.AreEqual(int.MaxValue, session.RemainingIncomingCapacity);
         }

         Assert.AreEqual(sessionCapacity, session.IncomingCapacity, "Unexpected session capacity");

         // Use a receiver to force more session window observations.
         IReceiver receiver = session.Receiver("receiver");
         receiver.Open();

         uint deliveryArrived = 0;
         IIncomingDelivery delivered = null;

         receiver.DeliveryReadHandler((delivery) =>
         {
            deliveryArrived++;
            delivered = delivery;
         });

         // Expect that a flow will be emitted and the window should match either default window
         // size or computed value if max frame size and capacity are set
         peer.ExpectFlow().WithLinkCredit(1)
                          .WithIncomingWindow(expectedWindowSize);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithMessageFormat(0)
                              .WithBody().WithString("test-message").Also().Queue();

         receiver.AddCredit(1);

         Assert.AreEqual(1, deliveryArrived, "Unexpected delivery count");
         Assert.IsNotNull(delivered);

         // Flow more credit after receiving a message but not consuming it should result in a decrease in
         // the incoming window if the capacity and max frame size are configured.
         if (setSessionCapacity && setFrameSize)
         {
            expectedWindowSize = expectedWindowSize - 1;
            Assert.IsTrue(TEST_SESSION_CAPACITY > session.RemainingIncomingCapacity);
         }

         peer.ExpectFlow().WithLinkCredit(1)
                          .WithIncomingWindow(expectedWindowSize);

         receiver.AddCredit(1);

         // Settle the transfer then flow more credit, verify the emitted incoming window
         // (it should increase 1 if capacity and frame size set) otherwise remains unchanged.
         if (setSessionCapacity && setFrameSize)
         {
            expectedWindowSize = expectedWindowSize + 1;
         }
         peer.ExpectFlow().WithLinkCredit(2).WithIncomingWindow(expectedWindowSize);

         // This will consume the bytes and free them from the session window tracking.
         Assert.IsNotNull(delivered.ReadAll());

         receiver.AddCredit(1);

         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();

         receiver.Close();
         session.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionHandlesDeferredOpenAndBeginResponses()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         uint sessionOpened = 0;
         uint sessionClosed = 0;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();
         peer.ExpectBegin();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();
         session.OpenHandler((result) => sessionOpened++);
         session.CloseHandler((result) => sessionClosed++);
         session.Open();

         peer.WaitForScriptToComplete();

         // This should happen after we inject the held open and attach
         peer.ExpectAttach().OfSender().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         // Inject held responses to get the ball rolling again
         peer.RemoteOpen().WithOfferedCapabilities("ANONYMOUS_RELAY").Now();
         peer.RespondToLastBegin().Now();

         ISender sender = session.Sender("sender-1");

         sender.Open();

         session.Close();

         Assert.AreEqual(1, sessionOpened, "Should get one opened event");
         Assert.AreEqual(1, sessionClosed, "Should get one closed event");

         connection.Close();

         peer.WaitForScriptToComplete();
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenWrittenAndResponseBeginWrittenAndResponse()
      {
         TestCloseAfterShutdownNoOutputAndNoException(true, true, true);
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenWrittenAndResponseBeginWrittenAndNoResponse()
      {
         TestCloseAfterShutdownNoOutputAndNoException(true, true, false);
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenWrittenButNoResponse()
      {
         TestCloseAfterShutdownNoOutputAndNoException(true, false, false);
      }

      [Test]
      public void TestCloseAfterShutdownDoesNotThrowExceptionOpenNotWritten()
      {
         TestCloseAfterShutdownNoOutputAndNoException(false, false, false);
      }

      private void TestCloseAfterShutdownNoOutputAndNoException(bool respondToHeader, bool respondToOpen, bool respondToBegin)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
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
               }
               else
               {
                  peer.ExpectBegin();
               }
            }
            else
            {
               peer.ExpectOpen();
               peer.ExpectBegin();
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

         engine.Shutdown();

         // Should clean up and not throw as we knowingly shutdown engine operations.
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenWrittenAndResponseBeginWrittenAndResponse()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(true, true, true);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenWrittenAndResponseBeginWrittenAndNoResponse()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(true, true, false);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenWrittenButNoResponse()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(true, false, false);
      }

      [Test]
      public void TestCloseAfterFailureThrowsEngineStateExceptionOpenNotWritten()
      {
         TestCloseAfterEngineFailedThrowsAndNoOutputWritten(false, false, false);
      }

      private void TestCloseAfterEngineFailedThrowsAndNoOutputWritten(bool respondToHeader, bool respondToOpen, bool respondToBegin)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool engineFailedEvent = false;

         if (respondToHeader)
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            if (respondToOpen)
            {
               peer.ExpectOpen().Respond();
               if (respondToBegin)
               {
                  peer.ExpectBegin().Respond();
               }
               else
               {
                  peer.ExpectBegin();
               }
               peer.ExpectClose();
            }
            else
            {
               peer.ExpectOpen();
               peer.ExpectBegin();
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
         session.EngineShutdownHandler((theEvent) => engineFailedEvent = true);
         session.Open();

         engine.EngineFailed(new IOException());

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

         Assert.IsFalse(engineFailedEvent, "Session should not have signalled engine failure");

         engine.Shutdown();  // Explicit shutdown now allows local close to complete

         Assert.IsTrue(engineFailedEvent, "Session should have signalled engine failure");

         // Should clean up and not throw as we knowingly shutdown engine operations.
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestSessionStopTrackingClosedSenders()
      {
         DoTestSessionTrackingOfSenders(true, true, false, true);
      }

      [Test]
      public void TestSessionStopTrackingDetachedSenders()
      {
         DoTestSessionTrackingOfSenders(true, true, false, false);
      }

      [Test]
      public void TestSessionStopTrackingClosedSendersRemoteGoesFirst()
      {
         DoTestSessionTrackingOfSenders(true, true, true, true);
      }

      [Test]
      public void TestSessionStopTrackingDetachedSendersRemoteGoesFirst()
      {
         DoTestSessionTrackingOfSenders(true, true, true, false);
      }

      [Test]
      public void TestSessionTracksRemotelyOpenSenders()
      {
         DoTestSessionTrackingOfSenders(true, false, false, false);
      }

      [Test]
      public void TestSessionTracksLocallyOpenSenders()
      {
         DoTestSessionTrackingOfSenders(false, true, false, false);
      }

      private void DoTestSessionTrackingOfSenders(bool localDetach, bool RemoteDetach, bool remoteGoesFirst, bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();

         session.Open();

         Assert.AreEqual(0, CountElements(session.Senders));

         peer.ExpectAttach().OfSender().Respond();

         ISender sender = session.Sender("test").Open();

         Assert.AreEqual(1, CountElements(session.Senders));

         if (RemoteDetach && remoteGoesFirst)
         {
            peer.RemoteDetach().WithClosed(close).Now();
         }

         if (localDetach)
         {
            peer.ExpectDetach().WithClosed(close);
            if (close)
            {
               sender.Close();
            }
            else
            {
               sender.Detach();
            }
         }

         if (RemoteDetach && !remoteGoesFirst)
         {
            peer.RemoteDetach().WithClosed(close).Now();
         }

         if (RemoteDetach && localDetach)
         {
            Assert.AreEqual(0, CountElements(session.Senders));
         }
         else
         {
            Assert.AreEqual(1, CountElements(session.Senders));
         }

         peer.ExpectEnd().Respond();
         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionStopTrackingClosedReceivers()
      {
         DoTestSessionTrackingOfReceivers(true, true, false, true);
      }

      [Test]
      public void TestSessionStopTrackingDetachedReceivers()
      {
         DoTestSessionTrackingOfReceivers(true, true, false, false);
      }

      [Test]
      public void TestSessionStopTrackingClosedReceiversRemoteGoesFirst()
      {
         DoTestSessionTrackingOfReceivers(true, true, true, true);
      }

      [Test]
      public void TestSessionStopTrackingDetachedReceiversRemoteGoesFirst()
      {
         DoTestSessionTrackingOfReceivers(true, true, true, false);
      }

      [Test]
      public void TestSessionTracksRemotelyOpenReceivers()
      {
         DoTestSessionTrackingOfReceivers(true, false, false, false);
      }

      [Test]
      public void TestSessionTracksLocallyOpenReceivers()
      {
         DoTestSessionTrackingOfReceivers(false, true, false, false);
      }

      private void DoTestSessionTrackingOfReceivers(bool localDetach, bool RemoteDetach, bool remoteGoesFirst, bool close)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();

         session.Open();

         Assert.AreEqual(0, CountElements(session.Receivers));

         peer.ExpectAttach().OfReceiver().Respond();

         IReceiver receiver = session.Receiver("test").Open();

         Assert.AreEqual(1, CountElements(session.Receivers));

         if (RemoteDetach && remoteGoesFirst)
         {
            peer.RemoteDetach().WithClosed(close).Now();
         }

         if (localDetach)
         {
            peer.ExpectDetach().WithClosed(close);
            if (close)
            {
               receiver.Close();
            }
            else
            {
               receiver.Detach();
            }
         }

         if (RemoteDetach && !remoteGoesFirst)
         {
            peer.RemoteDetach().WithClosed(close).Now();
         }

         if (RemoteDetach && localDetach)
         {
            Assert.AreEqual(0, CountElements(session.Receivers));
         }
         else
         {
            Assert.AreEqual(1, CountElements(session.Receivers));
         }

         peer.ExpectEnd().Respond();
         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestGetSenderFromSessionByName()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();

         session.Open();

         Assert.AreEqual(0, CountElements(session.Receivers));

         ISender link = session.Sender("test").Open();
         ISender lookup = session.Sender("test");

         Assert.AreSame(link, lookup);

         link.Close();

         ISender newLink = session.Sender("test");

         Assert.AreNotSame(link, newLink);

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestGetReceiverFromSessionByName()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();

         session.Open();

         Assert.AreEqual(0, CountElements(session.Receivers));

         IReceiver link = session.Receiver("test").Open();
         IReceiver lookup = session.Receiver("test");

         Assert.AreSame(link, lookup);

         link.Close();

         IReceiver newLink = session.Receiver("test");

         Assert.AreNotSame(link, newLink);

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCloseOrDetachWithErrorCondition()
      {
         string condition = "amqp:session:window-violation";
         string description = "something bad happened.";

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectEnd().WithError(condition, description).Respond();
         peer.ExpectClose();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session().Open();

         session.ErrorCondition = new ErrorCondition(Symbol.Lookup(condition), description);
         session.Close();

         connection.Close();

         peer.WaitForScriptToComplete();
      }

      [Test]
      public void TestSessionNotifiedOfRemoteSenderOpened()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool senderRemotelyOpened = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();

         session.SenderOpenHandler((result) => senderRemotelyOpened = true);
         session.Open();

         peer.RemoteAttach().OfReceiver().WithHandle(1)
                                         .WithInitialDeliveryCount(1)
                                         .WithName("remote-sender").Now();

         session.Close();

         Assert.IsTrue(senderRemotelyOpened, "Session should have reported remote sender open");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenSenderAndReceiverWithSameLinkNames()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool senderRemotelyOpened = false;
         bool receiverRemotelyOpened = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().WithHandle(0).WithName("link-name");
         peer.ExpectAttach().OfReceiver().WithHandle(1).WithName("link-name");
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("link-name").Open();
         IReceiver receiver = session.Receiver("link-name").Open();

         sender.OpenHandler((link) => senderRemotelyOpened = true);
         receiver.OpenHandler((link) => receiverRemotelyOpened = true);

         peer.RemoteAttach().OfSender().WithHandle(1)
                                       .WithInitialDeliveryCount(1)
                                       .WithName("link-name").Now();
         peer.RemoteAttach().OfReceiver().WithHandle(0)
                                         .WithInitialDeliveryCount(1)
                                         .WithName("link-name").Now();

         Assert.IsTrue(sender.IsLocallyOpen);
         Assert.IsTrue(sender.IsRemotelyOpen);
         Assert.IsTrue(receiver.IsLocallyOpen);
         Assert.IsTrue(receiver.IsRemotelyOpen);

         session.Close();

         Assert.IsTrue(senderRemotelyOpened, "Sender should have reported remote sender open");
         Assert.IsTrue(receiverRemotelyOpened, "Receiver should have reported remote sender open");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestBeginAndEndSessionBeforeRemoteBeginArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin();
         peer.ExpectEnd();

         IConnection connection = engine.Start();

         connection.Open();
         ISession session = connection.Session();

         session.Open();
         session.Close();

         peer.WaitForScriptToComplete();
         peer.RemoteBegin().WithRemoteChannel(0).WithNextOutgoingId(1).Now();
         peer.RemoteEnd().Now();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestHalfClosedSessiOnChannelNotImmediatelyRecycled()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().OnChannel(0);
         peer.ExpectEnd();

         IConnection connection = engine.Start();

         connection.Open();
         connection.Session().Open().Close();

         // Channel 0 should be skipped since we are still waiting for the being / end and
         // we have a free slot that can be used instead.
         peer.WaitForScriptToComplete();
         peer.ExpectBegin().OnChannel(1).Respond();
         peer.ExpectEnd().OnChannel(1).Respond();

         connection.Session().Open().Close();

         // Now channel 1 should reused since it was opened and closed properly
         peer.WaitForScriptToComplete();
         peer.ExpectBegin().OnChannel(1).Respond();
         peer.ExpectBegin().OnChannel(0).Respond();
         peer.ExpectEnd().OnChannel(0).Respond();

         connection.Session().Open();

         // Close the original session now and its slot should be free to be reused.
         peer.RemoteBegin().WithRemoteChannel(0).WithNextOutgoingId(1).Now();
         peer.RemoteEnd().Now();

         connection.Session().Open().Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestHalfClosedSessiOnChannelRecycledIfNoOtherAvailableChannels()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithChannelMax(1).Respond().WithContainerId("driver");
         peer.ExpectBegin().OnChannel(0);
         peer.ExpectEnd().OnChannel(0);
         peer.ExpectBegin().OnChannel(1);
         peer.ExpectBegin().OnChannel(0);

         IConnection connection = engine.Start();

         connection.ChannelMax = 1; // at most two channels
         connection.Open();
         connection.Session().Open().Close(); // Ch: 0
         connection.Session().Open(); // Ch: 1
         connection.Session().Open(); // Ch: 0 (recycled)

         peer.WaitForScriptToComplete();
         // Answer to initial Begin / End of session on Ch: 0
         peer.RemoteBegin().WithRemoteChannel(0).OnChannel(1).Now();
         peer.RemoteEnd().OnChannel(1).Now();
         // Answer to second session which should have begun on Ch: 1
         peer.RemoteBegin().WithRemoteChannel(1).OnChannel(0).Now();
         // Answer to third session which should have begun on Ch: 0 recycled
         peer.RemoteBegin().WithRemoteChannel(0).OnChannel(1).Now();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionEnforcesHandleMaxForLocalSenders()
      {
         DoTestSessionEnforcesHandleMaxForLocalEndpoints(false);
      }

      [Test]
      public void TestSessionEnforcesHandleMaxForLocalReceivers()
      {
         DoTestSessionEnforcesHandleMaxForLocalEndpoints(true);
      }

      private void DoTestSessionEnforcesHandleMaxForLocalEndpoints(bool receiver)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().WithHandleMax(0).Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectEnd().Respond();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session();
         session.HandleMax = 0;
         session.Open();

         Assert.AreEqual(0, session.HandleMax);

         if (receiver)
         {
            session.Receiver("receiver1").Open();
            try
            {
               session.Receiver("receiver2").Open();
               Assert.Fail("Should not allow receiver create on session with one handle maximum");
            }
            catch (InvalidOperationException)
            {
               // Expected
            }
            try
            {
               session.Sender("sender1").Open();
               Assert.Fail("Should not allow additional sender create on session with one handle maximum");
            }
            catch (InvalidOperationException)
            {
               // Expected
            }
         }
         else
         {
            session.Sender("sender1").Open();
            try
            {
               session.Sender("sender2").Open();
               Assert.Fail("Should not allow second sender create on session with one handle maximum");
            }
            catch (InvalidOperationException)
            {
               // Expected
            }
            try
            {
               session.Receiver("receiver1").Open();
               Assert.Fail("Should not allow additional receiver create on session with one handle maximum");
            }
            catch (InvalidOperationException)
            {
               // Expected
            }
         }

         session.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionEnforcesHandleMaxFromRemoteAttachOfSender()
      {
         DoTestSessionEnforcesHandleMaxFromRemoteAttach(true);
      }

      [Test]
      public void TestSessionEnforcesHandleMaxFromRemoteAttachOfReceiver()
      {
         DoTestSessionEnforcesHandleMaxFromRemoteAttach(false);
      }

      public void DoTestSessionEnforcesHandleMaxFromRemoteAttach(bool sender)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().WithHandleMax(0).Respond().WithHandleMax(42);

         if (sender)
         {
            peer.RemoteAttach().OfSender().WithHandle(1).WithName("link-name").Queue();
         }
         else
         {
            peer.RemoteAttach().OfReceiver().WithHandle(1).WithName("link-name").Queue();
         }

         peer.ExpectClose().WithError(ConnectionError.FRAMING_ERROR.ToString(), "Session handle-max exceeded").Respond();

         IConnection connection = engine.Start().Open();

         // Remote should attempt to attach a link and violate local handle max restrictions
         ISession session = connection.Session();
         session.HandleMax = 0;
         session.Open();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionOutgoingSetEqualToMaxFrameSize()
      {
         TestSessionConfigureOutgoingCapacity(1024, 1024, 1024);
      }

      [Test]
      public void TestSessionOutgoingSetToTwiceMaxFrameSize()
      {
         TestSessionConfigureOutgoingCapacity(1024, 2048, 2048);
      }

      [Test]
      public void TestSessionOutgoingSetToSmallerThanMaxFrameSize()
      {
         TestSessionConfigureOutgoingCapacity(1024, 512, 1024);
      }

      [Test]
      public void TestSessionOutgoingSetToLargerThanMaxFrameSizeAndNotEven()
      {
         TestSessionConfigureOutgoingCapacity(1024, 8199, 8192);
      }

      [Test]
      public void TestSessionOutgoingSetToZeroToDisableOutAdd()
      {
         TestSessionConfigureOutgoingCapacity(1024, 0, 0);
      }

      private void TestSessionConfigureOutgoingCapacity(uint frameSize, uint sessionCapacity, uint remainingCapacity)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(frameSize).Respond();
         peer.ExpectBegin().Respond();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = frameSize;
         connection.Open();
         ISession session = connection.Session().Open();

         peer.WaitForScriptToComplete();

         Assert.AreEqual(int.MaxValue, session.RemainingOutgoingCapacity);

         session.OutgoingCapacity = sessionCapacity;

         Assert.AreEqual(sessionCapacity, session.OutgoingCapacity);
         Assert.AreEqual(remainingCapacity, session.RemainingOutgoingCapacity);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSessionNotWritableWhenOutgoingCapacitySetToZeroAlsoReflectsInSenders()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(2).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test").Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(int.MaxValue, session.RemainingOutgoingCapacity);

         session.OutgoingCapacity = 0;

         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderCannotSendAfterUsingUpOutgoingCapacityLimit()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();

         ISession session = connection.Session();
         session.OutgoingCapacity = 2048;
         session.Open();

         ISender sender = session.Sender("test");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);
         peer.ExpectTransfer().WithPayload(payload);

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(2048, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         IOutgoingDelivery delivery2 = sender.Next();
         delivery2.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(2, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderGetsUpdatedOnceSessionOutgoingWindowIsExpandedByWriteCallbacks()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 3072;
         session.Open();
         ISender sender = session.Sender("test");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);
         peer.ExpectTransfer().WithPayload(payload);
         peer.ExpectTransfer().WithPayload(payload);

         int creditStateUpdated = 0;
         sender.CreditStateUpdateHandler((self) => creditStateUpdated++);

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(3072, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         IOutgoingDelivery delivery2 = sender.Next();
         delivery2.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         IOutgoingDelivery delivery3 = sender.Next();
         delivery3.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(3, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         Assert.AreEqual(1, creditStateUpdated);
         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(3072, session.RemainingOutgoingCapacity);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSetSameOutgoingWindowAfterBecomingNotWritableDoesNotTriggerWritable()
      {
         // Should not become writable because two outstanding writes but low water mark remains one frame pending.
         TestSessionOutgoingWindowExpandedAfterItBecomeNotWritable(2048, false);
      }

      [Test]
      public void TestExpandingOutgoingWindowAfterBecomingNotWritableUpdateSenderAsWritableOneFrameBigger()
      {
         // Should not become writable because two outstanding writes but low water mark remains one frame pending.
         TestSessionOutgoingWindowExpandedAfterItBecomeNotWritable(3072, false);
      }

      [Test]
      public void TestExpandingOutgoingWindowAfterBecomingNotWritableUpdateSenderAsWritableTwoFramesBuffer()
      {
         // Should become writable since low water mark was one but becomes two and we have only two pending.
         TestSessionOutgoingWindowExpandedAfterItBecomeNotWritable(4096, true);
      }

      [Test]
      public void TestDisableOutgoingWindowingAfterBecomingNotWritableUpdateSenderAsWritable()
      {
         // Should become pending since we are lifting restrictions
         TestSessionOutgoingWindowExpandedAfterItBecomeNotWritable(null, true);
      }

      private void TestSessionOutgoingWindowExpandedAfterItBecomeNotWritable(uint? updatedWindow, bool becomesWritable)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         uint maxFrameSize = 1024;
         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(maxFrameSize).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = maxFrameSize;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 2048;
         session.Open();
         ISender sender = session.Sender("test");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);
         peer.ExpectTransfer().WithPayload(payload);

         int creditStateUpdated = 0;
         sender.CreditStateUpdateHandler((self) => creditStateUpdated++);

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(2048, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         IOutgoingDelivery delivery2 = sender.Next();
         delivery2.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(2, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         session.OutgoingCapacity = (uint)(updatedWindow.HasValue ? updatedWindow : int.MaxValue);

         if (becomesWritable)
         {
            Assert.AreEqual(1, creditStateUpdated);
            Assert.IsTrue(sender.IsSendable);
         }
         else
         {
            Assert.AreEqual(0, creditStateUpdated);
            Assert.IsFalse(sender.IsSendable);
         }

         if (updatedWindow.HasValue)
         {
            Assert.AreEqual(updatedWindow - (asyncIOCallbacks.Count * maxFrameSize), session.RemainingOutgoingCapacity);
         }
         else
         {
            Assert.AreEqual(int.MaxValue, session.RemainingOutgoingCapacity);
         }

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestMultiplySendersCannotSendAfterUsingUpOutgoingCapacityLimit()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 2048;
         session.Open();
         ISender sender1 = session.Sender("test1");
         sender1.DeliveryTagGenerator = generator;
         sender1.Open();
         ISender sender2 = session.Sender("test2");
         sender2.DeliveryTagGenerator = generator;
         sender2.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);
         peer.ExpectTransfer().WithPayload(payload);

         Assert.IsTrue(sender1.IsSendable);
         Assert.IsTrue(sender2.IsSendable);
         Assert.AreEqual(2048, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach, Attach
         Assert.AreEqual(4, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender1.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         IOutgoingDelivery delivery2 = sender2.Next();
         delivery2.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(2, asyncIOCallbacks.Count);
         Assert.IsFalse(sender1.IsSendable);
         Assert.IsFalse(sender2.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestOnlyOneSenderNotifiedOfNewCapacityIfFirstOneUsesItUp()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 2048;
         session.Open();
         ISender sender1 = session.Sender("test1");
         sender1.DeliveryTagGenerator = generator;
         sender1.Open();
         ISender sender2 = session.Sender("test2");
         sender2.DeliveryTagGenerator = generator;
         sender2.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);
         peer.ExpectTransfer().WithPayload(payload);

         // One of them should write to the high water mark again and stop the other getting called.
         int creditStateUpdated = 0;
         sender1.CreditStateUpdateHandler((self) =>
         {
            creditStateUpdated++;
            IOutgoingDelivery delivery = self.Next();
            delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         });
         sender2.CreditStateUpdateHandler((self) =>
         {
            creditStateUpdated++;
            IOutgoingDelivery delivery = self.Next();
            delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         });

         Assert.IsTrue(sender1.IsSendable);
         Assert.IsTrue(sender2.IsSendable);
         Assert.AreEqual(2048, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach, Attach
         Assert.AreEqual(4, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender1.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         IOutgoingDelivery delivery2 = sender2.Next();
         delivery2.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         Assert.AreEqual(2, asyncIOCallbacks.Count);
         Assert.IsFalse(sender1.IsSendable);
         Assert.IsFalse(sender2.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         // Free a frame's worth of window which should allow a new write from one sender.
         asyncIOCallbacks.Dequeue().Invoke();

         Assert.AreEqual(2, asyncIOCallbacks.Count);
         Assert.IsFalse(sender1.IsSendable);
         Assert.IsFalse(sender2.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         Assert.AreEqual(1, creditStateUpdated);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReduceOutgoingWindowDoesNotStopSenderIfSomeWindowRemaining()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 4096;
         session.Open();
         ISender sender = session.Sender("test1");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(4096, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(3072, session.RemainingOutgoingCapacity);

         session.OutgoingCapacity = 2048;
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);
         Assert.IsTrue(sender.IsSendable);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestDisableOutgoingWindowMarksSenderAsNotSendableWhenWriteStillPending()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 4096;
         session.Open();
         ISender sender = session.Sender("test1");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(4096, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(3072, session.RemainingOutgoingCapacity);

         session.OutgoingCapacity = 0;
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         Assert.IsFalse(sender.IsSendable);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestReduceAndThenIncreaseOutgoingWindowRemembersPreviouslyPendingWrites()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 4096;
         session.Open();
         ISender sender = session.Sender("test1");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(4096, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery1 = sender.Next();
         delivery1.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(3072, session.RemainingOutgoingCapacity);

         session.OutgoingCapacity = 1024;
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         Assert.IsFalse(sender.IsSendable);
         session.OutgoingCapacity = 4096;
         Assert.AreEqual(3072, session.RemainingOutgoingCapacity);
         Assert.IsTrue(sender.IsSendable);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderNotifiedAfterSessionRemoteWindowOpenedAfterLocalCapacityRestored()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().WithNextOutgoingId(0).Respond().WithNextOutgoingId(0);
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).WithNextIncomingId(0).WithIncomingWindow(1).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 1024;
         session.Open();
         ISender sender = session.Sender("test1");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         // One of them should write to the high water mark again and stop the other getting called.
         int creditStateUpdated = 0;
         sender.CreditStateUpdateHandler((self) =>
         {
            creditStateUpdated++;
            IOutgoingDelivery delivery = self.Next();
            delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
         });

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery = sender.Next();
         delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         // Free a frame's worth of window which shouldn't signal writable as still no remote capacity.
         asyncIOCallbacks.Dequeue().Invoke();

         Assert.AreEqual(0, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);
         Assert.AreEqual(0, creditStateUpdated);

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);
         peer.RemoteFlow().WithLinkCredit(19).WithNextIncomingId(1).WithIncomingWindow(1).Now();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         Assert.AreEqual(1, creditStateUpdated);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSenderNotifiedAfterSessionRemoteWindowOpenedBeforeLocalCapacityRestored()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().WithNextOutgoingId(0).Respond().WithNextOutgoingId(0);
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).WithNextIncomingId(0).WithIncomingWindow(1).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 1024;
         session.Open();
         ISender sender = session.Sender("test1");
         sender.DeliveryTagGenerator = generator;
         sender.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         // One of them should write to the high water mark again and stop the other getting called.
         int creditStateUpdated = 0;
         sender.CreditStateUpdateHandler((self) =>
         {
            creditStateUpdated++;
            if (sender.IsSendable)
            {
               IOutgoingDelivery delivery = self.Next();
               delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
            }
         });

         Assert.IsTrue(sender.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach
         Assert.AreEqual(3, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery = sender.Next();
         delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         // Restore session remote incoming capacity but the sender should not send since
         // there should still be pending I/O work to be signaled.
         peer.RemoteFlow().WithLinkCredit(19).WithNextIncomingId(1).WithIncomingWindow(1).Now();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         Assert.AreEqual(1, creditStateUpdated);  // For now all flow events create a signal.

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         // Now local outgoing capacity should be opened up.
         asyncIOCallbacks.Dequeue().Invoke();

         Assert.AreEqual(1, asyncIOCallbacks.Count);
         Assert.IsFalse(sender.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         Assert.AreEqual(2, creditStateUpdated);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestBothSendersNotifiedAfterSessionOutgoingWindowOpenedWhenBothRequestedSendableState()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().WithNextOutgoingId(0).Respond().WithNextOutgoingId(0);
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).WithNextIncomingId(0).WithIncomingWindow(8192).Queue();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).WithNextIncomingId(0).WithIncomingWindow(8192).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 1024;
         session.Open();
         ISender sender1 = session.Sender("test1");
         sender1.DeliveryTagGenerator = generator;
         sender1.Open();
         ISender sender2 = session.Sender("test2");
         sender2.DeliveryTagGenerator = generator;
         sender2.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         int sender1CreditStateUpdated = 0;
         sender1.CreditStateUpdateHandler((self) =>
         {
            sender1CreditStateUpdated++;
         });

         int sender2CreditStateUpdated = 0;
         sender2.CreditStateUpdateHandler((self) =>
         {
            sender2CreditStateUpdated++;
         });

         Assert.IsTrue(sender1.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);
         Assert.IsTrue(sender2.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach, Attach
         Assert.AreEqual(4, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery = sender1.Next();
         delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();

         Assert.AreEqual(1, asyncIOCallbacks.Count);

         Assert.IsFalse(sender1.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         // Sender 2 shouldn't be able to send since sender 1 consumed the outgoing window
         Assert.IsFalse(sender2.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         // Free a frame's worth of window which should trigger both senders sendable update event
         asyncIOCallbacks.Dequeue().Invoke();
         Assert.AreEqual(0, asyncIOCallbacks.Count);

         Assert.IsTrue(sender1.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);
         Assert.AreEqual(1, sender1CreditStateUpdated);
         Assert.IsTrue(sender2.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);
         Assert.AreEqual(1, sender2CreditStateUpdated);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSingleSenderUpdatedWhenOutgoingWindowOpenedForTwoIfFirstConsumesSessionOutgoingWindow()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         Queue<Action> asyncIOCallbacks = new Queue<Action>();
         ProtonTestConnector peer = CreateTestPeer(engine, asyncIOCallbacks);

         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(1024).Respond();
         peer.ExpectBegin().WithNextOutgoingId(0).Respond().WithNextOutgoingId(0);
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).WithNextIncomingId(0).WithIncomingWindow(8192).Queue();
         peer.ExpectAttach().Respond();
         peer.RemoteFlow().WithLinkCredit(20).WithNextIncomingId(0).WithIncomingWindow(8192).Queue();

         IConnection connection = engine.Start();
         connection.MaxFrameSize = 1024;
         connection.Open();
         ISession session = connection.Session();
         session.OutgoingCapacity = 1024;
         session.Open();
         ISender sender1 = session.Sender("test1");
         sender1.DeliveryTagGenerator = generator;
         sender1.Open();
         ISender sender2 = session.Sender("test2");
         sender2.DeliveryTagGenerator = generator;
         sender2.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         int sender1CreditStateUpdated = 0;
         sender1.CreditStateUpdateHandler((self) =>
         {
            sender1CreditStateUpdated++;
            if (self.IsSendable)
            {
               IOutgoingDelivery delivery = self.Next();
               delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));
            }
         });

         int sender2CreditStateUpdated = 0;
         sender2.CreditStateUpdateHandler((self) =>
         {
            sender2CreditStateUpdated++;
         });

         Assert.IsTrue(sender1.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);
         Assert.IsTrue(sender2.IsSendable);
         Assert.AreEqual(1024, session.RemainingOutgoingCapacity);

         // Open, Begin, Attach, Attach
         Assert.AreEqual(4, asyncIOCallbacks.Count);
         foreach (Action action in asyncIOCallbacks)
         {
            action.Invoke();
         }
         asyncIOCallbacks.Clear();

         IOutgoingDelivery delivery = sender1.Next();
         delivery.WriteBytes(ProtonByteBufferAllocator.Instance.Wrap(payload));

         peer.WaitForScriptToComplete();
         peer.ExpectTransfer().WithPayload(payload);

         Assert.AreEqual(1, asyncIOCallbacks.Count);

         Assert.IsFalse(sender1.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         // Sender 2 shouldn't be able to send since sender 1 consumed the outgoing window
         Assert.IsFalse(sender2.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);

         // Should trigger sender 1 to send which should exhaust the outgoing credit
         asyncIOCallbacks.Dequeue().Invoke();
         Assert.AreEqual(1, asyncIOCallbacks.Count);

         Assert.IsFalse(sender1.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         Assert.AreEqual(1, sender1CreditStateUpdated);
         Assert.IsFalse(sender2.IsSendable);
         Assert.AreEqual(0, session.RemainingOutgoingCapacity);
         // Should not have triggered an event for sender 2 being able to send since
         // sender one consumed the outgoing window already.
         Assert.AreEqual(0, sender2CreditStateUpdated);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestHandleInUseErrorReturnedIfAttachWithAlreadyBoundHandleArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().WithHandle(0).Respond().WithHandle(0);
         peer.ExpectAttach().WithHandle(1).Respond().WithHandle(0);
         peer.ExpectEnd().WithError(SessionError.HANDLE_IN_USE.ToString(), "Attach received with handle that is already in use");

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         session.Sender("test1").Open();
         session.Sender("test2").Open();

         peer.WaitForScriptToComplete();
         peer.ExpectClose().Respond();

         connection.Close();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineFailedWhenSessionReceivesDetachForUnknownLink()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.RemoteDetach().WithHandle(2).OnChannel(0).Queue();
         peer.ExpectClose().WithError(Is.NotNullValue());

         IConnection connection = engine.Start().Open();
         connection.Session().Open();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is ProtocolViolationException);
      }

      [Test]
      public void TestEngineFailedWhenSessionReceivesTransferForUnknownLink()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.RemoteDetach().Queue();
         peer.RemoteTransfer().WithHandle(0)
                              .WithDeliveryId(1)
                              .WithDeliveryTag(new byte[] { 1 })
                              .OnChannel(0)
                              .Queue();
         peer.ExpectClose().WithError(Is.NotNullValue());

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         session.Receiver("test").Open();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is ProtocolViolationException);
      }

      [Test]
      public void TestSessionWideDeliveryMonitoringHandler()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool deliveryReadByReceiver = false;
         bool deliveryReadBySession = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfReceiver().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithHandle(0)
                              .WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 1 })
                              .OnChannel(0)
                              .Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();

         session.DeliveryReadHandler((delivery) => deliveryReadBySession = true);

         IReceiver receiver = session.Receiver("test");
         receiver.DeliveryReadHandler((delivery) => deliveryReadByReceiver = true);
         receiver.Open().AddCredit(1);

         peer.WaitForScriptToComplete();

         Assert.IsTrue(deliveryReadByReceiver);
         Assert.IsTrue(deliveryReadBySession);

         Assert.IsNull(failure);
      }
   }
}