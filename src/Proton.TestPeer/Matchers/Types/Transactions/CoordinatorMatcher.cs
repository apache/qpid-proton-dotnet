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
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions
{
   public class CoordinatorMatcher : ListDescribedTypeMatcher
   {
      public CoordinatorMatcher() : base(Enum.GetNames(typeof(CoordinatorField)).Length, Coordinator.DESCRIPTOR_CODE, Coordinator.DESCRIPTOR_SYMBOL)
      {
      }

      public CoordinatorMatcher(Coordinator coordinator) : this()
      {
         AddCoordinatorMatchers(coordinator);
      }

      protected override Type DescribedTypeClassType => typeof(Coordinator);

      public virtual CoordinatorMatcher WithCapabilities(params Symbol[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(capabilities));
      }

      public virtual CoordinatorMatcher WithCapabilities(params string[] capabilities)
      {
         return WithCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(capabilities)));
      }

      #region Matcher based with API

      public virtual CoordinatorMatcher WithCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)CoordinatorField.Capabilities, m);
         return this;
      }

      #endregion

      private void AddCoordinatorMatchers(Coordinator coordinator)
      {
         if (coordinator.Capabilities != null)
         {
            AddFieldMatcher((int)CoordinatorField.Capabilities, Is.EqualTo(coordinator.Capabilities));
         }
         else
         {
            AddFieldMatcher((int)CoordinatorField.Capabilities, Is.NullValue());
         }
      }
   }
}