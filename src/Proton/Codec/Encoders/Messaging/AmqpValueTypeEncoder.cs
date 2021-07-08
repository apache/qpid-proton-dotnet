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
using System.Collections;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Codec.Encoders.Primitives
{
   public sealed class AmqpValueTypeEncoder : AbstractDescribedTypeEncoder
   {
      public override Symbol DescriptorSymbol => AmqpValue.DescriptorSymbol;

      public override ulong DescriptorCode => AmqpValue.DescriptorCode;

      public override Type EncodesType => typeof(AmqpValue);

      public override void WriteArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         // Write the Array Type encoding code, we don't optimize here.
         buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);

         int startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.WriteInt(0);
         buffer.WriteInt(values.Length);

         WriteRawArray(buffer, state, values);

         // Move back and write the size
         int endIndex = buffer.WriteOffset;
         long writeSize = endIndex - startIndex - sizeof(int);

         if (writeSize > Int32.MaxValue)
         {
            throw new ArgumentOutOfRangeException("Cannot encode given array, encoded size to large: " + writeSize);
         }

         buffer.SetInt(startIndex, (int)writeSize);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         buffer.WriteUnsignedByte((byte)EncodingCodes.DescribedTypeIndicator);
         state.Encoder.WriteUnsignedLong(buffer, state, DescriptorCode);

         IList[] elements = new IList[values.Length];

         for (int i = 0; i < values.Length; ++i)
         {
            AmqpSequence sequence = (AmqpSequence)values[i];
            elements[i] = (IList)sequence.Value;
         }

         ITypeEncoder entryEncoder = state.Encoder.LookupTypeEncoder(values[0].GetType());
         entryEncoder.WriteRawArray(buffer, state, elements);
      }

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         buffer.WriteUnsignedByte((byte)EncodingCodes.DescribedTypeIndicator);
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpSequence.DescriptorCode);

        state.Encoder.WriteObject(buffer, state, ((AmqpValue)value).Value);
      }
   }
}