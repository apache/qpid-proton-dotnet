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
using NUnit.Framework;

namespace Apache.Qpid.Proton.Buffer
{
   /// <summary>
   /// A collection of tests cases that can be run on any IProtonBuffer implementation.
   /// </summary>
   public abstract class ProtonAbstractByteBufferTests
   {
      public static readonly int LargeCapacity = 4096; // Must be even for these tests
      public static readonly int BlockSize = 128;
      public static readonly int DefaultCapacity = 64;

      protected int seed;
      protected Random random;

      [SetUp]
      public virtual void Setup()
      {
         seed = Environment.TickCount;
         random = new Random(seed);
      }

      #region Test Buffer creation

      [Test]
      public void TestConstructWithDifferingCapacityAndMaxCapacity()
      {
         Assume.That(CanBufferCapacityBeChanged());

         int baseCapacity = DefaultCapacity + 10;

         IProtonBuffer buffer = AllocateBuffer(baseCapacity, baseCapacity + 100);

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(baseCapacity, buffer.Capacity);
         buffer.EnsureWritable(baseCapacity + 100);

         try
         {
            buffer.EnsureWritable(baseCapacity + 101);
            Assert.Fail("Should not be able to reserve more than the max capacity bytes");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestBufferRespectsMaxCapacityAfterGrowingToFit()
      {
         Assume.That(CanBufferCapacityBeChanged());

         IProtonBuffer buffer = AllocateBuffer(5, 10);

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(5, buffer.Capacity);

         for (sbyte i = 0; i < 10; ++i)
         {
            buffer.EnsureWritable(sizeof(sbyte));
            buffer.WriteByte(i);
         }

         try
         {
            buffer.WriteByte(10);
            Assert.Fail("Should not be able to write more than the max capacity bytes");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestBufferRespectsMaxCapacityLimitNoGrowthScenario()
      {
         IProtonBuffer buffer = AllocateBuffer(10, 10);

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(10, buffer.Capacity);

         // Writes to capacity work, but exceeding that should Assert.Fail.
         for (sbyte i = 0; i < 10; ++i)
         {
            buffer.WriteByte(i);
         }

         try
         {
            buffer.WriteByte(10);
            Assert.Fail("Should not be able to write more than the max capacity bytes");
         }
         catch (IndexOutOfRangeException) { }
      }

      #endregion

      #region Tests for altering a buffer's capacity

      [Test]
      public void TestFillEmptyBuffer()
      {
         IProtonBuffer buffer = AllocateBuffer(10, 10);
         buffer.Fill(1);

         for (int i = 0; i < buffer.Capacity; ++i)
         {
            Assert.AreEqual(1, buffer.GetByte(i));
         }
      }

      [Test]
      public void TestFillBufferWithNonDefaultOffsets()
      {
         IProtonBuffer buffer = AllocateBuffer(10, 10);
         buffer.Fill(1);

         for (int i = 0; i < buffer.Capacity; ++i)
         {
            Assert.AreEqual(1, buffer.GetByte(i));
         }

         buffer.WriteOffset = 8;
         buffer.ReadOffset = 3;

         buffer.Fill(2);

         for (int i = 0; i < buffer.Capacity; ++i)
         {
            Assert.AreEqual(2, buffer.GetByte(i));
         }
      }

      #endregion

      #region Tests for altering a buffer's capacity

      [Test]
      public void TestCapacityEnforceMaxCapacity()
      {
         Assume.That(CanBufferCapacityBeChanged());

         IProtonBuffer buffer = AllocateBuffer(3, 13);
         Assert.AreEqual(3, buffer.Capacity);

         Assert.Throws(typeof(ArgumentOutOfRangeException), () => buffer.EnsureWritable(14));
      }

      [Test]
      public void TestCapacityNegative()
      {
         Assume.That(CanBufferCapacityBeChanged());

         IProtonBuffer buffer = AllocateBuffer(3, 13);
         Assert.AreEqual(3, buffer.Capacity);

         Assert.DoesNotThrow(() => buffer.EnsureWritable(-1));
      }

      [Test]
      public void TestCapacityIncrease()
      {
         Assume.That(CanBufferCapacityBeChanged());

         IProtonBuffer buffer = AllocateBuffer(3, 13);
         Assert.AreEqual(3, buffer.Capacity);
         buffer.EnsureWritable(4);
         Assert.AreEqual(4, buffer.Capacity);
      }

      #endregion

      #region Tests for altering buffer properties

      [Test]
      public void TestSetReadOffsetWithNegative()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         try
         {
            buffer.ReadOffset = -1;
            Assert.Fail("Should not accept negative values");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetReadOffsetGreaterThanCapacity()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         try
         {
            buffer.ReadOffset = buffer.Capacity + buffer.Capacity;
            Assert.Fail("Should not accept values bigger than capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestWriteOffsetWithNegative()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         try
         {
            buffer.WriteOffset = -1;
            Assert.Fail("Should not accept negative values");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestWriteOffsetGreaterThanCapacity()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         try
         {
            buffer.WriteOffset = buffer.Capacity + buffer.Capacity;
            Assert.Fail("Should not accept values bigger than capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestIsReadable()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         Assert.IsFalse(buffer.IsReadable);
         Assert.IsTrue(buffer.IsWritable);
         buffer.WriteBoolean(false);
         Assert.IsTrue(buffer.IsReadable);
      }

      [Test]
      public void TestIsReadableWithAmount()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         Assert.IsFalse(buffer.ReadableBytes > 0);
         buffer.WriteBoolean(false);
         Assert.IsTrue(buffer.ReadableBytes == 1);
      }

      [Test]
      public void TestIsWriteable()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         Assert.IsTrue(buffer.IsWritable);
         buffer.WriteOffset = buffer.Capacity;
         Assert.IsFalse(buffer.IsWritable);
      }

      [Test]
      public void TestIsWriteableWithAmount()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         Assert.IsTrue(buffer.IsWritable);
         buffer.WriteOffset = buffer.Capacity - 1;
         Assert.IsTrue(buffer.WritableBytes == 1);
      }

      [Test]
      public void TestClear()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(0, buffer.WriteOffset);
         buffer.WriteOffset = 20;
         buffer.ReadOffset = 10;
         Assert.AreEqual(10, buffer.ReadOffset);
         Assert.AreEqual(20, buffer.WriteOffset);
         buffer.Reset();
         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(0, buffer.WriteOffset);
      }

      [Test]
      public void TestSkipBytes()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         buffer.WriteOffset = buffer.Capacity / 2;
         Assert.AreEqual(0, buffer.ReadOffset);
         buffer.SkipBytes(buffer.Capacity / 2);
         Assert.AreEqual(buffer.Capacity / 2, buffer.ReadOffset);
      }

      [Test]
      public void TestSkipBytesBeyondReadable()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         buffer.WriteOffset = buffer.Capacity / 2;
         Assert.AreEqual(0, buffer.ReadOffset);

         try
         {
            buffer.SkipBytes(buffer.ReadableBytes + 50);
            Assert.Fail("Should not be able to skip beyond write index");
         }
         catch (IndexOutOfRangeException) { }
      }

      #endregion

      #region Tests for altering buffer capacity

      [Test]
      public void TestIncreaseCapacity()
      {
         Assume.That(CanBufferCapacityBeChanged());

         byte[] source = new byte[100];

         IProtonBuffer buffer = AllocateBuffer(100).WriteBytes(source);
         Assert.AreEqual(100, buffer.Capacity);
         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(100, buffer.WriteOffset);

         buffer.EnsureWritable(200);
         Assert.AreEqual(300, buffer.Capacity);

         buffer.EnsureWritable(200);
         Assert.AreEqual(300, buffer.Capacity);

         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(100, buffer.WriteOffset);
      }

      #endregion

      #region Write Bytes to proton buffer tests

      [Test]
      public void TestWriteBytes()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteBytes(payload);

         Assert.AreEqual(payload.LongLength, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0; i < payload.LongLength; ++i)
         {
            Assert.AreEqual(payload[i], buffer.ReadUnsignedByte());
         }

         Assert.AreEqual(payload.LongLength, buffer.ReadOffset);
      }

      [Test]
      public void TestWriteBytesWithEmptyArray()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteBytes(new byte[0]);

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);
      }

