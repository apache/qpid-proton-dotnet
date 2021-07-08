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

namespace Apache.Qpid.Proton.Codec.Encoders.Messaging
{
   public sealed class SourceTypeEncoder : AbstractDescribedListTypeEncoder<Source>
   {
      public override Symbol DescriptorSymbol => Source.DescriptorSymbol;

      public override ulong DescriptorCode => Source.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Source value)
      {
         return EncodingCodes.List32;
      }

      protected override int GetElementCount(Source source)
      {
         if (source.Capabilities != null)
         {
            return 11;
         }
         else if (source.Outcomes != null)
         {
            return 10;
         }
         else if (source.DefaultOutcome != null)
         {
            return 9;
         }
         else if (source.Filter != null)
         {
            return 8;
         }
         else if (source.DistributionMode != null)
         {
            return 7;
         }
         else if (source.DynamicNodeProperties != null)
         {
            return 6;
         }
         else if (source.Dynamic)
         {
            return 5;
         }
         else if (source.Timeout != 0)
         {
            return 4;
         }
         else if (source.ExpiryPolicy != TerminusExpiryPolicy.SessionEnd)
         {
            return 3;
         }
         else if (source.Durable != TerminusDurability.None)
         {
            return 2;
         }
         else if (source.Address != null)
         {
            return 1;
         }
         else
         {
            return 0;
         }
      }

      protected override void WriteElement(Source source, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
                state.Encoder.WriteString(buffer, state, source.Address);
                break;
            case 1:
                state.Encoder.WriteUnsignedInteger(buffer, state, source.Durable.ToUInt32());
                break;
            case 2:
                state.Encoder.WriteSymbol(buffer, state, source.ExpiryPolicy.ToSymbol());
                break;
            case 3:
                state.Encoder.WriteUnsignedInteger(buffer, state, source.Timeout);
                break;
            case 4:
                buffer.WriteUnsignedByte((byte)(source.Dynamic ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
                break;
            case 5:
                state.Encoder.WriteMap(buffer, state, source.DynamicNodeProperties);
                break;
            case 6:
                state.Encoder.WriteSymbol(buffer, state, source.DistributionMode);
                break;
            case 7:
                state.Encoder.WriteMap(buffer, state, source.Filter);
                break;
            case 8:
                state.Encoder.WriteObject(buffer, state, source.DefaultOutcome);
                break;
            case 9:
                state.Encoder.WriteArray(buffer, state, source.Outcomes);
                break;
            case 10:
                state.Encoder.WriteArray(buffer, state, source.Capabilities);
                break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Source value index: " + index);
         }
      }
   }
}