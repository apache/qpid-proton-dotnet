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
   public sealed class TransferTypeEncoder : AbstractDescribedListTypeEncoder<Transfer>
   {
      public override Symbol DescriptorSymbol => Transfer.DescriptorSymbol;

      public override ulong DescriptorCode => Transfer.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Transfer value)
      {
         if (value.DeliveryState != null)
         {
            return EncodingCodes.List32;
         }
         else if (value.DeliveryTag != null && value.DeliveryTag.Length > 200)
         {
            return EncodingCodes.List32;
         }
         else
         {
            return EncodingCodes.List8;
         }
      }

      protected override int GetElementCount(Transfer value)
      {
         return value.GetElementCount();
      }

      protected override int GetMinElementCount()
      {
         return 1;
      }

      protected override void WriteElement(Transfer transfer, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).
         buffer.EnsureWritable(sizeof(int));

         switch (index)
         {
            case 0:
               if (transfer.HasHandle())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, transfer.Handle);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               if (transfer.HasDeliveryId())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, transfer.DeliveryId);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 2:
               if (transfer.HasDeliveryTag())
               {
                  state.Encoder.WriteDeliveryTag(buffer, state, transfer.DeliveryTag);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 3:
               if (transfer.HasMessageFormat())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, transfer.MessageFormat);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 4:
               if (transfer.HasSettled())
               {
                  buffer.WriteUnsignedByte((byte)(transfer.Settled ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 5:
               if (transfer.HasMore())
               {
                  buffer.WriteUnsignedByte((byte)(transfer.More ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 6:
               if (transfer.HasReceiverSettleMode())
               {
                  state.Encoder.WriteUnsignedByte(buffer, state, (byte)transfer.ReceiverSettleMode);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 7:
               state.Encoder.WriteObject(buffer, state, transfer.DeliveryState);
               break;
            case 8:
               if (transfer.HasResume())
               {
                  buffer.WriteUnsignedByte((byte)(transfer.Resume ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 9:
               if (transfer.HasAborted())
               {
                  buffer.WriteUnsignedByte((byte)(transfer.Aborted ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 10:
               if (transfer.HasBatchable())
               {
                  buffer.WriteUnsignedByte((byte)(transfer.Batchable ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Transfer value index: " + index);
         }
      }
   }
}