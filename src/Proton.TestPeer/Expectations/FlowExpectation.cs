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
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class FlowExpectation : AbstractExpectation<Flow>
   {
      private readonly FlowMatcher matcher = new FlowMatcher();

      private FlowInjectAction response;

      public FlowExpectation(AMQPTestDriver driver) : base(driver)
      {
         WithNextIncomingId(Matches.AnyOf(Is.NullValue(), Is.NotNullValue()));
         WithIncomingWindow(Is.NotNullValue());
         WithNextOutgoingId(Is.NotNullValue());
         WithOutgoingWindow(Is.NotNullValue());
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public override FlowExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public FlowInjectAction Respond()
      {
         response = new FlowInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      public override void HandleFlow(uint frameSize, Flow flow, byte[] payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleFlow(frameSize, flow, payload, channel, context);

         SessionTracker session = driver.Sessions.SessionFromRemoteChannel(channel);

         if (session == null)
         {
            throw new AssertionError(string.Format(
                "Received Flow on channel [{0}] that has no matching Session for that remote channel. ", channel));
         }

         LinkTracker linkTracker = session.HandleFlow(flow);  // Can be null if Flow was session level only.

         if (response != null)
         {
            // Input was validated now populate response With auto values where not configured
            // to say otherwise by the test.
            if (response.OnChannel() == null)
            {
               response.OnChannel(session.LocalChannel ?? 0);
            }

            // TODO: The auto response values need to be pulled from session activity to produce meaningful auto
            //       generated values for scripted responses.

            // Populate the fields of the response With defaults if non set by the test script
            if (response.Performative.NextIncomingId == null)
            {
               response.WithNextIncomingId((uint)flow.NextOutgoingId); //TODO: this could be wrong, need to know about the transfers received (and sent by peer).
            }

            if (response.Performative.IncomingWindow == null)
            {
               response.WithIncomingWindow(int.MaxValue); //TODO: shouldn't be hard coded
            }

            if (response.Performative.NextOutgoingId == null)
            {
               response.WithNextOutgoingId((uint)flow.NextIncomingId); //TODO: this could be wrong, need to know about the transfers sent (and received at recipient peer).
            }

            if (response.Performative.OutgoingWindow == null)
            {
               response.WithOutgoingWindow(0); //TODO: shouldn't be hard coded, session might have senders on it as well as receivers
            }

            if (response.Performative.Handle == null && linkTracker != null)
            {
               response.WithHandle((uint)linkTracker.Handle);
            }

            // TODO: blow up on response if credit not populated?

            // Other fields are left not set for now unless test script configured
         }
      }

      public FlowExpectation WithNextIncomingId(uint nextIncomingId)
      {
         return WithNextIncomingId(Is.EqualTo(nextIncomingId));
      }

      public FlowExpectation WithIncomingWindow(uint incomingWindow)
      {
         return WithIncomingWindow(Is.EqualTo(incomingWindow));
      }

      public FlowExpectation WithNextOutgoingId(uint nextOutgoingId)
      {
         return WithNextOutgoingId(Is.EqualTo(nextOutgoingId));
      }

      public FlowExpectation WithOutgoingWindow(uint outgoingWindow)
      {
         return WithOutgoingWindow(Is.EqualTo(outgoingWindow));
      }

      public FlowExpectation WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public FlowExpectation WithDeliveryCount(uint deliveryCount)
      {
         return WithDeliveryCount(Is.EqualTo(deliveryCount));
      }

      public FlowExpectation WithLinkCredit(uint linkCredit)
      {
         return WithLinkCredit(Is.EqualTo(linkCredit));
      }

      public FlowExpectation WithAvailable(uint available)
      {
         return WithAvailable(Is.EqualTo(available));
      }

      public FlowExpectation WithDrain(bool drain)
      {
         return WithDrain(Is.EqualTo(drain));
      }

      public FlowExpectation WithEcho(bool echo)
      {
         return WithEcho(Is.EqualTo(echo));
      }

      public FlowExpectation WithProperties(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      #region Matcher based With API

      public FlowExpectation WithNextIncomingId(IMatcher m)
      {
         matcher.WithNextIncomingId(m);
         return this;
      }

      public FlowExpectation WithIncomingWindow(IMatcher m)
      {
         matcher.WithIncomingWindow(m);
         return this;
      }

      public FlowExpectation WithNextOutgoingId(IMatcher m)
      {
         matcher.WithNextOutgoingId(m);
         return this;
      }

      public FlowExpectation WithOutgoingWindow(IMatcher m)
      {
         matcher.WithOutgoingWindow(m);
         return this;
      }

      public FlowExpectation WithHandle(IMatcher m)
      {
         matcher.WithHandle(m);
         return this;
      }

      public FlowExpectation WithDeliveryCount(IMatcher m)
      {
         matcher.WithDeliveryCount(m);
         return this;
      }

      public FlowExpectation WithLinkCredit(IMatcher m)
      {
         matcher.WithLinkCredit(m);
         return this;
      }

      public FlowExpectation WithAvailable(IMatcher m)
      {
         matcher.WithAvailable(m);
         return this;
      }

      public FlowExpectation WithDrain(IMatcher m)
      {
         matcher.WithDrain(m);
         return this;
      }

      public FlowExpectation WithEcho(IMatcher m)
      {
         matcher.WithEcho(m);
         return this;
      }

      public FlowExpectation WithProperties(IMatcher m)
      {
         matcher.WithProperties(m);
         return this;
      }

      #endregion
   }
}