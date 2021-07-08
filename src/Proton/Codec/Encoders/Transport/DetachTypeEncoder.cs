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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Codec.Encoders.Transport
{
   public sealed class DetachTypeEncoder : AbstractDescribedListTypeEncoder<Detach>
   {
      public override Symbol DescriptorSymbol => Detach.DescriptorSymbol;

      public override ulong DescriptorCode => Detach.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Detach value)
      {
        return value.Error == null ? EncodingCodes.List8 : EncodingCodes.List32;
      }

      protected override int GetElementCount(Detach begin)
      {
         return begin.GetElementCount();
      }

      protected override int GetMinElementCount()
      {
         return 1;
      }

      protected override void WriteElement(Detach detach, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
               if (detach.HasHandle())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, detach.Handle);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               buffer.WriteByte((sbyte)(detach.Closed ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               break;
            case 2:
               state.Encoder.WriteObject(buffer, state, detach.Error);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Detach value index: " + index);
         }
      }
   }
}