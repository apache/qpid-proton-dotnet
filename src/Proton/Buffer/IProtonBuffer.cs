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
      long Capacity { get; }

      /// <summary>
      /// Fill the buffer with the given byte value. This method does not respect the read offset
      /// nor the write offset but instead fills the entire backing buffer memory with the given
      /// value.
      /// </summary>
      /// <param name="value">The byte value to assign each byte in the backing store</param>
      /// <returns>This buffer instance.</returns>
      IProtonBuffer Fill(byte value);

      /// <summary>
      /// Requests that the buffer ensure that there is enough allocated internal capacity
      /// such that the given number of bytes can be written without requiring additional
      /// allocations and that this amount does not exceed any total capacity restrictions
      /// for this buffer.
      /// </summary>
      /// <param name="amount">the number of bytes that should be available fro writing</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="ArgumentOutOfRangeException">If the requested amount exceeds capacity restrictions</exception>
      IProtonBuffer EnsureWritable(long amount);

      /// <summary>
      /// Returns true if the current read offset is less than the current write offset meaning
      /// there are bytes available for reading.
      /// </summary>
      bool IsReadable { get; }

      /// <summary>
      /// Returns the number of bytes that can currently be read from this buffer.
      /// </summary>
      long ReadableBytes { get; }

      /// <summary>
      /// Returns true if write offset is less than the current buffer capacity limit.
      /// </summary>
      bool IsWritable { get; }

      /// <summary>
      /// Returns the number of bytes that can currently be written from this buffer.
      /// </summary>
      long WritableBytes { get; }

      /// <summary>
      /// Gets or sets the current read offset in this buffer.  If the read offset is set to
      /// a value larger than the current write offset an exception is thrown.
      /// </summary>
      long ReadOffset { get; set; }

      /// <summary>
      /// Gets or sets the current write offset in this buffer.  If the write offset is set to
      /// a value less than the current read offset or larger than the current buffer capcity
      /// an exception is thrown.
      /// </summary>
      long WriteOffset { get; set; }

      /// <summary>
      /// Resets the read and write offset values to zero.
      /// </summary>
      /// <returns>This buffer instance.</returns>
      IProtonBuffer Reset();

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
      IProtonBuffer CopyInto(long srcPos, byte[] dest, long destPos, long length);

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
      IProtonBuffer CopyInto(long srcPos, IProtonBuffer dest, long destPos, long length);

      /// <summary>
      /// Returns a copy of this buffer's readable bytes. Modifying the content of the
      /// returned buffer will not affect this buffers contents. The two buffers will
      /// maintain separate offsets.  The returned copy has the write offset set to the
      /// length of the copy meaning that the entire copied region is read for reading.
      /// </summary>
      /// <returns>A new buffer with a copy of the readable bytes in this buffer</returns>
      IProtonBuffer Copy();

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
      IProtonBuffer Copy(long index, long length);

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
      IProtonBuffer SkipBytes(long amount);

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
      /// write offset by the number of bytes written.
      /// </summary>
      /// <param name="source">The byte buffer to be written into this buffer</param>
      /// <param name="offset">The offset into the source buffer to start the write</param>
      /// <param name="length">The number of bytes from the source buffer to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBytes(byte[] source, long offset, long length);

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
      /// Discards the read bytes, and moves the buffer contents to the beginning of the buffer.
      /// </summary>
      /// <returns>this buffer instance</returns>
      IProtonBuffer Compact();

      /// <summary>
      /// Reclaims read buffer space and returns it to the operating system or other pooling
      /// mechanisms if those are in place, then compacts the remaining buffer contents.
      /// <para/>
      /// For a non-composite buffer this operation could consist of allocating a smaller
      /// buffer to house any remaining unread bytes and freeing the larger backing buffer in
      /// some cases or it may result in a no-op depending on the buffer implementation. For
      /// the composite buffer case this operation provides an API which allows for fully read
      /// buffer constituents to be released and returned to a memory pool or back to the O/S.
      /// </summary>
      /// <returns>this buffer instance</returns>
      IProtonBuffer Reclaim();

      /// <summary>
      /// Splits the buffer into two distinct buffers at the given index plus the current read
      /// offset. The returned buffer will retain the read offset and write offset of this buffer
      /// but will be truncated to match the capacity provided by the split index, which implies
      /// that they might both be set to the capacity if they were previously set beyond the split
      /// index. The returned buffer will set its read and write offsets to zero if they fell prior
      /// to the given index otherwise they will be truncated to match the new buffer capacity.
      /// <para/>
      /// Split buffers support the standard buffer operations including resizing to ensure
      /// writable regions which implies that a buffer resize on either will cause them to
      /// no longer reference the same underlying memory region.  If buffer implementations
      /// implement pooling then they must ensure proper release of shared buffer regions
      /// once both buffers no longer reference them.
      /// </summary>
      /// </summary>
      /// <param name="offset">The offset to split beyond the current read offset</param>
      /// <returns>A new buffer that access the front portion of the buffer split</returns>
      IProtonBuffer ReadSplit(long offset);

      /// <summary>
      /// Splits the buffer into two distinct buffers at the given index plus the current write
      /// offset. The returned buffer will retain the read offset and write offset of this buffer
      /// but will be truncated to match the capacity provided by the split index, which implies
      /// that they might both be set to the capacity if they were previously set to the split
      /// index. The returned buffer will set its read and write offsets to zero if they fell prior
      /// to the given index otherwise they will be truncated to match the new buffer capacity.
      /// <para/>
      /// Split buffers support the standard buffer operations including resizing to ensure
      /// writable regions which implies that a buffer resize on either will cause them to
      /// no longer reference the same underlying memory region.  If buffer implementations
      /// implement pooling then they must ensure proper release of shared buffer regions
      /// once both buffers no longer reference them.
      /// </summary>
      /// </summary>
      /// <param name="offset">The offset to split beyond the current write offset</param>
      /// <returns>A new buffer that access the front portion of the buffer split</returns>
      IProtonBuffer WriteSplit(long offset);

      /// <summary>
      /// Splits the buffer into two buffers at the write offset.  The resulting buffer
      /// will comprise the read and readable portions of this buffer with the write offset
      /// and capacity set to the current write offset.  This buffer will lose access to the
      /// split region and its read offset will be set to the current write offset.  This
      /// buffer will also have its capacity reduced by the number of bytes in the returned
      /// buffer (i.e. the current number of read and readable bytes).
      /// <para/>
      /// Split buffers support the standard buffer operations including resizing to ensure
      /// writable regions which implies that a buffer resize on either will cause them to
      /// no longer reference the same underlying memory region.  If buffer implementations
      /// implement pooling then they must ensure proper release of shared buffer regions
      /// once both buffers no longer reference them.
      /// </summary>
      /// <returns>A new buffer that access the front portion of the buffer split</returns>
      IProtonBuffer Split();

      /// <summary>
      /// Splits the buffer into two distinct buffers at the given index. The returned buffer will
      /// retain the read offset and write offset of this buffer but will be truncated to match
      /// the capacity provided by the split index, which implies that they might both be set to
      /// the capacity if they were previously set beyond the split index.  The returned buffer
      /// will set its read and write offsets to zero if they fell prior to the given index
      /// otherwise they will be truncated to match the new buffer capacity.
      /// <para/>
      /// Split buffers support the standard buffer operations including resizing to ensure
      /// writable regions which implies that a buffer resize on either will cause them to
      /// no longer reference the same underlying memory region.  If buffer implementations
      /// implement pooling then they must ensure proper release of shared buffer regions
      /// once both buffers no longer reference them.
      /// </summary>
      /// <param name="index">The index in this buffer where the split occurs</param>
      /// <returns>A new buffer that access the front portion of the buffer split</returns>
      IProtonBuffer Split(long index);

      /// <summary>
      /// Returns the number of component buffers in this buffer. If this is not a composite
      /// buffer instance then the count will always be one. For a composite buffer this will
      /// be the count of the current number of component buffers contained within.
      /// </summary>
      uint ComponentCount { get; }

      /// <summary>
      /// Returns the number of component buffers in this buffer that are readable and would
      /// be provided to calls to the for each readable buffer API. If this is not a composite
      /// buffer instance then the count will be at most one. For a composite buffer this will
      /// be the count of the current number of component buffers contained within that are
      /// readable.
      /// </summary>
      uint ReadableComponentCount { get; }

      /// <summary>
      /// Returns the number of component buffers in this buffer that are writable and would
      /// be provided to calls to the for each writable buffer API. If this is not a composite
      /// buffer instance then the count will be at most one. For a composite buffer this will
      /// be the count of the current number of component buffers contained within that are
      /// writable.
      /// </summary>
      uint WritableComponentCount { get; }

      /// <summary>
      /// Invokes the provided delegate for each readable component in this buffer
      /// and increments the provided index value for each invocation. The total
      /// number of buffers processed is returned to the caller.
      /// <para/>
      /// The delegate can stop processing at any time by returning false in which
      /// case this method will stop and return a negative value to indicate that
      /// processing stopped early and did not traverse all available components.
      /// </summary>
      /// <param name="index">a starting index which is increment after each call</param>
      /// <param name="processor">The delegate that will receive the components</param>
      /// <returns>The number of components processed or negative if stopped early.</returns>
      int ForEachReadableComponent(in int index, in Func<int, IReadableComponent, bool> processor);

      /// <summary>
      /// Invokes the provided delegate for each writable component in this buffer
      /// and increments the provided index value for each invocation. The total
      /// number of buffers processed is returned to the caller.
      /// <para/>
      /// The delegate can stop processing at any time by returning false in which
      /// case this method will stop and return a negative value to indicate that
      /// processing stopped early and did not traverse all available components.
      /// </summary>
      /// <param name="index">a starting index which is increment after each call</param>
      /// <param name="processor">The delegate that will receive the components</param>
      /// <returns>The number of components processed or negative if stopped early.</returns>
      int ForEachWritableComponent(in int index, in Func<int, IWritableComponent, bool> processor);

   }
}