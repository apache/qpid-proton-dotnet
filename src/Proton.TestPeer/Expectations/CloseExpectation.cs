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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class CloseExpectation : AbstractExpectation<Close>
   {
      private readonly CloseMatcher matcher = new CloseMatcher();

      private CloseInjectAction response;

      public CloseExpectation(AMQPTestDriver driver) : base(driver)
      {
      }

      public CloseInjectAction Respond()
      {
         response = new CloseInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public override void HandleClose(uint frameSize, Close close, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleClose(frameSize, close, payload, channel, context);

         if (response == null)
         {
            return;
         }

         // Input was validated now populate response With auto values where not configured
         // to say otherwise by the test.
         if (response.OnChannel() == null)
         {
            response.OnChannel(channel);
         }
      }

      public CloseExpectation WithError(ErrorCondition error)
      {
         return WithError(Is.EqualTo(error));
      }

      public CloseExpectation WithError(String condition, String description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description)));
      }

      public CloseExpectation WithError(String condition, String description, IDictionary<string, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description, TypeMapper.ToSymbolKeyedMap(info))));
      }

      public CloseExpectation WithError(Symbol condition, String description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description)));
      }

      public CloseExpectation WithError(Symbol condition, String description, IDictionary<Symbol, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description, info)));
      }

      #region Matcher API for explicit checks

      public CloseExpectation WithError(IMatcher m)
      {
         matcher.WithError(m);
         return this;
      }

      #endregion
   }
}