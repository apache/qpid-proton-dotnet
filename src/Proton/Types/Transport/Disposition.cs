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

namespace Apache.Qpid.Proton.Types.Transport
{
   public sealed class Disposition : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000015UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:disposition:list");

      private static readonly uint ROLE = 1;
      private static readonly uint FIRST = 2;
      private static readonly uint LAST = 4;
      private static readonly uint SETTLED = 8;
      private static readonly uint STATE = 16;
      private static readonly uint BATCHABLE = 32;

      private uint modified = 0;

      private Role role = Role.Sender;
      private uint first;
      private uint last;
      private bool settled;
      private IDeliveryState state;
      private bool batchable;

      public Disposition() : base() { }

      public Disposition(Disposition other) : this()
      {
         role = other.role;
         first = other.first;
         last = other.last;
         settled = other.settled;
         state = other.state;
         batchable = other.batchable;

         modified = other.modified;
      }

      public Disposition Reset()
      {
         modified = 0;
         role = Role.Sender;
         first = 0;
         last = 0;
         settled = false;
         state = null;
         batchable = false;

         return this;
      }

      public Disposition ClearRole()
      {
         modified &= ~ROLE;
         role = Role.Sender;
         return this;
      }

      public Disposition ClearFirst()
      {
         modified &= ~FIRST;
         first = 0;
         return this;
      }

      public Disposition ClearLast()
      {
         modified &= ~LAST;
         last = 0;
         return this;
      }

      public Disposition ClearSettled()
      {
         modified &= ~SETTLED;
         settled = false;
         return this;
      }

      public Disposition ClearState()
      {
         modified &= ~STATE;
         state = null;
         return this;
      }

      public Disposition ClearBatchable()
      {
         modified &= ~BATCHABLE;
         batchable = false;
         return this;
      }

      #region Element access

      public Role Role
      {
         get { return role; }
         set
         {
            modified |= ROLE;
            role = value;
         }
      }

      public uint First
      {
         get { return first; }
         set
         {
            modified |= FIRST;
            first = value;
         }
      }

      public uint Last
      {
         get { return last; }
         set
         {
            modified |= LAST;
            last = value;
         }
      }

      public bool Settled
      {
         get { return settled; }
         set
         {
            modified |= SETTLED;
            settled = value;
         }
      }

      public IDeliveryState State
      {
         get { return state; }
         set
         {
            modified |= STATE;
            state = value;
         }
      }

      public bool Batchable
      {
         get { return batchable; }
         set
         {
            modified |= BATCHABLE;
            batchable = value;
         }
      }

      #endregion

      #region Element count and value presence utility

      public bool IsEmpty() => modified == 0;

      public int GetElementCount() => 32 - BitOperations.LeadingZeroCount(modified);

      public bool HasRole() => (modified & ROLE) == ROLE;

      public bool HasFirst() => (modified & FIRST) == FIRST;

      public bool HasLast() => (modified & LAST) == LAST;

      public bool HasSettled() => (modified & SETTLED) == SETTLED;

      public bool HasState() => (modified & STATE) == STATE;

      public bool HasBatchable() => (modified & BATCHABLE) == BATCHABLE;

      #endregion

      public object Clone()
      {
         return new Disposition(this);
      }

      public PerformativeType Type => PerformativeType.Disposition;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, int channel, T context)
      {
         handler.HandleDisposition(this, payload, channel, context);
      }

      public new string ToString()
      {
         return "Disposition{" +
                "role=" + role +
                ", first=" + first +
                ", last=" + last +
                ", settled=" + settled +
                ", state=" + state +
                ", batchable=" + batchable +
                '}';
      }
   }
}