      [Test]
      public void TestWriteBytesNPEWhenNullGiven()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         try
         {
            buffer.WriteBytes((byte[])null);
            Assert.Fail();
         }
         catch (ArgumentNullException) { }

         try
         {
            buffer.WriteBytes((byte[])null, 0, 0);
            Assert.Fail();
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestWriteBytesWithLength()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteBytes(payload, 0, payload.LongLength);

         Assert.AreEqual(payload.LongLength, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0; i < payload.LongLength; ++i)
         {
            Assert.AreEqual(payload[i], buffer.ReadByte());
         }

         Assert.AreEqual(payload.LongLength, buffer.ReadOffset);
      }

      [Test]
      public void TestWriteBytesWithLengthToBig()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         try
         {
            buffer.WriteBytes(payload, 0, payload.LongLength + 1);
            Assert.Fail("Should not write when.Length given is to large.");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestWriteBytesWithNegativeLength()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         try
         {
            buffer.WriteBytes(payload, 0, -1);
            Assert.Fail("Should not write when.Length given is negative.");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestWriteBytesWithLengthAndOffset()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteBytes(payload, 0, payload.LongLength);

         Assert.AreEqual(payload.LongLength, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0; i < payload.LongLength; ++i)
         {
            Assert.AreEqual(payload[i], buffer.ReadByte());
         }

         Assert.AreEqual(payload.LongLength, buffer.ReadOffset);
      }

      [Test]
      public void TestWriteBytesWithLengthAndOffsetIncorrect()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         try
         {
            buffer.WriteBytes(payload, 0, payload.LongLength + 1);
            Assert.Fail("Should not write when.Length given is to large.");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            buffer.WriteBytes(payload, -1, payload.LongLength);
            Assert.Fail("Should not write when offset given is negative.");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            buffer.WriteBytes(payload, 0, -1);
            Assert.Fail("Should not write when.Length given is negative.");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            buffer.WriteBytes(payload, payload.LongLength + 1, 1);
            Assert.Fail("Should not write when offset given is to large.");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestWriteBytesFromProtonBuffer()
      {
         IProtonBuffer source = new ProtonByteBuffer(new byte[] { 0, 1, 2, 3, 4, 5 });
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteBytes(source);

         Assert.AreEqual(0, source.ReadableBytes);
         Assert.AreEqual(source.WriteOffset, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0; i < source.ReadableBytes; ++i)
         {
            Assert.AreEqual(source.GetByte(i), buffer.ReadByte());
         }

         Assert.AreEqual(source.ReadableBytes, buffer.ReadOffset);
      }

      #endregion

      #region  Write Primitives Tests

      [Test]
      public void TestWriteByte()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteByte((sbyte)56);

         Assert.AreEqual(1, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(56, buffer.ReadByte());

         Assert.AreEqual(1, buffer.WriteOffset);
         Assert.AreEqual(1, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteUnsignedByte()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteUnsignedByte((byte)56);

         Assert.AreEqual(1, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(56, buffer.ReadUnsignedByte());

         Assert.AreEqual(1, buffer.WriteOffset);
         Assert.AreEqual(1, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteBoolean()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteBoolean(true);
         buffer.WriteBoolean(false);

         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(true, buffer.ReadBoolean());

         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(1, buffer.ReadOffset);

         Assert.AreEqual(1, buffer.ReadableBytes);
         Assert.AreEqual(false, buffer.ReadBoolean());
         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(2, buffer.ReadOffset);
      }

      [Test]
      public void TestWriteShort()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteShort((short)42);

         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(42, buffer.ReadShort());

         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(2, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteUnsignedShort()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteUnsignedShort((ushort)42);

         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(42, buffer.ReadUnsignedShort());

         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(2, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteInt()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteInt(72);

         Assert.AreEqual(4, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(72, buffer.ReadInt());

         Assert.AreEqual(4, buffer.WriteOffset);
         Assert.AreEqual(4, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteUnsignedInt()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteUnsignedInt(72u);

         Assert.AreEqual(4, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(72u, buffer.ReadUnsignedInt());

         Assert.AreEqual(4, buffer.WriteOffset);
         Assert.AreEqual(4, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteLong()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteLong(500L);

         Assert.AreEqual(8, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(500L, buffer.ReadLong());

         Assert.AreEqual(8, buffer.WriteOffset);
         Assert.AreEqual(8, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteUnsignedLong()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteUnsignedLong(500ul);

         Assert.AreEqual(8, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(500ul, buffer.ReadUnsignedLong());

         Assert.AreEqual(8, buffer.WriteOffset);
         Assert.AreEqual(8, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteFloat()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteFloat(35.5f);

         Assert.AreEqual(4, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(35.5f, buffer.ReadFloat(), 0.4f);

         Assert.AreEqual(4, buffer.WriteOffset);
         Assert.AreEqual(4, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      [Test]
      public void TestWriteDouble()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.WriteDouble(1.66);

         Assert.AreEqual(8, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(1.66, buffer.ReadDouble(), 0.1);

         Assert.AreEqual(8, buffer.WriteOffset);
         Assert.AreEqual(8, buffer.ReadOffset);

         Assert.AreEqual(0, buffer.ReadableBytes);
      }

      #endregion

      #region Tests for read operations

      [Test]
      public void TestReadByte()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; ++i)
         {
            Assert.AreEqual(source[i], buffer.ReadByte());
         }

         try
         {
            buffer.ReadByte();
            Assert.Fail("Should not be able to read beyond current capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestReadBoolean()
      {
         byte[] source = new byte[] { 0, 1, 0, 1, 0, 1 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; ++i)
         {
            if ((i % 2) == 0)
            {
               Assert.IsFalse(buffer.ReadBoolean());
            }
            else
            {
               Assert.IsTrue(buffer.ReadBoolean());
            }
         }

         try
         {
            buffer.ReadBoolean();
            Assert.Fail("Should not be able to read beyond current capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestReadShort()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         buffer.WriteShort((short)2);
         buffer.WriteShort((short)20);
         buffer.WriteShort((short)200);
         buffer.WriteShort((short)256);
         buffer.WriteShort((short)512);
         buffer.WriteShort((short)1025);
         buffer.WriteShort((short)32767);
         buffer.WriteShort((short)-1);
         buffer.WriteShort((short)-8757);

         Assert.AreEqual(2, buffer.ReadShort());
         Assert.AreEqual(20, buffer.ReadShort());
         Assert.AreEqual(200, buffer.ReadShort());
         Assert.AreEqual(256, buffer.ReadShort());
         Assert.AreEqual(512, buffer.ReadShort());
         Assert.AreEqual(1025, buffer.ReadShort());
         Assert.AreEqual(32767, buffer.ReadShort());
         Assert.AreEqual(-1, buffer.ReadShort());
         Assert.AreEqual(-8757, buffer.ReadShort());

         try
         {
            buffer.ReadShort();
            Assert.Fail("Should not be able to read beyond current capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestReadInt()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         buffer.WriteInt((short)2);
         buffer.WriteInt((short)20);
         buffer.WriteInt((short)200);
         buffer.WriteInt((short)256);
         buffer.WriteInt((short)512);
         buffer.WriteInt((short)1025);
         buffer.WriteInt((short)32767);
         buffer.WriteInt((short)-1);
         buffer.WriteInt((short)-8757);

         Assert.AreEqual(2, buffer.ReadInt());
         Assert.AreEqual(20, buffer.ReadInt());
         Assert.AreEqual(200, buffer.ReadInt());
         Assert.AreEqual(256, buffer.ReadInt());
         Assert.AreEqual(512, buffer.ReadInt());
         Assert.AreEqual(1025, buffer.ReadInt());
         Assert.AreEqual(32767, buffer.ReadInt());
         Assert.AreEqual(-1, buffer.ReadInt());
         Assert.AreEqual(-8757, buffer.ReadInt());

         try
         {
            buffer.ReadInt();
            Assert.Fail("Should not be able to read beyond current capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestReadLong()
      {
         // This is not a capacity increase handling test so allocate with enough capacity for this test.
         IProtonBuffer buffer = AllocateBuffer(DefaultCapacity * 2);

         buffer.WriteLong((short)2);
         buffer.WriteLong((short)20);
         buffer.WriteLong((short)200);
         buffer.WriteLong((short)256);
         buffer.WriteLong((short)512);
         buffer.WriteLong((short)1025);
         buffer.WriteLong((short)32767);
         buffer.WriteLong((short)-1);
         buffer.WriteLong((short)-8757);

         Assert.AreEqual(2, buffer.ReadLong());
         Assert.AreEqual(20, buffer.ReadLong());
         Assert.AreEqual(200, buffer.ReadLong());
         Assert.AreEqual(256, buffer.ReadLong());
         Assert.AreEqual(512, buffer.ReadLong());
         Assert.AreEqual(1025, buffer.ReadLong());
         Assert.AreEqual(32767, buffer.ReadLong());
         Assert.AreEqual(-1, buffer.ReadLong());
         Assert.AreEqual(-8757, buffer.ReadLong());

         try
         {
            buffer.ReadLong();
            Assert.Fail("Should not be able to read beyond current readable bytes");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestReadFloat()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         buffer.WriteFloat(1.111f);
         buffer.WriteFloat(2.222f);
         buffer.WriteFloat(3.333f);

         Assert.AreEqual(1.111f, buffer.ReadFloat(), 0.111f);
         Assert.AreEqual(2.222f, buffer.ReadFloat(), 0.222f);
         Assert.AreEqual(3.333f, buffer.ReadFloat(), 0.333f);

         try
         {
            buffer.ReadFloat();
            Assert.Fail("Should not be able to read beyond current capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestReadDouble()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();

         buffer.WriteDouble(1.111);
         buffer.WriteDouble(2.222);
         buffer.WriteDouble(3.333);

         Assert.AreEqual(1.111, buffer.ReadDouble(), 0.111);
         Assert.AreEqual(2.222, buffer.ReadDouble(), 0.222);
         Assert.AreEqual(3.333, buffer.ReadDouble(), 0.333);

         try
         {
            buffer.ReadDouble();
            Assert.Fail("Should not be able to read beyond current capacity");
         }
         catch (IndexOutOfRangeException) { }
      }

      #endregion

      #region  Tests for get operations

      [Test]
      public void TestGetByte()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; ++i)
         {
            Assert.AreEqual((sbyte)source[i], buffer.GetByte(i));
         }

         try
         {
            buffer.ReadUnsignedByte();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetUnsignedByte()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; ++i)
         {
            Assert.AreEqual(source[i], buffer.GetUnsignedByte(i));
         }

         try
         {
            buffer.ReadUnsignedByte();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetBoolean()
      {
         byte[] source = new byte[] { 0, 1, 0, 1, 0, 1 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; ++i)
         {
            if ((i % 2) == 0)
            {
               Assert.IsFalse(buffer.GetBoolean(i));
            }
            else
            {
               Assert.IsTrue(buffer.GetBoolean(i));
            }
         }

         try
         {
            buffer.ReadBoolean();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetShort()
      {
         byte[] source = new byte[] { 0, 0, 0, 1, 0, 2, 0, 3, 0, 4 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; i += 2)
         {
            Assert.AreEqual(source[i + 1], buffer.GetShort(i));
         }

         try
         {
            buffer.ReadShort();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetUnsignedShort()
      {
         byte[] source = new byte[] { 0, 0, 0, 1, 0, 2, 0, 3, 0, 4 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; i += 2)
         {
            Assert.AreEqual(source[i + 1], buffer.GetUnsignedShort(i));
         }

         try
         {
            buffer.ReadUnsignedShort();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetInt()
      {
         byte[] source = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 4 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; i += 4)
         {
            Assert.AreEqual(source[i + 3], buffer.GetInt(i));
         }

         try
         {
            buffer.ReadInt();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetUnsignedInt()
      {
         byte[] source = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 3, 0, 0, 0, 4 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; i += 4)
         {
            Assert.AreEqual(source[i + 3], buffer.GetUnsignedInt(i));
         }

         try
         {
            buffer.ReadUnsignedInt();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetLong()
      {
         byte[] source = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0,
                                      0, 0, 0, 0, 0, 0, 0, 1,
                                      0, 0, 0, 0, 0, 0, 0, 2,
                                      0, 0, 0, 0, 0, 0, 0, 3,
                                      0, 0, 0, 0, 0, 0, 0, 4 };
         IProtonBuffer buffer = WrapBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         for (int i = 0; i < source.LongLength; i += 8)
         {
            Assert.AreEqual(source[i + 7], buffer.GetLong(i));
         }

         try
         {
            buffer.ReadLong();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetFloat()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         buffer.WriteInt(BitConverter.SingleToInt32Bits(1.1f));
         buffer.WriteInt(BitConverter.SingleToInt32Bits(2.2f));
         buffer.WriteInt(BitConverter.SingleToInt32Bits(42.3f));

         Assert.AreEqual(sizeof(int) * 3, buffer.ReadableBytes);

         Assert.AreEqual(1.1f, buffer.GetFloat(0), 0.1);
         Assert.AreEqual(2.2f, buffer.GetFloat(4), 0.1);
         Assert.AreEqual(42.3f, buffer.GetFloat(8), 0.1);

         try
         {
            buffer.ReadFloat();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      [Test]
      public void TestGetDouble()
      {
         IProtonBuffer buffer = AllocateDefaultBuffer();
         buffer.WriteLong(BitConverter.DoubleToInt64Bits(1.1));
         buffer.WriteLong(BitConverter.DoubleToInt64Bits(2.2));
         buffer.WriteLong(BitConverter.DoubleToInt64Bits(42.3));

         Assert.AreEqual(sizeof(long) * 3, buffer.ReadableBytes);

         Assert.AreEqual(1.1, buffer.GetDouble(0), 0.1);
         Assert.AreEqual(2.2, buffer.GetDouble(8), 0.1);
         Assert.AreEqual(42.3, buffer.GetDouble(16), 0.1);

         try
         {
            buffer.ReadDouble();
         }
         catch (IndexOutOfRangeException)
         {
            Assert.Fail("Should be able to read from the buffer");
         }
      }

      #endregion

      #region Tests for Copy operations

      [Test]
      public void TestCopyEmptyBuffer()
      {
         IProtonBuffer buffer = AllocateBuffer(10);
         IProtonBuffer copy = buffer.Copy();

         Assert.AreEqual(buffer.ReadableBytes, copy.ReadableBytes);
         Assert.AreEqual(buffer, copy);

         copy.EnsureWritable(10);
         copy.WriteByte(1);

         buffer.WriteOffset = 1;

         Assert.AreNotEqual(buffer, copy);
      }

      [Test]
      public void TestCopyBuffer()
      {
         IProtonBuffer buffer = AllocateBuffer(10);

         buffer.WriteByte(1);
         buffer.WriteByte(2);
         buffer.WriteByte(3);
         buffer.WriteByte(4);
         buffer.WriteByte(5);

         IProtonBuffer copy = buffer.Copy();

         Assert.AreEqual(buffer.ReadableBytes, copy.ReadableBytes);

         for (int i = 0; i < 5; ++i)
         {
            Assert.AreEqual(buffer.GetByte(i), copy.GetByte(i));
         }
      }

      [Test]
      public void TestCopy()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         for (long i = 0; i < buffer.Capacity; i++)
         {
            byte value = (byte)random.Next();
            buffer.SetUnsignedByte(i, value);
         }

         long readerIndex = LargeCapacity / 3;
         long writerIndex = LargeCapacity * 2 / 3;

         buffer.WriteOffset = writerIndex;
         buffer.ReadOffset = readerIndex;

         // Make sure all properties are copied.
         IProtonBuffer copy = buffer.Copy();
         Assert.AreEqual(0, copy.ReadOffset);
         Assert.AreEqual(buffer.ReadableBytes, copy.WriteOffset);
         Assert.AreEqual(buffer.ReadableBytes, copy.Capacity);
         for (int i = 0; i < copy.Capacity; i++)
         {
            Assert.AreEqual(buffer.GetByte(i + readerIndex), copy.GetByte(i));
         }

         // Make sure the buffer content is independent from each other.
         buffer.SetByte(readerIndex, (sbyte)(buffer.GetByte(readerIndex) + 1));
         Assert.IsTrue(buffer.GetByte(readerIndex) != copy.GetByte(0));
         copy.SetByte(1, (sbyte)(copy.GetByte(1) + 1));
         Assert.IsTrue(buffer.GetByte(readerIndex + 1) != copy.GetByte(1));
      }

      [Test]
      public void TestSequentialRandomFilledBufferIndexedCopy()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         byte[] value = new byte[BlockSize];
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(value);
            buffer.WriteOffset = i;
            buffer.WriteBytes(value);
         }

         buffer.WriteOffset = buffer.Capacity - 1;

         random = new Random(seed);

         byte[] expectedValueContent = new byte[BlockSize];
         IProtonBuffer expectedValue = new ProtonByteBuffer(expectedValueContent);
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(expectedValueContent);
            IProtonBuffer copy = buffer.Copy(i, BlockSize);
            for (int j = 0; j < BlockSize; j++)
            {
               Assert.AreEqual(expectedValue.GetByte(j), copy.GetByte(j));
            }
         }
      }

      #endregion

      #region Tests for string conversion

      [Test]
      public void TestToStringFromUTF8()
      {
         String sourceString = "Test-String-1";
         Encoding encoding = new UTF8Encoding();
         byte[] utf8 = encoding.GetBytes(sourceString);

         IProtonBuffer buffer = WrapBuffer(utf8);

         String decoded = buffer.ToString(encoding);

         Assert.AreEqual(sourceString, decoded);
      }

      #endregion

      #region Tests for equality and comparison

      [Test]
      public void TestEqualsSelf()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer = WrapBuffer(payload);
         Assert.IsTrue(buffer.Equals(buffer));
      }

      [Test]
      public void TestEqualsWithSameContents()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload);
         IProtonBuffer buffer2 = WrapBuffer(payload);

         Assert.IsTrue(buffer1.Equals(buffer2));
         Assert.IsTrue(buffer2.Equals(buffer1));
      }

      [Test]
      public void TestEqualsWithSameContentDifferenceArrays()
      {
         byte[] payload1 = new byte[] { 0, 1, 2, 3, 4 };
         byte[] payload2 = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload1);
         IProtonBuffer buffer2 = WrapBuffer(payload2);

         Assert.IsTrue(buffer1.Equals(buffer2));
         Assert.IsTrue(buffer2.Equals(buffer1));
      }

      [Test]
      public void TestEqualsWithDiffereingContent()
      {
         byte[] payload1 = new byte[] { 1, 2, 3, 4, 5 };
         byte[] payload2 = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload1);
         IProtonBuffer buffer2 = WrapBuffer(payload2);

         Assert.IsFalse(buffer1.Equals(buffer2));
         Assert.IsFalse(buffer2.Equals(buffer1));
      }

      [Test]
      public void TestEqualsWithDifferingReadableBytes()
      {
         byte[] payload1 = new byte[] { 0, 1, 2, 3, 4 };
         byte[] payload2 = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload1);
         IProtonBuffer buffer2 = WrapBuffer(payload2);

         buffer1.ReadUnsignedByte();

         Assert.IsFalse(buffer1.Equals(buffer2));
         Assert.IsFalse(buffer2.Equals(buffer1));
      }

      [Test]
      public void TestHashCode()
      {
         byte[] payload1 = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload1);
         Assert.AreNotEqual(0, buffer1.GetHashCode());

         byte[] payload2 = new byte[] { 5, 6, 7, 8, 9 };
         IProtonBuffer buffer2 = WrapBuffer(payload2);
         Assert.AreNotEqual(0, buffer1.GetHashCode());

         IProtonBuffer buffer3 = WrapBuffer(payload1);

         Assert.AreNotEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
         Assert.AreEqual(buffer1.GetHashCode(), buffer3.GetHashCode());
      }

      [Test]
      public void TestCompareToSameContents()
      {
         byte[] payload1 = new byte[] { 0, 1, 2, 3, 4 };
         byte[] payload2 = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload1);
         IProtonBuffer buffer2 = WrapBuffer(payload2);

         Assert.AreEqual(0, buffer1.CompareTo(buffer1));
         Assert.AreEqual(0, buffer1.CompareTo(buffer2));
         Assert.AreEqual(0, buffer2.CompareTo(buffer1));
      }

      [Test]
      public void TestCompareToSameContentsButInOffsetBuffers()
      {
         byte[] payload1 = new byte[] { 0, 1, 2, 3, 4 };
         byte[] payload2 = new byte[] { 9, 9, 9, 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload1);
         IProtonBuffer buffer2 = WrapBuffer(payload2);

         buffer2.ReadOffset = 3;

         Assert.AreEqual(0, buffer1.CompareTo(buffer1));
         Assert.AreEqual(0, buffer1.CompareTo(buffer2));
         Assert.AreEqual(0, buffer2.CompareTo(buffer1));
      }

      [Test]
      public void TestCompareToDifferentContents()
      {
         byte[] payload1 = new byte[] { 1, 2, 3, 4, 5 };
         byte[] payload2 = new byte[] { 0, 1, 2, 3, 4 };
         IProtonBuffer buffer1 = WrapBuffer(payload1);
         IProtonBuffer buffer2 = WrapBuffer(payload2);

         Assert.AreEqual(1, buffer1.CompareTo(buffer2));
         Assert.AreEqual(-1, buffer2.CompareTo(buffer1));
      }

      [Test]
      public void TestComparableInterfaceNotViolatedWithLongWrites()
      {
         IProtonBuffer buffer1 = AllocateBuffer(LargeCapacity);
         IProtonBuffer buffer2 = AllocateBuffer(LargeCapacity);

         buffer1.WriteOffset = buffer1.ReadOffset;
         buffer1.WriteLong(0);

         buffer2.WriteOffset = buffer2.ReadOffset;
         buffer2.WriteLong(0xF0000000L);

         Assert.IsTrue(buffer1.CompareTo(buffer2) < 0);
         Assert.IsTrue(buffer2.CompareTo(buffer1) > 0);
      }

      [Test]
      public void TestCompareToContract()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         try
         {
            buffer.CompareTo(null);
            Assert.Fail();
         }
         catch (NullReferenceException)
         {
            // Expected
         }

         // Fill the random stuff
         byte[] value = new byte[32];
         random.NextBytes(value);
         // Prevent overflow / underflow
         if (value[0] == 0)
         {
            value[0]++;
         }
         else if (value[0] == 255)
         {
            value[0]--;
         }

         buffer.Reset();
         buffer.WriteBytes(value);

         IProtonBuffer wrapped = ProtonByteBufferAllocator.Instance.Wrap(value);

         Assert.AreEqual(0, buffer.CompareTo(wrapped));

         value[0]++;
         Assert.IsTrue(buffer.CompareTo(wrapped) < 0);
         value[0] -= 2;
         Assert.IsTrue(buffer.CompareTo(wrapped) > 0);
         value[0]++;

         IProtonBuffer compared = new ProtonByteBuffer();
         compared.WriteBytes(value, 0, 31);

         Assert.IsTrue(buffer.CompareTo(compared) > 0);
      }

      #endregion

      #region Test for set by index

      [Test]
      public void TestSetByteAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(5, 5);

         for (int i = 0; i < buffer.Capacity; ++i)
         {
            buffer.SetByte(i, (sbyte)i);
         }

         for (int i = 0; i < buffer.Capacity; ++i)
         {
            Assert.AreEqual(i, buffer.GetByte(i));
         }

         try
         {
            buffer.SetByte(-1, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetByte(buffer.Capacity, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetBooleanAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(5, 5);

         for (int i = 0; i < buffer.Capacity; ++i)
         {
            if ((i % 2) == 0)
            {
               buffer.SetBoolean(i, false);
            }
            else
            {
               buffer.SetBoolean(i, true);
            }
         }

         for (int i = 0; i < buffer.Capacity; ++i)
         {
            if ((i % 2) == 0)
            {
               Assert.IsFalse(buffer.GetBoolean(i));
            }
            else
            {
               Assert.IsTrue(buffer.GetBoolean(i));
            }
         }

         try
         {
            buffer.SetBoolean(-1, true);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetBoolean(buffer.Capacity, false);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetShortAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(10, 10);

         for (short i = 0; i < buffer.Capacity / 2; i += 2)
         {
            buffer.SetShort(i, i);
         }

         for (short i = 0; i < buffer.Capacity / 2; i += 2)
         {
            Assert.AreEqual(i, buffer.GetShort(i));
         }

         try
         {
            buffer.SetShort(-1, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetShort(buffer.Capacity, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetIntAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(20, 20);

         for (int i = 0; i < buffer.Capacity / 4; i += 4)
         {
            buffer.SetInt(i, i);
         }

         for (int i = 0; i < buffer.Capacity / 4; i += 4)
         {
            Assert.AreEqual(i, buffer.GetInt(i));
         }

         try
         {
            buffer.SetInt(-1, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetInt(buffer.Capacity, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetLongAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(40, 40);

         for (long i = 0; i < buffer.Capacity / 8; i += 8)
         {
            buffer.SetLong(i, i);
         }

         for (long i = 0; i < buffer.Capacity / 8; i += 8)
         {
            Assert.AreEqual(i, buffer.GetLong((int)i));
         }

         try
         {
            buffer.SetLong(-1, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetLong(buffer.Capacity, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetFloatAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(8, 8);

         buffer.SetFloat(0, 1.5f);
         buffer.SetFloat(4, 45.2f);

         Assert.AreEqual(1.5f, buffer.GetFloat(0), 0.1);
         Assert.AreEqual(45.2f, buffer.GetFloat(4), 0.1);

         try
         {
            buffer.SetFloat(-1, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetFloat(buffer.Capacity, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetDoubleAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(16, 16);

         buffer.SetDouble(0, 1.5);
         buffer.SetDouble(8, 45.2);

         Assert.AreEqual(1.5, buffer.GetDouble(0), 0.1);
         Assert.AreEqual(45.2, buffer.GetDouble(8), 0.1);

         try
         {
            buffer.SetDouble(-1, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetDouble(buffer.Capacity, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      [Test]
      public void TestSetCharAtIndex()
      {
         IProtonBuffer buffer = AllocateBuffer(8, 8);

         buffer.SetChar(0, (char)65);
         buffer.SetChar(4, (char)66);

         Assert.AreEqual('A', buffer.GetChar(0));
         Assert.AreEqual('B', buffer.GetChar(4));

         try
         {
            buffer.SetDouble(-1, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.SetDouble(buffer.Capacity, 0);
            Assert.Fail("should throw an ArgumentOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      #endregion

      #region Miscellaneous buffer access stress tests

      [Test]
      public void TestRandomUnsignedByteAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         for (int i = 0; i < buffer.Capacity; i++)
         {
            byte value = (byte)random.Next();
            buffer.SetUnsignedByte(i, value);
         }

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity; i++)
         {
            byte value = (byte)random.Next();
            Assert.AreEqual(value, buffer.GetUnsignedByte(i));
         }
      }

      [Test]
      public void TestRandomShortAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         for (int i = 0; i < buffer.Capacity - 1; i += 2)
         {
            short value = (short)random.Next();
            buffer.SetShort(i, value);
         }

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity - 1; i += 2)
         {
            short value = (short)random.Next();
            Assert.AreEqual(value, buffer.GetShort(i));
         }
      }

      [Test]
      public void TestRandomIntAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         for (int i = 0; i < buffer.Capacity - 3; i += 4)
         {
            int value = random.Next();
            buffer.SetInt(i, value);
         }

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity - 3; i += 4)
         {
            int value = random.Next();
            Assert.AreEqual(value, buffer.GetInt(i));
         }
      }

      [Test]
      public void TestRandomLongAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         for (int i = 0; i < buffer.Capacity - 7; i += 8)
         {
            long value = random.Next();
            buffer.SetLong(i, value);
         }

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity - 7; i += 8)
         {
            long value = random.Next();
            Assert.AreEqual(value, buffer.GetLong(i));
         }
      }

      [Test]
      public void TestRandomFloatAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         for (int i = 0; i < buffer.Capacity - 7; i += 8)
         {
            float value = random.Next();
            buffer.SetFloat(i, value);
         }

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity - 7; i += 8)
         {
            float expected = random.Next();
            float actual = buffer.GetFloat(i);
            Assert.AreEqual(expected, actual, 0.01);
         }
      }

      [Test]
      public void TestRandomDoubleAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         for (int i = 0; i < buffer.Capacity - 7; i += 8)
         {
            double value = random.NextDouble();
            buffer.SetDouble(i, value);
         }

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity - 7; i += 8)
         {
            double expected = random.NextDouble();
            double actual = buffer.GetDouble(i);
            Assert.AreEqual(expected, actual, 0.01);
         }
      }

      [Test]
      public void TestSequentialByteAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         buffer.WriteOffset = 0;
         for (int i = 0; i < buffer.Capacity; i++)
         {
            sbyte value = (sbyte)random.Next();
            Assert.AreEqual(i, buffer.WriteOffset);
            Assert.IsTrue(buffer.IsWritable);
            buffer.WriteByte(value);
         }

         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsWritable);

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity; i++)
         {
            sbyte value = (sbyte)random.Next();
            Assert.AreEqual(i, buffer.ReadOffset);
            Assert.IsTrue(buffer.IsReadable);
            Assert.AreEqual(value, buffer.ReadByte());
         }

         Assert.AreEqual(buffer.Capacity, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsReadable);
         Assert.IsFalse(buffer.IsWritable);
      }

      [Test]
      public void TestSequentialShortAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         buffer.WriteOffset = 0;
         for (int i = 0; i < buffer.Capacity; i += 2)
         {
            short value = (short)random.Next();
            Assert.AreEqual(i, buffer.WriteOffset);
            Assert.IsTrue(buffer.IsWritable);
            buffer.WriteShort(value);
         }

         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsWritable);

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity; i += 2)
         {
            short value = (short)random.Next();
            Assert.AreEqual(i, buffer.ReadOffset);
            Assert.IsTrue(buffer.IsReadable);
            Assert.AreEqual(value, buffer.ReadShort());
         }

         Assert.AreEqual(buffer.Capacity, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsReadable);
         Assert.IsFalse(buffer.IsWritable);
      }

      [Test]
      public void TestSequentialIntAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         buffer.WriteOffset = 0;
         for (int i = 0; i < buffer.Capacity; i += 4)
         {
            int value = random.Next();
            Assert.AreEqual(i, buffer.WriteOffset);
            Assert.IsTrue(buffer.IsWritable);
            buffer.WriteInt(value);
         }

         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsWritable);

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity; i += 4)
         {
            int value = random.Next();
            Assert.AreEqual(i, buffer.ReadOffset);
            Assert.IsTrue(buffer.IsReadable);
            Assert.AreEqual(value, buffer.ReadInt());
         }

         Assert.AreEqual(buffer.Capacity, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsReadable);
         Assert.IsFalse(buffer.IsWritable);
      }

      [Test]
      public void TestSequentialLongAccess()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         buffer.WriteOffset = 0;
         for (int i = 0; i < buffer.Capacity; i += 8)
         {
            long value = BitConverter.DoubleToInt64Bits(random.NextDouble());
            Assert.AreEqual(i, buffer.WriteOffset);
            Assert.IsTrue(buffer.IsWritable);
            buffer.WriteLong(value);
         }

         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsWritable);

         random = new Random(seed);

         for (int i = 0; i < buffer.Capacity; i += 8)
         {
            long value = BitConverter.DoubleToInt64Bits(random.NextDouble());
            Assert.AreEqual(i, buffer.ReadOffset);
            Assert.IsTrue(buffer.IsReadable);
            Assert.AreEqual(value, buffer.ReadLong());
         }

         Assert.AreEqual(buffer.Capacity, buffer.ReadOffset);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);
         Assert.IsFalse(buffer.IsReadable);
         Assert.IsFalse(buffer.IsWritable);
      }

      [Test]
      public void TestByteArrayTransfer()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         byte[] value = new byte[BlockSize * 2];
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(value);
            buffer.WriteOffset = i;
            buffer.WriteBytes(value, random.Next(BlockSize), BlockSize);
         }

         random = new Random(seed);

         byte[] expectedValue = new byte[BlockSize * 2];
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(expectedValue);
            int Lookupfset = random.Next(BlockSize);
            buffer.CopyInto(i, value, Lookupfset, BlockSize);
            for (int j = Lookupfset; j < Lookupfset + BlockSize; j++)
            {
               Assert.AreEqual(expectedValue[j], value[j]);
            }
         }
      }

      [Test]
      public void TestRandomByteArrayTransfer1()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         byte[] value = new byte[BlockSize];
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(value);
            buffer.WriteOffset = i;
            buffer.WriteBytes(value);
         }

         random = new Random(seed);

         byte[] expectedValueContent = new byte[BlockSize];
         IProtonBuffer expectedValue = new ProtonByteBuffer(expectedValueContent);
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(expectedValueContent);
            buffer.CopyInto(i, value, 0, value.LongLength);
            for (int j = 0; j < BlockSize; j++)
            {
               Assert.AreEqual(expectedValue.GetUnsignedByte(j), value[j]);
            }
         }
      }

      [Test]
      public void TestRandomByteArrayTransfer2()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         byte[] value = new byte[BlockSize * 2];
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(value);
            buffer.WriteOffset = i;
            buffer.WriteBytes(value, random.Next(BlockSize), BlockSize);
         }

         random = new Random(seed);

         byte[] expectedValueContent = new byte[BlockSize * 2];
         IProtonBuffer expectedValue = new ProtonByteBuffer(expectedValueContent);
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(expectedValueContent);
            int Lookupfset = random.Next(BlockSize);
            buffer.CopyInto(i, value, Lookupfset, BlockSize);
            for (int j = Lookupfset; j < Lookupfset + BlockSize; j++)
            {
               Assert.AreEqual(expectedValue.GetUnsignedByte(j), value[j]);
            }
         }
      }

      [Test]
      public void TestRandomProtonBufferTransfer1()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);
         byte[] valueContent = new byte[BlockSize];
         IProtonBuffer value = new ProtonByteBuffer(BlockSize, BlockSize);

         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(valueContent);
            value.Reset();
            value.WriteBytes(valueContent);
            buffer.WriteOffset = i;
            buffer.WriteBytes(value);
            Assert.AreEqual(BlockSize, value.ReadOffset);
            Assert.AreEqual(BlockSize, value.WriteOffset);
         }

         random = new Random(seed);

         byte[] expectedValueContent = new byte[BlockSize];
         IProtonBuffer expectedValue = new ProtonByteBuffer(expectedValueContent);
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(expectedValueContent);
            value.Reset();
            buffer.CopyInto(i, value, 0, BlockSize);
            value.WriteOffset = BlockSize;
            Assert.AreEqual(0, value.ReadOffset);
            Assert.AreEqual(BlockSize, value.WriteOffset);
            for (int j = 0; j < BlockSize; j++)
            {
               Assert.AreEqual(expectedValue.GetByte(j), value.GetByte(j));
            }
         }
      }

      [Test]
      public void TestSequentialByteArrayTransfer1()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         byte[] value = new byte[BlockSize];
         buffer.WriteOffset = 0;
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(value);
            Assert.AreEqual(0, buffer.ReadOffset);
            Assert.AreEqual(i, buffer.WriteOffset);
            buffer.WriteBytes(value);
         }

         random = new Random(seed);

         byte[] expectedValue = new byte[BlockSize];
         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(expectedValue);
            Assert.AreEqual(i, buffer.ReadOffset);
            Assert.AreEqual(LargeCapacity, buffer.WriteOffset);
            buffer.CopyInto(i, value, 0, value.LongLength);
            buffer.ReadOffset += BlockSize;
            for (int j = 0; j < BlockSize; j++)
            {
               Assert.AreEqual(expectedValue[j], value[j]);
            }
         }
      }

      [Test]
      public void TestSequentialProtonBufferTransfer1()
      {
         IProtonBuffer buffer = AllocateBuffer(LargeCapacity);

         int SIZE = BlockSize * 2;
         byte[] valueContent = new byte[SIZE];
         IProtonBuffer value = new ProtonByteBuffer(SIZE, SIZE);

         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(valueContent);
            value.Reset().WriteBytes(valueContent);
            Assert.AreEqual(0, buffer.ReadOffset);
            Assert.AreEqual(i, buffer.WriteOffset);
            int randomOffset = random.Next(BlockSize);
            value.ReadOffset = randomOffset;
            value.WriteOffset = value.ReadOffset + BlockSize;
            buffer.WriteBytes(value);
            Assert.AreEqual(randomOffset + BlockSize, value.ReadOffset);
            Assert.AreEqual(randomOffset + BlockSize, value.WriteOffset);
         }

         random = new Random(seed);
         value.ReadOffset = 0;
         value.WriteOffset = valueContent.LongLength;
         byte[] expectedValueContent = new byte[BlockSize * 2];
         IProtonBuffer expectedValue = new ProtonByteBuffer(expectedValueContent);

         for (int i = 0; i < buffer.Capacity - BlockSize + 1; i += BlockSize)
         {
            random.NextBytes(expectedValueContent);
            int Lookupfset = random.Next(BlockSize);
            Assert.AreEqual(i, buffer.ReadOffset);
            Assert.AreEqual(LargeCapacity, buffer.WriteOffset);
            buffer.CopyInto(i, value, Lookupfset, BlockSize);
            for (int j = Lookupfset; j < Lookupfset + BlockSize; j++)
            {
               Assert.AreEqual(expectedValue.GetByte(j), value.GetByte(j));
            }
            Assert.AreEqual(0, value.ReadOffset);
            Assert.AreEqual(valueContent.LongLength, value.WriteOffset);
            buffer.ReadOffset += BlockSize;
         }
      }

      #endregion

      #region Test For buffer split behavior

      [Test]
      public void TestReadSplit()
      {
         DoTestReadSplit(3, 3, 1, 1);
      }

      [Test]
      public void TestReadSplitWriteOffsetLessThanCapacity()
      {
         DoTestReadSplit(5, 4, 2, 1);
      }

      [Test]
      public void TestReadSplitOffsetZero()
      {
         DoTestReadSplit(3, 3, 1, 0);
      }

      [Test]
      public void TestReadSplitOffsetToWriteOffset()
      {
         DoTestReadSplit(3, 3, 1, 2);
      }

      private void DoTestReadSplit(int capacity, int writeUnsignedBytes, int readBytes, int offset)
      {
         IProtonBuffer buffer = AllocateBuffer(capacity);
         WriteRandomBytes(buffer, writeUnsignedBytes);
         Assert.AreEqual(writeUnsignedBytes, buffer.WriteOffset);

         for (int i = 0; i < readBytes; i++)
         {
            buffer.ReadByte();
         }

         Assert.AreEqual(readBytes, buffer.ReadOffset);

         IProtonBuffer split = buffer.ReadSplit(offset);
         Assert.AreEqual(readBytes + offset, split.Capacity);
         Assert.AreEqual(split.Capacity, split.WriteOffset);
         Assert.AreEqual(readBytes, split.ReadOffset);

         Assert.AreEqual(capacity - split.Capacity, buffer.Capacity);
         Assert.AreEqual(writeUnsignedBytes - split.Capacity, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);
      }

      [Test]
      public void TestWriteSplit()
      {
         DoTestWriteSplit(5, 3, 1, 1);
      }

      [Test]
      public void TestWriteSplitWriteOffsetLessThanCapacity()
      {
         DoTestWriteSplit(5, 2, 2, 2);
      }

      [Test]
      public void TestWriteSplitOffsetZero()
      {
         DoTestWriteSplit(3, 3, 1, 0);
      }

      [Test]
      public void TestWriteSplitOffsetToCapacity()
      {
         DoTestWriteSplit(3, 1, 1, 2);
      }

      private void DoTestWriteSplit(int capacity, int writeUnsignedBytes, int readBytes, int offset)
      {
         IProtonBuffer buffer = AllocateBuffer(capacity);
         WriteRandomBytes(buffer, writeUnsignedBytes);
         Assert.AreEqual(writeUnsignedBytes, buffer.WriteOffset);

         for (int i = 0; i < readBytes; i++)
         {
            buffer.ReadByte();
         }

         Assert.AreEqual(readBytes, buffer.ReadOffset);

         IProtonBuffer split = buffer.WriteSplit(offset);
         Assert.AreEqual(writeUnsignedBytes + offset, split.Capacity);
         Assert.AreEqual(writeUnsignedBytes, split.WriteOffset);
         Assert.AreEqual(readBytes, split.ReadOffset);

         Assert.AreEqual(capacity - split.Capacity, buffer.Capacity);
         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);
      }

      [Test]
      public void TestSplitPostFull()
      {
         DoTestSplitPostFullOrRead(false);
      }

      [Test]
      public void TestSplitPostFullAndRead()
      {
         DoTestSplitPostFullOrRead(true);
      }

      private void DoTestSplitPostFullOrRead(bool read)
      {
         const int capacity = 3;
         IProtonBuffer buffer = AllocateBuffer(capacity);
         WriteRandomBytes(buffer, capacity);
         Assert.AreEqual(buffer.Capacity, buffer.WriteOffset);

         if (read)
         {
            for (int i = 0; i < capacity; i++)
            {
               buffer.ReadByte();
            }
         }

         Assert.AreEqual(read ? buffer.Capacity : 0, buffer.ReadOffset);

         IProtonBuffer split = buffer.Split();
         Assert.AreEqual(capacity, split.Capacity);
         Assert.AreEqual(split.Capacity, split.WriteOffset);
         Assert.AreEqual(read ? split.Capacity : 0, split.ReadOffset);

         Assert.AreEqual(0, buffer.Capacity);
         Assert.AreEqual(0, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);
      }

      [Test]
      public void TestSplitWithNegativeOffsetMustThrow()
      {
         IProtonBuffer buffer = AllocateBuffer(8);
         Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Split(-1));
      }

      [Test]
      public void TestSplitWithOversizedOffsetMustThrow()
      {
         IProtonBuffer buffer = AllocateBuffer(8);
         Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Split(9));
      }

      [Test]
      public void TestSplitOnOffsetMustTruncateGreaterOffsets()
      {
         IProtonBuffer buffer = AllocateBuffer(8);
         buffer.WriteInt(0x01020304);
         buffer.WriteByte((sbyte)0x05L);
         buffer.ReadInt();

         IProtonBuffer split = buffer.Split(2);
         Assert.AreEqual(buffer.ReadOffset, 2);
         Assert.AreEqual(buffer.WriteOffset, 3);

         Assert.AreEqual(split.ReadOffset, 2);
         Assert.AreEqual(split.WriteOffset, 2);
      }

      [Test]
      public void TestSplitOnOffsetMustExtendLesserOffsets()
      {
         IProtonBuffer buffer = AllocateBuffer(8);
         buffer.WriteInt(0x01020304);
         buffer.ReadInt();
         IProtonBuffer split = buffer.Split(6);

         Assert.AreEqual(buffer.ReadOffset, 0);
         Assert.AreEqual(buffer.WriteOffset, 0);

         Assert.AreEqual(split.ReadOffset, 4);
         Assert.AreEqual(split.WriteOffset, 4);
      }

      [Test]
      public void TestSplitPartMustContainFirstHalfOfBuffer()
      {
         IProtonBuffer buffer = AllocateBuffer(16);
         buffer.WriteLong(0x0102030405060708L);
         Assert.AreEqual(buffer.ReadByte(), 0x01);
         IProtonBuffer split = buffer.Split();
         // Original buffer:
         Assert.AreEqual(buffer.Capacity, 8);
         Assert.AreEqual(buffer.ReadOffset, 0);
         Assert.AreEqual(buffer.WriteOffset, 0);
         Assert.AreEqual(buffer.ReadableBytes, 0);
         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadByte());

         // Split part:
         Assert.AreEqual(split.Capacity, 8);
         Assert.AreEqual(split.ReadOffset, 1);
         Assert.AreEqual(split.WriteOffset, 8);
         Assert.AreEqual(split.ReadableBytes, 7);
         Assert.AreEqual(split.ReadByte(), 0x02);
         Assert.AreEqual(split.ReadInt(), 0x03040506);
         Assert.AreEqual(split.ReadByte(), 0x07);
         Assert.AreEqual(split.ReadByte(), 0x08);
         Assert.Throws<IndexOutOfRangeException>(() => split.ReadByte());

         // Re-test original split end to see if it was unaffected
         Assert.AreEqual(buffer.Capacity, 8);
         Assert.AreEqual(buffer.ReadOffset, 0);
         Assert.AreEqual(buffer.WriteOffset, 0);
         Assert.AreEqual(buffer.ReadableBytes, 0);
         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadByte());
      }

      [Test]
      public void TestMustBePossibleToSplitMoreThanOnce()
      {
         IProtonBuffer buffer = AllocateBuffer(16);
         buffer.WriteLong(0x0102030405060708L);
         IProtonBuffer a = buffer.Split();
         a.WriteOffset = 4;

         IProtonBuffer b = a.Split();
         Assert.AreEqual(0x01020304, b.ReadInt());
         a.WriteOffset = 4;
         Assert.AreEqual(0x05060708, a.ReadInt());
         Assert.Throws<IndexOutOfRangeException>(() => b.ReadByte());
         Assert.Throws<IndexOutOfRangeException>(() => a.ReadByte());
         buffer.WriteUnsignedLong(0xA1A2A3A4A5A6A7A8UL);
         buffer.WriteOffset = 4;

         IProtonBuffer c = buffer.Split();
         Assert.AreEqual(0xA1A2A3A4u, c.ReadUnsignedInt());
         buffer.WriteOffset = 4;
         Assert.AreEqual(0xA5A6A7A8u, buffer.ReadUnsignedInt());
         Assert.Throws<IndexOutOfRangeException>(() => c.ReadByte());
         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadByte());
      }

      [Test]
      public void TestMustBePossibleToSplitCopies()
      {
         IProtonBuffer buffer = AllocateBuffer(16);
         buffer.WriteLong(0x0102030405060708L);

         IProtonBuffer copy = buffer.Copy();
         IProtonBuffer split = copy.Split(4);
         split.Reset().EnsureWritable(sizeof(ulong));
         copy.Reset().EnsureWritable(sizeof(ulong));

         Assert.AreEqual(split.Capacity, sizeof(ulong));
         Assert.AreEqual(copy.Capacity, sizeof(ulong));
         Assert.AreEqual(0x01020304, split.GetInt(0));
         Assert.AreEqual(0x05060708, copy.GetInt(0));
      }

      [Test]
      public void TestEnsureWritableOnSplitBuffers()
      {
         IProtonBuffer buffer = AllocateBuffer(8);
         buffer.WriteUnsignedLong(0x0102030405060708UL);
         IProtonBuffer a = buffer.Split();
         Assert.AreEqual(0x0102030405060708UL, a.ReadUnsignedLong());

         a.EnsureWritable(8);
         a.WriteUnsignedLong(0xA1A2A3A4A5A6A7A8UL);
         Assert.AreEqual(0xA1A2A3A4A5A6A7A8UL, a.ReadUnsignedLong());

         buffer.EnsureWritable(8);
         buffer.WriteUnsignedLong(0xA1A2A3A4A5A6A7A8UL);
         Assert.AreEqual(0xA1A2A3A4A5A6A7A8UL, buffer.ReadUnsignedLong());
      }

      [Test]
      public void TestEnsureWritableOnSplitBuffersWithOddOffsets()
      {
         IProtonBuffer buffer = AllocateBuffer(10);
         buffer.WriteUnsignedLong(0x0102030405060708L);
         buffer.WriteByte(0x09);
         buffer.ReadByte();
         IProtonBuffer a = buffer.Split();

         Assert.AreEqual(0x0203040506070809UL, a.ReadUnsignedLong());
         a.EnsureWritable(8);
         a.WriteUnsignedLong(0xA1A2A3A4A5A6A7A8UL);
         Assert.AreEqual(0xA1A2A3A4A5A6A7A8UL, a.ReadUnsignedLong());

         buffer.EnsureWritable(8);
         buffer.WriteUnsignedLong(0xA1A2A3A4A5A6A7A8UL);
         Assert.AreEqual(0xA1A2A3A4A5A6A7A8UL, buffer.ReadUnsignedLong());
      }

      #endregion

      #region Tests need to define these allocation methods

      /// <summary>
      /// A check that should return true if the buffer type under test support capacity alterations.
      /// </summary>
      /// <returns>true if the buffer type under test support capacity alterations.</returns>
      protected virtual bool CanBufferCapacityBeChanged()
      {
         return true;
      }

      /// <summary>
      /// IProtonBuffer allocated with defaults for capacity and max-capacity.
      /// </summary>
      /// <returns>IProtonBuffer allocated with defaults for capacity and max-capacity.</returns>
      protected virtual IProtonBuffer AllocateDefaultBuffer()
      {
         return AllocateBuffer(DefaultCapacity);
      }

      /// <summary>
      /// IProtonBuffer allocated with the default max capacity but with the given initial cpacity.
      /// </summary>
      /// <param name="initialCapacity"></param>
      /// <returns>IProtonBuffer allocated with an initial capacity and a default max-capacity.</returns>
      protected abstract IProtonBuffer AllocateBuffer(int initialCapacity);

      /// <summary>
      /// IProtonBuffer allocated with the given max capacity but with the given initial cpacity.
      /// </summary>
      /// <param name="initialCapacity"></param>
      /// <returns>IProtonBuffer allocated with an initial capacity and a default max-capacity.</returns>
      protected abstract IProtonBuffer AllocateBuffer(int initialCapacity, int maxCapacity);

      /// <summary>
      /// IProtonBuffer that wraps the given buffer.
      /// </summary>
      /// <param name="array"></param>
      /// <returns>IProtonBuffer that wraps the given buffer.</returns>
      protected abstract IProtonBuffer WrapBuffer(byte[] array);

      #endregion

      #region Test utility methods accessable to all subclasses

      protected static void WriteRandomBytes(IProtonBuffer buf, int length)
      {
         byte[] data = new byte[length];
         Random random = new Random(Environment.TickCount);
         random.NextBytes(data);
         buf.WriteBytes(data);
      }

      #endregion
   }
}