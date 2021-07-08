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
   public sealed class RejectedTypeEncoder : AbstractDescribedListTypeEncoder<Rejected>
   {
      public override Symbol DescriptorSymbol => Rejected.DescriptorSymbol;

      public override ulong DescriptorCode => Rejected.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Rejected value)
      {
         if (value.Error != null)
         {
            return EncodingCodes.List32;
         }
         else
         {
            return EncodingCodes.List8;
         }
      }

      protected override int GetElementCount(Rejected value)
      {
         if (value.Error != null)
         {
            return 1;
         }
         else
         {
            return 0;
         }
      }

      protected override void WriteElement(Rejected source, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).

         switch (index)
         {
            case 0:
               state.Encoder.WriteObject(buffer, state, source.Error);
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Rejected value index: " + index);
         }
      }
   }
}