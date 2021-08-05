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

namespace Apache.Qpid.Proton.Utilities
{
   [TestFixture]
   public class SplayedDictionaryTest
   {
      protected int seed;
      protected Random random;

      [SetUp]
      public void Setup()
      {
         seed = Environment.TickCount;
         random = new Random(seed);
      }

      protected virtual SplayedDictionary<K, V> CreateMap<K, V>()
      {
         return new SplayedDictionary<K, V>();
      }

      protected virtual SplayedDictionary<K, V> CreateMap<K, V>(IDictionary<K, V> other)
      {
         return new SplayedDictionary<K, V>(other);
      }

      [Test]
      public void TestComparator()
      {
         SplayedDictionary<string, string> map = CreateMap<string, string>();

         Assert.IsNotNull(map.Comparer);
         Assert.AreSame(map.Comparer, map.Comparer);
      }

      [Test]
      public void TestClear()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         Assert.AreEqual(0, map.Count);

         map.Add(2, "two");
         map.Add(0, "zero");
         map.Add(1, "one");

         Assert.AreEqual(3, map.Count);

         map.Clear();

         Assert.AreEqual(0, map.Count);

         map.Add(5, "five");
         map.Add(9, "nine");
         map.Add(3, "three");
         map.Add(7, "seven");
         map.Add(0, "zero");

         Assert.AreEqual(5, map.Count);

         map.Clear();

         Assert.AreEqual(0, map.Count);

         map.Clear();
      }

