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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Defines an interface for an encoder of a specific type.
   /// </summary>
   public interface ITypeEncoder
   {
      /// <summary>
      /// The Type that this decoder can write.
      /// </summary>
      /// <returns>The Type value that this type encoder writes</returns>
      Type EncodesType { get; }

      /// <summary>
      /// Returns true if the value that this encoder is going to write is an array type.
      /// </summary>
      /// <returns>true if the decoder instance will read an array types</returns>
      bool IsArrayType { get; }

      /// <summary>
      /// Encodes the given value into the provided buffer.
      /// </summary>
      /// <param name="buffer">The buffer where the encoded bytes are to be written</param>
      /// <param name="state">The encoder state to use when writing the bytes</param>
      /// <param name="value">The value to be encoded</param>
      void WriteType(IProtonBuffer buffer, IEncoderState state, object value);

      /// <summary>
      /// Encodes a full array encoding of the given array elements into the provided buffer.
      /// </summary>
      /// <param name="buffer">The buffer where the encoded bytes are to be written</param>
      /// <param name="state">The encoder state to use when writing the bytes</param>
      /// <param name="value">The array value to be encoded</param>
      void WriteArray(IProtonBuffer buffer, IEncoderState state, Array value);

      /// <summary>
      /// Encodes only the individual elements of the given array into the provided buffer
      /// </summary>
      /// <param name="buffer">The buffer where the encoded bytes are to be written</param>
      /// <param name="state">The encoder state to use when writing the bytes</param>
      /// <param name="value">The array value to be encoded</param>
      void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values);

   }
}