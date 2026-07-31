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

      public override bool IsArrayType => true;

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         return ReadValue(buffer, state, typeof(object));
      }

      public object ReadValue(IProtonBuffer buffer, IDecoderState state, Type ofType)
      {
         state.IncreaseDepth();

         try
         {
            int size = ReadSize(buffer, state);

            if (size > buffer.ReadableBytes || size < 0)
            {
               throw new DecodeException(string.Format(
                  "Array size indicated {0} is greater than the amount of data available to decode ({1})",
                  (uint) size, buffer.ReadableBytes));
            }

            long startOffset = buffer.ReadOffset;
            int count = ReadCount(buffer, state);

            ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

            if (!decoder.DecodesType.IsAssignableTo(ofType))
            {
               throw new DecodeException(String.Format(
                  "Unexpected type {0}. Expected a type assignable to {1}", decoder.DecodesType,ofType.Name));
            }

            Array array;

            if (decoder is IPrimitiveArrayTypeDecoder arrayDecoder)
            {
               if (count > buffer.ReadableBytes || count < 0)
               {
                  throw new DecodeException(string.Format(
                     "Array count indicated {0} is greater than the amount of data available to decode ({1})",
                     (uint) count, buffer.ReadableBytes));
               }

               array = Array.CreateInstance(decoder.DecodesType, count);

               for (int i = 0; i < count; i++)
               {
                  array.SetValue(arrayDecoder.ReadValue(buffer, state), i);
               }
            }
            else
            {
               array = decoder.ReadArrayElements(buffer, state, count);
            }

            if (buffer.ReadOffset - startOffset != size)
            {
               throw new DecodeException(
                  "Encoded size indicates the array encoding should have been " + size +
                  " bytes but the actual bytes read was " + (buffer.ReadOffset - startOffset));
            }

            return array;
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         return ReadValue(stream, state, typeof(object));
      }

      public object ReadValue(Stream stream, IStreamDecoderState state, Type ofType)
      {
         state.IncreaseDepth();

         try
         {
            int size = ReadSize(stream, state);

            if (size > state.MaxArraySize || size < 0)
            {
               throw new DecodeException(string.Format(
                  "Array size indicated {0} is greater than the maximum encoded length to decode ({1})",
                  (uint) size, state.MaxArraySize));
            }

            int count = ReadCount(stream, state);

            IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

            if (!decoder.DecodesType.IsAssignableTo(ofType))
            {
               throw new DecodeException(String.Format(
                  "Unexpected type {0}. Expected a type assignable to {1}", decoder.DecodesType,ofType.Name));
            }

            if (decoder is IPrimitiveArrayTypeDecoder arrayDecoder)
            {
               if (count > size || count < 0)
               {
                  throw new DecodeException(string.Format(
                     "Array length indicated {0} is greater than the encoded array size ({1})", (uint) count, size));
               }

               object[] array = new object[count];
               for (int i = 0; i < count; i++)
               {
                  array[i] = arrayDecoder.ReadValue(stream, state);
               }

               return array;
            }
            else
            {
               return decoder.ReadArrayElements(stream, state, count);
            }
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         int size = ReadSize(buffer, state);

         if (size > buffer.ReadableBytes || size < 0)
         {
            throw new DecodeException(string.Format(
               "Array size indicated {0} is greater than the amount of data available to decode ({1})",
               (uint) size, buffer.ReadableBytes));
         }

         buffer.SkipBytes(size);
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         int size = ReadSize(stream, state);

         if (size > state.MaxArraySize || size < 0)
         {
            throw new DecodeException(string.Format(
               "Array size indicated {0} is greater than the maximum encoded length to decode ({1})",
               (uint) size, state.MaxArraySize));
         }

         ProtonStreamReadUtils.SkipBytes(stream, size);
      }

      protected abstract int ReadSize(IProtonBuffer buffer, IDecoderState state);

      protected abstract int ReadCount(IProtonBuffer buffer, IDecoderState state);

      protected abstract int ReadSize(Stream stream, IStreamDecoderState state);

      protected abstract int ReadCount(Stream stream, IStreamDecoderState state);

   }
}