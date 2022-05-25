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
using System.Diagnostics.CodeAnalysis;

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// A splay tree based dictionary implementation that provides fast access
   /// to recently visited entries and offered enumeration based on the natural
   /// ordering or comparer defined ordering of dictionary entries.
   /// </summary>
   /// <typeparam name="T">The type contained in the Queue</typeparam>
   public class SplayedDictionary<K, V> : IDictionary, ICollection<KeyValuePair<K, V>>, IDictionary<K, V>, IEnumerable, IEnumerable<KeyValuePair<K, V>>
   {
      private readonly IComparer<K> keyComparer = Comparer<K>.Default;

      /// <summary>
      /// Pooled entries used to prevent excessive allocations for rapid insert and removal operations.
      /// </summary>
      protected readonly RingQueue<SplayedEntry> entryPool = new(64);

      /// <summary>
      /// Root of the tree which can change as entries are splayed up and down the tree
      /// </summary>
      protected SplayedEntry root;

      /// <summary>
      /// Tracked tree size to prevent traversals for size operations.
      /// </summary>
      protected int size;

      /// <summary>
      /// Provides a means of tracking modification during enumeration errors.
      /// </summary>
      protected int modCount;

      /// <summary>
      /// Cached collection object that provides access to the dictionary keys
      /// </summary>
      protected ICollection<K> keySet;

      /// <summary>
      /// Cached collection object that provides access to the dictionary values
      /// </summary>
      protected ICollection<V> values;

      /// <summary>
      /// Create a new Splayed Dictionary instance that uses a default key comparer
      /// instance to provide relative ordering of keys within the mapping.
      /// </summary>
      public SplayedDictionary() : base()
      {
      }

      /// <summary>
      /// Create a new Splayed Dictionary instance that uses the give key comparer
      /// instance to provide relative ordering of keys within the mapping.
      /// </summary>
      /// <param name="comparer">The IComparer instance to use to compare keys</param>
      public SplayedDictionary(IComparer<K> comparer)
      {
         this.keyComparer = comparer ?? throw new ArgumentNullException(nameof(comparer), "Provided key comparer must not be null");
      }

      /// <summary>
      /// Create a new Splayed Dictionary instance that uses the give key comparer
      /// instance to provide relative ordering of keys within the mapping.
      /// </summary>
      /// <param name="comparer">The IComparer instance to use to compare keys</param>
      public SplayedDictionary(IDictionary<K, V> source)
      {
         if (source == null)
         {
            throw new ArgumentNullException(nameof(source), "Provided source dictionary cannot be null");
         }

         foreach (KeyValuePair<K, V> entry in source)
         {
            Add(entry);
         }
      }

      #region Splayed Dictionary API implementations

      public int Count => size;

      public bool IsReadOnly => false;

      public IComparer<K> Comparer => keyComparer;

      public bool IsFixedSize => false;

      public bool IsSynchronized => false;

      public object SyncRoot => entryPool;

      public void Add(K key, V value)
      {
         TryAdd(key, value, false);
      }

      public void Add(KeyValuePair<K, V> item)
      {
         TryAdd(item.Key, item.Value, false);
      }

      public void Add(object key, object value)
      {
         TryAdd((K)key, (V)value, false);
      }

      private void TryAdd(K key, V value, bool allowUpdates)
      {
         CheckSuppliedKeyIsNotNull(key, "Cannot write to an entry with a null key");

         bool entryAdded = true;

         if (root == null)
         {
            root = entryPool.Poll(CreateEntry).Initialize(key, value);
         }
         else
         {
            root = Splay(root, key);
            if (root.KeyEquals(key))
            {
               if (allowUpdates)
               {
                  root.Value = value;
                  entryAdded = false;
               }
               else
               {
                  throw new ArgumentException("An entry with the given key already exists");
               }
            }
            else
            {
               SplayedEntry node = entryPool.Poll(CreateEntry).Initialize(key, value);

               if (Compare(key, root.Key) < 0)
               {
                  ShiftRootRightOf(node);
               }
               else
               {
                  ShiftRootLeftOf(node);
               }
            }
         }

         if (entryAdded)
         {
            EntryAdded(root);
            size++;
         }

         modCount++;
      }

      public virtual void Clear()
      {
         root = null;
         size = 0;
         modCount++;
      }

      public bool Contains(object key)
      {
         return ContainsKey((K)key);
      }

      public bool Contains(KeyValuePair<K, V> item)
      {
         for (SplayedEntry entry = FirstEntry(root); entry != null; entry = Successor(entry))
         {
            if (entry.Equals(item))
            {
               return true;
            }
         }

         return false;
      }

      public bool ContainsValue(V value)
      {
         for (SplayedEntry entry = FirstEntry(root); entry != null; entry = Successor(entry))
         {
            if (entry.ValueEquals(value))
            {
               return true;
            }
         }

         return false;
      }

      public bool ContainsKey(K key)
      {
         if (root == null || key == null)
         {
            return false;
         }

         root = Splay(root, key);
         if (EqualityComparer<K>.Default.Equals(root.Key, key))
         {
            return true;
         }

         return false;
      }

      public virtual void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
         }

         if (arrayIndex < 0)
         {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
         }

         if (array.Length - arrayIndex > size)
         {
            throw new ArgumentException("Not enough space in array to store the dictionary entries");
         }

         for (SplayedEntry entry = FirstEntry(root); entry != null; entry = Successor(entry))
         {
            array[arrayIndex++] = entry.ToKeyValuePair();
         }
      }

      public virtual void CopyTo(Array array, int arrayIndex)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
         }

         if (arrayIndex < 0)
         {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
         }

         if (array.Length - arrayIndex > size)
         {
            throw new ArgumentException("Not enough space in array to store the dictionary entries");
         }

         for (SplayedEntry entry = FirstEntry(root); entry != null; entry = Successor(entry))
         {
            array.SetValue(entry.ToKeyValuePair(), arrayIndex++);
         }
      }

      public void Remove(object key)
      {
         Remove((K)key);
      }

      public bool Remove(K key)
      {
         bool removed = false;

         if (root != null)
         {
            root = Splay(root, key);
            if (root.KeyEquals(key))
            {
               // We splayed on the key and matched it so the root
               // will now be the node matching that key.
               Delete(root);
               removed = true;
               modCount++;
            }
         }

         return removed;
      }

      /// <summary>
      /// Removes the first entry from the dictionary that contains the specified value
      /// and returns true, if there is no entry with the given value this method returns
      /// false.
      /// </summary>
      /// <param name="target">The value to search for in the entries</param>
      /// <returns>true if an entry was removed or false if no match found</returns>
      public bool RemoveValue(V target)
      {
         for (SplayedEntry e = FirstEntry(root); e != null; e = Successor(e))
         {
            if (e.ValueEquals(target))
            {
               Delete(e);
               return true;
            }
         }
         return false;
      }

      public bool Remove(KeyValuePair<K, V> item)
      {
         bool removed = false;

         if (root != null)
         {
            root = Splay(root, item.Key);
            if (root.Equals(item))
            {
               // We splayed on the key and matched it so the root
               // will now be the node matching that key.
               Delete(root);
               removed = true;
               modCount++;
            }
         }

         return removed;
      }

      public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
      {
         CheckSuppliedKeyIsNotNull(key, "Cannot get an entry with a null key");

         bool result = false;

         if (root != null)
         {
            root = Splay(root, key);
            if (root.KeyEquals(key))
            {
               value = root.Value;
               result = true;
            }
            else
            {
               value = default;
            }
         }
         else
         {
            value = default;
         }

         return result;
      }

      public V this[K key]
      {
         get
         {
            if (TryGetValue(key, out V result))
            {
               return result;
            }

            throw new KeyNotFoundException("No entry exists for given key: " + key);
         }

         set => TryAdd(key, value, true);
      }

      public object this[object key]
      {
         get => this[(K)key];
         set => this[(K)key] = (V)value;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return this.GetEnumerator();
      }

      IDictionaryEnumerator IDictionary.GetEnumerator()
      {
         return (IDictionaryEnumerator)this.GetEnumerator();
      }

      public virtual IEnumerator<KeyValuePair<K, V>> GetEnumerator()
      {
         return new SplayedDictionaryEntryEnumerator(this);
      }

      public virtual ICollection<K> Keys
      {
         get
         {
            if (keySet == null)
            {
               keySet = new SplayedDictionaryKeys(this);
            }

            return keySet;
         }
      }

      public virtual ICollection<V> Values
      {
         get
         {
            if (values == null)
            {
               values = new SplayedDictionaryValues(this);
            }

            return values;
         }
      }

      ICollection IDictionary.Keys => (ICollection)this.Keys;

      ICollection IDictionary.Values => (ICollection)this.Values;

      #endregion

      #region Extension points used by the Linked version of the SplayedDictionary

      protected virtual void EntryAdded(SplayedEntry newEntry)
      {
         // Nothing to do in the base class implementation.
      }

      protected virtual void EntryDeleted(SplayedEntry deletedEntry)
      {
         // Nothing to do in the base class implementation.
      }

      #endregion

      #region Dictionary Enumerators

      private abstract class SplayedDictionaryEnumerator<TResult> : IEnumerator<TResult>, IEnumerator
      {
         protected readonly SplayedDictionary<K, V> parent;
         protected readonly int expectedModCount;
         protected SplayedEntry current;

         public SplayedDictionaryEnumerator(SplayedDictionary<K, V> parent)
         {
            this.parent = parent;
            this.expectedModCount = parent.modCount;
            this.current = null;
         }

         object IEnumerator.Current => this.Current;

         public abstract TResult Current { get; }

         public void Dispose()
         {
            current = null;
         }

         public bool MoveNext()
         {
            CheckNotModified();

            if (current == null)
            {
               current = FirstEntry(parent.root);
            }
            else
            {
               current = Successor(current);
            }

            return current != null;
         }

         public void Reset()
         {
            current = null;
         }

         protected void CheckNotModified()
         {
            if (expectedModCount != parent.modCount)
            {
               throw new InvalidOperationException("Parent Dictionary was modified during enumeration.");
            }
         }
      }

      private sealed class SplayedDictionaryKeyEnumerator : SplayedDictionaryEnumerator<K>
      {
         public SplayedDictionaryKeyEnumerator(SplayedDictionary<K, V> parent) : base(parent)
         {
         }

         public override K Current
         {
            get
            {
               if (current != null)
               {
                  return current.Key;
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      private sealed class SplayedDictionaryValueEnumerator : SplayedDictionaryEnumerator<V>
      {
         public SplayedDictionaryValueEnumerator(SplayedDictionary<K, V> parent) : base(parent)
         {
         }

         public override V Current
         {
            get
            {
               if (current != null)
               {
                  return current.Value;
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      private sealed class SplayedDictionaryEntryEnumerator : SplayedDictionaryEnumerator<KeyValuePair<K, V>>, IDictionaryEnumerator
      {
         public SplayedDictionaryEntryEnumerator(SplayedDictionary<K, V> parent) : base(parent)
         {
         }

         public override KeyValuePair<K, V> Current =>
            current?.ToKeyValuePair() ?? throw new InvalidOperationException("Current position is undefined.");

         public DictionaryEntry Entry =>
            current?.ToDictionaryEntry() ?? throw new InvalidOperationException("Current position is undefined.");

         public object Key
         {
            get
            {
               if (current != null)
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
               if (current != null)
               {
                  return current.Value;
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      #endregion

      #region Dictionary Collection Views

      private sealed class SplayedDictionaryValues : ICollection<V>, ICollection
      {
         private readonly SplayedDictionary<K, V> parent;

         public SplayedDictionaryValues(SplayedDictionary<K, V> parent) : base()
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
            return parent.RemoveValue(item);
         }

         public void CopyTo(V[] array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary values");
            }

            for (SplayedEntry entry = FirstEntry(parent.root); entry != null; entry = Successor(entry))
            {
               array[arrayIndex++] = entry.Value;
            }
         }

         public void CopyTo(Array array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary values");
            }

            for (SplayedEntry entry = FirstEntry(parent.root); entry != null; entry = Successor(entry))
            {
               array.SetValue(entry.Value, arrayIndex++);
            }
         }

         public IEnumerator<V> GetEnumerator()
         {
            return new SplayedDictionaryValueEnumerator(parent);
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            return new SplayedDictionaryValueEnumerator(parent);
         }
      }

      private sealed class SplayedDictionaryKeys : ICollection<K>, ICollection
      {
         private readonly SplayedDictionary<K, V> parent;

         public SplayedDictionaryKeys(SplayedDictionary<K, V> parent) : base()
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
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary keys");
            }

            for (SplayedEntry entry = FirstEntry(parent.root); entry != null; entry = Successor(entry))
            {
               array[arrayIndex++] = entry.Key;
            }
         }

         public void CopyTo(Array array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary keys");
            }

            for (SplayedEntry entry = FirstEntry(parent.root); entry != null; entry = Successor(entry))
            {
               array.SetValue(entry.Key, arrayIndex++);
            }
         }

         public bool Remove(K item)
         {
            return parent.Remove(item);
         }

         public IEnumerator<K> GetEnumerator()
         {
            return new SplayedDictionaryKeyEnumerator(parent);
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            return new SplayedDictionaryKeyEnumerator(parent);
         }
      }

      #endregion

      #region Splaying support methods

      private void ShiftRootRightOf(SplayedEntry newRoot)
      {
         newRoot.right = root;
         newRoot.left = root.left;
         if (newRoot.left != null)
         {
            newRoot.left.parent = newRoot;
         }
         root.left = null;
         root.parent = newRoot;
         root = newRoot;
      }

      private void ShiftRootLeftOf(SplayedEntry newRoot)
      {
         newRoot.left = root;
         newRoot.right = root.right;
         if (newRoot.right != null)
         {
            newRoot.right.parent = newRoot;
         }
         root.right = null;
         root.parent = newRoot;
         root = newRoot;
      }

      /*
       * Rotations of tree elements form the basis of search and balance operations
       * within the tree during put, get and remove type operations.
       *
       *       y                                     x
       *      / \     Zig (Right Rotation)          /  \
       *     x   T3   – - – - – - – - - ->         T1   y
       *    / \       < - - - - - - - - -              / \
       *   T1  T2     Zag (Left Rotation)            T2   T3
       *
       */

      private static SplayedEntry RightRotate(SplayedEntry node)
      {
         SplayedEntry rotated = node.left;
         node.left = rotated.right;
         rotated.right = node;

         // Reset the parent values for adjusted nodes.
         rotated.parent = node.parent;
         node.parent = rotated;
         if (node.left != null)
         {
            node.left.parent = node;
         }

         return rotated;
      }

      private static SplayedEntry LeftRotate(SplayedEntry node)
      {
         SplayedEntry rotated = node.right;
         node.right = rotated.left;
         rotated.left = node;

         // Reset the parent values for adjusted nodes.
         rotated.parent = node.parent;
         node.parent = rotated;
         if (node.right != null)
         {
            node.right.parent = node;
         }

         return rotated;
      }

      /// <summary>
      /// The requested key if present is brought to the root of the tree.  If it is not
      /// present then the last accessed element (nearest match) will be brought to the root
      /// as it is likely it will be the next accessed or one of the neighboring nodes which
      /// reduces the search time for that cluster.
      /// </summary>
      private SplayedEntry Splay(SplayedEntry root, K key)
      {
         if (root == null || EqualityComparer<K>.Default.Equals(root.Key, key))
         {
            return root;
         }

         SplayedEntry lessThanKeyRoot = null;
         SplayedEntry lessThanKeyNode = null;
         SplayedEntry greaterThanKeyRoot = null;
         SplayedEntry greaterThanKeyNode = null;

         while (true)
         {
            if (Compare(key, root.Key) < 0)
            {
               // Entry must be to the left of the current node so we bring that up
               // and then work from there to see if we can find the key
               if (root.left != null && Compare(key, root.left.Key) < 0)
               {
                  root = RightRotate(root);
               }

               // Is there nowhere else to go, if so we are done.
               if (root.left == null)
               {
                  break;
               }

               // Haven't found it yet but we now know the current element is greater
               // than the element we are looking for so it goes to the right tree.
               if (greaterThanKeyRoot == null)
               {
                  greaterThanKeyRoot = greaterThanKeyNode = root;
               }
               else
               {
                  greaterThanKeyNode.left = root;
                  greaterThanKeyNode.left.parent = greaterThanKeyNode;
                  greaterThanKeyNode = root;
               }

               root = root.left;
               root.parent = null;
            }
            else if (Compare(key, root.Key) > 0)
            {
               // Entry must be to the right of the current node so we bring that up
               // and then work from there to see if we can find the key
               if (root.right != null && Compare(key, root.right.Key) > 0)
               {
                  root = LeftRotate(root);
               }

               // Is there nowhere else to go, if so we are done.
               if (root.right == null)
               {
                  break;
               }

               // Haven't found it yet but we now know the current element is less
               // than the element we are looking for so it goes to the left tree.
               if (lessThanKeyRoot == null)
               {
                  lessThanKeyRoot = lessThanKeyNode = root;
               }
               else
               {
                  lessThanKeyNode.right = root;
                  lessThanKeyNode.right.parent = lessThanKeyNode;
                  lessThanKeyNode = root;
               }

               root = root.right;
               root.parent = null;
            }
            else
            {
               break; // Found it
            }
         }

         // Reassemble the tree from the left, right and middle the assembled nodes in the
         // left and right should have their last element either nulled out or linked to the
         // remaining items middle tree
         if (lessThanKeyRoot == null)
         {
            lessThanKeyRoot = root.left;
         }
         else
         {
            lessThanKeyNode.right = root.left;
            if (lessThanKeyNode.right != null)
            {
               lessThanKeyNode.right.parent = lessThanKeyNode;
            }
         }

         if (greaterThanKeyRoot == null)
         {
            greaterThanKeyRoot = root.right;
         }
         else
         {
            greaterThanKeyNode.left = root.right;
            if (greaterThanKeyNode.left != null)
            {
               greaterThanKeyNode.left.parent = greaterThanKeyNode;
            }
         }

         // The found or last accessed element is now rooted to the splayed
         // left and right trees and returned as the new tree.
         root.left = lessThanKeyRoot;
         if (root.left != null)
         {
            root.left.parent = root;
         }
         root.right = greaterThanKeyRoot;
         if (root.right != null)
         {
            root.right.parent = root;
         }

         return root;
      }

      protected void Delete(SplayedEntry node)
      {
         SplayedEntry grandparent = node.parent;
         SplayedEntry replacement = node.right;

         if (node.left != null)
         {
            replacement = Splay(node.left, node.Key);
            replacement.right = node.right;
         }

         if (replacement != null)
         {
            replacement.parent = grandparent;
         }

         if (grandparent != null)
         {
            if (grandparent.left == node)
            {
               grandparent.left = replacement;
            }
            else
            {
               grandparent.right = replacement;
            }
         }
         else
         {
            root = replacement;
         }

         // Clear node before moving to cache
         node.left = node.right = node.parent = null;
         entryPool.Offer(node);

         EntryDeleted(node);

         size--;
      }

      private static SplayedEntry FirstEntry(SplayedEntry node)
      {
         SplayedEntry firstEntry = node;
         if (firstEntry != null)
         {
            while (firstEntry.left != null)
            {
               firstEntry = firstEntry.left;
            }
         }

         return firstEntry;
      }

      private static SplayedEntry LastEntry(SplayedEntry node)
      {
         SplayedEntry lastEntry = node;
         if (lastEntry != null)
         {
            while (lastEntry.right != null)
            {
               lastEntry = lastEntry.right;
            }
         }

         return lastEntry;
      }

      private static SplayedEntry Successor(SplayedEntry node)
      {
         if (node == null)
         {
            return null;
         }
         else if (node.right != null)
         {
            // Walk to bottom of tree from this node's right child.
            SplayedEntry result = node.right;
            while (result.left != null)
            {
               result = result.left;
            }

            return result;
         }
         else
         {
            SplayedEntry parent = node.parent;
            SplayedEntry child = node;
            while (parent != null && child == parent.right)
            {
               child = parent;
               parent = parent.parent;
            }

            return parent;
         }
      }

      private static SplayedEntry Predecessor(SplayedEntry node)
      {
         if (node == null)
         {
            return null;
         }
         else if (node.left != null)
         {
            // Walk to bottom of tree from this node's left child.
            SplayedEntry result = node.left;
            while (result.right != null)
            {
               result = result.right;
            }

            return result;
         }
         else
         {
            SplayedEntry parent = node.parent;
            SplayedEntry child = node;
            while (parent != null && child == parent.left)
            {
               child = parent;
               parent = parent.parent;
            }

            return parent;
         }
      }

      private int Compare(K lhs, K rhs)
      {
         return this.keyComparer.Compare(lhs, rhs);
      }

      private static SplayedEntry CreateEntry()
      {
         return new SplayedEntry();
      }

      private static void CheckSuppliedKeyIsNotNull(in K key, in string error)
      {
         if (key is null)
         {
            throw new ArgumentNullException(error);
         }
      }

      #endregion

      #region Splayed Dictionary Entry object which forms the tree structure.

      protected class SplayedEntry : IEquatable<KeyValuePair<K, V>>
      {
         internal SplayedEntry left;
         internal SplayedEntry right;
         internal SplayedEntry parent;

         private K key;
         private V value;

         // Insertion order chain used by LinkedSplayMap
         internal SplayedEntry linkNext;
         internal SplayedEntry linkPrev;

         public SplayedEntry() => Initialize(key, value);

         public SplayedEntry Initialize(K key, V value)
         {
            this.key = key;
            this.value = value;
            // Node is circular list to start.
            this.linkNext = this;
            this.linkPrev = this;

            return this;
         }

         public K Key => key;

         public V Value
         {
            get => value;
            set => this.value = value;
         }

         internal bool KeyEquals(K other) => EqualityComparer<K>.Default.Equals(other, key);

         internal bool ValueEquals(V other) => EqualityComparer<V>.Default.Equals(other, value);

         public bool Equals(KeyValuePair<K, V> other)
         {
            return EqualityComparer<K>.Default.Equals(other.Key, key) &&
                   EqualityComparer<V>.Default.Equals(other.Value, value);
         }

         internal KeyValuePair<K, V> ToKeyValuePair()
         {
            return new KeyValuePair<K, V>(key, value);
         }

         internal DictionaryEntry ToDictionaryEntry()
         {
            return new DictionaryEntry(key, value);
         }
      }

      #endregion
   }
}