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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Security;

namespace Apache.Qpid.Proton.Codec.Encoders.Security
{
   public sealed class SaslChallengeTypeEncoder : AbstractDescribedListTypeEncoder<SaslChallenge>
   {
      public override Symbol DescriptorSymbol => SaslChallenge.DescriptorSymbol;

      public override ulong DescriptorCode => SaslChallenge.DescriptorCode;

      protected override EncodingCodes GetListEncoding(SaslChallenge value)
      {
         if (value.Challenge.ReadableBytes < 255)
         {
            return EncodingCodes.List8;
         }
         else
         {
            return EncodingCodes.List32;
         }
      }

      protected override int GetElementCount(SaslChallenge value)
      {
         return 1;
      }

      protected override int GetMinElementCount()
      {
         return 1;
      }

      protected override void WriteElement(SaslChallenge challenge, int index, IProtonBuffer buffer, IEncoderState state)
      {
         switch (index)
         {
            case 0:
                state.Encoder.WriteBinary(buffer, state, challenge.Challenge);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown SaslChallenge value index: " + index);
         }
      }
   }
}