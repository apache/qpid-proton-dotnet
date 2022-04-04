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
   public sealed class HeaderTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinHeaderListEntries = 0;
      private static readonly int MaxHeaderListEntries = 5;

      public override Symbol DescriptorSymbol => Header.DescriptorSymbol;

      public override ulong DescriptorCode => Header.DescriptorCode;

      public override Type DecodesType => typeof(Header);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadHeader(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Header[] result = new Header[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadHeader(buffer, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static Header ReadHeader(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Header result = new Header();

         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         // Don't decode anything if things already look wrong.
         if (count < MinHeaderListEntries)
         {
            throw new DecodeException("Not enough entries in Header list encoding: " + count);
         }

         if (count > MaxHeaderListEntries)
         {
            throw new DecodeException("To many entries in Header list encoding: " + count);
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
                  result.Durable = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 1:
                  result.Priority = state.Decoder.ReadUnsignedByte(buffer, state) ?? Header.DEFAULT_PRIORITY;
                  break;
               case 2:
                  result.TimeToLive = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 3:
                  result.FirstAcquirer = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 4:
                  result.DeliveryCount = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadHeader(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Header[] result = new Header[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadHeader(stream, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static Header ReadHeader(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Header result = new Header();

         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         // Don't decode anything if things already look wrong.
         if (count < MinHeaderListEntries)
         {
            throw new DecodeException("Not enough entries in Header list encoding: " + count);
         }

         if (count > MaxHeaderListEntries)
         {
            throw new DecodeException("To many entries in Header list encoding: " + count);
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
                  result.Durable = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 1:
                  result.Priority = state.Decoder.ReadUnsignedByte(stream, state) ?? Header.DEFAULT_PRIORITY;
                  break;
               case 2:
                  result.TimeToLive = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 3:
                  result.FirstAcquirer = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 4:
                  result.DeliveryCount = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
            }
         }

         return result;
      }
   }
}