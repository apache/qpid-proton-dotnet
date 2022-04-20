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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Decoders.Transport
{
   public sealed class AttachTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinAttachListEntries = 3;
      private static readonly int MaxAttachListEntries = 14;

      public override Symbol DescriptorSymbol => Attach.DescriptorSymbol;

      public override ulong DescriptorCode => Attach.DescriptorCode;

      public override Type DecodesType => typeof(Attach);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadAttach(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         Attach[] result = new Attach[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadAttach(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static Attach ReadAttach(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Attach result = new();

         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinAttachListEntries)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MaxAttachListEntries)
         {
            throw new DecodeException("To many entries in Attach list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            bool nullValue = buffer.GetByte(buffer.ReadOffset) == (byte)EncodingCodes.Null;
            if (nullValue)
            {
               // Ensure mandatory fields are set
               if (index < MinAttachListEntries)
               {
                  throw new DecodeException(ErrorForMissingRequiredFields(index));
               }

               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
               case 0:
                  result.Name = state.Decoder.ReadString(buffer, state);
                  break;
               case 1:
                  result.Handle = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 2:
                  bool role = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  result.Role = RoleExtension.Lookup(role);
                  break;
               case 3:
                  byte sndSettleMode = state.Decoder.ReadUnsignedByte(buffer, state) ?? 0;
                  result.SenderSettleMode = SenderSettleModeExtension.Lookup(sndSettleMode);
                  break;
               case 4:
                  byte rcvSettleMode = state.Decoder.ReadUnsignedByte(buffer, state) ?? 0;
                  result.ReceiverSettleMode = ReceiverSettleModeExtension.Lookup(rcvSettleMode);
                  break;
               case 5:
                  result.Source = state.Decoder.ReadObject<Source>(buffer, state);
                  break;
               case 6:
                  result.Target = state.Decoder.ReadObject<ITerminus>(buffer, state);
                  break;
               case 7:
                  result.Unsettled = state.Decoder.ReadMap<IProtonBuffer, IDeliveryState>(buffer, state);
                  break;
               case 8:
                  result.IncompleteUnsettled = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 9:
                  result.InitialDeliveryCount = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 10:
                  result.MaxMessageSize = state.Decoder.ReadUnsignedLong(buffer, state) ?? 0;
                  break;
               case 11:
                  result.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 12:
                  result.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 13:
                  result.Properties = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadAttach(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         Attach[] result = new Attach[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadAttach(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static Attach ReadAttach(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Attach result = new();

         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinAttachListEntries)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MaxAttachListEntries)
         {
            throw new DecodeException("To many entries in Attach list encoding: " + count);
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
                  // Ensure mandatory fields are set
                  if (index < MinAttachListEntries)
                  {
                     throw new DecodeException(ErrorForMissingRequiredFields(index));
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
                  result.Name = state.Decoder.ReadString(stream, state);
                  break;
               case 1:
                  result.Handle = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 2:
                  bool role = state.Decoder.ReadBoolean(stream, state) ?? false;
                  result.Role = RoleExtension.Lookup(role);
                  break;
               case 3:
                  byte sndSettleMode = state.Decoder.ReadUnsignedByte(stream, state) ?? 0;
                  result.SenderSettleMode = SenderSettleModeExtension.Lookup(sndSettleMode);
                  break;
               case 4:
                  byte rcvSettleMode = state.Decoder.ReadUnsignedByte(stream, state) ?? 0;
                  result.ReceiverSettleMode = ReceiverSettleModeExtension.Lookup(rcvSettleMode);
                  break;
               case 5:
                  result.Source = state.Decoder.ReadObject<Source>(stream, state);
                  break;
               case 6:
                  result.Target = state.Decoder.ReadObject<ITerminus>(stream, state);
                  break;
               case 7:
                  result.Unsettled = state.Decoder.ReadMap<IProtonBuffer, IDeliveryState>(stream, state);
                  break;
               case 8:
                  result.IncompleteUnsettled = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 9:
                  result.InitialDeliveryCount = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 10:
                  result.MaxMessageSize = state.Decoder.ReadUnsignedLong(stream, state) ?? 0;
                  break;
               case 11:
                  result.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 12:
                  result.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 13:
                  result.Properties = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
            }
         }

         return result;
      }

      private static string ErrorForMissingRequiredFields(int present)
      {
         return present switch
         {
            2 => "The role field cannot be omitted from the Attach",
            1 => "The handle field cannot be omitted from the Attach",
            _ => "The name field cannot be omitted from the Attach",
         };
      }
   }
}