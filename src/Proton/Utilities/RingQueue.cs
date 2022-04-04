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
   /// Simple Ring Queue implementation that has an enforced max size value.
   /// </summary>
   /// <typeparam name="T">The type contained in the Queue</typeparam>
   public sealed class RingQueue<T> : ICollection, IEnumerable<T>, IReadOnlyCollection<T>
   {
      private int read = 0;
      private int write = -1;
      private int size;

      private readonly T[] backingArray;

      /// <summary>
      /// Creates a new RingQueue instance with the given fixed queue size.
      /// </summary>
      /// <param name="queueSize"></param>
      public RingQueue(int queueSize)
      {
         this.backingArray = new T[queueSize];
      }

      /// <summary>
      /// Create a new RingQueue instance with elements of the given collection as
      /// the initial values contained in the queue.
      /// </summary>
      /// <param name="elements"></param>
      public RingQueue(IReadOnlyCollection<T> elements)
      {
         this.backingArray = new T[elements.Count];

         foreach (T item in elements)
         {
            Offer(item);
         }
      }

      #region IEnumerable and IEnumerable<T>

      IEnumerator IEnumerable.GetEnumerator() => new RingIterator(this);

      public IEnumerator<T> GetEnumerator() => new RingIterator(this);

      #endregion

      #region ICollection implementation

      public int Count => size;

      public bool IsSynchronized => false;

      public object SyncRoot => backingArray;

      public void CopyTo(Array array, int index)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array), "Array parameter cannot be null");
         }

         if (array.Rank > 1)
         {
            throw new ArgumentException("Array cannot be multi-dimensional");
         }

         if (array is not T[] tArray)
         {
            throw new ArgumentException("Target array cannot be cast to the correct type");
         }

         this.CopyTo(tArray, index);
      }

      #endregion

      #region RingQueue implementation

      public void CopyTo(T[] array, int index)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array), "Array parameter cannot be null");
         }

         if (array.Rank > 1)
         {
            throw new ArgumentException("Array cannot be multi-dimensional");
         }

         if (index < 0)
         {
            throw new ArgumentOutOfRangeException(nameof(index), "Index parameter cannot be negative");
         }

         if (array.Length - index < size)
         {
            throw new ArgumentException("Not enough space inf given array to hold the queue elements.");
         }

         foreach (T item in this)
         {
            array[index++] = item;
         }
      }

      /// <summary>
      /// Checks if this queue is empty or not.
      /// </summary>
      /// <returns>true if the queue has no elements</returns>
      public bool IsEmpty() => size == 0;

      /// <summary>
      /// Clears all elements from this queue and resets to an empty state.
      /// </summary>
      public void Clear()
      {
         read = 0;
         write = -1;
         size = 0;

         Array.Fill(backingArray, default(T));
      }

      /// <summary>
      /// Inserts the specified element into this queue if it is possible to do so
      /// immediately without violating capacity restrictions.
      /// </summary>
      /// <param name="e">The value to insert into the queue</param>
      /// <returns>true if the element was inserted successfully</returns>
      public bool Offer(T e)
      {
         if (IsFull())
         {
            return false;
         }
         else
         {
            write = Advance(write, backingArray.Length);
            size++;
            backingArray[write] = e;
            return true;
         }
      }

      /// <summary>
      /// Returns the value at the head of the queue if one is present or
      /// returns the default value for the type contained in the queue
      /// if empty.
      /// </summary>
      /// <returns></returns>
      public T Poll()
      {
         if (IsEmpty())
         {
            return default(T);
         }
         else
         {
            T result = backingArray[read];
            backingArray[read] = default(T);
            read = Advance(read, backingArray.Length);
            size--;
            return result;
         }
      }

      /// <summary>
      /// Retrieves and removes the head of this ring queue, and if the queue is
      /// currently empty a new instance of the queue type is provided by invoking
      /// the given function to supply a default value.
      /// </summary>
      /// <param name="createOnEmpty">Supplier function for value when empty</param>
      /// <returns>A value from the queue or a supplied value</returns>
      public T Poll(Func<T> createOnEmpty)
      {
         if (IsEmpty())
         {
            return createOnEmpty.Invoke();
         }
         else
         {
            return Poll();
         }
      }

      /// <summary>
      ///
      /// </summary>
      /// <returns>The head of the queue or the default value contained type</returns>
      public T Peek()
      {
         return IsEmpty() ? default(T) : backingArray[read];
      }

      /// <summary>
      /// Search the queue for the specified value and returns true if found.
      /// </summary>
      /// <param name="value">The value whose presence is being queried</param>
      /// <returns>true if the queue contains the given value.</returns>
      public bool Contains(T value)
      {
         int count = size;
         int position = read;

         if (value == null)
         {
            while (count > 0)
            {
               if (backingArray[position] == null)
               {
                  return true;
               }
               position = Advance(position, backingArray.Length);
               count--;
            }
         }
         else
         {
            while (count > 0)
            {
               if (value.Equals(backingArray[position]))
               {
                  return true;
               }
               position = Advance(position, backingArray.Length);
               count--;
            }
         }

         return false;
      }

      private bool IsFull()
      {
         return size == backingArray.Length;
      }

      private static int Advance(int value, int limit)
      {
         return (++value) % limit;
      }

      #endregion

      #region An Enumerator implementation for the RingQueue

      private sealed class RingIterator : IEnumerator<T>
      {
         private readonly RingQueue<T> parent;
         private readonly int expectedSize;
         private readonly int expectedReadIndex;

         private T nextElement;
         private int position;
         private int remaining;

         public RingIterator(RingQueue<T> parent)
         {
            this.parent = parent;
            this.expectedReadIndex = parent.read;
            this.expectedSize = parent.size;

            this.nextElement = default(T);
            this.position = parent.read;
            this.remaining = parent.size;
         }

         public void Dispose()
         {
            nextElement = default(T);
         }

         object IEnumerator.Current => this.nextElement;

         public T Current => nextElement;

         public bool MoveNext()
         {
            if (expectedSize != parent.size || expectedReadIndex != parent.read)
            {
               throw new InvalidOperationException("Parent Queue was modified during enumeration");
            }
            else if (remaining <= 0)
            {
               nextElement = default(T);
               return false;
            }
            else
            {
               nextElement = parent.backingArray[position];
               remaining--;
               position = RingQueue<T>.Advance(position, parent.backingArray.Length);

               return true;
            }
         }

         public void Reset()
         {
            this.nextElement = default(T);
            this.position = parent.read;
            this.remaining = parent.size;
         }
      }

      #endregion
   }
}