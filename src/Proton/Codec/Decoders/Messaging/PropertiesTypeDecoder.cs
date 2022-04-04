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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Decoders.Messaging
{
   public sealed class PropertiesTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinPropertiesListEntries = 0;
      private static readonly int MaxPropertiesListEntries = 13;

      public override Symbol DescriptorSymbol => Properties.DescriptorSymbol;

      public override ulong DescriptorCode => Properties.DescriptorCode;

      public override Type DecodesType => typeof(Properties);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadProperties(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Properties[] result = new Properties[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadProperties(buffer, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static Properties ReadProperties(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Properties result = new Properties();

         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         // Don't decode anything if things already look wrong.
         if (count < MinPropertiesListEntries)
         {
            throw new DecodeException("Not enough entries in Properties list encoding: " + count);
         }

         if (count > MaxPropertiesListEntries)
         {
            throw new DecodeException("To many entries in Properties list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            bool nullValue = buffer.GetByte(buffer.ReadOffset) == (byte)EncodingCodes.Null;
            if (nullValue)
            {
               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
                case 0:
                    result.MessageId = state.Decoder.ReadObject(buffer, state);
                    break;
                case 1:
                    result.UserId = state.Decoder.ReadBinary(buffer, state);
                    break;
                case 2:
                    result.To = state.Decoder.ReadString(buffer, state);
                    break;
                case 3:
                    result.Subject = state.Decoder.ReadString(buffer, state);
                    break;
                case 4:
                    result.ReplyTo = state.Decoder.ReadString(buffer, state);
                    break;
                case 5:
                    result.CorrelationId = state.Decoder.ReadObject(buffer, state);
                    break;
                case 6:
                    result.ContentType = state.Decoder.ReadSymbol(buffer, state)?.ToString() ?? null;
                    break;
                case 7:
                    result.ContentEncoding = state.Decoder.ReadSymbol(buffer, state)?.ToString() ?? null;
                    break;
                case 8:
                    result.AbsoluteExpiryTime = state.Decoder.ReadTimestamp(buffer, state) ?? 0;
                    break;
                case 9:
                    result.CreationTime = state.Decoder.ReadTimestamp(buffer, state) ?? 0;
                    break;
                case 10:
                    result.GroupId = state.Decoder.ReadString(buffer, state);
                    break;
                case 11:
                    result.GroupSequence = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                    break;
                case 12:
                    result.ReplyToGroupId = state.Decoder.ReadString(buffer, state);
                    break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadProperties(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Properties[] result = new Properties[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadProperties(stream, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static Properties ReadProperties(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Properties result = new Properties();

         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         // Don't decode anything if things already look wrong.
         if (count < MinPropertiesListEntries)
         {
            throw new DecodeException("Not enough entries in Properties list encoding: " + count);
         }

         if (count > MaxPropertiesListEntries)
         {
            throw new DecodeException("To many entries in Properties list encoding: " + count);
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
                    result.MessageId = state.Decoder.ReadObject(stream, state);
                    break;
                case 1:
                    result.UserId = state.Decoder.ReadBinary(stream, state);
                    break;
                case 2:
                    result.To = state.Decoder.ReadString(stream, state);
                    break;
                case 3:
                    result.Subject = state.Decoder.ReadString(stream, state);
                    break;
                case 4:
                    result.ReplyTo = state.Decoder.ReadString(stream, state);
                    break;
                case 5:
                    result.CorrelationId = state.Decoder.ReadObject(stream, state);
                    break;
                case 6:
                    result.ContentType = state.Decoder.ReadSymbol(stream, state)?.ToString() ?? null;
                    break;
                case 7:
                    result.ContentEncoding = state.Decoder.ReadSymbol(stream, state)?.ToString() ?? null;
                    break;
                case 8:
                    result.AbsoluteExpiryTime = state.Decoder.ReadTimestamp(stream, state) ?? 0;
                    break;
                case 9:
                    result.CreationTime = state.Decoder.ReadTimestamp(stream, state) ?? 0;
                    break;
                case 10:
                    result.GroupId = state.Decoder.ReadString(stream, state);
                    break;
                case 11:
                    result.GroupSequence = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                    break;
                case 12:
                    result.ReplyToGroupId = state.Decoder.ReadString(stream, state);
                    break;
            }
         }

         return result;
      }
   }
}