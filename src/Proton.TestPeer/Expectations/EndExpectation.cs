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
   public sealed class EndExpectation : AbstractExpectation<End>
   {
      private readonly EndMatcher matcher = new();

      private EndInjectAction response;

      public EndExpectation(AMQPTestDriver driver) : base(driver)
      {
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public override EndExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public EndInjectAction Respond()
      {
         response = new EndInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      public override void HandleEnd(uint frameSize, End end, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleEnd(frameSize, end, payload, channel, context);

         // Ensure that local session tracking knows that remote ended a Session.
         SessionTracker session = context.Sessions.HandleEnd(end, channel);

         if (response != null)
         {
            if (response.OnChannel() == null)
            {
               response.OnChannel((ushort)session.LocalChannel);
            }
         }
      }

      public EndExpectation WithError(ErrorCondition error)
      {
         return WithError(Is.EqualTo(error));
      }

      public EndExpectation WithError(string condition, string description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description)));
      }

      public EndExpectation WithError(string condition, string description, IDictionary<string, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description, TypeMapper.ToSymbolKeyedMap(info))));
      }

      public EndExpectation WithError(Symbol condition, string description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description)));
      }

      public EndExpectation WithError(Symbol condition, string description, IDictionary<Symbol, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description, info)));
      }

      #region Matcher based With API

      public EndExpectation WithError(IMatcher m)
      {
         matcher.WithError(m);
         return this;
      }

      #endregion
   }
}