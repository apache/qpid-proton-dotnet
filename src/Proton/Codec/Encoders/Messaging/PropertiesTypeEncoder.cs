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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Encoders.Primitives
{
   public sealed class PropertiesTypeEncoder : AbstractDescribedListTypeEncoder<Properties>
   {
      public override Symbol DescriptorSymbol => Properties.DescriptorSymbol;

      public override ulong DescriptorCode => Properties.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Properties value)
      {
         return EncodingCodes.List32;
      }

      protected override int GetElementCount(Properties value)
      {
         return value.GetElementCount();
      }

      protected override void WriteElement(Properties properties, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
               state.Encoder.WriteObject(buffer, state, properties.MessageId);
               break;
            case 1:
               state.Encoder.WriteBinary(buffer, state, properties.UserId);
               break;
            case 2:
               state.Encoder.WriteString(buffer, state, properties.To);
               break;
            case 3:
               state.Encoder.WriteString(buffer, state, properties.Subject);
               break;
            case 4:
               state.Encoder.WriteString(buffer, state, properties.ReplyTo);
               break;
            case 5:
               state.Encoder.WriteObject(buffer, state, properties.CorrelationId);
               break;
            case 6:
               state.Encoder.WriteSymbol(buffer, state, properties.ContentType);
               break;
            case 7:
               state.Encoder.WriteSymbol(buffer, state, properties.ContentEncoding);
               break;
            case 8:
               if (properties.HasAbsoluteExpiryTime())
               {
                  state.Encoder.WriteTimestamp(buffer, state, properties.AbsoluteExpiryTime);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 9:
               if (properties.HasCreationTime())
               {
                  state.Encoder.WriteTimestamp(buffer, state, properties.CreationTime);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 10:
               state.Encoder.WriteString(buffer, state, properties.GroupId);
               break;
            case 11:
               if (properties.HasGroupSequence())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, properties.GroupSequence);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 12:
               state.Encoder.WriteString(buffer, state, properties.ReplyToGroupId);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Properties value index: " + index);
         }
      }
   }
}