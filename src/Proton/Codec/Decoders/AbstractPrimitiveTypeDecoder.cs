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

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public abstract class AbstractPrimitiveTypeDecoder : IPrimitiveTypeDecoder
   {
      public bool IsArrayType() => false;

      public Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         Array array = Array.CreateInstance(DecodesType(), count);
         for (int i = 0; i < count; ++i)
         {
            array.SetValue(ReadValue(buffer, state), i);
         }

         return array;
      }

      public Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         Array array = Array.CreateInstance(DecodesType(), count);
         for (int i = 0; i < count; ++i)
         {
            array.SetValue(ReadValue(stream, state), i);
         }

         return array;
      }

      #region Interface methods handed off to the subclass

      public abstract EncodingCodes EncodingCode { get; }

      public abstract Type DecodesType();

      public abstract object ReadValue(IProtonBuffer buffer, IDecoderState state);

      public abstract object ReadValue(Stream stream, IStreamDecoderState state);

      public abstract void SkipValue(IProtonBuffer buffer, IDecoderState state);

      public abstract void SkipValue(Stream stream, IStreamDecoderState state);

      #endregion
   }
}