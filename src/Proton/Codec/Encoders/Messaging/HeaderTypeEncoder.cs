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
   public sealed class HeaderTypeEncoder : AbstractDescribedListTypeEncoder<Header>
   {
      public override Symbol DescriptorSymbol => Header.DescriptorSymbol;

      public override ulong DescriptorCode => Header.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Header value)
      {
         return EncodingCodes.List8;
      }

      protected override int GetElementCount(Header value)
      {
         return value.GetElementCount();
      }

      protected override void WriteElement(Header header, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
               if (header.HasDurable())
               {
                  buffer.WriteUnsignedByte((byte)(header.Durable ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               if (header.HasPriority())
               {
                  state.Encoder.WriteUnsignedByte(buffer, state, header.Priority);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 2:
               if (header.HasTimeToLive())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, header.TimeToLive);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 3:
               if (header.HasFirstAcquirer())
               {
                  buffer.WriteUnsignedByte((byte)(header.FirstAcquirer ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 4:
               if (header.HasDeliveryCount())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, header.DeliveryCount);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Header value index: " + index);
         }
      }
   }
}