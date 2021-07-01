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
   public sealed class OpenTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinOpenListEntries = 1;
      private static readonly int MaxOpenListEntries = 10;

      public override Symbol DescriptorSymbol => Open.DescriptorSymbol;

      public override ulong DescriptorCode => Open.DescriptorCode;

      public override Type DecodesType => typeof(Open);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadOpen(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         Open[] result = new Open[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadOpen(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private Open ReadOpen(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Open result = new Open();

         int size = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinOpenListEntries)
         {
            throw new DecodeException("The container-id field cannot be omitted from the Open");
         }

         if (count > MaxOpenListEntries)
         {
            throw new DecodeException("To many entries in Open list encoding: " + count);
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
                  throw new DecodeException("The container-id field cannot be omitted from the Open");
               }

               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
                case 0:
                    result.ContainerId = state.Decoder.ReadString(buffer, state);
                    break;
                case 1:
                    result.Hostname = state.Decoder.ReadString(buffer, state);
                    break;
                case 2:
                    result.MaxFrameSize = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                    break;
                case 3:
                    result.ChannelMax = state.Decoder.ReadUnsignedShort(buffer, state) ?? 0;
                    break;
                case 4:
                    result.IdleTimeout = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                    break;
                case 5:
                    result.OutgoingLocales = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                    break;
                case 6:
                    result.IncomingLocales = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                    break;
                case 7:
                    result.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                    break;
                case 8:
                    result.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                    break;
                case 9:
                    result.Properties = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                    break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadOpen(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         Open[] result = new Open[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadOpen(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private Open ReadOpen(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Open result = new Open();

         int size = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinOpenListEntries)
         {
            throw new DecodeException("The container-id field cannot be omitted from the Open");
         }

         if (count > MaxOpenListEntries)
         {
            throw new DecodeException("To many entries in Open list encoding: " + count);
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
                     throw new DecodeException("The container-id field cannot be omitted from the Open");
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
                    result.ContainerId = state.Decoder.ReadString(stream, state);
                    break;
                case 1:
                    result.Hostname = state.Decoder.ReadString(stream, state);
                    break;
                case 2:
                    result.MaxFrameSize = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                    break;
                case 3:
                    result.ChannelMax = state.Decoder.ReadUnsignedShort(stream, state) ?? 0;
                    break;
                case 4:
                    result.IdleTimeout = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                    break;
                case 5:
                    result.OutgoingLocales = state.Decoder.ReadMultiple<Symbol>(stream, state);
                    break;
                case 6:
                    result.IncomingLocales = state.Decoder.ReadMultiple<Symbol>(stream, state);
                    break;
                case 7:
                    result.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                    break;
                case 8:
                    result.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                    break;
                case 9:
                    result.Properties = state.Decoder.ReadMap<Symbol, object>(stream, state);
                    break;
            }
         }

         return result;
      }

   }
}