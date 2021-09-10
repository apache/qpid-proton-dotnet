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
   public sealed class BeginMatcher : ListDescribedTypeMatcher
   {
      public BeginMatcher() : base(Enum.GetNames(typeof(BeginField)).Length, Begin.DESCRIPTOR_CODE, Begin.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Begin);

      public BeginMatcher WithRemoteChannel(ushort remoteChannel)
      {
         return WithRemoteChannel(Is.EqualTo(remoteChannel));
      }

      public BeginMatcher WithNextOutgoingId(uint nextOutgoingId)
      {
         return WithNextOutgoingId(Is.EqualTo(nextOutgoingId));
      }

      public BeginMatcher WithIncomingWindow(uint incomingWindow)
      {
         return WithIncomingWindow(Is.EqualTo(incomingWindow));
      }

      public BeginMatcher WithOutgoingWindow(uint outgoingWindow)
      {
         return WithOutgoingWindow(Is.EqualTo(outgoingWindow));
      }

      public BeginMatcher WithHandleMax(uint handleMax)
      {
         return WithHandleMax(Is.EqualTo(handleMax));
      }

      public BeginMatcher WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(offeredCapabilities)));
      }

      public BeginMatcher WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(offeredCapabilities));
      }

      public BeginMatcher WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(desiredCapabilities)));
      }

      public BeginMatcher WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(desiredCapabilities));
      }

      public BeginMatcher WithPropertiesMap(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      public BeginMatcher WithProperties(IDictionary<string, object> properties)
      {
         return WithProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      #region Matcher based With API

      public BeginMatcher WithRemoteChannel(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.RemoteChannel, m);
         return this;
      }

      public BeginMatcher WithNextOutgoingId(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.NextOutgoingId, m);
         return this;
      }

      public BeginMatcher WithIncomingWindow(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.IncomingWindow, m);
         return this;
      }

      public BeginMatcher WithOutgoingWindow(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.OutgoingWindow, m);
         return this;
      }

      public BeginMatcher WithHandleMax(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.HandleMax, m);
         return this;
      }

      public BeginMatcher WithOfferedCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.OfferedCapabilities, m);
         return this;
      }

      public BeginMatcher WithDesiredCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.DesiredCapabilities, m);
         return this;
      }

      public BeginMatcher WithProperties(IMatcher m)
      {
         AddFieldMatcher((int)BeginField.Properties, m);
         return this;
      }

      #endregion
   }
}