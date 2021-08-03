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
   /// Simple Ring Queue implementation that has an enforced max size value.
   /// </summary>
   /// <typeparam name="T">The type contained in the Queue</typeparam>
   public class SplayedDictionary<K, V> : ICollection<KeyValuePair<K, V>>, IDictionary<K, V>, IEnumerable, IEnumerable<KeyValuePair<K, V>>
   {
      private IComparer<K> keyComparer = Comparer<K>.Default;

      /// <summary>
      /// Pooled entries used to prevent excessive allocations for rapied insert and removal operations.
      /// </summary>
      protected readonly RingQueue<SplayedEntry> entryPool = new RingQueue<SplayedEntry>(64);

      /// <summary>
      /// Root of the tree which can change as entries are splayed up and down the tree
      /// </summary>
      protected SplayedEntry root;

      /// <summary>
      /// Tracked tree size to prevent traversals for size operations.
      /// </summary>
      protected int size;

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
         if (comparer == null)
         {
            throw new ArgumentNullException("Provided key comparer must not be null");
         }

         this.keyComparer = comparer;
      }

      #region Splayed Dictionary API implementations

      public int Count => size;

      public bool IsReadOnly => false;

      public void Add(K key, V value)
      {
         if (key is null)
         {
            throw new ArgumentNullException("Cannot add an entry with a null key");
         }

         if (root == null)
         {
            root = entryPool.Poll(CreateEntry).Initialize(key, value);
         }
         else
         {
            root = Splay(root, key);
            if (root.KeyEquals(key))
            {
               throw new ArgumentException("An entry with the given key already exists");
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

         EntryAdded(root);
         size++;
      }

      public void Add(KeyValuePair<K, V> item)
      {
         Add(item.Key, item.Value);
      }

      public void Clear()
      {
         root = null;
         size = 0;
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
         if (root == null)
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

      public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
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

         for (SplayedEntry entry = FirstEntry(root); entry != null; entry = Successor(entry))
         {
            array[arrayIndex++] = entry.ToKeyValuePair();
         }
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
            }
         }

         return removed;
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
            }
         }

         return removed;
      }

      public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
      {
         if (key is null)
         {
            throw new ArgumentNullException("Cannot get an entry with a null key");
         }

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
               value = default(V);
            }
         }
         else
         {
            value = default(V);
         }

         return result;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         throw new NotImplementedException();
      }

      public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
      {
         throw new NotImplementedException();
      }

      public V this[K key]
      {
         get => throw new NotImplementedException();
         set => throw new NotImplementedException();
      }

      public ICollection<K> Keys
      {
         get
         {
            if (keySet == null)
            {
               // TODO
            }

            return keySet;
         }
      }

      public ICollection<V> Values
      {
         get
         {
            if (values == null)
            {
               // TODO
            }

            return values;
         }
      }

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

      private abstract class SplayedDictionaryEnumerator<TResult> : IEnumerator<TResult>
      {
         object IEnumerator.Current => this.Current;

         public abstract TResult Current { get; }

         public void Dispose()
         {
            throw new NotImplementedException();
         }

         public bool MoveNext()
         {
            throw new NotImplementedException();
         }

         public void Reset()
         {
            throw new NotImplementedException();
         }
      }

      private sealed class SplayedDictionaryKeyEnumerator : SplayedDictionaryEnumerator<K>
      {
         public override K Current => throw new NotImplementedException();
      }

      private sealed class SplayedDictionaryValueEnumerator : SplayedDictionaryEnumerator<V>
      {
         public override V Current => throw new NotImplementedException();
      }

      private sealed class SplayedDictionaryEntryEnumerator : SplayedDictionaryEnumerator<KeyValuePair<K, V>>
      {
         public override KeyValuePair<K, V> Current => throw new NotImplementedException();
      }

      #endregion

      #region Dictionary Collection Views

      private sealed class SplayedDictionaryValues : ICollection<V>
      {
         public int Count => throw new NotImplementedException();

         public bool IsReadOnly => throw new NotImplementedException();

         public void Add(V item)
         {
            throw new NotImplementedException();
         }

         public void Clear()
         {
            throw new NotImplementedException();
         }

         public bool Contains(V item)
         {
            throw new NotImplementedException();
         }

         public void CopyTo(V[] array, int arrayIndex)
         {
            throw new NotImplementedException();
         }

         public IEnumerator<V> GetEnumerator()
         {
            throw new NotImplementedException();
         }

         public bool Remove(V item)
         {
            throw new NotImplementedException();
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            throw new NotImplementedException();
         }
      }

      private sealed class SplayedDictionaryKeys : ICollection<K>
      {
         public int Count => throw new NotImplementedException();

         public bool IsReadOnly => throw new NotImplementedException();

         public void Add(K item)
         {
            throw new NotImplementedException();
         }

         public void Clear()
         {
            throw new NotImplementedException();
         }

         public bool Contains(K item)
         {
            throw new NotImplementedException();
         }

         public void CopyTo(K[] array, int arrayIndex)
         {
            throw new NotImplementedException();
         }

         public bool Remove(K item)
         {
            throw new NotImplementedException();
         }

         public IEnumerator<K> GetEnumerator()
         {
            throw new NotImplementedException();
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            throw new NotImplementedException();
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

      private SplayedEntry RightRotate(SplayedEntry node)
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

      private SplayedEntry LeftRotate(SplayedEntry node)
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

      private SplayedEntry FirstEntry(SplayedEntry node)
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

      private SplayedEntry LastEntry(SplayedEntry node)
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

      private SplayedEntry Successor(SplayedEntry node)
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

      private SplayedEntry Predecessor(SplayedEntry node)
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

         public V Value => value;

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
      }

      #endregion
   }
}