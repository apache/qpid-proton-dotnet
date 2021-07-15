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
   /// <summary>
   /// Process the given component from the buffer at the given index value. The provided
   /// readable component is only considered valid during the call after which any changes
   /// to the source buffer would invalidate it.
   /// </summary>
   /// <param name="index">an index that provides a view of which component access count</param>
   /// <param name="component">A readable component from the buffer</param>
   /// <returns>true to continue iteration and false to stop any further processing.</returns>
   public delegate bool ReadableComponentProcessor(in int index, in IReadableComponent component);

   /// <summary>
   /// Process the given component from the buffer at the given index value. The provided
   /// writable component is only considered valid during the call after which any changes
   /// to the source buffer would invalidate it.
   /// </summary>
   /// <param name="index">an index that provides a view of which component access count</param>
   /// <param name="component">A writable component from the buffer</param>
   /// <returns>true to continue iteration and false to stop any further processing.</returns>
   public delegate bool WritableComponentProcessor(in int index, in IWritableComponent component);

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
      bool Readable { get; }

      /// <summary>
      /// Returns the number of bytes that can currently be read from this buffer.
      /// </summary>
      long ReadableBytes { get; }

      /// <summary>
      /// Returns true if write offset is less than the current buffer capacity limit.
      /// </summary>
      bool Writable { get; }

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
      int ForEachReadableComponent(in int index, in ReadableComponentProcessor processor);

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
      int ForEachWritableComponent(in int index, in WritableComponentProcessor processor);

   }
}