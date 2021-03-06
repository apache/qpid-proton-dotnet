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
using Apache.Qpid.Proton.Types.Security;

namespace Apache.Qpid.Proton.Codec.Decoders.Security
{
   public sealed class SaslInitTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinListEntries = 0;
      private static readonly int MaxListEntries = 3;

      public override Symbol DescriptorSymbol => SaslInit.DescriptorSymbol;

      public override ulong DescriptorCode => SaslInit.DescriptorCode;

      public override Type DecodesType => typeof(SaslInit);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadType(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         SaslInit[] result = new SaslInit[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadType(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static SaslInit ReadType(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         SaslInit result = new();

         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         // Don't decode anything if things already look wrong.
         if (count < MinListEntries)
         {
            throw new DecodeException("Not enough entries in SaslInit list encoding: " + count);
         }

         if (count > MaxListEntries)
         {
            throw new DecodeException("To many entries in SaslInit list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  result.Mechanism = state.Decoder.ReadSymbol(buffer, state);
                  break;
               case 1:
                  result.InitialResponse = state.Decoder.ReadBinary(buffer, state);
                  break;
               case 2:
                  result.Hostname = state.Decoder.ReadString(buffer, state);
                  break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadType(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         SaslInit[] result = new SaslInit[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadType(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static SaslInit ReadType(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         SaslInit result = new();

         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         // Don't decode anything if things already look wrong.
         if (count < MinListEntries)
         {
            throw new DecodeException("Not enough entries in SaslInit list encoding: " + count);
         }

         if (count > MaxListEntries)
         {
            throw new DecodeException("To many entries in SaslInit list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  result.Mechanism = state.Decoder.ReadSymbol(stream, state);
                  break;
               case 1:
                  result.InitialResponse = state.Decoder.ReadBinary(stream, state);
                  break;
               case 2:
                  result.Hostname = state.Decoder.ReadString(stream, state);
                  break;
            }
         }

         return result;
      }
   }
}