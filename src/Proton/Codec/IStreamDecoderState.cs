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
   /// Defines a state object that is used with the AMQP Decoder type to hold
   /// intermediate state and provide additional functionality that can be used
   /// during the decode process.
   /// </summary>
   public interface IStreamDecoderState
   {
      /// <summary>
      /// Resets the decoder after a complete decode operation freeing any held
      /// resources and preparing for a new decode operation.
      /// </summary>
      void Reset();

      /// <summary>
      /// Gets the IDecoder instance that was used when creating this decoder state object.
      /// </summary>
      IStreamDecoder Decoder { get; }

      /// <summary>
      /// Decodes an encoded UTF-8 string value from the given buffer.  The number of bytes
      /// that comprise the encoding is provided by the caller, the decoder should not read
      /// more than that number of bytes from the provided buffer.
      /// </summary>
      /// <param name="buffer">The buffer where the bytes are to be read from.</param>
      /// <param name="length">The encoded size of the UTF-8 string.</param>
      /// <returns>A string value decoded from the UTF-8 bytes</returns>
      string DecodeUtf8(Stream stream, int length);

   }
}
