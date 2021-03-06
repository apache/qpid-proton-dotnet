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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Encoders.Messaging
{
   public sealed class ReceivedTypeEncoder : AbstractDescribedListTypeEncoder<Received>
   {
      public override Symbol DescriptorSymbol => Received.DescriptorSymbol;

      public override ulong DescriptorCode => Received.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Received value)
      {
         return EncodingCodes.List8;
      }

      protected override int GetElementCount(Received value)
      {
         return 2;
      }

      protected override void WriteElement(Received source, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
               state.Encoder.WriteUnsignedInteger(buffer, state, source.SectionNumber);
               break;
            case 1:
               state.Encoder.WriteUnsignedLong(buffer, state, source.SectionOffset);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Received value index: " + index);
         }
      }
   }
}