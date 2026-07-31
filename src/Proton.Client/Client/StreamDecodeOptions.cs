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
using System.Collections.Generic;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Codec.Decoders;

namespace Apache.Qpid.Proton.Client
{
   public class StreamDecodeOptions : ICloneable
   {
      /// <summary>
      /// A default used to limit reads from a stream for types that encode a count or
      /// size of encoded elements.
      /// </summary>
      protected static readonly uint DefaultMaxAllocationLimit = int.MaxValue - 8;

      /// <summary>
      /// Creates a default Decode options instance.
      /// </summary>
      public StreamDecodeOptions() : base()
      {
      }

      /// <summary>
      /// Create a new Decode options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The decode options instance to copy</param>
      public StreamDecodeOptions(StreamDecodeOptions other) : base()
      {
         other?.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new StreamDecodeOptions());
      }

      internal StreamDecodeOptions CopyInto(StreamDecodeOptions other)
      {
         other.MaxZeroWidthArrayElements = MaxZeroWidthArrayElements;
         other.DepthLimit = DepthLimit;

         return other;
      }

      /// <summary>
      /// The configured maximum number of elements that can be decoded from an array
      /// encoded with the zero width AMQP types (Null, UInt0. ULong0, List0, Boolean_False
      /// and Boolean_True). These are uncommon encodings and can lead to small encodings
      /// with large memory costs at decode which makes them discouraged for normal use.
      /// It is recommended that for implementations that implement this limit configuration
      /// the default be zero meaning zero width array encodings are disabled and will always
      /// throw a NotImplementedException.
      /// </summary>
      public uint MaxZeroWidthArrayElements { get; set; } = ProtonStreamDecoderState.DefaultMaxZeroWidthArrayElements;

      /// <summary>
      /// Access the configured maximum depth that nested types such as Lists, Maps and Arrays
      /// can have before a DecodeException is thrown to allow the decoder to error
      /// in cases where the depth of encoding exceeds what the environment is thought to be
      /// able to support.
      /// </summary>
      public uint DepthLimit { get; set; } = ProtonStreamDecoderState.DefaultMaxDecodeDepth;

      /// <summary>
      /// The configured maximum size of an encoded string that can be decoded before an error is thrown.
      /// </summary>
      public uint MaxStringSize { get; set; } = DefaultMaxAllocationLimit;

      /// <summary>
      /// The configured maximum size of an encoded array that can be decoded before an error is thrown.
      /// </summary>
      public uint MaxArraySize { get; set; } = DefaultMaxAllocationLimit;

      /// <summary>
      /// The configured maximum size of an encoded Binary that can be decoded before an error is thrown.
      /// </summary>
      public uint MaxBinarySize { get; set; } = DefaultMaxAllocationLimit;

      /// <summary>
      /// The configured maximum size of an encoded Symbol that can be decoded before an error is thrown.
      /// </summary>
      public uint MaxSymbolSize { get; set; } = DefaultMaxAllocationLimit;

      /// <summary>
      /// The configured maximum encoded list size that can be decoded before an error is thrown.
      /// </summary>
      public uint MaxListSize { get; set; } = DefaultMaxAllocationLimit;

      /// <summary>
      /// The configured maximum encoded map size that can be decoded before an error is thrown.
      /// </summary>
      public uint MaxMapSize { get; set; } = DefaultMaxAllocationLimit;

   }
}