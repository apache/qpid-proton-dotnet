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
using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public abstract class AbstractDescribedMapTypeDecoder<K> : AbstractDescribedTypeDecoder
   {
      private static readonly int MAX_MAP_PREALLOCATION = 256;

      public sealed override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            IMapTypeDecoder mapTypeDecoder =
               CheckIsExpectedTypeAndCast<IMapTypeDecoder>(state.Decoder.ReadNextTypeDecoder(buffer, state));

            return CreateDescribed(ReadMap(buffer, state, mapTypeDecoder));
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public sealed override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            IMapTypeDecoder mapTypeDecoder =
               CheckIsExpectedTypeAndCast<IMapTypeDecoder>(state.Decoder.ReadNextTypeDecoder(stream, state));

            return CreateDescribed(ReadMap(stream, state, mapTypeDecoder));
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public sealed override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         CheckIsExpectedType<IMapTypeDecoder>(state.Decoder.ReadNextTypeDecoder(buffer, state)).SkipValue(buffer, state);
      }

      public sealed override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         CheckIsExpectedType<IMapTypeDecoder>(state.Decoder.ReadNextTypeDecoder(stream, state)).SkipValue(stream, state);
      }

      public sealed override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         ValidateArrayConstraints(count, buffer, state, decoder);

         Array result = Array.CreateInstance(DecodesType, count);
         for (int i = 0; i < count; ++i)
         {
            result.SetValue(CreateDescribed(ReadMap(buffer, state, CheckIsExpectedTypeAndCast<IMapTypeDecoder>(decoder))), i);
         }

         return result;
      }

      public sealed override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         ValidateArrayConstraints(count, stream, state, decoder);

         Array result = Array.CreateInstance(DecodesType, count);
         for (int i = 0; i < count; ++i)
         {
            result.SetValue(CreateDescribed(ReadMap(stream, state, CheckIsExpectedTypeAndCast<IMapTypeDecoder>(decoder))), i);
         }

         return result;
      }

      protected abstract object CreateDescribed(IDictionary<K, object> map);

      protected abstract K ReadKey(IProtonBuffer buffer, IDecoder decoder, IDecoderState state);

      protected abstract K ReadKey(Stream stream, IStreamDecoder decoder, IStreamDecoderState state);

      private IDictionary<K, Object> ReadMap(IProtonBuffer buffer, IDecoderState state, IMapTypeDecoder mapDecoder)
      {
         int size = mapDecoder.ReadSize(buffer, state);
         long expectedEndPos = buffer.ReadOffset + size;
         int count = ValidateAndGetCount(size, buffer, state, mapDecoder);
         IDecoder decoder = state.Decoder;
         int entries = count / 2;

         // Count include both key and value so we must include that in the loop
         IDictionary<K, Object> map = new Dictionary<K, object>(Math.Min(MAX_MAP_PREALLOCATION, entries));

         for (int i = 0; i < entries; i++)
         {
            map.TryAdd(ReadKey(buffer, decoder, state), decoder.ReadObject(buffer, state));
         }

         if (buffer.ReadOffset != expectedEndPos)
         {
            throw new DecodeException("Map decoding did not read the expected amount of bytes: " + size);
         }

         return map;
      }

      private IDictionary<K, Object> ReadMap(Stream stream, IStreamDecoderState state, IMapTypeDecoder mapDecoder)
      {
         int size = mapDecoder.ReadSize(stream, state);
         int count = ValidateAndGetCount(size, stream, state, mapDecoder);
         IStreamDecoder decoder = state.Decoder;
         int entries = count / 2;

         // Count include both key and value so we must include that in the loop
         IDictionary<K, Object> map = new Dictionary<K, object>(Math.Min(MAX_MAP_PREALLOCATION, entries));

         for (int i = 0; i < entries; i++)
         {
            map.TryAdd(ReadKey(stream, decoder, state), decoder.ReadObject(stream, state));
         }

         return map;
      }

      protected static int ValidateAndGetCount(int size, IProtonBuffer buffer, IDecoderState state, IMapTypeDecoder decoder)
      {
         if (size > buffer.ReadableBytes || size < 0)
         {
            throw new DecodeException(String.Format(
               "Map encoded size is specified to be greater than the amount " +
               "of data available s:({0}) r:({1})", (uint) size, buffer.ReadableBytes));
         }

         int count = decoder.ReadCount(buffer, state);

         if (count > size || count < 0)
         {
            throw new DecodeException(String.Format(
               "Map encoded count is specified to be greater than the reported encoded size " +
               "s:({0}) c:({1})", size, (uint) count));
         }

         if (count % 2 != 0)
         {
            throw new DecodeException(String.Format(
               "Map encoded number of elements {0} is not an even number.", count));
         }

         return count;
      }

      protected static int ValidateAndGetCount(int size, Stream stream, IStreamDecoderState state, IMapTypeDecoder decoder)
      {
         if (size > state.MaxMapSize || size < 0)
         {
            throw new DecodeException(String.Format(
                "Map encoded size is specified to be greater than the configured maximum " +
                "map size allowed s:({0}) c:({1})", (uint) size, state.MaxMapSize));
         }

         int count = decoder.ReadCount(stream, state);

         if (count > size || count < 0)
         {
            throw new DecodeException(String.Format(
                "Map encoded count is specified to be greater than the reported encoded size " +
                "s:({0}) c:({1})", size, (uint) count));
         }

         if (count % 2 != 0)
         {
            throw new DecodeException(String.Format(
                "Map encoded number of elements {0} is not an even number.", count));
         }

         return count;
      }

      protected static void ValidateArrayConstraints(int count, IProtonBuffer buffer, IDecoderState state, ITypeDecoder decoder)
      {
         if (decoder.GetType() == typeof(void))
         {
            if (count > state.MaxZeroWidthArrayElements || count < 0)
            {
               throw new DecodeException(String.Format(
                  "Map encoded array count {0} is specified to be greater than limit for zero sized encoded array types (%d)",
                  (uint) count, state.MaxZeroWidthArrayElements));
            }
         }
         else
         {
            if (count > buffer.ReadableBytes || count < 0)
            {
               throw new DecodeException(String.Format(
                  "Map encoded array count {0} is specified to be greater than the amount " +
                  "of data available ({1})", (uint) count, buffer.ReadableBytes));
            }
         }
      }

      protected static void ValidateArrayConstraints(int count, Stream stream, IStreamDecoderState state, IStreamTypeDecoder decoder)
      {
         if (decoder.GetType() == typeof(void))
         {
            if (count > state.MaxZeroWidthArrayElements || count < 0)
            {
               throw new DecodeException(String.Format(
                  "Map encoded array count {0} is specified to be greater than limit for zero sized encoded array types (%d)",
                  (uint) count, state.MaxZeroWidthArrayElements));
            }
         }
         else if (count > state.MaxArraySize || count < 0)
         {
            throw new DecodeException(String.Format(
                  "Array encoded length {0} is specified to be greater than the amount " +
                  "of the configured max array length ({1})", (uint) count, state.MaxStringSize));
         }
      }
   }
}
