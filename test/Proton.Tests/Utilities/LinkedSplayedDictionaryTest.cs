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
   public class LinkedSplayedDictionaryTest : SplayedDictionaryTest
   {
      protected override LinkedSplayedDictionary<K, V> CreateMap<K, V>()
      {
         return new LinkedSplayedDictionary<K, V>();
      }

      protected override LinkedSplayedDictionary<K, V> CreateMap<K, V>(IDictionary<K, V> other)
      {
         return new LinkedSplayedDictionary<K, V>(other);
      }

      [Test]
      public override void TestValuesIterationFollowUnsignedOrderingExpectations()
      {
         IDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 3, 0, 99, 1, 1024, 2 }; // Linked insertion order

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
      public override void TestNonGenericValuesIterationFollowUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 3, 0, 99, 1, 1024, 2 };  // insertion order

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
      public override void TestKeysIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 55, 1, 47, 2 };
         uint[] expectedOrder = { 3, 0, 55, 1, 47, 2 }; // insertion order

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
      public override void TestNonGenericKeysIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 55, 1, 47, 2 };
         uint[] expectedOrder = { 3, 0, 55, 1, 47, 2 }; // insertion order

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
      public override void TestEntryIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 11, 1, 9, 2, 7, 15, 4 };
         uint[] expectedOrder = { 3, 0, 11, 1, 9, 2, 7, 15, 4 };

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
      public override void TestDictionaryEnumeratorIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 11, 1, 9, 2 };
         uint[] expectedOrder = { 3, 0, 11, 1, 9, 2 }; // insertion order

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
      public override void TestNonGenericIterationFollowsUnsignedOrderingExpectations()
      {
         IDictionary map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 11, 1, 9, 2 };
         uint[] expectedOrder = { 3, 0, 11, 1, 9, 2 }; // insertion order

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
      public override void TestCopyDictionaryToArray()
      {
         IDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 3, 0, 99, 1, 1024, 2 }; // insertion order

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
      public override void TestCopyDictionaryKeysToArray()
      {
         IDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 3, 0, 99, 1, 1024, 2 }; // Insertion order

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
      public override void TestCopyDictionaryValuesToArray()
      {
         IDictionary<uint, string> map = CreateMap<uint, string>();

         uint[] inputValues = { 3, 0, 99, 1, 1024, 2 };
         uint[] expectedOrder = { 3, 0, 99, 1, 1024, 2 }; // Insertion order

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
   }
}