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
   /// <remarks>
   ///
   ///  0 1 2 3 4 5 6 7 8 9
   /// ---------------------
   /// | | | | | | | | | | |   Empty head = tail = 0
   /// ---------------------
   ///  H
   ///  T
   ///
   ///  0 1 2 3 4 5 6 7 8 9
   /// ---------------------
   /// |x|x|x|x|x| | | | | |   Size 5; Head = 0, tail = 5
   /// ---------------------
   ///  H         T
   ///
   ///  0 1 2 3 4 5 6 7 8 9
   /// ---------------------
   /// |x|x|x| | | | | |x|x|   Size 5; Head = 8, tail = 3
   /// ---------------------
   ///        T         H
   ///
   /// * Head always advances to meat tail even on wrap.
   /// * In the inverted case the gap between head and tail is empty elements
   ///
   /// Following capacity increase when head > tail (or head == tail because of insert when
   /// one slot left)
   ///
   ///  0 1 2 3 4 5 6 7 8 9
   /// -----------------------
   /// |x|x|x| | | | | |x|x| |   Size 5; Head = 8, tail = 3
   /// -----------------------
   ///        T         H
   ///
   /// </remarks>
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

      public bool TryEnqueue(T value)
      {
         return TryEnqueueBack(value);
      }

      public void Enqueue(T value)
      {
         EnqueueBack(value);
      }

      public bool TryDequeue(out T front)
      {
         return TryDequeueFront(out front);
      }

      public T Dequeue()
      {
         return DequeueFront();
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

         // Head always point to an item in the queue at the front unless the queue
         // is empty so it must be reduced first then assigned to create a slot
         elements[head = CircularReduce(head, elements.Length)] = item;

         if (++count == elements.Length)
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

         // Tail is always the location when an item will be added to the back
         // of the queue so we assign first then advance.
         elements[tail] = item;
         tail = CircularAdvance(tail, elements.Length);

         if (++count == elements.Length)
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

         result = elements[head];
         elements[head] = default(T);
         head = CircularAdvance(head, elements.Length);
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

         int newTail = tail = CircularReduce(tail, elements.Length);

         result = elements[newTail];
         elements[newTail] = default(T);
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

      public T Peek()
      {
         return PeekFront();
      }

      public bool TryPeek(out T front)
      {
         return TryPeekFront(out front);
      }

      public T PeekFront()
      {
         T result;

         if (!TryPeekFront(out result))
         {
            throw new InvalidOperationException("Cannot peek at front of empty queue");
         }

         return result;
      }

      public bool TryPeekFront(out T front)
      {
         if (count > 0)
         {
            front = elements[head];
            return true;
         }
         else
         {
            front = default(T);
            return false;
         }
      }

      public T PeekBack()
      {
         T result;

         if (!TryPeekBack(out result))
         {
            throw new InvalidOperationException("Cannot peek at back of empty queue");
         }

         return result;
      }

      public bool TryPeekBack(out T back)
      {
         if (count > 0)
         {
            back = elements[CircularReduce(tail, elements.Length)];
            return true;
         }
         else
         {
            back = default(T);
            return false;
         }
      }

      public void Clear()
      {
         ClearDequeArray(elements, head, tail);
         head = tail = count = 0;
         modCount++;
      }

      public bool Contains(T item)
      {
         foreach (T element in this)
         {
            if (element?.Equals(item) ?? false)
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
         if (count != 0)
         {
            int range = count;

            for (int i = head, j = 0; j < range; i = CircularAdvance(i, range), j++)
            {
               if (elements[i]?.Equals(item) ?? false)
               {
                  RemoveInternal(i);
                  return true;
               }
            }
         }

         return false;
      }

      public IEnumerator<T> GetEnumerator()
      {
         LinkedList<T> list = new LinkedList<T>();
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
         int extraRoom = currentSize < sbyte.MaxValue ?
               Math.Max(currentSize, atLeast + 16) :  // When smaller double if we can
               atLeast + (currentSize >> 1);          // When larger gro by 50% beyond the at least value

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
            Array.ConstrainedCopy(elements, head, newElements, head + extraRoom, elements.Length - head);

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
            head = CircularAdvance(head, elements.Length);
         }
      }

      private static int CircularAdvance(int i, int length)
      {
         return ++i < length ? i : 0;
      }

      private static int CircularReduce(int i, int length)
      {
         return --i < 0 ? length - 1 : i;
      }

      /// <summary>
      /// A more optimal algorithm would figure out which end is closer and
      /// shift the elements that direction using memory copies which could
      /// be implemented somewhat more efficiently than single element copies.
      /// </summary>
      private void RemoveInternal(int current)
      {
         int cursor = current;
         while (cursor != head)
         {
            int next = CircularReduce(cursor, elements.Length);
            elements[cursor] = elements[next];
            cursor = next;
         }
         head = CircularAdvance(head, elements.Length);

         elements[cursor] = default(T);
         count--;
         modCount++;
      }

      public override bool Equals(object other)
      {
         if (other is IEnumerable<T>)
         {
            return Equals((IEnumerable<T>)other);
         }

         return false;
      }

      public bool Equals(IEnumerable<T> other)
      {
         if (other == null || other.Count() != Count)
         {
            return false;
         }

         if (other == this)
         {
            return true;
         }

         foreach (T thisValue in this)
         {
            foreach(T otherValue in other)
            {
               if ((thisValue == null && otherValue != null) ||
                   (thisValue != null && otherValue == null))
               {
                  return false;
               }

               if (thisValue?.Equals(otherValue) ?? (thisValue == null && otherValue == null))
               {
                  return true;
               }
            }
         }

         return false;
      }

      public override int GetHashCode()
      {
         HashCode hash = new HashCode();

         foreach(T element in this)
         {
            hash.Add(element);
         }

         return hash.ToHashCode();
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