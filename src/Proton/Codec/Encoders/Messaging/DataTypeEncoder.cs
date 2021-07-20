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
   public sealed class DataTypeEncoder : AbstractDescribedTypeEncoder
   {
      public override Symbol DescriptorSymbol => Data.DescriptorSymbol;

      public override ulong DescriptorCode => Data.DescriptorCode;

      public override Type EncodesType => typeof(Data);

      public override void WriteArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         // Write the Array Type encoding code, we don't optimize here.
         buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);

         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.WriteInt(0);
         buffer.WriteInt(values.Length);

         WriteRawArray(buffer, state, values);

         // Move back and write the size
         long endIndex = buffer.WriteOffset;
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
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)Data.DescriptorCode);

         buffer.WriteUnsignedByte((byte)EncodingCodes.VBin32);
         foreach (object value in values)
         {
            IProtonBuffer binary = (IProtonBuffer)value;
            buffer.WriteInt((int) binary.ReadableBytes);
            buffer.WriteBytes(binary);
         }
      }

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         buffer.WriteUnsignedByte((byte)EncodingCodes.DescribedTypeIndicator);
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)Data.DescriptorCode);

         state.Encoder.WriteBinary(buffer, state, ((Data)value).Value);
      }
   }
}