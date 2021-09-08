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
   public enum SaslMechanismsField
   {
      SaslServerMechanisms
   }

   public sealed class SaslMechanisms : SaslDescribedType
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:sasl-mechanisms:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000040ul;

      public SaslMechanisms() : base(Enum.GetNames(typeof(SaslMechanismsField)).Length)
      {
      }

      public SaslMechanisms(object described) : base(Enum.GetNames(typeof(SaslMechanismsField)).Length, (IList)described)
      {
      }

      public SaslMechanisms(IList described) : base(Enum.GetNames(typeof(SaslMechanismsField)).Length, described)
      {
      }

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Symbol[] SaslServerMechanisms
      {
         get => (Symbol[])List[((int)SaslMechanismsField.SaslServerMechanisms)];
         set => List[((int)SaslMechanismsField.SaslServerMechanisms)] = value;
      }

      public override SaslPerformativeType Type => SaslPerformativeType.Mechanisms;

      public override void Invoke<T>(ISaslPerformativeHandler<T> handler, uint frameSize, byte[] payload, ushort channel, T context)
      {
         handler.HandleMechanisms(frameSize, this, context);
      }

      public override string ToString()
      {
         return "SaslMechanisms{" + "saslServerMechanisms=" + SaslServerMechanisms + '}';
      }
   }
}