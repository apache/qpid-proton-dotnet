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
   public sealed class OpenMatcher : ListDescribedTypeMatcher
   {
      public OpenMatcher() : base(Enum.GetNames(typeof(OpenField)).Length, Open.DESCRIPTOR_CODE, Open.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Open);

      public OpenMatcher WithContainerId(string container)
      {
         return WithContainerId(Is.EqualTo(container));
      }

      public OpenMatcher WithHostname(string hostname)
      {
         return WithHostname(Is.EqualTo(hostname));
      }

      public OpenMatcher WithMaxFrameSize(uint maxFrameSize)
      {
         return WithMaxFrameSize(Is.EqualTo(maxFrameSize));
      }

      public OpenMatcher WithChannelMax(ushort channelMax)
      {
         return WithChannelMax(Is.EqualTo(channelMax));
      }

      public OpenMatcher WithIdleTimeOut(uint idleTimeout)
      {
         return WithIdleTimeOut(Is.EqualTo(idleTimeout));
      }

      public OpenMatcher WithOutgoingLocales(params string[] outgoingLocales)
      {
         return WithOutgoingLocales(Is.EqualTo(TypeMapper.ToSymbolArray(outgoingLocales)));
      }

      public OpenMatcher WithOutgoingLocales(params Symbol[] outgoingLocales)
      {
         return WithOutgoingLocales(Is.EqualTo(outgoingLocales));
      }

      public OpenMatcher WithIncomingLocales(params string[] incomingLocales)
      {
         return WithIncomingLocales(Is.EqualTo(TypeMapper.ToSymbolArray(incomingLocales)));
      }

      public OpenMatcher WithIncomingLocales(params Symbol[] incomingLocales)
      {
         return WithIncomingLocales(Is.EqualTo(incomingLocales));
      }

      public OpenMatcher WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(offeredCapabilities)));
      }

      public OpenMatcher WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(offeredCapabilities));
      }

      public OpenMatcher WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(desiredCapabilities)));
      }

      public OpenMatcher WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(desiredCapabilities));
      }

      public OpenMatcher WithPropertiesMap(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      public OpenMatcher WithProperties(IDictionary<string, object> properties)
      {
         return WithProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      #region Matcher based With API

      public OpenMatcher WithContainerId(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.ContainerId, m);
         return this;
      }

      public OpenMatcher WithHostname(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.Hostname, m);
         return this;
      }

      public OpenMatcher WithMaxFrameSize(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.MaxFrameSize, m);
         return this;
      }

      public OpenMatcher WithChannelMax(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.ChannelMax, m);
         return this;
      }

      public OpenMatcher WithIdleTimeOut(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.IdleTimeout, m);
         return this;
      }

      public OpenMatcher WithOutgoingLocales(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.OutgoingLocales, m);
         return this;
      }

      public OpenMatcher WithIncomingLocales(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.IncomingLocales, m);
         return this;
      }

      public OpenMatcher WithOfferedCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.OfferedCapabilities, m);
         return this;
      }

      public OpenMatcher WithDesiredCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.DesiredCapabilities, m);
         return this;
      }

      public OpenMatcher WithProperties(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.Properties, m);
         return this;
      }

      #endregion
   }
}