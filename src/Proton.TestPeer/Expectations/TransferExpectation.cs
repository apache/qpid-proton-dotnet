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
   public class TransferExpectation : AbstractExpectation<Transfer>
   {
      private readonly TransferMatcher matcher = new TransferMatcher();
      private readonly TransferDeliveryStateBuilder stateBuilder;

      private IMatcher payloadMatcher = Matches.Any(typeof(byte[]));

      private DispositionInjectAction response;

      public TransferExpectation(AMQPTestDriver driver) : base(driver)
      {
         WithHandle(Is.NotNullValue());

         this.stateBuilder = new TransferDeliveryStateBuilder(this);
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public DispositionInjectAction Respond()
      {
         response = new DispositionInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      public DispositionInjectAction Accept()
      {
         response = new DispositionInjectAction(driver);
         response.WithSettled(true);
         response.WithState(new Accepted());

         driver.AddScriptedElement(response);
         return response;
      }

      public DispositionInjectAction Release()
      {
         response = new DispositionInjectAction(driver);
         response.WithSettled(true);
         response.WithState(new Released());

         driver.AddScriptedElement(response);
         return response;
      }

      public DispositionInjectAction Reject()
      {
         return Reject(null);
      }

      public DispositionInjectAction Reject(string condition, string description)
      {
         return Reject(new ErrorCondition(new Symbol(condition), description));
      }

      public DispositionInjectAction Reject(Symbol condition, string description)
      {
         return Reject(new ErrorCondition(condition, description));
      }

      public DispositionInjectAction Reject(ErrorCondition error)
      {
         response = new DispositionInjectAction(driver);
         response.WithSettled(true);
         response.WithState(new Rejected(error));

         driver.AddScriptedElement(response);
         return response;
      }

      public DispositionInjectAction Modify(bool failed)
      {
         return Modify(failed, false);
      }

      public DispositionInjectAction Modify(bool failed, bool undeliverable)
      {
         response = new DispositionInjectAction(driver);
         response.WithSettled(true);
         response.WithState(new Modified(failed, undeliverable));

         driver.AddScriptedElement(response);
         return response;
      }

      public override TransferExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public override void HandleTransfer(uint frameSize, Transfer transfer, Span<byte> payload, ushort channel, AMQPTestDriver driver)
      {
         base.HandleTransfer(frameSize, transfer, payload, channel, driver);

         SessionTracker session = driver.Sessions.SessionFromRemoteChannel(channel);

         if (session == null)
         {
            throw new AssertionError(string.Format(
                "Received Transfer on channel [{0}] that has no matching Session for that remote channel. ", channel));
         }

         LinkTracker link = session.HandleTransfer(transfer, payload.ToArray());

         if (response != null)
         {
            // Input was validated now populate response With auto values where not configured
            // to say otherwise by the test.
            if (response.OnChannel() == null)
            {
               response.OnChannel((ushort)link.Session.LocalChannel);
            }

            // Populate the fields of the response With defaults if non set by the test script
            if (response.Performative.First == null)
            {
               response.WithFirst((uint)transfer.DeliveryId);
            }

            if (response.Performative.Role == null)
            {
               response.WithRole(link.Role);
            }

            // Remaining response fields should be set by the test script as they can't be inferred.
         }
      }

      public virtual TransferExpectation WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public virtual TransferExpectation WithDeliveryId(uint deliveryId)
      {
         return WithDeliveryId(Is.EqualTo(deliveryId));
      }

      public virtual TransferExpectation WithDeliveryTag(byte[] tag)
      {
         return WithDeliveryTag(new Binary(tag));
      }

      public virtual TransferExpectation WithDeliveryTag(Binary deliveryTag)
      {
         return WithDeliveryTag(Is.EqualTo(deliveryTag));
      }

      public virtual TransferExpectation WithNonNullDeliveryTag()
      {
         return WithDeliveryTag(Is.NotNullValue());
      }

      public virtual TransferExpectation WithNullDeliveryTag()
      {
         return WithDeliveryTag(Is.NullValue());
      }

      public virtual TransferExpectation WithMessageFormat(uint messageFormat)
      {
         return WithMessageFormat(Is.EqualTo(messageFormat));
      }

      public virtual TransferExpectation WithSettled(bool settled)
      {
         return WithSettled(Is.EqualTo(settled));
      }

      public virtual TransferExpectation WithMore(bool more)
      {
         return WithMore(Is.EqualTo(more));
      }

      public virtual TransferExpectation WithRcvSettleMode(ReceiverSettleMode rcvSettleMode)
      {
         return WithRcvSettleMode(Is.EqualTo(rcvSettleMode));
      }

      public virtual TransferExpectation WithState(IDeliveryState state)
      {
         return WithState(Is.EqualTo(state));
      }

      public virtual TransferDeliveryStateBuilder WithState()
      {
         return stateBuilder;
      }

      public virtual TransferExpectation WithNullState()
      {
         return WithState(Is.NullValue());
      }

      public virtual TransferExpectation WithResume(bool resume)
      {
         return WithResume(Is.EqualTo(resume));
      }

      public virtual TransferExpectation WithAborted(bool aborted)
      {
         return WithAborted(Is.EqualTo(aborted));
      }

      public virtual TransferExpectation WithBatchable(bool batchable)
      {
         return WithBatchable(Is.EqualTo(batchable));
      }

      public virtual TransferExpectation WithNonNullPayload()
      {
         this.payloadMatcher = Is.NotNullValue();
         return this;
      }

      public virtual TransferExpectation WithNullPayload()
      {
         this.payloadMatcher = Is.NullValue();
         return this;
      }

      public virtual TransferExpectation WithPayload(byte[] buffer)
      {
         // TODO - Create Matcher which describes the mismatch in detail
         this.payloadMatcher = Matchers.Is.EqualTo(buffer);
         return this;
      }

      #region Matcher based With API

      public virtual TransferExpectation WithHandle(IMatcher m)
      {
         matcher.WithHandle(m);
         return this;
      }

      public virtual TransferExpectation WithDeliveryId(IMatcher m)
      {
         matcher.WithDeliveryId(m);
         return this;
      }

      public virtual TransferExpectation WithDeliveryTag(IMatcher m)
      {
         matcher.WithDeliveryTag(m);
         return this;
      }

      public virtual TransferExpectation WithMessageFormat(IMatcher m)
      {
         matcher.WithMessageFormat(m);
         return this;
      }

      public virtual TransferExpectation WithSettled(IMatcher m)
      {
         matcher.WithSettled(m);
         return this;
      }

      public virtual TransferExpectation WithMore(IMatcher m)
      {
         matcher.WithMore(m);
         return this;
      }

      public virtual TransferExpectation WithRcvSettleMode(IMatcher m)
      {
         matcher.WithRcvSettleMode(m);
         return this;
      }

      public virtual TransferExpectation WithState(IMatcher m)
      {
         matcher.WithState(m);
         return this;
      }

      public virtual TransferExpectation WithResume(IMatcher m)
      {
         matcher.WithResume(m);
         return this;
      }

      public virtual TransferExpectation WithAborted(IMatcher m)
      {
         matcher.WithAborted(m);
         return this;
      }

      public virtual TransferExpectation WithBatchable(IMatcher m)
      {
         matcher.WithBatchable(m);
         return this;
      }

      public virtual TransferExpectation WithPayload(IMatcher payloadMatcher)
      {
         this.payloadMatcher = payloadMatcher;
         return this;
      }

      #endregion
   }

   public sealed class TransferDeliveryStateBuilder
   {
      private TransferExpectation expectation;

      public TransferDeliveryStateBuilder(TransferExpectation expectation)
      {
         this.expectation = expectation;
      }

      public TransferExpectation Accepted()
      {
         expectation.WithState(new Accepted());
         return expectation;
      }

      public TransferExpectation Released()
      {
         expectation.WithState(new Released());
         return expectation;
      }

      public TransferExpectation Rejected()
      {
         expectation.WithState(new Rejected());
         return expectation;
      }

      public TransferExpectation Rejected(string condition, string description)
      {
         expectation.WithState(new Rejected(new ErrorCondition(new Symbol(condition), description)));
         return expectation;
      }

      public TransferExpectation Modified()
      {
         expectation.WithState(new Modified());
         return expectation;
      }

      public TransferExpectation Modified(bool failed)
      {
         expectation.WithState(new Modified());
         return expectation;
      }

      public TransferExpectation Modified(bool failed, bool undeliverableHere)
      {
         expectation.WithState(new Modified());
         return expectation;
      }

      public TransferTransactionalStateMatcher Transactional()
      {
         TransferTransactionalStateMatcher matcher = new TransferTransactionalStateMatcher(expectation);
         expectation.WithState(matcher);
         return matcher;
      }
   }

   public sealed class TransferTransactionalStateMatcher : TransactionalStateMatcher
   {
      private readonly TransferExpectation expectation;

      public TransferTransactionalStateMatcher(TransferExpectation expectation)
      {
         this.expectation = expectation;
      }

      public TransferExpectation Also()
      {
         return expectation;
      }

      public TransferExpectation And()
      {
         return expectation;
      }

      public override TransferTransactionalStateMatcher WithTxnId(byte[] txnId)
      {
         base.WithTxnId(Is.EqualTo(new Binary(txnId)));
         return this;
      }

      public override TransferTransactionalStateMatcher WithTxnId(Binary txnId)
      {
         base.WithTxnId(Is.EqualTo(txnId));
         return this;
      }

      public override TransferTransactionalStateMatcher WithOutcome(IOutcome outcome)
      {
         base.WithOutcome(Is.EqualTo(outcome));
         return this;
      }

      #region Matcher based With API

      public override TransferTransactionalStateMatcher WithTxnId(IMatcher m)
      {
         base.WithOutcome(m);
         return this;
      }

      public override TransferTransactionalStateMatcher WithOutcome(IMatcher m)
      {
         base.WithOutcome(m);
         return this;
      }

      #endregion

      public TransferTransactionalStateMatcher WithAccepted()
      {
         base.WithOutcome(new Accepted());
         return this;
      }

      public TransferTransactionalStateMatcher WithReleased()
      {
         base.WithOutcome(new Released());
         return this;
      }

      public TransferTransactionalStateMatcher WithRejected()
      {
         base.WithOutcome(new Rejected());
         return this;
      }

      public TransferTransactionalStateMatcher WithRejected(string condition, string description)
      {
         base.WithOutcome(new Rejected(new ErrorCondition(new Symbol(condition), description)));
         return this;
      }

      public TransferTransactionalStateMatcher WithModified()
      {
         base.WithOutcome(new Modified());
         return this;
      }

      public TransferTransactionalStateMatcher WithModified(bool failed)
      {
         base.WithOutcome(new Modified(failed));
         return this;
      }

      public TransferTransactionalStateMatcher WithModified(bool failed, bool undeliverableHere)
      {
         base.WithOutcome(new Modified(failed, undeliverableHere));
         return this;
      }
   }
}