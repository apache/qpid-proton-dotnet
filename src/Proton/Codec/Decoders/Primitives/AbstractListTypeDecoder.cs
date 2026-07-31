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
using System.Collections;
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   /// <summary>
   /// Base list type decoder used by decoders of various AMQP types that represent
   /// list style serialized objects.
   /// </summary>
   public abstract class AbstractListTypeDecoder : AbstractPrimitiveTypeDecoder, IListTypeDecoder
   {
      private static readonly int MAX_LIST_PREALLOCATION = 256;

      public override Type DecodesType => typeof(IList);

      public IList<T> ReadList<T>(IProtonBuffer buffer, IDecoderState state)
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
                     "List element size {0} is specified to be greater than the amount " +
                     "of data available ({1})", (uint) size, buffer.ReadableBytes));
            }

            int count = ReadCount(buffer, state);

            if (count > size || count < 0)
            {
               throw new DecodeException(String.Format(
                     "List encoded element count is specified to be greater than the encoded size " +
                     "s:(%d) c:(%d)", size, count));
            }

            IList<T> list = new List<T>(Math.Min(MAX_LIST_PREALLOCATION, count));
            for (int i = 0; i < count; i++)
            {
               list.Add(state.Decoder.ReadObject<T>(buffer, state));
            }

            if (buffer.ReadOffset != expectedEndPos)
            {
               throw new DecodeException("List decoding did not read the expected amount of bytes: " + size);
            }

            return list;
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public IList<T> ReadList<T>(Stream stream, IStreamDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            int size = ReadSize(stream, state);

            if (size > state.MaxListSize || size < 0)
            {
               throw new DecodeException(String.Format(
                     "List encoding size is specified to be greater than the maximum allowed size " +
                     "c:(%d) m:(%d)", (uint) size, state.MaxListSize));
            }

            int count = ReadCount(stream, state);

            if (count > size || count < 0)
            {
               throw new DecodeException(String.Format(
                     "List encoded element count is specified to be greater than the encoded size " +
                     "s:(%d) c:(%d)", size, count));
            }

            IList<T> list = new List<T>(Math.Min(MAX_LIST_PREALLOCATION, count));
            for (int i = 0; i < count; i++)
            {
               list.Add(state.Decoder.ReadObject<T>(stream, state));
            }

            return list;
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         return ReadList<object>(buffer, state);
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         return ReadList<object>(stream, state);
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         int size = ReadSize(buffer, state);

         if (size > buffer.ReadableBytes || size < 0)
         {
            throw new DecodeException(string.Format(
                    "List element size {0} is specified to be greater than the amount " +
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

         if (size > state.MaxListSize || size < 0)
         {
            throw new DecodeException(String.Format(
                  "List encoding size is specified to be greater than the maximum allowed size " +
                  "c:(%d) m:(%d)", (uint) size, state.MaxListSize));
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

      #region Abstract IListTypeDecoder methods

      public abstract int ReadCount(IProtonBuffer buffer, IDecoderState state);

      public abstract int ReadCount(Stream stream, IStreamDecoderState state);

      public abstract int ReadSize(IProtonBuffer buffer, IDecoderState state);

      public abstract int ReadSize(Stream stream, IStreamDecoderState state);

      #endregion
   }
}