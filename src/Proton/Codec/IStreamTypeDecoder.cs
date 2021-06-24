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

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Defines an interface for an decoder of a specific type.
   /// </summary>
   public interface IStreamTypeDecoder
   {
      /// <summary>
      /// The Type that this decoder can read.
      /// </summary>
      /// <returns>The Type value that this type decoder decodes</returns>
      Type DecodesType();

      /// <summary>
      /// Returns true if the value that this decoder is going to read is an array type.
      /// </summary>
      /// <returns>true if the decoder instance will read an array types</returns>
      bool IsArrayType();

      /// <summary>
      /// Reads this decoders managed type from the given buffer and returns it.
      /// </summary>
      /// <param name="stream">The stream where the encoded bytes can be read from</param>
      /// <param name="state">The decoder state that can be used during decode</param>
      /// <returns>The decoded value from the byte stream</returns>
      object ReadValue(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Skips the value that this decoder is handling by skipping the encoded bytes
      /// in the provided buffer instance.
      /// </summary>
      /// <param name="stream">The stream where the encoded bytes can be read from</param>
      /// <param name="state">The decoder state that can be used during decode</param>
      void SkipValue(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads a series of this type that have been encoded into the body of an Array type.
      /// When encoded into an array the values are encoded in series following the identifier
      /// for the type, this method is given a count of the number of instances that are encoded
      /// and should read each in succession and returning them in a new array.
      /// </summary>
      /// <param name="stream">The stream where the encoded bytes can be read from</param>
      /// <param name="state">The decoder state that can be used during decode</param>
      /// <param name="count">the number of elements that the encoded array contains</param>
      /// <returns>The decoded array from the given stream</returns>
      Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count);

   }
}