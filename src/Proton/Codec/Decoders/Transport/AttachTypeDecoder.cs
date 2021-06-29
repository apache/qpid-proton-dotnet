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

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public sealed class AttachTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MIN_ATTACH_LIST_ENTRIES = 3;
      private static readonly int MAX_ATTACH_LIST_ENTRIES = 14;

      public override Symbol DescriptorSymbol => Attach.DescriptorSymbol;

      public override ulong DescriptorCode => Attach.DescriptorCode;

      public override Type DecodesType() => typeof(Attach);

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

      private Attach ReadAttach(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Attach attach = new Attach();

         int size = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MIN_ATTACH_LIST_ENTRIES)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MAX_ATTACH_LIST_ENTRIES)
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
               if (index < MIN_ATTACH_LIST_ENTRIES)
               {
                  throw new DecodeException(ErrorForMissingRequiredFields(index));
               }

               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
               case 0:
                  attach.Name = state.Decoder.ReadString(buffer, state);
                  break;
               case 1:
                  attach.Handle = state.Decoder.ReadUnsignedInt(buffer, state) ?? 0;
                  break;
               case 2:
                  bool role = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  attach.Role = RoleExtension.ValueOf(role);
                  break;
               case 3:
                  byte sndSettleMode = state.Decoder.ReadUnsignedByte(buffer, state) ?? 0;
                  attach.SenderSettleMode = SenderSettleModeExtension.ValueOf(sndSettleMode);
                  break;
               case 4:
                  byte rcvSettleMode = state.Decoder.ReadUnsignedByte(buffer, state) ?? 0;
                  attach.ReceiverSettleMode = ReceiverSettleModeExtension.ValueOf(rcvSettleMode);
                  break;
               case 5:
                  attach.Source = state.Decoder.ReadObject<Source>(buffer, state);
                  break;
               case 6:
                  attach.Target = state.Decoder.ReadObject<ITerminus>(buffer, state);
                  break;
               case 7:
                  attach.Unsettled = state.Decoder.ReadMap<IProtonBuffer, IDeliveryState>(buffer, state);
                  break;
               case 8:
                  attach.IncompleteUnsettled = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 9:
                  attach.InitialDeliveryCount = state.Decoder.ReadUnsignedInt(buffer, state) ?? 0;
                  break;
               case 10:
                  attach.MaxMessageSize = state.Decoder.ReadUnsignedLong(buffer, state) ?? 0;
                  break;
               case 11:
                  attach.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 12:
                  attach.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 13:
                  attach.Properties = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
            }
         }

         return attach;
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

      private Attach ReadAttach(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Attach attach = new Attach();

         int size = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MIN_ATTACH_LIST_ENTRIES)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MAX_ATTACH_LIST_ENTRIES)
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
                  if (index < MIN_ATTACH_LIST_ENTRIES)
                  {
                     throw new DecodeException(ErrorForMissingRequiredFields(index));
                  }

                  continue;
               }
               else
               {
                  stream.Seek(stream.Position - 1, SeekOrigin.Current);
               }
            }

            switch (index)
            {
               case 0:
                  attach.Name = state.Decoder.ReadString(stream, state);
                  break;
               case 1:
                  attach.Handle = state.Decoder.ReadUnsignedInt(stream, state) ?? 0;
                  break;
               case 2:
                  bool role = state.Decoder.ReadBoolean(stream, state) ?? false;
                  attach.Role = RoleExtension.ValueOf(role);
                  break;
               case 3:
                  byte sndSettleMode = state.Decoder.ReadUnsignedByte(stream, state) ?? 0;
                  attach.SenderSettleMode = SenderSettleModeExtension.ValueOf(sndSettleMode);
                  break;
               case 4:
                  byte rcvSettleMode = state.Decoder.ReadUnsignedByte(stream, state) ?? 0;
                  attach.ReceiverSettleMode = ReceiverSettleModeExtension.ValueOf(rcvSettleMode);
                  break;
               case 5:
                  attach.Source = state.Decoder.ReadObject<Source>(stream, state);
                  break;
               case 6:
                  attach.Target = state.Decoder.ReadObject<ITerminus>(stream, state);
                  break;
               case 7:
                  attach.Unsettled = state.Decoder.ReadMap<IProtonBuffer, IDeliveryState>(stream, state);
                  break;
               case 8:
                  attach.IncompleteUnsettled = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 9:
                  attach.InitialDeliveryCount = state.Decoder.ReadUnsignedInt(stream, state) ?? 0;
                  break;
               case 10:
                  attach.MaxMessageSize = state.Decoder.ReadUnsignedLong(stream, state) ?? 0;
                  break;
               case 11:
                  attach.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 12:
                  attach.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 13:
                  attach.Properties = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
            }
         }

         return attach;
      }

      private string ErrorForMissingRequiredFields(int present)
      {
         switch (present)
         {
            case 2:
               return "The role field cannot be omitted from the Attach";
            case 1:
               return "The handle field cannot be omitted from the Attach";
            default:
               return "The name field cannot be omitted from the Attach";
         }
      }
   }
}