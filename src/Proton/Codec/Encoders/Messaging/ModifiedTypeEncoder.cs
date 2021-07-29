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
   public sealed class ModifiedTypeEncoder : AbstractDescribedListTypeEncoder<Modified>
   {
      public override Symbol DescriptorSymbol => Modified.DescriptorSymbol;

      public override ulong DescriptorCode => Modified.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Modified value)
      {
         if (value.MessageAnnotations != null)
         {
            return EncodingCodes.List32;
         }
         else
         {
            return EncodingCodes.List8;
         }
      }

      protected override int GetElementCount(Modified value)
      {
         if (value.MessageAnnotations != null)
         {
            return 3;
         }
         else if (value.UndeliverableHere)
         {
            return 2;
         }
         else if (value.DeliveryFailed)
         {
            return 1;
         }
         else
         {
            return 0;
         }
      }

      protected override void WriteElement(Modified source, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
               buffer.EnsureWritable(sizeof(byte));
               buffer.WriteUnsignedByte((byte)(source.DeliveryFailed ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               break;
            case 1:
               buffer.EnsureWritable(sizeof(byte));
               buffer.WriteUnsignedByte((byte)(source.UndeliverableHere ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               break;
            case 2:
               state.Encoder.WriteMap(buffer, state, source.MessageAnnotations);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Modified value index: " + index);
         }
      }
   }
}