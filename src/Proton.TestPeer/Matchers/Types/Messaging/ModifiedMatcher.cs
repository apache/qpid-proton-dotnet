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
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public sealed class ModifiedMatcher : ListDescribedTypeMatcher
   {
      public ModifiedMatcher() : base(Enum.GetNames(typeof(ModifiedField)).Length, Modified.DESCRIPTOR_CODE, Modified.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Modified);

      public ModifiedMatcher WithDeliveryFailed(bool deliveryFailed)
      {
         return WithDeliveryFailed(Is.EqualTo(deliveryFailed));
      }

      public ModifiedMatcher WithUndeliverableHere(bool undeliverableHere)
      {
         return WithUndeliverableHere(Is.EqualTo(undeliverableHere));
      }

      public ModifiedMatcher WithMessageAnnotationsMap(IDictionary<Symbol, object> sectionNo)
      {
         return WithMessageAnnotations(Is.EqualTo(sectionNo));
      }

      public ModifiedMatcher WithMessageAnnotations(IDictionary<string, object> sectionNo)
      {
         return WithMessageAnnotations(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(sectionNo)));
      }

      #region Matcher based expectations

      public ModifiedMatcher WithDeliveryFailed(IMatcher m)
      {
         AddFieldMatcher((int)ModifiedField.DELIVERY_FAILED, m);
         return this;
      }

      public ModifiedMatcher WithUndeliverableHere(IMatcher m)
      {
         AddFieldMatcher((int)ModifiedField.UNDELIVERABLE_HERE, m);
         return this;
      }

      public ModifiedMatcher WithMessageAnnotations(IMatcher m)
      {
         AddFieldMatcher((int)ModifiedField.MESSAGE_ANNOTATIONS, m);
         return this;
      }

      #endregion
   }
}