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
   public sealed class EndTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinEndListEntries = 0;
      private static readonly int MaxEndListEntries = 1;

      public override Symbol DescriptorSymbol => End.DescriptorSymbol;

      public override ulong DescriptorCode => End.DescriptorCode;

      public override Type DecodesType() => typeof(End);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadEnd(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         End[] result = new End[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadEnd(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private End ReadEnd(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         End result = new End();

         int size = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinEndListEntries)
         {
            throw new DecodeException("Not enough entries in End list encoding: " + count);
         }

         if (count > MaxEndListEntries)
         {
            throw new DecodeException("To many entries in End list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
                case 0:
                    result.Error = state.Decoder.ReadObject<ErrorCondition>(buffer, state);
                    break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadEnd(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         End[] result = new End[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadEnd(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private End ReadEnd(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         End result = new End();

         int size = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinEndListEntries)
         {
            throw new DecodeException("Not enough entries in End list encoding: " + count);
         }

         if (count > MaxEndListEntries)
         {
            throw new DecodeException("To many entries in End list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
                case 0:
                    result.Error = state.Decoder.ReadObject<ErrorCondition>(stream, state);
                    break;
            }
         }

         return result;
      }
   }
}