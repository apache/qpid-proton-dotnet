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
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions
{
   public sealed class DischargeMatcher : ListDescribedTypeMatcher
   {
      public DischargeMatcher() : base(Enum.GetNames(typeof(DischargeField)).Length, Discharge.DESCRIPTOR_CODE, Discharge.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Discharge);

      public DischargeMatcher WithTxnId(byte[] txnId)
      {
         return WithTxnId(Is.EqualTo(new Binary(txnId)));
      }

      public DischargeMatcher WithTxnId(Binary txnId)
      {
         return WithTxnId(Is.EqualTo(txnId));
      }

      public DischargeMatcher WithFail(bool fail)
      {
         return WithFail(Is.EqualTo(fail));
      }

      #region Matcher based with API

      public DischargeMatcher WithTxnId(IMatcher m)
      {
         AddFieldMatcher((int)DischargeField.TxnId, m);
         return this;
      }

      public DischargeMatcher WithFail(IMatcher m)
      {
         AddFieldMatcher((int)DischargeField.Fail, m);
         return this;
      }

      #endregion
   }
}