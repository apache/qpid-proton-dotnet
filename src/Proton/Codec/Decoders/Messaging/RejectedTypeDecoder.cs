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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Decoders.Messaging
{
   public sealed class RejectedTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinRejectedListEntries = 0;
      private static readonly int MaxRejectedListEntries = 1;

      public override Symbol DescriptorSymbol => Rejected.DescriptorSymbol;

      public override ulong DescriptorCode => Rejected.DescriptorCode;

      public override Type DecodesType => typeof(Rejected);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadRejected(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Rejected[] result = new Rejected[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadRejected(buffer, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static Rejected ReadRejected(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Rejected result = new Rejected();

         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         // Don't decode anything if things already look wrong.
         if (count < MinRejectedListEntries)
         {
            throw new DecodeException("Not enough entries in Rejected list encoding: " + count);
         }

         if (count > MaxRejectedListEntries)
         {
            throw new DecodeException("To many entries in Rejected list encoding: " + count);
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

         return ReadRejected(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);
         IListTypeDecoder listDecoder = CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder);

         Rejected[] result = new Rejected[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadRejected(stream, state, listDecoder);
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static Rejected ReadRejected(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Rejected result = new Rejected();

         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         // Don't decode anything if things already look wrong.
         if (count < MinRejectedListEntries)
         {
            throw new DecodeException("Not enough entries in Rejected list encoding: " + count);
         }

         if (count > MaxRejectedListEntries)
         {
            throw new DecodeException("To many entries in Rejected list encoding: " + count);
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