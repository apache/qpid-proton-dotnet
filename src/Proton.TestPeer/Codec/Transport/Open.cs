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
   public enum OpenField
   {
      ContainerId,
      Hostname,
      MaxFrameSize,
      ChannelMax,
      IdleTimeout,
      OutgoingLocales,
      IncmoningLocales,
      OfferedCapabilities,
      DesiredCapabilities,
      Properties,
   }

   public sealed class Open : PerformativeDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:open:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000010ul;

      public Open() : base(Enum.GetNames(typeof(OpenField)).Length)
      {
      }

      public Open(object described) : base(Enum.GetNames(typeof(OpenField)).Length, (IList)described)
      {
      }

      public Open(IList described) : base(Enum.GetNames(typeof(OpenField)).Length, described)
      {
      }

      public override PerformativeType Type => PerformativeType.Open;

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public string ContainerId
      {
         get => (string)List[((int)OpenField.ContainerId)];
         set => List[((int)OpenField.ContainerId)] = value;
      }

      public string Hostname
      {
         get => (string)List[((int)OpenField.Hostname)];
         set => List[((int)OpenField.Hostname)] = value;
      }

      public uint? MaxFrameSize
      {
         get => (uint?)List[((int)OpenField.MaxFrameSize)];
         set => List[((int)OpenField.MaxFrameSize)] = value;
      }

      public ushort? ChannelMax
      {
         get => (ushort?)List[((int)OpenField.ChannelMax)];
         set => List[((int)OpenField.ChannelMax)] = value;
      }

      public uint? IdleTimeout
      {
         get => (uint?)List[((int)OpenField.IdleTimeout)];
         set => List[((int)OpenField.IdleTimeout)] = value;
      }

      public Symbol[] OutgoingLocales
      {
         get => (Symbol[])List[((int)OpenField.OutgoingLocales)];
         set => List[((int)OpenField.OutgoingLocales)] = value;
      }

      public Symbol[] IncomingLocales
      {
         get => (Symbol[])List[((int)OpenField.IncmoningLocales)];
         set => List[((int)OpenField.IncmoningLocales)] = value;
      }

      public Symbol[] OfferedCapabilities
      {
         get => (Symbol[])List[((int)OpenField.OfferedCapabilities)];
         set => List[((int)OpenField.OfferedCapabilities)] = value;
      }

      public Symbol[] DesiredCapabilities
      {
         get => (Symbol[])List[((int)OpenField.DesiredCapabilities)];
         set => List[((int)OpenField.DesiredCapabilities)] = value;
      }

      public IDictionary<Symbol, object> Properties
      {
         get => SafeDictionaryConvert<Symbol, object>((int)OpenField.Properties);
         set => List[((int)OpenField.Properties)] = value;
      }

      public override string ToString()
      {
         return "Open{" +
                " containerId='" + ContainerId + '\'' +
                ", hostname='" + Hostname + '\'' +
                ", maxFrameSize=" + MaxFrameSize +
                ", channelMax=" + ChannelMax +
                ", idleTimeOut=" + IdleTimeout +
                ", outgoingLocales=" + OutgoingLocales +
                ", incomingLocales=" + IncomingLocales +
                ", offeredCapabilities=" + OfferedCapabilities +
                ", desiredCapabilities=" + DesiredCapabilities +
                ", properties=" + Properties +
                '}';
      }

      public override void Invoke<T>(IPerformativeHandler<T> handler, uint frameSize, byte[] payload, ushort channel, T context)
      {
         handler.HandleOpen(frameSize, this, payload, channel, context);
      }
   }
}