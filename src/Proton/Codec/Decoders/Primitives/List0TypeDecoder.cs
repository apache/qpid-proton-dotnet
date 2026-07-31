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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders.Primitives
{
   public sealed class List0TypeDecoder : AbstractPrimitiveTypeDecoder, IListTypeDecoder
   {
      public override EncodingCodes EncodingCode => EncodingCodes.List0;

      public override Type DecodesType => typeof(IList);

      public override bool IsZeroWidth => true;

      public int ReadCount(IProtonBuffer buffer, IDecoderState state)
      {
         return 0;
      }

      public int ReadCount(Stream stream, IStreamDecoderState state)
      {
         return 0;
      }

      public IList<T> ReadList<T>(IProtonBuffer buffer, IDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            return (IList<T>)Array.Empty<T>();
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public IList<T> ReadList<T>(Stream stream, IStreamDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            return (IList<T>)Array.Empty<T>();
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public int ReadSize(IProtonBuffer buffer, IDecoderState state)
      {
         return 0;
      }

      public int ReadSize(Stream stream, IStreamDecoderState state)
      {
         return 0;
      }

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            return (IList) Array.Empty<object>();
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            return (IList) Array.Empty<object>();
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         try
         {
            state.IncreaseDepth();
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         try
         {
            state.IncreaseDepth();
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      protected override void ValidateArrayPreconditions(IProtonBuffer buffer, IDecoderState state, int count)
      {
         if (count > state.MaxZeroWidthArrayElements || count < 0)
         {
            throw new DecodeException(string.Format(
               "Array size indicated {0} is greater than the amount of elements allowed for zero sized primitives ({1})",
               (uint) count, state.MaxZeroWidthArrayElements));
         }
      }

      protected override void ValidateArrayPreconditions(Stream stream, IStreamDecoderState state, int count)
      {
         if (count > state.MaxZeroWidthArrayElements || count < 0)
         {
            throw new DecodeException(string.Format(
               "Array size indicated {0} is greater than the amount of elements allowed for zero sized primitives ({1})",
               (uint) count, state.MaxZeroWidthArrayElements));
         }
      }
   }
}