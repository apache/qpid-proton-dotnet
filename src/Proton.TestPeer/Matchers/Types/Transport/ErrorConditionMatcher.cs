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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class ErrorConditionMatcher : ListDescribedTypeMatcher
   {
      public ErrorConditionMatcher() : base(Enum.GetNames(typeof(ErrorConditionField)).Length, ErrorCondition.DESCRIPTOR_CODE, ErrorCondition.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(ErrorCondition);

      protected override bool MatchesSafely(ListDescribedType item)
      {
         return base.MatchesSafely(item);
      }

      public ErrorConditionMatcher WithCondition(string condition)
      {
         return WithCondition(Is.EqualTo(new Symbol(condition)));
      }

      public ErrorConditionMatcher WithCondition(Symbol condition)
      {
         return WithCondition(Is.EqualTo(condition));
      }

      public ErrorConditionMatcher WithDescription(string description)
      {
         return WithDescription(Is.EqualTo(description));
      }

      public ErrorConditionMatcher WithInfo(IDictionary<Symbol, object> info)
      {
         return WithInfo(Is.EqualTo(info));
      }

      public ErrorConditionMatcher WithInfo(IDictionary<string, object> info)
      {
         return WithInfo(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(info)));
      }

      #region Matcher based expectations

      public ErrorConditionMatcher WithCondition(IMatcher m)
      {
         AddFieldMatcher((int)ErrorConditionField.Condition, m);
         return this;
      }

      public ErrorConditionMatcher WithDescription(IMatcher m)
      {
         AddFieldMatcher((int)ErrorConditionField.Description, m);
         return this;
      }

      public ErrorConditionMatcher WithInfo(IMatcher m)
      {
         AddFieldMatcher((int)ErrorConditionField.Info, m);
         return this;
      }

      #endregion
   }
}