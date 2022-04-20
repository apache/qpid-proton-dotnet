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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Security
{
   public sealed class SaslInitMatcher : ListDescribedTypeMatcher
   {
      public SaslInitMatcher() : base(Enum.GetNames(typeof(SaslInitField)).Length, SaslInit.DESCRIPTOR_CODE, SaslInit.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(SaslInit);

      public SaslInitMatcher WithMechanism(string mechanism)
      {
         return WithMechanism(Is.EqualTo(new Symbol(mechanism)));
      }

      public SaslInitMatcher WithMechanism(Symbol mechanism)
      {
         return WithMechanism(Is.EqualTo(mechanism));
      }

      public SaslInitMatcher WithInitialResponse(byte[] initialResponse)
      {
         return WithInitialResponse(Is.EqualTo(new Binary(initialResponse)));
      }

      public SaslInitMatcher WithInitialResponse(Binary initialResponse)
      {
         return WithInitialResponse(Is.EqualTo(initialResponse));
      }

      public SaslInitMatcher WithHostname(string hostname)
      {
         return WithHostname(Is.EqualTo(hostname));
      }

      #region Matcher based With API

      public SaslInitMatcher WithMechanism(IMatcher m)
      {
         AddFieldMatcher((int)SaslInitField.Mechanism, m);
         return this;
      }

      public SaslInitMatcher WithInitialResponse(IMatcher m)
      {
         AddFieldMatcher((int)SaslInitField.InitialResponse, m);
         return this;
      }

      public SaslInitMatcher WithHostname(IMatcher m)
      {
         AddFieldMatcher((int)SaslInitField.Hostname, m);
         return this;
      }

      #endregion
   }
}