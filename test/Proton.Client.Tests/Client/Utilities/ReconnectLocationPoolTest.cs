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

using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Utilities
{
   [TestFixture, Timeout(20000)]
   public class ReconnectLocationPoolTest
   {
      private List<ReconnectLocation> entries;

      [SetUp]
      public void SetUp()
      {
         entries = new List<ReconnectLocation>();

         entries.Add(new ReconnectLocation("192.168.2.1", 5672));
         entries.Add(new ReconnectLocation("192.168.2.2", 5672));
         entries.Add(new ReconnectLocation("192.168.2.3", 5672));
         entries.Add(new ReconnectLocation("192.168.2.4", 5672));
      }

      [Test]
      public void TestCreateEmptyPool()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool();

         Assert.IsTrue(pool.IsEmpty);
         Assert.AreEqual(0, pool.Count);
         Assert.IsNotNull(pool.ToString());
      }

      [Test]
      public void TestCreateEmptyPoolFromNullEntryList()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(null);
         Assert.IsNull(pool.Next);
      }

      [Test]
      public void TestCreateNonEmptyPoolWithEntryList()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);

         Assert.AreEqual(entries, pool.ToList());
         Assert.IsNotNull(pool.Next);
         Assert.AreEqual(entries[1], pool.Next);
      }

      [Test]
      public void TestGetNextFromEmptyPool()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool();
         Assert.IsNull(pool.Next);
      }

      [Test]
      public void TestGetNextFromSingleValuePool()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries.GetRange(0, 1));

         Assert.AreEqual(entries[0], pool.Next);
         Assert.AreEqual(entries[0], pool.Next);
         Assert.AreEqual(entries[0], pool.Next);

         Assert.IsNotNull(pool.ToString());
      }

      [Test]
      public void TestAddEntryToEmptyPool()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool();
         Assert.IsTrue(pool.IsEmpty);
         pool.Add(entries[0]);
         Assert.IsFalse(pool.IsEmpty);
         Assert.AreEqual(entries[0], pool.Next);
      }

      [Test]
      public void TestDuplicatesNotAdded()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);

         Assert.AreEqual(entries.Count, pool.Count);
         pool.Add(entries[0]);
         Assert.AreEqual(entries.Count, pool.Count);
         pool.Add(entries[1]);
         Assert.AreEqual(entries.Count, pool.Count);
      }

      [Test]
      public void TestDuplicatesNotAddedByAddFirst()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);

         Assert.AreEqual(entries.Count, pool.Count);
         pool.AddFirst(entries[0]);
         Assert.AreEqual(entries.Count, pool.Count);
         pool.AddFirst(entries[1]);
         Assert.AreEqual(entries.Count, pool.Count);
      }

      [Test]
      public void TestDuplicatesNotAddedIfPortsMatch()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool();

         Assert.IsTrue(pool.IsEmpty);
         pool.Add(new ReconnectLocation("127.0.0.1", 5672));
         Assert.IsFalse(pool.IsEmpty);

         Assert.AreEqual(1, pool.Count);
         pool.Add(new ReconnectLocation("127.0.0.1", 5672));
         Assert.AreEqual(1, pool.Count);

         Assert.AreEqual(1, pool.Count);
         pool.Add(new ReconnectLocation("localhost", 5673));
         Assert.AreEqual(2, pool.Count);
      }

      [Test]
      public void TestAddEntryToPoolThenShuffle()
      {
         ReconnectLocation newEntry = new ReconnectLocation("192.168.2." + (entries.Count + 1), 5672);

         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         pool.Add(newEntry);

         pool.Shuffle();

         ReconnectLocation found = default(ReconnectLocation);

         for (int i = 0; i < entries.Count + 1; ++i)
         {
            ReconnectLocation next = pool.Next.Value;
            if (newEntry.Equals(next))
            {
               found = next;
            }
         }

         if (found == default(ReconnectLocation))
         {
            Assert.Fail("ReconnectEntry added was not retrieved from the pool");
         }
      }

      [Test]
      public void TestAddEntryToPoolNotRandomized()
      {
         ReconnectLocation newEntry = new ReconnectLocation("192.168.2." + (entries.Count + 1), 5672);

         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         pool.Shuffle();
         pool.Add(newEntry);

         for (int i = 0; i < entries.Count; ++i)
         {
            Assert.AreNotEqual(newEntry, pool.Next);
         }

         Assert.AreEqual(newEntry, pool.Next);
      }

      [Test]
      public void TestAddFirst()
      {
         ReconnectLocation newEntry = new ReconnectLocation("192.168.2." + (entries.Count + 1), 5672);

         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         pool.AddFirst(newEntry);

         Assert.AreEqual(newEntry, pool.Next);

         for (int i = 0; i < entries.Count; ++i)
         {
            Assert.AreNotEqual(newEntry, pool.Next);
         }

         Assert.AreEqual(newEntry, pool.Next);
      }

      [Test]
      public void TestAddFirstHandlesNulls()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         pool.AddFirst(default(ReconnectLocation));

         Assert.AreEqual(entries.Count, pool.Count);
      }

      [Test]
      public void TestAddFirstToEmptyPool()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool();
         Assert.IsTrue(pool.IsEmpty);
         pool.AddFirst(entries[0]);
         Assert.IsFalse(pool.IsEmpty);
         Assert.AreEqual(entries[0], pool.Next);
      }

      [Test]
      public void TestAddAllHandlesNulls()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         pool.AddAll(null);

         Assert.AreEqual(entries.Count, pool.Count);
      }

      [Test]
      public void TestAddAllHandlesEmpty()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         pool.AddAll(new List<ReconnectLocation>());

         Assert.AreEqual(entries.Count, pool.Count);
      }

      [Test]
      public void TestAddAll()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(null);

         Assert.AreEqual(0, pool.Count);
         Assert.IsFalse(entries.Count == 0);

         pool.AddAll(entries);

         Assert.AreEqual(entries.Count, pool.Count);
      }

      [Test]
      public void TestRemoveEntryFromPool()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);

         ReconnectLocation removed = entries[0];

         pool.Remove(removed);

         for (int i = 0; i < entries.Count + 1; ++i)
         {
            if (removed.Equals(pool.Next))
            {
               Assert.Fail("ReconnectEntry was not removed from the pool");
            }
         }
      }

      [Test]
      public void TestRemoveDoesNotApplyHostResolution()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool();

         Assert.IsTrue(pool.IsEmpty);
         pool.Add(new ReconnectLocation("127.0.0.1", 5672));
         Assert.IsFalse(pool.IsEmpty);
         pool.Remove(new ReconnectLocation("localhost", 5672));
         Assert.IsFalse(pool.IsEmpty);
         pool.Remove(new ReconnectLocation("127.0.0.1", 5673));
         Assert.IsFalse(pool.IsEmpty);
      }

      [Test]
      public void TestConnectedShufflesWhenRandomizing()
      {
         AssertConnectedEffectOnPool(true, true);
      }

      [Test]
      public void TestConnectedDoesNotShufflesWhenNoRandomizing()
      {
         AssertConnectedEffectOnPool(false, false);
      }

      private void AssertConnectedEffectOnPool(bool randomize, bool shouldShuffle)
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);

         if (randomize)
         {
            pool.Shuffle();
         }

         List<ReconnectLocation> current = new List<ReconnectLocation>();
         List<ReconnectLocation> previous = new List<ReconnectLocation>();

         bool shuffled = false;

         for (int i = 0; i < 10; ++i)
         {
            for (int j = 0; j < entries.Count; ++j)
            {
               current.Add(pool.Next.Value);
            }

            if (randomize)
            {
               pool.Shuffle();
            }

            if (previous.Count > 0 && !previous.SequenceEqual(current))
            {
               shuffled = true;
               break;
            }

            previous.Clear();
            previous.AddRange(current);
            current.Clear();
         }

         if (shouldShuffle)
         {
            Assert.IsTrue(shuffled, "ReconnectEntry list did not get randomized");
         }
         else
         {
            Assert.IsFalse(shuffled, "ReconnectEntry list should not get randomized");
         }
      }

      [Test]
      public void TestAddOrRemoveDefaultHasNoAffect()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         Assert.AreEqual(entries.Count, pool.Count);

         pool.Add(default(ReconnectLocation));
         Assert.AreEqual(entries.Count, pool.Count);
         pool.Remove(default(ReconnectLocation));
         Assert.AreEqual(entries.Count, pool.Count);
      }

      [Test]
      public void TestRemoveAll()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         Assert.AreEqual(entries.Count, pool.Count);

         pool.RemoveAll();
         Assert.IsTrue(pool.IsEmpty);
         Assert.AreEqual(0, pool.Count);

         pool.RemoveAll();
      }

      [Test]
      public void TestReplaceAll()
      {
         ReconnectLocationPool pool = new ReconnectLocationPool(entries);
         Assert.AreEqual(entries.Count, pool.Count);

         List<ReconnectLocation> newEntries = new List<ReconnectLocation>();

         newEntries.Add(new ReconnectLocation("192.168.2.1", 5672));
         newEntries.Add(new ReconnectLocation("192.168.2.2", 5672));

         pool.ReplaceAll(newEntries);
         Assert.IsFalse(pool.IsEmpty);
         Assert.AreEqual(newEntries.Count, pool.Count);
         Assert.AreEqual(newEntries, pool.ToList());

         pool.RemoveAll();
      }
   }
}