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
   public enum RejectedField
   {
      Error
   }

   public sealed class Rejected : ListDescribedType, IDeliveryState, IOutcome
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:rejected:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000025UL;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public DeliveryStateType Type => DeliveryStateType.Rejected;

      public Rejected() : base(Enum.GetNames(typeof(RejectedField)).Length)
      {
      }

      public Rejected(ErrorCondition error) : base(Enum.GetNames(typeof(RejectedField)).Length)
      {
         Error = error;
      }

      public Rejected(object described) : base(Enum.GetNames(typeof(RejectedField)).Length, (IList)described)
      {
      }

      public Rejected(IList described) : base(Enum.GetNames(typeof(RejectedField)).Length, described)
      {
      }

      public ErrorCondition Error
      {
         get => (ErrorCondition)List[((int)RejectedField.Error)];
         set => List[((int)RejectedField.Error)] = value;
      }

      public override bool Equals(Object obj)
      {
         if (obj == this)
         {
            return true;
         }

         if (!(obj is IDescribedType))
         {
            return false;
         }

         DescribedType d = (DescribedType)obj;
         if (!(DESCRIPTOR_CODE.Equals(d.Descriptor) || DESCRIPTOR_SYMBOL.Equals(d.Descriptor)))
         {
            return false;
         }

         object described = Described;
         object described2 = d.Described;
         if (described == null)
         {
            return described2 == null;
         }
         else
         {
            return described.Equals(described2);
         }
      }

      public override int GetHashCode()
      {
         return DESCRIPTOR_SYMBOL.GetHashCode();
      }

      public override string ToString()
      {
         return "Rejected{" + "error=" + Error + "}";
      }
   }
}