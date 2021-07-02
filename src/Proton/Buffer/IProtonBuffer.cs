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
using System.Text;

namespace Apache.Qpid.Proton.Buffer
{
   public interface IProtonBuffer : IEquatable<IProtonBuffer>, IComparable, IComparable<IProtonBuffer>, IProtonBufferAccessors
   {
      /// <summary>
      /// Gets the current capacity of this buffer instance which is the total amount of
      /// bytes that could be written before additional buffer capacity would be needed
      /// to allow more buffer writes. The remaining amount of writable bytes at any given
      /// time is the buffer capacity minus the write offset.
      /// </summary>
      int Capacity { get; }

      /// <summary>
      /// Requests that the buffer ensure that there is enough allocated internal capacity
      /// such that the given number of bytes can be written without requiring additional
      /// allocations and that this amount does not exceed any total capacity restrictions
      /// for this buffer.
      /// </summary>
      /// <param name="amount">the number of bytes that should be available fro writing</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="ArgumentOutOfRangeException">If the requested amount exceeds capacity restrictions</exception>
      IProtonBuffer EnsureWritable(int amount);

      /// <summary>
      /// Returns true if the current read offset is less than the current write offset meaning
      /// there are bytes available for reading.
      /// </summary>
      bool Readable => WriteOffset - ReadOffset > 0;

      /// <summary>
      /// Returns the number of bytes that can currently be read from this buffer.
      /// </summary>
      int ReadableBytes => WriteOffset - ReadOffset;

      /// <summary>
      /// Returns true if write offset is less than the current buffer capacity limit.
      /// </summary>
      bool Writable => Capacity - WriteOffset > 0;

      /// <summary>
      /// Returns the number of bytes that can currently be written from this buffer.
      /// </summary>
      int WritableBytes => Capacity - WriteOffset;

      /// <summary>
      /// Gets or sets the current read offset in this buffer.  If the read offset is set to
      /// a value larger than the current write offset an exception is thrown.
      /// </summary>
      int ReadOffset { get; set; }

      /// <summary>
      /// Gets or sets the current write offset in this buffer.  If the write offset is set to
      /// a value less than the current read offset or larger than the current buffer capcity
      /// an exception is thrown.
      /// </summary>
      int WriteOffset { get; set; }

      /// <summary>
      /// Resets the read and write offset values to zero.
      /// </summary>
      /// <returns>This buffer instance.</returns>
      IProtonBuffer Reset()
      {
         ReadOffset = 0;
         WriteOffset = 0;

         return this;
      }

      /// <summary>
      /// Copies the given number of bytes from this buffer into the target byte buffer starting
      /// the read from the given position in this buffer and the write to at the given position
      /// in the destination buffer. The length parameter controls how many bytes are copied to
      /// the destination. This method does not modify the read or write offset values in this
      /// buffer.
      /// </summary>
      /// <param name="srcPos">Position in this buffer to begin the copy from</param>
      /// <param name="dest">Destination buffer where the copied bytes are written</param>
      /// <param name="destPos">Position in the destination where the write begins</param>
      /// <param name="length">Number of byte to copy to the destination</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException"></exception>
      /// <exception cref="ArgumentOutOfRangeException"></exception>
      /// <exception cref="ArgumentNullException"></exception>
      IProtonBuffer CopyInto(int srcPos, byte[] dest, int destPos, int length);

      /// <summary>
      /// Copies the given number of bytes from this buffer into the target byte buffer starting
      /// the read from the given position in this buffer and the write to at the given position
      /// in the destination buffer. The length parameter controls how many bytes are copied to
      /// the destination. This method does not modify the read or write offset values in this
      /// buffer nor those of the destination buffer. The destination write index is an absolute
      /// index value unrelated to the write offset of the target.
      /// </summary>
      /// <param name="srcPos">Position in this buffer to begin the copy from</param>
      /// <param name="dest">Destination buffer where the copied bytes are written</param>
      /// <param name="destPos">Position in the destination where the write begins</param>
      /// <param name="length">Number of byte to copy to the destination</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException"></exception>
      /// <exception cref="ArgumentOutOfRangeException"></exception>
      /// <exception cref="ArgumentNullException"></exception>
      IProtonBuffer CopyInto(int srcPos, IProtonBuffer dest, int destPos, int length);

      /// <summary>
      /// Returns a copy of this buffer's readable bytes. Modifying the content of the
      /// returned buffer will not affect this buffers contents. The two buffers will
      /// maintain separate offsets.  The returned copy has the write offset set to the
      /// length of the copy meaning that the entire copied region is read for reading.
      /// </summary>
      /// <returns>A new buffer with a copy of the readable bytes in this buffer</returns>
      IProtonBuffer Copy()
      {
         return Copy(ReadOffset, ReadableBytes);
      }

      /// <summary>
      /// Returns a copy of this buffer's readable bytes. Modifying the content of the
      /// returned buffer will not affect this buffers contents. The two buffers will
      /// maintain separate offsets. The amount and start of the data to be copied is
      /// provided by the index and length arguments. The returned copy has the write
      /// offset set to the length of the copy meaning that the entire copied region
      /// is read for reading.
      /// </summary>
      /// <param name="index">The read offset where the copy begins</param>
      /// <param name="length">The number of bytes to copy</param>
      /// <returns>A new buffer with a copy of the readable bytes in the specified region</returns>
      IProtonBuffer Copy(int index, int length);

      /// <summary>
      /// Coverts the readable bytes in this buffer into a string value using the Encoding value
      /// provided. The underlying read and write offset values are not modified as a result of this
      /// operation.
      /// </summary>
      /// <param name="encoding">The encoding to use to convert the readable bytes</param>
      /// <returns>The string value of the readable bytes as converted by the provided Encoding</returns>
      string ToString(Encoding encoding);

      /// <summary>
      /// Advance the buffer read offset by the specified amount effectively skipping that number
      /// of bytes from being read.
      /// </summary>
      /// <param name="amount">The number of bytes to skip</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the amount is negative or larger than readable size</exception>
      IProtonBuffer SkipBytes(int amount)
      {
         ReadOffset = ReadOffset + amount;

         return this;
      }

      /// <summary>
      /// Writes the contents of the given byte array into this buffer and advances the
      /// write offset by the number of bytes written.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(byte[] source)
      {
         int size = source.Length;
         int offset = WriteOffset;
         WriteOffset = offset + size;

         for (int i = 0; i < size; i++)
         {
            SetUnsignedByte(offset + i, source[i]);
         }

         return this;
      }

      /// <summary>
      /// Transfers the bytes from the source buffer to this buffer starting at the current
      /// write offset and continues until the source buffer becomes unreadable.  The write
      /// index of this buffer is increased by the number of bytes read from the source.
      /// The method also increases the read offset of the source by the same amount as
      /// was written.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <param name="length">The number of bytes to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(IProtonBuffer source)
      {
         int size = source.ReadableBytes;
         int offset = WriteOffset;

         WriteOffset = offset + size;

         source.CopyInto(source.ReadOffset, this, offset, size);
         source.SkipBytes(size);

         return this;
      }
   }
}