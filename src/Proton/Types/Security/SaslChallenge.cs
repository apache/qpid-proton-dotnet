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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Utils;

namespace Apache.Qpid.Proton.Types.Security
{
   public sealed class SaslChallenge : ISaslPerformative
   {
      public static readonly ulong DescriptorCode = 0x0000000000000042UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:sasl-challenge:list");

      public SaslChallenge() { }

      public SaslChallenge(IProtonBuffer challenge) => Challenge = challenge;

      /// <summary>
      /// Reads the SASL Challenge buffer that was sent by the remote.
      /// </summary>
      public IProtonBuffer Challenge { get; set; }

      public SaslPerformativeType Type => SaslPerformativeType.Challenge;

      public void Invoke<T>(ISaslPerformativeHandler<T> handler, T context)
      {
         handler.HandleChallenge(this, context);
      }

      public object Clone()
      {
         return new SaslChallenge(Challenge);
      }

      public override string ToString()
      {
         return "SaslChallenge{" +
                "challenge=" + (Challenge == null ? StringUtils.ToQuotedString(Challenge) : "null") + '}';
      }
   }
}