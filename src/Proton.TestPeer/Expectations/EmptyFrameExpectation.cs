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
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class EmptyFrameExpectation : AbstractExpectation<Heartbeat>
   {
      private readonly HeartbeatMatcher matcher = new HeartbeatMatcher();

      public EmptyFrameExpectation(AMQPTestDriver driver) : base(driver)
      {
         OnChannel(0);
      }

      public EmptyFrameExpectation OnChannel(int channel)
      {
         if (channel != 0) throw new ArgumentException("Empty Frames must arrive on channel zero");
         base.OnChannel(0);
         return this;
      }

      public EmptyFrameInjectAction Respond()
      {
         EmptyFrameInjectAction response = new EmptyFrameInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      protected override IMatcher GetExpectationMatcher()
      {
         return matcher;
      }
   }
}