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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Security;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;
using Is = Apache.Qpid.Proton.Test.Driver.Matchers.Is;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonEngineTest : ProtonEngineTestSupport
   {
      [Test]
      public void TestEnginePipelineWriteFailsBeforeStart()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);

         // Engine cannot accept input bytes until started.
         Assert.IsFalse(engine.IsWritable);

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireWrite(new ProtonByteBuffer(0), null));

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireWrite(HeaderEnvelope.AMQP_HEADER_ENVELOPE));

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireWrite(new SaslEnvelope(new SaslInit())));

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireWrite(
               AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool().Take(new Open(), 0, null)));

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEnginePipelineReadFailsBeforeStart()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);

         // Engine cannot accept input bytes until started.
         Assert.IsFalse(engine.IsWritable);

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireRead(new ProtonByteBuffer(0)));

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireRead(HeaderEnvelope.AMQP_HEADER_ENVELOPE));

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireRead(new SaslEnvelope(new SaslInit())));

         Assert.Throws<EngineNotStartedException>(
            () => engine.Pipeline.FireRead(
               AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>.IncomingEnvelopePool().Take(new Open(), 0, null)));

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineStart()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);

         // Engine cannot accept input bytes until started.
         Assert.IsFalse(engine.IsWritable);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         Assert.IsFalse(engine.IsShutdown);
         Assert.IsFalse(engine.IsFailed);
         Assert.IsNull(engine.FailureCause);

         // Should be idempotent and return same Connection
         IConnection another = engine.Start();
         Assert.AreSame(connection, another);

         // Default engine should start and return a connection immediately
         Assert.IsTrue(engine.IsWritable);
         Assert.IsNotNull(connection);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineShutdown()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);

         // Engine cannot accept input bytes until started.
         Assert.IsFalse(engine.IsWritable);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         Assert.IsTrue(engine.IsWritable);
         Assert.IsFalse(engine.IsShutdown);
         Assert.IsFalse(engine.IsFailed);
         Assert.IsNull(engine.FailureCause);
         Assert.AreEqual(EngineState.Started, engine.EngineState);

         bool engineShutdownEventFired = false;

         engine.ShutdownHandler((theEngine) => engineShutdownEventFired = true);
         engine.Shutdown();

         Assert.IsFalse(engine.IsWritable);
         Assert.IsTrue(engine.IsShutdown);
         Assert.IsFalse(engine.IsFailed);
         Assert.IsNull(engine.FailureCause);
         Assert.AreEqual(EngineState.Shutdown, engine.EngineState);
         Assert.IsTrue(engineShutdownEventFired);

         Assert.IsNotNull(connection);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineFailure()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);

         // Engine cannot accept input bytes until started.
         Assert.IsFalse(engine.IsWritable);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         Assert.IsTrue(engine.IsWritable);
         Assert.IsFalse(engine.IsShutdown);
         Assert.IsFalse(engine.IsFailed);
         Assert.IsNull(engine.FailureCause);
         Assert.AreEqual(EngineState.Started, engine.EngineState);

         engine.EngineFailed(new SaslException());

         Assert.IsFalse(engine.IsWritable);
         Assert.IsFalse(engine.IsShutdown);
         Assert.IsTrue(engine.IsFailed);
         Assert.IsNotNull(engine.FailureCause);
         Assert.AreEqual(EngineState.Failed, engine.EngineState);

         engine.Shutdown();

         Assert.IsFalse(engine.IsWritable);
         Assert.IsTrue(engine.IsShutdown);
         Assert.IsTrue(engine.IsFailed);
         Assert.IsNotNull(engine.FailureCause);
         Assert.AreEqual(EngineState.Shutdown, engine.EngineState);

         Assert.IsNotNull(connection);
         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is SaslException);
      }

      [Test]
      public void TestEngineStartAfterConnectionOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Engine cannot accept input bytes until started.
         Assert.IsFalse(engine.IsWritable);

         IConnection connection = engine.Connection;
         Assert.IsNotNull(connection);

         Assert.IsFalse(engine.IsShutdown);
         Assert.IsFalse(engine.IsFailed);
         Assert.IsNull(engine.FailureCause);

         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();

         // Should be idempotent and return same Connection
         IConnection another = engine.Start();
         Assert.AreSame(connection, another);

         // Default engine should start and return a connection immediately
         Assert.IsTrue(engine.IsWritable);
         Assert.IsNotNull(connection);
         Assert.IsNull(failure);

         peer.WaitForScriptToComplete();
      }

      [Test]
      public void TestEngineEmitsAMQPHeaderOnConnectionOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");

         connection.ContainerId = "test";
         connection.Open();

         Assert.IsFalse(engine.IsFailed);

         peer.WaitForScriptToComplete();

         Assert.AreEqual(ConnectionState.Active, connection.ConnectionState);
         Assert.AreEqual(ConnectionState.Active, connection.RemoteConnectionState);

         Assert.IsNull(failure);
      }

      [Test]
      public void TestTickFailsWhenConnectionNotOpenedNoLocalIdleSet()
      {
         DoTestTickFailsBasedOnState(false, false, false, false);
      }

      [Test]
      public void TestTickFailsWhenConnectionNotOpenedLocalIdleSet()
      {
         DoTestTickFailsBasedOnState(true, false, false, false);
      }

      [Test]
      public void TestTickFailsWhenEngineIsShutdownNoLocalIdleSet()
      {
         DoTestTickFailsBasedOnState(false, true, true, true);
      }

      [Test]
      public void TestTickFailsWhenEngineIsShutdownLocalIdleSet()
      {
         DoTestTickFailsBasedOnState(true, true, true, true);
      }

      [Test]
      public void TestTickFailsWhenEngineIsShutdownButCloseNotCalledNoLocalIdleSet()
      {
         DoTestTickFailsBasedOnState(false, true, false, true);
      }

      [Test]
      public void TestTickFailsWhenEngineIsShutdownButCloseNotCalledLocalIdleSet()
      {
         DoTestTickFailsBasedOnState(true, true, false, true);
      }

      private void DoTestTickFailsBasedOnState(bool setLocalTimeout, bool open, bool close, bool shutdown)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         if (setLocalTimeout)
         {
            connection.IdleTimeout = 1000;
         }

         if (open)
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            connection.Open();
         }

         if (close)
         {
            peer.ExpectClose().Respond();
            connection.Close();
         }

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);

         if (shutdown)
         {
            engine.Shutdown();
         }

         try
         {
            engine.Tick(5000);
            Assert.Fail("Should not be able to Tick an unopened connection");
         }
         catch (InvalidOperationException)
         {
         }
         catch (EngineShutdownException)
         {
         }
      }

      [Test]
      public void TestAutoTickFailsWhenConnectionNotOpenedNoLocalIdleSet()
      {
         DoTestAutoTickFailsBasedOnState(false, false, false, false);
      }

      [Test]
      public void TestAutoTickFailsWhenConnectionNotOpenedLocalIdleSet()
      {
         DoTestAutoTickFailsBasedOnState(true, false, false, false);
      }

      [Test]
      public void TestAutoTickFailsWhenEngineShutdownNoLocalIdleSet()
      {
         DoTestAutoTickFailsBasedOnState(false, true, true, true);
      }

      [Test]
      public void TestAutoTickFailsWhenEngineShutdownLocalIdleSet()
      {
         DoTestAutoTickFailsBasedOnState(true, true, true, true);
      }

      private void DoTestAutoTickFailsBasedOnState(bool setLocalTimeout, bool open, bool close, bool shutdown)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         if (setLocalTimeout)
         {
            connection.IdleTimeout = 1000;
         }

         if (open)
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            connection.Open();
         }

         if (close)
         {
            peer.ExpectClose().Respond();
            connection.Close();
         }

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);

         if (shutdown)
         {
            engine.Shutdown();
         }

         try
         {
            engine.TickAuto(Task.Factory);
            Assert.Fail("Should not be able to Tick an unopened connection");
         }
         catch (InvalidOperationException)
         {
         }
         catch (EngineShutdownException)
         {
         }
      }

      [Test]
      public void TestTickAutoPreventsDoubleInvocation()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         connection.Open();

         engine.TickAuto(Task.Factory);

         try
         {
            engine.TickAuto(Task.Factory);
            Assert.Fail("Should not be able call tickAuto more than once.");
         }
         catch (InvalidOperationException)
         {
         }

         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotCallTickAfterTickAutoCalled()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         connection.Open();

         engine.TickAuto(Task.Factory);

         try
         {
            engine.Tick(5000);
            Assert.Fail("Should not be able call Tick after enabling the auto Tick feature.");
         }
         catch (InvalidOperationException)
         {
         }

         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      [Ignore("Issue in test peer around begin handling needs to be fixed")]
      public void TestTickRemoteTimeout()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         uint remoteTimeout = 4000;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithIdleTimeOut(Is.NullValue()).Respond().WithIdleTimeOut(remoteTimeout);

         // Set our local idleTimeout
         connection.Open();

         long deadline = engine.Tick(0);
         Assert.AreEqual(2000, deadline, "Expected to be returned a deadline of 2000");  // deadline = 4000 / 2

         deadline = engine.Tick(1000);    // Wait for less than the deadline with no data - get the same value
         Assert.AreEqual(2000, deadline, "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "When the deadline hasn't been reached Tick() shouldn't write data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(remoteTimeout / 2); // Wait for the deadline - next deadline should be (4000/2)*2
         Assert.AreEqual(4000, deadline, "When the deadline has been reached expected a new deadline to be returned 4000");
         Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written data");

         peer.ExpectBegin();
         ISession session = connection.Session().Open();

         deadline = engine.Tick(3000);
         Assert.AreEqual(5000, deadline, "Writing data resets the deadline");
         Assert.AreEqual(1, peer.EmptyFrameCount, "When the deadline is reset Tick() shouldn't write an empty frame");

         peer.ExpectAttach();
         session.Sender("test").Open();

         deadline = engine.Tick(4000);
         Assert.AreEqual(6000, deadline, "Writing data resets the deadline");
         Assert.AreEqual(1, peer.EmptyFrameCount, "When the deadline is reset Tick() shouldn't write an empty frame");

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      [Ignore("Issue in test peer around begin handling needs to be fixed")]
      public void TestTickLocalTimeout()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         uint localTimeout = 4000;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithIdleTimeOut(localTimeout).Respond();

         // Set our local idleTimeout
         connection.IdleTimeout = localTimeout;
         connection.Open();

         long deadline = engine.Tick(0);
         Assert.AreEqual(4000, deadline, "Expected to be returned a deadline of 4000");

         deadline = engine.Tick(1000);    // Wait for less than the deadline with no data - get the same value
         Assert.AreEqual(4000, deadline, "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "Reading data should never result in a frame being written");

         // remote sends an empty frame now
         peer.RemoteEmptyFrame().Now();

         deadline = engine.Tick(2000);
         Assert.AreEqual(6000, deadline, "Reading data resets the deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "Reading data should never result in a frame being written");
         Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Reading data before the deadline should keep the connection open");

         peer.ExpectClose().Respond();

         deadline = engine.Tick(7000);
         Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState, "Calling Tick() after the deadline should result in the connection being closed");

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestTickWithZeroIdleTimeoutsGivesZeroDeadline()
      {
         DoTickWithNoIdleTimeoutGivesZeroDeadlineTestImpl(true);
      }

      [Test]
      public void TestTickWithNullIdleTimeoutsGivesZeroDeadline()
      {
         DoTickWithNoIdleTimeoutGivesZeroDeadlineTestImpl(false);
      }

      private void DoTickWithNoIdleTimeoutGivesZeroDeadlineTestImpl(bool useZero)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         if (useZero)
         {
            peer.ExpectOpen().WithIdleTimeOut(Is.NullValue()).Respond().WithIdleTimeOut(0);
         }
         else
         {
            peer.ExpectOpen().WithIdleTimeOut(Is.NullValue()).Respond();
         }

         connection.Open();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);

         Assert.AreEqual(0, connection.RemoteIdleTimeout);

         long deadline = engine.Tick(0);
         Assert.AreEqual(0, deadline, "Unexpected deadline returned");

         deadline = engine.Tick(10);
         Assert.AreEqual(0, deadline, "Unexpected deadline returned");

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTickWithLocalTimeout()
      {
         // all-positive
         DoTickWithLocalTimeoutTestImpl(4000, 10000, 14000, 18000, 22000);

         // all-negative
         DoTickWithLocalTimeoutTestImpl(2000, -100000, -98000, -96000, -94000);

         // negative to positive missing 0
         DoTickWithLocalTimeoutTestImpl(500, -950, -450, 50, 550);

         // negative to positive striking 0
         DoTickWithLocalTimeoutTestImpl(3000, -6000, -3000, 1, 3001);
      }

      private void DoTickWithLocalTimeoutTestImpl(uint localTimeout, long tick1, long expectedDeadline1, long expectedDeadline2, long expectedDeadline3)
      {
         this.failure = null;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithIdleTimeOut(localTimeout).Respond();

         // Set our local idleTimeout
         connection.IdleTimeout = localTimeout;
         connection.Open();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);

         long deadline = engine.Tick(tick1);
         Assert.AreEqual(expectedDeadline1, deadline, "Unexpected deadline returned");

         // Wait for less time than the deadline with no data - get the same value
         long interimTick = tick1 + 10;
         Assert.IsTrue(interimTick < expectedDeadline1);
         Assert.AreEqual(expectedDeadline1, engine.Tick(interimTick), "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(1, peer.PerformativeCount, "When the deadline hasn't been reached Tick() shouldn't write data");
         Assert.IsNull(failure);

         peer.RemoteEmptyFrame().Now();

         deadline = engine.Tick(expectedDeadline1);
         Assert.AreEqual(expectedDeadline2, deadline, "When the deadline has been reached expected a new local deadline to be returned");
         Assert.AreEqual(1, peer.PerformativeCount, "When the deadline hasn't been reached Tick() shouldn't write data");
         Assert.IsNull(failure);

         peer.RemoteEmptyFrame().Now();

         deadline = engine.Tick(expectedDeadline2);
         Assert.AreEqual(expectedDeadline3, deadline, "When the deadline has been reached expected a new local deadline to be returned");
         Assert.AreEqual(1, peer.PerformativeCount, "When the deadline hasn't been reached Tick() shouldn't write data");
         Assert.IsNull(failure);

         peer.ExpectClose().WithError(Is.NotNullValue()).Respond();

         Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");
         engine.Tick(expectedDeadline3); // Wait for the deadline, but don't receive traffic, allow local timeout to expire
         Assert.AreEqual(2, peer.PerformativeCount, "Tick() should have written data");
         Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState, "Calling Tick() after the deadline should result in the connection being closed");

         peer.WaitForScriptToComplete();
         Assert.IsNotNull(failure);
      }

      [Test]
      [Ignore("Issue in test peer around begin handling needs to be fixed")]
      public void TestTickWithRemoteTimeout()
      {
         // all-positive
         DoTickWithRemoteTimeoutTestImpl(4000, 10000, 14000, 18000, 22000);

         // all-negative
         DoTickWithRemoteTimeoutTestImpl(2000, -100000, -98000, -96000, -94000);

         // negative to positive missing 0
         DoTickWithRemoteTimeoutTestImpl(500, -950, -450, 50, 550);

         // negative to positive striking 0
         DoTickWithRemoteTimeoutTestImpl(3000, -6000, -3000, 1, 3001);
      }

      private void DoTickWithRemoteTimeoutTestImpl(uint remoteTimeoutHalf, long tick1, long expectedDeadline1, long expectedDeadline2, long expectedDeadline3)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         // Handle the peer transmitting [half] their timeout. We half it on receipt to avoid spurious timeouts
         // if they not have transmitted half their actual timeout, as the AMQP spec only says they SHOULD do that.
         peer.ExpectOpen().Respond().WithIdleTimeOut(remoteTimeoutHalf * 2);

         connection.Open();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);

         long deadline = engine.Tick(tick1);
         Assert.AreEqual(expectedDeadline1, deadline, "Unexpected deadline returned");

         // Wait for less time than the deadline with no data - get the same value
         long interimTick = tick1 + 10;
         Assert.IsTrue(interimTick < expectedDeadline1);
         Assert.AreEqual(expectedDeadline1, engine.Tick(interimTick), "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(1, peer.PerformativeCount, "When the deadline hasn't been reached Tick() shouldn't write data");
         Assert.AreEqual(0, peer.EmptyFrameCount, "When the deadline hasn't been reached Tick() shouldn't write data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(expectedDeadline1);
         Assert.AreEqual(expectedDeadline2, deadline, "When the deadline has been reached expected a new remote deadline to be returned");
         Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written data");

         peer.ExpectBegin();

         // Do some actual work, create real traffic, removing the need to send empty frame to satisfy idle-timeout
         connection.Session().Open();

         Assert.AreEqual(2, peer.PerformativeCount, "session open should have written data");

         deadline = engine.Tick(expectedDeadline2);
         Assert.AreEqual(expectedDeadline3, deadline, "When the deadline has been reached expected a new remote deadline to be returned");
         Assert.AreEqual(2, peer.PerformativeCount, "Tick() should not have written data as there was actual activity");
         Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should not have written data as there was actual activity");

         peer.ExpectEmptyFrame();

         engine.Tick(expectedDeadline3);
         Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() should have written data");

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestTickWithBothTimeouts()
      {
         // all-positive
         DoTickWithBothTimeoutsTestImpl(true, 5000, 2000, 10000, 12000, 14000, 15000);
         DoTickWithBothTimeoutsTestImpl(false, 5000, 2000, 10000, 12000, 14000, 15000);

         // all-negative
         DoTickWithBothTimeoutsTestImpl(true, 10000, 4000, -100000, -96000, -92000, -90000);
         DoTickWithBothTimeoutsTestImpl(false, 10000, 4000, -100000, -96000, -92000, -90000);

         // negative to positive missing 0
         DoTickWithBothTimeoutsTestImpl(true, 500, 200, -450, -250, -50, 50);
         DoTickWithBothTimeoutsTestImpl(false, 500, 200, -450, -250, -50, 50);

         // negative to positive striking 0 with local deadline
         DoTickWithBothTimeoutsTestImpl(true, 500, 200, -500, -300, -100, 1);
         DoTickWithBothTimeoutsTestImpl(false, 500, 200, -500, -300, -100, 1);

         // negative to positive striking 0 with remote deadline
         DoTickWithBothTimeoutsTestImpl(true, 500, 200, -200, 1, 201, 300);
         DoTickWithBothTimeoutsTestImpl(false, 500, 200, -200, 1, 201, 300);
      }

      private void DoTickWithBothTimeoutsTestImpl(bool allowLocalTimeout, uint localTimeout, uint remoteTimeoutHalf, long tick1,
                                                  long expectedDeadline1, long expectedDeadline2, long expectedDeadline3)
      {
         this.failure = null;
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         // Handle the peer transmitting [half] their timeout. We half it on receipt to avoid spurious timeouts
         // if they not have transmitted half their actual timeout, as the AMQP spec only says they SHOULD do that.
         peer.ExpectOpen().Respond().WithIdleTimeOut(remoteTimeoutHalf * 2);

         connection.IdleTimeout = localTimeout;
         connection.Open();

         long deadline = engine.Tick(tick1);
         Assert.AreEqual(expectedDeadline1, deadline, "Unexpected deadline returned");

         // Wait for less time than the deadline with no data - get the same value
         long interimTick = tick1 + 10;
         Assert.IsTrue(interimTick < expectedDeadline1);
         Assert.AreEqual(expectedDeadline1, engine.Tick(interimTick), "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "When the deadline hasn't been reached Tick() shouldn't write data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(expectedDeadline1);
         Assert.AreEqual(expectedDeadline2, deadline, "When the deadline has been reached expected a new remote deadline to be returned");
         Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(expectedDeadline2);
         Assert.AreEqual(expectedDeadline3, deadline, "When the deadline has been reached expected a new local deadline to be returned");
         Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() should have written data");

         peer.WaitForScriptToComplete();

         if (allowLocalTimeout)
         {
            peer.ExpectClose().Respond();

            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");
            engine.Tick(expectedDeadline3); // Wait for the deadline, but don't receive traffic, allow local timeout to expire
            Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState, "Calling Tick() after the deadline should result in the connection being closed");
            Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() should have written data but not an empty frame");

            peer.WaitForScriptToComplete();
            Assert.IsNotNull(failure);
         }
         else
         {
            peer.RemoteEmptyFrame().Now();

            deadline = engine.Tick(expectedDeadline3);
            Assert.AreEqual(expectedDeadline2 + (remoteTimeoutHalf), deadline, "Receiving data should have reset the deadline (to the next remote one)");
            Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() shouldn't have written data");
            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");

            peer.WaitForScriptToComplete();
            Assert.IsNull(failure);
         }
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsLocalThenRemote()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsLocalThenRemoteTestImpl(false);
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsLocalThenRemoteWithLocalTimeout()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsLocalThenRemoteTestImpl(true);
      }

      private void DoTickWithNanoTimeDerivedValueWhichWrapsLocalThenRemoteTestImpl(bool allowLocalTimeout)
      {
         uint localTimeout = 5000;
         uint remoteTimeoutHalf = 2000;
         Assert.IsTrue(remoteTimeoutHalf < localTimeout);

         long offset = 2500;
         Assert.IsTrue(offset < localTimeout);
         Assert.IsTrue(offset > remoteTimeoutHalf);

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         // Handle the peer transmitting [half] their timeout. We half it on receipt to avoid spurious timeouts
         // if they not have transmitted half their actual timeout, as the AMQP spec only says they SHOULD do that.
         peer.ExpectOpen().Respond().WithIdleTimeOut(remoteTimeoutHalf * 2);

         connection.IdleTimeout = localTimeout;
         connection.Open();

         long deadline = engine.Tick(long.MaxValue - offset);
         Assert.AreEqual(long.MaxValue - offset + remoteTimeoutHalf, deadline, "Unexpected deadline returned");

         deadline = engine.Tick(long.MaxValue - (offset - 100));    // Wait for less time than the deadline with no data - get the same value
         Assert.AreEqual(long.MaxValue - offset + remoteTimeoutHalf, deadline, "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "When the deadline hasn't been reached Tick() shouldn't write data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(long.MaxValue - offset + remoteTimeoutHalf); // Wait for the deadline - next deadline should be previous + remoteTimeoutHalf;
         Assert.AreEqual(long.MinValue + (2 * remoteTimeoutHalf) - offset - 1, deadline, "When the deadline has been reached expected a new remote deadline to be returned");
         Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(long.MinValue + (2 * remoteTimeoutHalf) - offset - 1); // Wait for the deadline - next deadline should be orig + localTimeout;
         Assert.AreEqual(long.MinValue + (localTimeout - offset) - 1, deadline, "When the deadline has been reached expected a new local deadline to be returned");
         Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() should have written data");

         peer.WaitForScriptToComplete();

         if (allowLocalTimeout)
         {
            peer.ExpectClose().Respond();

            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");
            engine.Tick(long.MinValue + (localTimeout - offset) - 1); // Wait for the deadline, but don't receive traffic, allow local timeout to expire
            Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState, "Calling Tick() after the deadline should result in the connection being closed");
            Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() should have written data but not an empty frame");

            peer.WaitForScriptToComplete();
            Assert.IsNotNull(failure);
         }
         else
         {
            peer.RemoteEmptyFrame().Now();

            deadline = engine.Tick(long.MinValue + (localTimeout - offset) - 1); // Wait for the deadline - next deadline should be orig + 3*remoteTimeoutHalf;
            Assert.AreEqual(long.MinValue + (3 * remoteTimeoutHalf) - offset - 1, deadline, "Receiving data should have reset the deadline (to the remote one)");
            Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() shouldn't have written data");
            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");

            peer.WaitForScriptToComplete();
            Assert.IsNull(failure);
         }
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsRemoteThenLocal()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsRemoteThenLocalTestImpl(false);
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsRemoteThenLocalWithLocalTimeout()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsRemoteThenLocalTestImpl(true);
      }

      private void DoTickWithNanoTimeDerivedValueWhichWrapsRemoteThenLocalTestImpl(bool allowLocalTimeout)
      {
         uint localTimeout = 2000;
         uint remoteTimeoutHalf = 5000;
         Assert.IsTrue(localTimeout < remoteTimeoutHalf);

         long offset = 2500;
         Assert.IsTrue(offset > localTimeout);
         Assert.IsTrue(offset < remoteTimeoutHalf);

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         // Handle the peer transmitting [half] their timeout. We half it on receipt to avoid spurious timeouts
         // if they not have transmitted half their actual timeout, as the AMQP spec only says they SHOULD do that.
         peer.ExpectOpen().Respond().WithIdleTimeOut(remoteTimeoutHalf * 2);

         connection.IdleTimeout = localTimeout;
         connection.Open();

         long deadline = engine.Tick(long.MaxValue - offset);
         Assert.AreEqual(long.MaxValue - offset + localTimeout, deadline, "Unexpected deadline returned");

         deadline = engine.Tick(long.MaxValue - (offset - 100));    // Wait for less time than the deadline with no data - get the same value
         Assert.AreEqual(long.MaxValue - offset + localTimeout, deadline, "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "Tick() shouldn't have written data");

         // Receive Empty frame to satisfy local deadline
         peer.RemoteEmptyFrame().Now();

         deadline = engine.Tick(long.MaxValue - offset + localTimeout); // Wait for the deadline - next deadline should be orig + 2* localTimeout;
         Assert.AreEqual(long.MinValue + (localTimeout - offset) - 1 + localTimeout, deadline, "When the deadline has been reached expected a new local deadline to be returned");
         Assert.AreEqual(0, peer.EmptyFrameCount, "Tick() should not have written data");

         peer.WaitForScriptToComplete();

         if (allowLocalTimeout)
         {
            peer.ExpectClose().Respond();

            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");
            engine.Tick(long.MinValue + (localTimeout - offset) - 1 + localTimeout); // Wait for the deadline, but don't receive traffic, allow local timeout to expire
            Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState, "Calling Tick() after the deadline should result in the connection being closed");
            Assert.AreEqual(0, peer.EmptyFrameCount, "Tick() should have written data but not an empty frame");

            peer.WaitForScriptToComplete();
            Assert.IsNotNull(failure);
         }
         else
         {
            // Receive Empty frame to satisfy local deadline
            peer.RemoteEmptyFrame().Now();

            deadline = engine.Tick(long.MinValue + (localTimeout - offset) - 1 + localTimeout); // Wait for the deadline - next deadline should be orig + remoteTimeoutHalf;
            Assert.AreEqual(long.MinValue + remoteTimeoutHalf - offset - 1, deadline, "Receiving data should have reset the deadline (to the remote one)");
            Assert.AreEqual(0, peer.EmptyFrameCount, "Tick() shouldn't have written data");

            peer.ExpectEmptyFrame();

            deadline = engine.Tick(long.MinValue + remoteTimeoutHalf - offset - 1); // Wait for the deadline - next deadline should be orig + 3* localTimeout;
            Assert.AreEqual(long.MinValue + (3 * localTimeout) - offset - 1, deadline, "When the deadline has been reached expected a new local deadline to be returned");
            Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written an empty frame");
            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");

            peer.WaitForScriptToComplete();
            Assert.IsNull(failure);
         }
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsBothRemoteFirst()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsBothRemoteFirstTestImpl(false);
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsBothRemoteFirstWithLocalTimeout()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsBothRemoteFirstTestImpl(true);
      }

      private void DoTickWithNanoTimeDerivedValueWhichWrapsBothRemoteFirstTestImpl(bool allowLocalTimeout)
      {
         uint localTimeout = 2000;
         uint remoteTimeoutHalf = 2500;
         Assert.IsTrue(localTimeout < remoteTimeoutHalf);

         long offset = 500;
         Assert.IsTrue(offset < localTimeout);

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         // Handle the peer transmitting [half] their timeout. We half it on receipt to avoid spurious timeouts
         // if they not have transmitted half their actual timeout, as the AMQP spec only says they SHOULD do that.
         peer.ExpectOpen().Respond().WithIdleTimeOut(remoteTimeoutHalf * 2);

         connection.IdleTimeout = localTimeout;
         connection.Open();

         long deadline = engine.Tick(long.MaxValue - offset);
         Assert.AreEqual(long.MinValue + (localTimeout - offset) - 1, deadline, "Unexpected deadline returned");

         deadline = engine.Tick(long.MaxValue - (offset - 100));    // Wait for less time than the deadline with no data - get the same value
         Assert.AreEqual(long.MinValue + (localTimeout - offset) - 1, deadline, "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "Tick() shouldn't have written data");

         // Receive Empty frame to satisfy local deadline
         peer.RemoteEmptyFrame().Now();

         deadline = engine.Tick(long.MinValue + (localTimeout - offset) - 1); // Wait for the deadline - next deadline should be orig + remoteTimeoutHalf;
         Assert.AreEqual(long.MinValue + (remoteTimeoutHalf - offset) - 1, deadline, "When the deadline has been reached expected a new remote deadline to be returned");
         Assert.AreEqual(0, peer.EmptyFrameCount, "When the deadline hasn't been reached Tick() shouldn't write data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(long.MinValue + (remoteTimeoutHalf - offset) - 1); // Wait for the deadline - next deadline should be orig + 2* localTimeout;
         Assert.AreEqual(long.MinValue + (localTimeout - offset) - 1 + localTimeout, deadline, "When the deadline has been reached expected a new local deadline to be returned");
         Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written data");

         peer.WaitForScriptToComplete();

         if (allowLocalTimeout)
         {
            peer.ExpectClose().Respond();

            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");
            engine.Tick(long.MinValue + (localTimeout - offset) - 1 + localTimeout); // Wait for the deadline, but don't receive traffic, allow local timeout to expire
            Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState, "Calling Tick() after the deadline should result in the connection being closed");
            Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written data but not an empty frame");

            peer.WaitForScriptToComplete();
            Assert.IsNotNull(failure);
         }
         else
         {
            // Receive Empty frame to satisfy local deadline
            peer.RemoteEmptyFrame().Now();

            deadline = engine.Tick(long.MinValue + (localTimeout - offset) - 1 + localTimeout); // Wait for the deadline - next deadline should be orig + 2*remoteTimeoutHalf;
            Assert.AreEqual(long.MinValue + (2 * remoteTimeoutHalf) - offset - 1, deadline, "Receiving data should have reset the deadline (to the remote one)");
            Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() shouldn't have written data");
            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");

            peer.WaitForScriptToComplete();
            Assert.IsNull(failure);
         }
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsBothLocalFirst()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsBothLocalFirstTestImpl(false);
      }

      [Test]
      public void TestTickWithNanoTimeDerivedValueWhichWrapsBothLocalFirstWithLocalTimeout()
      {
         DoTickWithNanoTimeDerivedValueWhichWrapsBothLocalFirstTestImpl(true);
      }

      private void DoTickWithNanoTimeDerivedValueWhichWrapsBothLocalFirstTestImpl(bool allowLocalTimeout)
      {
         uint localTimeout = 5000;
         uint remoteTimeoutHalf = 2000;
         Assert.IsTrue(remoteTimeoutHalf < localTimeout);

         long offset = 500;
         Assert.IsTrue(offset < remoteTimeoutHalf);

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         // Handle the peer transmitting [half] their timeout. We half it on receipt to avoid spurious timeouts
         // if they not have transmitted half their actual timeout, as the AMQP spec only says they SHOULD do that.
         peer.ExpectOpen().Respond().WithIdleTimeOut(remoteTimeoutHalf * 2);

         connection.IdleTimeout = localTimeout;
         connection.Open();

         long deadline = engine.Tick(long.MaxValue - offset);
         Assert.AreEqual(long.MinValue + (remoteTimeoutHalf - offset) - 1, deadline, "Unexpected deadline returned");

         deadline = engine.Tick(long.MaxValue - (offset - 100));    // Wait for less time than the deadline with no data - get the same value
         Assert.AreEqual(long.MinValue + (remoteTimeoutHalf - offset) - 1, deadline, "When the deadline hasn't been reached Tick() should return the previous deadline");
         Assert.AreEqual(0, peer.EmptyFrameCount, "When the deadline hasn't been reached Tick() shouldn't write data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(long.MinValue + (remoteTimeoutHalf - offset) - 1); // Wait for the deadline - next deadline should be previous + remoteTimeoutHalf;
         Assert.AreEqual(long.MinValue + (remoteTimeoutHalf - offset) - 1 + remoteTimeoutHalf, deadline, "When the deadline has been reached expected a new remote deadline to be returned");
         Assert.AreEqual(1, peer.EmptyFrameCount, "Tick() should have written data");

         peer.ExpectEmptyFrame();

         deadline = engine.Tick(long.MinValue + (remoteTimeoutHalf - offset) - 1 + remoteTimeoutHalf); // Wait for the deadline - next deadline should be orig + localTimeout;
         Assert.AreEqual(long.MinValue + (localTimeout - offset) - 1, deadline, "When the deadline has been reached expected a new local deadline to be returned");
         Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() should have written data");

         peer.WaitForScriptToComplete();

         if (allowLocalTimeout)
         {
            peer.ExpectClose().Respond();

            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");
            engine.Tick(long.MinValue + (localTimeout - offset) - 1); // Wait for the deadline, but don't receive traffic, allow local timeout to expire
            Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState, "Calling Tick() after the deadline should result in the connection being closed");
            Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() should have written data but not an empty frame");

            peer.WaitForScriptToComplete();
            Assert.IsNotNull(failure);
         }
         else
         {
            // Receive Empty frame to satisfy local deadline
            peer.RemoteEmptyFrame().Now();

            deadline = engine.Tick(long.MinValue + (localTimeout - offset) - 1); // Wait for the deadline - next deadline should be orig + 3*remoteTimeoutHalf;
            Assert.AreEqual(long.MinValue + (3 * remoteTimeoutHalf) - offset - 1, deadline, "Receiving data should have reset the deadline (to the remote one)");
            Assert.AreEqual(2, peer.EmptyFrameCount, "Tick() shouldn't have written data");
            Assert.AreEqual(ConnectionState.Active, connection.ConnectionState, "Connection should be active");

            peer.WaitForScriptToComplete();
            Assert.IsNull(failure);
         }
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte1()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'a', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0 });
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte2()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'A', (byte)'m', (byte)'Q', (byte)'P', 0, 1, 0, 0 });
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte3()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'A', (byte)'M', (byte)'q', (byte)'P', 0, 1, 0, 0 });
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte4()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'p', 0, 1, 0, 0 });
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte5()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 99, 1, 0, 0 });
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte6()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 99, 0, 0 });
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte7()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 99, 0 });
      }

      [Test]
      public void TestEngineFailsWithMeaningfulErrorOnNonAMQPHeaderResponseBadByte8()
      {
         DoTestEngineFailsWithMalformedHeaderException(new byte[] { (byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 99 });
      }

      private void DoTestEngineFailsWithMalformedHeaderException(byte[] headerBytes)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithBytes(headerBytes);

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);
         connection.Negotiate();

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
         Assert.IsTrue(failure is MalformedAMQPHeaderException);
      }

      [Test]
      public void TestEngineConfiguresDefaultMaxFrameSizeLimits()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);
         ProtonEngineConfiguration configuration = (ProtonEngineConfiguration)engine.Configuration;
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(ProtonConstants.DefaultMaxAmqpFrameSize).Respond();

         connection.Open();

         Assert.AreEqual(ProtonConstants.DefaultMaxAmqpFrameSize, configuration.OutboundMaxFrameSize);
         Assert.AreEqual(ProtonConstants.DefaultMaxAmqpFrameSize, configuration.InboundMaxFrameSize);

         // Default engine should start and return a connection immediately
         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineConfiguresSpecifiedMaxFrameSizeLimitsMatchesDefaultMinMax()
      {
         DoTestEngineConfiguresSpecifiedFrameSizeLimits(512, 512);
      }

      [Test]
      public void TestEngineConfiguresSpecifiedMaxFrameSizeLimitsRemoteLargerThanLocal()
      {
         DoTestEngineConfiguresSpecifiedFrameSizeLimits(1024, 1025);
      }

      [Test]
      public void TestEngineConfiguresSpecifiedMaxFrameSizeLimitsRemoteSmallerThanLocal()
      {
         DoTestEngineConfiguresSpecifiedFrameSizeLimits(1024, 1023);
      }

      [Test]
      public void TestEngineConfiguresSpecifiedMaxFrameSizeLimitsGreaterThanDefaultValues()
      {
         DoTestEngineConfiguresSpecifiedFrameSizeLimits(
             ProtonConstants.DefaultMaxAmqpFrameSize + 32, ProtonConstants.DefaultMaxAmqpFrameSize + 64);
      }

      [Test]
      public void TestEngineConfiguresRemoteMaxFrameSizeSetToMaxUnsignedLong()
      {
         DoTestEngineConfiguresSpecifiedFrameSizeLimits(int.MaxValue, uint.MaxValue);
      }

      private void DoTestEngineConfiguresSpecifiedFrameSizeLimits(uint localValue, uint remoteResponse)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);
         ProtonEngineConfiguration configuration = (ProtonEngineConfiguration)engine.Configuration;
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(localValue)
                          .Respond()
                          .WithMaxFrameSize(remoteResponse);

         connection.MaxFrameSize = localValue;
         connection.Open();

         if (localValue > 0)
         {
            Assert.AreEqual(localValue, configuration.InboundMaxFrameSize);
         }
         else
         {
            Assert.AreEqual(int.MaxValue, configuration.InboundMaxFrameSize);
         }

         if (remoteResponse > localValue)
         {
            Assert.AreEqual(localValue, configuration.OutboundMaxFrameSize);
         }
         else
         {
            if (remoteResponse > 0)
            {
               Assert.AreEqual(remoteResponse, configuration.OutboundMaxFrameSize);
            }
            else
            {
               Assert.AreEqual(int.MaxValue, configuration.OutboundMaxFrameSize);
            }
         }

         Assert.AreEqual(localValue, connection.MaxFrameSize);
         Assert.AreEqual(remoteResponse, connection.RemoteMaxFrameSize);

         // Default engine should start and return a connection immediately
         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineErrorsOnLocalMaxFrameSizeLargerThanImposedLimit()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         Assert.Throws<ArgumentOutOfRangeException>(() => connection.MaxFrameSize = uint.MaxValue);
      }

      [Test]
      public void TestEngineShutdownHandlerThrowsIsIgnoredAndShutdownCompletes()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);

         engine.ShutdownHandler((theEngine) => throw new InvalidOperationException());

         IConnection connection = engine.Start();
         Assert.IsNotNull(connection);

         Assert.IsTrue(engine.IsWritable);
         Assert.IsTrue(engine.IsRunning);
         Assert.IsFalse(engine.IsShutdown);
         Assert.IsFalse(engine.IsFailed);
         Assert.IsNull(engine.FailureCause);
         Assert.AreEqual(EngineState.Started, engine.EngineState);

         try
         {
            engine.Shutdown();
            Assert.Fail("User event handler throw wasn't propagated");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         Assert.IsFalse(engine.IsWritable);
         Assert.IsFalse(engine.IsRunning);
         Assert.IsTrue(engine.IsShutdown);
         Assert.IsFalse(engine.IsFailed);
         Assert.IsNull(engine.FailureCause);
         Assert.AreEqual(EngineState.Shutdown, engine.EngineState);

         // should not perform any additional work.
         engine.Shutdown();

         Assert.IsNotNull(connection);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestEnginePipelineProtectsFromExternalUserMischief()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();

         engine.Start();

         Assert.IsTrue(engine.IsWritable);
         Assert.IsNotNull(connection);
         Assert.IsNull(failure);

         Assert.Throws<InvalidOperationException>(() => engine.Pipeline.FireEngineStarting());
         Assert.Throws<InvalidOperationException>(() => engine.Pipeline.FireEngineStateChanged());
         Assert.Throws<InvalidOperationException>(() => engine.Pipeline.FireFailed(new EngineFailedException(null)));

         engine.Shutdown();

         Assert.Throws<EngineShutdownException>(() => engine.Pipeline.First());
         Assert.Throws<EngineShutdownException>(() => engine.Pipeline.Last());
         Assert.Throws<EngineShutdownException>(() => engine.Pipeline.FirstContext());
         Assert.Throws<EngineShutdownException>(() => engine.Pipeline.LastContext());

         peer.WaitForScriptToComplete();
      }
   }
}