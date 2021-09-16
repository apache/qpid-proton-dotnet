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
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Expectation type that defines a want for some incoming activity
   /// that defines success or failure of a scripted test sequence.
   /// </summary>
   public abstract class ScriptedExpectation : IScriptedElement,
                                               IHeaderHandler<AMQPTestDriver>,
                                               IPerformativeHandler<AMQPTestDriver>,
                                               ISaslPerformativeHandler<AMQPTestDriver>
   {
      ScriptEntryType IScriptedElement.ScriptedType => ScriptEntryType.Expectation;

      /// <summary>
      /// Indicates if the scripted element is optional and the test should not fail
      /// if the element desired outcome is not met.
      /// </summary>
      public virtual bool IsOptional => false;

      /// <summary>
      /// Provides a scripted expectation the means of initiating some action
      /// following successful compeltion of the expectation.
      /// </summary>
      /// <returns>a scripted action to perform following the expectation being me</returns>
      public ScriptedAction PerformAfterwards() => null;

      public virtual void HandleAMQPHeader(AMQPHeader header, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleSASLHeader(AMQPHeader header, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleOpen(uint frameSize, Open open, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleBegin(uint frameSize, Begin begin, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleAttach(uint frameSize, Attach attach, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleFlow(uint frameSize, Flow flow, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleTransfer(uint frameSize, Transfer transfer, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleDisposition(uint frameSize, Disposition disposition, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleDetach(uint frameSize, Detach detach, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleEnd(uint frameSize, End end, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleClose(uint frameSize, Close close, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleHeartbeat(uint frameSize, Heartbeat beat, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleMechanisms(uint frameSize, SaslMechanisms saslMechanisms, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleInit(uint frameSize, SaslInit saslInit, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleChallenge(uint frameSize, SaslChallenge saslChallenge, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleResponse(uint frameSize, SaslResponse saslResponse, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }

      public virtual void HandleOutcome(uint frameSize, SaslOutcome saslOutcome, AMQPTestDriver context)
      {
         throw new NotImplementedException("Handle received AMQP performative but it wasn't handled.");
      }
   }
}