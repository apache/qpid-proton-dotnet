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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Encoders.Transport
{
   public sealed class BeginTypeEncoder : AbstractDescribedListTypeEncoder<Begin>
   {
      public override Symbol DescriptorSymbol => Begin.DescriptorSymbol;

      public override ulong DescriptorCode => Begin.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Begin value)
      {
         return EncodingCodes.List32;
      }

      protected override int GetElementCount(Begin begin)
      {
         return begin.GetElementCount();
      }

      protected override int GetMinElementCount()
      {
         return 4;
      }

      protected override void WriteElement(Begin begin, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).
         buffer.EnsureWritable(sizeof(int));

         switch (index)
         {
            case 0:
               if (begin.HasRemoteChannel())
               {
                  state.Encoder.WriteUnsignedShort(buffer, state, begin.RemoteChannel);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               if (begin.HasNextOutgoingId())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, begin.NextOutgoingId);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 2:
               if (begin.HasIncomingWindow())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, begin.IncomingWindow);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 3:
               if (begin.HasOutgoingWindow())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, begin.OutgoingWindow);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 4:
               if (begin.HasHandleMax())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, begin.HandleMax);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 5:
               if (begin.HasOfferedCapabilities())
               {
                  state.Encoder.WriteArray(buffer, state, begin.OfferedCapabilities);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 6:
               if (begin.HasDesiredCapabilities())
               {
                  state.Encoder.WriteArray(buffer, state, begin.DesiredCapabilities);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 7:
               if (begin.HasProperties())
               {
                  state.Encoder.WriteMap(buffer, state, begin.Properties);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Begin value index: " + index);
         }
      }
   }
}