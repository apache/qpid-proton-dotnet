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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Apache.Qpid.Proton.Engine.Utils;

namespace Apache.Qpid.Proton.Engine.Utils
{
   public class RingQueueTests
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
         RingQueue<string> testQ = new RingQueue<string>(16);

         Assert.True(testQ.IsEmpty());
         Assert.AreEqual(0, testQ.Count);
         Assert.Null(testQ.Peek());
      }

      [Test]
      public void TestOffer()
      {
         RingQueue<string> testQ = new RingQueue<string>(3);

         testQ.Offer("1");
         testQ.Offer("2");
         testQ.Offer("3");

         Assert.False(testQ.IsEmpty());
         Assert.AreEqual(3, testQ.Count);
         Assert.NotNull(testQ.Peek());
         Assert.AreEqual("1", testQ.Peek());
      }

      [Test]
      public void TestPoll()
      {
         RingQueue<string> testQ = new RingQueue<string>(3);

         testQ.Offer("1");
         testQ.Offer("2");
         testQ.Offer("3");

         Assert.False(testQ.IsEmpty());
         Assert.AreEqual(3, testQ.Count);
         Assert.NotNull(testQ.Peek());
         Assert.AreEqual("1", testQ.Peek());

         Assert.AreEqual("1", testQ.Poll());
         Assert.AreEqual("2", testQ.Poll());
         Assert.AreEqual("3", testQ.Poll());

         Assert.True(testQ.IsEmpty());
         Assert.AreEqual(0, testQ.Count);
         Assert.Null(testQ.Peek());
      }

      [Test]
      public void TestOfferAfterDequeueFromFull()
      {
         RingQueue<string> testQ = new RingQueue<string>(3);

         testQ.Offer("1");
         testQ.Offer("2");
         testQ.Offer("3");

         Assert.False(testQ.IsEmpty());
         Assert.AreEqual(3, testQ.Count);
         Assert.NotNull(testQ.Peek());
         Assert.AreEqual("1", testQ.Peek());
         Assert.AreEqual("1", testQ.Poll());
         Assert.False(testQ.IsEmpty());
         Assert.AreEqual(2, testQ.Count);
         Assert.NotNull(testQ.Peek());
         Assert.AreEqual("2", testQ.Peek());

         testQ.Offer("4");

         Assert.AreEqual("2", testQ.Poll());
         Assert.AreEqual("3", testQ.Poll());
         Assert.AreEqual("4", testQ.Poll());

         Assert.True(testQ.IsEmpty());
         Assert.AreEqual(0, testQ.Count);
         Assert.Null(testQ.Peek());
      }

      [Test]
      public void TestIterateOverFullQueue()
      {
         RingQueue<string> testQ = new RingQueue<string>(3);

         Queue<string> inputQ = new Queue<string>(new List<string> { "1", "2", "3" });

         foreach (var item in inputQ)
         {
            testQ.Offer(item);
         }

         Assert.False(testQ.IsEmpty());
         Assert.AreEqual(3, testQ.Count);
         Assert.NotNull(testQ.Peek());

         IEnumerator<string> iter = testQ.GetEnumerator();
         Assert.True(iter.MoveNext());

         do
         {
            string next = iter.Current;
            Assert.AreEqual(inputQ.Dequeue(), next);
         }
         while (iter.MoveNext());
      }

      [Test]
      public void TestContains()
      {
         const int COUNT = 100;
         RingQueue<string> testQ = new RingQueue<string>(COUNT);

         for (int i = 0; i < COUNT; ++i)
         {
            Assert.True(testQ.Offer(random.Next().ToString()));
         }

         random = new Random(seed);  // Reset

         for (int i = 0; i < COUNT; ++i)
         {
            Assert.True(testQ.Contains(random.Next().ToString()));
         }

         Assert.False(testQ.Contains("this-string"));
         Assert.False(testQ.Contains(null));
      }

      [Test]
      public void TestContainsNullElement()
      {
         RingQueue<string> testQ = new RingQueue<string>(10);

         testQ.Offer("1");
         testQ.Offer(null);
         testQ.Offer("2");
         testQ.Offer(null);
         testQ.Offer("3");

         Assert.True(testQ.Contains("1"));
         Assert.True(testQ.Contains(null));

         Assert.AreEqual("1", testQ.Poll());
         Assert.AreEqual(null, testQ.Poll());

         Assert.True(testQ.Contains("2"));
         Assert.True(testQ.Contains(null));
      }

      [Test]
      public void TestIterateOverQueue()
      {
         const int COUNT = 100;
         RingQueue<string> testQ = new RingQueue<string>(COUNT);

         for (int i = 0; i < COUNT; ++i)
         {
            Assert.True(testQ.Offer(random.Next().ToString()));
         }

         random = new Random(seed);  // Reset

         foreach (string item in testQ)
         {
            Assert.AreEqual(item, random.Next().ToString());
         }

         random = new Random(seed);  // Reset

         for (int i = 0; i < COUNT / 2; ++i)
         {
            Assert.AreEqual(random.Next().ToString(), testQ.Poll());
         }

         foreach (string item in testQ)
         {
            Assert.AreEqual(item, random.Next().ToString());
         }
      }

      [Test]
      public void testIterateOverQueueFilledViaCollection()
      {
         const int COUNT = 100;
         Queue<string> inputQ = new Queue<string>();

         for (int i = 0; i < COUNT; ++i)
         {
            inputQ.Enqueue(random.Next().ToString());
         }

         random = new Random(seed);  // Reset

         RingQueue<string> testQ = new RingQueue<string>(inputQ);
         foreach (string item in testQ)
         {
            Assert.AreEqual(item, random.Next().ToString());
         }

         random = new Random(seed);  // Reset

         for (int i = 0; i < COUNT / 2; ++i)
         {
            Assert.AreEqual(random.Next().ToString(), testQ.Poll());
         }

         foreach (string item in testQ)
         {
            Assert.AreEqual(item, random.Next().ToString());
         }
      }

      [Test]
      public void TestOfferPollAndOffer()
      {
         const int ITERATIONS = 10;
         const int COUNT = 100;

         List<string> dataSet = new List<string>(COUNT);
         for (int i = 0; i < COUNT; ++i)
         {
            dataSet.Add(random.Next().ToString());
         }

         RingQueue<string> testQ = new RingQueue<string>(COUNT);

         for (int iteration = 0; iteration < ITERATIONS; ++iteration)
         {
            testQ.Clear();

            for (int i = 0; i < COUNT; ++i)
            {
               Assert.True(testQ.Offer(dataSet[i]));
            }

            Assert.False(testQ.IsEmpty());

            for (int i = 0; i < COUNT; ++i)
            {
               Assert.NotNull(testQ.Poll());
            }

            Assert.True(testQ.IsEmpty());

            for (int i = 0; i < COUNT; ++i)
            {
               Assert.True(testQ.Offer(dataSet[i]));
            }

            Assert.False(testQ.IsEmpty());

            for (int i = 0; i < COUNT; ++i)
            {
               Assert.True(testQ.Contains(dataSet[0]));
            }
         }
      }

      [Test]
      public void TestIterateOverQueueReturnsNullIfMovedToFar()
      {
         const int COUNT = 100;
         RingQueue<string> testQ = new RingQueue<string>(COUNT);

         for (int i = 0; i < COUNT; ++i)
         {
            Assert.True(testQ.Offer(random.Next().ToString()));
         }

         random = new Random(seed);  // Reset

         IEnumerator<string> iterator = testQ.GetEnumerator();
         while (iterator.MoveNext())
         {
            Assert.AreEqual(random.Next().ToString(), iterator.Current);
         }

         Assert.False(iterator.MoveNext());
         Assert.Null(iterator.Current);
      }

      [Test]
      public void TestIteratorThrowsIfModifiedConcurrently()
      {
         const int COUNT = 100;
         RingQueue<string> testQ = new RingQueue<string>(COUNT);

         for (int i = 0; i < COUNT; ++i)
         {
            Assert.True(testQ.Offer(random.Next().ToString()));
         }

         random = new Random(seed);  // Reset

         IEnumerator<string> iterator = testQ.GetEnumerator();
         Assert.AreEqual(testQ.Poll(), random.Next().ToString());

         Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
      }

      [Test]
      public void TestIteratorThrowsIfModifiedConcurrentlySizeUnchanged()
      {
         const int COUNT = 100;
         RingQueue<string> testQ = new RingQueue<string>(COUNT);

         for (int i = 0; i < COUNT; ++i)
         {
            Assert.True(testQ.Offer(random.Next().ToString()));
         }

         random = new Random(seed);  // Reset

         IEnumerator<string> iterator = testQ.GetEnumerator();
         Assert.AreEqual(testQ.Poll(), random.Next().ToString());
         Assert.True(testQ.Offer(random.Next().ToString()));
         Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
      }
   }
}