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
   public sealed class DeliveryAnnotationsTypeDecoder : AbstractDescribedTypeDecoder
   {
      public override Symbol DescriptorSymbol => DeliveryAnnotations.DescriptorSymbol;

      public override ulong DescriptorCode => DeliveryAnnotations.DescriptorCode;

      public override Type DecodesType => typeof(DeliveryAnnotations);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         if (decoder is NullTypeDecoder)
         {
            return new DeliveryAnnotations();
         }

         return new DeliveryAnnotations(ReadMap(buffer, state, CheckIsExpectedTypeAndCast<IMapTypeDecoder>(decoder)));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         DeliveryAnnotations[] result = new DeliveryAnnotations[count];

         if (decoder is NullTypeDecoder)
         {
            for (int i = 0; i < count; ++i)
            {
               result[i] = new DeliveryAnnotations();
            }
            return result;
         }

         for (int i = 0; i < count; ++i)
         {
            result[i] = new DeliveryAnnotations(
               ReadMap(buffer, state, CheckIsExpectedTypeAndCast<IMapTypeDecoder>(decoder)));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         if (decoder is not NullTypeDecoder)
         {
            CheckIsExpectedType<IMapTypeDecoder>(decoder);
            decoder.SkipValue(buffer, state);
         }
      }

      private IDictionary<Symbol, object> ReadMap(IProtonBuffer buffer, IDecoderState state, IMapTypeDecoder mapDecoder)
      {
         int size = mapDecoder.ReadSize(buffer, state);
         int count = mapDecoder.ReadCount(buffer, state);

         if (count > buffer.ReadableBytes)
         {
            throw new DecodeException(string.Format(
                    "Map encoded size %d is specified to be greater than the amount " +
                    "of data available (%d)", size, buffer.ReadableBytes));
         }

         IDecoder decoder = state.Decoder;

         // Count include both key and value so we must include that in the loop
         IDictionary<Symbol, object> map = new Dictionary<Symbol, object>(count);
         for (int i = 0; i < count / 2; i++)
         {
            Symbol key = decoder.ReadSymbol(buffer, state);
            Object value = decoder.ReadObject(buffer, state);

            map.Add(key, value);
         }

         return map;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         if (decoder is NullTypeDecoder)
         {
            return new DeliveryAnnotations();
         }

         return new DeliveryAnnotations(ReadMap(stream, state, CheckIsExpectedTypeAndCast<IMapTypeDecoder>(decoder)));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         DeliveryAnnotations[] result = new DeliveryAnnotations[count];

         if (decoder is NullTypeDecoder)
         {
            for (int i = 0; i < count; ++i)
            {
               result[i] = new DeliveryAnnotations();
            }
            return result;
         }

         for (int i = 0; i < count; ++i)
         {
            result[i] = new DeliveryAnnotations(
               ReadMap(stream, state, CheckIsExpectedTypeAndCast<IMapTypeDecoder>(decoder)));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         if (decoder is not NullTypeDecoder)
         {
            CheckIsExpectedType<IMapTypeDecoder>(decoder);
            decoder.SkipValue(stream, state);
         }
      }

      private IDictionary<Symbol, object> ReadMap(Stream stream, IStreamDecoderState state, IMapTypeDecoder mapDecoder)
      {
         int size = mapDecoder.ReadSize(stream, state);
         int count = mapDecoder.ReadCount(stream, state);

         IStreamDecoder decoder = state.Decoder;

         // Count include both key and value so we must include that in the loop
         IDictionary<Symbol, object> map = new Dictionary<Symbol, object>(count);
         for (int i = 0; i < count / 2; i++)
         {
            Symbol key = decoder.ReadSymbol(stream, state);
            Object value = decoder.ReadObject(stream, state);

            map.Add(key, value);
         }

         return map;
      }
   }
}