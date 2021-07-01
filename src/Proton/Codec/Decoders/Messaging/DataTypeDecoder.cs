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
   public sealed class DataTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly Data EmptyData = new Data();

      public override Symbol DescriptorSymbol => Data.DescriptorSymbol;

      public override ulong DescriptorCode => Data.DescriptorCode;

      public override Type DecodesType => typeof(Data);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = (EncodingCodes)buffer.ReadByte();
         int size;

         switch (encodingCode)
         {
            case EncodingCodes.VBin8:
               size = buffer.ReadByte() & 0xFF;
               break;
            case EncodingCodes.VBin32:
               size = buffer.ReadInt();
               break;
            case EncodingCodes.Null:
               return EmptyData;
            default:
               throw new DecodeException("Expected Binary type but found encoding: " + encodingCode);
         }

         if (size > buffer.ReadableBytes)
         {
            throw new DecodeException("Binary data size " + size + " is specified to be greater than the " +
                                      "amount of data available (" + buffer.ReadableBytes + ")");
         }

         int position = buffer.ReadOffset;
         IProtonBuffer data = ProtonByteBufferAllocator.INSTANCE.Allocate(size, size);

         buffer.GetBytes(position, data.Array, data.ArrayOffset, size);
         data.WriteOffset = size;
         buffer.ReadOffset = position + size;

         return new Data(data);
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IBinaryTypeDecoder valueDecoder = CheckIsExpectedTypeAndCast<IBinaryTypeDecoder>(decoder);
         IProtonBuffer[] binaryArray = (IProtonBuffer[])valueDecoder.ReadArrayElements(buffer, state, count);

         Data[] dataArray = new Data[count];
         for (int i = 0; i < count; ++i)
         {
            dataArray[i] = new Data(binaryArray[i]);
         }

         return dataArray;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IBinaryTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = (EncodingCodes)ProtonStreamReadUtils.ReadByte(stream);
         int size;

         switch (encodingCode)
         {
            case EncodingCodes.VBin8:
               size = ProtonStreamReadUtils.ReadByte(stream) & 0xFF;
               break;
            case EncodingCodes.VBin32:
               size = ProtonStreamReadUtils.ReadInt(stream);
               break;
            case EncodingCodes.Null:
               return EmptyData;
            default:
               throw new DecodeException("Expected Binary type but found encoding: " + encodingCode);
         }

         return new Data(ProtonByteBufferAllocator.INSTANCE.Wrap(ProtonStreamReadUtils.ReadBytes(stream, size)));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IBinaryTypeDecoder valueDecoder = CheckIsExpectedTypeAndCast<IBinaryTypeDecoder>(decoder);
         IProtonBuffer[] binaryArray = (IProtonBuffer[])valueDecoder.ReadArrayElements(stream, state, count);

         Data[] dataArray = new Data[count];
         for (int i = 0; i < count; ++i)
         {
            dataArray[i] = new Data(binaryArray[i]);
         }

         return dataArray;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IBinaryTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }
   }
}