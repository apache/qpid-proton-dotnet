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
   public sealed class Begin : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000011UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:begin:list");

      private static readonly uint REMOTE_CHANNEL = 1;
      private static readonly uint NEXT_OUTGOING_ID = 2;
      private static readonly uint INCOMING_WINDOW = 4;
      private static readonly uint OUTGOING_WINDOW = 8;
      private static readonly uint HANDLE_MAX = 16;
      private static readonly uint OFFERED_CAPABILITIES = 32;
      private static readonly uint DESIRED_CAPABILITIES = 64;
      private static readonly uint PROPERTIES = 128;

      private uint modified = 0;

      private ushort remoteChannel;
      private uint nextOutgoingId;
      private uint incomingWindow;
      private uint outgoingWindow;
      private uint handleMax = UInt32.MaxValue;
      private Symbol[] offeredCapabilities;
      private Symbol[] desiredCapabilities;
      private IDictionary<Symbol, object> properties;

      public Begin() : base() { }

      public Begin(Begin other) : this()
      {
         remoteChannel = other.remoteChannel;
         nextOutgoingId = other.nextOutgoingId;
         incomingWindow = other.incomingWindow;
         outgoingWindow = other.outgoingWindow;
         handleMax = other.handleMax;
         offeredCapabilities = (Symbol[])(other.offeredCapabilities?.Clone());
         desiredCapabilities = (Symbol[])(other.desiredCapabilities?.Clone());

         if (other.properties != null)
         {
            properties = new Dictionary<Symbol, object>(other.properties);
         }

         modified = other.modified;
      }

      #region Element access

      public ushort RemoteChannel
      {
         get { return remoteChannel; }
         set
         {
            modified |= REMOTE_CHANNEL;
            remoteChannel = value;
         }
      }

      public uint NextOutgoingId
      {
         get { return nextOutgoingId; }
         set
         {
            modified |= NEXT_OUTGOING_ID;
            nextOutgoingId = value;
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

      public uint HandleMax
      {
         get { return handleMax; }
         set
         {
            modified |= HANDLE_MAX;
            handleMax = value;
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

      public bool HasRemoteChannel() => (modified & REMOTE_CHANNEL) == REMOTE_CHANNEL;

      public bool HasNextOutgoingId() => (modified & NEXT_OUTGOING_ID) == NEXT_OUTGOING_ID;

      public bool HasIncomingWindow() => (modified & INCOMING_WINDOW) == INCOMING_WINDOW;

      public bool HasOutgoingWindow() => (modified & OUTGOING_WINDOW) == OUTGOING_WINDOW;

      public bool HasHandleMax() => (modified & HANDLE_MAX) == HANDLE_MAX;

      public bool HasOfferedCapabilities() => (modified & OFFERED_CAPABILITIES) == OFFERED_CAPABILITIES;

      public bool HasDesiredCapabilities() => (modified & DESIRED_CAPABILITIES) == DESIRED_CAPABILITIES;

      public bool HasProperties() => (modified & PROPERTIES) == PROPERTIES;

      #endregion

      public object Clone()
      {
         return new Begin(this);
      }

      public Begin Copy()
      {
         return new Begin(this);
      }

      public PerformativeType Type => PerformativeType.Begin;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, int channel, T context)
      {
         handler.HandleBegin(this, payload, channel, context);
      }

      public override string ToString()
      {
        return "Begin{" +
               "remoteChannel=" + (HasRemoteChannel() ? remoteChannel : "null") +
               ", nextOutgoingId=" + (HasNextOutgoingId() ? nextOutgoingId : "null") +
               ", incomingWindow=" + (HasIncomingWindow() ? incomingWindow : "null") +
               ", outgoingWindow=" + (HasOutgoingWindow() ? outgoingWindow : "null") +
               ", handleMax=" + (HasHandleMax() ? handleMax : "null") +
               ", offeredCapabilities=" + (offeredCapabilities == null ? "null" : offeredCapabilities) +
               ", desiredCapabilities=" + (desiredCapabilities == null ? "null" : desiredCapabilities) +
               ", properties=" + properties +
               '}';
      }

      public override bool Equals(object obj)
      {
         return obj is Begin begin &&
                modified == begin.modified &&
                remoteChannel == begin.remoteChannel &&
                nextOutgoingId == begin.nextOutgoingId &&
                incomingWindow == begin.incomingWindow &&
                outgoingWindow == begin.outgoingWindow &&
                handleMax == begin.handleMax &&
                EqualityComparer<Symbol[]>.Default.Equals(offeredCapabilities, begin.offeredCapabilities) &&
                EqualityComparer<Symbol[]>.Default.Equals(desiredCapabilities, begin.desiredCapabilities) &&
                EqualityComparer<IDictionary<Symbol, object>>.Default.Equals(properties, begin.properties) &&
                RemoteChannel == begin.RemoteChannel &&
                NextOutgoingId == begin.NextOutgoingId &&
                IncomingWindow == begin.IncomingWindow &&
                OutgoingWindow == begin.OutgoingWindow &&
                HandleMax == begin.HandleMax &&
                EqualityComparer<Symbol[]>.Default.Equals(OfferedCapabilities, begin.OfferedCapabilities) &&
                EqualityComparer<Symbol[]>.Default.Equals(DesiredCapabilities, begin.DesiredCapabilities) &&
                EqualityComparer<IDictionary<Symbol, object>>.Default.Equals(Properties, begin.Properties) &&
                Type == begin.Type;
      }

      public override int GetHashCode()
      {
         HashCode hash = new HashCode();
         hash.Add(modified);
         hash.Add(remoteChannel);
         hash.Add(nextOutgoingId);
         hash.Add(incomingWindow);
         hash.Add(outgoingWindow);
         hash.Add(handleMax);
         hash.Add(offeredCapabilities);
         hash.Add(desiredCapabilities);
         hash.Add(properties);
         hash.Add(RemoteChannel);
         hash.Add(NextOutgoingId);
         hash.Add(IncomingWindow);
         hash.Add(OutgoingWindow);
         hash.Add(HandleMax);
         hash.Add(OfferedCapabilities);
         hash.Add(DesiredCapabilities);
         hash.Add(Properties);
         hash.Add(Type);
         return hash.ToHashCode();
      }
   }
}