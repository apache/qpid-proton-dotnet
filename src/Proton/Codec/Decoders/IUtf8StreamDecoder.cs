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

using System.IO;

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Interface for an external UTF8 Decoder that can be supplied by a client
   /// which implements custom decoding logic optimized for the application using
   /// the Codec.
   /// </summary>
   public interface IUtf8StreamDecoder
   {
      /// <summary>
      /// Decodes a String from the given UTF8 Bytes advancing the buffer read index
      /// by the given length value once complete.  If the implementation does not advance
      /// the buffer read index the outcome of future decode calls is not defined.
      /// </summary>
      /// <param name="stream">The stream that carries the UTF8 bytes</param>
      /// <param name="utf8length">the length of the UTF8 string</param>
      /// <returns>The decoded UTF-8 string</returns>
      string DecodeUTF8(Stream stream, int utf8length);

   }
}