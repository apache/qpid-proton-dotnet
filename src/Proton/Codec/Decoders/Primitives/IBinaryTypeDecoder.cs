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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   public interface IBinaryTypeDecoder : IPrimitiveTypeDecoder
   {
      /// <summary>
      /// Reads the encoded size value for the encoded binary payload and returns it. The
      /// read is destructive and the type decoder read methods cannot be called after this
      /// this unless the IProtonBuffer is reset via a position marker. This method can
      /// be useful when the caller intends to manually read the binary payload from the
      /// given IProtonBuffer.
      /// </summary>
      /// <param name="buffer">The buffer where the size should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>the size of the binary payload that is encoded in the given buffer</returns>
      int ReadSize(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded size value for the encoded binary payload and returns it. The
      /// read is destructive and the type decoder read methods cannot be called after this
      /// this unless the Stream is reset via a position marker. This method can be useful
      /// when the caller intends to manually read the binary payload from the given stream.
      /// </summary>
      /// <param name="stream">The stream where the size should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>the size of the binary payload that is encoded in the given stream</returns>
      int ReadSize(Stream stream, IStreamDecoderState state);

   }
}