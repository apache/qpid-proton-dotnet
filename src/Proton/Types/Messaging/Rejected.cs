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

using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Types.Messaging
{
   public sealed class Rejected : IOutcome, IDeliveryState
   {
      public static readonly ulong DescriptorCode = 0x0000000000000025UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:rejected:list");

      public DeliveryStateType Type => DeliveryStateType.Rejected;

      public Rejected() : base()
      {
      }

      public Rejected(ErrorCondition error) : this()
      {
         Error = error;
      }

      /// <summary>
      /// Provides an error condition that defines the reason for the rejection.
      /// </summary>
      public ErrorCondition Error { get; set; }

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
            return Equals((Rejected)other);
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
            Rejected other = (Rejected)state;

            if (other.Error is null && Error is not null)
            {
               return false;
            }
            else if (Error != null)
            {
               return Error.Equals(other.Error);
            }
            else
            {
               return true;
            }
         }
      }

      public override string ToString()
      {
         return "Rejected{" + "error=" + Error + "}";
      }

      public object Clone()
      {
         return this.MemberwiseClone();
      }
   }
}