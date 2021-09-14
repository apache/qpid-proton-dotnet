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
   public sealed class SaslResponseExpectation : AbstractExpectation<SaslResponse>
   {
      private readonly SaslResponseMatcher matcher = new SaslResponseMatcher();

      public SaslResponseExpectation(AMQPTestDriver driver) : base(driver)
      {
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public SaslResponseExpectation WithResponse(byte[] response)
      {
         return WithResponse(Is.EqualTo(new Binary(response)));
      }

      public SaslResponseExpectation WithResponse(Binary response)
      {
         return WithResponse(Is.EqualTo(response));
      }

      #region Matcher based With API

      public SaslResponseExpectation WithResponse(IMatcher m)
      {
         matcher.WithResponse(m);
         return this;
      }

      #endregion
   }
}