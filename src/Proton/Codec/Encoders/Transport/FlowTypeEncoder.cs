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
   public sealed class FlowTypeEncoder : AbstractDescribedListTypeEncoder<Flow>
   {
      public override Symbol DescriptorSymbol => Flow.DescriptorSymbol;

      public override ulong DescriptorCode => Flow.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Flow value)
      {
         if (value.Properties == null)
         {
            return EncodingCodes.List8;
         }
         else
         {
            return EncodingCodes.List32;
         }
      }

      protected override int GetElementCount(Flow value)
      {
         return value.GetElementCount();
      }

      protected override int GetMinElementCount()
      {
         return 4;
      }

      protected override void WriteElement(Flow flow, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
               if (flow.HasNextIncomingId())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.NextIncomingId);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               if (flow.HasIncomingWindow())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.IncomingWindow);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 2:
               if (flow.HasNextOutgoingId())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.NextOutgoingId);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 3:
               if (flow.HasOutgoingWindow())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.OutgoingWindow);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 4:
               if (flow.HasHandle())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.Handle);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 5:
               if (flow.HasDeliveryCount())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.DeliveryCount);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 6:
               if (flow.HasLinkCredit())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.LinkCredit);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 7:
               if (flow.HasAvailable())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, flow.Available);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 8:
               if (flow.HasDrain())
               {
                  buffer.WriteUnsignedByte((byte)(flow.Drain ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 9:
               if (flow.HasEcho())
               {
                  buffer.WriteUnsignedByte((byte)(flow.Echo ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 10:
               state.Encoder.WriteMap(buffer, state, flow.Properties);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Flow value index: " + index);
         }
      }
   }
}