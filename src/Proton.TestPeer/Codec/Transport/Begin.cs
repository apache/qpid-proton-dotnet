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
   public enum BeginField
   {
      RemoteChannel,
      NextOutgoingId,
      IncomingWindow,
      OutgoingWindow,
      HandleMax,
      OfferedCapabilities,
      DesiredCapabilities,
      Properties,
   }

   public sealed class Begin : PerformativeDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:begin:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000011ul;

      public Begin() : base(Enum.GetNames(typeof(BeginField)).Length)
      {
      }

      public Begin(object described) : base(Enum.GetNames(typeof(BeginField)).Length, (IList)described)
      {
      }

      public Begin(IList described) : base(Enum.GetNames(typeof(BeginField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Begin;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public ushort? RemoteChannel
      {
         get => (ushort?)List[((int)BeginField.RemoteChannel)];
         set => List[((int)BeginField.RemoteChannel)] = value;
      }

      public uint? NextOutgoingId
      {
         get => (ushort?)List[((int)BeginField.NextOutgoingId)];
         set => List[((int)BeginField.NextOutgoingId)] = value;
      }

      public uint? IncomingWindow
      {
         get => (ushort?)List[((int)BeginField.IncomingWindow)];
         set => List[((int)BeginField.IncomingWindow)] = value;
      }

      public uint? OutgoingWindow
      {
         get => (ushort?)List[((int)BeginField.OutgoingWindow)];
         set => List[((int)BeginField.OutgoingWindow)] = value;
      }

      public uint? HandleMax
      {
         get => (uint?)List[((int)BeginField.HandleMax)];
         set => List[((int)BeginField.HandleMax)] = value;
      }

      public Symbol[] OfferedCapabilities
      {
         get => (Symbol[])List[((int)BeginField.OfferedCapabilities)];
         set => List[((int)BeginField.OfferedCapabilities)] = value;
      }

      public Symbol[] DesiredCapabilities
      {
         get => (Symbol[])List[((int)BeginField.DesiredCapabilities)];
         set => List[((int)BeginField.DesiredCapabilities)] = value;
      }

      public IDictionary<Symbol, object> Properties
      {
         get => (IDictionary<Symbol, object>)List[((int)BeginField.Properties)];
         set => List[((int)BeginField.Properties)] = value;
      }

      public override string ToString()
      {
         return "Begin{" +
                "remoteChannel=" + RemoteChannel +
                ", nextOutgoingId=" + NextOutgoingId +
                ", incomingWindow=" + IncomingWindow +
                ", outgoingWindow=" + OutgoingWindow +
                ", handleMax=" + HandleMax +
                ", offeredCapabilities=" + OfferedCapabilities +
                ", desiredCapabilities=" + DesiredCapabilities +
                ", properties=" + Properties +
                '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, byte[] payload, ushort channel, T context)
      {
         handler.HandleBegin(frameSize, this, payload, channel, context);
      }
   }
}