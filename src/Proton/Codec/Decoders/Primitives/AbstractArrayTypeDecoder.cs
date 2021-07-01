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

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   public abstract class AbstractArrayTypeDecoder : AbstractPrimitiveTypeDecoder, IPrimitiveArrayTypeDecoder
   {
      public override Type DecodesType => typeof(Array);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         int size = ReadSize(buffer, state);
         int count = ReadCount(buffer, state);

         if (EncodingCode == EncodingCodes.Array32)
         {
            size -= 8; // 4 bytes each for size and count;
         }
         else
         {
            size -= 2; // 1 byte each for size and count;
         }

         if (size > buffer.ReadableBytes)
         {
            throw new DecodeException(string.Format(
                "Array size indicated %d is greater than the amount of data available to decode (%d)",
                size, buffer.ReadableBytes));
         }

         throw new NotImplementedException();
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         int size = ReadSize(stream, state);
         int count = ReadCount(stream, state);

         throw new NotImplementedException();
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         buffer.SkipBytes(ReadSize(buffer, state));
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         ProtonStreamReadUtils.SkipBytes(stream, ReadSize(stream, state));
      }

      protected abstract int ReadSize(IProtonBuffer buffer, IDecoderState state);

      protected abstract int ReadCount(IProtonBuffer buffer, IDecoderState state);

      protected abstract int ReadSize(Stream stream, IStreamDecoderState state);

      protected abstract int ReadCount(Stream stream, IStreamDecoderState state);

   }
}