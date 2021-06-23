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
   public sealed class Modified : IDeliveryState
   {
      public static readonly ulong DescriptorCode = 0x0000000000000027UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:modified:list");

      public Modified() : base()
      {
      }

      public Modified(bool deliveryFailed) : this(deliveryFailed, false, null)
      {
      }

      public Modified(bool deliveryFailed, bool undeliverableHere) : this(deliveryFailed, undeliverableHere, null)
      {
      }

      public Modified(bool deliveryFailed, bool undeliverableHere, IDictionary<Symbol, object> annotations) : this()
      {
         DeliveryFailed = deliveryFailed;
         UndeliverableHere = undeliverableHere;
         MessageAnnotations = annotations;
      }

      public DeliveryStateType Type => DeliveryStateType.Modified;

      /// <summary>
      /// Did delivery failed for some reason.
      /// </summary>
      public bool DeliveryFailed { get; set; }

      /// <summary>
      /// Should the delivery be redeliverable on this link or routed elsewhere
      /// </summary>
      public bool UndeliverableHere { get; set; }

      /// <summary>
      /// Message annotations that should update those in the modified delivery.
      /// </summary>
      public IDictionary<Symbol, object> MessageAnnotations { get; set; }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public override bool Equals(object other)
      {
         return other == null ? false : other.GetType() == GetType();
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
            Modified other = (Modified)state;

            if (other.DeliveryFailed != DeliveryFailed)
            {
               return false;
            }
            else if (other.UndeliverableHere != UndeliverableHere)
            {
               return false;
            }
            else if (other.MessageAnnotations is null && MessageAnnotations is not null)
            {
               return false;
            }
            else
            {
               return other.MessageAnnotations.Equals(MessageAnnotations);
            }
         }
      }

      public override string ToString()
      {
         return "Modified{" +
                "deliveryFailed=" + DeliveryFailed +
                ", undeliverableHere=" + UndeliverableHere +
                ", messageAnnotations=" + MessageAnnotations +
                '}';
      }
   }
}
