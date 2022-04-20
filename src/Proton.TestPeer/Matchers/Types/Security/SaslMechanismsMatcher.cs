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
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Security
{
   public sealed class SaslMechanismsMatcher : ListDescribedTypeMatcher
   {
      public SaslMechanismsMatcher() : base(Enum.GetNames(typeof(SaslMechanismsField)).Length, SaslMechanisms.DESCRIPTOR_CODE, SaslMechanisms.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(SaslMechanisms);

      public SaslMechanismsMatcher WithSaslServerMechanisms(params string[] mechanisms)
      {
         return WithSaslServerMechanisms(Is.EqualTo(TypeMapper.ToSymbolArray(mechanisms)));
      }

      public SaslMechanismsMatcher WithSaslServerMechanisms(params Symbol[] mechanisms)
      {
         return WithSaslServerMechanisms(Is.EqualTo(mechanisms));
      }

      #region Matcher based with API

      public SaslMechanismsMatcher WithSaslServerMechanisms(IMatcher m)
      {
         AddFieldMatcher((int)SaslMechanismsField.SaslServerMechanisms, m);
         return this;
      }

      #endregion
   }
}