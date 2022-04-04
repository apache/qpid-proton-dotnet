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
   public sealed class FlowTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinFlowListEntries = 4;
      private static readonly int MaxFlowListEntries = 11;

      public override Symbol DescriptorSymbol => Flow.DescriptorSymbol;

      public override ulong DescriptorCode => Flow.DescriptorCode;

      public override Type DecodesType => typeof(Flow);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadFlow(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         Flow[] result = new Flow[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadFlow(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private static Flow ReadFlow(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Flow result = new Flow();

         _ = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinFlowListEntries)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MaxFlowListEntries)
         {
            throw new DecodeException("To many entries in Flow list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            bool nullValue = buffer.GetByte(buffer.ReadOffset) == (byte)EncodingCodes.Null;
            if (nullValue)
            {
               // Ensure mandatory fields are set but account for the nextIncomingId
               // being optional for the Flow performative.
               if (index > 0 && index < MinFlowListEntries)
               {
                  throw new DecodeException(ErrorForMissingRequiredFields(index));
               }

               buffer.ReadOffset += 1;
               continue;
            }

            switch (index)
            {
               case 0:
                  result.NextIncomingId = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 1:
                  result.IncomingWindow = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 2:
                  result.NextOutgoingId = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 3:
                  result.OutgoingWindow = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 4:
                  result.Handle = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 5:
                  result.DeliveryCount = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 6:
                  result.LinkCredit = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 7:
                  result.Available = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 8:
                  result.Drain = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 9:
                  result.Echo = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 10:
                  result.Properties = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadFlow(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         Flow[] result = new Flow[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadFlow(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private static Flow ReadFlow(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Flow result = new Flow();

         _ = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinFlowListEntries)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MaxFlowListEntries)
         {
            throw new DecodeException("To many entries in Flow list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            if (stream.CanSeek)
            {
               bool nullValue = stream.ReadByte() == (byte)EncodingCodes.Null;
               if (nullValue)
               {
                  // Ensure mandatory fields are set but account for the nextIncomingId
                  // being optional for the Flow performative.
                  if (index > 0 && index < MinFlowListEntries)
                  {
                     throw new DecodeException(ErrorForMissingRequiredFields(index));
                  }

                  continue;
               }
               else
               {
                  stream.Seek(-1, SeekOrigin.Current);
               }
            }

            switch (index)
            {
               case 0:
                  result.NextIncomingId = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 1:
                  result.IncomingWindow = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 2:
                  result.NextOutgoingId = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 3:
                  result.OutgoingWindow = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 4:
                  result.Handle = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 5:
                  result.DeliveryCount = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 6:
                  result.LinkCredit = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 7:
                  result.Available = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 8:
                  result.Drain = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 9:
                  result.Echo = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 10:
                  result.Properties = state.Decoder.ReadMap<Symbol, object>(stream, state);
                  break;
            }
         }

         return result;
      }

      private static string ErrorForMissingRequiredFields(int present)
      {
         return present switch
         {
            3 => "The outgoing-window field cannot be omitted from the Flow",
            2 => "The next-outgoing-id field cannot be omitted from the Flow",
            _ => "The incoming-window field cannot be omitted from the Flow",
         };
      }
   }
}