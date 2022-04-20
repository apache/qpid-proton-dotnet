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

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class Header : ICloneable, ISection
   {
      public static readonly ulong DescriptorCode = 0x0000000000000070UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:header:list");

      public static readonly bool DEFAULT_DURABILITY = false;
      public static readonly byte DEFAULT_PRIORITY = 4;
      public static readonly uint DEFAULT_TIME_TO_LIVE = uint.MaxValue;
      public static readonly bool DEFAULT_FIRST_ACQUIRER = false;
      public static readonly uint DEFAULT_DELIVERY_COUNT = 0;

      private static readonly uint DURABLE = 1;
      private static readonly uint PRIORITY = 2;
      private static readonly uint TIME_TO_LIVE = 4;
      private static readonly uint FIRST_ACQUIRER = 8;
      private static readonly uint DELIVERY_COUNT = 16;

      private uint modified = 0;

      private bool durable = DEFAULT_DURABILITY;
      private byte priority = DEFAULT_PRIORITY;
      private uint timeToLive = DEFAULT_TIME_TO_LIVE;
      private bool firstAcquirer = DEFAULT_FIRST_ACQUIRER;
      private uint deliveryCount = DEFAULT_DELIVERY_COUNT;

      public Header() : base()
      {
      }

      public Header(Header other) : this()
      {
         durable = other.durable;
         priority = other.priority;
         timeToLive = other.timeToLive;
         firstAcquirer = other.firstAcquirer;
         deliveryCount = other.deliveryCount;
         modified = other.modified;
      }

      public object Clone()
      {
         return Copy();
      }

      public Header Copy()
      {
         return new Header(this);
      }

      public Header ClearDurable()
      {
         modified &= ~DURABLE;
         durable = DEFAULT_DURABILITY;
         return this;
      }

      public Header ClearPriority()
      {
         modified &= ~PRIORITY;
         priority = DEFAULT_PRIORITY;
         return this;
      }

      public Header ClearTimeToLive()
      {
         modified &= ~TIME_TO_LIVE;
         timeToLive = DEFAULT_TIME_TO_LIVE;
         return this;
      }

      public Header ClearFirstAcquirer()
      {
         modified &= ~FIRST_ACQUIRER;
         firstAcquirer = DEFAULT_FIRST_ACQUIRER;
         return this;
      }

      public Header ClearDeliveryCount()
      {
         modified &= ~DELIVERY_COUNT;
         deliveryCount = DEFAULT_DELIVERY_COUNT;
         return this;
      }

      public Header Reset()
      {
         modified = 0;
         durable = DEFAULT_DURABILITY;
         priority = DEFAULT_PRIORITY;
         timeToLive = DEFAULT_TIME_TO_LIVE;
         firstAcquirer = DEFAULT_FIRST_ACQUIRER;
         deliveryCount = DEFAULT_DELIVERY_COUNT;
         return this;
      }

      #region Element access

      public bool Durable
      {
         get { return durable; }
         set
         {
            if (value)
            {
               modified |= DURABLE;
            }
            else
            {
               modified &= ~DURABLE;
            }

            durable = value;
         }
      }

      public byte Priority
      {
         get { return priority; }
         set
         {
            if (value == DEFAULT_PRIORITY)
            {
               modified &= ~PRIORITY;
            }
            else
            {
               modified |= PRIORITY;
            }

            priority = value;
         }
      }

      public uint TimeToLive
      {
         get { return timeToLive; }
         set
         {
            modified |= TIME_TO_LIVE;
            timeToLive = value;
         }
      }

      public bool FirstAcquirer
      {
         get { return firstAcquirer; }
         set
         {
            if (value)
            {
               modified |= FIRST_ACQUIRER;
            }
            else
            {
               modified &= ~FIRST_ACQUIRER;
            }

            firstAcquirer = value;
         }
      }

      public uint DeliveryCount
      {
         get { return deliveryCount; }
         set
         {
            if (value == DEFAULT_DELIVERY_COUNT)
            {
               modified &= ~DELIVERY_COUNT;
            }
            else
            {
               modified |= DELIVERY_COUNT;
            }
            deliveryCount = value;
         }
      }

      #endregion

      #region Element count and value presence utility

      public bool IsEmpty() => modified == 0;

      public int GetElementCount() => 32 - BitOperations.LeadingZeroCount(modified);

      public bool HasDurable() => (modified & DURABLE) == DURABLE;

      public bool HasPriority() => (modified & PRIORITY) == PRIORITY;

      public bool HasTimeToLive() => (modified & TIME_TO_LIVE) == TIME_TO_LIVE;

      public bool HasDeliveryCount() => (modified & DELIVERY_COUNT) == DELIVERY_COUNT;

      public bool HasFirstAcquirer() => (modified & FIRST_ACQUIRER) == FIRST_ACQUIRER;

      #endregion

      public SectionType Type => SectionType.Header;

      public object Value => this;

      public override string ToString()
      {
         return "Header{ " +
                "durable=" + (HasDurable() ? durable : "null") +
                ", priority=" + (HasPriority() ? priority : "null") +
                ", ttl=" + (HasTimeToLive() ? timeToLive : "null") +
                ", firstAcquirer=" + (HasFirstAcquirer() ? firstAcquirer : "null") +
                ", deliveryCount=" + (HasDeliveryCount() ? deliveryCount : "null") +
                " }";
      }
   }
}