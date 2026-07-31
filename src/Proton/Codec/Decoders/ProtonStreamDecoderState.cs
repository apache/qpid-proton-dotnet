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

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public class ProtonStreamDecoderState : IStreamDecoderState
   {
      /// <summary>
      /// The default number of elements a zero width array encoding is allowed to have
      /// before an decoder exception is thrown.
      /// </summary>
      public static readonly uint DefaultMaxZeroWidthArrayElements = 0;

      /// <summary>
      /// The default maximum depth the decoder will allow before triggering an DecodeException when the
      /// state depth value is increased during a decode process of complex types that can next objects.
      /// </summary>
      public static readonly uint DefaultMaxDecodeDepth = 32;

      private readonly ProtonStreamDecoder decoder;

      public IStreamDecoder Decoder => decoder;

      private uint decodeDepth;

      public ProtonStreamDecoderState(ProtonStreamDecoder parent) : base()
      {
         this.decoder = parent;
      }

      /// <summary>
      /// Sets a custom UTF-8 string decoder that will be used for all string decoding
      /// done from the decoder associated with this decoder state instance.  If no
      /// decoder is registered then the implementation uses its own decoding algorithm.
      /// </summary>
      public IUtf8StreamDecoder Utf8Decoder { get; set; }

      public uint MaxZeroWidthArrayElements { get; set; } = DefaultMaxZeroWidthArrayElements;

      public uint MaxStringSize { get; set; } = IStreamDecoderState.DefaultMaxAllocationLimit;

      public uint MaxArraySize { get; set; } = IStreamDecoderState.DefaultMaxAllocationLimit;

      public uint MaxBinarySize { get; set; } = IStreamDecoderState.DefaultMaxAllocationLimit;

      public uint MaxSymbolSize { get; set; } = IStreamDecoderState.DefaultMaxAllocationLimit;

      public uint MaxListSize { get; set; } = IStreamDecoderState.DefaultMaxAllocationLimit;

      public uint MaxMapSize { get; set; } = IStreamDecoderState.DefaultMaxAllocationLimit;

      public void Reset()
      {
         decodeDepth = 0;
      }

      public uint DepthLimit { get; set; } = DefaultMaxDecodeDepth;

      public void IncreaseDepth()
      {
         if (++decodeDepth > DepthLimit)
         {
            --decodeDepth; // Unwind decrement to ensure the depth returns to zero.
            throw new DecodeException(
               "The nesting of types in the object being decoded exceeded the configured limit: " + DepthLimit);
         }
      }

      public void DecreaseDepth()
      {
         decodeDepth = decodeDepth > 0 ? decodeDepth - 1 : 0;
      }

      public string DecodeUtf8(Stream stream, int length)
      {
         if (length < 0)
         {
            throw new DecodeException("Specified UTF length:" + length + " cannot be negative.");
         }

         if (length > MaxStringSize)
         {
            throw new DecodeException(string.Format(
                "String encoded size %d is specified to be greater than the amount " +
                "of data available (%d)", length, MaxStringSize));
         }

         if (Utf8Decoder == null)
         {
            return InternalDecode(stream, length);
         }
         else
         {
            try
            {
               return Utf8Decoder.DecodeUTF8(stream, length);
            }
            catch (Exception ex)
            {
               throw new DecodeException("Cannot parse encoded UTF8 String", ex);
            }
         }
      }

      private static string InternalDecode(Stream stream, int length)
      {
         byte[] byteArray = new byte[length];
         if (stream.Read(byteArray, 0, length) != length)
         {
            throw new DecodeEOFException("Unable to read the required number of bytes to decode a string");
         }

         return System.Text.Encoding.UTF8.GetString(byteArray);
      }
   }
}