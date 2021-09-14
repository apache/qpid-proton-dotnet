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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions
{
   public class TransactionalStateMatcher : ListDescribedTypeMatcher
   {
      public TransactionalStateMatcher() : base(Enum.GetNames(typeof(TransactionalStateField)).Length, TransactionalState.DESCRIPTOR_CODE, TransactionalState.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(TransactionalState);

      public virtual TransactionalStateMatcher WithTxnId(byte[] txnId)
      {
         return WithTxnId(Is.EqualTo(new Binary(txnId)));
      }

      public virtual TransactionalStateMatcher WithTxnId(Binary txnId)
      {
         return WithTxnId(Is.EqualTo(txnId));
      }

      public virtual TransactionalStateMatcher WithOutcome(IOutcome outcome)
      {
         return WithOutcome(Is.EqualTo(outcome));
      }

      #region Matcher based with API

      public virtual TransactionalStateMatcher WithTxnId(IMatcher m)
      {
         AddFieldMatcher((int)TransactionalStateField.TxnId, m);
         return this;
      }

      public virtual TransactionalStateMatcher WithOutcome(IMatcher m)
      {
         AddFieldMatcher((int)TransactionalStateField.Outcome, m);
         return this;
      }

      #endregion
   }
}