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
using Apache.Qpid.Proton.Codec.Encoders;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Utilities
{
   public class NoLocalTypeEncoder : AbstractDescribedTypeEncoder
   {
      public override Symbol DescriptorSymbol => NoLocalType.DescriptorSymbol;

      public override ulong DescriptorCode => NoLocalType.DescriptorCode;

      public override Type EncodesType => typeof(NoLocalType);

      public override void WriteArray(IProtonBuffer buffer, IEncoderState state, Array value)
      {
         // Write the Array Type encoding code, we don't optimize here.
         buffer.EnsureWritable(sizeof(long) + sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));

         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.WriteInt(0);
         buffer.WriteInt(value.Length);

         WriteRawArray(buffer, state, value);

         // Move back and write the size
         long endIndex = buffer.WriteOffset;
         long writeSize = endIndex - startIndex - sizeof(int);

         if (writeSize > Int32.MaxValue)
         {
            throw new ArgumentOutOfRangeException("Cannot encode given array, encoded size to large: " + writeSize);
         }

         buffer.SetInt(startIndex, (int)writeSize);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));

         state.Encoder.WriteUnsignedLong(buffer, state, DescriptorCode);

         Object[] elements = new Object[values.Length];

         for (int i = 0; i < values.Length; ++i)
         {
            NoLocalType value = (NoLocalType)values.GetValue(i);
            elements[i] = value.Described;
         }

         ITypeEncoder entryEncoder = state.Encoder.LookupTypeEncoder(typeof(string));
         entryEncoder.WriteArray(buffer, state, elements);
      }

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));

         state.Encoder.WriteUnsignedLong(buffer, state, DescriptorCode);
         state.Encoder.WriteString(buffer, state, (string)(((NoLocalType)value).Described));
      }
   }
}