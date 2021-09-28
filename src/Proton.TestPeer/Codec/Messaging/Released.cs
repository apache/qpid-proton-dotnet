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
   public class Released : ListDescribedType, IDeliveryState, IOutcome
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:released:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000026L;

      private static readonly Released INSTANCE = new Released();

      public Released() : base(0)
      {
      }

      public Released(object described) : base(0, (IList)described)
      {
      }

      public Released(IList described) : base(0, described)
      {
      }

      public DeliveryStateType Type => DeliveryStateType.Released;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Released Instance => INSTANCE;

      public override bool Equals(object obj)
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

         Released other = (Released)obj;
         if (other.GetHighestSetFieldId() != GetHighestSetFieldId())
         {
            return false;
         }

         return true;
      }

      public override int GetHashCode()
      {
         return Descriptor.GetHashCode();
      }
   }
}