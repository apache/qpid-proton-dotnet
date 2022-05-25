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
   public sealed class DischargeExpectation : TransferExpectation
   {
      private readonly DischargeMatcher discharge = new();

      public DischargeExpectation(AMQPTestDriver driver) : base(driver)
      {
         WithPayload(new AmqpValueMatcher(discharge));
      }

      public override void HandleTransfer(uint frameSize, Transfer transfer, byte[] payload, ushort channel, AMQPTestDriver driver)
      {
         base.HandleTransfer(frameSize, transfer, payload, channel, driver);
      }

      public override DischargeExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public DischargeExpectation WithFail(bool fail)
      {
         discharge.WithFail(fail);
         return this;
      }

      public DischargeExpectation WithTxnId(byte[] txnId)
      {
         discharge.WithTxnId(new Binary(txnId));
         return this;
      }

      public DischargeExpectation WithTxnId(Binary txnId)
      {
         discharge.WithTxnId(txnId);
         return this;
      }

      public DischargeExpectation WithDischarge(Discharge discharge)
      {
         WithPayload(new AmqpValueMatcher(discharge));
         return this;
      }

      public DischargeExpectation WithNullDischarge()
      {
         WithPayload(new AmqpValueMatcher(null));
         return this;
      }

      #region Type specific With API that performs simple equals checks

      public override DischargeExpectation WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public override DischargeExpectation WithDeliveryId(uint deliveryId)
      {
         return WithDeliveryId(Is.EqualTo(deliveryId));
      }

      public override DischargeExpectation WithDeliveryTag(byte[] tag)
      {
         return WithDeliveryTag(new Binary(tag));
      }

      public override DischargeExpectation WithDeliveryTag(Binary deliveryTag)
      {
         return WithDeliveryTag(Is.EqualTo(deliveryTag));
      }

      public override DischargeExpectation WithMessageFormat(uint messageFormat)
      {
         return WithMessageFormat(Is.EqualTo(messageFormat));
      }

      public override DischargeExpectation WithSettled(bool settled)
      {
         return WithSettled(Is.EqualTo(settled));
      }

      public override DischargeExpectation WithMore(bool more)
      {
         return WithMore(Is.EqualTo(more));
      }

      public override DischargeExpectation WithRcvSettleMode(ReceiverSettleMode rcvSettleMode)
      {
         return WithRcvSettleMode(Is.EqualTo(rcvSettleMode));
      }

      public override DischargeExpectation WithState(IDeliveryState state)
      {
         return WithState(Is.EqualTo(state));
      }

      public override DischargeExpectation WithNullState()
      {
         return WithState(Is.NullValue());
      }

      public override DischargeExpectation WithResume(bool resume)
      {
         return WithResume(Is.EqualTo(resume));
      }

      public override DischargeExpectation WithAborted(bool aborted)
      {
         return WithAborted(Is.EqualTo(aborted));
      }

      public override DischargeExpectation WithBatchable(bool batchable)
      {
         return WithBatchable(Is.EqualTo(batchable));
      }

      #endregion

      #region Matcher based API overrides

      public override DischargeExpectation WithHandle(IMatcher m)
      {
         base.WithHandle(m);
         return this;
      }

      public override DischargeExpectation WithDeliveryId(IMatcher m)
      {
         base.WithDeliveryId(m);
         return this;
      }

      public override DischargeExpectation WithDeliveryTag(IMatcher m)
      {
         base.WithDeliveryTag(m);
         return this;
      }

      public override DischargeExpectation WithMessageFormat(IMatcher m)
      {
         base.WithMessageFormat(m);
         return this;
      }

      public override DischargeExpectation WithSettled(IMatcher m)
      {
         base.WithSettled(m);
         return this;
      }

      public override DischargeExpectation WithMore(IMatcher m)
      {
         base.WithMore(m);
         return this;
      }

      public override DischargeExpectation WithRcvSettleMode(IMatcher m)
      {
         base.WithRcvSettleMode(m);
         return this;
      }

      public override DischargeExpectation WithState(IMatcher m)
      {
         base.WithState(m);
         return this;
      }

      public override DischargeExpectation WithResume(IMatcher m)
      {
         base.WithResume(m);
         return this;
      }

      public override DischargeExpectation WithAborted(IMatcher m)
      {
         base.WithAborted(m);
         return this;
      }

      public override DischargeExpectation WithBatchable(IMatcher m)
      {
         base.WithBatchable(m);
         return this;
      }

      #endregion
   }
}