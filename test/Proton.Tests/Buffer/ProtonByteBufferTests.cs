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
   [TestFixture]
   public class ProtonByteBufferTests : ProtonAbstractByteBufferTests
   {
      [SetUp]
      public override void Setup()
      {
         base.Setup();
      }

      #region Test Buffer creation

      [Test]
      public void TestDefaultConstructor()
      {
         IProtonBuffer buffer = new ProtonByteBuffer();

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);

         buffer.ForEachWritableComponent(0, (idx, comp) =>
         {
            Assert.IsTrue(comp.HasWritableArray);
            Assert.NotNull(comp.WritableArray);
            Assert.AreEqual(0, comp.WritableArrayOffset);

            return true;
         });
      }

      [Test]
      public void TestConstructorCapacityAndMaxCapacityAllocatesArray()
      {
         int baseCapacity = ProtonByteBuffer.DefaultCapacity + 10;
         IProtonBuffer buffer = new ProtonByteBuffer(baseCapacity, baseCapacity + 100);

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(baseCapacity, buffer.Capacity);
         Assert.DoesNotThrow(() => buffer.EnsureWritable(baseCapacity + 100));
         buffer.WriteByte(1);
         Assert.AreEqual(1u, buffer.ReadableComponentCount);

         buffer.ForEachReadableComponent(0, (idx, comp) =>
         {
            Assert.IsTrue(comp.HasReadableArray);
            Assert.NotNull(comp.ReadableArray);
            Assert.AreEqual(0, comp.ReadableArrayOffset);

            return true;
         });
      }

      [Test]
      public void TestConstructorCapacityExceptions()
      {
         Assert.Throws(typeof(ArgumentOutOfRangeException), () => new ProtonByteBuffer(-1));
      }

      [Test]
      public void TestConstructorCapacityMaxCapacity()
      {
         IProtonBuffer buffer = new ProtonByteBuffer(
             ProtonByteBuffer.DefaultCapacity + 10, ProtonByteBuffer.DefaultMaximumCapacity - 100);

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(ProtonByteBuffer.DefaultCapacity + 10, buffer.Capacity);

         Assert.Throws(typeof(ArgumentOutOfRangeException),
            () => buffer.EnsureWritable(ProtonByteBuffer.DefaultMaximumCapacity - 50));
      }

      [Test]
      public void TestConstructorCapacityMaxCapacityExceptions()
      {
         try
         {
            new ProtonByteBuffer(-1, ProtonByteBuffer.DefaultMaximumCapacity);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            new ProtonByteBuffer(ProtonByteBuffer.DefaultCapacity, -1);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            new ProtonByteBuffer(100, 10);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestConstructorByteArray()
      {
         byte[] source = new byte[ProtonByteBuffer.DefaultCapacity + 10];

         IProtonBuffer buffer = new ProtonByteBuffer(source);

         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);
         Assert.AreEqual(ProtonByteBuffer.DefaultCapacity + 10, buffer.Capacity);
         Assert.AreEqual(source.LongLength, buffer.ReadableBytes);

         Assert.Throws(typeof(ArgumentOutOfRangeException),
            () => buffer.EnsureWritable(ProtonByteBuffer.DefaultCapacity + 200));

         buffer.ForEachReadableComponent(0, (idx, comp) =>
         {
            Assert.IsTrue(comp.HasReadableArray);
            Assert.NotNull(comp.ReadableArray);
            Assert.AreEqual(0, comp.ReadableArrayOffset);
            Assert.AreSame(source, comp.ReadableArray);

            return true;
         });
      }

      [Test]
      public void TestConstructorByteArrayThrowsWhenNull()
      {
         try
         {
            new ProtonByteBuffer(null);
            Assert.Fail("Should throw NullReferenceException");
         }
         catch (NullReferenceException) { }
      }

      #endregion

      #region Tests that attempt to alter buffer capacity

      [Test]
      public void TestCannotIncreaseCapacityWhenAssignedArray()
      {
         byte[] source = new byte[100];

         IProtonBuffer buffer = new ProtonByteBuffer(source);
         Assert.AreEqual(100, buffer.Capacity);

         Assert.Throws(typeof(ArgumentOutOfRangeException), () => buffer.EnsureWritable(2048));
      }

      [Test]
      public void TestEnsureWritableReallocatesArrayWhenSpaceNeedsItOtherwiseNot()
      {
         IProtonBuffer buffer = new ProtonByteBuffer();

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);

         byte[] first = null;
         byte[] second = null;

         buffer.ForEachWritableComponent(0, (idx, comp) =>
         {
            Assert.IsTrue(comp.HasWritableArray);
            Assert.NotNull(comp.WritableArray);
            Assert.AreEqual(0, comp.WritableArrayOffset);

            first = comp.WritableArray;

            return true;
         });

         Assert.NotNull(first);
         buffer.EnsureWritable(ProtonByteBuffer.DefaultCapacity + 12);
         Assert.IsTrue(buffer.Capacity >= ProtonByteBuffer.DefaultCapacity + 12);

         buffer.ForEachWritableComponent(0, (idx, comp) =>
         {
            second = comp.WritableArray;

            return true;
         });

         Assert.AreNotSame(first, second);
      }

      #endregion

      #region Tests for buffer Copy operations

      [Test]
      public void TestCopyEmptyBufferCopiesBackingArray()
      {
         IProtonBuffer buffer = new ProtonByteBuffer(10);
         IProtonBuffer copy = buffer.Copy();

         Assert.AreEqual(buffer.ReadableBytes, copy.ReadableBytes);

         copy.EnsureWritable(1);
         copy.WriteByte(1);

         Assert.AreNotEqual(buffer.GetByte(0), copy.GetByte(0));
      }

      [Test]
      public void TestCopyBufferResultsInMatchingBackingArrays()
      {
         IProtonBuffer buffer = new ProtonByteBuffer(10);

         buffer.WriteByte(1);
         buffer.WriteByte(2);
         buffer.WriteByte(3);
         buffer.WriteByte(4);
         buffer.WriteByte(5);

         // Make it writable without resizing so we can check the backing arrays.
         IProtonBuffer copy = buffer.Copy().Reset();

         byte[] first = null;
         byte[] second = null;

         buffer.ForEachWritableComponent(0, (idx, comp) =>
         {
            first = comp.WritableArray;

            return true;
         });

         copy.ForEachWritableComponent(0, (idx, comp) =>
         {
            second = comp.WritableArray;

            return true;
         });

         Assert.IsNotNull(first);
         Assert.IsNotNull(second);
         Assert.AreNotSame(first.GetHashCode(), second.GetHashCode());

         for (int i = 0; i < 5; ++i)
         {
            Assert.AreEqual(first[i], second[i]);
         }
      }

      #endregion

      #region Tests for string conversion

      [Test]
      public void TestToStringUTF8FromProtonBuffer()
      {
         string sourceString = "Test-String-1";
         Encoding utf8 = new UTF8Encoding();

         IProtonBuffer buffer = AllocateBuffer(sourceString.Length);

         byte[] utf8Bytes = utf8.GetBytes(sourceString);

         buffer.WriteBytes(utf8Bytes);

         String decoded = buffer.ToString(utf8);

         Assert.AreEqual(sourceString, decoded);
      }

      #endregion

      #region Abstract test class method implementations

      protected override IProtonBuffer AllocateBuffer(int initialCapacity)
      {
         return ProtonByteBufferAllocator.Instance.Allocate(initialCapacity);
      }

      protected override IProtonBuffer AllocateBuffer(int initialCapacity, int maxCapacity)
      {
         return ProtonByteBufferAllocator.Instance.Allocate(initialCapacity, maxCapacity);
      }

      protected override IProtonBuffer WrapBuffer(byte[] array)
      {
         return ProtonByteBufferAllocator.Instance.Wrap(array);
      }

      #endregion
   }
}