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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Messaging
{
   public enum HeaderField
   {
      DURABLE,
      PRIORITY,
      TTL,
      FIRST_ACQUIRER,
      DELIVERY_COUNT,
   }

   public sealed class Header : ListDescribedType
   {
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000070UL;
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:header:list");

      public Header() : base(Enum.GetNames(typeof(HeaderField)).Length)
      {
      }

      public Header(object described) : base(Enum.GetNames(typeof(HeaderField)).Length, (IList)described)
      {
      }

      public Header(IList described) : base(Enum.GetNames(typeof(HeaderField)).Length, described)
      {
      }

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public bool? Durable
      {
         get => (bool)List[((int)HeaderField.DURABLE)];
         set => List[((int)HeaderField.DURABLE)] = value;
      }

      public byte? Priority
      {
         get => (byte)List[((int)HeaderField.PRIORITY)];
         set => List[((int)HeaderField.PRIORITY)] = value;
      }

      public uint? Ttl
      {
         get => (uint)List[((int)HeaderField.TTL)];
         set => List[((int)HeaderField.TTL)] = value;
      }

      public bool? FirstAcquirer
      {
         get => (bool)List[((int)HeaderField.FIRST_ACQUIRER)];
         set => List[((int)HeaderField.FIRST_ACQUIRER)] = value;
      }

      public uint? DeliveryCount
      {
         get => (uint)List[((int)HeaderField.DELIVERY_COUNT)];
         set => List[((int)HeaderField.DELIVERY_COUNT)] = value;
      }

      public override string ToString()
      {
         return "Header{ " +
                 "durable=" + Durable +
                 ", priority=" + Priority +
                 ", ttl=" + Ttl +
                 ", firstAcquirer=" + FirstAcquirer +
                 ", deliveryCount=" + DeliveryCount +
                 " }";
      }
   }
}