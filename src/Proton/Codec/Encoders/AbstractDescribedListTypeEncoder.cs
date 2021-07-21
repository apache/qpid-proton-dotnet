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
   public abstract class AbstractDescribedListTypeEncoder<T> : AbstractDescribedTypeEncoder
   {
      public override Type EncodesType => typeof(T);

      /// <summary>
      /// Determine the list type the given value can be encoded to based on the number of
      /// bytes that would be needed to hold the encoded form of the resulting list entries.
      /// </summary>
      /// <remarks>
      /// Most encoders will return List32 but for cases where the type is known to be
      /// be encoded to List8 the encoder can optimize the encode step and not compute
      /// sizes.
      /// </remarks>
      /// <param name="value">The value that is encoded as a list type</param>
      /// <returns>The encoding code to use to write the list body</returns>
      protected virtual EncodingCodes GetListEncoding(T value)
      {
         return EncodingCodes.List32;
      }

      /// <summary>
      /// Instructs the encoder to write the element identified with the given index
      /// </summary>
      /// <param name="source">The source of the list entries to write</param>
      /// <param name="index">The index in the list that is currently being written</param>
      /// <param name="buffer">The buffer where the encoded entry is written</param>
      /// <param name="state">The encoder state to use when writing</param>
      protected abstract void WriteElement(T source, int index, IProtonBuffer buffer, IEncoderState state);

      /// <summary>
      /// Gets the number of elements that will result when this type is encoded
      /// into an AMQP List type.
      /// </summary>
      /// <param name="value">The value being encoded</param>
      /// <returns>The number of entries that will be encoded</returns>
      protected abstract int GetElementCount(T value);

      /// <summary>
      /// Return the minimum number of elements that this AMQP type must provide
      /// in order to be considered a valid type.
      /// </summary>
      /// <returns>A lower limit on the element count that can be encoded</returns>
      protected virtual int GetMinElementCount()
      {
         return 0;
      }

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         this.WriteType(buffer, state, (T)value);
      }

      public virtual void WriteType(IProtonBuffer buffer, IEncoderState state, T value)
      {
         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));

         state.Encoder.WriteUnsignedLong(buffer, state, DescriptorCode);

         int count = GetElementCount(value);
         EncodingCodes encodingCode = GetListEncoding(value);

         if (count < GetMinElementCount())
         {
            throw new EncodeException("Incomplete Type cannot be encoded");
         }

         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)encodingCode));

         switch (encodingCode)
         {
            case EncodingCodes.List8:
               WriteSmallType(buffer, state, value, count);
               break;
            case EncodingCodes.List32:
               WriteLargeType(buffer, state, value, count);
               break;
         }
      }

      private void WriteSmallType(IProtonBuffer buffer, IEncoderState state, T value, int elementCount)
      {
         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.EnsureWritable(sizeof(short));
         buffer.WriteUnsignedByte((byte)0);
         buffer.WriteUnsignedByte((byte)elementCount);

         // Write the list elements and then compute total size written.
         for (int i = 0; i < elementCount; ++i)
         {
            WriteElement(value, i, buffer, state);
         }

         // Move back and write the size
         long writeSize = buffer.WriteOffset - startIndex - sizeof(byte);

         buffer.SetUnsignedByte(startIndex, ((byte)writeSize));
      }

      private void WriteLargeType(IProtonBuffer buffer, IEncoderState state, T value, int elementCount)
      {
         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the count of list elements.
         buffer.EnsureWritable(sizeof(long));
         buffer.WriteInt(0);
         buffer.WriteInt(elementCount);

         // Write the list elements and then compute total size written.
         for (int i = 0; i < elementCount; ++i)
         {
            WriteElement(value, i, buffer, state);
         }

         // Move back and write the size
         long writeSize = buffer.WriteOffset - startIndex - sizeof(int);

         buffer.SetInt(startIndex, (int)writeSize);
      }

      public override void WriteArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         buffer.EnsureWritable(sizeof(long) + sizeof(short));
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

         if (writeSize > sizeof(int))
         {
            throw new ArgumentOutOfRangeException("Cannot encode given array, encoded size to large: " + writeSize);
         }

         buffer.SetInt(startIndex, (int)writeSize);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));

         for (int i = 0; i < values.Length; ++i)
         {
            T listType = (T)values[i];
            int count = GetElementCount(listType);
            long elementStartIndex = buffer.WriteOffset;

            // Reserve space for the size and write the count of list elements.
            buffer.EnsureWritable(sizeof(long));
            buffer.WriteInt(0);
            buffer.WriteInt(count);

            // Write the list elements and then compute total size written.
            for (int j = 0; j < count; ++j)
            {
               WriteElement(listType, j, buffer, state);
            }

            // Move back and write the size
            long listWriteSize = buffer.WriteOffset - elementStartIndex - sizeof(int);

            buffer.SetInt(elementStartIndex, (int)listWriteSize);
         }
      }
   }
}