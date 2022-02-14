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
   public sealed class ProtonByteBuffer : IProtonBuffer, IReadableComponent, IWritableComponent
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

      /// <summary>
      /// How far into the array does position zero begin
      /// </summary>
      private int arrayOffset;

      /// <summary>
      /// Limits the usable region of the array if not set to the array length.
      /// This allows the window of operations to exists in a subregion of the
      /// array.
      /// </summary>
      private int arrayLimit;

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
      public ProtonByteBuffer(long initialCapacity) : this(initialCapacity, DefaultMaximumCapacity)
      {
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given initial capacity
      /// and limited to a max capacity of the given amount.
      /// </summary>
      /// <param name="initialCapacity">The initial capacity of this buffer</param>
      public ProtonByteBuffer(long initialCapacity, long maxCapacity) : base()
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
         this.arrayOffset = 0;
         this.arrayLimit = array.Length;
         this.maxCapacity = maxCapacity;
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given backing array whose
      /// size determines that largest the buffer can ever be.
      /// </summary>
      /// <param name="backingArray">The actual byte array that backs this buffer</param>
      public ProtonByteBuffer(byte[] backingArray) : this(backingArray, 0, Int32.MaxValue)
      {
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given backing array as the
      /// starting backing store and uses the provided max capacity value to control
      /// how large the buffer could ever grow.
      /// </summary>
      /// <param name="backingArray">The actual byte array that backs this buffer</param>
      /// <param name="maxCapacity">The maximum capcity this buffer can grow to</param>
      public ProtonByteBuffer(byte[] backingArray, long maxCapacity) : this(backingArray, 0, maxCapacity)
      {
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given backing array as the
      /// starting backing store and uses the provided max capacity value to control
      /// how large the buffer could ever grow.
      /// </summary>
      /// <param name="backingArray">The actual byte array that backs this buffer</param>
      /// <param name="arrayOffset">The offset into the backing array where the buffer starts</param>
      /// <param name="maxCapacity">The maximum capcity this buffer can grow to</param>
      public ProtonByteBuffer(byte[] backingArray, int arrayOffset, long maxCapacity)
       : this(backingArray, arrayOffset, backingArray.Length - arrayOffset, maxCapacity)
      {
      }

      /// <summary>
      /// Create a new proton byte buffer instance with given backing array as the
      /// starting backing store and uses the provided max capacity value to control
      /// how large the buffer could ever grow.
      /// </summary>
      /// <param name="backingArray">The actual byte array that backs this buffer</param>
      /// <param name="arrayOffset">The offset index into the backing array where the buffer starts</param>
      /// <param name="capacity">The capacity limit for this view of the assigned array</param>
      /// <param name="maxCapacity">The maximum capcity this buffer can grow to</param>
      public ProtonByteBuffer(byte[] backingArray, int arrayOffset, int capacity, long maxCapacity) : base()
      {
         if (arrayOffset > backingArray.Length)
         {
            throw new ArgumentOutOfRangeException("Array offset cannot exceed the array length");
         }

         if (capacity > backingArray.Length - arrayOffset)
         {
            throw new ArgumentOutOfRangeException(
               "Array segment capacity cannot exceed the configured array length minus the offset");
         }

         this.array = backingArray;
         this.arrayOffset = arrayOffset;
         this.arrayLimit = arrayOffset + capacity;
         this.maxCapacity = maxCapacity;
      }

      #region Buffer State and Management APIs

      public long Capacity => arrayLimit - arrayOffset;

      public bool IsReadable => ReadOffset < WriteOffset;

      public long ReadableBytes => WriteOffset - ReadOffset;

      public bool IsWritable => WriteOffset < Capacity;

      public long WritableBytes => Capacity - WriteOffset;

      public long ReadOffset
      {
         get => readOffset;
         set
         {
            CheckRead(value, 0);
            readOffset = value;
         }
      }

      public long WriteOffset
      {
         get => writeOffset;
         set
         {
            CheckWrite(value, 0);
            writeOffset = value;
         }
      }

      public uint ComponentCount => 1;

      public uint ReadableComponentCount => IsReadable ? 1u : 0u;

      public uint WritableComponentCount => IsWritable ? 1u : 0u;

      public IProtonBuffer EnsureWritable(long amount)
      {
         return InternalEnsureWritable(true, 1, amount);
      }

      public IProtonBuffer Compact()
      {
         if (readOffset != 0)
         {
            // Compress the current readable section into the front of the
            // array and then update the offsets to match the new reality.
            Array.Copy(array, Offset(readOffset), array, arrayOffset, writeOffset - readOffset);
            writeOffset -= readOffset;
            readOffset = 0;
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
         CheckRead(readOffset, amount);
         readOffset += amount;
         return this;
      }

      public IProtonBuffer Fill(byte value)
      {
         if (Capacity > 0)
         {
            Array.Fill<byte>(array, value, (int)arrayOffset, (int)Capacity);
         }
         return this;
      }

      #endregion

      #region Buffer Copy API implementation

      public IProtonBuffer Copy()
      {
         return Copy(readOffset, writeOffset - readOffset);
      }

      public IProtonBuffer Copy(long index, long length)
      {
         CheckGet(index, length);
         byte[] copyBytes = new byte[length];
         Array.Copy(array, Offset(index), copyBytes, 0, length);

         IProtonBuffer copy = new ProtonByteBuffer(copyBytes, maxCapacity);
         copy.WriteOffset = length;

         return copy;
      }

      public IProtonBuffer CopyInto(long srcPos, byte[] dest, long destPos, long length)
      {
         CheckCopyIntoArgs(srcPos, length, destPos, dest.LongLength);
         Array.Copy(array, Offset(srcPos), dest, destPos, length);
         return this;
      }

      public IProtonBuffer CopyInto(long srcPos, IProtonBuffer dest, long destPos, long length)
      {
         CheckCopyIntoArgs(srcPos, length, destPos, dest.Capacity);

         if (dest is ProtonByteBuffer)
         {
            ProtonByteBuffer destByteBuffer = (ProtonByteBuffer)dest;
            Array.Copy(array, Offset(srcPos), destByteBuffer.array, destPos, length);
         }
         else
         {
            long writePos = dest.WriteOffset;
            for (long i = 0; i < length; i++)
            {
               dest.SetUnsignedByte(writePos + i, array[srcPos + i]);
            }
         }

         return this;
      }

      #endregion

      #region Buffer iteration API implementations

      public bool HasReadableArray => true;

      public byte[] ReadableArray => array;

      public int ReadableArrayOffset => (int)Offset(readOffset); // Array backing means int casting is ok.

      public int ReadableArrayLength => (int)ReadableBytes; // Array backing means int casting is ok.

      public bool HasWritableArray => true;

      public byte[] WritableArray => array;

      public int WritableArrayOffset => (int)Offset(writeOffset); // Array backing means int casting is ok.

      public int WritableArrayLength => (int)WritableBytes; // Array backing means int casting is ok.

      public int ForEachReadableComponent(in int index, in Func<int, IReadableComponent, bool> processor)
      {
         CheckGet(readOffset, Math.Max(1, ReadableBytes));
         return processor.Invoke(index, this) ? 1 : -1;
      }

      public int ForEachWritableComponent(in int index, in Func<int, IWritableComponent, bool> processor)
      {
         CheckSet(writeOffset, Math.Max(1, WritableBytes));
         return processor.Invoke(index, this) ? 1 : -1;
      }

      #endregion

      #region Implementation of the proton buffer accessors API

      public bool GetBoolean(long index)
      {
         CheckGet(index, sizeof(byte));
         return ProtonByteUtils.ReadBoolean(array, (int)Offset(index));
      }

      public sbyte GetByte(long index)
      {
         CheckGet(index, sizeof(sbyte));
         return ProtonByteUtils.ReadByte(array, (int)Offset(index));
      }

      public char GetChar(long index)
      {
         CheckGet(index, sizeof(char));
         return ProtonByteUtils.ReadChar(array, (int)Offset(index));
      }

      public double GetDouble(long index)
      {
         CheckGet(index, sizeof(double));
         return ProtonByteUtils.ReadDouble(array, (int)Offset(index));
      }

      public float GetFloat(long index)
      {
         CheckGet(index, sizeof(float));
         return ProtonByteUtils.ReadFloat(array, (int)Offset(index));
      }

      public int GetInt(long index)
      {
         CheckGet(index, sizeof(int));
         return ProtonByteUtils.ReadInt(array, (int)Offset(index));
      }

      public long GetLong(long index)
      {
         CheckGet(index, sizeof(long));
         return ProtonByteUtils.ReadLong(array, (int)Offset(index));
      }

      public short GetShort(long index)
      {
         CheckGet(index, sizeof(short));
         return ProtonByteUtils.ReadShort(array, (int)Offset(index));
      }

      public byte GetUnsignedByte(long index)
      {
         CheckGet(index, sizeof(byte));
         return ProtonByteUtils.ReadUnsignedByte(array, (int)Offset(index));
      }

      public uint GetUnsignedInt(long index)
      {
         CheckGet(index, sizeof(uint));
         return ProtonByteUtils.ReadUnsignedInt(array, (int)Offset(index));
      }

      public ulong GetUnsignedLong(long index)
      {
         CheckGet(index, sizeof(ulong));
         return ProtonByteUtils.ReadUnsignedLong(array, (int)Offset(index));
      }

      public ushort GetUnsignedShort(long index)
      {
         CheckGet(index, sizeof(ushort));
         return ProtonByteUtils.ReadUnsignedShort(array, (int)Offset(index));
      }

      public bool ReadBoolean()
      {
         CheckRead(readOffset, sizeof(byte));
         bool result = ProtonByteUtils.ReadBoolean(array, (int)Offset(readOffset));
         readOffset += sizeof(byte);
         return result;
      }

      public sbyte ReadByte()
      {
         CheckRead(readOffset, sizeof(sbyte));
         sbyte result = ProtonByteUtils.ReadByte(array, (int)Offset(readOffset));
         readOffset += sizeof(sbyte);
         return result;
      }

      public char ReadChar()
      {
         CheckRead(readOffset, sizeof(char));
         char result = ProtonByteUtils.ReadChar(array, (int)Offset(readOffset));
         readOffset += sizeof(char);
         return result;
      }

      public double ReadDouble()
      {
         CheckRead(readOffset, sizeof(double));
         double result = ProtonByteUtils.ReadDouble(array, (int)Offset(readOffset));
         readOffset += sizeof(double);
         return result;
      }

      public float ReadFloat()
      {
         CheckRead(readOffset, sizeof(float));
         float result = ProtonByteUtils.ReadFloat(array, (int)Offset(readOffset));
         readOffset += sizeof(float);
         return result;
      }

      public int ReadInt()
      {
         CheckRead(readOffset, sizeof(int));
         int result = ProtonByteUtils.ReadInt(array, (int)Offset(readOffset));
         readOffset += sizeof(int);
         return result;
      }

      public long ReadLong()
      {
         CheckRead(readOffset, sizeof(long));
         long result = ProtonByteUtils.ReadLong(array, (int)Offset(readOffset));
         readOffset += sizeof(long);
         return result;
      }

      public short ReadShort()
      {
         CheckRead(readOffset, sizeof(short));
         short result = ProtonByteUtils.ReadShort(array, (int)Offset(readOffset));
         readOffset += sizeof(short);
         return result;
      }

      public byte ReadUnsignedByte()
      {
         CheckRead(readOffset, sizeof(byte));
         byte result = ProtonByteUtils.ReadUnsignedByte(array, (int)Offset(readOffset));
         readOffset += sizeof(byte);
         return result;
      }

      public uint ReadUnsignedInt()
      {
         CheckRead(readOffset, sizeof(uint));
         uint result = ProtonByteUtils.ReadUnsignedInt(array, (int)Offset(readOffset));
         readOffset += sizeof(uint);
         return result;
      }

      public ulong ReadUnsignedLong()
      {
         CheckRead(readOffset, sizeof(ulong));
         ulong result = ProtonByteUtils.ReadUnsignedLong(array, (int)Offset(readOffset));
         readOffset += sizeof(ulong);
         return result;
      }

      public ushort ReadUnsignedShort()
      {
         CheckRead(readOffset, sizeof(ushort));
         ushort result = ProtonByteUtils.ReadUnsignedShort(array, (int)Offset(readOffset));
         readOffset += sizeof(ushort);
         return result;
      }

      public IProtonBuffer SetBoolean(long index, bool value)
      {
         CheckSet(index, sizeof(byte));
         ProtonByteUtils.WriteBoolean(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetByte(long index, sbyte value)
      {
         CheckSet(index, sizeof(sbyte));
         ProtonByteUtils.WriteByte(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetChar(long index, char value)
      {
         CheckSet(index, sizeof(short));
         ProtonByteUtils.WriteShort((short)value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetDouble(long index, double value)
      {
         CheckSet(index, sizeof(double));
         ProtonByteUtils.WriteDouble(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetFloat(long index, float value)
      {
         CheckSet(index, sizeof(float));
         ProtonByteUtils.WriteFloat(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetInt(long index, int value)
      {
         CheckSet(index, sizeof(int));
         ProtonByteUtils.WriteInt(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetLong(long index, long value)
      {
         CheckSet(index, sizeof(long));
         ProtonByteUtils.WriteLong(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetShort(long index, short value)
      {
         CheckSet(index, sizeof(short));
         ProtonByteUtils.WriteShort(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetUnsignedByte(long index, byte value)
      {
         CheckSet(index, sizeof(byte));
         ProtonByteUtils.WriteUnsignedByte(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetUnsignedInt(long index, uint value)
      {
         CheckSet(index, sizeof(uint));
         ProtonByteUtils.WriteUnsignedInt(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetUnsignedLong(long index, ulong value)
      {
         CheckSet(index, sizeof(ulong));
         ProtonByteUtils.WriteUnsignedLong(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer SetUnsignedShort(long index, ushort value)
      {
         CheckSet(index, sizeof(ushort));
         ProtonByteUtils.WriteUnsignedShort(value, array, (int)Offset(index));
         return this;
      }

      public IProtonBuffer WriteBoolean(bool value)
      {
         CheckWrite(writeOffset, sizeof(byte));
         ProtonByteUtils.WriteBoolean(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(byte);
         return this;
      }

      public IProtonBuffer WriteByte(sbyte value)
      {
         CheckWrite(writeOffset, sizeof(byte));
         ProtonByteUtils.WriteByte(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(byte);
         return this;
      }

      public IProtonBuffer WriteDouble(double value)
      {
         CheckWrite(writeOffset, sizeof(double));
         ProtonByteUtils.WriteDouble(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(double);
         return this;
      }

      public IProtonBuffer WriteFloat(float value)
      {
         CheckWrite(writeOffset, sizeof(float));
         ProtonByteUtils.WriteFloat(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(float);
         return this;
      }

      public IProtonBuffer WriteInt(int value)
      {
         CheckWrite(writeOffset, sizeof(int));
         ProtonByteUtils.WriteInt(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(int);
         return this;
      }

      public IProtonBuffer WriteLong(long value)
      {
         CheckWrite(writeOffset, sizeof(long));
         ProtonByteUtils.WriteLong(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(long);
         return this;
      }

      public IProtonBuffer WriteShort(short value)
      {
         CheckWrite(writeOffset, sizeof(short));
         ProtonByteUtils.WriteShort(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(short);
         return this;
      }

      public IProtonBuffer WriteUnsignedByte(byte value)
      {
         CheckWrite(writeOffset, sizeof(byte));
         ProtonByteUtils.WriteUnsignedByte(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(byte);
         return this;
      }

      public IProtonBuffer WriteUnsignedInt(uint value)
      {
         CheckWrite(writeOffset, sizeof(uint));
         ProtonByteUtils.WriteUnsignedInt(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(uint);
         return this;
      }

      public IProtonBuffer WriteUnsignedLong(ulong value)
      {
         CheckWrite(writeOffset, sizeof(ulong));
         ProtonByteUtils.WriteUnsignedLong(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(ulong);
         return this;
      }

      public IProtonBuffer WriteUnsignedShort(ushort value)
      {
         CheckWrite(writeOffset, sizeof(ushort));
         ProtonByteUtils.WriteUnsignedShort(value, array, (int)Offset(writeOffset));
         writeOffset += sizeof(ushort);
         return this;
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
         if (source == null)
         {
            throw new ArgumentNullException("Input source array cannot be null");
         }

         if (length > source.LongLength - offset)
         {
            throw new ArgumentOutOfRangeException("Number of bytes to copy from source array: " + length +
                                                  " is greater than the readable span of the array: " +
                                                  (source.LongLength - offset));
         }

         CheckCopyIntoArgs(offset, length, writeOffset, Capacity);
         Array.Copy(source, offset, array, Offset(writeOffset), length);
         writeOffset += length;
         return this;
      }

      public IProtonBuffer WriteBytes(IProtonBuffer source)
      {
         long length = source.ReadableBytes;
         CheckWrite(writeOffset, length);
         source.CopyInto(source.ReadOffset, array, Offset(writeOffset), length);
         source.ReadOffset += length;
         writeOffset += length;

         return this;
      }

      #endregion

      #region Standard language API implementations

      public int CompareTo(object obj)
      {
         if (obj is IProtonBuffer)
         {
            return CompareTo(obj as IProtonBuffer);
         }

         throw new InvalidCastException("Cannot compare input type to an proton buffer type");
      }

      public int CompareTo(IProtonBuffer other)
      {
         long stopIndex = ReadOffset + Math.Min(ReadableBytes, other.ReadableBytes);

         for (long i = ReadOffset, j = other.ReadOffset; i < stopIndex; i++, j++)
         {
            int cmp = Compare(GetUnsignedByte(i), other.GetUnsignedByte(j));
            if (cmp != 0)
            {
               return cmp;
            }
         }

         return (int)(ReadableBytes - other.ReadableBytes);
      }

      public override int GetHashCode()
      {
         return ProtonBufferSupport.GetHashCode(this);
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
         return ProtonBufferSupport.Equals(this, other);
      }

      public string ToString(Encoding encoding)
      {
         if (!IsReadable)
         {
            return "";
         }

         Decoder decoder = encoding.GetDecoder();
         int charCount = decoder.GetCharCount(array, (int)Offset(readOffset), (int)(writeOffset - readOffset));
         char[] output = new char[charCount];

         int outputChars = decoder.GetChars(array, (int)Offset(readOffset), (int)(writeOffset - readOffset), output, 0);

         return new string(output, 0, outputChars);
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

      private long Offset(long index)
      {
         return index + arrayOffset;
      }

      private IProtonBuffer InternalEnsureWritable(bool allowCompaction, long minimumGrowth, long requiredWritable)
      {
         // Called when we know that we don't need to validate if the minimum writable
         // value is negative.
         if (requiredWritable <= WritableBytes)
         {
            return this;
         }

         if (requiredWritable > maxCapacity - writeOffset)
         {
            throw new ArgumentOutOfRangeException(string.Format(
                "writeOffset(%d) + requiredWritable(%d) exceeds maximum buffer capcity(%d): %s",
                writeOffset, requiredWritable, maxCapacity, this));
         }

         // Can we solve this with compaction instead of creating more buffers
         if (allowCompaction && WritableBytes + ReadOffset >= requiredWritable)
         {
            Compact();
         }
         else
         {
            long newCapacity = Capacity + (long)Math.Max(requiredWritable - WritableBytes, minimumGrowth);
            if (newCapacity < 1)
            {
               throw new ArgumentOutOfRangeException("Buffer size must be positive, but was " + newCapacity + '.');
            }

            int oldCapacity = arrayLimit - arrayOffset;
            if (newCapacity > oldCapacity)
            {
               byte[] newArray = new byte[newCapacity];
               Array.ConstrainedCopy(array, arrayOffset, newArray, 0, arrayLimit - arrayOffset);
               array = newArray;
               arrayOffset = 0;
               arrayLimit = newArray.Length;
            }
         }

         return this;
      }

      private void CheckWrite(long index, long size)
      {
         if (index < readOffset || (arrayLimit - arrayOffset) < index + size)
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

      private void CheckGet(long index, long size)
      {
         if (index < 0 || Capacity < index + size)
         {
            throw OutOfBounds(index);
         }
      }

      private void CheckSet(long index, long size)
      {
         if (index < 0 || Capacity < index + size)
         {
            throw OutOfBounds(index);
         }
      }

      private void CheckCopyIntoArgs(long srcPos, long length, long destPos, long destLength)
      {
         if (srcPos < 0)
         {
            throw new ArgumentOutOfRangeException("The srcPos cannot be negative: " + srcPos + '.');
         }
         if (length < 0)
         {
            throw new ArgumentOutOfRangeException("The length cannot be negative: " + length + '.');
         }
         if (Capacity < srcPos + length)
         {
            throw new ArgumentOutOfRangeException("The srcPos + length is beyond the end of the buffer: " +
                                                  "srcPos = " + srcPos + ", length = " + length + '.');
         }
         if (destPos < 0)
         {
            throw new ArgumentOutOfRangeException("The destPos cannot be negative: " + destPos + '.');
         }
         if (destLength < destPos + length)
         {
            throw new ArgumentOutOfRangeException("The destPos + length is beyond the end of the destination: " +
                                                  "destPos = " + destPos + ", length = " + length + '.');
         }
      }

      private Exception OutOfBounds(long index)
      {
         return new IndexOutOfRangeException(
               "Index " + index + " is out of bounds: [read 0 to " + writeOffset + ", write 0 to " + Capacity + "].");
      }

      private static int Compare(byte x, byte y)
      {
         return (x < y) ? -1 : ((x == y) ? 0 : 1);
      }

      #endregion
   }
}