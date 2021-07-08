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

namespace Apache.Qpid.Proton.Codec.Encoders
{
   public abstract class AbstractPrimitiveTypeEncoder<T> : IPrimitiveTypeEncoder<T>
   {
      public Type EncodesType => typeof(T);

      #region Abstract API that cannot be generically implemented here

      public abstract void WriteRawArray(IProtonBuffer buffer, IEncoderState state, object[] values);

      public abstract void WriteType(IProtonBuffer buffer, IEncoderState state, object value);

      #endregion

      public bool IsArrayType => false;

      public virtual void WriteArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         WriteAsArray32(buffer, state, values);
      }

      protected void WriteAsArray8(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         // Reserve the capacity for the Array preamble
         buffer.EnsureWritable(sizeof(byte) * 3);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Array8));

         int startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte((byte)values.Length);

         // Write the array elements after writing the array length
         WriteRawArray(buffer, state, values);

         // Move back and write the size
         int endIndex = buffer.WriteOffset;
         long writeSize = endIndex - startIndex - sizeof(byte);

         buffer.SetUnsignedByte(startIndex, (byte)writeSize);
      }

      protected void WriteAsArray32(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         // Reserve the capacity for the Array preamble
         buffer.EnsureWritable(sizeof(byte) + sizeof(uint) + sizeof(uint));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));

         int startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.WriteInt(0);
         buffer.WriteInt(values.Length);

         // Write the array elements after writing the array length
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
   }
}