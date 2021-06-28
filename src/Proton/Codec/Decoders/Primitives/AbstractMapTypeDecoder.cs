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
      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         int size = ReadSize(buffer, state);

         // Ensure we do not allocate an array of size greater then the available data, otherwise there is a risk for an OOM error
         if (size > buffer.ReadableBytes)
         {
            throw new DecodeException(string.Format(
                    "Map element size %d is specified to be greater than the amount " +
                    "of data available (%d)", size, buffer.ReadableBytes));
         }

         int count = ReadCount(buffer, state);

         if (count % 2 != 0)
         {
            throw new DecodeException(string.Format(
                "Map encoded number of elements %d is not an even number.", count));
         }

         // Count include both key and value so we must include that in the loop
         IDictionary<object, object> map = new Dictionary<object, object>(count);
         for (int i = 0; i < count / 2; i++)
         {
            object key = state.Decoder.ReadObject(buffer, state);
            object value = state.Decoder.ReadObject(buffer, state);

            map.Add(key, value);
         }

         return map;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         ReadSize(stream, state);
         int count = ReadCount(stream, state);

         if (count % 2 != 0)
         {
            throw new DecodeException(string.Format(
                "Map encoded number of elements %d is not an even number.", count));
         }

         // Count include both key and value so we must include that in the loop
         IDictionary<object, object> map = new Dictionary<object, object>(count);
         for (int i = 0; i < count / 2; i++)
         {
            object key = state.Decoder.ReadObject(stream, state);
            object value = state.Decoder.ReadObject(stream, state);

            map.Add(key, value);
         }

         return map;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         try
         {
            ProtonStreamReadUtils.SkipBytes(stream, ReadSize(stream, state));
         }
         catch (IOException ex)
         {
            throw new DecodeException("Error while reading List payload bytes", ex);
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