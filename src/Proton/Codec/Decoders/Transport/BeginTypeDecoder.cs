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
   public sealed class BeginTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinBeginListEntries = 4;
      private static readonly int MaxBeginListEntries = 8;

      public override Symbol DescriptorSymbol => Begin.DescriptorSymbol;

      public override ulong DescriptorCode => Begin.DescriptorCode;

      public override Type DecodesType => typeof(Begin);

      protected override int MinListElements => MinBeginListEntries;

      protected override int MaxListElements => MaxBeginListEntries;

      protected override Begin ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Begin result = new();

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            bool nullValue = buffer.GetByte(buffer.ReadOffset) == (byte)EncodingCodes.Null;
            if (nullValue)
            {
               // Ensure mandatory fields are set, channel is not mandatory so it can
               // be null an so we have to account for that here.
               if (index > 0 && index < MinBeginListEntries)
               {
                  throw new DecodeException(ErrorForMissingRequiredFields(index));
               }

               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
               case 0:
                  result.RemoteChannel = state.Decoder.ReadUnsignedShort(buffer, state) ?? 0;
                  break;
               case 1:
                  result.NextOutgoingId = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 2:
                  result.IncomingWindow = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 3:
                  result.OutgoingWindow = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 4:
                  result.HandleMax = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 5:
                  result.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 6:
                  result.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(buffer, state);
                  break;
               case 7:
                  result.Properties = state.Decoder.ReadMap<Symbol, object>(buffer, state);
                  break;
            }
         }

         return result;
      }

      protected override Begin ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Begin result = new();

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
                  if (index < MinBeginListEntries)
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
                  result.RemoteChannel = state.Decoder.ReadUnsignedShort(stream, state) ?? 0;
                  break;
               case 1:
                  result.NextOutgoingId = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 2:
                  result.IncomingWindow = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 3:
                  result.OutgoingWindow = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 4:
                  result.HandleMax = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 5:
                  result.OfferedCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 6:
                  result.DesiredCapabilities = state.Decoder.ReadMultiple<Symbol>(stream, state);
                  break;
               case 7:
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
            3 => "The outgoing-window field cannot be omitted from the Begin",
            2 => "The incoming-window field cannot be omitted from the Begin",
            _ => "The next-outgoing-id field cannot be omitted from the Begin",
         };
      }
   }
}