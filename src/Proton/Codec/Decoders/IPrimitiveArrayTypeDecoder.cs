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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec
{
   /// <summary>
   /// Defines an interface for an decoder of a primitive array types
   /// </summary>
   public interface IPrimitiveArrayTypeDecoder : IPrimitiveTypeDecoder
   {
      /// <summary>
      /// Reads the given array from the bytes in the buffer but only if the type encoding
      /// of the given array matches the given Type filter. This allows the caller to
      /// effectively limit if the given array is decoded at all if the type encoding is
      /// not a match and also can act to limit an array to only a single level as the type
      /// encoding of nested arrays will not be the value type until the bottom of the array
      /// is reached. To allow all types and any array nesting the caller should pass the
      /// object Type.
      /// </summary>
      /// <param name="buffer"></param>
      /// <param name="state"></param>
      /// <param name="ofType"></param>
      /// <returns>An array containing elements of the given type</returns>
      object ReadValue(IProtonBuffer buffer, IDecoderState state, Type ofType);

      /// <summary>
      /// Reads the given array from the bytes in the buffer but only if the type encoding
      /// of the given array matches the given Type filter. This allows the caller to
      /// effectively limit if the given array is decoded at all if the type encoding is
      /// not a match and also can act to limit an array to only a single level as the type
      /// encoding of nested arrays will not be the value type until the bottom of the array
      /// is reached. To allow all types and any array nesting the caller should pass the
      /// object Type.
      /// </summary>
      /// <param name="stream"></param>
      /// <param name="state"></param>
      /// <param name="ofType"></param>
      /// <returns>An array containing elements of the given type</returns>
      object ReadValue(Stream stream, IStreamDecoderState state, Type ofType);

   }
}