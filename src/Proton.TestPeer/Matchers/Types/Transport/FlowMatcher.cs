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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class FlowMatcher : ListDescribedTypeMatcher
   {
      public FlowMatcher() : base(Enum.GetNames(typeof(FlowField)).Length, Flow.DESCRIPTOR_CODE, Flow.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Flow);

      public FlowMatcher WithNextIncomingId(uint nextIncomingId)
      {
         return WithNextIncomingId(Is.EqualTo(nextIncomingId));
      }

      public FlowMatcher WithIncomingWindow(uint incomingWindow)
      {
         return WithIncomingWindow(Is.EqualTo(incomingWindow));
      }

      public FlowMatcher WithNextOutgoingId(uint nextOutgoingId)
      {
         return WithNextOutgoingId(Is.EqualTo(nextOutgoingId));
      }

      public FlowMatcher WithOutgoingWindow(uint outgoingWindow)
      {
         return WithOutgoingWindow(Is.EqualTo(outgoingWindow));
      }

      public FlowMatcher WithHandle(uint handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public FlowMatcher WithDeliveryCount(uint deliveryCount)
      {
         return WithDeliveryCount(Is.EqualTo(deliveryCount));
      }

      public FlowMatcher WithLinkCredit(uint linkCredit)
      {
         return WithLinkCredit(Is.EqualTo(linkCredit));
      }

      public FlowMatcher WithAvailable(uint available)
      {
         return WithAvailable(Is.EqualTo(available));
      }

      public FlowMatcher WithDrain(bool drain)
      {
         return WithDrain(Is.EqualTo(drain));
      }

      public FlowMatcher WithEcho(bool echo)
      {
         return WithEcho(Is.EqualTo(echo));
      }

      public FlowMatcher WithProperties(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      public FlowMatcher WithProperties(IDictionary<string, object> properties)
      {
         return WithProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      #region With API for field matching

      public FlowMatcher WithNextIncomingId(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.NextIncomingId, m);
         return this;
      }

      public FlowMatcher WithIncomingWindow(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.IncomingWindow, m);
         return this;
      }

      public FlowMatcher WithNextOutgoingId(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.NextOutgoingId, m);
         return this;
      }

      public FlowMatcher WithOutgoingWindow(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.OutgoingWindow, m);
         return this;
      }

      public FlowMatcher WithHandle(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.Handle, m);
         return this;
      }

      public FlowMatcher WithDeliveryCount(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.DeliveryCount, m);
         return this;
      }

      public FlowMatcher WithLinkCredit(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.LinkCredit, m);
         return this;
      }

      public FlowMatcher WithAvailable(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.Available, m);
         return this;
      }

      public FlowMatcher WithDrain(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.Drain, m);
         return this;
      }

      public FlowMatcher WithEcho(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.Echo, m);
         return this;
      }

      public FlowMatcher WithProperties(IMatcher m)
      {
         AddFieldMatcher((int)FlowField.Properties, m);
         return this;
      }

      #endregion
   }
}