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
using Apache.Qpid.Proton.Engine.Implementation.Sasl;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Security;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonSaslHandlerTest : ProtonEngineTestSupport
   {
      private FrameRecordingTransportHandler testHandler;

      [SetUp]
      public void SetUpHandler()
      {
         testHandler = new FrameRecordingTransportHandler();
      }

      [Test]
      public void TestCanRemoveSaslClientHandlerBeforeEngineStarted()
      {
         DoTestCanRemoveSaslHandlerBeforeEngineStarted(false);
      }

      [Test]
      public void TestCanRemoveSaslServerHandlerBeforeEngineStarted()
      {
         DoTestCanRemoveSaslHandlerBeforeEngineStarted(true);
      }

      private void DoTestCanRemoveSaslHandlerBeforeEngineStarted(bool server)
      {
         IEngine engine;

         if (server)
         {
            engine = CreateSaslServerEngine();
         }
         else
         {
            engine = CreateSaslClientEngine();
         }

         Assert.IsNotNull(engine.Pipeline.Find(ProtonConstants.SaslPerformativeHandler));

         engine.Pipeline.Remove(ProtonConstants.SaslPerformativeHandler);

         Assert.IsNull(engine.Pipeline.Find(ProtonConstants.SaslPerformativeHandler));
      }

      [Test]
      public void TestCannotInitiateSaslClientHandlerAfterEngineShutdown()
      {
         DoTestCannotInitiateSaslHandlerAfterEngineShutdown(false);
      }

      [Test]
      public void TestCannotInitiateSaslServerHandlerAfterEngineShutdown()
      {
         DoTestCannotInitiateSaslHandlerAfterEngineShutdown(true);
      }

      private void DoTestCannotInitiateSaslHandlerAfterEngineShutdown(bool server)
      {
         IEngine engine = CreateSaslCapableEngine();

         engine.Shutdown();

         if (server)
         {
            Assert.Throws<InvalidOperationException>(() => engine.SaslDriver.Server());
         }
         else
         {
            Assert.Throws<InvalidOperationException>(() => engine.SaslDriver.Client());
         }
      }

      [Test]
      public void TestCannotSaslDriverChangeMaxFrameSizeAfterSASLAuthBegins()
      {
         IEngine engine = CreateSaslServerEngine();

         engine.Start();
         engine.Pipeline.FireRead(HeaderEnvelope.SASL_HEADER_ENVELOPE);

         Assert.Throws<InvalidOperationException>(() => engine.SaslDriver.MaxFrameSize = 1024);
      }

      [Test]
      public void TestCannotSaslDriverChangeMaxFrameSizeSmallerThanSpecMin()
      {
         IEngine engine = CreateSaslServerEngine();

         engine.Start();

         Assert.Throws<ArgumentOutOfRangeException>(() => engine.SaslDriver.MaxFrameSize = 256);
      }

      [Test]
      public void TestCanChangeSaslDriverMaxFrameSizeSmallerThanSpecMin()
      {
         IEngine engine = CreateSaslServerEngine();

         engine.Start();
         engine.SaslDriver.MaxFrameSize = 2048;

         Assert.AreEqual(2048, engine.SaslDriver.MaxFrameSize);
      }

      [Test]
      public void TestCannotRegisterSaslDriverAfterEngineStarted()
      {
         ProtonEngine engine = (ProtonEngine)IEngineFactory.Proton.CreateEngine();

         engine.Start();

         Assert.IsTrue(engine.IsRunning);
         Assert.Throws<EngineStartedException>(() => engine.RegisterSaslDriver(new ProtonEngineNoOpSaslDriver()));
      }

      /**
       * Test that when the SASL server handler reads an AMQP Header before negotiations
       * have started it rejects the exchange by sending a SASL Header back to the remote
       */
      [Test]
      public void TestSaslRejectsAMQPHeader()
      {
         bool headerRead = false;

         IEngine engine = CreateSaslServerEngine();

         engine.SaslDriver.Server().Authenticator = new TestSaslServerAuthenticator();
         engine.Start();

         try
         {
            engine.Pipeline.FireRead(HeaderEnvelope.AMQP_HEADER_ENVELOPE);
            Assert.Fail("SASL handler should reject a non-SASL AMQP Header read.");
         }
         catch (ProtocolViolationException)
         {
            // Expected
         }

         Assert.IsFalse(headerRead, "Should not receive a Header");

         IList<HeaderEnvelope> headers = testHandler.HeadersWritten;

         Assert.AreEqual(1, headers.Count, "Sasl Anonymous exchange output not as expected");

         for (int i = 0; i < headers.Count; ++i)
         {
            HeaderEnvelope frame = headers[i];
            switch (i)
            {
               case 0:
                  Assert.IsTrue(frame.FrameType == HeaderEnvelope.HeaderFrameType);
                  HeaderEnvelope header = (HeaderEnvelope)frame;
                  Assert.IsTrue(header.Body.IsSaslHeader(), "Should have written a SASL Header in response");
                  break;
            }
         }

         Assert.AreEqual(0, testHandler.AmqpFramesWritten.Count);
         Assert.AreEqual(0, testHandler.SaslFramesWritten.Count);

         Assert.AreEqual(EngineState.Failed, engine.EngineState);
      }

      [Test]
      public void TestExchangeSaslHeader()
      {
         bool saslHeaderRead = false;
         IEngine engine = CreateSaslServerEngine().Start().Engine;

         engine.SaslDriver.Server().Authenticator = new TestSaslServerAuthenticator()
            .HeaderHandler((context, header) =>
            {
               saslHeaderRead = true;
               context.SendMechanisms(new Symbol[] { Symbol.Lookup("ANONYMOUS") });
            });

         engine.Pipeline.FireRead(HeaderEnvelope.SASL_HEADER_ENVELOPE);

         Assert.Throws<InvalidOperationException>(() => engine.SaslDriver.Client());

         Assert.IsTrue(saslHeaderRead, "Did not receive a SASL Header");

         IList<HeaderEnvelope> frames = testHandler.HeadersWritten;

         // We should get a SASL header indicating that the server accepted SASL
         Assert.AreEqual(1, frames.Count, "Sasl Anonymous exchange output not as expected");

         for (int i = 0; i < frames.Count; ++i)
         {
            HeaderEnvelope frame = frames[i];
            switch (i)
            {
               case 0:
                  Assert.IsTrue(frame.FrameType == HeaderEnvelope.HeaderFrameType);
                  HeaderEnvelope header = (HeaderEnvelope)frame;
                  Assert.IsTrue(header.Body.IsSaslHeader());
                  break;
               default:
                  Assert.Fail("Invalid Frame read during exchange: " + frame);
                  break;
            }
         }
      }

      [Test]
      public void TestSaslAnonymousExchange()
      {
         bool saslHeaderRead = false;

         string clientHostname = null;
         Symbol clientMechanism = null;
         bool emptyResponse = false;

         IEngine engine = CreateSaslServerEngine();

         engine.SaslDriver.Server().Authenticator = new TestSaslServerAuthenticator()
            .HeaderHandler((context, header) =>
            {
               saslHeaderRead = true;
               context.SendMechanisms(new Symbol[] { Symbol.Lookup("ANONYMOUS") });
            })
            .InitHandler((context, mechanism, initialResponse) =>
            {
               clientHostname = context.Hostname;
               clientMechanism = context.ChosenMechanism;
               if (initialResponse.ReadableBytes == 0)
               {
                  emptyResponse = true;
               }

               context.SendOutcome(SaslAuthOutcome.SaslOk, null);
            });

         // Check for Header processing
         engine.Start().Engine.Pipeline.FireRead(HeaderEnvelope.SASL_HEADER_ENVELOPE);

         Assert.IsTrue(saslHeaderRead, "Did not receive a SASL Header");

         SaslInit clientInit = new SaslInit();
         clientInit.Hostname = "HOST-NAME";
         clientInit.Mechanism = Symbol.Lookup("ANONYMOUS");
         clientInit.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[0]);

         // Check for Initial Response processing
         engine.Pipeline.FireRead(new SaslEnvelope(clientInit));

         Assert.AreEqual("HOST-NAME", clientHostname);
         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), clientMechanism);
         Assert.IsTrue(emptyResponse, "Response should be an empty byte array");

         Assert.AreEqual(1, testHandler.HeadersWritten.Count);
         Assert.AreEqual(2, testHandler.SaslFramesWritten.Count);

         HeaderEnvelope header = testHandler.HeadersWritten[0];
         Assert.IsTrue(header.FrameType == HeaderEnvelope.HeaderFrameType);
         Assert.IsTrue(header.Body.IsSaslHeader());

         SaslEnvelope firstSaslSend = testHandler.SaslFramesWritten[0];
         SaslEnvelope secondSaslSend = testHandler.SaslFramesWritten[1];

         Assert.AreEqual(SaslPerformativeType.Mechanisms, firstSaslSend.Body.Type);
         SaslMechanisms mechanisms = (SaslMechanisms)firstSaslSend.Body;
         Assert.AreEqual(1, mechanisms.Mechanisms.Length);
         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), mechanisms.Mechanisms[0]);

         Assert.AreEqual(SaslPerformativeType.Outcome, secondSaslSend.Body.Type);
         SaslOutcome outcome = (SaslOutcome)secondSaslSend.Body;
         Assert.AreEqual(SaslCode.Ok, outcome.Code);
      }

      [Test]
      public void TestEngineFailedIfMoreSaslFramesArriveAfterSaslDone()
      {
         bool saslHeaderRead = false;

         string clientHostname = null;
         Symbol clientMechanism = null;
         bool emptyResponse = false;

         IEngine engine = CreateSaslServerEngine();

         engine.SaslDriver.Server().Authenticator = new TestSaslServerAuthenticator()
            .HeaderHandler((context, header) =>
            {
               saslHeaderRead = true;
               context.SendMechanisms(new Symbol[] { Symbol.Lookup("ANONYMOUS") });
            })
            .InitHandler((context, mechanism, initialResponse) =>
            {
               clientHostname = context.Hostname;
               clientMechanism = context.ChosenMechanism;
               if (initialResponse.ReadableBytes == 0)
               {
                  emptyResponse = true;
               }

               context.SendOutcome(SaslAuthOutcome.SaslOk, null);
            });

         // Check for Header processing
         engine.Start().Engine.Pipeline.FireRead(HeaderEnvelope.SASL_HEADER_ENVELOPE);

         Assert.IsTrue(saslHeaderRead, "Did not receive a SASL Header");

         SaslInit clientInit = new SaslInit();
         clientInit.Hostname = "HOST-NAME";
         clientInit.Mechanism = Symbol.Lookup("ANONYMOUS");
         clientInit.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[0]);

         // Check for Initial Response processing
         engine.Pipeline.FireRead(new SaslEnvelope(clientInit));

         Assert.AreEqual("HOST-NAME", clientHostname);
         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), clientMechanism);
         Assert.IsTrue(emptyResponse, "Response should be an empty byte array");

         Assert.AreEqual(1, testHandler.HeadersWritten.Count);
         Assert.AreEqual(2, testHandler.SaslFramesWritten.Count);

         Assert.AreEqual(engine.SaslDriver.SaslState, EngineSaslState.Authenticated);

         // Fire another SASL frame and the engine should fail
         try
         {
            engine.Pipeline.FireRead(new SaslEnvelope(clientInit));
            Assert.Fail("Server should fail on unexpected SASL frames");
         }
         catch (EngineFailedException)
         {
         }

         Assert.IsTrue(engine.IsFailed);
      }

      [Test]
      public void TestSaslHandlerDefaultsIntoServerMode()
      {
         IEngine engine = CreateSaslCapableEngine();

         // Swallow incoming so we can test that an AMQP Header arrives after SASL
         engine.Pipeline.AddFirst("read-sink", new FrameReadSinkTransportHandler());

         // Check for Header processing
         engine.Start().Engine.Pipeline.FireRead(HeaderEnvelope.SASL_HEADER_ENVELOPE);

         SaslInit clientInit = new SaslInit();
         clientInit.Hostname = "HOST-NAME";
         clientInit.Mechanism = Symbol.Lookup("ANONYMOUS");
         clientInit.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[0]);

         // Check for Initial Response processing
         engine.Pipeline.FireRead(new SaslEnvelope(clientInit));

         Assert.AreEqual(1, testHandler.HeadersWritten.Count);
         Assert.AreEqual(2, testHandler.SaslFramesWritten.Count);

         HeaderEnvelope header1 = testHandler.HeadersWritten[0];
         Assert.IsTrue(header1.FrameType == HeaderEnvelope.HeaderFrameType);
         Assert.IsTrue(header1.Body.IsSaslHeader());

         SaslEnvelope firstSaslSend = testHandler.SaslFramesWritten[0];
         SaslEnvelope secondSaslSend = testHandler.SaslFramesWritten[1];

         Assert.AreEqual(SaslPerformativeType.Mechanisms, firstSaslSend.Body.Type);
         SaslMechanisms mechanisms = (SaslMechanisms)firstSaslSend.Body;
         Assert.AreEqual(1, mechanisms.Mechanisms.Length);
         Assert.AreEqual(Symbol.Lookup("PLAIN"), mechanisms.Mechanisms[0]);

         Assert.AreEqual(SaslPerformativeType.Outcome, secondSaslSend.Body.Type);
         SaslOutcome outcome = (SaslOutcome)secondSaslSend.Body;
         Assert.AreEqual(SaslCode.Auth, outcome.Code);
      }

      [Test]
      public void TestEngineFailedWhenNonSaslFrameWrittenDuringSaslExchange()
      {
         bool saslHeaderRead = false;

         string clientHostname = null;
         Symbol clientMechanism = null;

         IEngine engine = CreateSaslServerEngine();

         engine.SaslDriver.Server().Authenticator = new TestSaslServerAuthenticator()
            .HeaderHandler((context, header) =>
            {
               saslHeaderRead = true;
               context.SendMechanisms(new Symbol[] { Symbol.Lookup("ANONYMOUS") });
            })
            .InitHandler((context, mechanism, initialResponse) =>
            {
               clientHostname = context.Hostname;
               clientMechanism = context.ChosenMechanism;
            });

         // Check for Header processing
         engine.Start().Engine.Pipeline.FireRead(HeaderEnvelope.SASL_HEADER_ENVELOPE);

         Assert.IsTrue(saslHeaderRead, "Did not receive a SASL Header");

         SaslInit clientInit = new SaslInit();
         clientInit.Hostname = "HOST-NAME";
         clientInit.Mechanism = Symbol.Lookup("ANONYMOUS");
         clientInit.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[0]);

         // Check for Initial Response processing
         engine.Pipeline.FireRead(new SaslEnvelope(clientInit));

         Assert.AreEqual("HOST-NAME", clientHostname);
         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), clientMechanism);

         Assert.AreEqual(1, testHandler.HeadersWritten.Count);
         Assert.AreEqual(1, testHandler.SaslFramesWritten.Count);

         try
         {
            engine.Pipeline.FireWrite(
               AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool().Take(new Open(), 0, null));
         }
         catch (ProtocolViolationException) { }

         Assert.IsTrue(engine.IsFailed);

         HeaderEnvelope header1 = testHandler.HeadersWritten[0];
         Assert.IsTrue(header1.FrameType == HeaderEnvelope.HeaderFrameType);
         Assert.IsTrue(header1.Body.IsSaslHeader());

         SaslEnvelope firstSaslSend = testHandler.SaslFramesWritten[0];

         Assert.AreEqual(SaslPerformativeType.Mechanisms, firstSaslSend.Body.Type);
         SaslMechanisms mechanisms = (SaslMechanisms)firstSaslSend.Body;
         Assert.AreEqual(1, mechanisms.Mechanisms.Length);
         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), mechanisms.Mechanisms[0]);
      }

      private IEngine CreateSaslServerEngine()
      {
         ProtonEngine engine = new ProtonEngine();

         engine.Pipeline.AddLast("sasl", new ProtonSaslHandler());
         engine.Pipeline.AddLast("test", testHandler);
         engine.Pipeline.AddLast("write-sink", new FrameWriteSinkTransportHandler());

         // Ensure engine SASL driver is configured for server mode.
         engine.SaslDriver.Server();

         return engine;
      }

      private IEngine CreateSaslClientEngine()
      {
         ProtonEngine engine = new ProtonEngine();

         engine.Pipeline.AddLast("sasl", new ProtonSaslHandler());
         engine.Pipeline.AddLast("test", testHandler);
         engine.Pipeline.AddLast("write-sink", new FrameWriteSinkTransportHandler());

         // Ensure engine SASL driver is configured for client mode.
         engine.SaslDriver.Client();

         return engine;
      }

      private IEngine CreateSaslCapableEngine()
      {
         ProtonEngine engine = new ProtonEngine();

         engine.Pipeline.AddLast("sasl", new ProtonSaslHandler());
         engine.Pipeline.AddLast("test", testHandler);
         engine.Pipeline.AddLast("write-sink", new FrameWriteSinkTransportHandler());

         return engine;
      }

      private class TestSaslServerAuthenticator : ISaslServerAuthenticator
      {
         private Action<ISaslServerContext, AmqpHeader> saslHeaderHandler;
         private Action<ISaslServerContext, Symbol, IProtonBuffer> saslInitHandler;
         private Action<ISaslServerContext, IProtonBuffer> saslResponseHandler;

         public TestSaslServerAuthenticator HeaderHandler(Action<ISaslServerContext, AmqpHeader> handler)
         {
            this.saslHeaderHandler = handler;
            return this;
         }

         public TestSaslServerAuthenticator InitHandler(Action<ISaslServerContext, Symbol, IProtonBuffer> handler)
         {
            this.saslInitHandler = handler;
            return this;
         }

         public TestSaslServerAuthenticator ResponseHandler(Action<ISaslServerContext, IProtonBuffer> handler)
         {
            this.saslResponseHandler = handler;
            return this;
         }

         public void HandleSaslHeader(ISaslServerContext context, AmqpHeader header)
         {
            saslHeaderHandler?.Invoke(context, header);
         }

         public void HandleSaslInit(ISaslServerContext context, Symbol mechanism, IProtonBuffer initResponse)
         {
            saslInitHandler?.Invoke(context, mechanism, initResponse);
         }

         public void HandleSaslResponse(ISaslServerContext context, IProtonBuffer response)
         {
            saslResponseHandler?.Invoke(context, response);
         }
      }
   }
}