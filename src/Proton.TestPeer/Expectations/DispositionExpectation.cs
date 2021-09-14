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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class DispositionExpectation : AbstractExpectation<Detach>
   {
      private readonly DispositionMatcher matcher = new DispositionMatcher();
      private readonly DispositionDeliveryStateBuilder stateBuilder;

      public DispositionExpectation(AMQPTestDriver driver) : base(driver)
      {
         this.stateBuilder = new DispositionDeliveryStateBuilder(this);

         // Default validation of mandatory fields
         WithRole(Is.NotNullValue());
         WithFirst(Is.NotNullValue());
      }

      protected override IMatcher GetExpectationMatcher()
      {
         return matcher;
      }

      public override void HandleDisposition(uint frameSize, Disposition disposition, Span<byte> payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleDisposition(frameSize, disposition, payload, channel, context);

         SessionTracker session = driver.Sessions.SessionFromRemoteChannel(channel);

         if (session == null)
         {
            throw new AssertionError(string.Format(
                "Received Disposition on channel [{0}] that has no matching Session for that remote channel. ", channel));
         }

         session.HandleDisposition(disposition);
      }

      public DispositionExpectation WithRole(Role role)
      {
         WithRole(Is.EqualTo(role));
         return this;
      }

      public DispositionExpectation WithFirst(uint first)
      {
         return WithFirst(Is.EqualTo(first));
      }

      public DispositionExpectation WithLast(uint last)
      {
         return WithLast(Is.EqualTo(last));
      }

      public DispositionExpectation WithSettled(bool settled)
      {
         return WithSettled(Is.EqualTo(settled));
      }

      public DispositionExpectation WithState(IDeliveryState state)
      {
         return WithState(Is.EqualTo(state));
      }

      public DispositionDeliveryStateBuilder WithState()
      {
         return stateBuilder;
      }

      public DispositionExpectation WithBatchable(bool batchable)
      {
         return WithBatchable(Is.EqualTo(batchable));
      }

      #region Matcher based With API

      public DispositionExpectation WithRole(IMatcher m)
      {
         matcher.WithRole(m);
         return this;
      }

      public DispositionExpectation WithFirst(IMatcher m)
      {
         matcher.WithFirst(m);
         return this;
      }

      public DispositionExpectation WithLast(IMatcher m)
      {
         matcher.WithLast(m);
         return this;
      }

      public DispositionExpectation WithSettled(IMatcher m)
      {
         matcher.WithSettled(m);
         return this;
      }

      public DispositionExpectation WithState(IMatcher m)
      {
         matcher.WithState(m);
         return this;
      }

      public DispositionExpectation WithBatchable(IMatcher m)
      {
         matcher.WithBatchable(m);
         return this;
      }

      #endregion
   }

   public sealed class DispositionDeliveryStateBuilder
   {
      private readonly DispositionExpectation expectation;

      internal DispositionDeliveryStateBuilder(DispositionExpectation expectation)
      {
         this.expectation = expectation;
      }

      public DispositionExpectation Accepted()
      {
         expectation.WithState(new Accepted());
         return expectation;
      }

      public DispositionExpectation Released()
      {
         expectation.WithState(new Released());
         return expectation;
      }

      public DispositionExpectation Rejected()
      {
         expectation.WithState(new Rejected());
         return expectation;
      }

      public DispositionExpectation Rejected(string condition, string description)
      {
         expectation.WithState(new Rejected(new ErrorCondition(new Symbol(condition), description)));
         return expectation;
      }

      public DispositionExpectation Modified()
      {
         expectation.WithState(new Modified());
         return expectation;
      }

      public DispositionExpectation Modified(bool failed)
      {
         expectation.WithState(new Modified());
         return expectation;
      }

      public DispositionExpectation Modified(bool failed, bool undeliverableHere)
      {
         expectation.WithState(new Modified());
         return expectation;
      }

      public DispositionExpectation Declared()
      {
         byte[] txnId = new byte[4];

         Random rand = new Random(Environment.TickCount);

         rand.NextBytes(txnId);

         expectation.WithState(new Declared(txnId));
         return expectation;
      }

      public DispositionExpectation Declared(byte[] txnId)
      {
         expectation.WithState(new Declared(txnId));
         return expectation;
      }

      public DispositionTransactionalStateMatcher Transactional()
      {
         DispositionTransactionalStateMatcher matcher = new DispositionTransactionalStateMatcher(expectation);
         expectation.WithState(matcher);
         return matcher;
      }
   }

   public sealed class DispositionTransactionalStateMatcher : TransactionalStateMatcher
   {
      private readonly DispositionExpectation expectation;

      internal DispositionTransactionalStateMatcher(DispositionExpectation expectation)
      {
         this.expectation = expectation;
      }

      public DispositionExpectation Also()
      {
         return expectation;
      }

      public DispositionExpectation And()
      {
         return expectation;
      }

      public override DispositionTransactionalStateMatcher WithTxnId(byte[] txnId)
      {
         base.WithTxnId(Is.EqualTo(new Binary(txnId)));
         return this;
      }

      public override DispositionTransactionalStateMatcher WithTxnId(Binary txnId)
      {
         base.WithTxnId(Is.EqualTo(txnId));
         return this;
      }

      public override DispositionTransactionalStateMatcher WithOutcome(IOutcome outcome)
      {
         base.WithOutcome(Is.EqualTo(outcome));
         return this;
      }

      #region Matcher based With API

      public override DispositionTransactionalStateMatcher WithTxnId(IMatcher m)
      {
         base.WithOutcome(m);
         return this;
      }

      public override DispositionTransactionalStateMatcher WithOutcome(IMatcher m)
      {
         base.WithOutcome(m);
         return this;
      }

      #endregion

      #region Layer to allow configuring the outcome Without specific type dependencies

      public DispositionTransactionalStateMatcher WithAccepted()
      {
         base.WithOutcome(new Accepted());
         return this;
      }

      public DispositionTransactionalStateMatcher WithReleased()
      {
         base.WithOutcome(new Released());
         return this;
      }

      public DispositionTransactionalStateMatcher WithRejected()
      {
         base.WithOutcome(new Rejected());
         return this;
      }

      public DispositionTransactionalStateMatcher WithRejected(String condition, String description)
      {
         base.WithOutcome(new Rejected(new ErrorCondition(new Symbol(condition), description)));
         return this;
      }

      public DispositionTransactionalStateMatcher WithModified()
      {
         base.WithOutcome(new Modified());
         return this;
      }

      public DispositionTransactionalStateMatcher WithModified(bool failed)
      {
         base.WithOutcome(new Modified(failed));
         return this;
      }

      public DispositionTransactionalStateMatcher WithModified(bool failed, bool undeliverableHere)
      {
         base.WithOutcome(new Modified(failed, undeliverableHere));
         return this;
      }

      #endregion
   }
}