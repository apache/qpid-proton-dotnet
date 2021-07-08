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
   public sealed class TargetTypeEncoder : AbstractDescribedListTypeEncoder<Target>
   {
      public override Symbol DescriptorSymbol => Target.DescriptorSymbol;

      public override ulong DescriptorCode => Target.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Target value)
      {
         return EncodingCodes.List32;
      }

      protected override int GetElementCount(Target target)
      {
         if (target.Capabilities != null)
         {
            return 7;
         }
         else if (target.DynamicNodeProperties != null)
         {
            return 6;
         }
         else if (target.Dynamic)
         {
            return 5;
         }
         else if (target.Timeout != 0)
         {
            return 4;
         }
         else if (target.ExpiryPolicy != TerminusExpiryPolicy.SessionEnd)
         {
            return 3;
         }
         else if (target.Durable != TerminusDurability.None)
         {
            return 2;
         }
         else if (target.Address != null)
         {
            return 1;
         }
         else
         {
            return 0;
         }
      }

      protected override void WriteElement(Target target, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
                state.Encoder.WriteString(buffer, state, target.Address);
                break;
            case 1:
                state.Encoder.WriteUnsignedInteger(buffer, state, target.Durable.ToUInt32());
                break;
            case 2:
                state.Encoder.WriteSymbol(buffer, state, target.ExpiryPolicy.ToSymbol());
                break;
            case 3:
                state.Encoder.WriteUnsignedInteger(buffer, state, target.Timeout);
                break;
            case 4:
                buffer.WriteUnsignedByte((byte)(target.Dynamic ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
                break;
            case 5:
                state.Encoder.WriteMap(buffer, state, target.DynamicNodeProperties);
                break;
            case 6:
                state.Encoder.WriteArray(buffer, state, target.Capabilities);
                break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Target value index: " + index);
         }
      }
   }
}