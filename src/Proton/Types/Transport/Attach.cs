/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
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
using System.Numerics;
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Tramsactions;

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class Attach : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000012UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:attach:list");

      private static readonly uint NAME = 1;
      private static readonly uint HANDLE = 2;
      private static readonly uint ROLE = 4;
      private static readonly uint SENDER_SETTLE_MODE = 8;
      private static readonly uint RECEIVER_SETTLE_MODE = 16;
      private static readonly uint SOURCE = 32;
      private static readonly uint TARGET = 64;
      private static readonly uint UNSETTLED = 128;
      private static readonly uint INCOMPLETE_UNSETTLED = 256;
      private static readonly uint INITIAL_DELIVERY_COUNT = 512;
      private static readonly uint MAX_MESSAGE_SIZE = 1024;
      private static readonly uint OFFERED_CAPABILITIES = 2048;
      private static readonly uint DESIRED_CAPABILITIES = 4096;
      private static readonly uint PROPERTIES = 8192;

      private uint modified = 0;

      private string name;
      private uint handle;
      private Role role = Role.Sender;
      private SenderSettleMode sndSettleMode = SenderSettleMode.Mixed;
      private ReceiverSettleMode rcvSettleMode = ReceiverSettleMode.First;
      private Source source;
      private ITerminus target;
      private IDictionary<IProtonBuffer, IDeliveryState> unsettled;
      private bool incompleteUnsettled;
      private uint initialDeliveryCount;
      private ulong maxMessageSize;
      private Symbol[] offeredCapabilities;
      private Symbol[] desiredCapabilities;
      private IDictionary<Symbol, object> properties;

      public Attach() : base() { }

      public Attach(Attach other) : this()
      {
         name = other.name;
         handle = other.handle;
         role = other.role;
         sndSettleMode = other.sndSettleMode;
         rcvSettleMode = other.rcvSettleMode;
         source = (Source)(other.source?.Clone());
         target = (ITerminus)(other.target?.Clone());
         incompleteUnsettled = other.incompleteUnsettled;
         initialDeliveryCount = other.initialDeliveryCount;
         maxMessageSize = other.maxMessageSize;
         offeredCapabilities = (Symbol[])(other.offeredCapabilities?.Clone());
         desiredCapabilities = (Symbol[])(other.desiredCapabilities?.Clone());

         if (other.unsettled != null)
         {
            unsettled = new Dictionary<IProtonBuffer, IDeliveryState>(other.unsettled);
         }

         if (other.properties != null)
         {
            properties = new Dictionary<Symbol, object>(other.properties);
         }

         modified = other.modified;
      }

      #region Element access

      public string Name
      {
         get { return name; }
         set
         {
            if (name == null)
            {
               throw new ArgumentNullException("the name field is mandatory");
            }

            modified |= NAME;
            name = value;
         }
      }

      public uint Handle
      {
         get { return handle; }
         set
         {
            modified |= HANDLE;
            handle = value;
         }
      }

      public Role Role
      {
         get { return role; }
         set
         {
            modified |= ROLE;
            role = value;
         }
      }

      public SenderSettleMode SenderSettleMode
      {
         get { return sndSettleMode; }
         set
         {
            modified |= SENDER_SETTLE_MODE;
            sndSettleMode = value;
         }
      }

      public ReceiverSettleMode ReceiverSettleMode
      {
         get { return rcvSettleMode; }
         set
         {
            modified |= RECEIVER_SETTLE_MODE;
            rcvSettleMode = value;
         }
      }

      public Source Source
      {
         get { return source; }
         set
         {
            modified |= SOURCE;
            source = value;
         }
      }

      public ITerminus Target
      {
         get { return target; }
         set
         {
            if (value is Target || value is Coordinator)
            {
               modified |= TARGET;
               target = value;
            }
            else
            {
               throw new ArgumentException("Terminus must either be a Target or a Coordinator");
            }
         }
      }

      public IDictionary<IProtonBuffer, IDeliveryState> Unsettled
      {
         get { return unsettled; }
         set
         {
            modified |= UNSETTLED;
            unsettled = value;
         }
      }

      public bool IncompleteUnsettled
      {
         get { return incompleteUnsettled; }
         set
         {
            modified |= INCOMPLETE_UNSETTLED;
            incompleteUnsettled = value;
         }
      }

      public uint InitialDeliveryCount
      {
         get { return initialDeliveryCount; }
         set
         {
            modified |= INITIAL_DELIVERY_COUNT;
            initialDeliveryCount = value;
         }
      }

      public ulong MaxMessageSize
      {
         get { return maxMessageSize; }
         set
         {
            modified |= MAX_MESSAGE_SIZE;
            maxMessageSize = value;
         }
      }

      public Symbol[] OfferedCapabilities
      {
         get { return offeredCapabilities; }
         set
         {
            modified |= OFFERED_CAPABILITIES;
            offeredCapabilities = value;
         }
      }

      public Symbol[] DesiredCapabilities
      {
         get { return desiredCapabilities; }
         set
         {
            modified |= DESIRED_CAPABILITIES;
            desiredCapabilities = value;
         }
      }

      public IDictionary<Symbol, object> Properties
      {
         get { return properties; }
         set
         {
            modified |= PROPERTIES;
            properties = value;
         }
      }

      #endregion

      #region Element count and value presence utility

      public bool IsEmpty() => modified == 0;

      public int GetElementCount() => 32 - BitOperations.LeadingZeroCount(modified);

      public bool HasName() => (modified & NAME) == NAME;

      public bool HasHandle() => (modified & HANDLE) == HANDLE;

      public bool HasRole() => (modified & ROLE) == ROLE;

      public bool HasSenderSettleMode() => (modified & SENDER_SETTLE_MODE) == SENDER_SETTLE_MODE;

      public bool HasReceiverSettleMode() => (modified & RECEIVER_SETTLE_MODE) == RECEIVER_SETTLE_MODE;

      public bool HasSource() => (modified & SOURCE) == SOURCE;

      public bool HasTargetOrCoordinator() => (modified & TARGET) == TARGET;

      public bool HasTarget() => (modified & TARGET) == TARGET && target is Target;

      public bool HasCoordinator() => (modified & TARGET) == TARGET && target is Coordinator;

      public bool HasUnsettled() => (modified & UNSETTLED) == UNSETTLED;

      public bool HasIncompleteUnsettled() => (modified & INCOMPLETE_UNSETTLED) == INCOMPLETE_UNSETTLED;

      public bool HasInitialDeliveryCount() => (modified & INITIAL_DELIVERY_COUNT) == INITIAL_DELIVERY_COUNT;

      public bool HasMaxMessageSize() => (modified & MAX_MESSAGE_SIZE) == MAX_MESSAGE_SIZE;

      public bool HasOfferedCapabilities() => (modified & OFFERED_CAPABILITIES) == OFFERED_CAPABILITIES;

      public bool HasDesiredCapabilities() => (modified & DESIRED_CAPABILITIES) == DESIRED_CAPABILITIES;

      public bool HasProperties() => (modified & PROPERTIES) == PROPERTIES;

      #endregion

      public object Clone()
      {
         return new Attach(this);
      }

      public PerformativeType Type => PerformativeType.Attach;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, int channel, T context)
      {
         handler.HandleAttach(this, payload, channel, context);
      }

      public new string ToString()
      {
        return "Attach{" +
            "name='" + name + '\'' +
            ", handle=" + (HasHandle() ? handle : "null") +
            ", role=" + (HasRole() ? role : "null") +
            ", sndSettleMode=" + (HasSenderSettleMode() ? sndSettleMode : "null") +
            ", rcvSettleMode=" + (HasReceiverSettleMode() ? rcvSettleMode : "null") +
            ", source=" + source +
            ", target=" + target +
            ", unsettled=" + unsettled +
            ", incompleteUnsettled=" + (HasIncompleteUnsettled() ? incompleteUnsettled : "null") +
            ", initialDeliveryCount=" + (HasInitialDeliveryCount() ? initialDeliveryCount : "null") +
            ", maxMessageSize=" + maxMessageSize +
            ", offeredCapabilities=" + (offeredCapabilities == null ? "null" : offeredCapabilities) +
            ", desiredCapabilities=" + (desiredCapabilities == null ? "null" : desiredCapabilities) +
            ", properties=" + properties + '}';
      }
   }
}