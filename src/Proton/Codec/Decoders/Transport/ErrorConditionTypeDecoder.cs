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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Decoders.Transport
{
   public sealed class ErrorConditionTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinErrorConditionListEntries = 1;
      private static readonly int MaxErrorConditionListEntries = 3;

      public override Symbol DescriptorSymbol => ErrorCondition.DescriptorSymbol;

      public override ulong DescriptorCode => ErrorCondition.DescriptorCode;

      public override Type DecodesType => typeof(ErrorCondition);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadErrorCondition(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         ErrorCondition[] result = new ErrorCondition[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadErrorCondition(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static ErrorCondition ReadErrorCondition(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinErrorConditionListEntries)
         {
            throw new DecodeException("Not enough entries in ErrorCondition list encoding: " + count);
         }

         if (count > MaxErrorConditionListEntries)
         {
            throw new DecodeException("To many entries in ErrorCondition list encoding: " + count);
         }

         Symbol condition = null;
         string description = null;
         IDictionary<Symbol, object> info = null;

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  condition = state.Decoder.ReadSymbol(buffer, state);
                  break;
               case 1:
                  description = state.Decoder.ReadString(buffer, state);
                  break;
               case 2:
                  info = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
            }
         }

         return new ErrorCondition(condition, description, info);
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadErrorCondition(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         ErrorCondition[] result = new ErrorCondition[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadErrorCondition(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static ErrorCondition ReadErrorCondition(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinErrorConditionListEntries)
         {
            throw new DecodeException("Not enough entries in ErrorCondition list encoding: " + count);
         }

         if (count > MaxErrorConditionListEntries)
         {
            throw new DecodeException("To many entries in ErrorCondition list encoding: " + count);
         }

         Symbol condition = null;
         string description = null;
         IDictionary<Symbol, object> info = null;

         for (int index = 0; index < count; ++index)
         {
            switch (index)
            {
               case 0:
                  condition = state.Decoder.ReadSymbol(stream, state);
                  break;
               case 1:
                  description = state.Decoder.ReadString(stream, state);
                  break;
               case 2:
                  info = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
            }
         }

         return new ErrorCondition(condition, description, info);
      }
   }
}