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
            Assert.Fail("Should not be able to tick an unopened connection");
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
            Assert.Fail("Should not be able to tick an unopened connection");
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
            Assert.Fail("Should not be able call tick after enabling the auto tick feature.");
         }
         catch (InvalidOperationException)
         {
         }

         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }
   }
}