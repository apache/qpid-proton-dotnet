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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class TransferMatcher : ListDescribedTypeMatcher
   {
      public TransferMatcher() : base(Enum.GetNames(typeof(TransferField)).Length, Transfer.DESCRIPTOR_CODE, Transfer.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Transfer);

      public TransferMatcher WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public TransferMatcher WithDeliveryId(uint deliveryId)
      {
         return WithDeliveryId(Is.EqualTo(deliveryId));
      }

      public TransferMatcher WithDeliveryTag(byte[] tag)
      {
         return WithDeliveryTag(new Binary(tag));
      }

      public TransferMatcher WithDeliveryTag(Binary deliveryTag)
      {
         return WithDeliveryTag(Is.EqualTo(deliveryTag));
      }

      public TransferMatcher WithMessageFormat(uint messageFormat)
      {
         return WithMessageFormat(Is.EqualTo(messageFormat));
      }

      public TransferMatcher WithSettled(bool settled)
      {
         return WithSettled(Is.EqualTo(settled));
      }

      public TransferMatcher WithMore(bool more)
      {
         return WithMore(Is.EqualTo(more));
      }

      public TransferMatcher WithRcvSettleMode(ReceiverSettleMode rcvSettleMode)
      {
         return WithRcvSettleMode(Is.EqualTo(rcvSettleMode));
      }

      public TransferMatcher WithState(IDeliveryState state)
      {
         return WithState(Is.EqualTo(state));
      }

      public TransferMatcher WithNullState()
      {
         return WithState(Is.NullValue());
      }

      public TransferMatcher WithResume(bool resume)
      {
         return WithResume(Is.EqualTo(resume));
      }

      public TransferMatcher WithAborted(bool aborted)
      {
         return WithAborted(Is.EqualTo(aborted));
      }

      public TransferMatcher WithBatchable(bool batchable)
      {
         return WithBatchable(Is.EqualTo(batchable));
      }

      #region Matcher based With API

      public TransferMatcher WithHandle(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.Handle, m);
         return this;
      }

      public TransferMatcher WithDeliveryId(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.DeliveryId, m);
         return this;
      }

      public TransferMatcher WithDeliveryTag(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.DeliveryTag, m);
         return this;
      }

      public TransferMatcher WithMessageFormat(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.MessageFormat, m);
         return this;
      }

      public TransferMatcher WithSettled(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.Settled, m);
         return this;
      }

      public TransferMatcher WithMore(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.More, m);
         return this;
      }

      public TransferMatcher WithRcvSettleMode(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.ReceiverSettleMode, m);
         return this;
      }

      public TransferMatcher WithState(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.State, m);
         return this;
      }

      public TransferMatcher WithResume(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.Resume, m);
         return this;
      }

      public TransferMatcher WithAborted(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.Aborted, m);
         return this;
      }

      public TransferMatcher WithBatchable(IMatcher m)
      {
         AddFieldMatcher((int)TransferField.Batchable, m);
         return this;
      }

      #endregion
   }
}