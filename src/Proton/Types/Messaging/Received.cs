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

using System.Collections.Generic;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class Received : IDeliveryState
   {
      public static readonly ulong DescriptorCode = 0x0000000000000023UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:received:list");

      public DeliveryStateType Type => DeliveryStateType.Received;

      public Received() : base()
      {
      }

      public Received(uint sectionNumber, ulong sectionOffset) : base()
      {
         SectionNumber = sectionNumber;
         SectionOffset = sectionOffset;
      }

      public uint SectionNumber { get; set; }

      public ulong SectionOffset { get; set; }

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
            return Equals((Received) other);
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
            Received other = (Received)state;

            if (other.SectionNumber != SectionNumber)
            {
               return false;
            }
            else if (other.SectionOffset != SectionOffset)
            {
               return false;
            }
            else
            {
               return true;
            }
         }
      }

      public override string ToString()
      {
         return "Received{" +
                "sectionNumber=" + SectionNumber +
                ", sectionOffset=" + SectionOffset +
                '}';
      }
   }
}
