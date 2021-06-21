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
   public sealed class Transfer : IPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000014UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:transfer:list");

      private static readonly uint HANDLE = 1;
      private static readonly uint DELIVERY_ID = 2;
      private static readonly uint DELIVERY_TAG = 4;
      private static readonly uint MESSAGE_FORMAT = 8;
      private static readonly uint SETTLED = 16;
      private static readonly uint MORE = 32;
      private static readonly uint RCV_SETTLE_MODE = 64;
      private static readonly uint STATE = 128;
      private static readonly uint RESUME = 256;
      private static readonly uint ABORTED = 512;
      private static readonly uint BATCHABLE = 1024;

      private uint modified = 0;

      private uint handle;
      private uint deliveryId;
      private IDeliveryTag deliveryTag;
      private uint messageFormat;
      private bool settled;
      private bool more;
      private ReceiverSettleMode? rcvSettleMode;
      private IDeliveryState state;
      private bool resume;
      private bool aborted;
      private bool batchable;

      public Transfer() : base() { }

      public Transfer(Transfer other) : this()
      {
         handle = other.handle;
         deliveryId = other.deliveryId;
         deliveryTag = (IDeliveryTag)(other.deliveryTag?.Clone());
         messageFormat = other.messageFormat;
         settled = other.settled;
         more = other.more;
         rcvSettleMode = other.rcvSettleMode;
         state = other.state;
         resume = other.resume;
         aborted = other.aborted;
         batchable = other.batchable;

         modified = other.modified;
      }

      public Transfer Reset()
      {
         modified = 0;
         handle = 0;
         deliveryId = 0;
         deliveryTag = null;
         messageFormat = 0;
         settled = false;
         more = false;
         rcvSettleMode = null;
         state = null;
         resume = false;
         aborted = false;
         batchable = false;

         return this;
      }

      public Transfer ClearHandle()
      {
         modified &= ~HANDLE;
         handle = 0;
         return this;
      }

      public Transfer ClearDeliveryId()
      {
         modified &= ~DELIVERY_ID;
         deliveryId = 0;
         return this;
      }

      public Transfer ClearDeliveryTag()
      {
         modified &= ~DELIVERY_TAG;
         deliveryTag = null;
         return this;
      }

      public Transfer ClearMessageFormat()
      {
         modified &= ~MESSAGE_FORMAT;
         messageFormat = 0;
         return this;
      }

      public Transfer ClearSettled()
      {
         modified &= ~SETTLED;
         settled = false;
         return this;
      }

      public Transfer ClearMore()
      {
         modified &= ~MORE;
         more = false;
         return this;
      }

      public Transfer ClearReceiverSettleMode()
      {
         modified &= ~RCV_SETTLE_MODE;
         rcvSettleMode = null;
         return this;
      }

      public Transfer ClearState()
      {
         modified &= ~STATE;
         state = null;
         return this;
      }

      public Transfer ClearResume()
      {
         modified &= ~RESUME;
         resume = false;
         return this;
      }

      public Transfer ClearAborted()
      {
         modified &= ~ABORTED;
         aborted = false;
         return this;
      }

      public Transfer ClearBatchable()
      {
         modified &= ~BATCHABLE;
         batchable = false;
         return this;
      }

      #region Element access

      public uint Handle
      {
         get { return handle; }
         set
         {
            modified |= HANDLE;
            handle = value;
         }
      }

      public uint DeliveryId
      {
         get { return deliveryId; }
         set
         {
            modified |= DELIVERY_ID;
            deliveryId = value;
         }
      }

      public IDeliveryTag DeliveryTag
      {
         get { return deliveryTag; }
         set
         {
            modified |= DELIVERY_TAG;
            deliveryTag = value;
         }
      }

      public uint MessageFormat
      {
         get { return messageFormat; }
         set
         {
            modified |= MESSAGE_FORMAT;
            messageFormat = value;
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

      public bool More
      {
         get { return more; }
         set
         {
            modified |= MORE;
            more = value;
         }
      }

      public ReceiverSettleMode? ReceiverSettleMode
      {
         get { return rcvSettleMode; }
         set
         {
            modified |= RCV_SETTLE_MODE;
            rcvSettleMode = value;
         }
      }

      public IDeliveryState DeliveryState
      {
         get { return state; }
         set
         {
            modified |= STATE;
            state = value;
         }
      }

      public bool Resume
      {
         get { return resume; }
         set
         {
            modified |= RESUME;
            resume = value;
         }
      }

      public bool Aborted
      {
         get { return aborted; }
         set
         {
            modified |= ABORTED;
            aborted = value;
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

      public bool HasHandle() => (modified & HANDLE) == HANDLE;

      public bool HasDeliveryId() => (modified & DELIVERY_ID) == DELIVERY_ID;

      public bool HasDeliveryTag() => (modified & DELIVERY_TAG) == DELIVERY_TAG;

      public bool HasMessageFormat() => (modified & MESSAGE_FORMAT) == MESSAGE_FORMAT;

      public bool HasSettled() => (modified & SETTLED) == SETTLED;

      public bool HasMore() => (modified & MORE) == MORE;

      public bool HasReceiverSettleMode() => (modified & RCV_SETTLE_MODE) == RCV_SETTLE_MODE;

      public bool HasState() => (modified & STATE) == STATE;

      public bool HasResume() => (modified & RESUME) == RESUME;

      public bool HasAborted() => (modified & ABORTED) == ABORTED;

      public bool HasBatchable() => (modified & BATCHABLE) == BATCHABLE;

      #endregion

      public object Clone()
      {
         return new Transfer(this);
      }

      public PerformativeType Type => PerformativeType.Transfer;

      public void Invoke<T>(IPerformativeHandler<T> handler, IProtonBuffer payload, int channel, T context)
      {
         handler.HandleTransfer(this, payload, channel, context);
      }

      public new string ToString()
      {
         return "Transfer{" +
                "handle=" + (HasHandle() ? handle : "null") +
                ", deliveryId=" + (HasDeliveryId() ? deliveryId : "null") +
                ", deliveryTag=" + (HasDeliveryTag() ? deliveryTag : "null") +
                ", messageFormat=" + (HasMessageFormat() ? messageFormat : "null") +
                ", settled=" + (HasSettled() ? settled : "null") +
                ", more=" + (HasMore() ? more : "null") +
                ", rcvSettleMode=" + (HasReceiverSettleMode() ? rcvSettleMode : "null") +
                ", state=" + (HasState() ? state : "null") +
                ", resume=" + (HasResume() ? resume : "null") +
                ", aborted=" + (HasAborted() ? aborted : "null") +
                ", batchable=" + (HasBatchable() ? batchable : "null") +
                '}';
      }
   }
}