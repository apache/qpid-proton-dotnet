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
   public sealed class DetachTypeDecoder : AbstractDescribedTypeDecoder
   {
      private static readonly int MinDetachListEntries = 1;
      private static readonly int MaxDetachListEntries = 3;

      public override Symbol DescriptorSymbol => Detach.DescriptorSymbol;

      public override ulong DescriptorCode => Detach.DescriptorCode;

      public override Type DecodesType() => typeof(Detach);

      public override object ReadValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         return ReadDetach(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(IProtonBuffer buffer, IDecoderState state, int count)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         Detach[] result = new Detach[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadDetach(buffer, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(buffer, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(buffer, state);
      }

      private Detach ReadDetach(IProtonBuffer buffer, IDecoderState state, IListTypeDecoder listDecoder)
      {
         Detach result = new Detach();

         int size = listDecoder.ReadSize(buffer, state);
         int count = listDecoder.ReadCount(buffer, state);

         if (count < MinDetachListEntries)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MaxDetachListEntries)
         {
            throw new DecodeException("To many entries in Detach list encoding: " + count);
         }

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            bool nullValue = buffer.GetByte(buffer.ReadOffset) == (byte)EncodingCodes.Null;
            if (nullValue)
            {
               // Ensure mandatory fields are set
               if (index < MinDetachListEntries)
               {
                  throw new DecodeException(ErrorForMissingRequiredFields(index));
               }

               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
                case 0:
                    result.Handle = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                    break;
                case 1:
                    result.Closed = state.Decoder.ReadBoolean(buffer, state) ?? false;
                    break;
                case 2:
                    result.Error = state.Decoder.ReadObject<ErrorCondition>(buffer, state);
                    break;
            }
         }

         return result;
      }

      public override object ReadValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         return ReadDetach(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
      }

      public override Array ReadArrayElements(Stream stream, IStreamDecoderState state, int count)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         Detach[] result = new Detach[count];
         for (int i = 0; i < count; ++i)
         {
            result[i] = ReadDetach(stream, state, CheckIsExpectedTypeAndCast<IListTypeDecoder>(decoder));
         }

         return result;
      }

      public override void SkipValue(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = state.Decoder.ReadNextTypeDecoder(stream, state);

         CheckIsExpectedType<IListTypeDecoder>(decoder);

         decoder.SkipValue(stream, state);
      }

      private Detach ReadDetach(Stream stream, IStreamDecoderState state, IListTypeDecoder listDecoder)
      {
         Detach result = new Detach();

         int size = listDecoder.ReadSize(stream, state);
         int count = listDecoder.ReadCount(stream, state);

         if (count < MinDetachListEntries)
         {
            throw new DecodeException(ErrorForMissingRequiredFields(count));
         }

         if (count > MaxDetachListEntries)
         {
            throw new DecodeException("To many entries in Detach list encoding: " + count);
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
                  // Ensure mandatory fields are set
                  if (index < MinDetachListEntries)
                  {
                     throw new DecodeException(ErrorForMissingRequiredFields(index));
                  }

                  continue;
               }
               else
               {
                  stream.Seek(stream.Position - 1, SeekOrigin.Current);
               }
            }

            switch (index)
            {
                case 0:
                    result.Handle = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                    break;
                case 1:
                    result.Closed = state.Decoder.ReadBoolean(stream, state) ?? false;
                    break;
                case 2:
                    result.Error = state.Decoder.ReadObject<ErrorCondition>(stream, state);
                    break;
            }
         }

         return result;
      }

      private string ErrorForMissingRequiredFields(int present)
      {
         switch (present)
         {
            default:
               return "The Handle field cannot be omitted from the Detach";
         }
      }
   }
}