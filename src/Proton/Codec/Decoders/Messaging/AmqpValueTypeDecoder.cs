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
   public sealed class AmqpValueTypeDecoder : AbstractDescribedTypeDecoder
   {
      public override Symbol DescriptorSymbol => AmqpValue.DescriptorSymbol;

      public override ulong DescriptorCode => AmqpValue.DescriptorCode;

      public override Type DecodesType() => typeof(AmqpValue);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         return new AmqpValue(decoder.ReadValue(buffer, state));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         Array elements = decoder.ReadArrayElements(buffer, state, count);

         AmqpValue[] result = new AmqpValue[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = new AmqpValue(elements.GetValue(i));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         state.Decoder.ReadNextTypeDecoder(buffer, state).SkipValue(buffer, state);
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         return new AmqpValue(decoder.ReadValue(stream, state));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         Array elements = decoder.ReadArrayElements(stream, state, count);

         AmqpValue[] result = new AmqpValue[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = new AmqpValue(elements.GetValue(i));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         state.Decoder.ReadNextTypeDecoder(stream, state).SkipValue(stream, state);
      }
   }
}