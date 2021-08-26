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
   public enum FlowField
   {
      NextIncomingId,
      IncomingWindow,
      NextOutgoingId,
      OutgoingWindow,
      Handle,
      DeliveryCount,
      LinkCredit,
      Available,
      Drain,
      Echo,
      Properties,
   }

   public sealed class Flow : PerformativeDescribedType
   {
      public static Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:flow:list");
      public static ulong DESCRIPTOR_CODE = 0x0000000000000013ul;

      public Flow() : base(Enum.GetNames(typeof(FlowField)).Length)
      {
      }

      public Flow(object described) : base(Enum.GetNames(typeof(FlowField)).Length, (IList)described)
      {
      }

      public Flow(IList described) : base(Enum.GetNames(typeof(FlowField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Flow;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public uint? NextIncomingId
      {
         get => (uint?)List[((int)FlowField.NextIncomingId)];
         set => List[((int)FlowField.NextIncomingId)] = value;
      }

      public uint? IncomingWindow
      {
         get => (uint?)List[((int)FlowField.IncomingWindow)];
         set => List[((int)FlowField.IncomingWindow)] = value;
      }

      public uint? NextOutgoingId
      {
         get => (uint?)List[((int)FlowField.NextOutgoingId)];
         set => List[((int)FlowField.NextOutgoingId)] = value;
      }

      public uint? OutgoingWindow
      {
         get => (uint?)List[((int)FlowField.OutgoingWindow)];
         set => List[((int)FlowField.OutgoingWindow)] = value;
      }

      public uint? Handle
      {
         get => (uint?)List[((int)FlowField.Handle)];
         set => List[((int)FlowField.Handle)] = value;
      }

      public uint? DeliveryCount
      {
         get => (uint?)List[((int)FlowField.DeliveryCount)];
         set => List[((int)FlowField.DeliveryCount)] = value;
      }

      public uint? LinkCredit
      {
         get => (uint?)List[((int)FlowField.LinkCredit)];
         set => List[((int)FlowField.LinkCredit)] = value;
      }

      public uint? Available
      {
         get => (uint?)List[((int)FlowField.Available)];
         set => List[((int)FlowField.Available)] = value;
      }

      public bool? Drain
      {
         get => (bool?)List[((int)FlowField.Drain)];
         set => List[((int)FlowField.Drain)] = value;
      }

      public bool? Echo
      {
         get => (bool?)List[((int)FlowField.Echo)];
         set => List[((int)FlowField.Echo)] = value;
      }

      public IDictionary<Symbol, object> Properties
      {
         get => (IDictionary<Symbol, object>)List[((int)FlowField.Properties)];
         set => List[((int)FlowField.Properties)] = value;
      }

      public override string ToString()
      {
         return "Flow{" +
                "nextIncomingId=" + NextIncomingId +
                ", incomingWindow=" + IncomingWindow +
                ", nextOutgoingId=" + NextOutgoingId +
                ", outgoingWindow=" + OutgoingWindow +
                ", handle=" + Handle +
                ", deliveryCount=" + DeliveryCount +
                ", linkCredit=" + LinkCredit +
                ", available=" + Available +
                ", drain=" + Drain +
                ", echo=" + Echo +
                ", properties=" + Properties +
                '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, Span<byte> payload, ushort channel, T context)
      {
         handler.HandleFlow(frameSize, this, payload, channel, context);
      }
   }
}