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
   /// Defines a state object that is used with the AMQP Decoder type to hold
   /// intermediate state and provide additional functionality that can be used
   /// during the decode process.
   /// </summary>
   public interface IDecoderState
   {
      /// <summary>
      /// Resets the decoder after a complete decode operation freeing any held
      /// resources and preparing for a new decode operation.
      /// </summary>
      void Reset();

      /// <summary>
      /// Gets the IDecoder instance that was used when creating this decoder state object.
      /// </summary>
      IDecoder Decoder { get; }

      /// <summary>
      /// The configured maximum number of elements that can be decoded from an array
      /// encoded with the zero width AMQP types (Null, UInt0. ULong0, List0, Boolean_False
      /// and Boolean_True). These are uncommon encodings and can lead to small encodings
      /// with large memory costs at decode which makes them discouraged for normal use.
      /// A value of zero (the default) should be handled by the implementation as meaning
      /// that only an array of zero width types with zero elements can be decoded.
      /// </summary>
      uint MaxZeroWidthArrayElements
      {
         get => 0;
         set => throw new NotImplementedException("Default implementation cannot set an array width  limit");
      }

      /// <summary>
      /// Access the configured maximum depth that nested types such as Lists, Maps and Arrays
      /// can have before a DecodeException is thrown to allow the decoder to error
      /// in cases where the depth of encoding exceeds what the environment is thought to be
      /// able to support.
      /// </summary>
      uint DepthLimit
      {
         get => uint.MaxValue;
         set => throw new NotSupportedException("Depth limits not supported by default");
      }

      /// <summary>
      /// During decode of AMQP types which can be comprised of a nesting of other AMQP types
      /// the such as Lists, Maps and Arrays, the depth is increased to track the amount of
      /// type nesting that comprises the type being decoded. Implementations can use this
      /// value to impose limits on the depth of nested objects within complex types and throw
      /// an DecodeException if that depth value is reached.
      /// </summary>
      void IncreaseDepth()
      {
      }

      /// <summary>
      /// Called once decoding of one level of a nested type completes to reduce to the previous
      /// level before proceeding to the next element on the current level if any.
      /// </summary>
      void DecreaseDepth()
      {
      }

      /// <summary>
      /// Decodes an encoded UTF-8 string value from the given buffer.  The number of bytes
      /// that comprise the encoding is provided by the caller, the decoder should not read
      /// more than that number of bytes from the provided buffer.
      /// </summary>
      /// <param name="buffer">The buffer where the bytes are to be read from.</param>
      /// <param name="length">The encoded size of the UTF-8 string.</param>
      /// <returns>A string value decoded from the UTF-8 bytes</returns>
      string DecodeUtf8(IProtonBuffer buffer, int length);

   }
}
