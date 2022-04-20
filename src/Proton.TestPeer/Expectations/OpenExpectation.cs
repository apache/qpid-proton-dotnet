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
   public sealed class OpenExpectation : AbstractExpectation<Open>
   {
      private readonly OpenMatcher matcher = new OpenMatcher();

      private OpenInjectAction response;
      private bool explicitlyNullContainerId;

      public OpenExpectation(AMQPTestDriver driver) : base(driver)
      {
      }

      public OpenInjectAction Respond()
      {
         response = new OpenInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      public CloseInjectAction Reject()
      {
         response = new OpenInjectAction(driver);
         driver.AddScriptedElement(response);

         CloseInjectAction closeAction = new CloseInjectAction(driver);
         driver.AddScriptedElement(closeAction);

         return closeAction;
      }

      public CloseInjectAction Reject(string condition, string description)
      {
         return Reject(new Symbol(condition), description);
      }

      public CloseInjectAction Reject(string condition, string description, IDictionary<string, object> infoMap)
      {
         return Reject(new Symbol(condition), description, TypeMapper.ToSymbolKeyedMap(infoMap));
      }

      public CloseInjectAction Reject(Symbol condition, string description)
      {
         return Reject(condition, description, null);
      }

      public CloseInjectAction Reject(Symbol condition, string description, IDictionary<Symbol, object> infoMap)
      {
         response = new OpenInjectAction(driver);
         driver.AddScriptedElement(response);

         CloseInjectAction closeAction = new CloseInjectAction(driver).WithErrorCondition(condition, description, infoMap);
         driver.AddScriptedElement(closeAction);

         return closeAction;
      }

      public override void HandleOpen(uint frameSize, Open open, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleOpen(frameSize, open, payload, channel, context);

         if (response != null)
         {
            // Input was validated now populate response With auto values where not configured
            // to say otherwise by the test.
            if (response.Performative.ContainerId == null && !explicitlyNullContainerId)
            {
               response.Performative.ContainerId = "driver";
            }

            if (response.OnChannel() == null)
            {
               response.OnChannel(channel);
            }
         }
      }

      public OpenExpectation WithContainerId(string container)
      {
         explicitlyNullContainerId = container == null;
         return WithContainerId(Is.EqualTo(container));
      }

      public OpenExpectation WithHostname(string hostname)
      {
         return WithHostname(Is.EqualTo(hostname));
      }

      public OpenExpectation WithMaxFrameSize(uint maxFrameSize)
      {
         return WithMaxFrameSize(Is.EqualTo(maxFrameSize));
      }

      public OpenExpectation WithChannelMax(ushort channelMax)
      {
         return WithChannelMax(Is.EqualTo(channelMax));
      }

      public OpenExpectation WithIdleTimeOut(uint idleTimeout)
      {
         return WithIdleTimeOut(Is.EqualTo(idleTimeout));
      }

      public OpenExpectation WithOutgoingLocales(params string[] outgoingLocales)
      {
         return WithOutgoingLocales(Is.EqualTo(TypeMapper.ToSymbolArray(outgoingLocales)));
      }

      public OpenExpectation WithOutgoingLocales(params Symbol[] outgoingLocales)
      {
         return WithOutgoingLocales(Is.EqualTo(outgoingLocales));
      }

      public OpenExpectation WithIncomingLocales(params string[] incomingLocales)
      {
         return WithIncomingLocales(Is.EqualTo(TypeMapper.ToSymbolArray(incomingLocales)));
      }

      public OpenExpectation WithIncomingLocales(params Symbol[] incomingLocales)
      {
         return WithIncomingLocales(Is.EqualTo(incomingLocales));
      }

      public OpenExpectation WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(offeredCapabilities)));
      }

      public OpenExpectation WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(offeredCapabilities));
      }

      public OpenExpectation WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(desiredCapabilities)));
      }

      public OpenExpectation WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(desiredCapabilities));
      }

      public OpenExpectation WithPropertiesMap(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      public OpenExpectation WithProperties(IDictionary<string, object> properties)
      {
         return WithProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      #region Matcher based With API

      public OpenExpectation WithContainerId(IMatcher m)
      {
         matcher.WithContainerId(m);
         return this;
      }

      public OpenExpectation WithHostname(IMatcher m)
      {
         matcher.WithHostname(m);
         return this;
      }

      public OpenExpectation WithMaxFrameSize(IMatcher m)
      {
         matcher.WithMaxFrameSize(m);
         return this;
      }

      public OpenExpectation WithChannelMax(IMatcher m)
      {
         matcher.WithChannelMax(m);
         return this;
      }

      public OpenExpectation WithIdleTimeOut(IMatcher m)
      {
         matcher.WithIdleTimeOut(m);
         return this;
      }

      public OpenExpectation WithOutgoingLocales(IMatcher m)
      {
         matcher.WithOutgoingLocales(m);
         return this;
      }

      public OpenExpectation WithIncomingLocales(IMatcher m)
      {
         matcher.WithIncomingLocales(m);
         return this;
      }

      public OpenExpectation WithOfferedCapabilities(IMatcher m)
      {
         matcher.WithOfferedCapabilities(m);
         return this;
      }

      public OpenExpectation WithDesiredCapabilities(IMatcher m)
      {
         matcher.WithDesiredCapabilities(m);
         return this;
      }

      public OpenExpectation WithProperties(IMatcher m)
      {
         matcher.WithProperties(m);
         return this;
      }

      #endregion
   }
}