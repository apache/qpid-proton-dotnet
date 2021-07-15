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
         catch (ArgumentOutOfRangeException) { }
      }

      [Test]
      public void TestBufferRespectsMaxCapacityLimitNoGrowthScenario()
      {
         IProtonBuffer buffer = AllocateBuffer(10, 10);

         Assert.AreEqual(0, buffer.ReadableBytes);
         Assert.AreEqual(10, buffer.Capacity);

         // Writes to capacity work, but exceeding that should fail.
         for (sbyte i = 0; i < 10; ++i)
         {
            buffer.WriteByte(i);
         }

         try
         {
            buffer.WriteByte(10);
            Assert.Fail("Should not be able to write more than the max capacity bytes");
         }
         catch (ArgumentOutOfRangeException) { }
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
         Assert.AreEqual(13, buffer.Capacity);
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
      /// ProtonBuffer allocated with defaults for capacity and max-capacity.
      /// </summary>
      /// <returns>ProtonBuffer allocated with defaults for capacity and max-capacity.</returns>
      protected virtual IProtonBuffer AllocateDefaultBuffer()
      {
         return AllocateBuffer(DefaultCapacity);
      }

      /// <summary>
      /// ProtonBuffer allocated with the default max capacity but with the given initial cpacity.
      /// </summary>
      /// <param name="initialCapacity"></param>
      /// <returns>ProtonBuffer allocated with an initial capacity and a default max-capacity.</returns>
      protected abstract IProtonBuffer AllocateBuffer(int initialCapacity);

      /// <summary>
      /// ProtonBuffer allocated with the given max capacity but with the given initial cpacity.
      /// </summary>
      /// <param name="initialCapacity"></param>
      /// <returns>ProtonBuffer allocated with an initial capacity and a default max-capacity.</returns>
      protected abstract IProtonBuffer AllocateBuffer(int initialCapacity, int maxCapacity);

      /// <summary>
      /// ProtonBuffer that wraps the given buffer.
      /// </summary>
      /// <param name="array"></param>
      /// <returns>ProtonBuffer that wraps the given buffer.</returns>
      protected abstract IProtonBuffer WrapBuffer(byte[] array);

      #endregion

   }
}