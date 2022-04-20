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

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class DetachMatcher : ListDescribedTypeMatcher
   {
      public DetachMatcher() : base(Enum.GetNames(typeof(DetachField)).Length, Detach.DESCRIPTOR_CODE, Detach.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Detach);

      public DetachMatcher WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public DetachMatcher WithClosed(bool closed)
      {
         return WithClosed(Is.EqualTo(closed));
      }

      public DetachMatcher WithError(ErrorCondition error)
      {
         return WithError(Is.EqualTo(error));
      }

      public DetachMatcher WithError(string condition)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(Is.NullValue()));
      }

      public DetachMatcher WithError(string condition, string description)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(description));
      }

      public DetachMatcher WithError(string condition, string description, IDictionary<string, object> info)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(description).WithInfo(info));
      }

      public DetachMatcher WithError(Symbol condition, string description)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(description));
      }

      public DetachMatcher WithError(Symbol condition, string description, IDictionary<Symbol, object> info)
      {
         return WithError(new ErrorConditionMatcher().WithCondition(condition).WithDescription(description).WithInfo(info));
      }

      #region Matcher based With API

      public DetachMatcher WithHandle(IMatcher m)
      {
         AddFieldMatcher((int)DetachField.Handle, m);
         return this;
      }

      public DetachMatcher WithClosed(IMatcher m)
      {
         AddFieldMatcher((int)DetachField.Closed, m);
         return this;
      }

      public DetachMatcher WithError(IMatcher m)
      {
         AddFieldMatcher((int)DetachField.Error, m);
         return this;
      }

      #endregion
   }
}