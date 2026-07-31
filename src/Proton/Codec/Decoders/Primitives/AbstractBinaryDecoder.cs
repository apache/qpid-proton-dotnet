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
   public abstract class AbstractBinaryTypeDecoder : AbstractPrimitiveTypeDecoder, IBinaryTypeDecoder
   {
      public override Type DecodesType => typeof(IProtonBuffer);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         int length = ReadSize(buffer, state);

         if (length > buffer.ReadableBytes || length < 0)
         {
            throw new DecodeException(
               string.Format("Binary data size {0} is specified to be greater than the amount " +
                             "of data available ({1})", (uint) length, buffer.ReadableBytes));
         }

         IProtonBuffer payload = buffer.Copy(buffer.ReadOffset, length);

         buffer.SkipBytes(length);

         return payload;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         int length = ReadSize(stream, state);

         if (length > state.MaxBinarySize || length < 0)
         {
            throw new DecodeException(String.Format(
                  "Binary encoded length is specified to be greater than the maximum allowed length " +
                  "l:(%d) m:(%d)", (uint) length, state.MaxBinarySize));
         }

         try
         {
            return ProtonByteBufferAllocator.Instance.Wrap(
               ProtonStreamReadUtils.ReadBytes(stream, length));
         }
         catch (IOException ex)
         {
            throw new DecodeException("Error while reading Binary payload bytes", ex);
         }
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         int length = ReadSize(buffer, state);

         if (length > buffer.ReadableBytes || length < 0)
         {
            throw new DecodeException(
                string.Format("Binary data size {0} is specified to be greater than the amount " +
                              "of data available ({1})", (uint) length, buffer.ReadableBytes));
         }

         buffer.SkipBytes(length);
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         int length = ReadSize(stream, state);

         if (length > state.MaxBinarySize || length < 0)
         {
            throw new DecodeException(String.Format(
                  "Binary encoded length is specified to be greater than the maximum allowed length " +
                  "l:(%d) m:(%d)", length, state.MaxBinarySize));
         }

         ProtonStreamReadUtils.SkipBytes(stream, (uint) length);
      }

      #region BinaryTypeDecoder abstract methods

      public abstract int ReadSize(IProtonBuffer buffer, IDecoderState state);

      public abstract int ReadSize(Stream stream, IStreamDecoderState state);

      #endregion
   }
}