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
   public class UnsettledDictionaryTest
   {
      private int seed;
      private Random random;

      private uint[] uintArray = new uint[1000];
      private DeliveryType[] objArray = new DeliveryType[1000];
      private UnsettledDictionary<DeliveryType> tracker = null;

      [SetUp]
      public void Setup()
      {
         seed = Environment.TickCount;
         random = new Random(seed);
         tracker = new UnsettledDictionary<DeliveryType>(d => d.DeliveryId);

         for (uint i = 1; i <= objArray.Length; i++)
         {
            uint x = uintArray[i - 1] = i;
            DeliveryType y = objArray[i - 1] = new DeliveryType(i);
            tracker.Add(x, y);
         }
      }

      [Test]
      public void TestCreateFailsWhenInitialBucketsNegativeSingleArgument()
      {
         Assert.Throws<ArgumentException> (() => new UnsettledDictionary<DeliveryType>(d => d.DeliveryId, -1));
      }

      [Test]
      public void TestCreateFailsWhenInitialBucketsNegative()
      {
         Assert.Throws<ArgumentException>(() => new UnsettledDictionary<DeliveryType>(d => d.DeliveryId, -1, 10));
      }

      [Test]
      public void TestCreateFailsWhenBucketsSizeNegative()
      {
         Assert.Throws<ArgumentException>(() => new UnsettledDictionary<DeliveryType>(d => d.DeliveryId, 10, -1));
      }

      [Test]
      public void TestComparator()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();

         Assert.IsNotNull(map.Comparer);
         Assert.AreSame(map.Comparer, map.Comparer);
      }

      [Test]
      public void TestCreateUnsettledTracker()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);
         Assert.IsTrue(tracker.IsEmpty);
         Assert.IsFalse(tracker.IsReadOnly);
         Assert.IsFalse(tracker.IsFixedSize);
      }

      [Test]
      public void TestContainsKeyOnEmptyMap()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         Assert.IsFalse(tracker.ContainsKey(0));
         Assert.IsFalse(tracker.ContainsKey(1));
      }

      [Test]
      public void TestGetWhenEmpty()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         DeliveryType delivery;

         Assert.IsFalse(tracker.TryGetValue(0, out delivery));
      }

      [Test]
      public void TestConstructFromAnotherDictionary()
      {
         IDictionary<uint, DeliveryType> source = new Dictionary<uint, DeliveryType>
         {
            { 0, new DeliveryType(0) },
            { 1, new DeliveryType(1) },
            { 2, new DeliveryType(2) },
            { 3, new DeliveryType(3) },
            { 5, new DeliveryType(5) },
            { 9, new DeliveryType(9) },
            { 7, new DeliveryType(7) },
            { 1024, new DeliveryType(1024) }
         };

         UnsettledDictionary<DeliveryType> map = CreateMap(source);

         Assert.AreEqual(8, map.Count);

         Assert.AreEqual(new DeliveryType(0), map[0]);
         Assert.AreEqual(new DeliveryType(1), map[1]);
         Assert.AreEqual(new DeliveryType(2), map[2]);
         Assert.AreEqual(new DeliveryType(3), map[3]);
         Assert.AreEqual(new DeliveryType(5), map[5]);
         Assert.AreEqual(new DeliveryType(9), map[9]);
         Assert.AreEqual(new DeliveryType(7), map[7]);
         Assert.AreEqual(new DeliveryType(1024), map[1024]);
      }

      [Test]
      public void TestGet()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(2, new DeliveryType(2));
         tracker.Add(65535, new DeliveryType(65535));
         tracker.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.IsTrue(tracker.TryGetValue(0, out DeliveryType delivery1));
         Assert.IsTrue(tracker.TryGetValue(1, out DeliveryType delivery2));
         Assert.IsTrue(tracker.TryGetValue(2, out DeliveryType delivery3));
         Assert.IsTrue(tracker.TryGetValue(65535, out DeliveryType delivery4));
         Assert.IsTrue(tracker.TryGetValue(uint.MaxValue, out DeliveryType delivery5));

         Assert.AreEqual(new DeliveryType(0), delivery1);
         Assert.AreEqual(new DeliveryType(1), delivery2);
         Assert.AreEqual(new DeliveryType(2), delivery3);
         Assert.AreEqual(new DeliveryType(65535), delivery4);
         Assert.AreEqual(new DeliveryType(uint.MaxValue), delivery5);

         Assert.IsFalse(tracker.TryGetValue(3, out DeliveryType delivery6));
         Assert.IsNull(delivery6);

         Assert.AreEqual(5, tracker.Count);
      }

      [Test]
      public void TestContainsKey()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(uint.MaxValue - 3, new DeliveryType(uint.MaxValue - 3));

         Assert.IsTrue(tracker.ContainsKey(0));
         Assert.IsFalse(tracker.ContainsKey(3));

         Assert.AreEqual(3, tracker.Count);
      }

      [Test]
      public void TestContainsValue()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.IsTrue(tracker.ContainsValue(new DeliveryType(0)));
         Assert.IsFalse(tracker.ContainsValue(new DeliveryType(4)));

         Assert.AreEqual(3, tracker.Count);
      }

      [Test]
      public void TestContainsValueOnEmptyDictionary()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         Assert.IsFalse(tracker.ContainsValue(new DeliveryType(0)));
      }

      [Test]
      public void TestRemoveIsIdempotent()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(2, new DeliveryType(2));

         Assert.AreEqual(3, tracker.Count);

         Assert.AreEqual(true, tracker.Remove(0));
         Assert.AreEqual(false, tracker.Remove(0));

         Assert.AreEqual(2, tracker.Count);

         Assert.AreEqual(true, tracker.Remove(1));
         Assert.AreEqual(false, tracker.Remove(1));

         Assert.AreEqual(1, tracker.Count);

         Assert.AreEqual(true, tracker.Remove(2));
         Assert.AreEqual(false, tracker.Remove(2));

         Assert.AreEqual(0, tracker.Count);
      }

      [Test]
      public void TestRemoveValueNotInMap()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(9, new DeliveryType(9));
         tracker.Add(7, new DeliveryType(7));
         tracker.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.AreEqual(5, tracker.Count);
         Assert.IsFalse(tracker.Remove(5));
         Assert.AreEqual(5, tracker.Count);
      }

      [Test]
      public void TestRemoveFirstEntryTwice()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(16, new DeliveryType(16));

         Assert.IsTrue(tracker.Remove(0));
         Assert.IsFalse(tracker.Remove(0));
      }

      [Test]
      public void TestRemoveEntryWithValue()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();

         Assert.IsFalse(map.Remove(new KeyValuePair<uint, DeliveryType>(1, new DeliveryType(0))));

         map.Add(0, new DeliveryType(0));
         map.Add(1, new DeliveryType(1));
         map.Add(9, new DeliveryType(9));
         map.Add(7, new DeliveryType(7));
         map.Add(99, new DeliveryType(99));

         Assert.AreEqual(5, map.Count);
         Assert.IsFalse(map.Remove(new KeyValuePair<uint, DeliveryType>(1, new DeliveryType(0))));
         Assert.AreEqual(5, map.Count);
         Assert.IsTrue(map.Remove(new KeyValuePair<uint, DeliveryType>(1, new DeliveryType(1))));
         Assert.AreEqual(4, map.Count);
         Assert.IsFalse(map.Remove(new KeyValuePair<uint, DeliveryType>(42, new DeliveryType(42))));
         Assert.AreEqual(4, map.Count);

         Assert.AreEqual(new DeliveryType(0), map[0]);
         Assert.AreEqual(new DeliveryType(9), map[9]);
         Assert.AreEqual(new DeliveryType(7), map[7]);
         Assert.AreEqual(new DeliveryType(99), map[99]);
      }

      [Test]
      public void TestRemoveWithInvalidType()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));

         try
         {
            tracker.Remove("foo");
            Assert.Fail("Should not accept incompatible types");
         }
         catch (Exception) {}
      }

      [Test]
      public void TestRemoveEntriesFromMiddleBucket()
      {
         // Start with three buckets of size two
         UnsettledDictionary<DeliveryType> tracker = CreateMap(3, 2);

         tracker.Add(1, new DeliveryType(1));
         tracker.Add(2, new DeliveryType(2));
         tracker.Add(3, new DeliveryType(3));
         tracker.Add(4, new DeliveryType(4));
         tracker.Add(5, new DeliveryType(5));
         tracker.Add(6, new DeliveryType(6));

         Assert.AreEqual(6, tracker.Count);

         tracker.Remove(3);
         tracker.Remove(4);

         Assert.AreEqual(4, tracker.Count);

         Assert.IsTrue(tracker.ContainsKey(1));
         Assert.IsTrue(tracker.ContainsKey(2));
         Assert.IsTrue(tracker.ContainsKey(5));
         Assert.IsTrue(tracker.ContainsKey(6));

         Assert.IsFalse(tracker.ContainsKey(3));
         Assert.IsFalse(tracker.ContainsKey(4));

         tracker.Add(7, new DeliveryType(7));
         tracker.Add(8, new DeliveryType(8));

         Assert.AreEqual(6, tracker.Count);
      }

      [Test]
      public void TestRemoveOneFromTail()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(2, 10);

         for (uint i = 0; i < 10; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.IsNotNull(tracker.Remove(1)); // Right after tail value of zero

         Assert.IsTrue(tracker.TryGetValue(0, out DeliveryType result1));
         Assert.IsTrue(tracker.TryGetValue(9, out DeliveryType result2));

         Assert.AreEqual(0, result1.DeliveryId);
         Assert.AreEqual(9, result2.DeliveryId);
      }

      [Test]
      public void TestRemoveOneFromHead()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(2, 10);

         for (uint i = 0; i < 10; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.IsNotNull(tracker.Remove(8)); // Right before head of the list value nine

         Assert.IsTrue(tracker.TryGetValue(0, out DeliveryType result1));
         Assert.IsTrue(tracker.TryGetValue(9, out DeliveryType result2));

         Assert.AreEqual(0, result1.DeliveryId);
         Assert.AreEqual(9, result2.DeliveryId);
      }

      [Test]
      public void TestInsert()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(2, new DeliveryType(2));
         tracker.Add(3, new DeliveryType(3));
         tracker.Add(5, new DeliveryType(5));
         tracker.Add(9, new DeliveryType(9));
         tracker.Add(7, new DeliveryType(7));
         tracker.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.AreEqual(8, tracker.Count);
      }

      [Test]
      public void TestAddedDeliveriesUpdatesSizeValue()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         DeliveryType delivery1 = new DeliveryType(0);
         DeliveryType delivery2 = new DeliveryType(1);

         tracker.Add(delivery1.DeliveryId, delivery1);
         Assert.AreEqual(1, tracker.Count);

         tracker.Add(delivery2.DeliveryId, delivery2);
         Assert.AreEqual(2, tracker.Count);
      }

      [Test]
      public void TestAddThenRemoveDelivery()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         DeliveryType delivery1 = new DeliveryType(127);
         DeliveryType delivery2 = new DeliveryType(32);

         tracker.Add(delivery1.DeliveryId, delivery1);
         Assert.AreEqual(1, tracker.Count);
         tracker.Remove(delivery1.DeliveryId);
         Assert.AreEqual(0, tracker.Count);

         tracker.Add(delivery2.DeliveryId, delivery2);
         Assert.AreEqual(1, tracker.Count);
         tracker.Remove(delivery2.DeliveryId);
         Assert.AreEqual(0, tracker.Count);
      }

      [Test]
      public void TestAddThenRemoveMultipleDeliveriesInSequence()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         DeliveryType delivery1 = new DeliveryType(int.MaxValue);
         DeliveryType delivery2 = new DeliveryType(uint.MaxValue);

         tracker.Add(delivery1.DeliveryId, delivery1);
         tracker.Add(delivery2.DeliveryId, delivery2);

         Assert.AreEqual(2, tracker.Count);
         Assert.IsNotNull(tracker.Remove(delivery1.DeliveryId));
         Assert.AreEqual(1, tracker.Count);
         Assert.IsNotNull(tracker.Remove(delivery2.DeliveryId));
         Assert.AreEqual(0, tracker.Count);
      }

      [Test]
      public void TestAddThenClearMultipleDeliveriesAddedInSequence()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         DeliveryType delivery1 = new DeliveryType(0);
         DeliveryType delivery2 = new DeliveryType(1);
         DeliveryType delivery3 = new DeliveryType(2);
         DeliveryType delivery4 = new DeliveryType(3);

         tracker.Add(delivery1.DeliveryId, delivery1);
         tracker.Add(delivery2.DeliveryId, delivery2);
         tracker.Add(delivery3.DeliveryId, delivery3);
         tracker.Add(delivery4.DeliveryId, delivery4);

         Assert.AreEqual(4, tracker.Count);
         tracker.Clear();
         Assert.AreEqual(0, tracker.Count);
      }

      [Test]
      public void TestGetOneDeliveryInBetweenOthersThatWereAdded()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         DeliveryType delivery1 = new DeliveryType(0);
         DeliveryType delivery2 = new DeliveryType(1);
         DeliveryType delivery3 = new DeliveryType(2);
         DeliveryType delivery4 = new DeliveryType(3);

         tracker.Add(delivery1.DeliveryId, delivery1);
         tracker.Add(delivery2.DeliveryId, delivery2);
         tracker.Add(delivery3.DeliveryId, delivery3);
         tracker.Add(delivery4.DeliveryId, delivery4);

         Assert.AreEqual(4, tracker.Count);
         Assert.AreEqual(delivery3, tracker[delivery3.DeliveryId]);
         Assert.AreEqual(4, tracker.Count);
      }

      [Test]
      public void TestAddLargeSeriesOfDeliveriesAndThenEnumerateOverThemWithGet()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         uint COUNT = 4080;

         for (uint i = 0; i < COUNT; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(COUNT, tracker.Count);

         for (uint i = 0; i < COUNT; ++i)
         {
            Assert.AreEqual(i, tracker[i].DeliveryId);
         }
      }

      [Test]
      public void TestAddLargeSeriesOfDeliveriesAndThenIterateOverThemWithValues()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         uint COUNT = 4080;

         for (uint i = 0; i < COUNT; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(COUNT, tracker.Count);

         ICollection<DeliveryType> values = tracker.Values;

         uint index = 0;

         foreach (DeliveryType delivery in values)
         {
            Assert.AreEqual(index++, delivery.DeliveryId);
         }

         Assert.AreEqual(index, COUNT);
      }

      [Test]
      public void TestForEachEntry()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] inputValues = { 3, 0, uint.MaxValue, 1, uint.MaxValue - 1, 2 };

         foreach (uint entry in inputValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         int index = 0;

         tracker.ForEach((value) =>
         {
            int i = index++;
            Assert.AreEqual(new DeliveryType(inputValues[i]), value);
         });

         Assert.AreEqual(index, inputValues.Length);
      }

      [Test]
      public void TestForEachDeliveryIteratesOverLargeSeriesOfDeliveries()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         int COUNT = 4080;

         for (uint i = 0; i < COUNT; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(COUNT, tracker.Count);

         int index = 0;

         tracker.ForEach((delivery) => index++);

         Assert.AreEqual(index, COUNT);
      }

      [Test]
      public void TestRangedForEachDeliveryIteratesOverSmallSeriesOfDeliveries()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         int COUNT = 512;

         for (uint i = 0; i < COUNT; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(COUNT, tracker.Count);

         int index = 0;

         tracker.ForEach(260, 262, (delivery) => index++);

         Assert.AreEqual(index, 3);
      }

      [Test]
      public void TestRangedForEachDeliveryIteratesSeriesWhenValuesOverflowIntRange()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         Assert.AreEqual(0, tracker.Count);

         uint[] elements = new uint[] { 0, 1, int.MaxValue, (uint)int.MaxValue + 1, (uint)int.MaxValue + 2, (uint)int.MaxValue + 3, (uint)int.MaxValue + 4};

         foreach (uint value in elements)
         {
            tracker.Add(value, new DeliveryType(value));
         }

         int index = 0;

         tracker.ForEach(int.MaxValue, (uint) int.MaxValue + 2, (delivery) => index++);

         Assert.AreEqual(index, 3);
      }

      [Test]
      public void TestForEachWhereLastValueNotPresentButGreaterValuesAre()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();
         const int NUM_ENTRIES = 100;

         map.Add(0, new DeliveryType(0));

         for (uint i = 0, j = 512; i < NUM_ENTRIES; ++i, j += 25)
         {
            map.Add(j, new DeliveryType(j));
         }

         map.Add(65534, new DeliveryType(65534));
         map.Add(65536, new DeliveryType(65536));
         map.Add(65537, new DeliveryType(65537));

         map.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.AreEqual(NUM_ENTRIES + 5, map.Count);

         int traversed = 0;

         map.ForEach(0, 65535, (delivery) => traversed++);

         Assert.AreEqual(NUM_ENTRIES + 2, traversed);
      }

      [Test]
      public void TestForEachWhereLastNotPresentAndNextValuesAreOverflow()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();
         const int NUM_ENTRIES = 1;

         map.Add(0, new DeliveryType(0));

         for (uint i = 0, j = 512; i < NUM_ENTRIES; ++i, j += 25)
         {
            map.Add(j, new DeliveryType(j));
         }

         map.Add(65534, new DeliveryType(65534));

         map.Add(0, new DeliveryType(0));
         map.Add(1, new DeliveryType(1));
         map.Add(2, new DeliveryType(2));

         Assert.AreEqual(NUM_ENTRIES + 5, map.Count);

         int traversed = 0;

         map.ForEach(0, 65535, (delivery) => traversed++);

         Assert.AreEqual(NUM_ENTRIES + 5, traversed);
      }

      [Test]
      public void TestValuesCollection()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(2, new DeliveryType(2));
         tracker.Add(3, new DeliveryType(3));

         ICollection<DeliveryType> values = tracker.Values;
         Assert.IsNotNull(values);
         Assert.AreEqual(4, values.Count);
         Assert.IsFalse(values.Count == 0);
         Assert.AreSame(values, tracker.Values);
      }

      [Test]
      public void TestValuesIteration()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] intValues = { 0, 1, 2, 3 };

         foreach (uint entry in intValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         ICollection<DeliveryType> values = tracker.Values;
         IEnumerator<DeliveryType> iterator = values.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         uint counter = 0;

         do
         {
            Assert.AreEqual(new DeliveryType(intValues[counter++]), iterator.Current);
         }
         while (iterator.MoveNext());

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);
      }

      [Test]
      public void TestValuesIterationAfterReset()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();

         uint[] intValues = { 0u, 1u, 2u, 3u };

         foreach (uint entry in intValues)
         {
            map.Add(entry, new DeliveryType(entry));
         }

         ICollection<DeliveryType> values = map.Values;
         IEnumerator<DeliveryType> iterator = values.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;

         while (iterator.MoveNext())
         {
            Assert.AreEqual(new DeliveryType(intValues[counter++]), iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);

         counter = 0;
         iterator.Reset();

         while (iterator.MoveNext())
         {
            Assert.AreEqual(new DeliveryType(intValues[counter++]), iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);
      }

      [Test]
      public void TestValuesIterationFollowUnsignedOrderingExpectations()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] inputValues = { 3, 0, uint.MaxValue, 1, uint.MaxValue - 1, 2 };

         foreach (uint entry in inputValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         ICollection<DeliveryType> values = tracker.Values;
         IEnumerator<DeliveryType> iterator = values.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         int counter = 0;

         do
         {
            Assert.AreEqual(new DeliveryType(inputValues[counter++]), iterator.Current);
         }
         while (iterator.MoveNext());

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestValuesIterationFailsWhenConcurrentlyModified()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] inputValues = { 1, 2, 3, 5, 7, 9, 11 };

         foreach (uint entry in inputValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         ICollection<DeliveryType> values = tracker.Values;
         IEnumerator<DeliveryType> iterator = values.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         tracker.Remove(3);

         try
         {
            iterator.MoveNext();
            Assert.Fail("Should not iterate when modified outside of iterator");
         }
         catch (InvalidOperationException) {}
      }

      [Test]
      public void TestValuesIterationOnEmptyTree()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         ICollection<DeliveryType> values = tracker.Values;
         IEnumerator<DeliveryType> iterator = values.GetEnumerator();

         Assert.IsFalse(iterator.MoveNext());

         try
         {
            _ = iterator.Current;
            Assert.Fail("Should have thrown a exception");
         }
         catch (InvalidOperationException) { }
      }

      [Test]
      public void TestKeySetReturned()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(2, new DeliveryType(2));
         tracker.Add(3, new DeliveryType(3));

         ICollection<uint> keys = tracker.Keys;
         Assert.IsNotNull(keys);
         Assert.AreEqual(4, keys.Count);
         Assert.AreSame(keys, tracker.Keys);
      }

      [Test]
      public void TestKeysIteration()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] intValues = { 0, 1, 2, 3 };

         foreach (uint entry in intValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         ICollection<uint> keys = tracker.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         uint counter = 0;

         do
         {
            Assert.AreEqual(intValues[counter++], iterator.Current);
         }
         while (iterator.MoveNext());

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);
         Assert.AreEqual(intValues.Length, tracker.Count);
      }

      [Test]
      public void TestKeysIterationFollowsUnsignedOrderingExpectations()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] inputValues = { 3, 0, uint.MaxValue, 1, uint.MaxValue - 1, 2 };

         foreach (uint entry in inputValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         ICollection<uint> keys = tracker.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         int counter = 0;

         do
         {
            Assert.AreEqual(inputValues[counter++], iterator.Current);
         }
         while (iterator.MoveNext());

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestKeysIterationFailsWhenConcurrentlyModified()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] inputValues = { 1, 3, 5, 7, 9, 11, 13 };

         foreach (uint entry in inputValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         ICollection<uint> keys = tracker.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         tracker.Remove(3);

         try
         {
            iterator.MoveNext();
            Assert.Fail("Should not iterate when modified outside of iterator");
         }
         catch (InvalidOperationException) { }
      }

      [Test]
      public void TesKeysIterationOnEmptyTree()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         ICollection<uint> values = tracker.Keys;
         IEnumerator<uint> iterator = values.GetEnumerator();

         Assert.IsFalse(iterator.MoveNext());

         try
         {
            _ = iterator.Current;
            Assert.Fail("Should have thrown a exception");
         }
         catch (InvalidOperationException) { }
      }

      [Test]
      public void TestKeyValuePairEnumerator()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));
         tracker.Add(2, new DeliveryType(2));
         tracker.Add(3, new DeliveryType(3));

         IEnumerator<KeyValuePair<uint, DeliveryType>> entries = tracker.GetEnumerator();
         Assert.IsNotNull(entries);
         Assert.IsTrue(entries.MoveNext());
      }

      [Test]
      public void TestKeyValuePairEnumeratorIteration()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] intValues = { 0, 1, 2, 3 };

         foreach (uint entry in intValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         IEnumerator<KeyValuePair<uint, DeliveryType>> iterator = tracker.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         uint counter = 0;

         do
         {
            KeyValuePair<uint, DeliveryType> entry = iterator.Current;
            Assert.IsNotNull(entry);
            Assert.AreEqual(intValues[counter], entry.Key);
            Assert.AreEqual(new DeliveryType(intValues[counter++]), entry.Value);
         }
         while (iterator.MoveNext());

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);
      }

      [Test]
      public void TestEntryIterationFollowsInsertionOrderingExpectations()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] inputValues = { 3, 0, uint.MaxValue, 1, uint.MaxValue - 1, 2 };

         foreach (uint entry in inputValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         IEnumerator<KeyValuePair<uint, DeliveryType>> iterator = tracker.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         uint counter = 0;

         do
         {
            KeyValuePair<uint, DeliveryType> entry = iterator.Current;
            Assert.IsNotNull(entry);
            Assert.AreEqual(inputValues[counter], entry.Key);
            Assert.AreEqual(new DeliveryType(inputValues[counter++]), entry.Value);
         }
         while (iterator.MoveNext());

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestEntryIterationFailsWhenConcurrentlyModified()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] inputValues = { 2, 3, 5, 9, 12, 42 };

         foreach (uint entry in inputValues)
         {
            tracker.Add(entry, new DeliveryType(entry));
         }

         IEnumerator<KeyValuePair<uint, DeliveryType>> iterator = tracker.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         tracker.Remove(3);

         try
         {
            iterator.MoveNext();
            Assert.Fail("Should not iterate when modified outside of iterator");
         }
         catch (InvalidOperationException) { }
      }

      [Test]
      public void TestEntrySetIterationOnEmptyTree()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();
         IEnumerator<KeyValuePair<uint, DeliveryType>> iterator = tracker.GetEnumerator();

         Assert.IsFalse(iterator.MoveNext());

         try
         {
            _ = iterator.Current;
            Assert.Fail("Should have thrown a exception");
         }
         catch (InvalidOperationException) {}
      }

      [Test]
      public void TestRandomProduceAndConsumeWithBacklog()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         int ITERATIONS = 8192;

         try
         {
            for (uint i = 0; i < ITERATIONS; ++i)
            {
               tracker.Add(i, new DeliveryType(i));
            }

            for (uint i = 0; i < ITERATIONS; ++i)
            {
               uint p = (uint)random.Next(ITERATIONS);
               uint c = (uint)random.Next(ITERATIONS);

               tracker.Add(p, new DeliveryType(p));
               tracker.Remove(c);
            }
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestRandomProduceAndConsumeWithBacklogAndCustomSize()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(10, 64);

         int ITERATIONS = 8192;

         try
         {
            for (uint i = 0; i < ITERATIONS; ++i)
            {
               tracker.Add(i, new DeliveryType(i));
            }

            for (uint i = 0; i < ITERATIONS; ++i)
            {
               uint p = (uint)random.Next(ITERATIONS);
               uint c = (uint)random.Next(ITERATIONS);

               tracker.Add(p, new DeliveryType(p));
               tracker.Remove(c);
            }
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestRandomPutAndRemoveIntoEmptyMap()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         int ITERATIONS = 8192;

         try
         {
            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint p = (uint)random.Next(ITERATIONS);
               uint c = (uint)random.Next(ITERATIONS);

               tracker.Add(p, new DeliveryType(p));
               tracker.Remove(c);
            }
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestRandomPutAndRemoveIntoEmptyMapWithCustomBucketSize()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(10, 64);

         int ITERATIONS = 8192;

         try
         {
            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint p = (uint)random.Next(ITERATIONS);
               uint c1 = (uint)random.Next(ITERATIONS);
               uint c2 = (uint)random.Next(ITERATIONS);

               tracker.Add(p, new DeliveryType(p));
               tracker.Remove(c1);
               tracker.Remove(c2);
            }
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestRandomPutAndGetIntoEmptyMapWithCustomBucketSize()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(2, 4);

         int ITERATIONS = 8192;

         try
         {
            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint p = (uint)random.Next(ITERATIONS);
               uint c = (uint)random.Next(ITERATIONS);

               tracker.Add(p, new DeliveryType(p));
               tracker.Remove(c);
            }
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestPutEntriesWithDuplicateIdsIntoMapThenRemoveInSameOrder()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] ids = new uint[] { 4, 7, 5, 0, 7, 7, 0 };

         foreach (uint id in ids)
         {
            tracker.Add(id, new DeliveryType(id));
         }

         foreach (uint id in ids)
         {
            Assert.AreEqual(new DeliveryType(id), tracker[id]);
         }

         foreach (uint id in ids)
         {
            Assert.IsTrue(tracker.Remove(id));
         }

         Assert.IsTrue(tracker.IsEmpty);
      }

      [Test]
      public void TestPutThatBreaksOrderLeavesMapUsable()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint[] ids = new uint[] { 1, 2, 0 };

         foreach (uint id in ids)
         {
            tracker.Add(id, new DeliveryType(id));
         }

         foreach (uint id in ids)
         {
            Assert.AreEqual(new DeliveryType(id), tracker[id]);
         }

         foreach (uint id in ids)
         {
            Assert.IsTrue(tracker.Remove(id));
         }

         Assert.IsTrue(tracker.IsEmpty);
      }

      [Test]
      public void TestPutInSeriesAndRemoveAllValuesRandomly()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         List<uint> values = new List<uint>();
         List<uint> removes = new List<uint>();

         int ITERATIONS = 8192;

         for (uint i = 0; i < ITERATIONS; ++i)
         {
            values.Add(i);
         }

         removes.AddRange(values);
         Shuffle(removes, random);

         try
         {
            foreach (uint id in values)
            {
               tracker.Add(id, new DeliveryType(id));
            }

            Assert.AreEqual(ITERATIONS, tracker.Count);

            foreach (uint id in values)
            {
               Assert.AreEqual(new DeliveryType(id), tracker[id]);
            }

            foreach (uint id in removes)
            {
               Assert.IsTrue(tracker.Remove(id));
            }

            Assert.IsTrue(tracker.IsEmpty);
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestPutInRandomOrderAndRemoveAllValuesInSeries()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         List<uint> values = new List<uint>();
         List<uint> removes = new List<uint>();

         int ITERATIONS = 8192;

         for (uint i = 0; i < ITERATIONS; ++i)
         {
            values.Add(i);
         }

         removes.AddRange(values);
         Shuffle(values, random);

         try
         {
            foreach (uint id in values)
            {
               tracker.Add(id, new DeliveryType(id));
            }

            Assert.AreEqual(ITERATIONS, tracker.Count);

            foreach (uint id in values)
            {
               Assert.AreEqual(new DeliveryType(id), tracker[id]);
            }

            foreach (uint id in removes)
            {
               Assert.IsTrue(tracker.Remove(id));
            }

            Assert.IsTrue(tracker.IsEmpty);
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestPutInRandomOrderAndRemoveAllValuesInRandomOrder()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         List<uint> values = new List<uint>();
         List<uint> removes = new List<uint>();

         int ITERATIONS = 8192;

         for (uint i = 0; i < ITERATIONS; ++i)
         {
            values.Add(i);
         }

         removes.AddRange(values);
         Shuffle(values, random);
         Shuffle(removes, random);

         try
         {
            foreach (uint id in values)
            {
               tracker.Add(id, new DeliveryType(id));
            }

            Assert.AreEqual(ITERATIONS, tracker.Count);

            foreach (uint id in values)
            {
               Assert.AreEqual(new DeliveryType(id), tracker[id]);
            }

            foreach (uint id in removes)
            {
               Assert.IsTrue(tracker.Remove(id));
            }

            Assert.IsTrue(tracker.IsEmpty);
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestPutRandomValueIntoMapThenRemoveInSameOrder()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         int ITERATIONS = 8192;

         try
         {
            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint index = (uint)random.Next(ITERATIONS);
               tracker.Add(index, new DeliveryType(index));
            }

            // Reset to verify insertions
            random = new Random(seed);

            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint index = (uint)random.Next(ITERATIONS);
               Assert.AreEqual(new DeliveryType(index), tracker[index]);
            }

            // Reset to remove
            random = new Random(seed);

            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint index = (uint)random.Next(ITERATIONS);
               Assert.IsTrue(tracker.Remove(index));
            }

            Assert.IsTrue(tracker.IsEmpty);
         }
         catch (Exception error)
         {
            DumpRandomDataSet(ITERATIONS, seed, true);
            throw error;
         }
      }

      [Test]
      public void TestPutInSeriesAndClear()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         int LOOPS = 16;
         int ITERATIONS = 8192;

         uint putDeliveryId = 0;
         uint getDeliveryId = 0;

         for (int loop = 0; loop < LOOPS; loop++)
         {
            try
            {
               for (int i = 0; i < ITERATIONS; ++i, putDeliveryId++)
               {
                  tracker.Add(putDeliveryId, new DeliveryType(putDeliveryId));
               }

               for (int i = 0; i < ITERATIONS; ++i, getDeliveryId++)
               {
                  Assert.AreEqual(new DeliveryType(getDeliveryId), tracker[getDeliveryId]);
               }

               tracker.Clear();

               Assert.IsTrue(tracker.IsEmpty);
            }
            catch (Exception error)
            {
               DumpRandomDataSet(ITERATIONS, seed, true);
               throw error;
            }
         }
      }

      [Test]
      public void TestPutInSeriesAndRemoveInSeries()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         int LOOPS = 16;
         int ITERATIONS = 8192;

         uint putDeliveryId = 0;
         uint getDeliveryId = 0;
         uint removeDeliveryId = 0;

         for (int loop = 0; loop < LOOPS; loop++)
         {
            try
            {
               for (int i = 0; i < ITERATIONS; ++i, putDeliveryId++)
               {
                  tracker.Add(putDeliveryId, new DeliveryType(putDeliveryId));
               }

               for (int i = 0; i < ITERATIONS; ++i, getDeliveryId++)
               {
                  Assert.AreEqual(new DeliveryType(getDeliveryId), tracker[getDeliveryId]);
               }

               for (int i = 0; i < ITERATIONS; ++i, removeDeliveryId++)
               {
                  Assert.IsTrue(tracker.Remove(removeDeliveryId));
               }

               Assert.IsTrue(tracker.IsEmpty);
            }
            catch (Exception error)
            {
               DumpRandomDataSet(ITERATIONS, seed, true);
               throw error;
            }
         }
      }

      [Test]
      public void TestValues()
      {
         ICollection<DeliveryType> vals = tracker.Values;
         _ = vals.GetEnumerator();
         Assert.AreEqual(vals.Count, objArray.Length, "Returned collection of incorrect size");
         foreach (DeliveryType element in objArray)
         {
            Assert.IsTrue(vals.Contains(element), "Collection contains incorrect elements");
         }

         Assert.AreEqual(1000, vals.Count);
         int j = 0;
         for (IEnumerator<DeliveryType> iter = vals.GetEnumerator(); iter.MoveNext(); j++)
         {
            DeliveryType element = iter.Current;
            Assert.IsNotNull(element);
         }
         Assert.AreEqual(1000, j);

         UnsettledDictionary<DeliveryType> myMap = new UnsettledDictionary<DeliveryType>(d => d.DeliveryId);
         for (int i = 0; i < 100; i++)
         {
            myMap.Add(uintArray[i], objArray[i]);
         }
         ICollection<DeliveryType> values = myMap.Values;
         Assert.AreEqual(100, values.Count);
         Assert.IsTrue(values.Remove(new DeliveryType(1)));
         Assert.IsTrue(!myMap.ContainsKey(1), "Removing from the values collection should remove from the original map");
         Assert.IsTrue(!myMap.ContainsValue(new DeliveryType(1)), "Removing from the values collection should remove from the original map");
         Assert.AreEqual(99, values.Count);
         j = 0;
         for (IEnumerator<DeliveryType> iter = values.GetEnumerator(); iter.MoveNext(); j++)
         {
            _ = iter.Current;
         }

         Assert.AreEqual(99, j);
      }

      [Test]
      public void TestRemoveRangeRemovesNoValues()
      {
         uint afterLast = (uint)(uintArray.Length + 1); // Entries are one based
         bool removed = false;

         tracker.RemoveEach(afterLast, afterLast + 10, (delivery) => removed = true);

         Assert.IsFalse(removed);
      }

      [Test]
      public void TestRemoveRangeRemovesLastValue()
      {
         uint lastEntry = (uint)uintArray.Length; // Entries are one based
         int removed = 0;

         tracker.RemoveEach(lastEntry, lastEntry, (delivery) => removed++);

         Assert.AreEqual(1, removed);
      }

      [Test]
      public void TestRemoveRangeRemovesLastValueAndRangeOutsideOfActualEntries()
      {
         uint lastEntry = (uint)uintArray.Length; // Entries are one based
         int removed = 0;

         tracker.RemoveEach(lastEntry, lastEntry + 10, (delivery) => removed++);

         Assert.AreEqual(1, removed);
      }

      [Test]
      public void TestRemoveAllEntriesFromFirstBucket()
      {
         DoTestRemoveEach(0, 15);
      }

      [Test]
      public void TestRemoveAllEntriesFromMiddleBucket()
      {
         DoTestRemoveEach(16, 31);
      }

      [Test]
      public void TestRemoveAllEntriesFromEndBucket()
      {
         DoTestRemoveEach(32, 47);
      }

      [Test]
      public void TestRemoveEntriesSpanningThreeBuckets()
      {
         DoTestRemoveEach(8, 39);
      }

      [Test]
      public void TestRemoveAllEntriesWithClosedRange()
      {
         DoTestRemoveEach(0, 47);
      }

      [Test]
      public void TestRemoveAllEntriesWithOpenRange()
      {
         DoTestRemoveEach(0, 64);
      }

      public void DoTestRemoveEach(uint start, uint end)
      {
         int numBuckets = 3;
         int bucketSize = 16;
         int numEntries = numBuckets * bucketSize;
         int numRemoved = (int)Math.Min(end - start + 1, numEntries);

         UnsettledDictionary<DeliveryType> map = CreateMap(numBuckets, bucketSize);

         for (uint i = 0; i < numEntries; ++i)
         {
            map.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(numEntries, map.Count);

         int removed = 0;

         map.RemoveEach(start, end, (delivery) => removed++);

         Assert.AreEqual(numRemoved, removed);
         Assert.AreEqual(numEntries - numRemoved, map.Count);
      }

      [Test]
      public void TestRemoveAllEntriesInSmallChunks()
      {
         int removed = 0;

         for (int i = 0; i < uintArray.Length; i += 2)
         {
            tracker.RemoveEach(uintArray[i], uintArray[i + 1], (delivery) => removed++);
         }

         Assert.AreEqual(uintArray.Length, removed);
      }

      [Test]
      public void TestRemoveEachWhereLastValueNotPresentButGreaterValuesAre()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();
         const int NUM_ENTRIES = 100;

         map.Add(0, new DeliveryType(0));

         for (uint i = 0, j = 512; i < NUM_ENTRIES; ++i, j += 25)
         {
            map.Add(j, new DeliveryType(j));
         }

         map.Add(65534, new DeliveryType(65534));
         map.Add(65536, new DeliveryType(65536));
         map.Add(65537, new DeliveryType(65537));

         map.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.AreEqual(NUM_ENTRIES + 5, map.Count);

         int removed = 0;

         map.RemoveEach(0, 65535, (delivery) => removed++);

         Assert.AreEqual(NUM_ENTRIES + 2, removed);
         Assert.AreEqual(3, map.Count);
      }

      [Test]
      public void TestRemoveEachWhereLastNotPresentAndNextValuesAreOverflow()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();
         const int NUM_ENTRIES = 100;

         map.Add(0, new DeliveryType(0));

         for (uint i = 0, j = 512; i < NUM_ENTRIES; ++i, j += 25)
         {
            map.Add(j, new DeliveryType(j));
         }

         map.Add(65534, new DeliveryType(65534));

         map.Add(0, new DeliveryType(0));
         map.Add(1, new DeliveryType(1));
         map.Add(2, new DeliveryType(2));

         Assert.AreEqual(NUM_ENTRIES + 5, map.Count);

         int removed = 0;

         map.RemoveEach(0, 65535, (delivery) => removed++);

         Assert.AreEqual(NUM_ENTRIES + 5, removed);
         Assert.AreEqual(0, map.Count);
      }

      [Test]
      public void TestRemoveEachWithRangeMuchLargerThanContainedEntries()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();
         int NUM_ENTRIES = 100;

         map.Add(0, new DeliveryType(0));

         for (uint i = 0, j = 512; i < NUM_ENTRIES; ++i, j += 25)
         {
            map.Add(j, new DeliveryType(j));
         }

         map.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.AreEqual(NUM_ENTRIES + 2, map.Count);

         int removed = 0;

         map.RemoveEach(0, uint.MaxValue, (delivery) => removed++);

         Assert.AreEqual(NUM_ENTRIES + 2, removed);
         Assert.AreEqual(0, map.Count);
      }

      [Test]
      public void TestRemoveEachEntireDeliveryIdRange()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();
         uint NUM_ENTRIES = ushort.MaxValue;

         for (uint i = 0; i < NUM_ENTRIES; ++i)
         {
            map.Add(i, new DeliveryType(i));
         }

         int removed = 0;

         map.RemoveEach(0, NUM_ENTRIES, (delivery) => removed++);

         Assert.AreEqual(NUM_ENTRIES, removed);
         Assert.AreEqual(0, map.Count);
      }

      [Test]
      public void TestRemoveEachWithRangeThatMatchesMany()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap(5, 50);

         uint[] entries = new uint[] { 512, 513, 512, 513, 512, 513 };

         foreach (uint i in entries)
         {
            map.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(entries.Length, map.Count);

         int removed = 0;

         map.RemoveEach(512, 513, (delivery) => removed++);

         Assert.AreEqual(2, removed);
         Assert.AreEqual(entries.Length - 2, map.Count);
      }

      [Test]
      public void TestForEachWithRangeMuchLargerThanContainedEntries()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();
         int NUM_ENTRIES = 100;

         map.Add(0, new DeliveryType(0));

         for (uint i = 0, j = 512; i < NUM_ENTRIES; ++i, j += 25)
         {
            map.Add(j, new DeliveryType(j));
         }

         map.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));

         Assert.AreEqual(NUM_ENTRIES + 2, map.Count);

         int traversed = 0;

         map.ForEach(0, uint.MaxValue, (delivery) => traversed++);

         Assert.AreEqual(NUM_ENTRIES + 2, traversed);
         Assert.AreEqual(NUM_ENTRIES + 2, map.Count);
      }

      [Test]
      public void TestForEachWithRangeThatWrapped()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap(5, 50);

         uint[] entries = new uint[] { 512, 513, 0, 1, 2, 3, uint.MaxValue };

         foreach (uint i in entries)
         {
            map.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(entries.Length, map.Count);

         int traversed = 0;

         map.ForEach(0, uint.MaxValue, (delivery) => traversed++);

         Assert.AreEqual(entries.Length, traversed);
         Assert.AreEqual(entries.Length, map.Count);
      }

      [Test]
      public void TestForEachCoversElementsInBetweenGivenRangeInOtherBuckets()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap(5, 10);
         const int NUM_ENTRIES = 100;

         for (uint i = 0, j = 512; i < NUM_ENTRIES; ++i, j += 25)
         {
            map.Add(j, new DeliveryType(j));
         }

         Assert.AreEqual(NUM_ENTRIES, map.Count);

         int traversed = 0;

         map.ForEach(0, uint.MaxValue, (delivery) => traversed++);

         Assert.AreEqual(NUM_ENTRIES, traversed);
         Assert.AreEqual(NUM_ENTRIES, map.Count);
      }

      [Test]
      public void TestForEachFindsNoValues()
      {
         uint afterLast = (uint)(uintArray.Length + 1); // Entries are one based
         bool traversed = false;

         tracker.ForEach(afterLast, afterLast + 10, (delivery) => traversed = true);

         Assert.IsFalse(traversed);
      }

      [Test]
      public void TestForEachRangeFindsOnlyLastValue()
      {
         uint lastEntry = (uint)uintArray.Length; // Entries are one based
         int traversed = 0;

         tracker.ForEach(lastEntry, lastEntry, (delivery) => traversed++);

         Assert.AreEqual(1, traversed);
      }

      [Test]
      public void TestForEachRangedLastValueAndRangeOutsideOfActualEntries()
      {
         uint lastEntry = (uint)uintArray.Length; // Entries are one based
         int traversed = 0;

         tracker.ForEach(lastEntry, lastEntry + 10, (delivery) => traversed++);

         Assert.AreEqual(1, traversed);
      }

      [Test]
      public void TestForEachWithRangeThatMatchesMany()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap(5, 50);
         uint[] entries = new uint[] { 512, 513, 512, 513, 512, 513 };

         foreach (uint i in entries)
         {
            map.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(entries.Length, map.Count);

         int traversed = 0;

         map.ForEach(512, 513, (delivery) => traversed++);

         Assert.AreEqual(2, traversed);
         Assert.AreEqual(entries.Length, map.Count);
      }

      [Test]
      public virtual void TestCopyDictionaryToArray()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, new DeliveryType(entry));
         }

         KeyValuePair<uint, DeliveryType>[] array = new KeyValuePair<uint, DeliveryType>[map.Count + 1];
         map.CopyTo(array, 1);

         int counter = 0;
         for (; counter < map.Count; counter++)
         {
            Assert.AreEqual(inputValues[counter], array[counter + 1].Key);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestCopyDictionaryKeysToArray()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, new DeliveryType(entry));
         }

         uint[] array = new uint[map.Count + 1];
         map.Keys.CopyTo(array, 1);

         int counter = 0;
         for (; counter < map.Count; counter++)
         {
            Assert.AreEqual(inputValues[counter], array[counter + 1]);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestCopyDictionaryValuesToArray()
      {
         UnsettledDictionary<DeliveryType> map = CreateMap();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, new DeliveryType(entry));
         }

         DeliveryType[] array = new DeliveryType[map.Count + 1];
         map.Values.CopyTo(array, 1);

         int counter = 0;
         for (; counter < map.Count; counter++)
         {
            Assert.AreEqual(new DeliveryType(inputValues[counter]), array[counter + 1]);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestRemoveFromMiddleBucket()
      {
         const int numBuckets = 5;
         const int bucketSize = 10;
         const int numEntries = numBuckets * bucketSize;

         UnsettledDictionary<DeliveryType> map = CreateMap(numBuckets, bucketSize);

         for (uint i = 0; i < numEntries; ++i)
         {
            map.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(numEntries, map.Count);

         uint position = bucketSize + (bucketSize / 2);
         uint lastValue = 0;
         DeliveryType lastDelivery = null;

         // Remove from center of bucket two into bucket three until a compaction event should occur.
         for (uint i = 0; i < bucketSize + 3; ++i)
         {
            Assert.IsTrue(map.TryGetValue(position, out lastDelivery));
            lastValue = lastDelivery.DeliveryId;
            Assert.IsTrue(map.Remove(position++));
         }

         Assert.IsTrue(map.TryGetValue(position, out lastDelivery));
         Assert.AreEqual(lastValue + 1, lastDelivery.DeliveryId);
      }

      [Test]
      public void TestRepeatedRemoveOldestHotPath()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         const int COUNT = 10000;

         for (uint i = 0; i < COUNT; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         for (uint i = 0; i < COUNT; i++)
         {
            Assert.IsTrue(tracker.Remove(i, out DeliveryType removed));
            Assert.IsNotNull(removed);
            Assert.AreEqual(i, removed.DeliveryId);
         }

         Assert.IsTrue(tracker.IsEmpty);
      }

      [Test]
      public void TestRemoveOldestAcrossBucketBoundaries()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(3, 4);

         for (uint i = 0; i < 12; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         for (uint i = 0; i < 12; i++)
         {
            Assert.IsTrue(tracker.Remove(i, out DeliveryType removed));
            Assert.IsNotNull(removed);
            Assert.AreEqual(i, removed.DeliveryId);
         }

         Assert.AreEqual(0, tracker.Count);
      }

      [Test]
      public void TestSlidingWindowPutRemoveOldest()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint window = 1024;

         for (uint i = 0; i < window; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         for (uint i = 0; i < 5000; i++)
         {
            tracker.Add(window + i, new DeliveryType(window + i));
            Assert.IsTrue(tracker.Remove(i));
         }

         Assert.AreEqual(window, tracker.Count);
      }

      [Test]
      public void TestMixedTailAndMiddleRemovals()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         for (uint i = 0; i < 1000; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.IsTrue(tracker.Remove(0));     // tail
         Assert.IsTrue(tracker.Remove(500));   // middle
         Assert.IsTrue(tracker.Remove(1));     // tail again

         Assert.IsFalse(tracker.ContainsKey(0));
         Assert.IsFalse(tracker.ContainsKey(1));
         Assert.IsFalse(tracker.ContainsKey(500));
         Assert.IsTrue(tracker.TryGetValue(499, out DeliveryType result));
      }

      [Test]
      public void TestDuplicateIdsAfterWrap()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         uint nearMax = int.MaxValue - 50;

         for (uint i = 0; i < 100; i++)
         {
            tracker.Add(nearMax + i, new DeliveryType(nearMax + i));
         }

         // Wrap
         for (uint i = 0; i < 100; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         // Validate both ranges exist
         Assert.IsTrue(tracker.TryGetValue(nearMax + 10, out DeliveryType first));
         Assert.IsTrue(tracker.TryGetValue(10, out DeliveryType last));
      }

      [Test]
      public void TestRemoveDuplicateIdRemovesCorrectInstance()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(1, new DeliveryType(1));   // old
                                                // simulate wrap
         tracker.Add(1, new DeliveryType(1));   // new

         tracker.Remove(1);

         // One should still remain
         Assert.IsTrue(tracker.ContainsKey(1));
      }

      [Test]
      public void TestRemoveEachWithNonZeroReadOffset()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(2, 16);

         for (uint i = 0; i < 32; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         for (uint i = 0; i < 5; i++)
         {
            Assert.IsTrue(tracker.Remove(i));
         }

         tracker.RemoveEach(5, 20, d => { });

         for (uint i = 5; i <= 20; i++)
         {
            Assert.IsFalse(tracker.ContainsKey(i));
         }
      }

      [Test]
      public void TestCompactionAtLowWaterMarkBoundary()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(2, 16);

         for (uint i = 0; i < 32; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         for (uint i = 0; i < 12; i++)
         {
            Assert.IsTrue(tracker.Remove(i));
         }

         // Now near low water mark
         Assert.IsTrue(tracker.Remove(12));

         // Validate still consistent
         Assert.IsTrue(tracker.TryGetValue(20, out DeliveryType result));
      }

      [Test]
      public void TestRemoveEachClearsThenReuseMap()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         for (uint i = 0; i < 100; i++)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         tracker.RemoveEach(0, 200, d => { });

         Assert.IsTrue(tracker.IsEmpty);

         tracker.Add(999, new DeliveryType(999));

         Assert.AreEqual(1, tracker.Count);
         Assert.IsTrue(tracker.TryGetValue(999, out DeliveryType result));
      }

      [Test]
      public void TestRecycleBucketWhenTailHasWrapped()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(5, 2);

         // Fill all 5 buckets: bucket0=[0,1], bucket1=[2,3], bucket2=[4,5], bucket3=[6,7], bucket4=[8,9]
         for (uint i = 0; i < 10; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }
         Assert.AreEqual(10, tracker.Count);

         // Remove 0..5 => drains bucket0, bucket1, bucket2 fully (advances tail to bucket3)
         for (uint i = 0; i < 6; ++i)
         {
            Assert.IsTrue(tracker.Remove(i), "Expected to remove existing id: " + i);
         }
         Assert.AreEqual(4, tracker.Count); // remaining: 6,7,8,9

         // Add 10..13:
         // bucket4 is full so putting 10 advances head 4=>0 (wrap) and uses bucket0 again
         // then 12 advances head 0=>1 and uses bucket1 again
         for (uint i = 10; i < 14; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }
         Assert.AreEqual(8, tracker.Count); // now: 6,7,8,9,10,11,12,13

         // Now remove entries that occupy bucket index 0 (10,11) fully.
         // This should recycle bucket index 0 while tail is at 3 and head at 1:
         // tail > head and index(0) < tail(3) =>  else branch in recycleBucket.
         tracker.RemoveEach(10, 11, d => { });

         Assert.AreEqual(6, tracker.Count);
         Assert.IsFalse(tracker.ContainsKey(10));
         Assert.IsFalse(tracker.ContainsKey(11));
         Assert.IsTrue(tracker.ContainsKey(12));
         Assert.IsTrue(tracker.ContainsKey(13));

         // Validate map remains consistent and all remaining values are removable.
         uint[] remaining = { 6, 7, 8, 9, 12, 13 };

         foreach (uint id in remaining)
         {
            Assert.IsTrue(tracker.Remove(id, out DeliveryType removed));
            Assert.IsNotNull(removed, "Expected to remove existing id: " + id);
            Assert.AreEqual(id, removed.DeliveryId);
         }

         Assert.IsTrue(tracker.IsEmpty);
      }

      [Test]
      public void TestRecycleHeadBucketDoesNotLoseEarlierEntries()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(8, 4);  // 8 buckets of 4 entries each

         // Fill exactly 3 buckets: [0..11]. Head should be at the 3rd bucket.
         for (uint i = 0; i < 12; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         Assert.AreEqual(12, tracker.Count);

         // Remove the last bucket's range [8..11] which should fully drain the head bucket,
         // forcing recycleBucket(head).
         tracker.RemoveEach(8, 11, d => { });

         // Now [0..7] must still exist
         Assert.AreEqual(8, tracker.Count);

         for (uint i = 0; i < 8; ++i)
         {
            Assert.IsTrue(tracker.TryGetValue(i, out DeliveryType result1), "Missing entry " + i + " after recycling head bucket");
         }
         for (uint i = 8; i < 12; ++i)
         {
            Assert.IsFalse(tracker.TryGetValue(i, out DeliveryType result2), "Entry " + i + " should have been removed");
         }

         // Ensure map still accepts new writes after head recycling.
         tracker.Add(12, new DeliveryType(12));
         Assert.IsTrue(tracker.TryGetValue(12, out DeliveryType result3));
         Assert.AreEqual(9, tracker.Count);
      }

      [Test]
      public void TestRecycleBucketInWrappedSpanDoesNotCorruptMap()
      {
         // 5 buckets of size 2 => easy to force wrap.
         UnsettledDictionary<DeliveryType> tracker = CreateMap(5, 2);

         // Fill all 5 buckets: IDs 0..9
         for (uint i = 0; i < 10; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         // Drain 0..5 => recycles buckets at the front, tail advances forward
         for (uint i = 0; i < 6; ++i)
         {
            Assert.IsTrue(tracker.Remove(i));
         }

         // Add 10..13 => forces head wrap-around into earlier indices
         for (uint i = 10; i < 14; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         // Now remove a bucket that is *not* tail and not head, but sits in the wrapped portion.
         // This should recycle an internal bucket and still keep all remaining IDs accessible.
         tracker.RemoveEach(10, 11, d => { });

         Assert.IsFalse(tracker.TryGetValue(10, out DeliveryType _));
         Assert.IsFalse(tracker.TryGetValue(11, out DeliveryType _));
         Assert.IsTrue(tracker.TryGetValue(12, out DeliveryType _));
         Assert.IsTrue(tracker.TryGetValue(13, out DeliveryType _));

         // Verify remaining removals are consistent and no entries were lost
         uint[] remaining = { 6, 7, 8, 9, 12, 13 };

         foreach (uint id in remaining)
         {
            Assert.IsTrue(tracker.Remove(id), "Expected to remove remaining id: " + id);
         }

         Assert.IsTrue(tracker.IsEmpty);
      }

      [Test]
      public void TestRemoveFromMiddleDoesNotLoseTailEntriesWhenCompactionTriggered()
      {
         // Use small bucketSize so bucketLowWaterMark is small and compaction is more likely.
         UnsettledDictionary<DeliveryType> tracker = CreateMap(6, 10);  // low-water ≈ 3

         // Fill enough to create multiple buckets.
         for (uint i = 0; i < 60; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         // Make tail bucket sparse (but not empty) so tryCompact(tail) has a chance to do something.
         for (uint i = 0; i < 8; ++i)
         {
            Assert.IsTrue(tracker.Remove(i));
         }

         // Now remove from a middle region (not tail) and ensure the early tail-adjacent IDs remain.
         // If remove incorrectly compacts tail and corrupts the ring, these can disappear.
         Assert.IsTrue(tracker.Remove(25));  // removal from a non-tail bucket

         // Sanity: nearby entries should still exist
         Assert.IsTrue(tracker.TryGetValue(24, out DeliveryType _));
         Assert.IsFalse(tracker.TryGetValue(25, out DeliveryType _));
         Assert.IsTrue(tracker.TryGetValue(26, out DeliveryType _));

         // Tail-adjacent entries (8..15) should still exist
         for (uint i = 8; i < 16; ++i)
         {
            Assert.IsTrue(tracker.TryGetValue(i, out DeliveryType result), "Entry " + i + " missing after middle removal/compaction");
            Assert.AreEqual(result.DeliveryId, i);
         }
      }

      [Test]
      public void TestRecycleHeadWhenTailNotZeroDoesNotCorruptSpan()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(8, 4);

         // Fill 3 buckets worth: 0..11
         for (uint i = 0; i < 12; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         // Drain first bucket completely (0..3) so tail advances away from 0.
         for (uint i = 0; i < 4; ++i)
         {
            Assert.IsTrue(tracker.Remove(i));
         }

         // Now map contains 4..11; tail should no longer be 0 internally.

         // Remove the last bucket worth (8..11) to fully empty the head bucket and force
         // recycleBucket(head).
         tracker.RemoveEach(8, 11, d => { });

         // Remaining should be 4..7 exactly
         Assert.AreEqual(4, tracker.Count);

         for (uint i = 4; i < 8; ++i)
         {
            Assert.IsTrue(tracker.TryGetValue(i, out DeliveryType _), "Missing " + i + " after head recycle with tail != 0");
         }
         for (uint i = 8; i < 12; ++i)
         {
            Assert.IsFalse(tracker.TryGetValue(i, out DeliveryType _), "Expected removed " + i);
         }

         // Continue using the map to ensure head/tail pointers are still consistent.
         tracker.Add(12, new DeliveryType(12));
         tracker.Add(13, new DeliveryType(13));
         Assert.IsTrue(tracker.TryGetValue(12, out DeliveryType _));
         Assert.IsTrue(tracker.TryGetValue(13, out DeliveryType _));
      }

      [Test]
      public void TestRemoveFromNonTailTriggersWrongCompactionAndStillPreservesCorrectness()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(6, 10);

         // Fill 3 buckets: 0..29
         for (uint i = 0; i < 30; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         // Make tail bucket small (remove 0..6 leaves 7..9 in first bucket => 3 entries == low-water)
         for (uint i = 0; i < 7; ++i)
         {
            Assert.IsTrue(tracker.Remove(i));
         }

         // Now remove entries from a *non-tail* bucket to make that bucket sparse too.
         // Removing these should make the target bucket hit <= low-water and trigger the compaction path.
         Assert.IsTrue(tracker.Remove(15));
         Assert.IsTrue(tracker.Remove(16));
         Assert.IsTrue(tracker.Remove(17));

         // If tryCompact(tail) corrupts the map, these will be missing or inconsistent.
         for (uint i = 7; i < 10; ++i)
         {
            Assert.IsTrue(tracker.TryGetValue(i, out DeliveryType _), "Tail-adjacent entry missing after non-tail removal triggered compaction");
         }
         Assert.IsFalse(tracker.TryGetValue(15, out DeliveryType _));
         Assert.IsFalse(tracker.TryGetValue(16, out DeliveryType _));
         Assert.IsFalse(tracker.TryGetValue(17, out DeliveryType _));
         Assert.IsTrue(tracker.TryGetValue(18, out DeliveryType _));
      }

      [Test]
      public void TestRemoveEachSpanningMultipleBucketRecyclesDoesNotSkipBuckets()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap(10, 4);

         // Fill 7 buckets worth => 28 entries: 0..27
         for (uint i = 0; i < 28; ++i)
         {
            tracker.Add(i, new DeliveryType(i));
         }

         // Remove a span that starts mid-bucket and ends exactly at a bucket boundary.
         // Buckets: [0..3],[4..7],[8..11],[12..15],[16..19],[20..23],[24..27]
         // Remove 2..19 covers partial first bucket + full next four buckets.
         tracker.RemoveEach(2, 19, d => { });

         DeliveryType result = null;

         // Keys 2..19 must be gone
         for (uint i = 2; i <= 19; ++i)
         {
            Assert.IsFalse(tracker.TryGetValue(i, out result), "Expected removed " + i);
         }

         // Keys outside range must remain
         for (uint i = 0; i < 2; ++i)
         {
            Assert.IsTrue(tracker.TryGetValue(i, out result), "Unexpectedly missing " + i);
         }
         for (uint i = 20; i < 28; ++i)
         {
            Assert.IsTrue(tracker.TryGetValue(i, out result), "Unexpectedly missing " + i);
         }
      }

      [Test]
      public void TestEnumerateBucketsThatHaveWrappedIds()
      {
         UnsettledDictionary<DeliveryType> tracker = CreateMap();

         tracker.Add(uint.MaxValue, new DeliveryType(uint.MaxValue));
         tracker.Add(0, new DeliveryType(0));
         tracker.Add(1, new DeliveryType(1));

         ICollection<uint> keys = tracker.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.IsTrue(iterator.MoveNext());

         uint counter = 0;

         uint[] expected = { uint.MaxValue, 0, 1 };

         do
         {
            Assert.AreEqual(expected[counter++], iterator.Current);
         }
         while (iterator.MoveNext());

         Assert.AreEqual(3, counter);
      }

      private UnsettledDictionary<DeliveryType> CreateMap()
      {
         return new UnsettledDictionary<DeliveryType>(d => d.DeliveryId);
      }

      private UnsettledDictionary<DeliveryType> CreateMap(IDictionary<uint, DeliveryType> other)
      {
         return new UnsettledDictionary<DeliveryType>(d => d.DeliveryId, other);
      }

      private UnsettledDictionary<DeliveryType> CreateMap(int numBuckets, int bucketSize)
      {
         return new UnsettledDictionary<DeliveryType>(d => d.DeliveryId, numBuckets, bucketSize);
      }

      /// <summary>
      /// Simple Delivery Type used throughout these tests
      /// </summary>
      private sealed class DeliveryType
      {
         private readonly uint deliveryId;

         public DeliveryType(uint deliveryId)
         {
            this.deliveryId = deliveryId;
         }

         public uint DeliveryId => deliveryId;

         public override bool Equals(object other)
         {
            if (other is DeliveryType otherType)
            {
               return otherType.deliveryId == deliveryId;
            }

            return false;
         }

         public override int GetHashCode()
         {
            return base.GetHashCode() + (int)deliveryId;
         }

         public override string ToString()
         {
            return "DeliveryType: { " + deliveryId + " }";
         }
      }

      protected void DumpRandomDataSet(int iterations, int seed, bool bounded)
      {
         uint[] dataSet = new uint[iterations];

         random = new Random(seed);

         for (int i = 0; i < iterations; ++i)
         {
            if (bounded)
            {
               dataSet[i] = (uint)random.Next(iterations);
            }
            else
            {
               dataSet[i] = (uint)random.Next();
            }
         }

         Console.WriteLine("Iterations was {0}, Random seed was: {1}", iterations, seed);
         Console.WriteLine("Entries in data set: {0}", string.Join(", ", dataSet));
      }

      public static void Shuffle(List<uint> list, Random random)
      {
         int n = list.Count;

         while (n > 1)
         {
            int k = random.Next(n--);
            uint value = list[k];
            list[k] = list[n];
            list[n] = value;
         }
      }
   }
}