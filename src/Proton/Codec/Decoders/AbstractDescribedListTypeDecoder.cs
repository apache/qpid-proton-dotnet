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

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public abstract class AbstractDescribedListTypeDecoder : AbstractDescribedTypeDecoder
   {
      public sealed override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

            return ReadSingle(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
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
            IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

            return ReadSingle(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public sealed override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         CheckIsExpectedType<IListTypeDecoder>(state.Decoder.ReadNextTypeDecoder(buffer, state)).SkipValue(buffer, state);
      }

      public sealed override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         CheckIsExpectedType<IListTypeDecoder>(state.Decoder.ReadNextTypeDecoder(stream, state)).SkipValue(stream, state);
      }

      public sealed override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         if (listDecoder.IsZeroWidth)
         {
            if (count > state.MaxZeroWidthArrayElements)
            {
               throw new DecodeException(
                  "Array element count " + count + " is specified to be greater than limit for zero sized " +
                  "encoded array types (" + state.MaxZeroWidthArrayElements + ")");
            }
         }
         else if (count > buffer.ReadableBytes)
         {
            throw new DecodeException(
                "Array encoded element count " + count + " is specified to be greater than the " +
                "amount of data available " + buffer.ReadableBytes);
         }

         Array result = Array.CreateInstance(DecodesType, count);
         for (int i = 0; i < count; ++i)
         {
            result.SetValue(ReadSingle(buffer, state, listDecoder), i);
         }

         return result;
      }

      public sealed override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         if (listDecoder.IsZeroWidth && count > state.MaxZeroWidthArrayElements)
         {
            throw new DecodeException(
                "Array element count " + count + " is specified to be greater than limit for zero sized " +
                "encoded array types (" + state.MaxZeroWidthArrayElements + ")");
         }
         else if (count > state.MaxArraySize)
         {
            throw new DecodeException(
                "Array encoded element count " + count + " is specified to be greater than the " +
                "configured max array size " + state.MaxArraySize);
         }

         Array result = Array.CreateInstance(DecodesType, count);
         for (int i = 0; i < count; ++i)
         {
            result.SetValue(ReadSingle(stream, state, listDecoder), i);
         }

         return result;
      }

      /// <summary>
      /// Gets the minimum number of elements this described list is allowed to carry.
      /// </summary>
      protected abstract int MinListElements { get; }

      /// <summary>
      /// Gets the maximum number of elements this described list is allowed to carry.
      /// </summary>
      protected abstract int MaxListElements { get; }

      protected virtual object ReadSingle(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         int size = listDecoder.ReadSize(buffer, state);
         long expectedEndPos = buffer.ReadOffset + size;

         if (size > buffer.ReadableBytes || size < 0)
         {
            throw new DecodeException(String.Format(
               "List encoded size is specified to be greater than the amount " +
               "of data available s:({0}) r:({1})", (uint) size, buffer.ReadableBytes));
         }

         int count = listDecoder.ReadCount(buffer, state);

         if (count > size || count < 0)
         {
            throw new DecodeException(String.Format(
               "List encoded count is specified to be greater than the reported encoded size " +
               "s:({0}) c:({1})", size, (uint) count));
         }

         if (count < MinListElements)
         {
            throw new DecodeException(String.Format(
                "Not enough list elements indicated in the encoded count, expected {0} but got {1}",
                MinListElements, count));
         }

         if (count > MaxListElements)
         {
            throw new DecodeException(String.Format(
                "To many elements indicated in the encoded count, maximum {0} but got {1}",
                MaxListElements, count));
         }

         object type = ReadType(count, buffer, state.Decoder, state);

         if (buffer.ReadOffset != expectedEndPos)
         {
            throw new DecodeException("List decoding did not read the expected amount of bytes: " + size);
         }

         return type;
      }

      protected virtual object ReadSingle(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         int size = listDecoder.ReadSize(stream, state);

         if (size > state.MaxListSize || size < 0)
         {
            throw new DecodeException(String.Format(
                "List encoded size is specified to be greater than the configured maximum " +
                "List size allowed s:({0}) c:({1})", (uint) size, state.MaxListSize));
         }

         int count = listDecoder.ReadCount(stream, state);

         if (count > size || count < 0)
         {
            throw new DecodeException(String.Format(
                "List encoded count is specified to be greater than the reported encoded size " +
                "s:(%d) c:(%d)", size, (uint) count));
         }

         if (count < MinListElements)
         {
            throw new DecodeException(String.Format(
                "Not enough list elements indicated in the encoded count, expected {0} but got {1}",
                MinListElements, count));
         }

         if (count > MaxListElements)
         {
            throw new DecodeException(String.Format(
                "To many elements indicated in the encoded count, maximum {0} but got {1}",
                MaxListElements, count));
         }

         return ReadType(count, stream, state.Decoder, state);
      }

      /// <summary>
      /// Reads the actual type from the byte stream with the given number of encoded list elements populated
      /// </summary>
      /// <returns>The decoded described type</returns>
      protected abstract object ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state);

      /// <summary>
      /// Reads the actual type from the byte stream with the given number of encoded list elements populated
      /// </summary>
      /// <returns>The decoded described type</returns>
      protected abstract object ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state);

   }
}
