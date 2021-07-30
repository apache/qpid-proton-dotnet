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
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Encoders.Transport
{
   public sealed class DispositionTypeEncoder : AbstractDescribedListTypeEncoder<Disposition>
   {
      public override Symbol DescriptorSymbol => Disposition.DescriptorSymbol;

      public override ulong DescriptorCode => Disposition.DescriptorCode;

      protected override EncodingCodes GetListEncoding(Disposition value)
      {
         if (value.State == null || value.State == Accepted.Instance || value.State == Released.Instance)
         {
            return EncodingCodes.List8;
         }
         else
         {
            return EncodingCodes.List32;
         }
      }

      protected override int GetElementCount(Disposition value)
      {
         return value.GetElementCount();
      }

      protected override int GetMinElementCount()
      {
         return 2;
      }

      protected override void WriteElement(Disposition disposition, int index, IProtonBuffer buffer, IEncoderState state)
      {
         // When encoding ensure that values that were never set are omitted and a simple
         // NULL entry is written in the slot instead (don't write defaults).
         buffer.EnsureWritable(sizeof(int));

         switch (index)
         {
            case 0:
               if (disposition.HasRole())
               {
                  buffer.WriteUnsignedByte((byte)disposition.Role.ToBooleanEncoding());
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 1:
               if (disposition.HasFirst())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, disposition.First);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 2:
               if (disposition.HasLast())
               {
                  state.Encoder.WriteUnsignedInteger(buffer, state, disposition.Last);
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 3:
               if (disposition.HasSettled())
               {
                  buffer.WriteUnsignedByte((byte)(disposition.Settled ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            case 4:
               if (disposition.HasState())
               {
                  if (disposition.State == Accepted.Instance)
                  {
                     buffer.WriteUnsignedByte((byte)EncodingCodes.DescribedTypeIndicator);
                     buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
                     buffer.WriteUnsignedByte((byte)Accepted.DescriptorCode);
                     buffer.WriteUnsignedByte((byte)EncodingCodes.List0);
                  }
                  else
                  {
                     state.Encoder.WriteObject(buffer, state, disposition.State);
                  }
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }

               break;
            case 5:
               if (disposition.HasBatchable())
               {
                  buffer.WriteUnsignedByte((byte)(disposition.Batchable ? EncodingCodes.BooleanTrue : EncodingCodes.BooleanFalse));
               }
               else
               {
                  buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
               }
               break;
            default:
               throw new ArgumentOutOfRangeException("Unknown Disposition value index: " + index);
         }
      }
   }
}