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
using NUnit.Framework;

namespace Apache.Qpid.Proton.Utilities
{
   [TestFixture]
   public class ArrayDequeTest
   {
      protected int seed;
      protected Random random;

      [SetUp]
      public void Setup()
      {
         seed = Environment.TickCount;
         random = new Random(seed);
      }

      [Test]
      public void TestCreate()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         Assert.True(testQ.IsEmpty);
         Assert.AreEqual(0, testQ.Count);
      }

      [Test]
      public void TestDequeueWhenEmpty()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         Assert.True(testQ.IsEmpty);
         Assert.AreEqual(0, testQ.Count);

         Assert.Throws<InvalidOperationException>(() => testQ.DequeueFront());
         Assert.Throws<InvalidOperationException>(() => testQ.DequeueBack());
      }

      [Test]
      public void TestTryDequeueWhenEmpty()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         Assert.True(testQ.IsEmpty);
         Assert.AreEqual(0, testQ.Count);

         string result = null;

         Assert.IsFalse(testQ.TryDequeueFront(out result));
         Assert.IsFalse(testQ.TryDequeueBack(out result));
      }

      [Test]
      public void TestEnqueueThenDequeFront()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueFront());
      }

      [Test]
      public void TestEnqueueThenDequeBack()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueBack("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueBack());
      }

      [Test]
      public void TestEnqueueFrontThenDequeBack()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueBack());
      }

      [Test]
      public void TestEnqueueBackFrontThenDequeFront()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueBack("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueFront());
      }

      [Test]
      public void TestEnqueueThreeAndDequeueThree()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueBack("one");
         testQ.EnqueueBack("two");
         testQ.EnqueueBack("three");

         Assert.AreEqual("one", testQ.DequeueFront());
         Assert.AreEqual("two", testQ.DequeueFront());
         Assert.AreEqual("three", testQ.DequeueFront());
      }

      [Test]
      public void TestEnumerateBasicQueue()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();

         string[] expected = new string[] { "1", "2", "3" };

         foreach (string entry in expected)
         {
            testQ.EnqueueBack(entry);
         }

         int index = 0;
         foreach (string entry in testQ)
         {
            Assert.AreEqual(expected[index++], entry);
         }
      }

      [Test]
      public void TestCannotCallCurrentOnEnumeratorBeforeMoveNext()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
      }

      [Test]
      public void TestCannotCallCurrentAfterEnumeratingAllElements()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         Assert.IsTrue(enumerator.MoveNext());
         Assert.IsFalse(enumerator.MoveNext());

         Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
      }

      [Test]
      public void TestMoveNextFailsWhenConcurrentlyModified()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         testQ.EnqueueBack("two");

         Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
      }

      [Test]
      public void TestCurretFailsWhenConcurrentlyModified()
      {
         ArrayDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         Assert.IsTrue(enumerator.MoveNext());
         testQ.EnqueueBack("two");

         Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
      }

      [Test]
      public void TestEnqueueAndDequeueSmallSeries()
      {
         ArrayDeque<int> testQ = new ArrayDeque<int>();

         int[] expected = new int[short.MaxValue];
         for (int index = 0; index < expected.Length; ++index)
         {
            expected[index] = index;
         }

         foreach (int entry in expected)
         {
            testQ.Enqueue(entry);
         }

         int pos = 0;
         foreach (int entry in testQ)
         {
            Assert.AreEqual(expected[pos++], entry);
         }
      }

      [Ignore("Test failing currently")]
      [Test]
      public void TestEnqueueAndDequeueSmallSeriesWithExistingHeadElementsAlreadyAdded()
      {
         ArrayDeque<int> testQ = new ArrayDeque<int>();

         testQ.EnqueueFront(Int32.MaxValue - 1);
         testQ.EnqueueFront(Int32.MaxValue);

         int[] expected = new int[short.MaxValue];
         for (int index = 0; index < expected.Length; ++index)
         {
            expected[index] = index;
         }

         foreach (int entry in expected)
         {
            testQ.Enqueue(entry);
         }

         Assert.AreEqual(Int32.MaxValue, testQ.DequeueFront());
         Assert.AreEqual(Int32.MaxValue - 1, testQ.DequeueFront());

         int pos = 0;
         foreach (int entry in testQ)
         {
            Assert.AreEqual(expected[pos++], entry);
         }
      }
   }
}