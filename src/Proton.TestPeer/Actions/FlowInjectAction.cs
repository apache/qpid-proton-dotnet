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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Performative into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public class FlowInjectAction : AbstractPerformativeInjectAction<Flow>
   {
      private readonly Flow flow = new Flow();

      private bool explicitlyNullHandle;

      public FlowInjectAction(AMQPTestDriver driver) : base(driver)
      {
      }

      public override Flow Performative => flow;

      public FlowInjectAction WithNextIncomingId(uint nextIncomingId)
      {
         flow.NextIncomingId = nextIncomingId;
         return this;
      }

      public FlowInjectAction WithIncomingWindow(uint incomingWindow)
      {
         flow.IncomingWindow = incomingWindow;
         return this;
      }

      public FlowInjectAction WithNextOutgoingId(uint nextOutgoingId)
      {
         flow.NextOutgoingId = nextOutgoingId;
         return this;
      }

      public FlowInjectAction WithOutgoingWindow(uint outgoingWindow)
      {
         flow.OutgoingWindow = outgoingWindow;
         return this;
      }

      public FlowInjectAction WithHandle(uint handle)
      {
         flow.Handle = handle;
         return this;
      }

      public FlowInjectAction WithNullHandle()
      {
         explicitlyNullHandle = true;
         flow.Handle = null;
         return this;
      }

      public FlowInjectAction WithDeliveryCount(uint deliveryCount)
      {
         flow.DeliveryCount = deliveryCount;
         return this;
      }

      public FlowInjectAction WithLinkCredit(uint linkCredit)
      {
         flow.LinkCredit = linkCredit;
         return this;
      }

      public FlowInjectAction WithAvailable(uint available)
      {
         flow.Available = available;
         return this;
      }

      public FlowInjectAction WithDrain(bool drain)
      {
         flow.Drain = drain;
         return this;
      }

      public FlowInjectAction WithEcho(bool echo)
      {
         flow.Echo = echo;
         return this;
      }

      public FlowInjectAction withProperties(IDictionary<Symbol, object> properties)
      {
         flow.Properties = properties;
         return this;
      }

      protected override void BeforeActionPerformed(AMQPTestDriver driver)
      {
         SessionTracker session = driver.Sessions.LastLocallyOpenedSession;
         LinkTracker link = session.LastOpenedLink;

         // We fill in a channel using the next available channel id if one isn't set, then
         // report the outbound begin to the session so it can track this new session.
         if (channel == null)
         {
            channel = session.LocalChannel;
         }

         // TODO: The values set in the outbound flow should be read from actively maintained
         //       values in the parent session / link.

         // Auto select last opened sender on last opened session, unless there's no links opened
         // in which case we can assume this is session only flow.  Also check if the test scripted
         // this as null which indicates the test is trying to send session only.
         if (flow.Handle == null && !explicitlyNullHandle && link != null)
         {
            flow.Handle = link.Handle;
         }
         if (flow.IncomingWindow == null)
         {
            flow.IncomingWindow = session.LocalBegin.IncomingWindow;
         }
         if (flow.NextIncomingId == null)
         {
            flow.NextIncomingId = session.NextIncomingId;
         }
         if (flow.NextOutgoingId == null)
         {
            flow.NextOutgoingId = session.LocalBegin.NextOutgoingId;
         }
         if (flow.OutgoingWindow == null)
         {
            flow.OutgoingWindow = session.LocalBegin.OutgoingWindow;
         }
      }
   }
}