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
   public sealed class TargetTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinTargetListEntries = 0;
      private static readonly int MaxTargetListEntries = 7;

      public override Symbol DescriptorSymbol => Target.DescriptorSymbol;

      public override ulong DescriptorCode => Target.DescriptorCode;

      public override Type DecodesType => typeof(Target);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadTarget(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Target[] result = new Target[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadTarget(buffer, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private Target ReadTarget(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Target result = new Target();

         int size = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         // Don't decode anything if things already look wrong.
         if (count < MinTargetListEntries)
         {
            throw new DecodeException("Not enough entries in Target list encoding: " + count);
         }

         if (count > MaxTargetListEntries)
         {
            throw new DecodeException("To many entries in Target list encoding: " + count);
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
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadTarget(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Target[] result = new Target[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadTarget(stream, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private Target ReadTarget(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Target result = new Target();

         int size = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         // Don't decode anything if things already look wrong.
         if (count < MinTargetListEntries)
         {
            throw new DecodeException("Not enough entries in Target list encoding: " + count);
         }

         if (count > MaxTargetListEntries)
         {
            throw new DecodeException("To many entries in Target list encoding: " + count);
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
                  result.Capabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
            }
         }

         return result;
      }
   }
}