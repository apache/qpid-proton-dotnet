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

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class Open : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000010UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:open:list");

      private static readonly uint CONTAINER_ID = 1;
      private static readonly uint HOSTNAME = 2;
      private static readonly uint MAX_FRAME_SIZE = 4;
      private static readonly uint CHANNEL_MAX = 8;
      private static readonly uint IDLE_TIMEOUT = 16;
      private static readonly uint OUTGOING_LOCALES = 32;
      private static readonly uint INCOMING_LOCALES = 64;
      private static readonly uint OFFERED_CAPABILITIES = 128;
      private static readonly uint DESIRED_CAPABILITIES = 256;
      private static readonly uint PROPERTIES = 512;

      private uint modified = CONTAINER_ID;

      private string containerId = "";
      private string hostname;
      private uint maxFrameSize = UInt32.MaxValue;
      private ushort channelMax = UInt16.MaxValue;
      private uint idleTimeout;
      private Symbol[] outgoingLocales;
      private Symbol[] incomingLocales;
      private Symbol[] offeredCapabilities;
      private Symbol[] desiredCapabilities;
      private IDictionary<Symbol, object> properties;

      public Open() : base() { }

      public Open(Open other) : this()
      {
         containerId = other.containerId;
         hostname = other.hostname;
         maxFrameSize = other.maxFrameSize;
         channelMax = other.channelMax;
         idleTimeout = other.idleTimeout;
         outgoingLocales = other.outgoingLocales;
         incomingLocales = other.incomingLocales;
         offeredCapabilities = other.offeredCapabilities;
         desiredCapabilities = other.desiredCapabilities;
         if (other.properties != null)
         {
            properties = new Dictionary<Symbol, object>(other.properties);
         }

         modified = other.modified;
      }

      #region Element access

      public string ContainerId
      {
         get { return containerId; }
         set
         {
            modified |= CONTAINER_ID;
            containerId = value ?? throw new NullReferenceException("the container-id field is mandatory");
         }
      }

      public string Hostname
      {
         get { return hostname; }
         set
         {
            if (value != null)
            {
               modified |= HOSTNAME;
            }
            else
            {
               modified &= ~HOSTNAME;
            }

            hostname = value;
         }
      }

      public uint MaxFrameSize
      {
         get { return maxFrameSize; }
         set
         {
            modified |= MAX_FRAME_SIZE;
            maxFrameSize = value;
         }
      }

      public ushort ChannelMax
      {
         get { return channelMax; }
         set
         {
            modified |= CHANNEL_MAX;
            channelMax = value;
         }
      }

      public uint IdleTimeout
      {
         get { return idleTimeout; }
         set
         {
            modified |= IDLE_TIMEOUT;
            idleTimeout = value;
         }
      }

      public Symbol[] OutgoingLocales
      {
         get { return outgoingLocales; }
         set
         {
            if (value != null)
            {
               modified |= OUTGOING_LOCALES;
            }
            else
            {
               modified &= ~OUTGOING_LOCALES;
            }

            outgoingLocales = value;
         }
      }

      public Symbol[] IncomingLocales
      {
         get { return incomingLocales; }
         set
         {
            if (value != null)
            {
               modified |= INCOMING_LOCALES;
            }
            else
            {
               modified &= ~INCOMING_LOCALES;
            }

            incomingLocales = value;
         }
      }

      public Symbol[] OfferedCapabilities
      {
         get { return offeredCapabilities; }
         set
         {
            if (value != null)
            {
               modified |= OFFERED_CAPABILITIES;
            }
            else
            {
               modified &= ~OFFERED_CAPABILITIES;
            }

            offeredCapabilities = value;
         }
      }

      public Symbol[] DesiredCapabilities
      {
         get { return desiredCapabilities; }
         set
         {
            if (value != null)
            {
               modified |= DESIRED_CAPABILITIES;
            }
            else
            {
               modified &= ~DESIRED_CAPABILITIES;
            }

            desiredCapabilities = value;
         }
      }

      public IDictionary<Symbol, object> Properties
      {
         get { return properties; }
         set
         {
            if (value != null)
            {
               modified |= PROPERTIES;
            }
            else
            {
               modified &= ~PROPERTIES;
            }

            properties = value;
         }
      }

      #endregion

      #region Element count and value presence utility

      public bool IsEmpty() => modified == 0;

      public int GetElementCount() => 32 - BitOperations.LeadingZeroCount(modified);

      public bool HasContainerId() => (modified & CONTAINER_ID) == CONTAINER_ID;

      public bool HasHostname() => (modified & HOSTNAME) == HOSTNAME;

      public bool HasMaxFrameSize() => (modified & MAX_FRAME_SIZE) == MAX_FRAME_SIZE;

      public bool HasChannelMax() => (modified & CHANNEL_MAX) == CHANNEL_MAX;

      public bool HasIdleTimeout() => (modified & IDLE_TIMEOUT) == IDLE_TIMEOUT;

      public bool HasOutgoingLocales() => (modified & OUTGOING_LOCALES) == OUTGOING_LOCALES;

      public bool HasIncomingLocales() => (modified & INCOMING_LOCALES) == INCOMING_LOCALES;

      public bool HasOfferedCapabilities() => (modified & OFFERED_CAPABILITIES) == OFFERED_CAPABILITIES;

      public bool HasDesiredCapabilities() => (modified & DESIRED_CAPABILITIES) == DESIRED_CAPABILITIES;

      public bool HasProperties() => (modified & PROPERTIES) == PROPERTIES;

      #endregion

      public object Clone() => new Open(this);

      public Open Copy() => new Open(this);

      public PerformativeType Type => PerformativeType.Open;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, ushort channel, T context)
      {
         handler.HandleOpen(this, payload, channel, context);
      }

      public override string ToString()
      {
         return "Open{" +
                " containerId='" + containerId + '\'' +
                ", hostname='" + hostname + '\'' +
                ", maxFrameSize=" + (HasMaxFrameSize() ? maxFrameSize : "null") +
                ", channelMax=" + (HasChannelMax() ? channelMax : "null") +
                ", idleTimeOut=" + (HasIdleTimeout() ? idleTimeout : "null") +
                ", outgoingLocales=" + (outgoingLocales == null ? "null" : outgoingLocales) +
                ", incomingLocales=" + (incomingLocales == null ? "null" : incomingLocales) +
                ", offeredCapabilities=" + (offeredCapabilities == null ? "null" : offeredCapabilities) +
                ", desiredCapabilities=" + (desiredCapabilities == null ? "null" : desiredCapabilities) +
                ", properties=" + properties +
                '}';
      }
   }
}