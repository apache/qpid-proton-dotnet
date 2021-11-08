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
   public class ProtonCompositeBufferTest
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
   }
}