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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Decoders.Messaging
{
   public sealed class SourceTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinSourceListEntries = 0;
      private static readonly int MaxSourceListEntries = 11;

      public override Symbol DescriptorSymbol => Source.DescriptorSymbol;

      public override ulong DescriptorCode => Source.DescriptorCode;

      public override Type DecodesType() => typeof(Source);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadSource(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Source[] result = new Source[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadSource(buffer, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private Source ReadSource(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Source result = new Source();

         int size = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         // Don't decode anything if things already look wrong.
         if (count < MinSourceListEntries)
         {
            throw new DecodeException("Not enough entries in Source list encoding: " + count);
         }

         if (count > MaxSourceListEntries)
         {
            throw new DecodeException("To many entries in Source list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  result.Address = state.Decoder.ReadString(buffer, state);
                  break;
               case 1:
                  uint durability = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  result.Durable = TerminusDurabilityExtension.ValueOf(durability);
                  break;
               case 2:
                  Symbol expiryPolicy = state.Decoder.ReadSymbol(buffer, state);
                  result.ExpiryPolicy = TerminusExpiryPolicyExtension.ValueOf(expiryPolicy);
                  break;
               case 3:
                  result.Timeout = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 4:
                  result.Dynamic = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 5:
                  result.DynamicNodeProperties = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
               case 6:
                  result.DistributionMode = state.Decoder.ReadSymbol(buffer, state);
                  break;
               case 7:
                  result.Filter = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
               case 8:
                  result.DefaultOutcome = state.Decoder.ReadObject<IOutcome>(buffer, state);
                  break;
               case 9:
                  result.Outcomes = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 10:
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadSource(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Source[] result = new Source[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadSource(stream, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private Source ReadSource(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Source result = new Source();

         int size = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         // Don't decode anything if things already look wrong.
         if (count < MinSourceListEntries)
         {
            throw new DecodeException("Not enough entries in Source list encoding: " + count);
         }

         if (count > MaxSourceListEntries)
         {
            throw new DecodeException("To many entries in Source list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  result.Address = state.Decoder.ReadString(stream, state);
                  break;
               case 1:
                  uint durability = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  result.Durable = TerminusDurabilityExtension.ValueOf(durability);
                  break;
               case 2:
                  Symbol expiryPolicy = state.Decoder.ReadSymbol(stream, state);
                  result.ExpiryPolicy = TerminusExpiryPolicyExtension.ValueOf(expiryPolicy);
                  break;
               case 3:
                  result.Timeout = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 4:
                  result.Dynamic = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 5:
                  result.DynamicNodeProperties = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
               case 6:
                  result.DistributionMode = state.Decoder.ReadSymbol(stream, state);
                  break;
               case 7:
                  result.Filter = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
               case 8:
                  result.DefaultOutcome = state.Decoder.ReadObject<IOutcome>(stream, state);
                  break;
               case 9:
                  result.Outcomes = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 10:
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
            }
         }

         return result;
      }
   }
}