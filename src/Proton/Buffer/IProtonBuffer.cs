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
      /// Returns if this buffer implementation has a backing byte array.  If it does
      /// than the various array access methods will allow calls, otherwise an exception
      /// will be thrown if there is no backing array and an access operation occurs.
      /// </summary>
      /// <returns>true if the buffer has a backing byte array</returns>
      bool HasArray { get; }

      /// <summary>
      /// If the buffer implementation has a backing array this method returns that array
      /// this method returns it, otherwise it will throw an exception to indicate that.
      /// </summary>
      byte[] Array { get; }

      /// <summary>
      /// If the buffer implementation has a backing array this method returns that array
      /// offset used to govern where reads and writes start in the wrapped array, otherwise
      /// it will throw an exception to indicate that.
      /// </summary>
      int ArrayOffset { get; }

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
         int offset = ReadOffset;
         int length = ReadableBytes;

         return Copy(offset, length);
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
      /// Transfers this buffer's data to the specified destination starting at the specified
      /// absolute index until the destination becomes non-writable.  This method is basically
      /// the same as the variation that takes a destination index and length except that this
      /// method will increase the write offset of the destination by the number of bytes that
      /// are written. This method does not alter the read or write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the transfer should start</param>
      /// <param name="destination">The destination buffer which will be written to.</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer GetBytes(int index, IProtonBuffer destination);

      /// <summary>
      /// Transfers this buffer's data to the specified destination starting at the specified
      /// absolute index until the given number of bytes is written.  This method is basically
      /// the same as the variation that takes a destination index and length except that this
      /// method will increase the write offset of the destination by the number of bytes that
      /// are written. This method does not alter the read or write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the transfer should start</param>
      /// <param name="destination">The destination buffer which will be written to.</param>
      /// <param name="length"></param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer GetBytes(int index, IProtonBuffer destination, int length);

      /// <summary>
      /// Transfers this buffer's data to the specified destination starting at the specified
      /// absolute index until the given number of bytes is written.  The bytes transferred are
      /// written starting at the specified index in the destination buffer.  The write offset of
      /// the destination buffer is not affected by this operation.  This method does not alter
      /// the read or write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the transfer should start</param>
      /// <param name="destination">The destination buffer which will be written to.</param>
      /// <param name="destinationIdex">The index in the destination where writes begin</param>
      /// <param name="length">The number of bytes to transfer from this buffer to the destination</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer GetBytes(int index, IProtonBuffer destination, int destinationIdex, int length);

      /// <summary>
      /// Transfers this buffer's data to the specified destination starting at the specified
      /// absolute index until the destination length bytes are copied. This method does not alter
      /// the read or write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the transfer should start</param>
      /// <param name="destination">The destination byte array which will be written to.</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer GetBytes(int index, byte[] destination);

      /// <summary>
      /// Transfers this buffer's data to the specified destination starting at the specified
      /// absolute index until the given number of bytes is written. This method does not alter
      /// the read or write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the transfer should start</param>
      /// <param name="destination">The destination byte array which will be written to.</param>
      /// <param name="length"></param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer GetBytes(int index, byte[] destination, int length);

      /// <summary>
      /// Transfers this buffer's data to the specified destination starting at the specified
      /// absolute index until the given number of bytes is written. The writes are offset into
      /// the destination byte array by the provided destination index. This method does not alter
      /// the read or write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the transfer should start</param>
      /// <param name="destination">The destination byte array which will be written to.</param>
      /// <param name="destinationIdex">The index in the destination where writes begin</param>
      /// <param name="length"></param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer GetBytes(int index, byte[] destination, int destinationIndex, int length);

      /// <summary>
      /// Transfers the specified source buffer's data to this buffer starting at the specified
      /// absolute index until the source buffer becomes unreadable. This method increases the
      /// read offset of the source buffer by the number of bytes read. This method does not modify
      /// the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the write should begin</param>
      /// <param name="buffer">The source buffer whose bytes are written into this one</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetBytes(int index, IProtonBuffer buffer);

      /// <summary>
      /// Transfers the specified source buffer's data to this buffer starting at the specified
      /// absolute index until specified number of bytes is written. This method increases the
      /// read offset of the source buffer by the number of bytes read. This method does not modify
      /// the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the write should begin</param>
      /// <param name="buffer">The source buffer whose bytes are written into this one</param>
      /// <param name="length"></param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetBytes(int index, IProtonBuffer buffer, int length);

      /// <summary>
      /// Transfers the specified source buffer's data to this buffer starting at the specified
      /// absolute index until specified number of bytes is written starting at the given index
      /// in the source buffer.  This method does not modify the read or write offset of the source
      /// of target buffers.
      /// </summary>
      /// <param name="index">The index in this buffer where the write should begin</param>
      /// <param name="buffer">The source buffer whose bytes are written into this one</param>
      /// <param name="sourceIndex"></param>
      /// <param name="length"></param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetBytes(int index, IProtonBuffer buffer, int sourceIndex, int length);

      /// <summary>
      /// Transfers the specified source buffer's data to this buffer starting at the specified
      /// absolute index until the source byte buffer size is written. This method does not modify
      /// the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the write should begin</param>
      /// <param name="buffer">The source buffer whose bytes are written into this one</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetBytes(int index, byte[] buffer);

      /// <summary>
      /// Transfers the specified source buffer's data to this buffer starting at the specified
      /// absolute index until the source byte buffer size is written, the write from the source
      /// buffer begin at the specified source index. This method does not modify the write offset
      /// of this buffer.
      /// </summary>
      /// <param name="index">The index in this buffer where the write should begin</param>
      /// <param name="buffer">The source buffer whose bytes are written into this one</param>
      /// <param name="sourceIndex"></param>
      /// <param name="length"></param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetBytes(int index, byte[] buffer, int sourceIndex, int length);

      /// <summary>
      /// Advance the buffer read offset by the specified amount effectively skipping that number
      /// of bytes from being read.
      /// </summary>
      /// <param name="amount">The number of bytes to skip</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the amount is negative or larger than readable size</exception>
      IProtonBuffer SkipBytes(int amount);

      /// <summary>
      /// Reads bytes from this buffer and writes them into the destination byte array incrementing
      /// the read offset by the value of the length of the destination array.
      /// </summary>
      /// <param name="target">The byte array where the bytes are written</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the buffer has no more readable bytes</exception>
      IProtonBuffer ReadBytes(byte[] target);

      /// <summary>
      /// Reads bytes from this buffer and writes them into the destination byte array incrementing
      /// the read offset by the value of the length of the destination array.  The number of bytes
      /// read is controlled by the length parameter.
      /// </summary>
      /// <param name="target">The byte array where the bytes are written</param>
      /// <param name="length">The number of bytes to read from this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the buffer has no more readable bytes</exception>
      IProtonBuffer ReadBytes(byte[] target, int length);

      /// <summary>
      /// Reads bytes from this buffer and writes them into the destination byte array incrementing
      /// the read offset by the value of the length of the destination array.  The number of bytes
      /// read is controlled by the length parameter, and the write start at the given offset into
      /// the provided byte array.
      /// </summary>
      /// <param name="target">The byte array where the bytes are written</param>
      /// <param name="length">The number of bytes to read from this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the buffer has no more readable bytes</exception>
      IProtonBuffer ReadBytes(byte[] target, int offset, int length);

      /// <summary>
      /// Reads bytes from this buffer and writes them into the destination buffer incrementing
      /// the read offset by the number of writable bytes in the target buffer.  The read amount is
      /// dictated by the number of writable bytes in the target buffer.  The write offset of the
      /// target buffer is incremented by the amount written.
      /// </summary>
      /// <param name="target">The buffer where the bytes are written</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the read violates the constraints of either buffer</exception>
      IProtonBuffer ReadBytes(IProtonBuffer target);

      /// <summary>
      /// Reads bytes from this buffer and writes them into the destination buffer incrementing
      /// the read offset by the number of writable bytes indicated. The write offset of the target
      /// buffer is incremented by the amount written.
      /// </summary>
      /// <param name="target">The buffer where the bytes are written</param>
      /// <param name="length">The number of bytes to read from this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the read violates the constraints of either buffer</exception>
      IProtonBuffer ReadBytes(IProtonBuffer target, int length);

      /// <summary>
      /// Reads bytes from this buffer and writes them into the target buffer starting at the offset
      /// value indicated, the number of byte read is provided by the length argument.  The read offset
      /// of this buffer is incremented to match the nubmer of byte read however the target buffer's
      /// read and write offset values are not changed as a result of this operation.
      /// </summary>
      /// <param name="target">The buffer where the bytes are written</param>
      /// <param name="offset">The offset in the target where writes should begin</param>
      /// <param name="length">The number of bytes to read from this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the read violates the constraints of either buffer</exception>
      IProtonBuffer ReadBytes(IProtonBuffer target, int offset, int length);

      /// <summary>
      /// Writes the contents of the given byte array into this buffer and advances the
      /// write offset by the number of bytes written.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(byte[] source);

      /// <summary>
      /// Writes the contents of the given byte array into this buffer and advances the
      /// write offset by the number of bytes written which is controlled by the length
      /// parameter.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <param name="length">The number of bytes to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(byte[] source, int length);

      /// <summary>
      /// Writes the contents of the given byte array into this buffer and advances the
      /// write offset by the number of bytes written which is controlled by the length
      /// parameter.  The starting read location in the source buffer is controlled by
      /// the offset parameter.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <param name="offset">The offset in the source buffer to begin reading</param>
      /// <param name="length">The number of bytes to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(byte[] source, int offset, int length);

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
      IProtonBuffer WriteBytes(IProtonBuffer source);

      /// <summary>
      /// Transfers the bytes from the source buffer to this buffer starting at the current
      /// write offset and continues until the specified number of bytes is written.  The write
      /// index of this buffer is increased by the number of bytes read from the source.
      /// The method also increases the read offset of the source by the same amount as
      /// was written.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <param name="length">The number of bytes to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(IProtonBuffer source, int length);

      /// <summary>
      /// Transfers the bytes from the source buffer to this buffer starting at the current
      /// write offset and continues until the specified number of bytes is written.  The write
      /// index of this buffer is increased by the number of bytes read from the source.
      /// The read operation starts at the given offset into the source buffer and continues
      /// for the specified length number of bytes however the read offset of the source buffer
      /// is not modified as a result of this call.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <param name="offset">The offset in the source buffer to begin reading</param>
      /// <param name="length">The number of bytes to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(IProtonBuffer source, int offset, int length);

   }
}