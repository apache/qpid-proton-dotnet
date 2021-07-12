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
   public abstract class AbstractDescribedMapTypeEncoder<K, V, M> : AbstractDescribedTypeEncoder
   {
      public override Type EncodesType => typeof(M);

      /// <summary>
      /// Determine the map type the given value can be encoded to based on the number of
      /// bytes that would be needed to hold the encoded form of the resulting map entries.
      /// </summary>
      /// <remarks>
      /// Most encoders will return MAP32 but for cases where the type is known to be
      /// be encoded to MAP8 the encoder can optimize the encode step and not compute
      /// sizes.
      /// </remarks>
      /// <param name="value">The value that is encoded as a map type</param>
      /// <returns>The encoding code to use to write the map body</returns>
      protected virtual EncodingCodes GetMapEncoding(M value)
      {
         return EncodingCodes.Map32;
      }

      /// <summary>
      /// Returns false when the value to be encoded has no Map body and can be
      /// written as a Null body type instead of a Map type.
      /// </summary>
      /// <param name="value">The value that is encoded as a map type</param>
      /// <returns>if the map type needs a map body or can be null</returns>
      protected abstract bool HasMap(M value);

      /// <summary>
      /// Gets the number of elements that will result when this type is encoded
      /// into an AMQP Map type.
      /// </summary>
      /// <param name="value">The value that is encoded as a map type</param>
      /// <returns>The number of elements to encode in the map body</returns>
      protected abstract int GetMapEntries(M value);

      /// <summary>
      /// Performs the write of the Map entries to the given buffer, the caller
      /// takes care of writing the Map preamble and tracking the final size of
      /// the written elements of the Map.
      /// </summary>
      /// <param name="buffer">The buffer to write the map entries into</param>
      /// <param name="state">The encoder state to use when writing</param>
      /// <param name="value">The value to be encoded</param>
      protected abstract void WriteMapEntries(IProtonBuffer buffer, IEncoderState state, M value);

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         this.WriteType(buffer, state, (M)value);
      }

      public virtual void WriteType(IProtonBuffer buffer, IEncoderState state, M value)
      {
         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         state.Encoder.WriteUnsignedLong(buffer, state, DescriptorCode);

         if (HasMap(value))
         {
            int count = GetMapEntries(value);
            EncodingCodes encodingCode = GetMapEncoding(value);

            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)encodingCode));

            switch (encodingCode)
            {
               case EncodingCodes.Map8:
                  WriteSmallType(buffer, state, value, count);
                  break;
               case EncodingCodes.Map32:
                  WriteLargeType(buffer, state, value, count);
                  break;
            }
         }
         else
         {
            state.Encoder.WriteNull(buffer, state);
         }
      }

      private void WriteSmallType(IProtonBuffer buffer, IEncoderState state, M value, int elementCount)
      {
         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.EnsureWritable(sizeof(byte) + sizeof(byte));
         buffer.WriteUnsignedByte((byte)0);
         buffer.WriteUnsignedByte((byte)(elementCount * 2));

         WriteMapEntries(buffer, state, value);

         // Move back and write the size
         long writeSize = (buffer.WriteOffset - startIndex) - sizeof(byte);

         buffer.SetUnsignedByte(startIndex, ((byte)writeSize));
      }

      private void WriteLargeType(IProtonBuffer buffer, IEncoderState state, M value, int elementCount)
      {
         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.EnsureWritable(sizeof(int) + sizeof(int));
         buffer.WriteInt(0);
         buffer.WriteInt(elementCount * 2);

         WriteMapEntries(buffer, state, value);

         // Move back and write the size
         long writeSize = (buffer.WriteOffset - startIndex) - sizeof(int);

         buffer.SetInt(startIndex, (int)writeSize);
      }

      public override void WriteArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         buffer.EnsureWritable(sizeof(int) + sizeof(int) + sizeof(byte) + sizeof(byte));
         // Write the Array Type encoding code, we don't optimize here.
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));

         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.WriteInt(0);
         buffer.WriteInt(values.Length);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));

         state.Encoder.WriteUnsignedLong(buffer, state, DescriptorCode);

         WriteRawArray(buffer, state, values);

         // Move back and write the size
         long writeSize = buffer.WriteOffset - startIndex - sizeof(int);

         if (writeSize > Int32.MaxValue)
         {
            throw new ArgumentOutOfRangeException("Cannot encode given array, encoded size to large: " + writeSize);
         }

         buffer.SetInt(startIndex, (int)writeSize);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Object[] values)
      {
         buffer.EnsureWritable(sizeof(int) + sizeof(int) + sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));

         for (int i = 0; i < values.Length; ++i)
         {
            M map = (M)values[i];
            int count = GetMapEntries(map);
            long mapStartIndex = buffer.WriteOffset;

            // Reserve space for the size and write the count of list elements.
            buffer.WriteInt(0);
            buffer.WriteInt(count * 2);

            WriteMapEntries(buffer, state, map);

            // Move back and write the size
            long writeSize = buffer.WriteOffset - mapStartIndex - sizeof(int);

            buffer.SetInt(mapStartIndex, (int)writeSize);
         }
      }
   }
}