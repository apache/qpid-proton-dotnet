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
   public sealed class HeaderTypeDecoder : AbstractDescribedListTypeDecoder
   {
      private static readonly int MinHeaderListEntries = 0;
      private static readonly int MaxHeaderListEntries = 5;

      public override Symbol DescriptorSymbol => Header.DescriptorSymbol;

      public override ulong DescriptorCode => Header.DescriptorCode;

      public override Type DecodesType => typeof(Header);

      protected override int MinListElements => MinHeaderListEntries;

      protected override int MaxListElements => MaxHeaderListEntries;

      protected override Header ReadType(int count, IProtonBuffer buffer, IDecoder decoder, IDecoderState state)
      {
         Header result = new();

         for (int index = 0; index < count; ++index)
         {
            // Peek ahead and see if there is a null in the next slot, if so we don't call
            // the setter for that entry to ensure the returned type reflects the encoded
            // state in the modification entry.
            bool nullValue = buffer.GetByte(buffer.ReadOffset) == (byte)EncodingCodes.Null;
            if (nullValue)
            {
               buffer.ReadByte();
               continue;
            }

            switch (index)
            {
               case 0:
                  result.Durable = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 1:
                  result.Priority = state.Decoder.ReadUnsignedByte(buffer, state) ?? Header.DEFAULT_PRIORITY;
                  break;
               case 2:
                  result.TimeToLive = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
               case 3:
                  result.FirstAcquirer = state.Decoder.ReadBoolean(buffer, state) ?? false;
                  break;
               case 4:
                  result.DeliveryCount = state.Decoder.ReadUnsignedInteger(buffer, state) ?? 0;
                  break;
            }
         }

         return result;
      }

      protected override Header ReadType(int count, Stream stream, IStreamDecoder decoder, IStreamDecoderState state)
      {
         Header result = new();

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
                  result.Durable = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 1:
                  result.Priority = state.Decoder.ReadUnsignedByte(stream, state) ?? Header.DEFAULT_PRIORITY;
                  break;
               case 2:
                  result.TimeToLive = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
               case 3:
                  result.FirstAcquirer = state.Decoder.ReadBoolean(stream, state) ?? false;
                  break;
               case 4:
                  result.DeliveryCount = state.Decoder.ReadUnsignedInteger(stream, state) ?? 0;
                  break;
            }
         }

         return result;
      }
   }
}