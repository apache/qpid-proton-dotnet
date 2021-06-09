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
   public interface IProtonBuffer : IEquatable<IProtonBuffer>, IComparable, IComparable<IProtonBuffer>
   {
      /// <summary>
      /// Returns if this buffer implementation has a backing byte array.  If it does
      /// than the various array access methods will allow calls, otherwise an exception
      /// will be thrown if there is no backing array and an access operation occurs.
      /// </summary>
      /// <returns>true if the buffer has a backing byte array</returns>
      bool HasArray();

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
      /// Gets or sets the current capcity value for this buffer which indicates
      /// the total number of bytes that can currently be stored within the buffer
      /// before more memory must be allocated.
      ///
      /// When setting the capacity the buffer can either shrink or grow the depending
      /// on the value given.  If the value given is larger than the current capacity
      /// more space will be allocated unless the requested value exceeds any set capacity
      /// limit for this buffer.
      /// </summary>
      int Capacity { get; set; }

      /// <summary>
      /// Requests that the buffer ensure that there is enough allocated internal capacity
      /// such that the given number of bytes can be written without requiring additional
      /// allocations and that this amount does not exceed any total capacity restrictions
      /// for this buffer.
      /// </summary>
      /// <param name="amout">the number of bytes that should be available fro writing</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="ArgumentOutOfRangeException">If the requested amount exceeds capacity restrictions</exception>
      IProtonBuffer EnsureWritable(int amout);

      /// <summary>
      /// Returns true if the current read index is less than the current write index meaning
      /// there are bytes available for reading.
      /// </summary>
      bool Readable { get; }

      /// <summary>
      /// Returns the number of bytes that can currently be read from this buffer.
      /// </summary>
      int ReadableBytes { get; }

      /// <summary>
      /// Returns true if write index is less than the current buffer capacity limit.
      /// </summary>
      bool Writable { get; }

      /// <summary>
      /// Returns the number of bytes that can currently be written from this buffer.
      /// </summary>
      int WritableBytes { get; }

      /// <summary>
      /// Gets or sets the current read index in this buffer.  If the read index is set to
      /// a value larger than the current write index an exception is thrown.
      /// </summary>
      int ReadIndex { get; set; }

      /// <summary>
      /// Gets or sets the current write index in this buffer.  If the write index is set to
      /// a value less than the current read index or larger than the current buffer capcity
      /// an exception is thrown.
      /// </summary>
      int WriteIndex { get; set; }

      /// <summary>
      /// Coverts the readable bytes in this buffer into a string value using the Encoding value
      /// provided. The underlying read and write index values are not modified as a result of this
      /// operation.
      /// </summary>
      /// <param name="encoding">The encoding to use to convert the readable bytes</param>
      /// <returns>The string value of the readable bytes as converted by the provided Encoding</returns>
      string ToString(Encoding encoding);

      /// <summary>
      /// Reads a single byte from the given index and returns a boolean value indicating if the
      /// byte was zero (false) or greater than zero (true).
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the boolean value of the byte at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      bool GetBool(int index);

      /// <summary>
      /// Reads a single signed byte from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the byte value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      sbyte GetByte(int index);

      /// <summary>
      /// Reads a single unsigned byte from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the byte value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      byte GetUnsignedByte(int index);

      /// <summary>
      /// Reads a single 2 byte char from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the char value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      char GetChar(int index);

      /// <summary>
      /// Reads a single 2 byte short from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the short value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      short GetShort(int index);

      /// <summary>
      /// Reads a single 2 byte unsigned short from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the short value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      ushort GetUnsignedShort(int index);

      /// <summary>
      /// Reads a single 4 byte int from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the int value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      int GetInt(int index);

      /// <summary>
      /// Reads a single 4 byte unsigned int from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the int value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      uint GetUnsignedInt(int index);

      /// <summary>
      /// Reads a single 8 byte long from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the long value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      long GetLong(int index);

      /// <summary>
      /// Reads a single 8 byte unsigned long from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the long value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      ulong GetUnsignedLong(int index);

      /// <summary>
      /// Reads a single 4 byte float from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the float value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      float GetFloat(int index);

      /// <summary>
      /// Reads a single 8 byte double from the given index and returns it.
      /// </summary>
      /// <param name="index"></param>
      /// <returns>the double value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      double GetDouble(int index);

      /// <summary>
      /// Transfers this buffer's data to the specified destination starting at the specified
      /// absolute index until the destination becomes non-writable.  This method is basically
      /// the same as the variation that takes a destination index and length except that this
      /// method will increase the write index of the destination by the number of bytes that
      /// are written.
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
      /// method will increase the write index of the destination by the number of bytes that
      /// are written.
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
      /// written starting at the specified index in the destination buffer.  The write index of
      /// the destination buffer is not affected by this operation.
      /// </summary>
      /// <param name="index">The index in this buffer where the transfer should start</param>
      /// <param name="destination">The destination buffer which will be written to.</param>
      /// <param name="destinationIdex">The index in the destination where writes begin</param>
      /// <param name="length">The number of bytes to transfer from this buffer to the destination</param>
      /// <returns>This buffer instance.</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer GetBytes(int index, IProtonBuffer destination, int destinationIdex, int length);

   }
}