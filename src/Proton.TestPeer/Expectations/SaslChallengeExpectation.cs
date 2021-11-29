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
   public sealed class SaslChallengeExpectation : AbstractExpectation<SaslChallenge>
   {
      private readonly SaslChallengeMatcher matcher = new SaslChallengeMatcher();

      public SaslChallengeExpectation(AMQPTestDriver driver) : base(driver)
      {
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public SaslChallengeExpectation WithChallenge(byte[] challenge)
      {
         return WithChallenge(Is.EqualTo(new Binary(challenge)));
      }

      public SaslChallengeExpectation WithChallenge(Binary challenge)
      {
         return WithChallenge(Is.EqualTo(challenge));
      }

      #region Matcher based With API

      public SaslChallengeExpectation WithChallenge(IMatcher m)
      {
         matcher.WithChallenge(m);
         return this;
      }

      #endregion
   }
}