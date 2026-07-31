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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   /// <summary>
   /// Base map type decoder used by decoders of various AMQP types that represent
   /// map style serialized objects.
   /// </summary>
   public abstract class AbstractMapTypeDecoder : AbstractPrimitiveTypeDecoder, IMapTypeDecoder
   {
      private static readonly int MAX_MAP_PREALLOCATION = 256;

      public override Type DecodesType => typeof(IDictionary);

      public IDictionary<K, V> ReadMap<K, V>(IProtonBuffer buffer, IDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            int size = ReadSize(buffer, state);
            long expectedEndPos = buffer.ReadOffset + size;

            // Ensure we do not allocate an array of size greater then the available data, otherwise there is a risk for an OOM error
            if (size > buffer.ReadableBytes || size < 0)
            {
               throw new DecodeException(string.Format(
                     "Map element size {0} is specified to be greater than the amount " +
                     "of data available ({1})", (uint) size, buffer.ReadableBytes));
            }

            int count = ReadCount(buffer, state);

            if (count > size || count < 0)
            {
               throw new DecodeException(string.Format(
                     "Map element count {0} is specified to be greater than the amount " +
                     "of data available ({1})", (uint) count, size));
            }

            if (count % 2 != 0)
            {
               throw new DecodeException(string.Format(
                  "Map encoded number of elements {0} is not an even number.", count));
            }

            int elements = count / 2;
            IDictionary<K, V> map = new Dictionary<K, V>(Math.Min(MAX_MAP_PREALLOCATION, elements));
            for (int i = 0; i < elements; i++)
            {
               K key = state.Decoder.ReadObject<K>(buffer, state);
               V value = state.Decoder.ReadObject<V>(buffer, state);

               map.Add(key, value);
            }

            if (buffer.ReadOffset != expectedEndPos)
            {
               throw new DecodeException("Map decoding did not read the expected amount of bytes: " + size);
            }

            return map;
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public IDictionary<K, V> ReadMap<K, V>(Stream stream, IStreamDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            int size = ReadSize(stream, state);

            if (size > state.MaxMapSize || size < 0)
            {
               throw new DecodeException(string.Format(
                     "Map encoding size {0} is specified to be greater than the maximum " +
                     "allowed size value ({1})", (uint) size, state.MaxMapSize));
            }

            int count = ReadCount(stream, state);

            if (count > size || count < 0)
            {
               throw new DecodeException(string.Format(
                     "Map element count {0} is specified to be greater than the amount " +
                     "of data available ({1})", (uint) count, size));
            }

            if (count % 2 != 0)
            {
               throw new DecodeException(string.Format(
                  "Map encoded number of elements {0} is not an even number.", count));
            }

            int elements = count / 2;
            IDictionary<K, V> map = new Dictionary<K, V>(Math.Min(MAX_MAP_PREALLOCATION, elements));
            for (int i = 0; i < elements; i++)
            {
               K key = state.Decoder.ReadObject<K>(stream, state);
               V value = state.Decoder.ReadObject<V>(stream, state);
               map.Add(key, value);
            }

            return map;
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         return ReadMap<object, object>(buffer, state);
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         return ReadMap<object, object>(stream, state);
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         int size = ReadSize(buffer, state);

         // Ensure we do not allocate an array of size greater then the available data, otherwise there is a risk for an OOM error
         if (size > buffer.ReadableBytes || size < 0)
         {
            throw new DecodeException(string.Format(
                    "Map element size {0} is specified to be greater than the amount " +
                    "of data available ({1})", (uint) size, buffer.ReadableBytes));
         }

         state.IncreaseDepth();

         try
         {
            buffer.SkipBytes(size);
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         int size = ReadSize(stream, state);

         if (size > state.MaxMapSize || size < 0)
         {
            throw new DecodeException(string.Format(
                  "Map encoding size {0} is specified to be greater than the maximum " +
                  "allowed size value ({1})", (uint) size, state.MaxMapSize));
         }

         state.IncreaseDepth();

         try
         {
            ProtonStreamReadUtils.SkipBytes(stream, size);
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      #region Abstract IMapTypeDecoder methods

      public abstract int ReadCount(IProtonBuffer buffer, IDecoderState state);

      public abstract int ReadCount(Stream stream, IStreamDecoderState state);

      public abstract int ReadSize(IProtonBuffer buffer, IDecoderState state);

      public abstract int ReadSize(Stream stream, IStreamDecoderState state);

      #endregion
   }
}