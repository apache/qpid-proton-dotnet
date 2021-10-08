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
using Apache.Qpid.Proton.Test.Driver.Codec.Utilities;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Security
{
   public enum SaslChallengeField
   {
      Challenge
   }

   public sealed class SaslChallenge : SaslDescribedType
   {
    public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:sasl-challenge:list");
    public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000042ul;

      public SaslChallenge() : base(Enum.GetNames(typeof(SaslChallengeField)).Length)
      {
      }

      public SaslChallenge(object described) : base(Enum.GetNames(typeof(SaslChallengeField)).Length, (IList)described)
      {
      }

      public SaslChallenge(IList described) : base(Enum.GetNames(typeof(SaslChallengeField)).Length, described)
      {
      }

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public Binary Challenge
      {
         get => (Binary)List[((int)SaslChallengeField.Challenge)];
         set => List[((int)SaslChallengeField.Challenge)] = value;
      }

      public override SaslPerformativeType Type => SaslPerformativeType.Challenge;

      public override void Invoke<T>(ISaslPerformativeHandler<T> handler, uint frameSize, byte[] payload, ushort channel, T context)
      {
        handler.HandleChallenge(frameSize, this, context);
      }

      public override string ToString()
      {
         return "SaslChallenge{" + "challenge=" + TypeMapper.ToQuotedString(Challenge) + '}';
      }
   }
}