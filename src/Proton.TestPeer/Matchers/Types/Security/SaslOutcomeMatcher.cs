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
using Apache.Qpid.Proton.Test.Driver.Codec.Security;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Security
{
   public sealed class SaslOutcomeMatcher : ListDescribedTypeMatcher
   {
      public SaslOutcomeMatcher() : base(Enum.GetNames(typeof(SaslOutcomeField)).Length, SaslOutcome.DESCRIPTOR_CODE, SaslOutcome.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(SaslOutcome);

      public SaslOutcomeMatcher WithCode(byte code)
      {
         return WithCode(Is.EqualTo((SaslCode)code));
      }

      public SaslOutcomeMatcher WithCode(SaslCode code)
      {
         return WithCode(Is.EqualTo(code));
      }

      public SaslOutcomeMatcher WithAdditionalData(byte[] additionalData)
      {
         return WithAdditionalData(Is.EqualTo(new Binary(additionalData)));
      }

      public SaslOutcomeMatcher WithAdditionalData(Binary additionalData)
      {
         return WithAdditionalData(Is.EqualTo(additionalData));
      }

      #region Matcher based with API

      public SaslOutcomeMatcher WithCode(IMatcher m)
      {
         AddFieldMatcher((int)SaslOutcomeField.Code, m);
         return this;
      }

      public SaslOutcomeMatcher WithAdditionalData(IMatcher m)
      {
         AddFieldMatcher((int)SaslOutcomeField.AdditionalData, m);
         return this;
      }

      #endregion
   }
}