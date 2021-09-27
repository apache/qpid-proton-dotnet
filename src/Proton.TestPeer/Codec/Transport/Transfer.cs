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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transport
{
   public enum TransferField
   {
      Handle,
      DeliveryId,
      DeliveryTag,
      MessageFormat,
      Settled,
      More,
      ReceiverSettleMode,
      State,
      Resume,
      Aborted,
      Batchable
   }

   public sealed class Transfer : PerformativeDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:transfer:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000014ul;

      public Transfer() : base(Enum.GetNames(typeof(TransferField)).Length)
      {
      }

      public Transfer(object described) : base(Enum.GetNames(typeof(TransferField)).Length, (IList)described)
      {
      }

      public Transfer(IList described) : base(Enum.GetNames(typeof(TransferField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Transfer;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public uint? Handle
      {
         get => (uint?)List[((int)TransferField.Handle)];
         set => List[((int)TransferField.Handle)] = value;
      }

      public uint? DeliveryId
      {
         get => (uint?)List[((int)TransferField.DeliveryId)];
         set => List[((int)TransferField.DeliveryId)] = value;
      }

      public Binary DeliveryTag
      {
         get => (Binary)List[((int)TransferField.DeliveryTag)];
         set => List[((int)TransferField.DeliveryTag)] = value;
      }

      public uint? MessageFormat
      {
         get => (uint?)List[((int)TransferField.MessageFormat)];
         set => List[((int)TransferField.MessageFormat)] = value;
      }

      public bool? Settled
      {
         get => (bool?)List[((int)TransferField.Settled)];
         set => List[((int)TransferField.Settled)] = value;
      }

      public bool? More
      {
         get => (bool?)List[((int)TransferField.More)];
         set => List[((int)TransferField.More)] = value;
      }

      public ReceiverSettleMode? ReceiverSettleMode
      {
         get => (ReceiverSettleMode?)List[((int)TransferField.ReceiverSettleMode)];
         set => List[((int)TransferField.ReceiverSettleMode)] = ((byte?)value);
      }

      public IDeliveryState State
      {
         get => (IDeliveryState)List[((int)TransferField.State)];
         set => List[((int)TransferField.State)] = value;
      }

      public bool? Resume
      {
         get => (bool?)List[((int)TransferField.Resume)];
         set => List[((int)TransferField.Resume)] = value;
      }

      public bool? Aborted
      {
         get => (bool?)List[((int)TransferField.Aborted)];
         set => List[((int)TransferField.Aborted)] = value;
      }

      public bool? Batchable
      {
         get => (bool?)List[((int)TransferField.Batchable)];
         set => List[((int)TransferField.Batchable)] = value;
      }

      public override string ToString()
      {
         return "Transfer{" +
                "handle=" + Handle +
                ", deliveryId=" + DeliveryId +
                ", deliveryTag=" + DeliveryTag +
                ", messageFormat=" + MessageFormat +
                ", settled=" + Settled +
                ", more=" + More +
                ", rcvSettleMode=" + ReceiverSettleMode +
                ", state=" + State +
                ", resume=" + Resume +
                ", aborted=" + Aborted +
                ", batchable=" + Batchable +
                '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, byte[] payload, ushort channel, T context)
      {
         handler.HandleTransfer(frameSize, this, payload, channel, context);
      }
   }
}