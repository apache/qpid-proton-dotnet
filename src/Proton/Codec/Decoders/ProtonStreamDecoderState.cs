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
      private readonly ProtonStreamDecoder decoder;

      public IStreamDecoder Decoder => decoder;

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

      public void Reset()
      {
         throw new NotImplementedException();
      }

      public string DecodeUtf8(Stream stream, int length)
      {
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