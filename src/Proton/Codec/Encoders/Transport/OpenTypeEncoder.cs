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
   public sealed class OpenTypeEncoder : AbstractDescribedListTypeEncoder<Open>
   {
      public override Symbol DescriptorSymbol => Open.DescriptorSymbol;

      public override ulong DescriptorCode => Open.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Open value)
      {
         return EncodingCodes.List32;
      }

      protected override int GetElementCount(Open value)
      {
         return value.GetElementCount();
      }

      protected override int GetMinElementCount()
      {
         return 1;
      }

      protected override void WriteElement(Open open, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).
         buffer.EnsureWritable(sizeof(int));

         switch (index)
         {
            case 0:
               if (open.HasContainerId())
               {
                  state.Encoder.WriteString(buffer, state, open.ContainerId);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               if (open.HasHostname())
               {
                  state.Encoder.WriteString(buffer, state, open.Hostname);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 2:
               if (open.HasMaxFrameSize())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, open.MaxFrameSize);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 3:
               if (open.HasChannelMax())
               {
                  state.Encoder.WriteUnsignedShort(buffer, state, open.ChannelMax);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 4:
               if (open.HasIdleTimeout())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, open.IdleTimeout);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 5:
               if (open.HasOutgoingLocales())
               {
                  state.Encoder.WriteArray(buffer, state, open.OutgoingLocales);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 6:
               if (open.HasIncomingLocales())
               {
                  state.Encoder.WriteArray(buffer, state, open.IncomingLocales);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 7:
               if (open.HasOfferedCapabilities())
               {
                  state.Encoder.WriteArray(buffer, state, open.OfferedCapabilities);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 8:
               if (open.HasDesiredCapabilities())
               {
                  state.Encoder.WriteArray(buffer, state, open.DesiredCapabilities);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 9:
               if (open.HasProperties())
               {
                  state.Encoder.WriteMap(buffer, state, open.Properties);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Open value index: " + index);
         }
      }
   }
}