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

using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Security;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class SaslInitExpectation : AbstractExpectation<SaslInit>
   {
      private readonly SaslInitMatcher matcher = new();

      public SaslInitExpectation(AMQPTestDriver driver) : base(driver)
      {
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public SaslInitExpectation WithMechanism(string mechanism)
      {
         return WithMechanism(Is.EqualTo(new Symbol(mechanism)));
      }

      public SaslInitExpectation WithMechanism(Symbol mechanism)
      {
         return WithMechanism(Is.EqualTo(mechanism));
      }

      public SaslInitExpectation WithInitialResponse(byte[] initialResponse)
      {
         return WithInitialResponse(Is.EqualTo(new Binary(initialResponse)));
      }

      public SaslInitExpectation WithInitialResponse(Binary initialResponse)
      {
         return WithInitialResponse(Is.EqualTo(initialResponse));
      }

      public SaslInitExpectation WithHostname(string hostname)
      {
         return WithHostname(Is.EqualTo(hostname));
      }

      #region Matcher based With API

      public SaslInitExpectation WithMechanism(IMatcher m)
      {
         matcher.WithMechanism(m);
         return this;
      }

      public SaslInitExpectation WithInitialResponse(IMatcher m)
      {
         matcher.WithInitialResponse(m);
         return this;
      }

      public SaslInitExpectation WithHostname(IMatcher m)
      {
         matcher.WithHostname(m);
         return this;
      }

      #endregion
   }
}