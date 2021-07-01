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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Types.Transactions
{
   public sealed class TransactionalState : IDeliveryState
   {
      public static readonly ulong DescriptorCode = 0x0000000000000034UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:transactional-state:list");

      public TransactionalState() : base() { }

      public TransactionalState(TransactionalState other) : this()
      {
         TxnId = other.TxnId?.Copy();
         Outcome = (IOutcome)other.Outcome?.Clone();
      }

      public DeliveryStateType Type => DeliveryStateType.Transactional;

      public IProtonBuffer TxnId { get; set; }

      public IOutcome Outcome { get; set; }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public override bool Equals(object other)
      {
         if (other == null || other.GetType() == GetType())
         {
            return false;
         }
         else
         {
            return Equals((TransactionalState)other);
         }
      }

      public bool Equals(IDeliveryState state)
      {
         if (state == this)
         {
            return true;
         }
         else if (state is null)
         {
            return false;
         }
         else if (GetType() != state.GetType())
         {
            return false;
         }
         else
         {
            TransactionalState other = (TransactionalState)state;

            if ((other.TxnId is null && TxnId is not null) || (other.Outcome is null && Outcome is not null))
            {
               return false;
            }
            else
            {
               if (TxnId != null && !TxnId.Equals(other.TxnId))
               {
                  return false;
               }
               else if (Outcome != null && !Outcome.Equals(other.Outcome))
               {
                  return false;
               }
               else
               {
                  return true;
               }
            }
         }
      }

      public object Clone()
      {
         return new TransactionalState(this);
      }

      public override string ToString()
      {
         return "TransactionalState{" + "txnId=" + TxnId + ", outcome=" + Outcome + '}';
      }
   }
}