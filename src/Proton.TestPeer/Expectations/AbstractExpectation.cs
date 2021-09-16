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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Apache.Qpid.Proton.Test.Driver.Matchers;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// The AMQP Test driver internal frame processing a script handler class.
   /// </summary>
   public abstract class AbstractExpectation<T> : ScriptedExpectation where T : ListDescribedType
   {
      private ushort? channelExpectation;
      private uint? frameSizeExpectation;
      private bool optional;

      protected readonly AMQPTestDriver driver;

      public AbstractExpectation(AMQPTestDriver driver)
      {
         this.driver = driver;
      }

      public virtual AbstractExpectation<T> OnChannel(ushort channel)
      {
         this.channelExpectation = channel;
         return this;
      }

      public override bool IsOptional => optional;

      public virtual AbstractExpectation<T> Optional()
      {
         this.optional = true;
         return this;
      }

      /// <summary>
      /// Configures the expected frame size that this expectation will enforce when processing
      /// a new scripted expectation. Generally useful for testing that split framed transfers
      /// are transmitted in optimal or at least correct sized frames.
      /// </summary>
      /// <param name="frameSize">The expected frame size this performative arrives in</param>
      public void WithFrameSize(uint frameSize)
      {
         this.frameSizeExpectation = frameSize;
      }

      #region Verification handlers which perform the performative checks

      protected void VerifyPerformative(T performative)
      {
         // TODO
         // LOG.debug("About to check the fields of the performative." +
         //          "\n  Received:" + performative + "\n  Expectations: " + getExpectationMatcher());

         MatcherAssert.AssertThat("Performative does not match expectation", performative, GetExpectationMatcher());
      }

      protected void VerifyPayload(byte[] payload)
      {
         if (GetPayloadMatcher() != null)
         {
            MatcherAssert.AssertThat("Payload does not match expectation", payload, GetPayloadMatcher());
         }
         else if (payload != null)
         {
            throw new ArgumentException("Performative should not have been sent with a payload: ");
         }
      }

      protected void VerifyChannel(ushort channel)
      {
         if ((channelExpectation ?? channel) != channel)
         {
            throw new ArgumentOutOfRangeException("Expected send on channel + " + channelExpectation + ": but was on channel:" + channel);
         }
      }

      protected void VerifyFrameSize(uint frameSize)
      {
         if ((frameSizeExpectation ?? frameSize) != frameSize)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "Expected frame size {0} did not match that of the received frame: {1}", frameSizeExpectation, frameSize));
         }
      }

      protected abstract IMatcher GetExpectationMatcher();

      protected IMatcher GetPayloadMatcher()
      {
         return null;
      }

      #endregion

      #region Overrides for all the incoming frames and headers events

      public override void HandleAMQPHeader(AMQPHeader header, AMQPTestDriver context)
      {
         DoVerification((uint)header.Buffer.Length, header, null, 0, context);
      }

      public override void HandleSASLHeader(AMQPHeader header, AMQPTestDriver context)
      {
         DoVerification((uint)header.Buffer.Length, header, null, 0, context);
      }

      public override void HandleOpen(uint frameSize, Open open, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, open, payload, channel, context);
      }

      public override void HandleBegin(uint frameSize, Begin begin, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, begin, payload, channel, context);
      }

      public override void HandleAttach(uint frameSize, Attach attach, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, attach, payload, channel, context);
      }

      public override void HandleFlow(uint frameSize, Flow flow, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, flow, payload, channel, context);
      }

      public override void HandleTransfer(uint frameSize, Transfer transfer, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, transfer, payload, channel, context);
      }

      public override void HandleDisposition(uint frameSize, Disposition disposition, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, disposition, payload, channel, context);
      }

      public override void HandleDetach(uint frameSize, Detach detach, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, detach, payload, channel, context);
      }

      public override void HandleEnd(uint frameSize, End end, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, end, payload, channel, context);
      }

      public override void HandleClose(uint frameSize, Close close, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         DoVerification(frameSize, close, payload, channel, context);
      }

      public override void HandleMechanisms(uint frameSize, SaslMechanisms saslMechanisms, AMQPTestDriver context)
      {
         DoVerification(frameSize, saslMechanisms, null, 0, context);
      }

      public override void HandleInit(uint frameSize, SaslInit saslInit, AMQPTestDriver context)
      {
         DoVerification(frameSize, saslInit, null, 0, context);
      }

      public override void HandleChallenge(uint frameSize, SaslChallenge saslChallenge, AMQPTestDriver context)
      {
         DoVerification(frameSize, saslChallenge, null, 0, context);
      }

      public override void HandleResponse(uint frameSize, SaslResponse saslResponse, AMQPTestDriver context)
      {
         DoVerification(frameSize, saslResponse, null, 0, context);
      }

      public override void HandleOutcome(uint frameSize, SaslOutcome saslOutcome, AMQPTestDriver context)
      {
         DoVerification(frameSize, saslOutcome, null, 0, context);
      }

      #endregion

      private void DoVerification(uint frameSize, object performative, byte[] payload, ushort channel, AMQPTestDriver driver)
      {
         if (typeof(T).Equals(performative.GetType()))
         {
            VerifyFrameSize(frameSize);
            VerifyPayload(payload);
            VerifyChannel(channel);
            VerifyPerformative((T)performative);
         }
         else
         {
            ReportTypeExpectationError(performative, typeof(T));
         }
      }

      private void ReportTypeExpectationError(object received, Type expected)
      {
         throw new UnexpectedPerformativeError("Expected type: " + expected + " but received value: " + received);
      }
   }
}