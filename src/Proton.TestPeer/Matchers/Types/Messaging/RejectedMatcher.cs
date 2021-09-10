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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public sealed class RejectedMatcher : ListDescribedTypeMatcher
   {
      public RejectedMatcher() : base(Enum.GetNames(typeof(RejectedField)).Length, Rejected.DESCRIPTOR_CODE, Rejected.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Rejected);

      public RejectedMatcher WithError(ErrorCondition error)
      {
         return WithError(Is.EqualTo(error));
      }

      public RejectedMatcher WithError(string condition)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition));
      }

      public RejectedMatcher WithError(Symbol condition)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition));
      }

      public RejectedMatcher WithError(string condition, string description)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(description));
      }

      public RejectedMatcher WithError(string condition, string description, IDictionary<string, object> info)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(description).WithInfo(info));
      }

      public RejectedMatcher WithError(Symbol condition, string description, IDictionary<Symbol, object> info)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(description).WithInfoMap(info));
      }

      #region Matcher based expectations

      public RejectedMatcher WithError(IMatcher m)
      {
         AddFieldMatcher((int)RejectedField.Error, m);
         return this;
      }

      #endregion
   }
}