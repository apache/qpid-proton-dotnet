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
using System.Linq;

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// A resizable-array implementation of the IDeque interface which has no capacity restrictions.
   /// The backing array will grow as necessary to support usage. This collection is not thread-safe.
   /// Null elements are prohibited in this collection.
   /// </summary>
   /// <typeparam name="T">The type that is stored in this double ended queue implementation</typeparam>
   public sealed class ArrayDeque<T> : IDeque<T>
   {
      public readonly object sync = new object();

      public const int DefaultInitialSize = 16;
      public const int AtLeastOne = 1;

      private T[] elements;
      private int head;
      private int tail;

      private int count;

      public ArrayDeque(uint size = DefaultInitialSize)
      {
         elements = new T[size == DefaultInitialSize ? size : ((size == Int32.MaxValue ? Int32.MaxValue : size + 1))];
      }

      public ArrayDeque(IEnumerable<T> elements) : this((uint)elements.Count())
      {
         foreach (T element in elements)
         {
            AddLast(element);
         }
      }

      public bool IsEmpty => head == tail;

      public int Count => count;

      public bool IsReadOnly => false;

      public bool IsSynchronized => false;

      public object SyncRoot => sync;

      public void AddFirst(T item)
      {
         if (item == null)
         {
            throw new ArgumentNullException("Values added to an array deque cannot be null");
         }

         T[] localElements = this.elements;
         localElements[head = CircularReduce(head, localElements.Length)] = item;

         if (++count == localElements.Length)
         {
            EnsureAdditionalCapacity(AtLeastOne);
         }
      }

      public void AddLast(T item)
      {
         if (item == null)
         {
            throw new ArgumentNullException("Values added to an array deque cannot be null");
         }

         throw new NotImplementedException();
      }

      public void Add(T item)
      {
         AddLast(item);
      }

      public void Clear()
      {
         throw new NotImplementedException();
      }

      public bool Contains(T item)
      {
         throw new NotImplementedException();
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
         throw new NotImplementedException();
      }

      public void CopyTo(Array array, int index)
      {
         throw new NotImplementedException();
      }

      public bool Remove(T item)
      {
         throw new NotImplementedException();
      }

      public bool TryAddFirst(T value)
      {
         throw new NotImplementedException();
      }

      public bool TryAddLast(T value)
      {
         throw new NotImplementedException();
      }

      public bool TryPeekFirst(out T value)
      {
         throw new NotImplementedException();
      }

      public bool TryPeekLast(out T value)
      {
         throw new NotImplementedException();
      }

      public bool TryRemoveFirst(out T value)
      {
         throw new NotImplementedException();
      }

      public bool TryRemoveLast(out T value)
      {
         throw new NotImplementedException();
      }

      public IEnumerator<T> GetEnumerator()
      {
         return new ArrayDequeEnumerator(this);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return new ArrayDequeEnumerator(this);
      }

      #region array element access and updates

      private void EnsureAdditionalCapacity(int atLeast)
      {

      }

      private static int CircularReduce(int i, int length) {
         if (--i < 0)
         {
            i = length - 1;
         }

         return i;
      }

      #endregion

      #region ArrayDeque enumerator implementation

      private class ArrayDequeEnumerator : IEnumerator<T>
      {
         private ArrayDeque<T> parent;

         public ArrayDequeEnumerator(ArrayDeque<T> parent)
         {
            this.parent = parent;
         }

         public T Current => throw new NotImplementedException();

         object IEnumerator.Current => throw new NotImplementedException();

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

      #endregion
   }
}