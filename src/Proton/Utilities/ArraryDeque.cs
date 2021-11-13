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
      private int head; // Where the item at the head of the queue is
      private int tail; // Where the next item added to the tail will go

      private int count;
      private int modCount;

      public ArrayDeque(uint size = DefaultInitialSize)
      {
         elements = new T[size == DefaultInitialSize ? size : ((size == Int32.MaxValue ? Int32.MaxValue : size + 1))];
      }

      public ArrayDeque(IEnumerable<T> elements) : this((uint)elements.Count())
      {
         foreach (T element in elements)
         {
            EnqueueBack(element);
         }
      }

      public bool IsEmpty => head == tail;

      public int Count => count;

      public bool IsReadOnly => false;

      public bool IsSynchronized => false;

      public object SyncRoot => sync;

      public void Enqueue(T value)
      {
         EnqueueBack(value);
      }

      public bool TryEnqueue(T value)
      {
         return TryEnqueueBack(value);
      }

      public T Dequeue()
      {
         return DequeueFront();
      }

      public bool TryDequeue(out T front)
      {
         return TryDequeueFront(out front);
      }

      public bool TryEnqueueFront(T item)
      {
         if (item == null)
         {
            throw new ArgumentNullException("Values added to an array deque cannot be null");
         }

         if (count == Int32.MaxValue)
         {
            return false;
         }

         T[] localElements = this.elements;
         // Head always point to an item in the queue at the front unless the queue
         // is empty so it must be reduced first then assigned to create a slot
         localElements[head = CircularReduce(head, localElements.Length)] = item;

         if (++count == localElements.Length)
         {
            EnsureAdditionalCapacity(AtLeastOne);
         }

         return true;
      }

      public void EnqueueFront(T item)
      {
         if (!TryEnqueueFront(item))
         {
            throw new InvalidOperationException("The double ended queue is full");
         }
      }

      public bool TryEnqueueBack(T item)
      {
         if (item == null)
         {
            throw new ArgumentNullException("Values added to an array deque cannot be null");
         }

         if (count == Int32.MaxValue)
         {
            return false;
         }

         modCount++;
         T[] localElements = this.elements;
         // Tail is always the location when an item will be added to the back
         // of the queue so we assign first then advance.
         localElements[tail] = item;
         tail = CircularAdvance(tail, localElements.Length);

         if (++count == localElements.Length)
         {
            EnsureAdditionalCapacity(AtLeastOne);
         }

         return true;
      }

      public void EnqueueBack(T item)
      {
         if (!TryEnqueueBack(item))
         {
            throw new InvalidOperationException("The double ended queue is full");
         }
      }

      public bool TryDequeueFront(out T result)
      {
         if (count == 0)
         {
            result = default(T);
            return false;
         }

         modCount++;
         T[] localElements = this.elements;
         int localHead = head;

         result = localElements[localHead];
         localElements[localHead] = default(T);
         head = CircularAdvance(localHead, localElements.Length);
         count--;

         return true;
      }

      public T DequeueFront()
      {
         T result;

         if (!TryDequeueFront(out result))
         {
            throw new InvalidOperationException("Failed to dequeue a value from the front of the queue");
         }

         return result;
      }

      public bool TryDequeueBack(out T result)
      {
         if (count == 0)
         {
            result = default(T);
            return false;
         }

         T[] localElements = this.elements;
         int newTail = tail = CircularReduce(tail, localElements.Length);

         result = localElements[newTail];
         localElements[newTail] = default(T);
         count--;

         return true;
      }

      public T DequeueBack()
      {
         T result;

         if (!TryDequeueBack(out result))
         {
            throw new InvalidOperationException("Failed to dequeue a value from the back of the queue");
         }

         return result;
      }

      public void Clear()
      {
         ClearDequeArray(elements, head, tail);
         head = tail = count = 0;
      }

      public bool Contains(T item)
      {
         foreach (T element in this)
         {
            if (element.Equals(item))
            {
               return true;
            }
         }

         return false;
      }

      public void CopyTo(T[] array, int index)
      {
         Statics.CheckFromIndexSize(index, count, array.Length);

         foreach (T element in this)
         {
            array[index++] = element;
         }
      }

      public void CopyTo(Array array, int index)
      {
         Statics.CheckFromIndexSize(index, count, array.Length);

         foreach (T element in this)
         {
            array.SetValue(element, index++);
         }
      }

      public bool Remove(T item)
      {
         throw new NotImplementedException("ArrayDeque does not currently support item removal");
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

      private int RemainginCapacity()
      {
         return elements.Length - count;
      }

      private void EnsureAdditionalCapacity(int atLeast)
      {
         if (atLeast == 0) return;

         int currentSize = elements.Length;

         // Grow by more than the at least value to try and prevent repeated resizing.
         int extraRoom = currentSize < sbyte.MaxValue ? Math.Max(currentSize, atLeast + 16) : atLeast + 16;

         if (extraRoom < 0)
         {
            extraRoom = currentSize + atLeast;
            if (extraRoom < 0)
            {
               throw new ArgumentOutOfRangeException("Cannot grow to meat requested capacity increase of: " + atLeast);
            }
         }

         T[] newElements = Statics.CopyOf(elements, currentSize + extraRoom);
         // for the wrapped head case we must push head elements forward to the end of the array
         // All other cases head would be less than tail and the copy would capture the state
         // without any disruptions to current element positions.
         if (tail < head || (tail == head && count != 0))
         {
            Array.ConstrainedCopy(elements, head, newElements, head + extraRoom, extraRoom);

            for (int i = head; i < head + extraRoom; ++i)
            {
               newElements[i] = default(T);
            }

            // Update head now that we've moved things.
            head = head + extraRoom;
         }

         this.elements = newElements;
      }

      private void ClearDequeArray(T[] elements, int head, int tail)
      {
         while (head != tail)
         {
            elements[head] = default(T);
            CircularAdvance(head, elements.Length);
         }
      }

      private static int CircularAdvance(int i, int length)
      {
         return ++i > length ? 0 : i;
      }

      private static int CircularReduce(int i, int length)
      {
         return --i >= 0 ? 0 : length - 1;
      }

      #endregion

      #region ArrayDeque enumerator implementation

      private class ArrayDequeEnumerator : IEnumerator<T>
      {
         private readonly ArrayDeque<T> parent;
         private readonly int expectedModCount;
         private int head = -1;

         public ArrayDequeEnumerator(ArrayDeque<T> parent)
         {
            this.parent = parent;
            this.expectedModCount = parent.modCount;
         }

         public T Current
         {
            get
            {
               if (parent.modCount != expectedModCount)
               {
                  throw new InvalidOperationException("The ArrayDeque was modified during enumeration");
               }

               if (head == -1)
               {
                  throw new InvalidOperationException("Must call MoveNext before a call to Current");
               }

               if (head == parent.tail)
               {
                  throw new InvalidOperationException("Cannot read beyond the end of the queue");
               }

               return parent.elements[head];
            }
         }

         object IEnumerator.Current => this.Current;

         public void Dispose()
         {
         }

         public bool MoveNext()
         {
            if (parent.modCount != expectedModCount)
            {
               throw new InvalidOperationException("The ArrayDeque was modified during enumeration");
            }

            head = head == -1 ? head = parent.head :
                                head = CircularAdvance(head, parent.elements.Length);

            return head != parent.tail;
         }

         public void Reset()
         {
            head = -1;
         }
      }

      #endregion
   }
}