      [Test]
      public void TestCount()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         Assert.AreEqual(0, map.Count);
         map.Add(0, "zero");
         Assert.AreEqual(1, map.Count);
         map.Add(1, "one");
         Assert.AreEqual(2, map.Count);
         map[0] = "update";
         Assert.AreEqual(2, map.Count);
         map.Remove(0);
         Assert.AreEqual(1, map.Count);
         map.Remove(1);
         Assert.AreEqual(0, map.Count);
      }

      [Test]
      public void TestInsert()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(2, "two");
         map.Add(3, "three");
         map.Add(5, "five");
         map.Add(9, "nine");
         map.Add(7, "seven");
         map.Add(99, "ninety nine");

         Assert.AreEqual(8, map.Count);
      }

      [Test]
      public void TestAddCannotReplace()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(2, "foo");

         Assert.Throws<ArgumentException>(() => map.Add(2, "two"));

         Assert.AreEqual("foo", map[2]);
      }

      [Test]
      public void TestInsertAndReplace()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(2, "foo");

         map[2] = "two";
         Assert.AreEqual("two", map[2]);

         Assert.AreEqual("zero", map[0]);
         Assert.AreEqual("one", map[1]);
         Assert.AreEqual("two", map[2]);

         Assert.AreEqual(3, map.Count);
      }

      [Test]
      public void TestInsertAndRemove()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(2, "two");

         Assert.AreEqual(3, map.Count);

         Assert.IsFalse(map.Remove(3));
         Assert.IsTrue(map.Remove(0));
         Assert.IsTrue(map.Remove(1));
         Assert.IsTrue(map.Remove(2));

         Assert.AreEqual(0, map.Count);
      }

      [Test]
      public void TestNonGenericInsertAndRemove()
      {
         IDictionary map = CreateMap<uint, string>();

         map.Add(0u, "zero");
         map.Add(1u, "one");
         map.Add(2u, "two");

         Assert.AreEqual(3, map.Count);

         Assert.Throws<InvalidCastException>(() => map.Remove("test"));

         map.Remove(3u);
         map.Remove(0u);
         map.Remove(1u);
         map.Remove(2u);

         Assert.AreEqual(0, map.Count);
      }

      [Test]
      public void TestConstructFromAnotherDictionary()
      {
         IDictionary<uint, string> source = new Dictionary<uint, string>();

         source.Add(0, "zero");
         source.Add(1, "one");
         source.Add(2, "two");
         source.Add(3, "three");
         source.Add(5, "five");
         source.Add(9, "nine");
         source.Add(7, "seven");
         source.Add(1024, "ten-twenty-four");

         SplayedDictionary<uint, string> map = CreateMap<uint, string>(source);

         Assert.AreEqual(8, map.Count);

         Assert.AreEqual("zero", map[0]);
         Assert.AreEqual("one", map[1]);
         Assert.AreEqual("two", map[2]);
         Assert.AreEqual("three", map[3]);
         Assert.AreEqual("five", map[5]);
         Assert.AreEqual("nine", map[9]);
         Assert.AreEqual("seven", map[7]);
         Assert.AreEqual("ten-twenty-four", map[1024]);
      }

      [Test]
      public void TestGetWhenEmpty()
      {
         IDictionary<string, string> source = new Dictionary<string, string>();

         Assert.Throws<KeyNotFoundException>(() =>
         {
            string test = source["test"];
         });
      }

      [Test]
      public void TestIndexedGet()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(1, "one");
         map.Add(10, "ten");
         map.Add(0, "zero");

         Assert.AreEqual("zero", map[0]);
         Assert.AreEqual("one", map[1]);
         Assert.AreEqual("ten", map[10]);

         Assert.Throws<KeyNotFoundException>(() =>
         {
            string test = map[3];
         });

         Assert.AreEqual(3, map.Count);
      }

      [Test]
      public void TestNonGenericIndexedGet()
      {
         IDictionary map = CreateMap<uint, string>();

         map.Add(1u, "one");
         map.Add(10u, "ten");
         map.Add(0u, "zero");

         Assert.AreEqual("zero", map[0u]);
         Assert.AreEqual("one", map[1u]);
         Assert.AreEqual("ten", map[10u]);

         Assert.Throws<KeyNotFoundException>(() =>
         {
            object test = map[3u];
         });

         Assert.AreEqual(3, map.Count);
      }

      [Test]
      public void TestContainsKeyOnEmptyMap()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         Assert.IsFalse(map.ContainsKey(99));
         Assert.IsFalse(map.ContainsKey(0u));
      }

      [Test]
      public void TestContainsKey()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(99, "ninety-nine");

         Assert.IsTrue(map.ContainsKey(0));
         Assert.IsFalse(map.ContainsKey(3));

         Assert.AreEqual(3, map.Count);
      }

      [Test]
      public void TestContainsValue()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(99, "ninety-nine");

         Assert.IsTrue(map.ContainsValue("zero"));
         Assert.IsFalse(map.ContainsValue("four"));

         Assert.AreEqual(3, map.Count);
      }

      [Test]
      public void TestContainsValueOnEmptyMap()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         Assert.IsFalse(map.ContainsValue("0"));
      }

      [Test]
      public void TestRemove()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(9, "nine");
         map.Add(7, "seven");
         map.Add(99, "ninety-nine");

         Assert.AreEqual(5, map.Count);
         Assert.IsFalse(map.Remove(5));
         Assert.AreEqual(5, map.Count);
         Assert.IsTrue(map.Remove(9));
         Assert.AreEqual(4, map.Count);
      }

      [Test]
      public void TestRemoveIsIdempotent()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(2, "two");

         Assert.AreEqual(3, map.Count);

         Assert.IsTrue(map.Remove(0));
         Assert.IsFalse(map.Remove(0));

         Assert.AreEqual(2, map.Count);

         Assert.IsTrue(map.Remove(1));
         Assert.IsFalse(map.Remove(1));

         Assert.AreEqual(1, map.Count);

         Assert.IsTrue(map.Remove(2));
         Assert.IsFalse(map.Remove(2));

         Assert.AreEqual(0, map.Count);
      }

      [Test]
      public void TestRemoveValueNotInMap()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(9, "nine");
         map.Add(7, "seven");
         map.Add(99, "ninety-nine");

         Assert.IsFalse(map.Remove(5));
      }

      [Test]
      public void TestRemoveFirstEntryTwice()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(16, "sixteen");

         Assert.IsTrue(map.Remove(0));
         Assert.IsFalse(map.Remove(0));
      }

      [Test]
      public void TestRemoveEntryWithValue()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         Assert.IsFalse(map.Remove(new KeyValuePair<uint, string>(1, "zero")));

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(9, "nine");
         map.Add(7, "seven");
         map.Add(99, "ninety-nine");

         Assert.AreEqual(5, map.Count);
         Assert.IsFalse(map.Remove(new KeyValuePair<uint, string>(1, "zero")));
         Assert.AreEqual(5, map.Count);
         Assert.IsTrue(map.Remove(new KeyValuePair<uint, string>(1, "one")));
         Assert.AreEqual(4, map.Count);
         Assert.IsFalse(map.Remove(new KeyValuePair<uint, string>(42, "forty-two")));
         Assert.AreEqual(4, map.Count);

         Assert.AreEqual("zero", map[0]);
         Assert.AreEqual("nine", map[9]);
         Assert.AreEqual("seven", map[7]);
         Assert.AreEqual("ninety-nine", map[99]);
      }

      [Test]
      public void TestReplaceValue()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(1, "one");
         map.Add(9, "nine");
         map.Add(0, "zero");
         map.Add(7, "seven");
         map.Add(99, "ninety-nine");

         Assert.AreEqual(5, map.Count);
         Assert.DoesNotThrow(() => map[1] = "one-one");
         Assert.AreEqual(5, map.Count);
         Assert.DoesNotThrow(() => map[42] = "forty-two");
         Assert.AreEqual(6, map.Count);
         Assert.AreEqual("one-one", map[1]);

         Assert.AreEqual(6, map.Count);
         Assert.AreEqual("zero", map[0]);
         Assert.AreEqual("one-one", map[1]);
         Assert.AreEqual("nine", map[9]);
         Assert.AreEqual("seven", map[7]);
         Assert.AreEqual("ninety-nine", map[99]);
      }

      [Test]
      public void TestValuesCollection()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(2, "one");
         map.Add(3, "one");

         ICollection<String> values = map.Values;
         Assert.IsNotNull(values);
         Assert.AreEqual(4, values.Count);
         Assert.AreSame(values, map.Values);
      }

      [Test]
      public void TestValuesIteration()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] intValues = { 0u, 1u, 2u, 3u };

         foreach (uint entry in intValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection<string> values = map.Values;
         IEnumerator<string> iterator = values.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            Assert.AreEqual(intValues[counter++].ToString(), iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);
      }

      [Test]
      public void TestValuesIterationAfterReset()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] intValues = { 0u, 1u, 2u, 3u };

         foreach (uint entry in intValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection<string> values = map.Values;
         IEnumerator<string> iterator = values.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            Assert.AreEqual(intValues[counter++].ToString(), iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);

         counter = 0;
         iterator.Reset();

         while (iterator.MoveNext())
         {
            Assert.AreEqual(intValues[counter++].ToString(), iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(intValues.Length, counter);
      }

      [Test]
      public virtual void TestNonGenericValuesIterationFollowUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 99, 1024 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection values = map.Values;
         IEnumerator iterator = values.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            Assert.AreEqual(expectedOrder[counter++].ToString(), iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestValuesIterationFollowUnsignedOrderingExpectations()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 99, 1024 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection<string> values = map.Values;
         IEnumerator<string> iterator = values.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            Assert.AreEqual(expectedOrder[counter++].ToString(), iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestValuesIterationFailsWhenConcurrentlyModified()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 2, 1, 9, 7 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection<string> values = map.Values;
         IEnumerator<string> iterator = values.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         map.Remove(3);

         Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
      }

      [Test]
      public void TestValuesIterationOnEmptyTree()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();
         ICollection<string> values = map.Values;
         IEnumerator<string> iterator = values.GetEnumerator();

         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });
         Assert.IsFalse(iterator.MoveNext());
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });
         Assert.IsFalse(iterator.MoveNext());
      }

      [Test]
      public void TestKeySetReturned()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         map.Add(0, "zero");
         map.Add(1, "one");
         map.Add(2, "two");
         map.Add(3, "three");

         ICollection<uint> keys = map.Keys;

         Assert.IsNotNull(keys);
         Assert.AreEqual(4, keys.Count);
         Assert.AreSame(keys, map.Keys);
      }

      [Test]
      public void TestKeysIteration()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 0, 1, 2, 3 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection<uint> keys = map.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            Assert.AreEqual(inputValues[counter++], iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestNonGenericKeysIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 55, 1, 47, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 47, 55 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection keys = map.Keys;
         IEnumerator iterator = keys.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            Assert.AreEqual(expectedOrder[counter++], iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestKeysIterationFollowsUnsignedOrderingExpectations()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 55, 1, 47, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 47, 55 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection<uint> keys = map.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();

         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            Assert.AreEqual(expectedOrder[counter++], iterator.Current);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestKeysIterationFailsWhenConcurrentlyModified()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 22, 1, 11, 2 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         ICollection<uint> keys = map.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();

         Assert.IsNotNull(iterator);

         map.Remove(3);

         Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
      }

      [Test]
      public void TestKeysIterationOnEmptyTree()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();
         ICollection<uint> keys = map.Keys;
         IEnumerator<uint> iterator = keys.GetEnumerator();

         Assert.IsFalse(iterator.MoveNext());
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });
      }

      [Test]
      public void TestEntryIteration()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 0, 1, 2, 3 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         IEnumerator<KeyValuePair<uint, string>> iterator = map.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            KeyValuePair<uint, string> entry = iterator.Current;
            Assert.IsNotNull(entry);
            Assert.AreEqual(inputValues[counter], entry.Key);
            Assert.AreEqual(inputValues[counter++].ToString(), entry.Value);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestEntryIterationUsingNonGenericEnumeration()
      {
         IDictionary map = (IDictionary)CreateMap<uint, string>();

         uint[] inputValues = { 0, 1, 2, 3 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         IEnumerator iterator = map.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            KeyValuePair<uint, string> entry = (KeyValuePair<uint, string>)iterator.Current;
            Assert.IsNotNull(entry);
            Assert.AreEqual(inputValues[counter], entry.Key);
            Assert.AreEqual(inputValues[counter++].ToString(), entry.Value);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestEntryIterationFollowsUnsignedOrderingExpectations()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 11, 1, 9, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 9, 11 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         IEnumerator<KeyValuePair<uint, string>> iterator = map.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            KeyValuePair<uint, string> entry = iterator.Current;
            Assert.IsNotNull(entry);
            Assert.AreEqual(expectedOrder[counter], entry.Key);
            Assert.AreEqual(expectedOrder[counter++].ToString(), entry.Value);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestDictionaryEnumeratorIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 11, 1, 9, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 9, 11 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         IDictionaryEnumerator iterator = map.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            DictionaryEntry entry = iterator.Entry;
            Assert.IsNotNull(entry);
            Assert.AreEqual(expectedOrder[counter], entry.Key);
            Assert.AreEqual(expectedOrder[counter++].ToString(), entry.Value);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestNonGenericIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 11, 1, 9, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 9, 11 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         IEnumerator iterator = map.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         int counter = 0;
         while (iterator.MoveNext())
         {
            KeyValuePair<uint, string> entry = (KeyValuePair<uint, string>)iterator.Current;
            Assert.IsNotNull(entry);
            Assert.AreEqual(expectedOrder[counter], entry.Key);
            Assert.AreEqual(expectedOrder[counter++].ToString(), entry.Value);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestEntryIterationFailsWhenConcurrentlyModified()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 11, 1, 9, 2 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         IEnumerator<KeyValuePair<uint, string>> iterator = map.GetEnumerator();
         Assert.IsNotNull(iterator);
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });

         map.Remove(3);

         Assert.Throws<InvalidOperationException>(() => iterator.MoveNext());
      }

      [Test]
      public void TestEntrySetIterationOnEmptyTree()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();
         IEnumerator<KeyValuePair<uint, string>> iterator = map.GetEnumerator();

         Assert.IsFalse(iterator.MoveNext());
         Assert.Throws<InvalidOperationException>(() => { object result = iterator.Current; });
      }

      [Test]
      public virtual void TestCopyDictionaryToArray()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 99, 1024 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         KeyValuePair<uint, string>[] array = new KeyValuePair<uint, string>[map.Count + 1];
         map.CopyTo(array, 1);

         int counter = 0;
         for (; counter < map.Count; counter++)
         {
            Assert.AreEqual(expectedOrder[counter], array[counter + 1].Key);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestCopyDictionaryKeysToArray()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 99, 1024 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         uint[] array = new uint[map.Count + 1];
         map.Keys.CopyTo(array, 1);

         int counter = 0;
         for (; counter < map.Count; counter++)
         {
            Assert.AreEqual(expectedOrder[counter], array[counter + 1]);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public virtual void TestCopyDictionaryValuesToArray()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 0, 1, 2, 3, 99, 1024 };

         foreach (uint entry in inputValues)
         {
            map.Add(entry, entry.ToString());
         }

         string[] array = new string[map.Count + 1];
         map.Values.CopyTo(array, 1);

         int counter = 0;
         for (; counter < map.Count; counter++)
         {
            Assert.AreEqual(expectedOrder[counter].ToString(), array[counter + 1]);
         }

         // Check that we really did iterate.
         Assert.AreEqual(inputValues.Length, counter);
      }

      [Test]
      public void TestRandomProduceAndConsumeWithBacklog()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         const int ITERATIONS = 8192;
         String DUMMY_STRING = "test";

         try
         {
            for (uint i = 0; i < ITERATIONS; ++i)
            {
               map.Add(i, DUMMY_STRING);
            }

            for (uint i = 0; i < ITERATIONS; ++i)
            {
               uint p = (uint)random.Next(ITERATIONS);
               uint c = (uint)random.Next(ITERATIONS);

               map[p] = DUMMY_STRING;
               map.Remove(c);
            }
         }
         catch (Exception)
         {
            DumpRandomDataSet(ITERATIONS, true);
            throw;
         }
      }

      [Test]
      public void TestRandomPutAndGetIntoEmptyMap()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         const int ITERATIONS = 8192;
         String DUMMY_STRING = "test";

         try
         {
            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint p = (uint)random.Next(ITERATIONS);
               uint c = (uint)random.Next(ITERATIONS);

               map[p] = DUMMY_STRING;
               map.Remove(c);
            }
         }
         catch (Exception)
         {
            DumpRandomDataSet(ITERATIONS, true);
            throw;
         }
      }

      [Test]
      public void TestPutRandomValueIntoMapThenRemoveInSameOrder()
      {
         SplayedDictionary<uint, string> map = CreateMap<uint, string>();

         const int ITERATIONS = 8192;

         try
         {
            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint index = (uint)random.Next(ITERATIONS);
               map[index] = index.ToString();
            }

            // Reset to verify insertions
            random = new Random(seed);

            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint index = (uint)random.Next(ITERATIONS);
               Assert.AreEqual(index.ToString(), map[index]);
            }

            // Reset to remove
            random = new Random(seed);

            for (int i = 0; i < ITERATIONS; ++i)
            {
               uint index = (uint)random.Next(ITERATIONS);
               map.Remove(index);
            }

            Assert.AreEqual(0, map.Count);
         }
         catch (Exception)
         {
            DumpRandomDataSet(ITERATIONS, true);
            throw;
         }
      }

      protected void DumpRandomDataSet(int iterations, bool bounded)
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

         Console.WriteLine("Random seed was: {0}", seed);
         Console.WriteLine("Entries in data set: {0}", dataSet);
      }
   }
}