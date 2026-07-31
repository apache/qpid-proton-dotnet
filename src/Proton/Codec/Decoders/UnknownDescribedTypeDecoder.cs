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

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public sealed class UnknownDescribedTypeDecoder : AbstractDescribedTypeDecoder
   {
      private readonly object descriptor;

      public UnknownDescribedTypeDecoder(object descriptor) : base()
      {
         this.descriptor = descriptor;
      }

      public override Symbol DescriptorSymbol => descriptor as Symbol;

      public override ulong DescriptorCode => ((ulong)(descriptor is ulong ? descriptor : 0));

      public override Type DecodesType => typeof(IDescribedType);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         if (decoder is not IPrimitiveTypeDecoder)
         {
            throw new DecodeException("The described value must be an AMQP primitive type.");
         }

         return new UnknownDescribedType(descriptor, decoder.ReadValue(buffer, state));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         if (decoder is IPrimitiveTypeDecoder primitiveDecoder)
         {
            if (primitiveDecoder.IsZeroWidth)
            {
               if (count > state.MaxZeroWidthArrayElements)
               {
                  throw new DecodeException(
                     "Array element count " + count + " is specified to be greater than limit for " +
                     "zero sized encoded array types (" + state.MaxZeroWidthArrayElements + ")");
               }
            }
            else if (count > buffer.ReadableBytes)
            {
               throw new DecodeException(
                  "Array encoded length " + count + " is specified to be greater than the amount " +
                  "of the remaining readable bytes (" + buffer.ReadableBytes + ")");
            }
         }
         else
         {
            throw new DecodeException("The described value must be an AMQP primitive type.");
         }

         UnknownDescribedType[] result = new UnknownDescribedType[count];

         for (int i = 0; i < count; ++i)
         {
            object described = decoder.ReadValue(buffer, state);
            result[i] = new UnknownDescribedType(descriptor, described);
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            state.Decoder.ReadNextTypeDecoder(buffer, state).SkipValue(buffer, state);
         }
         finally
         {
            state.DecreaseDepth();
         }
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         if (decoder is not IPrimitiveTypeDecoder)
         {
            throw new DecodeException("The described value must be an AMQP primitive type.");
         }

         return new UnknownDescribedType(descriptor, decoder.ReadValue(stream, state));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         if (decoder is IPrimitiveTypeDecoder primitiveDecoder)
         {
            if (primitiveDecoder.IsZeroWidth)
            {
               if (count > state.MaxZeroWidthArrayElements)
               {
                  throw new DecodeException(
                     "Array element count " + count + " is specified to be greater than limit for " +
                     "zero sized encoded array types (" + state.MaxZeroWidthArrayElements + ")");
               }
            }
            else if (count > state.MaxArraySize)
            {
               throw new DecodeException(
                  "Array encoded length " + count + " is specified to be greater than the amount " +
                  "of the configured max array length (" + state.MaxArraySize + ")");
            }
         }
         else
         {
            throw new DecodeException("The described value must be an AMQP primitive type.");
         }

         UnknownDescribedType[] result = new UnknownDescribedType[count];

         for (int i = 0; i < count; ++i)
         {
            object described = decoder.ReadValue(stream, state);
            result[i] = new UnknownDescribedType(descriptor, described);
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         state.IncreaseDepth();

         try
         {
            state.Decoder.ReadNextTypeDecoder(stream, state).SkipValue(stream, state);
         }
         finally
         {
            state.DecreaseDepth();
         }
      }
   }
}
