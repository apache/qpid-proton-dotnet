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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Decoders.Messaging
{
   public sealed class AmqpSequenceTypeDecoder : AbstractDescribedTypeDecoder
   {
      public override Symbol DescriptorSymbol => AmqpSequence.DescriptorSymbol;

      public override ulong DescriptorCode => AmqpSequence.DescriptorCode;

      public override Type DecodesType => typeof(AmqpSequence);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder valueDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);
         IList<object> result = (IList<object>)valueDecoder.ReadValue(buffer, state);

         return new AmqpSequence(result);
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder valueDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);
         List<object>[] elements = (List<object>[])valueDecoder.ReadArrayElements(buffer, state, count);

         AmqpSequence[] array = new AmqpSequence[count];
         for (int i = 0; i < count; ++i)
         {
            array[i] = new AmqpSequence(elements[i]);
         }

         return array;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder valueDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);
         IList<object> result = (IList<object>)valueDecoder.ReadValue(stream, state);

         return new AmqpSequence(result);
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder valueDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);
         List<object>[] elements = (List<object>[])valueDecoder.ReadArrayElements(stream, state, count);

         AmqpSequence[] array = new AmqpSequence[count];
         for (int i = 0; i < count; ++i)
         {
            array[i] = new AmqpSequence(elements[i]);
         }

         return array;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }
   }
}