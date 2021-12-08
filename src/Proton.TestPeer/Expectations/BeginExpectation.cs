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
   public sealed class BeginExpectation : AbstractExpectation<Begin>
   {
      private readonly BeginMatcher matcher = new BeginMatcher();

      private BeginInjectAction response;

      public BeginExpectation(AMQPTestDriver driver) : base(driver)
      {
         // Configure default expectations for a valid Attach
         WithRemoteChannel(Apache.Qpid.Proton.Test.Driver.Matchers.Matches.AnyOf(Is.NullValue(), Is.NotNullValue()));
         WithNextOutgoingId(Is.NotNullValue());
         WithIncomingWindow(Is.NotNullValue());
         WithOutgoingWindow(Is.NotNullValue());
      }

      public override BeginExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public override BeginExpectation Optional()
      {
         base.Optional();
         return this;
      }

      public BeginInjectAction Respond()
      {
         response = new BeginInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      public EndInjectAction Reject(string condition, string description)
      {
         return Reject(new Symbol(condition), description);
      }

      public EndInjectAction Reject(Symbol condition, string description)
      {
         response = new BeginInjectAction(driver);
         driver.AddScriptedElement(response);

         EndInjectAction endAction = new EndInjectAction(driver).WithErrorCondition(condition, description);
         driver.AddScriptedElement(endAction);

         return endAction;
      }

      public BeginExpectation WithRemoteChannel(ushort remoteChannel)
      {
         return WithRemoteChannel(Is.EqualTo(remoteChannel));
      }

      public BeginExpectation WithNextOutgoingId(uint nextOutgoingId)
      {
         return WithNextOutgoingId(Is.EqualTo(nextOutgoingId));
      }

      public BeginExpectation WithIncomingWindow(uint incomingWindow)
      {
         return WithIncomingWindow(Is.EqualTo(incomingWindow));
      }

      public BeginExpectation WithOutgoingWindow(uint outgoingWindow)
      {
         return WithOutgoingWindow(Is.EqualTo(outgoingWindow));
      }

      public BeginExpectation WithHandleMax(uint handleMax)
      {
         return WithHandleMax(Is.EqualTo(handleMax));
      }

      public BeginExpectation WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(offeredCapabilities)));
      }

      public BeginExpectation WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(offeredCapabilities));
      }

      public BeginExpectation WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(desiredCapabilities)));
      }

      public BeginExpectation WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(desiredCapabilities));
      }

      public BeginExpectation WithProperties(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      public BeginExpectation WithProperties(IDictionary<string, object> properties)
      {
         return WithProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      #region Matcher API for explicit matching

      public BeginExpectation WithRemoteChannel(IMatcher m)
      {
         matcher.WithRemoteChannel(m);
         return this;
      }

      public BeginExpectation WithNextOutgoingId(IMatcher m)
      {
         matcher.WithNextOutgoingId(m);
         return this;
      }

      public BeginExpectation WithIncomingWindow(IMatcher m)
      {
         matcher.WithIncomingWindow(m);
         return this;
      }

      public BeginExpectation WithOutgoingWindow(IMatcher m)
      {
         matcher.WithOutgoingWindow(m);
         return this;
      }

      public BeginExpectation WithHandleMax(IMatcher m)
      {
         matcher.WithHandleMax(m);
         return this;
      }

      public BeginExpectation WithOfferedCapabilities(IMatcher m)
      {
         matcher.WithOfferedCapabilities(m);
         return this;
      }

      public BeginExpectation WithDesiredCapabilities(IMatcher m)
      {
         matcher.WithDesiredCapabilities(m);
         return this;
      }

      public BeginExpectation WithProperties(IMatcher m)
      {
         matcher.WithProperties(m);
         return this;
      }

      #endregion

      public override void HandleBegin(uint frameSize, Begin begin, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleBegin(frameSize, begin, payload, channel, context);

         context.Sessions.HandleBegin(begin, channel);

         if (response != null)
         {
            response.WithRemoteChannel(channel);
         }
      }

      protected override IMatcher GetExpectationMatcher()
      {
         return matcher;
      }
   }
}