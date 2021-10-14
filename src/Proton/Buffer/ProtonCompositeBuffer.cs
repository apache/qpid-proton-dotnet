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
using System.Collections.Generic;
using System.Text;

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// A composite buffer contains zero, one or more proton buffer instances
   /// chained together to behave as if it were one single contiguous buffer
   /// which can be read or written to.
   /// </summary>
   public sealed class ProtonCompositeBuffer : IProtonCompositeBuffer
   {
      /// <summary>
      /// Limit capcity to a value that might still allow for a non-composite
      /// buffer copy of this buffer to be made if requested.
      /// </summary>
      private static readonly int MAX_CAPACITY = Int32.MaxValue;

      private readonly List<IProtonBuffer> buffers = new List<IProtonBuffer>();
      private readonly List<int> indexTracker = new List<int>();
      private readonly SplitBufferAccessor splitBufferAccessor;

      private int readOffset;
      private int writeOffset;
      private int capacity;

      /// <summary>
      /// Before any read or write the composite must determine an index into the
      /// chosen buffer to be read or written to where that operation should start.
      /// </summary>
      private int nextComputedAccessIndex;

      internal ProtonCompositeBuffer(IProtonBufferAllocator allocator)
      {
         this.splitBufferAccessor = new SplitBufferAccessor(this);
      }

      internal ProtonCompositeBuffer(IProtonBufferAllocator allocator, IEnumerable<IProtonBuffer> buffers) : this(allocator)
      {
         // TODO Process incoming buffers
      }

      #region Basic Buffer information and state APIs

      public long Capacity => capacity;

      public bool IsReadable => readOffset < writeOffset;

      public long ReadableBytes => writeOffset - readOffset;

      public bool IsWritable => writeOffset < capacity;

      public long WritableBytes => capacity - writeOffset;

      public long ReadOffset
      {
         get => readOffset;
         set => throw new NotImplementedException();
      }

      public long WriteOffset
      {
         get => writeOffset;
         set => throw new NotImplementedException();
      }

      public uint ComponentCount
      {
         get
         {
            uint count = 0;
            foreach (IProtonBuffer buffer in buffers)
            {
               count += buffer.ComponentCount;
            }

            return count;
         }
      }

      public uint ReadableComponentCount
      {
         get
         {
            uint count = 0;
            foreach (IProtonBuffer buffer in buffers)
            {
               count += buffer.ReadableComponentCount;
            }

            return count;
         }
      }

      public uint WritableComponentCount
      {
         get
         {
            uint count = 0;
            foreach (IProtonBuffer buffer in buffers)
            {
               count += buffer.WritableComponentCount;
            }

            return count;
         }
      }

      #endregion

      public IProtonBuffer Fill(byte value)
      {
         foreach (IProtonBuffer buffer in buffers)
         {
            buffer.Fill(value);
         }

         return this;
      }

      public IProtonBuffer Reset()
      {
         ReadOffset = 0;
         WriteOffset = 0;

         return this;
      }

      public IProtonBuffer SkipBytes(long amount)
      {
         ReadOffset += amount;
         return this;
      }

      public IProtonCompositeBuffer Append(IProtonBuffer buffer)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Compact()
      {
         throw new NotImplementedException();
      }

      public IEnumerable<IProtonBuffer> DecomposeBuffer()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Copy()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Copy(long index, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(long srcPos, byte[] dest, long destPos, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(long srcPos, IProtonBuffer dest, long destPos, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer EnsureWritable(long amount)
      {
         throw new NotImplementedException();
      }

      public int CompareTo(object obj)
      {
         return CompareTo((IProtonBuffer)obj);
      }

      public int CompareTo(IProtonBuffer other)
      {
         throw new NotImplementedException();
      }

      public bool Equals(IProtonBuffer other)
      {
         throw new NotImplementedException();
      }

      public int ForEachReadableComponent(in int index, in Func<int, IReadableComponent, bool> processor)
      {
         throw new NotImplementedException();
      }

      public int ForEachWritableComponent(in int index, in Func<int, IWritableComponent, bool> processor)
      {
         throw new NotImplementedException();
      }

      public bool GetBoolean(long index)
      {
         throw new NotImplementedException();
      }

      public sbyte GetByte(long index)
      {
         throw new NotImplementedException();
      }

      public char GetChar(long index)
      {
         throw new NotImplementedException();
      }

      public double GetDouble(long index)
      {
         throw new NotImplementedException();
      }

      public float GetFloat(long index)
      {
         throw new NotImplementedException();
      }

      public short GetShort(long index)
      {
         throw new NotImplementedException();
      }

      public int GetInt(long index)
      {
         throw new NotImplementedException();
      }

      public long GetLong(long index)
      {
         throw new NotImplementedException();
      }

      public byte GetUnsignedByte(long index)
      {
         throw new NotImplementedException();
      }

      public ushort GetUnsignedShort(long index)
      {
         throw new NotImplementedException();
      }

      public uint GetUnsignedInt(long index)
      {
         throw new NotImplementedException();
      }

      public ulong GetUnsignedLong(long index)
      {
         throw new NotImplementedException();
      }

      public bool ReadBoolean()
      {
         throw new NotImplementedException();
      }

      public sbyte ReadByte()
      {
         throw new NotImplementedException();
      }

      public char ReadChar()
      {
         throw new NotImplementedException();
      }

      public double ReadDouble()
      {
         throw new NotImplementedException();
      }

      public float ReadFloat()
      {
         throw new NotImplementedException();
      }

      public short ReadShort()
      {
         throw new NotImplementedException();
      }

      public int ReadInt()
      {
         throw new NotImplementedException();
      }

      public long ReadLong()
      {
         throw new NotImplementedException();
      }

      public byte ReadUnsignedByte()
      {
         throw new NotImplementedException();
      }

      public ushort ReadUnsignedShort()
      {
         throw new NotImplementedException();
      }

      public uint ReadUnsignedInt()
      {
         throw new NotImplementedException();
      }

      public ulong ReadUnsignedLong()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetBoolean(long index, bool value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetByte(long index, sbyte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetChar(long index, char value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetDouble(long index, double value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetFloat(long index, float value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetShort(long index, short value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetInt(long index, int value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetLong(long index, long value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedByte(long index, byte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedShort(long index, ushort value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedInt(long index, uint value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetUnsignedLong(long index, ulong value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteBoolean(bool value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteByte(sbyte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteDouble(double value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteFloat(float value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteShort(short value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteInt(int value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteLong(long value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedByte(byte value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedShort(ushort value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedInt(uint value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteUnsignedLong(ulong value)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer WriteBytes(byte[] source)
      {
         if (source == null)
         {
            throw new ArgumentNullException("Input source array cannot be null");
         }

         return WriteBytes(source, 0, source.Length);
      }

      public IProtonBuffer WriteBytes(byte[] source, long offset, long length)
      {
         // Inefficient but workable solution that should be optimized later
         long woff = WriteOffset;
         WriteOffset = woff + length;
         for (long i = 0; i < length; i++)
         {
            SetUnsignedByte(woff + i, source[offset + i]);
         }

         return this;
      }

      public IProtonBuffer WriteBytes(IProtonBuffer source)
      {
         long size = source.ReadableBytes;
         long woff = WriteOffset;
         source.CopyInto(source.ReadOffset, this, woff, size);
         source.ReadOffset = source.ReadOffset + size;
         WriteOffset = woff + size;

         return this;
      }

      public string ToString(Encoding encoding)
      {
         // Inefficient but workable solution which should be optimized in the future.
         byte[] data = new byte[ReadableBytes];
         CopyInto(ReadOffset, data, 0, ReadableBytes);

         Decoder decoder = encoding.GetDecoder();
         int charCount = decoder.GetCharCount(data, 0, data.Length);
         char[] output = new char[charCount];

         int outputChars = decoder.GetChars(data, 0, data.Length, output, 0);

         return new string(output, 0, outputChars);
      }

      public override string ToString()
      {
         return "Buffer[readOffset:" + readOffset + ", writeOffset:" + writeOffset + ", cap:" + capacity + ']';
      }

      #region Internal composite buffer APIs

      private int SearchIndexTracker(int index)
      {
         int i = indexTracker.BinarySearch(index);
         return i < 0 ? -(i + 2) : i;
      }

      private IProtonBufferAccessors ChooseBuffer(int index, int size)
      {
         int i = SearchIndexTracker(index);

         // When the read and write offsets are at the end of the buffer they
         // will be equal to the number of tracked buffers and we return null
         // as no read or write operation can occur in this state.
         if (i == buffers.Count)
         {
            return null;
         }

         int off = index - indexTracker[i];
         IProtonBuffer candidate = buffers[i];

         // Space available in the selected buffer to accommodate the
         // requested read or write so we can return it directly but
         // in the case where the operation will run past the buffer
         // we use our internal accessor which will splice the operation
         // across two or more buffers.
         if (off + size <= candidate.Capacity)
         {
            nextComputedAccessIndex = off;
            return candidate;
         }
         else
         {
            nextComputedAccessIndex = index;
            return splitBufferAccessor;
         }
      }

      private void CheckReadBounds(int index, int size)
      {
         if (index < 0 || writeOffset < index + size)
         {
            throw IndexOutOfBounds(index, false);
         }
      }

      private void CheckGetBounds(int index, int size)
      {
         if (index < 0 || capacity < index + size)
         {
            throw IndexOutOfBounds(index, false);
         }
      }

      private void CheckWriteBounds(int index, int size)
      {
         if (index < 0 || capacity < index + size)
         {
            throw IndexOutOfBounds(index, true);
         }
      }

      private Exception IndexOutOfBounds(int index, bool write)
      {
         return new ArgumentOutOfRangeException(
            "Index " + index + " is out of bounds: [read 0 to " + writeOffset + ", write 0 to " + capacity + "].");
      }

      #endregion

      #region Internal buffer accessor for use when read / write occurs accross two or more buffers

      /// <summary>
      /// When a read or write will cross the boundary of two or more buffers, the split
      /// buffer accessor perform single byte operations to span that gap.
      /// </summary>
      private class SplitBufferAccessor : IProtonBufferAccessors
      {
         private readonly IProtonBuffer buffer;

         public SplitBufferAccessor(IProtonBuffer buffer)
         {
            this.buffer = buffer;
         }

         #region Single byte methods which should never be used

         public bool GetBoolean(long index)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public sbyte GetByte(long index)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public byte GetUnsignedByte(long index)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public bool ReadBoolean()
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public sbyte ReadByte()
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public byte ReadUnsignedByte()
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public IProtonBuffer SetBoolean(long index, bool value)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public IProtonBuffer SetByte(long index, sbyte value)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public IProtonBuffer SetUnsignedByte(long index, byte value)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public IProtonBuffer WriteBoolean(bool value)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public IProtonBuffer WriteByte(sbyte value)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         public IProtonBuffer WriteUnsignedByte(byte value)
         {
            throw new NotImplementedException("The split buffer accessor does not supply byte level access");
         }

         #endregion

         public char GetChar(long index)
         {
            return (char)GetUnsignedShort(index);
         }

         public short GetShort(long index)
         {
            return (short)GetUnsignedShort(index);
         }

         public int GetInt(long index)
         {
            return (int)GetUnsignedInt(index);
         }

         public long GetLong(long index)
         {
            return (long)GetUnsignedLong(index);
         }

         public double GetDouble(long index)
         {
            return BitConverter.Int64BitsToDouble(GetLong(index));
         }

         public float GetFloat(long index)
         {
            return BitConverter.Int32BitsToSingle(GetInt(index));
         }

         public ushort GetUnsignedShort(long index)
         {
            return (ushort)(buffer.GetUnsignedByte(index) << 8 |
                            buffer.GetUnsignedByte(index + 1));
         }

         public uint GetUnsignedInt(long index)
         {
            return (uint)(buffer.GetUnsignedByte(index) << 24 |
                          buffer.GetUnsignedByte(index + 1) << 16 |
                          buffer.GetUnsignedByte(index + 2) << 8 |
                          buffer.GetUnsignedByte(index + 3));
         }

         public ulong GetUnsignedLong(long index)
         {
            return (ulong)(buffer.GetUnsignedByte(index) << 56 |
                           buffer.GetUnsignedByte(index + 1) << 48 |
                           buffer.GetUnsignedByte(index + 2) << 40 |
                           buffer.GetUnsignedByte(index + 3) << 32 |
                           buffer.GetUnsignedByte(index + 4) << 24 |
                           buffer.GetUnsignedByte(index + 5) << 16 |
                           buffer.GetUnsignedByte(index + 6) << 8 |
                           buffer.GetUnsignedByte(index + 7));
         }

         public char ReadChar()
         {
            return (char)ReadUnsignedShort();
         }

         public double ReadDouble()
         {
            return BitConverter.Int64BitsToDouble(ReadLong());
         }

         public float ReadFloat()
         {
            return BitConverter.Int32BitsToSingle(ReadInt());
         }

         public short ReadShort()
         {
            return (short)ReadUnsignedShort();
         }

         public int ReadInt()
         {
            return (int)ReadUnsignedInt();
         }

         public long ReadLong()
         {
            return (long)ReadUnsignedLong();
         }

         public ushort ReadUnsignedShort()
         {
            return (ushort)(buffer.ReadUnsignedByte() << 8 |
                            buffer.ReadUnsignedByte());
         }

         public uint ReadUnsignedInt()
         {
            return (uint)(buffer.ReadUnsignedByte() << 24 |
                          buffer.ReadUnsignedByte() << 16 |
                          buffer.ReadUnsignedByte() << 8 |
                          buffer.ReadUnsignedByte());
         }

         public ulong ReadUnsignedLong()
         {
            return (uint)(buffer.ReadUnsignedByte() << 56 |
                          buffer.ReadUnsignedByte() << 48 |
                          buffer.ReadUnsignedByte() << 40 |
                          buffer.ReadUnsignedByte() << 32 |
                          buffer.ReadUnsignedByte() << 24 |
                          buffer.ReadUnsignedByte() << 16 |
                          buffer.ReadUnsignedByte() << 8 |
                          buffer.ReadUnsignedByte());
         }

         public IProtonBuffer SetChar(long index, char value)
         {
            return SetUnsignedShort(index, (ushort)value);
         }

         public IProtonBuffer SetDouble(long index, double value)
         {
            return SetUnsignedLong(index, (ulong)BitConverter.DoubleToInt64Bits(value));
         }

         public IProtonBuffer SetFloat(long index, float value)
         {
            return SetUnsignedInt(index, (uint)BitConverter.SingleToInt32Bits(value));
         }

         public IProtonBuffer SetShort(long index, short value)
         {
            return SetUnsignedShort(index, (ushort)value);
         }

         public IProtonBuffer SetInt(long index, int value)
         {
            return SetUnsignedInt(index, (uint)value);
         }

         public IProtonBuffer SetLong(long index, long value)
         {
            return SetUnsignedLong(index, (ulong)value);
         }

         public IProtonBuffer SetUnsignedShort(long index, ushort value)
         {
            buffer.SetUnsignedByte(index, (byte)(value >> 8));
            buffer.SetUnsignedByte(index + 1, (byte)(value >> 0));

            return buffer;
         }

         public IProtonBuffer SetUnsignedInt(long index, uint value)
         {
            buffer.SetUnsignedByte(index, (byte)(value >> 24));
            buffer.SetUnsignedByte(index + 1, (byte)(value >> 16));
            buffer.SetUnsignedByte(index + 2, (byte)(value >> 8));
            buffer.SetUnsignedByte(index + 3, (byte)(value >> 0));

            return buffer;
         }

         public IProtonBuffer SetUnsignedLong(long index, ulong value)
         {
            buffer.SetUnsignedByte(index, (byte)(value >> 56));
            buffer.SetUnsignedByte(index + 1, (byte)(value >> 48));
            buffer.SetUnsignedByte(index + 2, (byte)(value >> 40));
            buffer.SetUnsignedByte(index + 3, (byte)(value >> 32));
            buffer.SetUnsignedByte(index + 4, (byte)(value >> 24));
            buffer.SetUnsignedByte(index + 5, (byte)(value >> 16));
            buffer.SetUnsignedByte(index + 6, (byte)(value >> 8));
            buffer.SetUnsignedByte(index + 7, (byte)(value >> 0));

            return buffer;
         }

         public IProtonBuffer WriteDouble(double value)
         {
            return WriteUnsignedLong((ulong)BitConverter.DoubleToInt64Bits(value));
         }

         public IProtonBuffer WriteFloat(float value)
         {
            return WriteUnsignedInt((uint)BitConverter.SingleToInt32Bits(value));
         }

         public IProtonBuffer WriteShort(short value)
         {
            return WriteUnsignedShort((ushort)value);
         }

         public IProtonBuffer WriteInt(int value)
         {
            return WriteUnsignedInt((uint)value);
         }

         public IProtonBuffer WriteLong(long value)
         {
            return WriteUnsignedLong((ulong)value);
         }

         public IProtonBuffer WriteUnsignedShort(ushort value)
         {
            buffer.WriteUnsignedByte((byte)(value >> 8));
            buffer.WriteUnsignedByte((byte)(value >> 0));

            return buffer;
         }

         public IProtonBuffer WriteUnsignedInt(uint value)
         {
            buffer.WriteUnsignedByte((byte)(value >> 24));
            buffer.WriteUnsignedByte((byte)(value >> 16));
            buffer.WriteUnsignedByte((byte)(value >> 8));
            buffer.WriteUnsignedByte((byte)(value >> 0));

            return buffer;
         }

         public IProtonBuffer WriteUnsignedLong(ulong value)
         {
            buffer.WriteUnsignedByte((byte)(value >> 56));
            buffer.WriteUnsignedByte((byte)(value >> 48));
            buffer.WriteUnsignedByte((byte)(value >> 40));
            buffer.WriteUnsignedByte((byte)(value >> 32));
            buffer.WriteUnsignedByte((byte)(value >> 24));
            buffer.WriteUnsignedByte((byte)(value >> 16));
            buffer.WriteUnsignedByte((byte)(value >> 8));
            buffer.WriteUnsignedByte((byte)(value >> 0));

            return buffer;
         }
      }

      #endregion
   }
}