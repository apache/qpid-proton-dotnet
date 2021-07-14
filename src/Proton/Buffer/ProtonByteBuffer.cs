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
   /// A proton buffer implementation that wraps a single heap allocated
   /// byte array and provides read and write operations on that array
   /// along with self resizing based on capacity limits.
   /// </summary>
   public sealed class ProtonByteBuffer : IProtonBuffer
   {
      /// <summary>
      /// Default initial capacity when created without initial value.
      /// </summary>
      public static readonly int DefaultCapacity = 64;

      /// <summary>
      /// Default max cpacity based on maximum array size limit as this
      /// buffer is backed by a byte array.
      /// </summary>
      public static readonly int DefaultMaximumCapacity = Int32.MaxValue;

      private byte[] array;

      private long readOffset;
      private long writeOffset;
      private long maxCapacity;

      /// <summary>
      /// Create a new proton byte buffer instance with default initial capacity
      /// and limited only by the size of a byte array in max capacity.
      /// </summary>
      public ProtonByteBuffer() : this(DefaultCapacity, DefaultMaximumCapacity)
      {
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given initial capacity
      /// and limited only by the size of a byte array in max capacity.
      /// </summary>
      /// <param name="initialCapacity">The initial capacity of this buffer</param>
      public ProtonByteBuffer(int initialCapacity) : this(initialCapacity, DefaultMaximumCapacity)
      {
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given initial capacity
      /// and limited to a max capacity of the given amount.
      /// </summary>
      /// <param name="initialCapacity">The initial capacity of this buffer</param>
      public ProtonByteBuffer(int initialCapacity, int maxCapacity) : base()
      {
         if (initialCapacity < 0)
         {
            throw new ArgumentOutOfRangeException("Initial capacity cannot be negative");
         }

         if (maxCapacity < initialCapacity)
         {
            throw new ArgumentOutOfRangeException("The maximum capacity cannot be less than the initial value");
         }

         this.array = new byte[initialCapacity];
         this.maxCapacity = maxCapacity;
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given backing array whose
      /// size determines that largest the buffer can ever be.
      /// </summary>
      /// <param name="backingArray"></param>
      public ProtonByteBuffer(byte[] backingArray) : this(backingArray.Length, backingArray.Length)
      {
         this.array = backingArray;
      }

      public long Capacity => array.Length;

      public bool Readable => ReadOffset < WriteOffset;

      public long ReadableBytes => WriteOffset - ReadOffset;

      public bool Writable => WriteOffset < Capacity;

      public long WritableBytes => Capacity - WriteOffset;

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

      public uint ComponentCount => 1;

      public uint ReadableComponentCount => Readable ? 1u : 0u;

      public uint WritableComponentCount => Writable ? 1u : 0u;

      public IProtonBuffer EnsureWritable(long amount)
      {
         return InternalEnsureWritable(amount);
      }

      public IProtonBuffer Compact()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Reset()
      {
         ReadOffset = 0;
         WriteOffset = 0;

         return this;
      }

      public IProtonBuffer SkipBytes(long amount)
      {
         CheckRead(readOffset, amount);
         readOffset += amount;
         return this;
      }

      #region Buffer Copy API implementation

      public IProtonBuffer Copy()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer Copy(long index, long length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(long srcPos, byte[] dest, int destPos, int length)
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer CopyInto(long srcPos, IProtonBuffer dest, long destPos, long length)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region Buffer iteration API implementations

      public uint ForEachReadableComponent(in int index, in ReadableComponentProcessor processor)
      {
         throw new NotImplementedException();
      }

      public uint ForEachWritableComponent(in int index, in WritableComponentProcessor processor)
      {
         throw new NotImplementedException();
      }

      #endregion

      #region Implementation of the proton buffer accessors API

      public bool GetBool(long index)
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

      public int GetInt(long index)
      {
         throw new NotImplementedException();
      }

      public long GetLong(long index)
      {
         throw new NotImplementedException();
      }

      public short GetShort(long index)
      {
         throw new NotImplementedException();
      }

      public byte GetUnsignedByte(long index)
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

      public ushort GetUnsignedShort(long index)
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

      public double ReadDouble()
      {
         throw new NotImplementedException();
      }

      public float ReadFloat()
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

      public short ReadShort()
      {
         throw new NotImplementedException();
      }

      public byte ReadUnsignedByte()
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

      public ushort ReadUnsignedShort()
      {
         throw new NotImplementedException();
      }

      public IProtonBuffer SetBoolean(long index, bool value)
      {
         CheckWrite(index, sizeof(byte));
         ProtonByteUtils.WriteBoolean(value, array, (int)index);
         return this;
      }

      public IProtonBuffer SetByte(long index, sbyte value)
      {
         CheckWrite(writeOffset, sizeof(sbyte));
         ProtonByteUtils.WriteByte(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetChar(long index, char value)
      {
         CheckWrite(writeOffset, sizeof(short));
         ProtonByteUtils.WriteShort((short) value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetDouble(long index, double value)
      {
         CheckWrite(writeOffset, sizeof(double));
         ProtonByteUtils.WriteDouble(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetFloat(long index, float value)
      {
         CheckWrite(writeOffset, sizeof(float));
         ProtonByteUtils.WriteFloat(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetInt(long index, int value)
      {
         CheckWrite(writeOffset, sizeof(int));
         ProtonByteUtils.WriteInt(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetLong(long index, long value)
      {
         CheckWrite(writeOffset, sizeof(long));
         ProtonByteUtils.WriteLong(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetShort(long index, short value)
      {
         CheckWrite(writeOffset, sizeof(short));
         ProtonByteUtils.WriteShort(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetUnsignedByte(long index, byte value)
      {
         CheckWrite(writeOffset, sizeof(byte));
         ProtonByteUtils.WriteUnsignedByte(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetUnsignedInt(long index, uint value)
      {
         CheckWrite(writeOffset, sizeof(uint));
         ProtonByteUtils.WriteUnsignedInt(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetUnsignedLong(long index, ulong value)
      {
         CheckWrite(writeOffset, sizeof(ulong));
         ProtonByteUtils.WriteUnsignedLong(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer SetUnsignedShort(long index, ushort value)
      {
         CheckWrite(writeOffset, sizeof(ushort));
         ProtonByteUtils.WriteUnsignedShort(value, array, (int)writeOffset);
         return this;
      }

      public IProtonBuffer WriteBoolean(bool value)
      {
         CheckWrite(writeOffset, sizeof(byte));
         ProtonByteUtils.WriteBoolean(value, array, (int)writeOffset);
         writeOffset += sizeof(byte);
         return this;
      }

      public IProtonBuffer WriteByte(sbyte value)
      {
         CheckWrite(writeOffset, sizeof(byte));
         ProtonByteUtils.WriteByte(value, array, (int)writeOffset);
         writeOffset += sizeof(byte);
         return this;
      }

      public IProtonBuffer WriteDouble(double value)
      {
         CheckWrite(writeOffset, sizeof(double));
         ProtonByteUtils.WriteDouble(value, array, (int)writeOffset);
         writeOffset += sizeof(double);
         return this;
      }

      public IProtonBuffer WriteFloat(float value)
      {
         CheckWrite(writeOffset, sizeof(float));
         ProtonByteUtils.WriteFloat(value, array, (int)writeOffset);
         writeOffset += sizeof(float);
         return this;
      }

      public IProtonBuffer WriteInt(int value)
      {
         CheckWrite(writeOffset, sizeof(int));
         ProtonByteUtils.WriteInt(value, array, (int)writeOffset);
         writeOffset += sizeof(int);
         return this;
      }

      public IProtonBuffer WriteLong(long value)
      {
         CheckWrite(writeOffset, sizeof(long));
         ProtonByteUtils.WriteLong(value, array, (int)writeOffset);
         writeOffset += sizeof(long);
         return this;
      }

      public IProtonBuffer WriteShort(short value)
      {
         CheckWrite(writeOffset, sizeof(short));
         ProtonByteUtils.WriteShort(value, array, (int)writeOffset);
         writeOffset += sizeof(short);
         return this;
      }

      public IProtonBuffer WriteUnsignedByte(byte value)
      {
         CheckWrite(writeOffset, sizeof(byte));
         ProtonByteUtils.WriteUnsignedByte(value, array, (int)writeOffset);
         writeOffset += sizeof(byte);
         return this;
      }

      public IProtonBuffer WriteUnsignedInt(uint value)
      {
         CheckWrite(writeOffset, sizeof(uint));
         ProtonByteUtils.WriteUnsignedInt(value, array, (int)writeOffset);
         writeOffset += sizeof(uint);
         return this;
      }

      public IProtonBuffer WriteUnsignedLong(ulong value)
      {
         CheckWrite(writeOffset, sizeof(ulong));
         ProtonByteUtils.WriteUnsignedLong(value, array, (int)writeOffset);
         writeOffset += sizeof(ulong);
         return this;
      }

      public IProtonBuffer WriteUnsignedShort(ushort value)
      {
         CheckWrite(writeOffset, sizeof(ushort));
         ProtonByteUtils.WriteUnsignedShort(value, array, (int)writeOffset);
         writeOffset += sizeof(ushort);
         return this;
      }

      public IProtonBuffer WriteBytes(byte[] source)
      {
         long size = source.Length;
         long offset = WriteOffset;
         WriteOffset = offset + size;
         for (int i = 0; i < size; i++)
         {
            // TODO : Optimize
            SetUnsignedByte(offset + i, source[i]);
         }

         return this;
      }

      public IProtonBuffer WriteBytes(byte[] source, int offset, int length)
      {
         CheckWrite(writeOffset, length);

         throw new NotImplementedException();
      }

      public IProtonBuffer WriteBytes(IProtonBuffer source)
      {
         CheckWrite(writeOffset, source.ReadableBytes);

         throw new NotImplementedException();
      }

      #endregion

      #region Standard language API implementations

      public int CompareTo(object obj)
      {
         throw new NotImplementedException();
      }

      public int CompareTo(IProtonBuffer other)
      {
         throw new NotImplementedException();
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public override bool Equals(object other)
      {
         if (other is IProtonBuffer)
         {
            return this.Equals(other as IProtonBuffer);
         }
         else
         {
            return false;
         }
      }

      public bool Equals(IProtonBuffer other)
      {
         throw new NotImplementedException();
      }

      public string ToString(Encoding encoding)
      {
         throw new NotImplementedException();
      }

      public override string ToString()
      {
         return GetType().Name +
                "{ read:" + ReadOffset +
                ", write: " + WriteOffset +
                ", capacity: " + Capacity + "}";
      }

      #endregion

      #region Internal byte buffer utility methods

      private IProtonBuffer InternalEnsureWritable(long minWritableBytes)
      {
         // Called when we know that we don't need to validate if the minimum writable
         // value is negative.
         if (minWritableBytes <= WritableBytes)
         {
            return this;
         }

         if (minWritableBytes > maxCapacity - writeOffset)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "writeOffset(%d) + minWritableBytes(%d) exceeds maximum buffer capcity(%d): %s",
                writeOffset, minWritableBytes, maxCapacity, this));
         }

         long newCapacity = CalculateNewCapacity(writeOffset + minWritableBytes, maxCapacity);

         // Adjust to the new capacity.
         IncreaseCapacity(newCapacity);

         return this;
      }

      private long CalculateNewCapacity(long minNewCapacity, long maxCapacity)
      {
         if (minNewCapacity < 0)
         {
            throw new ArgumentOutOfRangeException("minNewCapacity: " + minNewCapacity + " (expected: 0+)");
         }

         if (minNewCapacity > maxCapacity)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "minNewCapacity: %d (expected: not greater than maximum buffer capacity(%d)",
                minNewCapacity, maxCapacity));
         }

         int newCapacity = 64;
         while (newCapacity < minNewCapacity)
         {
            newCapacity <<= 1;
         }

         return Math.Min(newCapacity, maxCapacity);
      }

      private void IncreaseCapacity(long newCapacity)
      {
         CheckNewCapacity(newCapacity);

         int oldCapacity = array.Length;
         if (newCapacity > oldCapacity)
         {
            byte[] newArray = new byte[newCapacity];
            Array.Copy(array, newArray, array.Length);
            array = newArray;
         }
      }

      private void CheckNewCapacity(long newCapacity)
      {
         if (newCapacity < 0 || newCapacity > maxCapacity)
         {
            throw new ArgumentOutOfRangeException("newCapacity: " + newCapacity + " (expected: 0-" + maxCapacity + ')');
         }
      }

      private void CheckWrite(long index, long size)
      {
         if (index < readOffset || array.LongLength < index + size)
         {
            throw OutOfBounds(index);
         }
      }

      private void CheckRead(long index, long size)
      {
         if (index < 0 || writeOffset < index + size)
         {
            throw OutOfBounds(index);
         }
      }

      private ArgumentOutOfRangeException OutOfBounds(long index)
      {
         return new ArgumentOutOfRangeException(
               "Index " + index + " is out of bounds: [read 0 to " + writeOffset + ", write 0 to " + array.Length + "].");
      }

      #endregion
   }
}