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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Security
{
   public enum SaslInitField
   {
      Mechanism,
      InitialResponse,
      Hostname
   }

   public sealed class SaslInit : SaslDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:sasl-init:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000041ul;

      public SaslInit() : base(Enum.GetNames(typeof(SaslInitField)).Length)
      {
      }

      public SaslInit(object described) : base(Enum.GetNames(typeof(SaslInitField)).Length, (IList)described)
      {
      }

      public SaslInit(IList described) : base(Enum.GetNames(typeof(SaslInitField)).Length, described)
      {
      }

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Symbol Mechanism
      {
         get => (Symbol)List[((int)SaslInitField.Mechanism)];
         set => List[((int)SaslInitField.Mechanism)] = value;
      }

      public byte[] InitialResponse
      {
         get => (byte[])List[((int)SaslInitField.InitialResponse)];
         set => List[((int)SaslInitField.InitialResponse)] = value;
      }

      public string Hostname
      {
         get => (string)List[((int)SaslInitField.Hostname)];
         set => List[((int)SaslInitField.Hostname)] = value;
      }

      public override SaslPerformativeType Type => SaslPerformativeType.Challenge;

      public override void Invoke<T>(ISaslPerformativeHandler<T> handler, uint frameSize, Span<byte> payload, ushort channel, T context)
      {
        handler.HandleInit(frameSize, this, context);
      }

      public override string ToString()
      {
         return "SaslInit{" +
                "mechanism=" + Mechanism +
                ", initialResponse=" + // TODO TypeMapper.toQuotedString(getInitialResponse()) +
                ", hostname='" + Hostname + '\'' + '}';
      }
   }
}