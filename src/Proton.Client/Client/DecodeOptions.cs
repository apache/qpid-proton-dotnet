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
using Apache.Qpid.Proton.Codec.Decoders;

namespace Apache.Qpid.Proton.Client
{
   public class DecodeOptions : ICloneable
   {
      /// <summary>
      /// Creates a default Decode options instance.
      /// </summary>
      public DecodeOptions() : base()
      {
      }

      /// <summary>
      /// Create a new Decode options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The decode options instance to copy</param>
      public DecodeOptions(DecodeOptions other) : base()
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
         return CopyInto(new DecodeOptions());
      }

      internal DecodeOptions CopyInto(DecodeOptions other)
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
      public uint MaxZeroWidthArrayElements { get; set; } = ProtonDecoderState.DefaultMaxZeroWidthArrayElements;

      /// <summary>
      /// Access the configured maximum depth that nested types such as Lists, Maps and Arrays
      /// can have before a DecodeException is thrown to allow the decoder to error
      /// in cases where the depth of encoding exceeds what the environment is thought to be
      /// able to support.
      /// </summary>
      public uint DepthLimit { get; set; } = ProtonDecoderState.DefaultMaxDecodeDepth;

   }
}