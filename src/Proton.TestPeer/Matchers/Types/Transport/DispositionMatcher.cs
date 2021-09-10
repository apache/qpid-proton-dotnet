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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class DispositionMatcher : ListDescribedTypeMatcher
   {
      public DispositionMatcher() : base(Enum.GetNames(typeof(DispositionField)).Length, Disposition.DESCRIPTOR_CODE, Disposition.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Disposition);

      public DispositionMatcher WithRole(bool role)
      {
         return WithRole(Is.EqualTo(role ? Role.Receiver : Role.Sender));
      }

      public DispositionMatcher WithRole(Role role)
      {
         return WithRole(Is.EqualTo(role));
      }

      public DispositionMatcher WithFirst(uint first)
      {
         return WithFirst(Is.EqualTo(first));
      }

      public DispositionMatcher WithLast(uint last)
      {
         return WithLast(Is.EqualTo(last));
      }

      public DispositionMatcher WithSettled(bool settled)
      {
         return WithSettled(Is.EqualTo(settled));
      }

      public DispositionMatcher WithState(IDeliveryState state)
      {
         return WithState(Is.EqualTo(state));
      }

      public DispositionMatcher WithBatchable(bool batchable)
      {
         return WithBatchable(Is.EqualTo(batchable));
      }

      #region Matcher based With API

      public DispositionMatcher WithRole(IMatcher m)
      {
         AddFieldMatcher((int)DispositionField.Role, m);
         return this;
      }

      public DispositionMatcher WithFirst(IMatcher m)
      {
         AddFieldMatcher((int)DispositionField.First, m);
         return this;
      }

      public DispositionMatcher WithLast(IMatcher m)
      {
         AddFieldMatcher((int)DispositionField.Last, m);
         return this;
      }

      public DispositionMatcher WithSettled(IMatcher m)
      {
         AddFieldMatcher((int)DispositionField.Settled, m);
         return this;
      }

      public DispositionMatcher WithState(IMatcher m)
      {
         AddFieldMatcher((int)DispositionField.State, m);
         return this;
      }

      public DispositionMatcher WithBatchable(IMatcher m)
      {
         AddFieldMatcher((int)DispositionField.Batchable, m);
         return this;
      }

      #endregion
   }
}