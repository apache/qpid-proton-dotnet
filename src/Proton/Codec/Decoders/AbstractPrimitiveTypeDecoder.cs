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
      public virtual bool IsArrayType => false;

      public virtual bool IsZeroWidth => false;

      public virtual Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ValidateArrayPreconditions(buffer, state, count);

         Array array = Array.CreateInstance(DecodesType, count);
         for (int i = 0; i < count; ++i)
         {
            array.SetValue(ReadValue(buffer, state), i);
         }

         return array;
      }

      public virtual Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         ValidateArrayPreconditions(stream, state, count);

         Array array = Array.CreateInstance(DecodesType, count);
         for (int i = 0; i < count; ++i)
         {
            array.SetValue(ReadValue(stream, state), i);
         }

         return array;
      }

      protected virtual void ValidateArrayPreconditions(IProtonBuffer buffer, IDecoderState state, int count)
      {
         if (count > buffer.ReadableBytes || count < 0)
         {
            throw new DecodeException(string.Format(
               "Array count indicated {0} is greater than the amount of data available to decode ({1})",
               (uint) count, buffer.ReadableBytes));
         }
      }

      protected virtual void ValidateArrayPreconditions(Stream stream, IStreamDecoderState state, int count)
      {
         if (count > state.MaxArraySize || count < 0)
         {
            throw new DecodeException(string.Format(
               "Array count indicated {0} is greater than the amount of the configured max array size ({1})",
               (uint) count, state.MaxArraySize));
         }
      }

      #region Interface methods handed off to the subclass

      public abstract EncodingCodes EncodingCode { get; }

      public abstract Type DecodesType { get; }

      public abstract object ReadValue(IProtonBuffer buffer, IDecoderState state);

      public abstract object ReadValue(Stream stream, IStreamDecoderState state);

      public abstract void SkipValue(IProtonBuffer buffer, IDecoderState state);

      public abstract void SkipValue(Stream stream, IStreamDecoderState state);

      #endregion
   }
}