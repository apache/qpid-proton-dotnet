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

using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Matchers;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Expectation entry for AMQP Headers
   /// </summary>
   public sealed class AMQPHeaderExpectation : ScriptedExpectation
   {
      private readonly AMQPHeader expected;
      private readonly AMQPTestDriver driver;

      public AMQPHeaderExpectation(AMQPHeader expected, AMQPTestDriver driver)
      {
         this.expected = expected;
         this.driver = driver;
      }

      public AMQPHeaderInjectAction RespondWithAMQPHeader()
      {
         AMQPHeaderInjectAction response = new AMQPHeaderInjectAction(driver, AMQPHeader.Header);
         driver.AddScriptedElement(response);
         return response;
      }

      public AMQPHeaderInjectAction RespondWithSASLHeader()
      {
         AMQPHeaderInjectAction response = new AMQPHeaderInjectAction(driver, AMQPHeader.SASLHeader);
         driver.AddScriptedElement(response);
         return response;
      }

      public RawBytesInjectAction RespondWithBytes(byte[] buffer)
      {
         RawBytesInjectAction response = new RawBytesInjectAction(driver, buffer);
         driver.AddScriptedElement(response);
         return response;
      }

      public override void HandleAMQPHeader(AMQPHeader header, AMQPTestDriver context)
      {
         MatcherAssert.AssertThat("AMQP Header should match expected.", expected, Is.EqualTo(header));
      }

      public override void HandleSASLHeader(AMQPHeader header, AMQPTestDriver driver)
      {
         MatcherAssert.AssertThat("SASL Header should match expected.", expected, Is.EqualTo(header));
      }
   }
}