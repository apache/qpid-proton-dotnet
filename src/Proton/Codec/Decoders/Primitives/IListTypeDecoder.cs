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

using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   /// <summary>
   /// Interface for type decoders that handle AMQP List encodings.
   /// </summary>
   public interface IListTypeDecoder : IPrimitiveTypeDecoder
   {
      /// <summary>
      /// Attempts to read and decode an AMQP List from the given incoming byte
      /// buffer. The decoder applies type restrictions to the incoming List
      /// based on the types given in the methods generic parameters. The resulting
      /// IList is created using the type parameters provided to this method.
      /// </summary>
      /// <typeparam name="T">The type restriction for the entries of the List</typeparam>
      /// <param name="buffer">The buffer where the List should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>An IList that was created using the type parameters</returns>
      IList<T> ReadList<T>(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Attempts to read and decode an AMQP List from the given incoming byte
      /// stream. The decoder applies type restrictions to the incoming List
      /// based on the types given in the methods generic parameters. The resulting
      /// IList is created using the type parameters provided to this method.
      /// </summary>
      /// <typeparam name="T">The type restriction for the entries of the List</typeparam>
      /// <param name="stream">The stream where the List should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>An IList that was created using the type parameters</returns>
      IList<T> ReadList<T>(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the encoded size value for the encoded list payload and returns it. The
      /// read is destructive and the type decoder read methods cannot be called after this
      /// this unless the IProtonBuffer is reset via a position marker. This method can
      /// be useful when the caller intends to manually read the binary payload from the
      /// given IProtonBuffer.
      /// </summary>
      /// <param name="buffer">The buffer where the size should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>the size of the list payload that is encoded in the given buffer</returns>
      int ReadSize(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the number of elements contained in the encoded list from the provided IProtonBuffer.
      /// The implementation must read the correct size encoding based on the type of list that this
      /// type decoder handles.
      /// </summary>
      /// <param name="buffer">The buffer where the count should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>the count of the list elements that is encoded in the given buffer</returns>
      int ReadCount(IProtonBuffer buffer, IDecoderState state);

      /// <summary>
      /// Reads the encoded size value for the encoded list payload and returns it. The
      /// read is destructive and the type decoder read methods cannot be called after this
      /// this unless the Stream is reset via a position marker. This method can be useful
      /// when the caller intends to manually read the binary payload from the given stream.
      /// </summary>
      /// <param name="stream">The stream where the size should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>the size of the list payload that is encoded in the given stream</returns>
      int ReadSize(Stream stream, IStreamDecoderState state);

      /// <summary>
      /// Reads the number of elements contained in the encoded list from the provided Stream.
      /// The implementation must read the correct size encoding based on the type of list that this
      /// type decoder handles.
      /// </summary>
      /// <param name="stream">The stream where the element count should be read from</param>
      /// <param name="state">The decoder state that provides support</param>
      /// <returns>the count of the list elements that is encoded in the given buffer</returns>
      int ReadCount(Stream stream, IStreamDecoderState state);

   }
}