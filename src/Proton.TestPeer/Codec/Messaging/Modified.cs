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
using System.Collections;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Messaging
{
   public enum ModifiedField : int
   {
      DELIVERY_FAILED,
      UNDELIVERABLE_HERE,
      MESSAGE_ANNOTATIONS
   }

   public sealed class Modified : ListDescribedType, IDeliveryState, IOutcome
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:modified:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000027UL;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public DeliveryStateType Type => DeliveryStateType.Modified;

      public Modified() : base(Enum.GetNames(typeof(ModifiedField)).Length)
      {
      }

      public Modified(bool deliveryFailed) : base(Enum.GetNames(typeof(ModifiedField)).Length)
      {
         DeliveryFailed = deliveryFailed;
      }

      public Modified(bool deliveryFailed, bool undeliverableHere) : base(Enum.GetNames(typeof(ModifiedField)).Length)
      {
         DeliveryFailed = deliveryFailed;
         UndeliverableHere = undeliverableHere;
      }

      public Modified(Object described) : base(Enum.GetNames(typeof(ModifiedField)).Length, (IList)described)
      {
      }

      public Modified(IList described) : base(Enum.GetNames(typeof(ModifiedField)).Length, described)
      {
      }

      public bool? DeliveryFailed
      {
         get => (bool?)List[((int)ModifiedField.DELIVERY_FAILED)];
         set => List[((int)ModifiedField.DELIVERY_FAILED)] = value;
      }

      public bool? UndeliverableHere
      {
         get => (bool?)List[((int)ModifiedField.UNDELIVERABLE_HERE)];
         set => List[((int)ModifiedField.UNDELIVERABLE_HERE)] = value;
      }

      public IDictionary MessageAnnotations
      {
         get => (IDictionary)List[((int)ModifiedField.MESSAGE_ANNOTATIONS)];
         set => List[((int)ModifiedField.MESSAGE_ANNOTATIONS)] = (IDictionary)value;
      }

      public override bool Equals(Object obj)
      {
         if (obj == this)
         {
            return true;
         }

         if (!(obj is IDescribedType)) {
            return false;
         }

         IDescribedType d = (IDescribedType)obj;
         if (!(DESCRIPTOR_CODE.Equals(d.Descriptor) || DESCRIPTOR_SYMBOL.Equals(d.Descriptor)))
         {
            return false;
         }

         if (d is Modified other)
         {
            if (UndeliverableHere != other.UndeliverableHere || DeliveryFailed != other.DeliveryFailed)
            {
               return false;
            }
            else if (MessageAnnotations != null)
            {
               return MessageAnnotations.Equals(other.MessageAnnotations);
            }
            else if (other.MessageAnnotations != null)
            {
               return other.MessageAnnotations.Equals(MessageAnnotations);
            }
            else
            {
               return true;
            }
         }

         return false;
      }

      public override int GetHashCode()
      {
         return Descriptor.GetHashCode();
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
