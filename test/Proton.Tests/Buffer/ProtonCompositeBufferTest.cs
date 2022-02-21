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
using System.Linq;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Buffer
{
   [TestFixture]
   public class ProtonCompositeBufferTest : ProtonAbstractByteBufferTests
   {
      #region Composite Buffer create tests

      [Test]
      public void TestCreateEmptyCompositeBuffer()
      {
         IProtonCompositeBuffer buffer = IProtonCompositeBuffer.Compose();

         Assert.IsNotNull(buffer);
         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ComponentCount);
      }

      [Test]
      public void TestCreateFiltersEmptyBuffer()
      {
         IProtonBuffer target = ProtonByteBufferAllocator.Instance.Allocate();
         IProtonCompositeBuffer buffer = IProtonCompositeBuffer.Compose();

         Assert.IsNotNull(buffer);
         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ComponentCount);
      }

      [Test]
      public void TestCreateFromSingleBuffer()
      {
         IProtonBuffer target = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });
         IProtonCompositeBuffer buffer = IProtonCompositeBuffer.Compose(ProtonByteBufferAllocator.Instance, target);

         Assert.IsNotNull(buffer);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(1, buffer.ComponentCount);
      }

      [Test]
      public void TestCreateFromSameBufferReferencesFails()
      {
         IProtonBuffer target = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });
         Assert.Throws<ArgumentException>(() =>
            IProtonCompositeBuffer.Compose(ProtonByteBufferAllocator.Instance, target, target));
      }

      [Test]
      public void TestCreateFromMultipleBuffers()
      {
         IProtonBuffer target1 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });
         IProtonBuffer target2 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 5, 6, 7, 8, 9 });
         IProtonBuffer target3 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 10, 11, 12, 13, 14 });

         IProtonCompositeBuffer buffer = IProtonCompositeBuffer.Compose(ProtonByteBufferAllocator.Instance, target1, target2, target3);

         Assert.IsNotNull(buffer);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(3, buffer.ComponentCount);
         Assert.AreEqual(3, buffer.ReadableComponentCount);
         Assert.AreEqual(0, buffer.WritableComponentCount);
      }

      [Test]
      public void TestCreateFromOneCompositeFromAnotherFlattensComponents()
      {
         IProtonBuffer target1 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });
         IProtonBuffer target2 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 5, 6, 7, 8, 9 });
         IProtonBuffer target3 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 10, 11, 12, 13, 14 });

         IProtonCompositeBuffer buffer1 = IProtonCompositeBuffer.Compose(ProtonByteBufferAllocator.Instance, target1, target2, target3);
         IProtonCompositeBuffer buffer2 = IProtonCompositeBuffer.Compose(ProtonByteBufferAllocator.Instance, buffer1);

         Assert.AreNotSame(buffer1, buffer2);
         Assert.AreEqual(0, buffer1.ComponentCount);
         Assert.AreEqual(3, buffer2.ComponentCount);
         Assert.AreEqual(3, buffer2.ReadableComponentCount);
         Assert.AreEqual(0, buffer2.WritableComponentCount);
      }

      #endregion

      #region Composite buffer read indexed tests

      [Test]
      public void TestManipulateReadIndexWithOneArrayAtCreateTime()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });
         DoTestManipulateReadIndexWithOneArrayAppended(
            IProtonCompositeBuffer.Compose(ProtonByteBufferAllocator.Instance, buffer));
      }

      [Test]
      public void TestManipulateReadIndexWithOneArrayAppended()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();
         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
         DoTestManipulateReadIndexWithOneArrayAppended(buffer);
      }

      private void DoTestManipulateReadIndexWithOneArrayAppended(IProtonCompositeBuffer buffer)
      {
         Assert.AreEqual(10, buffer.Capacity);
         Assert.AreEqual(10, buffer.WriteOffset);
         Assert.AreEqual(0, buffer.ReadOffset);

         buffer.ReadOffset = 5;
         Assert.AreEqual(5, buffer.ReadOffset);

         buffer.ReadOffset = 6;
         Assert.AreEqual(6, buffer.ReadOffset);

         buffer.ReadOffset = 10;
         Assert.AreEqual(10, buffer.ReadOffset);

         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadOffset = 11);
      }

      [Test]
      public void TestPositionEnforcesPreconditions()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         // test with nothing appended.
         try
         {
            buffer.ReadOffset = 2;
            Assert.Fail("Should throw a IndexOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.ReadOffset = -1;
            Assert.Fail("Should throw a IndexOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         // Test with something appended
         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 127 }));

         try
         {
            buffer.ReadOffset = 2;
            Assert.Fail("Should throw a IndexOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }

         try
         {
            buffer.ReadOffset = -1;
            Assert.Fail("Should throw a IndexOutOfRangeException");
         }
         catch (IndexOutOfRangeException) { }
      }

      #endregion

      #region Test reading from composite with multiple elements

      [Test]
      public void TestGetByteWithManyArraysWithOneElements()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 3 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 4 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 5 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 6 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 7 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 8 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 9 }));

         Assert.AreEqual(10, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0; i < 10; ++i)
         {
            Assert.AreEqual(i, buffer.ReadByte());
         }

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(10, buffer.ReadOffset);
         Assert.AreEqual(10, buffer.WriteOffset);

         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadByte());
      }

      [Test]
      public void TestGetByteWithManyArraysWithVariedElements()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 3, 4, 5 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 6 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 7, 8, 9 }));

         Assert.AreEqual(10, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(10, buffer.WriteOffset);

         for (int i = 0; i < 10; ++i)
         {
            Assert.AreEqual(i, buffer.ReadByte());
         }

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(10, buffer.ReadOffset);

         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadByte());
      }

      [Test]
      public void TestGetShortByteWithNothingAppended()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();
         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadShort());
      }

      [Test]
      public void TestGetShortWithTwoArraysContainingOneElement()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 8 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }));

         Assert.AreEqual(2, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(2048, buffer.ReadShort());

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(2, buffer.ReadOffset);

         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadShort());
      }

      [Test]
      public void TestGetIntWithTwoArraysContainingOneElement()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 0 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 8, 0 }));

         Assert.AreEqual(4, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(2048, buffer.ReadInt());

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(4, buffer.ReadOffset);

         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadInt());
      }

      [Test]
      public void TestGetLongWithTwoArraysContainingOneElement()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 0, 0, 0 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 0, 8, 0 }));

         Assert.AreEqual(8, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(2048, buffer.ReadLong());

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(8, buffer.ReadOffset);

         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadLong());
      }

      [Test]
      public void TestGetLongWithTwoArraysContainingOneElementWithUnEvenSplit()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 9, 8 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 255, 255, 0, 1, 2, 3 }));

         Assert.AreEqual(8, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         Assert.AreEqual(651051616836846083L, buffer.ReadLong());

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(8, buffer.ReadOffset);

         Assert.Throws<IndexOutOfRangeException>(() => buffer.ReadLong());
      }

      [Test]
      public void TestGetWritableBufferWithContentsInSeveralArrays()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         IProtonBuffer data1 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });
         IProtonBuffer data2 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 5, 6, 7, 8, 9 });
         IProtonBuffer data3 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 10, 11, 12 });

         long size = data1.ReadableBytes + data2.ReadableBytes + data3.ReadableBytes;

         buffer.Append(data1).Append(data2).Append(data3);

         Assert.AreEqual(size, buffer.WriteOffset);

         IProtonBuffer destination = ProtonByteBufferAllocator.Instance.Allocate(1, 1);

         for (int i = 0; i < size; i++)
         {
            Assert.AreEqual(buffer.ReadOffset, 0);
            IProtonBuffer self = buffer.CopyInto(i, destination, 0, 1);
            Assert.AreEqual(destination.GetByte(0), buffer.GetByte(i));
            Assert.AreSame(self, buffer);
            destination.WriteOffset = 0;
         }

         Assert.DoesNotThrow(() => buffer.ReadByte());
      }

      [Test]
      public void TestGetintWithContentsInMultipleArrays()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 }))
               .Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 5, 6, 7, 8, 9 }));

         for (int i = 0; i < buffer.Capacity; i++)
         {
            Assert.AreEqual(buffer.ReadOffset, i);
            Assert.AreEqual(buffer.ReadByte(), buffer.GetByte(i));
         }

         Assert.Throws<IndexOutOfRangeException>(() => buffer.GetByte(-1));
         Assert.Throws<IndexOutOfRangeException>(() => buffer.GetByte(buffer.WriteOffset));
      }

      [Test]
      public void TestSetAndGetShortAcrossMultipleArrays()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         const int NUM_ELEMENTS = 4;

         for (int i = 0; i < sizeof(ushort) * NUM_ELEMENTS; ++i)
         {
            buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }));
         }

         for (int i = 0, j = 1; i < buffer.ReadableBytes; i += sizeof(ushort), j++)
         {
            buffer.SetShort(i, (short)j);
         }

         Assert.AreEqual(sizeof(ushort) * NUM_ELEMENTS, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0, j = 1; i < buffer.ReadableBytes; i += sizeof(ushort), j++)
         {
            Assert.AreEqual(j, buffer.GetShort(i));
         }

         Assert.AreEqual(sizeof(ushort) * NUM_ELEMENTS, buffer.ReadableBytes);
         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(sizeof(ushort) * NUM_ELEMENTS, buffer.ReadableBytes);
      }

      [Test]
      public void TestSetAndGetIntegersAcrossMultipleArrays()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         const int NUM_ELEMENTS = 4;

         for (int i = 0; i < sizeof(uint) * NUM_ELEMENTS; ++i)
         {
            buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }));
         }

         for (int i = 0, j = 1; i < buffer.ReadableBytes; i += sizeof(uint), j++)
         {
            buffer.SetInt(i, (int)j);
         }

         Assert.AreEqual(sizeof(uint) * NUM_ELEMENTS, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0, j = 1; i < buffer.ReadableBytes; i += sizeof(uint), j++)
         {
            Assert.AreEqual(j, buffer.GetInt(i));
         }

         Assert.AreEqual(sizeof(uint) * NUM_ELEMENTS, buffer.ReadableBytes);
         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(sizeof(uint) * NUM_ELEMENTS, buffer.ReadableBytes);
      }

      [Test]
      public void TestSetAndGetLongsAcrossMultipleArrays()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         const int NUM_ELEMENTS = 4;

         for (int i = 0; i < sizeof(ulong) * NUM_ELEMENTS; ++i)
         {
            buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 }));
         }

         for (int i = 0, j = 1; i < buffer.ReadableBytes; i += sizeof(ulong), j++)
         {
            buffer.SetLong(i, (long)j);
         }

         Assert.AreEqual(sizeof(ulong) * NUM_ELEMENTS, buffer.ReadableBytes);
         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadOffset);

         for (int i = 0, j = 1; i < buffer.ReadableBytes; i += sizeof(ulong), j++)
         {
            Assert.AreEqual(j, buffer.GetLong(i));
         }

         Assert.AreEqual(sizeof(ulong) * NUM_ELEMENTS, buffer.ReadableBytes);
         Assert.AreEqual(0, buffer.ReadOffset);
         Assert.AreEqual(sizeof(ulong) * NUM_ELEMENTS, buffer.ReadableBytes);
      }

      #endregion

      #region Test appends to buffer

      [Test]
      public void TestAppendToBufferAtEndOfContentArray()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         byte[] source1 = new byte[] { 0, 1, 2, 3 };

         Assert.AreEqual(0, buffer.ComponentCount);

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source1));

         Assert.AreEqual(1, buffer.ComponentCount);

         buffer.ReadOffset = source1.Length;

         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadableBytes);

         byte[] source2 = new byte[] { 4, 5, 6, 7 };
         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source2));

         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(source2.Length, buffer.ReadableBytes);
         Assert.AreEqual(2, buffer.ComponentCount);
         Assert.AreEqual(source1.Length, buffer.ReadOffset);

         // Check each position in the array is read
         for (int i = 0; i < source2.Length; i++)
         {
            Assert.AreEqual(source1.Length + i, buffer.ReadByte());
         }
      }

      [Test]
      public void TestAppendToBufferAtEndOfContentList()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         byte[] source1 = new byte[] { 0, 1, 2, 3 };
         byte[] source2 = new byte[] { 4, 5, 6, 7 };

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source1));
         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source2));

         Assert.AreEqual(2, buffer.ComponentCount);

         buffer.ReadOffset = source1.Length + source2.Length;

         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ReadableBytes);

         byte[] source3 = new byte[] { 8, 9, 10, 11 };
         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source3));

         Assert.IsTrue(buffer.IsReadable);
         Assert.AreEqual(source3.Length, buffer.ReadableBytes);
         Assert.AreEqual(3, buffer.ComponentCount);
         Assert.AreEqual(source1.Length + source2.Length, buffer.ReadOffset);

         // Check each position in the array is read
         for (int i = 0; i < source3.Length; i++)
         {
            Assert.AreEqual(source1.Length + source2.Length + i, buffer.ReadByte());
         }
      }

      [Test]
      public void TestAppendToBufferAtWhenWriteIndexNotAtEnd()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         byte[] source1 = new byte[] { 0, 1, 2, 3 };
         byte[] source2 = new byte[] { 4, 5, 6, 7 };

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source1));

         Assert.AreEqual(source1.Length, buffer.WriteOffset);

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source2));

         Assert.AreEqual(source2.Length + source1.Length, buffer.WriteOffset);

         byte[] source3 = new byte[] { 8, 9, 10, 11 };

         buffer.WriteOffset = 2;
         Assert.AreEqual(2, buffer.WriteOffset);

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(source3).Reset());

         Assert.AreEqual(2, buffer.WriteOffset);
         Assert.AreEqual(3, buffer.ComponentCount);
         Assert.AreEqual(3, buffer.WritableComponentCount);
         Assert.AreEqual(1, buffer.ReadableComponentCount);
      }

      [Test]
      public void TestAppendNullBuffer()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         try
         {
            buffer.Append(null);
            Assert.Fail("Should not be able to add a null array");
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestAppendEmptyBuffer()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[0]));

         Assert.IsFalse(buffer.IsReadable);
         Assert.AreEqual(0, buffer.ComponentCount);
      }

      #endregion

      #region Hash code generation tests

      [Test]
      public void TestHashCodeNotFromIdentity()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         Assert.AreEqual(1, buffer.GetHashCode());

         byte[] data = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(data));

         int originalHashCode = buffer.GetHashCode();

         Assert.IsTrue(buffer.GetHashCode() != 1);
         Assert.AreEqual(originalHashCode, buffer.GetHashCode());

         _ = buffer.ReadByte();

         Assert.AreNotEqual(originalHashCode, buffer.GetHashCode());
      }

      [Test]
      public void TestHashCodeOnSameBackingBuffer()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer3 = new ProtonCompositeBuffer();

         byte[] data = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data));
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data));
         buffer3.Append(ProtonByteBufferAllocator.Instance.Wrap(data));

         Assert.AreEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
         Assert.AreEqual(buffer2.GetHashCode(), buffer3.GetHashCode());
         Assert.AreEqual(buffer3.GetHashCode(), buffer1.GetHashCode());
      }

      [Test]
      public void TestHashCodeOnDifferentBackingBuffer()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] data2 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data2));

         Assert.AreNotEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
      }

      [Test]
      public void TestHashCodeOnSplitBufferContentsNotSame()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] data2 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data2));
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data2))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data1));

         Assert.AreNotEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
      }

      [Test]
      public void TestHashCodeOnSplitBufferContentsSame()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] data2 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data2));
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data1))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data2));

         Assert.AreEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
      }

      [Test]
      public void TestHashCodeMatchesByteBufferWhenLimitSetGivesNoRemaining()
      {
         byte[] data = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data));
         buffer1.ReadOffset = buffer1.WriteOffset;

         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Wrap(data);
         buffer2.ReadOffset = buffer1.WriteOffset;

         Assert.AreEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
      }

      [Test]
      public void TestHashCodeMatchesByteBufferSingleArrayContents()
      {
         byte[] data = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data));

         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Wrap(data);

         Assert.AreEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
      }

      [Test]
      public void TestHashCodeMatchesByteBufferMultipleArrayContents()
      {
         byte[] data = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

         byte[] data1 = new byte[] { 9, 8, 7, 6, 5 };
         byte[] data2 = new byte[] { 4, 3, 2, 1, 0 };

         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));
         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data2));

         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Wrap(data);

         Assert.AreEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
      }

      [Test]
      public void TestHashCodeMatchesByteBufferMultipleArrayContentsWithRangeOfLimits()
      {
         byte[] data = new byte[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

         byte[] data1 = new byte[] { 10, 9 };
         byte[] data2 = new byte[] { 8, 7 };
         byte[] data3 = new byte[] { 6, 5, 4 };
         byte[] data4 = new byte[] { 3 };
         byte[] data5 = new byte[] { 2, 1, 0 };

         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data2))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data3))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data4))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data5));

         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Wrap(data);

         for (int i = 0; i < data.Length; ++i)
         {
            buffer1.WriteOffset = i;
            buffer2.WriteOffset = i;

            Assert.AreEqual(buffer1.GetHashCode(), buffer2.GetHashCode());
         }
      }

      #endregion

      #region Test for composite buffer equals

      [Test]
      public void TestEqualsOnSameBackingBuffer()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer3 = new ProtonCompositeBuffer();

         byte[] data = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data));
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data));
         buffer3.Append(ProtonByteBufferAllocator.Instance.Wrap(data));

         Assert.AreEqual(buffer1, buffer2);
         Assert.AreEqual(buffer2, buffer3);
         Assert.AreEqual(buffer3, buffer1);

         Assert.AreEqual(0, buffer1.ReadOffset);
         Assert.AreEqual(0, buffer2.ReadOffset);
         Assert.AreEqual(0, buffer3.ReadOffset);
      }

      [Test]
      public void TestEqualsOnDifferentBackingBuffer()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] data2 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data2));

         Assert.AreNotEqual(buffer1, buffer2);

         Assert.AreEqual(0, buffer1.ReadOffset);
         Assert.AreEqual(0, buffer2.ReadOffset);
      }

      [Test]
      public void TestEqualsWhenContentsInMultipleArraysNotSame()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 };
         byte[] data2 = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data2));
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data2))
                .Append(ProtonByteBufferAllocator.Instance.Wrap(data1));

         Assert.AreNotEqual(buffer1, buffer2);

         Assert.AreEqual(0, buffer1.ReadOffset);
         Assert.AreEqual(0, buffer2.ReadOffset);
      }

      [Test]
      public void TestEqualsWhenContentRemainingWithDifferentStartPositionsSame()
      {
         doEqualsWhenContentRemainingWithDifferentStartPositionsSameTestImpl(false);
      }

      [Test]
      public void TestEqualsWhenContentRemainingWithDifferentStartPositionsSameMultipleArrays()
      {
         doEqualsWhenContentRemainingWithDifferentStartPositionsSameTestImpl(true);
      }

      private void doEqualsWhenContentRemainingWithDifferentStartPositionsSameTestImpl(bool multipleArrays)
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 255, 0, 1, 2, 3, 4, 5 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));
         buffer1.ReadOffset = 2;

         // Offset wrapped buffer should behave same as buffer 1
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data2));
         buffer2.ReadOffset = 3;

         long oldWriteIndex1 = buffer1.WriteOffset;
         long oldWriteIndex2 = buffer2.WriteOffset;

         if (multipleArrays)
         {
            byte[] data3 = new byte[] { 5, 4, 3, 2, 1 };
            buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data2).Reset());
            buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data2).Reset());
         }

         Assert.AreEqual(buffer1, buffer2);

         Assert.AreEqual(2, buffer1.ReadOffset);
         Assert.AreEqual(3, buffer2.ReadOffset);
      }

      [Test]
      public void TestEqualsWhenContentRemainingIsSubsetOfSingleChunkInMultiArrayBufferSame()
      {
         ProtonCompositeBuffer buffer1 = new ProtonCompositeBuffer();
         ProtonCompositeBuffer buffer2 = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 255, 0, 1, 2, 3, 4, 5 };

         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));
         buffer1.ReadOffset = 2;

         // Offset the wrapped buffer which means these two should behave the same
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data2));
         buffer2.ReadOffset = 3;

         byte[] data3 = new byte[] { 5, 4, 3, 2, 1 };
         buffer1.Append(ProtonByteBufferAllocator.Instance.Wrap(data3).Reset());
         buffer2.Append(ProtonByteBufferAllocator.Instance.Wrap(data3).Reset());

         buffer1.WriteOffset = data1.Length;
         buffer2.WriteOffset = data2.Length;

         Assert.AreEqual(6, buffer1.ReadableBytes);
         Assert.AreEqual(6, buffer2.ReadableBytes);

         Assert.AreEqual(buffer1, buffer2);
         Assert.AreEqual(buffer2, buffer1);

         Assert.AreEqual(2, buffer1.ReadOffset);
         Assert.AreEqual(3, buffer2.ReadOffset);
      }

      #endregion

      #region Tests for string reads from buffer

      [Test]
      public void TestReadStringFromEmptyBuffer()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         Assert.AreEqual("", buffer.ToString(System.Text.Encoding.UTF8));
      }

      #endregion

      #region Tests for composite buffer Append other composites API

      [Test]
      public void TestAppendTwoByteBackedBuffers()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();
         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 255, 0, 1, 2, 3, 4, 5 };

         // For Equality checks
         byte[] dataAll = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5, 255, 255, 255, 0, 1, 2, 3, 4, 5 };
         IProtonBuffer bufferAll = ProtonByteBufferAllocator.Instance.Wrap(dataAll);
         Assert.AreEqual(bufferAll.ReadableBytes, data1.Length + data2.Length);

         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(data1).Reset());
         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(data2).Reset());

         Assert.AreEqual(buffer.WritableBytes, data1.Length + data2.Length);
         Assert.AreNotEqual(buffer, bufferAll);

         buffer.WriteOffset += buffer.WritableBytes;

         Assert.AreEqual(buffer, bufferAll);
      }

      [Test]
      public void TestAppendRejectsReadGapedBuffer()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         IProtonBuffer buffer1 = ProtonByteBufferAllocator.Instance.Wrap(data1);
         buffer1.WriteOffset += buffer1.WritableBytes;

         byte[] data2 = new byte[] { 255, 255, 255, 0, 1, 2, 3, 4, 5 };
         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Wrap(data2);
         buffer2.WriteOffset += buffer2.WritableBytes;
         buffer2.ReadOffset += 1;

         buffer.Append(buffer1);

         Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Append(buffer2));
      }

      [Test]
      public void TestAppendRejectsWriteGapedBuffer()
      {
         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();

         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         IProtonBuffer buffer1 = ProtonByteBufferAllocator.Instance.Wrap(data1);
         buffer1.WriteOffset -= 1;

         byte[] data2 = new byte[] { 255, 255, 255, 0, 1, 2, 3, 4, 5 };
         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Wrap(data2);

         buffer.Append(buffer1);

         Assert.Throws<ArgumentOutOfRangeException>(() => buffer.Append(buffer2));
      }

      [Test]
      public void TestAppendOneCompositeBufferToAnotherEmptyComposite()
      {
         ProtonCompositeBuffer appendTo = new ProtonCompositeBuffer();
         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 255, 0, 1, 2, 3, 4, 5 };
         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data1).Reset());
         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data2).Reset());

         // For Equality checks
         byte[] dataAll = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5, 255, 255, 255, 0, 1, 2, 3, 4, 5 };
         IProtonBuffer bufferAll = ProtonByteBufferAllocator.Instance.Wrap(dataAll);
         Assert.AreEqual(bufferAll.ReadableBytes, data1.Length + data2.Length);

         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();
         buffer.Append(appendTo);

         Assert.AreEqual(buffer.WritableBytes, data1.Length + data2.Length);
         Assert.AreNotEqual(buffer, bufferAll);

         buffer.WriteOffset += buffer.WritableBytes;

         Assert.AreEqual(buffer, bufferAll);
         Assert.AreEqual(2, buffer.DecomposeBuffer().Count());
      }

      [Test]
      public void TestAppendOneCompositeBufferToAnotherNonEmptyComposite()
      {
         ProtonCompositeBuffer appendTo = new ProtonCompositeBuffer();
         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 255, 0, 1, 2, 3, 4, 5 };
         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data1).Reset());
         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data2).Reset());

         // For Equality checks
         byte[] dataAll = new byte[] { 9, 8, 255, 255, 0, 1, 2, 3, 4, 5, 255, 255, 255, 0, 1, 2, 3, 4, 5 };
         IProtonBuffer bufferAll = ProtonByteBufferAllocator.Instance.Wrap(dataAll);
         Assert.AreEqual(bufferAll.ReadableBytes, data1.Length + data2.Length + 2);

         ProtonCompositeBuffer buffer = new ProtonCompositeBuffer();
         buffer.Append(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 9, 8 }).Reset());
         buffer.Append(appendTo);

         Assert.AreEqual(buffer.WritableBytes, data1.Length + data2.Length + 2);
         Assert.AreNotEqual(buffer, bufferAll);

         buffer.WriteOffset += buffer.WritableBytes;

         Assert.AreEqual(buffer, bufferAll);
         Assert.AreEqual(3, buffer.DecomposeBuffer().Count());
      }

      [Test]
      public void TestCompactReadBufferAndAppendAnother()
      {
         ProtonCompositeBuffer appendTo = new ProtonCompositeBuffer();
         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };

         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));

         Assert.AreEqual(255, appendTo.ReadUnsignedByte());
         Assert.AreEqual(255, appendTo.ReadUnsignedByte());

         appendTo.ReadOffset = data1.LongLength;

         Assert.AreEqual(1, appendTo.ComponentCount);
         Assert.AreEqual(0, appendTo.ReadableBytes);
         Assert.AreEqual(0, appendTo.WritableBytes);

         appendTo.Compact();

         Assert.AreEqual(1, appendTo.ComponentCount);
         Assert.AreEqual(0, appendTo.ReadableBytes);
         Assert.AreEqual(data1.Length, appendTo.WritableBytes);

         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data2).Reset());

         Assert.AreEqual(2, appendTo.ComponentCount);
         Assert.AreEqual(0, appendTo.ReadableBytes);
         Assert.AreEqual(data1.Length + data2.Length, appendTo.WritableBytes);
      }

      [Test]
      public void TestSplitReadBufferAndAppendAnother()
      {
         ProtonCompositeBuffer appendTo = new ProtonCompositeBuffer();
         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };

         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));

         Assert.AreEqual(255, appendTo.ReadUnsignedByte());
         Assert.AreEqual(255, appendTo.ReadUnsignedByte());

         appendTo.ReadOffset = data1.LongLength;

         Assert.AreEqual(1, appendTo.ComponentCount);
         Assert.AreEqual(0, appendTo.ReadableBytes);
         Assert.AreEqual(0, appendTo.WritableBytes);

         appendTo.Split();

         Assert.AreEqual(0, appendTo.ComponentCount);
         Assert.AreEqual(0, appendTo.ReadableBytes);
         Assert.AreEqual(0, appendTo.WritableBytes);

         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data2).Reset());

         Assert.AreEqual(1, appendTo.ComponentCount);
         Assert.AreEqual(0, appendTo.ReadableBytes);
         Assert.AreEqual(data2.Length, appendTo.WritableBytes);
      }

      #endregion

      #region Tests buffer reclaim

      [Test]
      public void TestReclaimReadBuffer()
      {
         ProtonCompositeBuffer appendTo = new ProtonCompositeBuffer();
         byte[] data1 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };
         byte[] data2 = new byte[] { 255, 255, 0, 1, 2, 3, 4, 5 };

         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data1));
         appendTo.Append(ProtonByteBufferAllocator.Instance.Wrap(data2));

         Assert.AreEqual(2, appendTo.ComponentCount);
         appendTo.Reclaim();
         Assert.AreEqual(2, appendTo.ComponentCount);

         appendTo.ReadOffset = data1.LongLength - 1;
         appendTo.Reclaim();
         Assert.AreEqual(2, appendTo.ComponentCount);

         appendTo.ReadOffset = data1.LongLength;
         appendTo.Reclaim();
         Assert.AreEqual(1, appendTo.ComponentCount);

         appendTo.ReadOffset = data2.LongLength;
         appendTo.Reclaim();
         Assert.AreEqual(0, appendTo.ComponentCount);

         appendTo.EnsureWritable(1);
         appendTo.WriteBoolean(true);
         Assert.IsTrue(appendTo.ReadBoolean());
      }

      #endregion

      #region The Abstract methods needed for the base buffers tests.

      protected override bool CanBufferCapacityBeChanged()
      {
         return true; // Cannot resize the composite at the moment
      }

      protected override IProtonBuffer AllocateBuffer(int initialCapacity)
      {
         return new ProtonCompositeBuffer().Append(ProtonByteBufferAllocator.Instance.Allocate(initialCapacity));
      }

      protected override IProtonBuffer AllocateBuffer(int initialCapacity, int maxCapacity)
      {
         return new ProtonCompositeBuffer(maxCapacity).Append(ProtonByteBufferAllocator.Instance.Allocate(initialCapacity));
      }

      protected override IProtonBuffer WrapBuffer(byte[] array)
      {
         return new ProtonCompositeBuffer().Append(ProtonByteBufferAllocator.Instance.Wrap(array));
      }

      #endregion
   }
}