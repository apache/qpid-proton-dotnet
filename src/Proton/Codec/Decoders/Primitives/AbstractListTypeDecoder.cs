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
      public override Type DecodesType => typeof(IList);

      public IList<T> ReadList<T>(IProtonBuffer buffer, IDecoderState state)
      {
         int size = ReadSize(buffer, state);

         // Ensure we do not allocate an array of size greater then the available data, otherwise there is a risk for an OOM error
         if (size > buffer.ReadableBytes)
         {
            throw new DecodeException(string.Format(
                    "List element size {0} is specified to be greater than the amount " +
                    "of data available ({1})", size, buffer.ReadableBytes));
         }

         int count = ReadCount(buffer, state);

         if (count > buffer.ReadableBytes)
         {
            throw new DecodeException(string.Format(
                    "List encoded element count {0} is specified to be greater than the amount " +
                    "of data available ({1})", count, buffer.ReadableBytes));
         }

         IList<T> list = new List<T>(count);
         for (int i = 0; i < count; i++)
         {
            list.Add(state.Decoder.ReadObject<T>(buffer, state));
         }

         return list;
      }

      public IList<T> ReadList<T>(Stream stream, IStreamDecoderState state)
      {
         ReadSize(stream, state);
         int count = ReadCount(stream, state);

         IList<T> list = new List<T>(count);
         for (int i = 0; i < count; i++)
         {
            list.Add(state.Decoder.ReadObject<T>(stream, state));
         }

         return list;
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
         buffer.SkipBytes(ReadSize(buffer, state));
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

      #region Abstract IListTypeDecoder methods

      public abstract int ReadCount(IProtonBuffer buffer, IDecoderState state);

      public abstract int ReadCount(Stream stream, IStreamDecoderState state);

      public abstract int ReadSize(IProtonBuffer buffer, IDecoderState state);

      public abstract int ReadSize(Stream stream, IStreamDecoderState state);

      #endregion
   }
}