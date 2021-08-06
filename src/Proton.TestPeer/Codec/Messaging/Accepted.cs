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
   public class Accepted : ListDescribedType, IDeliveryState, IOutcome
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:accepted:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000024UL;

      private static readonly Accepted INSTANCE = new Accepted();

      public Accepted() : base(0)
      {
      }

      public Accepted(object described) : base(0, (IList)described)
      {
      }

      public Accepted(IList described) : base(0, described)
      {
      }

      public DeliveryStateType Type => DeliveryStateType.Accepted;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Accepted Instance => INSTANCE;

      public override bool Equals(object obj)
      {
         if (obj == this)
         {
            return true;
         }

         if (!(obj is DescribedType)) {
            return false;
         }

         DescribedType d = (DescribedType)obj;
         if (!(DESCRIPTOR_CODE.Equals(d.Descriptor) || DESCRIPTOR_SYMBOL.Equals(d.Descriptor)))
         {
            return false;
         }

         Object described = Described;
         Object described2 = d.Described;
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
         return Descriptor.GetHashCode();
      }
   }
}