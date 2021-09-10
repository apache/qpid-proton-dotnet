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

      public OpenMatcher withContainerId(string container)
      {
         return withContainerId(Is.EqualTo(container));
      }

      public OpenMatcher withHostname(string hostname)
      {
         return withHostname(Is.EqualTo(hostname));
      }

      public OpenMatcher withMaxFrameSize(uint maxFrameSize)
      {
         return withMaxFrameSize(Is.EqualTo(maxFrameSize));
      }

      public OpenMatcher withChannelMax(ushort channelMax)
      {
         return withChannelMax(Is.EqualTo(channelMax));
      }

      public OpenMatcher withIdleTimeOut(uint idleTimeout)
      {
         return withIdleTimeOut(Is.EqualTo(idleTimeout));
      }

      public OpenMatcher withOutgoingLocales(params string[] outgoingLocales)
      {
         return withOutgoingLocales(Is.EqualTo(TypeMapper.ToSymbolArray(outgoingLocales)));
      }

      public OpenMatcher withOutgoingLocales(params Symbol[] outgoingLocales)
      {
         return withOutgoingLocales(Is.EqualTo(outgoingLocales));
      }

      public OpenMatcher withIncomingLocales(params string[] incomingLocales)
      {
         return withIncomingLocales(Is.EqualTo(TypeMapper.ToSymbolArray(incomingLocales)));
      }

      public OpenMatcher withIncomingLocales(params Symbol[] incomingLocales)
      {
         return withIncomingLocales(Is.EqualTo(incomingLocales));
      }

      public OpenMatcher withOfferedCapabilities(params string[] offeredCapabilities)
      {
         return withOfferedCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(offeredCapabilities)));
      }

      public OpenMatcher withOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         return withOfferedCapabilities(Is.EqualTo(offeredCapabilities));
      }

      public OpenMatcher withDesiredCapabilities(params string[] desiredCapabilities)
      {
         return withDesiredCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(desiredCapabilities)));
      }

      public OpenMatcher withDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         return withDesiredCapabilities(Is.EqualTo(desiredCapabilities));
      }

      public OpenMatcher withPropertiesMap(IDictionary<Symbol, object> properties)
      {
         return withProperties(Is.EqualTo(properties));
      }

      public OpenMatcher withProperties(IDictionary<string, object> properties)
      {
         return withProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      #region Matcher based With API

      public OpenMatcher withContainerId(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.ContainerId, m);
         return this;
      }

      public OpenMatcher withHostname(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.Hostname, m);
         return this;
      }

      public OpenMatcher withMaxFrameSize(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.MaxFrameSize, m);
         return this;
      }

      public OpenMatcher withChannelMax(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.ChannelMax, m);
         return this;
      }

      public OpenMatcher withIdleTimeOut(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.IdleTimeout, m);
         return this;
      }

      public OpenMatcher withOutgoingLocales(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.OutgoingLocales, m);
         return this;
      }

      public OpenMatcher withIncomingLocales(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.IncmoningLocales, m);
         return this;
      }

      public OpenMatcher withOfferedCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.OfferedCapabilities, m);
         return this;
      }

      public OpenMatcher withDesiredCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.DesiredCapabilities, m);
         return this;
      }

      public OpenMatcher withProperties(IMatcher m)
      {
         AddFieldMatcher((int)OpenField.Properties, m);
         return this;
      }

      #endregion
   }
}