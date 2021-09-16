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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   public enum DispositionField
   {
      Role,
      First,
      Last,
      Settled,
      State,
      Batchable
   }

   public sealed class Disposition : PerformativeDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:disposition:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000015ul;

      public Disposition() : base(Enum.GetNames(typeof(DispositionField)).Length)
      {
      }

      public Disposition(object described) : base(Enum.GetNames(typeof(DispositionField)).Length, (IList)described)
      {
      }

      public Disposition(IList described) : base(Enum.GetNames(typeof(DispositionField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Detach;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Role? Role
      {
         get => (Role?)List[((int)DispositionField.Role)];
         set => List[((int)DispositionField.Role)] = value == null ? null : value == Transport.Role.Receiver ? true : false;
      }

      public uint? First
      {
         get => (uint?)List[((int)DispositionField.First)];
         set => List[((int)DispositionField.First)] = value;
      }

      public uint? Last
      {
         get => (uint?)List[((int)DispositionField.Last)];
         set => List[((int)DispositionField.Last)] = value;
      }

      public bool? Settled
      {
         get => (bool?)List[((int)DispositionField.Settled)];
         set => List[((int)DispositionField.Settled)] = value;
      }

      public IDeliveryState State
      {
         get => (IDeliveryState)List[((int)DispositionField.State)];
         set => List[((int)DispositionField.State)] = value;
      }

      public bool? Batchable
      {
         get => (bool?)List[((int)DispositionField.Batchable)];
         set => List[((int)DispositionField.Batchable)] = value;
      }

      public override string ToString()
      {
         return "Disposition{" +
                "role=" + Role +
                ", first=" + First +
                ", last=" + Last +
                ", settled=" + Settled +
                ", state=" + State +
                ", batchable=" + Batchable +
                '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, byte[] payload, ushort channel, T context)
      {
         handler.HandleDisposition(frameSize, this, payload, channel, context);
      }
   }
}
