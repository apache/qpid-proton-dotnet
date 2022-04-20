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
using System.IO;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Decoders.Transport
{
   public sealed class TransferTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinTransferListEntries = 1;
      private static readonly int MaxTransferListEntries = 11;

      public override Symbol DescriptorSymbol => Transfer.DescriptorSymbol;

      public override ulong DescriptorCode => Transfer.DescriptorCode;

      public override Type DecodesType => typeof(Transfer);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadTransfer(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         Transfer[] result = new Transfer[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadTransfer(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static Transfer ReadTransfer(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Transfer result = new();

         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinTransferListEntries)
         {
            throw new DecodeException("The handle field cannot be omitted from the Transfer");
         }

         if (count > MaxTransferListEntries)
         {
            throw new DecodeException("To many entries in Transfer list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            bool nullValue = buffer.GetByte(buffer.ReadOffset) == (byte)EncodingCodes.Null;
            if (nullValue)
            {
               if (index == 0)
               {
                  throw new DecodeException("The handle field cannot be omitted from the Transfer");
               }

               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
               case 0:
                  result.Handle = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 1:
                  result.DeliveryId = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 2:
                  result.DeliveryTag = state.Decoder.ReadDeliveryTag(buffer, state);
                  break;
               case 3:
                  result.MessageFormat = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 4:
                  result.Settled = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 5:
                  result.More = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 6:
                  byte rcvSettleMode = state.Decoder.ReadUnsignedByte(buffer, state) ?? 0;
                  result.ReceiverSettleMode = ReceiverSettleModeExtension.Lookup(rcvSettleMode);
                  break;
               case 7:
                  result.DeliveryState = state.Decoder.ReadObject<IDeliveryState>(buffer, state);
                  break;
               case 8:
                  result.Resume = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 9:
                  result.Aborted = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 10:
                  result.Batchable = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadTransfer(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         Transfer[] result = new Transfer[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadTransfer(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static Transfer ReadTransfer(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Transfer result = new();

         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinTransferListEntries)
         {
            throw new DecodeException("The handle field cannot be omitted from the Transfer");
         }

         if (count > MaxTransferListEntries)
         {
            throw new DecodeException("To many entries in Transfer list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            if (stream.CanSeek)
            {
               bool nullValue = stream.ReadByte() == (byte)EncodingCodes.Null;
               if (nullValue)
               {
                  if (index == 0)
                  {
                     throw new DecodeException("The handle field cannot be omitted from the Transfer");
                  }

                  continue;
               }
               else
               {
                  stream.Seek(-1, SeekOrigin.Current);
               }
            }

            switch (index)
            {
               case 0:
                  result.Handle = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 1:
                  result.DeliveryId = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 2:
                  result.DeliveryTag = state.Decoder.ReadDeliveryTag(stream, state);
                  break;
               case 3:
                  result.MessageFormat = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 4:
                  result.Settled = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 5:
                  result.More = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 6:
                  byte rcvSettleMode = state.Decoder.ReadUnsignedByte(stream, state) ?? 0;
                  result.ReceiverSettleMode = ReceiverSettleModeExtension.Lookup(rcvSettleMode);
                  break;
               case 7:
                  result.DeliveryState = state.Decoder.ReadObject<IDeliveryState>(stream, state);
                  break;
               case 8:
                  result.Resume = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 9:
                  result.Aborted = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 10:
                  result.Batchable = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
            }
         }

         return result;
      }
   }
}