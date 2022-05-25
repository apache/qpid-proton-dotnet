/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed With
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance With
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
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class DeclareExpectation : TransferExpectation
   {
      private readonly AmqpValueMatcher defaultPayloadMatcher = new(new DeclareMatcher());

      public DeclareExpectation(AMQPTestDriver driver) : base(driver)
      {
         WithPayload(defaultPayloadMatcher);
      }

      public override DispositionInjectAction Accept()
      {
         byte[] txnId = new byte[4];

         Random rand = new(Environment.TickCount);
         rand.NextBytes(txnId);

         return Accept(txnId);
      }

      /// <summary>
      /// Indicate a successful transaction declaration and provides a
      /// transaction Id value that identifies the new transaction.
      /// </summary>
      /// <param name="txnId">The transaction id for the newly declared transaction</param>
      /// <returns>A disposition inject action that will be sent as a response.</returns>
      public DispositionInjectAction Accept(byte[] txnId)
      {
         response = new DispositionInjectAction(driver);
         response.WithSettled(true);
         if (txnId != null)
         {
            response.WithState(new Declared(new Binary(txnId)));
         }
         else
         {
            response.WithState(new Declared());
         }

         driver.AddScriptedElement(response);
         return response;
      }

      /// <summary>
      /// Indicate a successful transaction declaration and provides a
      /// transaction Id value that identifies the new transaction.
      /// </summary>
      /// <param name="txnId">The transaction id for the newly declared transaction</param>
      /// <returns>A disposition inject action that will be sent as a response.</returns>
      public DispositionInjectAction Declared(byte[] txnId)
      {
         response = new DispositionInjectAction(driver);
         response.WithSettled(true);
         if (txnId != null)
         {
            response.WithState(new Declared(new Binary(txnId)));
         }
         else
         {
            response.WithState(new Declared());
         }

         driver.AddScriptedElement(response);
         return response;
      }

      public override DeclareExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public DeclareExpectation WithDeclare(Declare declare)
      {
         WithPayload(new AmqpValueMatcher(declare));
         return this;
      }

      public DeclareExpectation WithNullDeclare()
      {
         WithPayload(new AmqpValueMatcher(null));
         return this;
      }

      #region Type specific With API that performs simple equals checks

      public override DeclareExpectation WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public override DeclareExpectation WithDeliveryId(uint deliveryId)
      {
         return WithDeliveryId(Is.EqualTo(deliveryId));
      }

      public override DeclareExpectation WithDeliveryTag(byte[] tag)
      {
         return WithDeliveryTag(new Binary(tag));
      }

      public override DeclareExpectation WithDeliveryTag(Binary deliveryTag)
      {
         return WithDeliveryTag(Is.EqualTo(deliveryTag));
      }

      public override DeclareExpectation WithMessageFormat(uint messageFormat)
      {
         return WithMessageFormat(Is.EqualTo(messageFormat));
      }

      public override DeclareExpectation WithSettled(bool settled)
      {
         return WithSettled(Is.EqualTo(settled));
      }

      public override DeclareExpectation WithMore(bool more)
      {
         return WithMore(Is.EqualTo(more));
      }

      public override DeclareExpectation WithRcvSettleMode(ReceiverSettleMode rcvSettleMode)
      {
         return WithRcvSettleMode(Is.EqualTo(rcvSettleMode));
      }

      public override DeclareExpectation WithState(IDeliveryState state)
      {
         return WithState(Is.EqualTo(state));
      }

      public override DeclareExpectation WithNullState()
      {
         return WithState(Is.NullValue());
      }

      public override DeclareExpectation WithResume(bool resume)
      {
         return WithResume(Is.EqualTo(resume));
      }

      public override DeclareExpectation WithAborted(bool aborted)
      {
         return WithAborted(Is.EqualTo(aborted));
      }

      public override DeclareExpectation WithBatchable(bool batchable)
      {
         return WithBatchable(Is.EqualTo(batchable));
      }

      #endregion

      #region Matcher based API overrides

      public override DeclareExpectation WithHandle(IMatcher m)
      {
         base.WithHandle(m);
         return this;
      }

      public override DeclareExpectation WithDeliveryId(IMatcher m)
      {
         base.WithDeliveryId(m);
         return this;
      }

      public override DeclareExpectation WithDeliveryTag(IMatcher m)
      {
         base.WithDeliveryTag(m);
         return this;
      }

      public override DeclareExpectation WithMessageFormat(IMatcher m)
      {
         base.WithMessageFormat(m);
         return this;
      }

      public override DeclareExpectation WithSettled(IMatcher m)
      {
         base.WithSettled(m);
         return this;
      }

      public override DeclareExpectation WithMore(IMatcher m)
      {
         base.WithMore(m);
         return this;
      }

      public override DeclareExpectation WithRcvSettleMode(IMatcher m)
      {
         base.WithRcvSettleMode(m);
         return this;
      }

      public override DeclareExpectation WithState(IMatcher m)
      {
         base.WithState(m);
         return this;
      }

      public override DeclareExpectation WithResume(IMatcher m)
      {
         base.WithResume(m);
         return this;
      }

      public override DeclareExpectation WithAborted(IMatcher m)
      {
         base.WithAborted(m);
         return this;
      }

      public override DeclareExpectation WithBatchable(IMatcher m)
      {
         base.WithBatchable(m);
         return this;
      }

      #endregion
   }
}