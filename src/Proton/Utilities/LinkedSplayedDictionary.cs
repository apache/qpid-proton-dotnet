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

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// A Linked node version of the Splayed Dictionary that provides
   /// consisted enumeration of dictionary entries based on insertion
   /// order over natural or comparer based enumeration from the default
   /// splayed dictionary implementation.
   /// </summary>
   /// <typeparam name="T">The type contained in the Queue</typeparam>
   public sealed class LinkedSplayedDictionary<K, V> : SplayedDictionary<K, V>
   {
      /// <summary>
      /// A dummy entry in the circular linked list of entries in the map.
      /// The first real entry is root.next, and the last is header.pervious.
      /// If the map is empty, root.next == root and root.previous == root.
      /// </summary>
      private readonly SplayedEntry entries = new SplayedEntry();

      public LinkedSplayedDictionary()
      {
      }

      public LinkedSplayedDictionary(IComparer<K> comparer) : base(comparer)
      {
      }

      public LinkedSplayedDictionary(IDictionary<K, V> source) : base(source)
      {
      }

      public override void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
      {
         if (array == null)
         {
            throw new ArgumentNullException("The provided array cannot be null");
         }

         if (arrayIndex < 0)
         {
            throw new ArgumentOutOfRangeException("Array index must be greater than zero");
         }

         if (array.Length - arrayIndex > size)
         {
            throw new ArgumentException("Not enough space in array to store the dictionary entries");
         }

         for (SplayedEntry entry = entries.linkNext; entry != entries; entry = entry.linkNext)
         {
            array[arrayIndex++] = entry.ToKeyValuePair();
         }
      }

      public override void CopyTo(Array array, int arrayIndex)
      {
         if (array == null)
         {
            throw new ArgumentNullException("The provided array cannot be null");
         }

         if (arrayIndex < 0)
         {
            throw new ArgumentOutOfRangeException("Array index must be greater than zero");
         }

         if (array.Length - arrayIndex > size)
         {
            throw new ArgumentException("Not enough space in array to store the dictionary entries");
         }

         for (SplayedEntry entry = entries.linkNext; entry != entries; entry = entry.linkNext)
         {
            array.SetValue(entry.ToKeyValuePair(), arrayIndex++);
         }
      }

      public override void Clear()
      {
         base.Clear();

         // Unlink all the entries and reset to no insertions state
         entries.linkNext = entries.linkPrev = entries;
      }

      public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
      {
         return new LinkedSplayedDictionaryEntryEnumerator(this);
      }

      public override ICollection<K> Keys
      {
         get
         {
            if (keySet == null)
            {
               keySet = new LinkedSplayedDictionaryKeys(this);
            }

            return keySet;
         }
      }

      public override ICollection<V> Values
      {
         get
         {
            if (values == null)
            {
               values = new LinkedSplayedDictionaryValues(this);
            }

            return values;
         }
      }

      protected override void EntryAdded(SplayedEntry newEntry)
      {
         // Insertion ordering of the splayed map entries recorded here
         // and the list of entries doesn't change until an entry is removed.
         newEntry.linkNext = entries;
         newEntry.linkPrev = entries.linkPrev;
         entries.linkPrev.linkNext = newEntry;
         entries.linkPrev = newEntry;
      }

      protected override void EntryDeleted(SplayedEntry deletedEntry)
      {
         // Remove the entry from the insertion ordered entry list.
         deletedEntry.linkNext.linkPrev = deletedEntry.linkPrev;
         deletedEntry.linkPrev.linkNext = deletedEntry.linkNext;
         deletedEntry.linkNext = deletedEntry.linkPrev = null;
      }

      #region Dictionary Enumerators

      private abstract class LinkedSplayedDictionaryEnumerator<TResult> : IEnumerator<TResult>, IEnumerator
      {
         protected readonly LinkedSplayedDictionary<K, V> parent;
         protected readonly int expectedModCount;
         protected SplayedEntry current;
         protected bool disposed;

         public LinkedSplayedDictionaryEnumerator(LinkedSplayedDictionary<K, V> parent)
         {
            this.parent = parent;
            this.expectedModCount = parent.modCount;
            this.current = parent.entries;
         }

         object IEnumerator.Current => this.Current;

         public abstract TResult Current { get; }

         public void Dispose()
         {
            current = parent.entries;
            disposed = true;
         }

         public bool MoveNext()
         {
            CheckNotModified();

            return (current = current.linkNext) != parent.entries;
         }

         public void Reset()
         {
            current = parent.entries;
         }

         protected void CheckNotModified()
         {
            if (expectedModCount != parent.modCount)
            {
               throw new InvalidOperationException("Parent Dictionary was modified during enumeration.");
            }
         }
      }

      private sealed class LinkedSplayedDictionaryKeyEnumerator : LinkedSplayedDictionaryEnumerator<K>
      {
         public LinkedSplayedDictionaryKeyEnumerator(LinkedSplayedDictionary<K, V> parent) : base(parent)
         {
         }

         public override K Current
         {
            get
            {
               if (current != parent.entries)
               {
                  return current.Key;
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      private sealed class LinkedSplayedDictionaryValueEnumerator : LinkedSplayedDictionaryEnumerator<V>
      {
         public LinkedSplayedDictionaryValueEnumerator(LinkedSplayedDictionary<K, V> parent) : base(parent)
         {
         }

         public override V Current
         {
            get
            {
               if (current != parent.entries)
               {
                  return current.Value;
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      private sealed class LinkedSplayedDictionaryEntryEnumerator : LinkedSplayedDictionaryEnumerator<KeyValuePair<K, V>>, IDictionaryEnumerator
      {
         public LinkedSplayedDictionaryEntryEnumerator(LinkedSplayedDictionary<K, V> parent) : base(parent)
         {
         }

         public override KeyValuePair<K, V> Current
         {
            get
            {
               if (current != parent.entries)
               {
                  return current.ToKeyValuePair();
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }

         public DictionaryEntry Entry
         {
            get
            {
               if (current != parent.entries)
               {
                  return current.ToDictionaryEntry();
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }

         public object Key
         {
            get
            {
               if (current != parent.entries)
               {
                  return current.Key;
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }

         public object Value
         {
            get
            {
               if (current != parent.entries)
               {
                  return current.Value;
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      #endregion

      #region Dictionary Collection Views

      private sealed class LinkedSplayedDictionaryValues : ICollection<V>, ICollection
      {
         private readonly LinkedSplayedDictionary<K, V> parent;

         public LinkedSplayedDictionaryValues(LinkedSplayedDictionary<K, V> parent) : base()
         {
            this.parent = parent;
         }

         public int Count => parent.Count;

         public bool IsReadOnly => parent.IsReadOnly;

         public bool IsSynchronized => false;

         public object SyncRoot => parent.entryPool;

         public void Add(V item)
         {
            throw new NotSupportedException("Cannot add a value only entry to the parent Dictionary");
         }

         public void Clear()
         {
            parent.Clear();
         }

         public bool Contains(V item)
         {
            return parent.ContainsValue(item);
         }

         public bool Remove(V item)
         {
            SplayedEntry root = parent.entries;

            for (SplayedEntry entry = root.linkNext; entry != root; entry = entry.linkNext)
            {
               if (entry.ValueEquals(item))
               {
                  parent.Delete(entry);
                  return true;
               }
            }
            return false;
         }

         public void CopyTo(V[] array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException("The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException("Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary values");
            }

            SplayedEntry root = parent.entries;

            for (SplayedEntry entry = root.linkNext; entry != root; entry = entry.linkNext)
            {
               array[arrayIndex++] = entry.Value;
            }
         }

         public void CopyTo(Array array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException("The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException("Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary values");
            }

            SplayedEntry root = parent.entries;

            for (SplayedEntry entry = root.linkNext; entry != root; entry = entry.linkNext)
            {
               array.SetValue(entry.Value, arrayIndex++);
            }
         }

         public IEnumerator<V> GetEnumerator()
         {
            return new LinkedSplayedDictionaryValueEnumerator(parent);
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            return new LinkedSplayedDictionaryValueEnumerator(parent);
         }
      }

      private sealed class LinkedSplayedDictionaryKeys : ICollection<K>, ICollection
      {
         private readonly LinkedSplayedDictionary<K, V> parent;

         public LinkedSplayedDictionaryKeys(LinkedSplayedDictionary<K, V> parent) : base()
         {
            this.parent = parent;
         }

         public int Count => parent.Count;

         public bool IsReadOnly => parent.IsReadOnly;

         public bool IsSynchronized => false;

         public object SyncRoot => parent.entryPool;

         public void Add(K item)
         {
            throw new NotSupportedException("Cannot add a key only entry to the parent Dictionary");
         }

         public void Clear()
         {
            parent.Clear();
         }

         public bool Contains(K item)
         {
            return parent.ContainsKey(item);
         }

         public void CopyTo(K[] array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException("The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException("Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary keys");
            }

            SplayedEntry root = parent.entries;

            for (SplayedEntry entry = root.linkNext; entry != root; entry = entry.linkNext)
            {
               array[arrayIndex++] = entry.Key;
            }
         }

         public void CopyTo(Array array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException("The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException("Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary values");
            }

            SplayedEntry root = parent.entries;

            for (SplayedEntry entry = root.linkNext; entry != root; entry = entry.linkNext)
            {
               array.SetValue(entry.Key, arrayIndex++);
            }
         }

         public bool Remove(K item)
         {
            SplayedEntry root = parent.entries;

            for (SplayedEntry entry = root.linkNext; entry != root; entry = entry.linkNext)
            {
               if (entry.KeyEquals(item))
               {
                  parent.Delete(entry);
                  return true;
               }
            }
            return false;
         }

         public IEnumerator<K> GetEnumerator()
         {
            return new LinkedSplayedDictionaryKeyEnumerator(parent);
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            return new LinkedSplayedDictionaryKeyEnumerator(parent);
         }
      }

      #endregion
   }
}