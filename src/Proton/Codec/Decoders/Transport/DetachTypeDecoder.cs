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
   public sealed class DetachTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinDetachListEntries = 1;
      private static readonly int MaxDetachListEntries = 3;

      public override Symbol DescriptorSymbol => Detach.DescriptorSymbol;

      public override ulong DescriptorCode => Detach.DescriptorCode;

      public override Type DecodesType => typeof(Detach);

      protected override int MinListElements => MinDetachListEntries;

      protected override int MaxListElements => MaxDetachListEntries;

      protected override Detach ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Detach result = new();

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

      protected override Detach ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Detach result = new();

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
                  stream.Seek(-1, SeekOrigin.Current);
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

      private static string ErrorForMissingRequiredFields(int present)
      {
         return present switch
         {
            _ => "The Handle field cannot be omitted from the Detach",
         };
      }
   }
}