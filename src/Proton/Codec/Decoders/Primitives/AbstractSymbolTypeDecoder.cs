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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   /// <summary>
   /// Base symbol type decoder used by decoders of various AMQP types that represent
   /// map style serialized objects.
   /// </summary>
   public abstract class AbstractSymbolTypeDecoder : AbstractPrimitiveTypeDecoder, ISymbolTypeDecoder
   {
      public override Type DecodesType => typeof(Symbol);

      public override Symbol ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         int length = ReadSize(buffer, state);

         if (length == 0)
         {
            return Symbol.Lookup("");
         }

         if (length > buffer.ReadableBytes || length < 0)
         {
            throw new DecodeException(string.Format(
                    "Symbol encoded size {0} is specified to be greater than the amount " +
                    "of data available {1}", (uint) length, buffer.ReadableBytes));
         }

         IProtonBuffer symbolBuffer = buffer.Copy(buffer.ReadOffset, length);

         buffer.SkipBytes(length);

         return SymbolLookup(symbolBuffer, true);
      }

      public override Symbol ReadValue(Stream stream, IStreamDecoderState state)
      {
         int length = ReadSize(stream, state);

         if (length == 0)
         {
            return Symbol.Lookup("");
         }

         if (length > state.MaxSymbolSize || length < 0)
         {
            throw new DecodeException(String.Format(
                  "Binary encoded length is specified to be greater than the maximum allowed length " +
                  "l:(%d) m:(%d)", (uint) length, state.MaxSymbolSize));
         }

         byte[] symbolBytes;

         try
         {
            symbolBytes = ProtonStreamReadUtils.ReadBytes(stream, length);
         }
         catch (IOException ex)
         {
            throw new DecodeException("Error while reading Symbol payload bytes", ex);
         }

         return SymbolLookup(ProtonByteBufferAllocator.Instance.Wrap(symbolBytes), false);
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         int length = ReadSize(buffer, state);

         if (length > buffer.ReadableBytes || length < 0)
         {
            throw new DecodeException(string.Format(
                  "Symbol encoded size {0} is specified to be greater than the amount " +
                  "of data available {1}", (uint) length, buffer.ReadableBytes));
         }

         buffer.SkipBytes(length);
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         int length = ReadSize(stream, state);

         if (length > state.MaxSymbolSize || length < 0)
         {
            throw new DecodeException(String.Format(
                  "Binary encoded length is specified to be greater than the maximum allowed length " +
                  "l:(%d) m:(%d)", (uint) length, state.MaxSymbolSize));
         }

         ProtonStreamReadUtils.SkipBytes(stream, length);
      }

      protected abstract int ReadSize(IProtonBuffer buffer, IDecoderState state);

      protected abstract int ReadSize(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Gets a singleton Symbol instance that matches the given IProtonBuffer byte view
      /// of the Symbol. A subclass can override this to produce the Symbol singleton from
      /// a source other than the default which is the general symbol cache.
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="copyOnCreate"></param>
      /// <returns>A Symbol object that is backed by the value in the buffer</returns>
      protected virtual Symbol SymbolLookup(IProtonBuffer buffer, bool copyOnCreate)
      {
         return Symbol.Lookup(buffer, copyOnCreate);
      }
   }
}