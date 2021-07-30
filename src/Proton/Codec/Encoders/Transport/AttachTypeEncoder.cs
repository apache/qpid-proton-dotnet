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
   public sealed class AttachTypeEncoder : AbstractDescribedListTypeEncoder<Attach>
   {
      public override Symbol DescriptorSymbol => Attach.DescriptorSymbol;

      public override ulong DescriptorCode => Attach.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Attach value)
      {
         return EncodingCodes.List32;
      }

      protected override int GetElementCount(Attach attach)
      {
         return attach.GetElementCount();
      }

      protected override int GetMinElementCount()
      {
         return 3;
      }

      protected override void WriteElement(Attach attach, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).
         buffer.EnsureWritable(sizeof(int));

         switch (index)
         {
            case 0:
               if (attach.HasName())
               {
                  state.Encoder.WriteString(buffer, state, attach.Name);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               if (attach.HasHandle())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, attach.Handle);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 2:
               if (attach.HasRole())
               {
                  buffer.WriteUnsignedByte((byte)attach.Role.ToBooleanEncoding());
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 3:
               if (attach.HasSenderSettleMode())
               {
                  state.Encoder.WriteUnsignedByte(buffer, state, (byte)attach.SenderSettleMode);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 4:
               if (attach.HasReceiverSettleMode())
               {
                  state.Encoder.WriteUnsignedByte(buffer, state, (byte)attach.ReceiverSettleMode);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 5:
               if (attach.HasSource())
               {
                  state.Encoder.WriteObject(buffer, state, attach.Source);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 6:
               if (attach.HasTargetOrCoordinator())
               {
                  state.Encoder.WriteObject(buffer, state, attach.Target);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 7:
               if (attach.HasUnsettled())
               {
                  state.Encoder.WriteMap(buffer, state, attach.Unsettled);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 8:
               if (attach.HasIncompleteUnsettled())
               {
                  buffer.WriteUnsignedByte((byte)(attach.IncompleteUnsettled ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 9:
               if (attach.HasInitialDeliveryCount())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, attach.InitialDeliveryCount);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 10:
               if (attach.HasMaxMessageSize())
               {
                  state.Encoder.WriteUnsignedLong(buffer, state, attach.MaxMessageSize);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 11:
               if (attach.HasOfferedCapabilities())
               {
                  state.Encoder.WriteArray(buffer, state, attach.OfferedCapabilities);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 12:
               if (attach.HasDesiredCapabilities())
               {
                  state.Encoder.WriteArray(buffer, state, attach.DesiredCapabilities);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 13:
               if (attach.HasProperties())
               {
                  state.Encoder.WriteMap(buffer, state, attach.Properties);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Attach value index: " + index);
         }
      }
   }
}