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
   public sealed class DeclareMatcher : ListDescribedTypeMatcher
   {
      public DeclareMatcher() : base(Enum.GetNames(typeof(DeclareField)).Length, Declare.DESCRIPTOR_CODE, Declare.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Declare);

      #region Matcher based with API

      protected override bool MatchesSafely(ListDescribedType item)
      {
         return base.MatchesSafely(item);
      }

      #endregion
   }
}