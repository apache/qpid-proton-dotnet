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
   public enum ReceivedField : uint
   {
      SectionNumber,
      SectionOffset
   }

   public sealed class Received : ListDescribedType, IDeliveryState, IOutcome
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new("amqp:received:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000023UL;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Received() : base(Enum.GetNames(typeof(ReceivedField)).Length)
      {
      }

      public Received(object described) : base(Enum.GetNames(typeof(ReceivedField)).Length, (IList)described)
      {
      }

      public Received(IList described) : base(Enum.GetNames(typeof(ReceivedField)).Length, described)
      {
      }

      public uint? SectionNumber
      {
         get => (uint?)List[((int)ReceivedField.SectionNumber)];
         set => List[((int)ReceivedField.SectionNumber)] = value;
      }

      public ulong? SectionOffset
      {
         get => (ulong?)List[((int)ReceivedField.SectionOffset)];
         set => List[((int)ReceivedField.SectionOffset)] = value;
      }

      public DeliveryStateType Type => DeliveryStateType.Received;

      public override string ToString()
      {
         return "Received{" +
                "sectionNumber=" + SectionNumber +
                ", sectionOffset=" + SectionOffset +
                '}';
      }
   }
}