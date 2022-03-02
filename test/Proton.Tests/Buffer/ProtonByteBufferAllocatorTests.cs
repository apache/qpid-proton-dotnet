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
using NUnit.Framework;

namespace Apache.Qpid.Proton.Buffer
{
   [TestFixture]
   public class ProtonByteBufferAllocatorTests
   {
      [Test]
      public void TestAllocate()
      {
         ProtonByteBuffer buffer = (ProtonByteBuffer)ProtonByteBufferAllocator.Instance.Allocate();

         Assert.IsNotNull(buffer);
         Assert.AreEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);
      }

      [Test]
      public void TestAllocateWithInitialCapacity()
      {
         ProtonByteBuffer buffer = (ProtonByteBuffer)ProtonByteBufferAllocator.Instance.Allocate(1024);

         Assert.IsNotNull(buffer);
         Assert.AreNotEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);
         Assert.AreEqual(1024, buffer.Capacity);
      }

      [Test]
      public void TestAllocateWithInvalidInitialCapacity()
      {
         try
         {
            ProtonByteBufferAllocator.Instance.Allocate(-1);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestAllocateWithInitialAndMaximumCapacity()
      {
         ProtonByteBuffer buffer = (ProtonByteBuffer)ProtonByteBufferAllocator.Instance.Allocate(1023, 2048);

         Assert.IsNotNull(buffer);
         Assert.AreNotEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);

         Assert.AreNotEqual(1024, buffer.Capacity);
         Assert.DoesNotThrow(() => buffer.EnsureWritable(2048));
         Assert.Throws<ArgumentOutOfRangeException>(() => buffer.EnsureWritable(2049));
      }

      [Test]
      public void TestAllocateWithInvalidInitialAndMaximimCapacity()
      {
         try
         {
            ProtonByteBufferAllocator.Instance.Allocate(64, 32);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            ProtonByteBufferAllocator.Instance.Allocate(-1, 64);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            ProtonByteBufferAllocator.Instance.Allocate(-1, -1);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }

         try
         {
            ProtonByteBufferAllocator.Instance.Allocate(64, -1);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestWrapByteArray()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(source);

         Assert.IsNotNull(buffer);
         Assert.AreNotEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);
         Assert.AreEqual(source.LongLength, buffer.Capacity);

         try
         {
            buffer.EnsureWritable(ProtonByteBuffer.DefaultCapacity);
            Assert.Fail("Should have thrown an ArgumentOutOfRangeException");
         }
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestWrapByteArrayWithOffsetAndLength()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(source, 0, source.Length);

         Assert.IsNotNull(buffer);
         Assert.AreNotEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);
         Assert.AreNotEqual(ProtonByteBuffer.DefaultMaximumCapacity, buffer.Capacity);

         Assert.AreEqual(source.Length, buffer.Capacity);
         Assert.AreEqual(1, buffer.ReadableComponentCount);

         int index = 0;
         buffer.ForEachReadableComponent(index, (i, component) =>
         {
            Assert.AreSame(source, component.ReadableArray);
            Assert.AreEqual(0, component.ReadableArrayOffset);
            Assert.AreEqual(source.Length, component.ReadableArrayLength);
            return true;
         });
      }

      [Test]
      public void TestWrapByteArrayWithOffsetAndLengthSubset()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(source, 1, source.Length - 2);

         Assert.IsNotNull(buffer);
         Assert.AreNotEqual(ProtonByteBuffer.DefaultCapacity, buffer.Capacity);
         Assert.AreNotEqual(ProtonByteBuffer.DefaultMaximumCapacity, buffer.Capacity);

         Assert.AreEqual(source.Length - 2, buffer.Capacity);
         Assert.AreEqual(1, buffer.ReadableComponentCount);

         int index = 0;
         buffer.ForEachReadableComponent(index, (i, component) =>
         {
            Assert.AreSame(source, component.ReadableArray);
            Assert.AreEqual(1, component.ReadableArrayOffset);
            Assert.AreEqual(source.Length - 2, component.ReadableArrayLength);
            return true;
         });
      }
   }
}