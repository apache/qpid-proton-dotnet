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

using System.Numerics;
using Apache.Qpid.Proton.Buffer;
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class Flow : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000013UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:flow:list");

      private static readonly uint NEXT_INCOMING_ID = 1;
      private static readonly uint INCOMING_WINDOW = 2;
      private static readonly uint NEXT_OUTGOING_ID = 4;
      private static readonly uint OUTGOING_WINDOW = 8;
      private static readonly uint HANDLE = 16;
      private static readonly uint DELIVERY_COUNT = 32;
      private static readonly uint LINK_CREDIT = 64;
      private static readonly uint AVAILABLE = 128;
      private static readonly uint DRAIN = 256;
      private static readonly uint ECHO = 512;
      private static readonly uint PROPERTIES = 1024;

      private uint modified = 0;

      private uint nextIncomingId;
      private uint incomingWindow;
      private uint nextOutgoingId;
      private uint outgoingWindow;
      private uint handle;
      private uint deliveryCount;
      private uint linkCredit;
      private uint available;
      private bool drain;
      private bool echo;
      private IDictionary<Symbol, object> properties;

      public Flow() : base() { }

      public Flow(Flow other) : this()
      {
         nextIncomingId = other.nextIncomingId;
         incomingWindow = other.incomingWindow;
         nextOutgoingId = other.nextOutgoingId;
         outgoingWindow = other.outgoingWindow;
         handle = other.handle;
         deliveryCount = other.deliveryCount;
         linkCredit = other.linkCredit;
         available = other.available;
         drain = other.drain;
         echo = other.echo;

         if (other.properties != null)
         {
            properties = new Dictionary<Symbol, object>(other.properties);
         }

         modified = other.modified;
      }

      public Flow Reset()
      {
         modified = 0;
         nextIncomingId = 0;
         incomingWindow = 0;
         nextOutgoingId = 0;
         outgoingWindow = 0;
         handle = 0;
         deliveryCount = 0;
         linkCredit = 0;
         available = 0;
         drain = false;
         echo = false;
         properties = null;

         return this;
      }

      public Flow ClearNextIncomingId()
      {
         modified &= ~NEXT_INCOMING_ID;
         nextIncomingId = 0;
         return this;
      }

      public Flow ClearNextOutgoingId()
      {
         modified &= ~NEXT_OUTGOING_ID;
         nextOutgoingId = 0;
         return this;
      }

      public Flow ClearIncomingWindow()
      {
         modified &= ~INCOMING_WINDOW;
         incomingWindow = 0;
         return this;
      }

      public Flow ClearOutgoingWindow()
      {
         modified &= ~OUTGOING_WINDOW;
         outgoingWindow = 0;
         return this;
      }

      public Flow ClearHandle()
      {
         modified &= ~HANDLE;
         handle = 0;
         return this;
      }

      public Flow ClearDeliveryCount()
      {
         modified &= ~DELIVERY_COUNT;
         deliveryCount = 0;
         return this;
      }

      public Flow ClearLinkCredit()
      {
         modified &= ~LINK_CREDIT;
         linkCredit = 0;
         return this;
      }

      public Flow ClearAvailable()
      {
         modified &= ~AVAILABLE;
         available = 0;
         return this;
      }

      public Flow ClearDrain()
      {
         modified &= ~DRAIN;
         drain = false;
         return this;
      }

      public Flow ClearEcho()
      {
         modified &= ~ECHO;
         echo = false;
         return this;
      }

      public Flow ClearProperties()
      {
         modified &= ~PROPERTIES;
         properties = null;
         return this;
      }

      #region Element access

      public uint NextOutgoingId
      {
         get { return nextOutgoingId; }
         set
         {
            modified |= NEXT_OUTGOING_ID;
            nextOutgoingId = value;
         }
      }

      public uint NextIncomingId
      {
         get { return nextIncomingId; }
         set
         {
            modified |= NEXT_INCOMING_ID;
            nextIncomingId = value;
         }
      }

      public uint IncomingWindow
      {
         get { return incomingWindow; }
         set
         {
            modified |= INCOMING_WINDOW;
            incomingWindow = value;
         }
      }

      public uint OutgoingWindow
      {
         get { return outgoingWindow; }
         set
         {
            modified |= OUTGOING_WINDOW;
            outgoingWindow = value;
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

      public uint DeliveryCount
      {
         get { return deliveryCount; }
         set
         {
            modified |= DELIVERY_COUNT;
            deliveryCount = value;
         }
      }

      public uint LinkCredit
      {
         get { return linkCredit; }
         set
         {
            modified |= LINK_CREDIT;
            linkCredit = value;
         }
      }

      public uint Available
      {
         get { return available; }
         set
         {
            modified |= AVAILABLE;
            available = value;
         }
      }

      public bool Drain
      {
         get { return drain; }
         set
         {
            modified |= DRAIN;
            drain = value;
         }
      }

      public bool Echo
      {
         get { return echo; }
         set
         {
            modified |= ECHO;
            echo = value;
         }
      }

      public IDictionary<Symbol, object> Properties
      {
         get { return properties; }
         set
         {
            if (value == null)
            {
               modified &= ~PROPERTIES;
            }
            else
            {
               modified |= PROPERTIES;
            }

            properties = value;
         }
      }

      #endregion

      #region Element count and value presence utility

      public bool IsEmpty() => modified == 0;

      public int GetElementCount() => 32 - BitOperations.LeadingZeroCount(modified);

      public bool HasNextIncomingId() => (modified & NEXT_INCOMING_ID) == NEXT_INCOMING_ID;

      public bool HasIncomingWindow() => (modified & INCOMING_WINDOW) == INCOMING_WINDOW;

      public bool HasNextOutgoingId() => (modified & NEXT_OUTGOING_ID) == NEXT_OUTGOING_ID;

      public bool HasOutgoingWindow() => (modified & OUTGOING_WINDOW) == OUTGOING_WINDOW;

      public bool HasHandle() => (modified & HANDLE) == HANDLE;

      public bool HasDeliveryCount() => (modified & DELIVERY_COUNT) == DELIVERY_COUNT;

      public bool HasLinkCredit() => (modified & LINK_CREDIT) == LINK_CREDIT;

      public bool HasAvailable() => (modified & AVAILABLE) == AVAILABLE;

      public bool HasDrain() => (modified & DRAIN) == DRAIN;

      public bool HasEcho() => (modified & ECHO) == ECHO;

      public bool HasProperties() => (modified & PROPERTIES) == PROPERTIES;

      #endregion

      public object Clone() => new Flow(this);

      public Flow Copy() => new(this);

      public PerformativeType Type => PerformativeType.Flow;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, ushort channel, T context)
      {
         handler.HandleFlow(this, payload, channel, context);
      }

      public override string ToString()
      {
         return "Flow{" +
                "nextIncomingId=" + (HasNextIncomingId() ? nextIncomingId : "null") +
                ", incomingWindow=" + (HasIncomingWindow() ? incomingWindow : "null") +
                ", nextOutgoingId=" + (HasNextOutgoingId() ? nextOutgoingId : "null") +
                ", outgoingWindow=" + (HasOutgoingWindow() ? outgoingWindow : "null") +
                ", handle=" + (HasHandle() ? handle : "null") +
                ", deliveryCount=" + (HasDeliveryCount() ? deliveryCount : "null") +
                ", linkCredit=" + (HasLinkCredit() ? linkCredit : "null") +
                ", available=" + (HasAvailable() ? available : "null") +
                ", drain=" + (HasDrain() ? drain : "null") +
                ", echo=" + (HasEcho() ? echo : "null") +
                ", properties=" + properties +
                '}';
      }
   }
}