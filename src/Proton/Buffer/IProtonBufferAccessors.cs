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

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// Interface for proton buffer primitive type accessors that can be used with
   /// custom types to extend or otherwise customize buffer access.
   /// </summary>
   public interface IProtonBufferAccessors
   {
      /// <summary>
      /// Indexed access to single unsigned byte values within the buffer which does not modify
      /// the read or write index value.  The given index must adhere to the same constraints
      /// as the get byte and set byte level APIs in this buffer class.
      /// </summary>
      /// <param name="i"></param>
      /// <returns>The unsigned byte value that is present at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      byte this[int i]
      {
         get => GetUnsignedByte(i);
         set => SetUnsignedByte(i, value);
      }

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
      /// Write the given byte value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetByte(int index, sbyte value);

      /// <summary>
      /// Write the given unsigned byte value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetUnsignedByte(int index, byte value);

      /// <summary>
      /// Write the given boolean value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetBoolean(int index, bool value);

      /// <summary>
      /// Write the given 2 byte char value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetChar(int index, char value);

      /// <summary>
      /// Write the given 2 byte short value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetShort(int index, short value);

      /// <summary>
      /// Write the given 2 byte unsigned short value at the given location in the buffer backing store
      /// without modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetUnsignedShort(int index, ushort value);

      /// <summary>
      /// Write the given 4 byte int value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetInt(int index, int value);

      /// <summary>
      /// Write the given 4 byte unsigned int value at the given location in the buffer backing store
      /// without modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetUnsignedInt(int index, uint value);

      /// <summary>
      /// Write the given 8 byte long value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetLong(int index, long value);

      /// <summary>
      /// Write the given 8 byte unsigned long value at the given location in the buffer backing store
      /// without modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetUnsignedLong(int index, ulong value);

      /// <summary>
      /// Write the given 4 byte float value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetFloat(int index, float value);

      /// <summary>
      /// Write the given 8 byte double value at the given location in the buffer backing store without
      /// modifying the write offset of this buffer.
      /// </summary>
      /// <param name="index">The index in the buffer where the write should occur</param>
      /// <param name="value">The value to be written at the specified index</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If the index is negative or larger than buffer capacity</exception>
      IProtonBuffer SetDouble(int index, double value);

      /// <summary>
      /// Read a signed byte from the buffer and advance the read offset.
      /// </summary>
      /// <returns>The value read from the buffer</returns>
      /// <exception cref="IndexOutOfRangeException">If the buffer has no more readable bytes</exception>
      sbyte ReadByte();

      /// <summary>
      /// Read a unsigned byte from the buffer and advance the read offset.
      /// </summary>
      /// <returns>The value read from the buffer</returns>
      /// <exception cref="IndexOutOfRangeException">If the buffer has no more readable bytes</exception>
      byte ReadUnsignedByte();

      /// <summary>
      /// Reads the next byte from the buffer and returns the boolean value.
      /// </summary>
      /// <returns>the boolean value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      bool ReadBoolean();

      /// <summary>
      /// Reads the next two bytes from the buffer and returns the short value.
      /// </summary>
      /// <returns>the short value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      short ReadShort();

      /// <summary>
      /// Reads the next two bytes from the buffer and returns the unsigned short value.
      /// </summary>
      /// <returns>the unsigned short value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      ushort ReadUnsignedShort();

      /// <summary>
      /// Reads the next four bytes from the buffer and returns the int value.
      /// </summary>
      /// <returns>the int value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      int ReadInt();

      /// <summary>
      /// Reads the next four bytes from the buffer and returns the unsigned int value.
      /// </summary>
      /// <returns>the unsigned int value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      uint ReadUnsignedInt();

      /// <summary>
      /// Reads the next eight bytes from the buffer and returns the long value.
      /// </summary>
      /// <returns>the long value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      long ReadLong();

      /// <summary>
      /// Reads the next eight bytes from the buffer and returns the unsigned long value.
      /// </summary>
      /// <returns>the unsigned long value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      ulong ReadUnsignedLong();

      /// <summary>
      /// Reads the next four bytes from the buffer and returns the float value.
      /// </summary>
      /// <returns>the float value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      float ReadFloat();

      /// <summary>
      /// Reads the next eight bytes from the buffer and returns the double value.
      /// </summary>
      /// <returns>the double value at the given index</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough readable bytes</exception>
      double ReadDouble();

      /// <summary>
      /// Writes the given byte value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteByte(sbyte value);

      /// <summary>
      /// Writes the given unsigned byte value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteUnsignedByte(byte value);

      /// <summary>
      /// Writes the given boolean value into this buffer as a single byte and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteBoolean(bool value);

      /// <summary>
      /// Writes the given two byte short value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteShort(short value);

      /// <summary>
      /// Writes the given two byte unsigned short value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteUnsignedShort(ushort value);

      /// <summary>
      /// Writes the given four byte int value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteInt(int value);

      /// <summary>
      /// Writes the given four byte unsigned int value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteUnsignedInt(uint value);

      /// <summary>
      /// Writes the given eight byte long value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteLong(long value);

      /// <summary>
      /// Writes the given eight byte unsigned long value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteUnsignedLong(ulong value);

      /// <summary>
      /// Writes the given four byte float value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteFloat(float value);

      /// <summary>
      /// Writes the given eight byte double value into this buffer and increases the write offset.
      /// </summary>
      /// <param name="value">The value to write into this buffer</param>
      /// <returns>this buffer instance</returns>
      /// <exception cref="IndexOutOfRangeException">If there are not enough writable bytes</exception>
      IProtonBuffer WriteDouble(double value);

   }
}