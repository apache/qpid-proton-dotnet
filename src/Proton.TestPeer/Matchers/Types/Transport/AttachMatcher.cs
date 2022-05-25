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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transactions;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport
{
   public sealed class AttachMatcher : ListDescribedTypeMatcher
   {
      public AttachMatcher() : base(Enum.GetNames(typeof(AttachField)).Length, Attach.DESCRIPTOR_CODE, Attach.DESCRIPTOR_SYMBOL)
      {
      }

      protected override Type DescribedTypeClassType => typeof(Attach);

      public AttachMatcher WithName(string name)
      {
         return WithName(Is.EqualTo(name));
      }

      public AttachMatcher WithHandle(int handle)
      {
         return WithHandle(Is.EqualTo(handle));
      }

      public AttachMatcher WithRole(bool role)
      {
         return WithRole(Is.EqualTo(role));
      }

      public AttachMatcher WithRole(Role role)
      {
         return WithRole(Is.EqualTo(role.ToBooleanEncoding()));
      }

      public AttachMatcher WithSndSettleMode(byte sndSettleMode)
      {
         return WithSndSettleMode(Is.EqualTo(sndSettleMode));
      }

      public AttachMatcher WithSndSettleMode(SenderSettleMode? sndSettleMode)
      {
         return WithSndSettleMode(sndSettleMode == null ? Is.NullValue() : Is.EqualTo((byte)sndSettleMode));
      }

      public AttachMatcher WithRcvSettleMode(byte rcvSettleMode)
      {
         return WithRcvSettleMode(Is.EqualTo((ReceiverSettleMode)rcvSettleMode));
      }

      public AttachMatcher WithRcvSettleMode(ReceiverSettleMode? rcvSettleMode)
      {
         return WithRcvSettleMode(rcvSettleMode == null ? Is.NullValue() : Is.EqualTo((byte)rcvSettleMode));
      }

      public AttachMatcher WithSource(Source source)
      {
         if (source != null)
         {
            SourceMatcher sourceMatcher = new(source);
            return WithSource(sourceMatcher);
         }
         else
         {
            return WithSource(Is.NullValue());
         }
      }

      public AttachMatcher WithTarget(Target target)
      {
         if (target != null)
         {
            TargetMatcher targetMatcher = new(target);
            return WithTarget(targetMatcher);
         }
         else
         {
            return WithTarget(Is.NullValue());
         }
      }

      public AttachMatcher WithCoordinator(Coordinator coordinator)
      {
         if (coordinator != null)
         {
            CoordinatorMatcher coordinatorMatcher = new();
            return WithCoordinator(coordinatorMatcher);
         }
         else
         {
            return WithCoordinator(Is.NullValue());
         }
      }

      public AttachMatcher WithUnsettled(IDictionary<Binary, IDeliveryState> unsettled)
      {
         return WithUnsettled(Is.EqualTo(unsettled));
      }

      public AttachMatcher WithIncompleteUnsettled(bool incomplete)
      {
         return WithIncompleteUnsettled(Is.EqualTo(incomplete));
      }

      public AttachMatcher WithInitialDeliveryCount(uint initialDeliveryCount)
      {
         return WithInitialDeliveryCount(Is.EqualTo(initialDeliveryCount));
      }

      public AttachMatcher WithMaxMessageSize(ulong maxMessageSize)
      {
         return WithMaxMessageSize(Is.EqualTo(maxMessageSize));
      }

      public AttachMatcher WithOfferedCapabilities(params Symbol[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(offeredCapabilities));
      }

      public AttachMatcher WithOfferedCapabilities(params string[] offeredCapabilities)
      {
         return WithOfferedCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(offeredCapabilities)));
      }

      public AttachMatcher WithDesiredCapabilities(params Symbol[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(desiredCapabilities));
      }

      public AttachMatcher WithDesiredCapabilities(params string[] desiredCapabilities)
      {
         return WithDesiredCapabilities(Is.EqualTo(TypeMapper.ToSymbolArray(desiredCapabilities)));
      }

      public AttachMatcher WithPropertiesMap(IDictionary<Symbol, object> properties)
      {
         return WithProperties(Is.EqualTo(properties));
      }

      public AttachMatcher WithProperties(IDictionary<string, object> properties)
      {
         return WithProperties(Is.EqualTo(TypeMapper.ToSymbolKeyedMap(properties)));
      }

      #region Matcher based With API

      public AttachMatcher WithName(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Name, m);
         return this;
      }

      public AttachMatcher WithHandle(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Handle, m);
         return this;
      }

      public AttachMatcher WithRole(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Role, m);
         return this;
      }

      public AttachMatcher WithSndSettleMode(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.SenderSettleMode, m);
         return this;
      }

      public AttachMatcher WithRcvSettleMode(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.ReceiverSettleMode, m);
         return this;
      }

      public AttachMatcher WithSource(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Source, m);
         return this;
      }

      public AttachMatcher WithTarget(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Target, m);
         return this;
      }

      public AttachMatcher WithCoordinator(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Target, m);
         return this;
      }

      public AttachMatcher WithUnsettled(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Unsettled, m);
         return this;
      }

      public AttachMatcher WithIncompleteUnsettled(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.IncompleteUnsettled, m);
         return this;
      }

      public AttachMatcher WithInitialDeliveryCount(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.InitialDeliveryCount, m);
         return this;
      }

      public AttachMatcher WithMaxMessageSize(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.MaxMessageSize, m);
         return this;
      }

      public AttachMatcher WithOfferedCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.OfferedCapabilities, m);
         return this;
      }

      public AttachMatcher WithDesiredCapabilities(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.DesiredCapabilities, m);
         return this;
      }

      public AttachMatcher WithProperties(IMatcher m)
      {
         AddFieldMatcher((int)AttachField.Properties, m);
         return this;
      }

      #endregion
   }
}