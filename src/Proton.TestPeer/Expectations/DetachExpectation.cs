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
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Expectations
{
   /// <summary>
   /// Scripted expectation for the AMQP performative
   /// </summary>
   public sealed class DetachExpectation : AbstractExpectation<Detach>
   {
      private readonly DetachMatcher matcher = new DetachMatcher();

      private DetachInjectAction response;

      public DetachExpectation(AMQPTestDriver driver) : base(driver)
      {
         // Default validation of mandatory fields
         WithHandle(Is.NotNullValue());
      }

      protected override IMatcher GetExpectationMatcher() => matcher;

      public override DetachExpectation OnChannel(ushort channel)
      {
         base.OnChannel(channel);
         return this;
      }

      public DetachInjectAction Respond()
      {
         response = new DetachInjectAction(driver);
         driver.AddScriptedElement(response);
         return response;
      }

      public override void HandleDetach(uint frameSize, Detach detach, Span<byte> payload, ushort channel, AMQPTestDriver context)
      {
         base.HandleDetach(frameSize, detach, payload, channel, context);

         SessionTracker session = driver.Sessions.SessionFromRemoteChannel(channel);

         if (session == null)
         {
            throw new AssertionError(string.Format(
                "Received Detach on channel [{0}] that has no matching Session for that remote channel. ", channel));
         }

         LinkTracker link = session.HandleRemoteDetach(detach);

         if (link == null)
         {
            throw new AssertionError(string.Format(
                "Received Detach on channel [{0}] that has no matching Attached link for that remote handle. ", detach.Handle));
         }

         if (response != null)
         {
            // Input was validated now populate response With auto values where not configured
            // to say otherwise by the test.
            if (response.OnChannel() == null)
            {
               response.OnChannel((ushort)link.Session.LocalChannel);
            }

            if (response.Performative.Handle == null)
            {
               response.WithHandle((uint)link.Handle);
            }

            if (response.Performative.Closed == null)
            {
               response.WithClosed((bool)detach.Closed);
            }
         }
      }

      public DetachExpectation WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public DetachExpectation WithClosed(bool closed)
      {
         return WithClosed(Is.EqualTo(closed));
      }

      public DetachExpectation WithError(ErrorCondition error)
      {
         return WithError(Is.EqualTo(error));
      }

      public DetachExpectation WithError(String condition, String description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description)));
      }

      public DetachExpectation WithError(String condition, String description, IDictionary<string, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(new Symbol(condition), description, TypeMapper.ToSymbolKeyedMap(info))));
      }

      public DetachExpectation WithError(Symbol condition, String description)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description)));
      }

      public DetachExpectation WithError(Symbol condition, String description, IDictionary<Symbol, object> info)
      {
         return WithError(Is.EqualTo(new ErrorCondition(condition, description, info)));
      }

      #region Matcher based With API

      public DetachExpectation WithHandle(IMatcher m)
      {
         matcher.WithHandle(m);
         return this;
      }

      public DetachExpectation WithClosed(IMatcher m)
      {
         matcher.WithClosed(m);
         return this;
      }

      public DetachExpectation WithError(IMatcher m)
      {
         matcher.WithError(m);
         return this;
      }

      #endregion
   }
}