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
   [TestFixture, Timeout(20000)]
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
         IDeque<string> testQ = new ArrayDeque<string>();

         Assert.True(testQ.IsEmpty);
         Assert.AreEqual(0, testQ.Count);

         Assert.IsFalse(testQ.IsReadOnly);
         Assert.IsFalse(testQ.IsSynchronized);
         Assert.IsNotNull(testQ.SyncRoot);
      }

      [Test]
      public void TestDequeueWhenEmpty()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         Assert.True(testQ.IsEmpty);
         Assert.AreEqual(0, testQ.Count);

         Assert.Throws<InvalidOperationException>(() => testQ.DequeueFront());
         Assert.Throws<InvalidOperationException>(() => testQ.DequeueBack());
      }

      [Test]
      public void TestTryDequeueWhenEmpty()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         Assert.True(testQ.IsEmpty);
         Assert.AreEqual(0, testQ.Count);

         string result = null;

         Assert.IsFalse(testQ.TryDequeueFront(out result));
         Assert.IsFalse(testQ.TryDequeueBack(out result));
      }

      [Test]
      public void TestEnqueueThenDequeFront()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueFront());
      }

      [Test]
      public void TestEnqueueThenDequeBack()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueBack("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueBack());
      }

      [Test]
      public void TestEnqueueFrontThenDequeBack()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueBack());
      }

      [Test]
      public void TestEnqueueBackFrontThenDequeFront()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueBack("test");
         Assert.AreEqual(1, testQ.Count);
         Assert.AreEqual("test", testQ.DequeueFront());
      }

      [Test]
      public void TestEnqueueThreeAndDequeueThree()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

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
         IDeque<string> testQ = new ArrayDeque<string>();

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
      public void TestMoveNextOnEmptyQueueReturnsFalse()
      {
         IDeque<string> testQ = new ArrayDeque<string>();
         IEnumerator<string> enumerator = testQ.GetEnumerator();

         Assert.IsFalse(enumerator.MoveNext());

         Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
      }

      [Test]
      public void TestCannotCallCurrentOnEnumeratorBeforeMoveNext()
      {
         IDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
      }

      [Test]
      public void TestCannotCallCurrentAfterEnumeratingAllElements()
      {
         IDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         Assert.IsTrue(enumerator.MoveNext());
         Assert.IsFalse(enumerator.MoveNext());

         Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
      }

      [Test]
      public void TestMoveNextFailsWhenConcurrentlyModified()
      {
         IDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         testQ.EnqueueBack("two");

         Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
      }

      [Test]
      public void TestCurretFailsWhenConcurrentlyModified()
      {
         IDeque<string> testQ = new ArrayDeque<string>();
         testQ.EnqueueBack("one");

         IEnumerator<string> enumerator = testQ.GetEnumerator();

         Assert.IsTrue(enumerator.MoveNext());
         testQ.EnqueueBack("two");

         Assert.Throws<InvalidOperationException>(() => _ = enumerator.Current);
      }

      [Test]
      public void TestEnqueueAndDequeueSmallSeries()
      {
         IDeque<int> testQ = new ArrayDeque<int>();

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

      [Test]
      public void TestEnqueueFrontAndDequeueBackLargeSeries()
      {
         IDeque<int> testQ = new ArrayDeque<int>();

         int[] expected = new int[ushort.MaxValue];
         for (int index = 0; index < expected.Length; ++index)
         {
            expected[index] = index;
         }

         foreach (int entry in expected)
         {
            testQ.EnqueueFront(entry);
         }

         for (int index = 0; index < expected.Length; ++index)
         {
            Assert.AreEqual(expected[index], testQ.DequeueBack());
         }
      }

      [Test]
      public void TestEnqueueAndDequeueSmallSeriesWithExistingHeadElementsAlreadyAdded()
      {
         IDeque<int> testQ = new ArrayDeque<int>();

         testQ.EnqueueFront(Int32.MaxValue - 1);
         testQ.EnqueueFront(Int32.MaxValue);

         int[] expected = new int[ushort.MaxValue];
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

      [Test]
      public void TestPeekAtHeadOfQueueAndDequeue()
      {
         IDeque<int> testQ = new ArrayDeque<int>();

         testQ.EnqueueFront(Int32.MaxValue - 1);
         testQ.EnqueueFront(Int32.MaxValue);

         int[] expected = new int[ushort.MaxValue];
         for (int index = 0; index < expected.Length; ++index)
         {
            expected[index] = index;
         }

         foreach (int entry in expected)
         {
            testQ.Enqueue(entry);
         }

         Assert.AreEqual(Int32.MaxValue, testQ.Peek());
         Assert.AreEqual(Int32.MaxValue, testQ.Dequeue());
         Assert.AreEqual(Int32.MaxValue - 1, testQ.Peek());
         Assert.AreEqual(Int32.MaxValue - 1, testQ.Dequeue());

         foreach (int entry in expected)
         {
            Assert.AreEqual(entry, testQ.Peek());
            Assert.AreEqual(entry, testQ.Dequeue());
         }

         Assert.AreEqual(0, testQ.Count);
         Assert.IsTrue(testQ.IsEmpty);
      }

      [Test]
      public void TestPeekAtTailOfQueueAndDequeueBack()
      {
         IDeque<int> testQ = new ArrayDeque<int>();

         testQ.EnqueueBack(Int32.MaxValue - 1);
         testQ.EnqueueBack(Int32.MaxValue);

         int[] expected = new int[ushort.MaxValue];
         for (int index = 0; index < expected.Length; ++index)
         {
            expected[index] = index;
         }

         foreach (int entry in expected)
         {
            testQ.Enqueue(entry);
         }

         for (int i = expected.Length - 1; i >= 0; --i)
         {
            Assert.AreEqual(expected[i], testQ.PeekBack());
            Assert.AreEqual(expected[i], testQ.DequeueBack());
         }

         Assert.AreEqual(Int32.MaxValue, testQ.PeekBack());
         Assert.AreEqual(Int32.MaxValue, testQ.DequeueBack());
         Assert.AreEqual(Int32.MaxValue - 1, testQ.PeekBack());
         Assert.AreEqual(Int32.MaxValue - 1, testQ.DequeueBack());

         Assert.AreEqual(0, testQ.Count);
         Assert.IsTrue(testQ.IsEmpty);
      }

      [Test]
      public void TestRemoveItemAtFront()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Enqueue("one");
         testQ.Enqueue("two");
         testQ.Enqueue("three");

         Assert.IsTrue(testQ.Remove("one"));
         Assert.AreEqual(2, testQ.Count);

         Assert.AreEqual("two", testQ.Dequeue());
         Assert.AreEqual("three", testQ.Dequeue());
      }

      [Test]
      public void TestRemoveItemAtBack()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Enqueue("one");
         testQ.Enqueue("two");
         testQ.Enqueue("three");

         Assert.IsTrue(testQ.Remove("three"));
         Assert.AreEqual(2, testQ.Count);

         Assert.AreEqual("one", testQ.Dequeue());
         Assert.AreEqual("two", testQ.Dequeue());
      }

      [Test]
      public void TestRemoveItemAtMiddle()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Enqueue("one");
         testQ.Enqueue("two");
         testQ.Enqueue("three");

         Assert.IsTrue(testQ.Remove("two"));
         Assert.AreEqual(2, testQ.Count);

         Assert.AreEqual("one", testQ.Dequeue());
         Assert.AreEqual("three", testQ.Dequeue());
      }

      [Test]
      public void TestRemoveItemFromBackwardsWrappedQueue()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("two");
         testQ.EnqueueFront("one");
         testQ.Enqueue("x");
         testQ.EnqueueBack("three");
         testQ.EnqueueBack("four");

         Assert.IsTrue(testQ.Remove("x"));
         Assert.AreEqual(4, testQ.Count);

         Assert.AreEqual("one", testQ.Dequeue());
         Assert.AreEqual("two", testQ.Dequeue());
         Assert.AreEqual("three", testQ.Dequeue());
         Assert.AreEqual("four", testQ.Dequeue());
      }

      [Test]
      public void TestRemoveFromEmptyQueue()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         Assert.IsFalse(testQ.Remove("x"));
      }

      [Test]
      public void TestRemoveSingularItemFromElementOneQueue()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Add("x");

         Assert.IsTrue(testQ.Remove("x"));
         Assert.IsFalse(testQ.Remove("x"));
      }

      [Test]
      public void TestTryRemoveFromQueueWhenNotInQueue()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Add("y");

         Assert.IsFalse(testQ.Remove("x"));
      }

      [Test]
      public void TestClearQueueAllInsertsAtFront()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Enqueue("one");
         testQ.Enqueue("two");
         testQ.Enqueue("three");

         testQ.Clear();
         Assert.AreEqual(0, testQ.Count);
         Assert.IsTrue(testQ.IsEmpty);
      }

      [Test]
      public void TestEnumeratorFailsFastOnClearedQueue()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Enqueue("one");
         testQ.Enqueue("two");
         testQ.Enqueue("three");

         IEnumerator<string> cursor = testQ.GetEnumerator();
         Assert.IsTrue(cursor.MoveNext());

         testQ.Clear();

         Assert.AreEqual(0, testQ.Count);
         Assert.IsTrue(testQ.IsEmpty);

         Assert.Throws<InvalidOperationException>(() => _ = cursor.Current);
         Assert.Throws<InvalidOperationException>(() => _ = cursor.MoveNext());
      }

      [Test]
      public void TestTryPeekFrontAndBack()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.Enqueue("one");
         testQ.Enqueue("two");
         testQ.Enqueue("three");

         Assert.AreEqual("one", testQ.Peek());
         Assert.AreEqual("one", testQ.PeekFront());
         Assert.AreEqual("three", testQ.PeekBack());

         string value = null;

         Assert.IsTrue(testQ.TryPeek(out value));
         Assert.AreEqual("one", value);
         Assert.IsTrue(testQ.TryPeekFront(out value));
         Assert.AreEqual("one", value);
         Assert.IsTrue(testQ.TryPeekBack(out value));
         Assert.AreEqual("three", value);

         testQ.Clear();

         Assert.IsFalse(testQ.TryPeek(out value));
         Assert.IsFalse(testQ.TryPeekFront(out value));
         Assert.IsFalse(testQ.TryPeekBack(out value));
      }

      [Test]
      public void TestCopyToFromEmptyQueue()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         Assert.True(testQ.IsEmpty);
         Assert.AreEqual(0, testQ.Count);

         string[] destination = new string[10];

         testQ.CopyTo(destination, 0);

         foreach (string value in destination)
         {
            Assert.IsNull(value);
         }
      }

      [Test]
      public void TestCopyToFromQueue()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("two");
         testQ.EnqueueFront("one");
         testQ.EnqueueBack("three");

         Assert.False(testQ.IsEmpty);
         Assert.AreEqual(3, testQ.Count);

         string[] destination = new string[testQ.Count + 2];

         testQ.CopyTo(destination, 1);

         Assert.IsNull(destination[0]);
         Assert.IsNull(destination[4]);

         Assert.AreEqual("one", destination[1]);
         Assert.AreEqual("two", destination[2]);
         Assert.AreEqual("three", destination[3]);
      }

      [Test]
      public void TestCopyToFromQueueToOpaqueArray()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("two");
         testQ.EnqueueFront("one");
         testQ.EnqueueBack("three");

         Assert.False(testQ.IsEmpty);
         Assert.AreEqual(3, testQ.Count);

         Array destination = new string[testQ.Count + 2];

         testQ.CopyTo(destination, 1);

         Assert.IsNull(destination.GetValue(0));
         Assert.IsNull(destination.GetValue(4));

         Assert.AreEqual("one", destination.GetValue(1));
         Assert.AreEqual("two", destination.GetValue(2));
         Assert.AreEqual("three", destination.GetValue(3));
      }

      [Test]
      public void TestCopyToFromQueueChecksIndexAgainstArraySize()
      {
         IDeque<string> testQ = new ArrayDeque<string>();

         testQ.EnqueueFront("two");

         string[] destination = new string[1];

         Assert.Throws<ArgumentOutOfRangeException>(() => testQ.CopyTo(destination, 1));
      }

      [Test]
      public void TestQueuesWithSameValuesAreEqual()
      {
         IDeque<string> testQ1 = new ArrayDeque<string>();
         IDeque<string> testQ2 = new ArrayDeque<string>();

         testQ1.EnqueueFront("two");
         testQ1.EnqueueFront("one");
         testQ1.EnqueueBack("three");

         testQ2.Enqueue("one");
         testQ2.Enqueue("two");
         testQ2.Enqueue("three");

         Assert.AreEqual(testQ1, testQ2);
      }

      [Test]
      public void TestQueuesWithNotSameValuesAreEqual()
      {
         IDeque<string> testQ1 = new ArrayDeque<string>();
         IDeque<string> testQ2 = new ArrayDeque<string>();

         testQ1.EnqueueFront("two");
         testQ1.EnqueueFront("one");
         testQ1.EnqueueBack("three");
         testQ1.EnqueueBack("four");

         testQ2.Enqueue("one");
         testQ2.Enqueue("two");
         testQ2.Enqueue("three");

         Assert.AreNotEqual(testQ1, testQ2);
      }

      [Test]
      public void TestQueuesWithSameValuesAreEqualInDifferentColleciton()
      {
         IDeque<string> testQ1 = new ArrayDeque<string>();
         IList<string> testQ2 = new List<string>();

         testQ1.EnqueueFront("two");
         testQ1.EnqueueFront("one");
         testQ1.EnqueueBack("three");

         testQ2.Add("one");
         testQ2.Add("two");
         testQ2.Add("three");

         Assert.AreEqual(testQ1, testQ2);
      }
   }
}