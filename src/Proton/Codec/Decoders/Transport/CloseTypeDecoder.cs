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
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Decoders.Transport
{
   public sealed class CloseTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinCloseListEntries = 0;
      private static readonly int MaxCloseListEntries = 1;

      public override Symbol DescriptorSymbol => Close.DescriptorSymbol;

      public override ulong DescriptorCode => Close.DescriptorCode;

      public override Type DecodesType => typeof(Close);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadClose(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         Close[] result = new Close[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadClose(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private Close ReadClose(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Close result = new Close();

         int size = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinCloseListEntries)
         {
            throw new DecodeException("Not enough entries in Close list encoding: " + count);
         }
         else if (count > MaxCloseListEntries)
         {
            throw new DecodeException("To many entries in Close list encoding: " + count);
         }
         else if (count == 1)
         {
            result.Error = state.Decoder.ReadObject<ErrorCondition>(buffer, state);
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadClose(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         Close[] result = new Close[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadClose(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private Close ReadClose(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Close result = new Close();

         int size = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinCloseListEntries)
         {
            throw new DecodeException("Not enough entries in Close list encoding: " + count);
         }
         else if (count > MaxCloseListEntries)
         {
            throw new DecodeException("To many entries in Close list encoding: " + count);
         }
         else if (count == 1)
         {
            result.Error = state.Decoder.ReadObject<ErrorCondition>(stream, state);
         }

         return result;
      }
   }
}
