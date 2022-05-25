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
   /// A linear collection type that supports element insertion and removal at both ends.
   /// This double ended queue type will most commonly be implemented with no underlying
   /// fixed capacity limit however the interface allows for restricted capacity versions
   /// to be implemented all the same.
   /// </summary>
   /// <remarks>
   /// This interface defines methods to access the elements at both ends of the deque.
   /// Methods are provided to insert, remove, and or peek at elements. Each of these methods
   /// exists in two forms: one throws an exception if the operation fails, the other returns a
   /// boolean value to indicate if the operation succeeded or failed. The try based form of the
   /// insert operation is designed specifically for use with capacity-restricted queue
   /// implementations; in most implementations, insert operations cannot fail.
   /// </remarks>
   /// <typeparam name="T">The type that is carried in this collection</typeparam>
   public interface IDeque<T> : ICollection<T>, IEnumerable<T>, IReadOnlyCollection<T>, IEnumerable,
                                ICollection, IEquatable<IEnumerable<T>>
   {
      /// <summary>
      /// Returns true if the double ended queue is currently empty. This method provides
      /// an optimized means of checking for empty in this collection type where otherwise
      /// the count property might be used which could require a calculation on each call
      /// to determine the current element count.
      /// </summary>
      bool IsEmpty { get; }

      /// <summary>
      /// Inserts the given value onto the back of this queue unless the queue has reached
      /// its capacity limit in which case this method would throw an exception. Generally it
      /// is preferable to call the TryEnqueue method and check the return value to determine
      /// if the operation succeeded.
      /// </summary>
      /// <param name="value">The value to add to the tail of the queue</param>
      /// <exception cref="InvalidOperationException">If the queue is currently at max capacity</exception>
      void Enqueue(T value);

      /// <summary>
      /// Attempts to insert the given value onto the back of this queue unless the deque has
      /// reached its capacity limit in which case this method would throw an exception.
      /// </summary>
      /// <param name="value">The value to add to the front of the queue</param>
      /// <returns>True if the value was added to the deque</returns>
      bool TryEnqueue(T value);

      /// <summary>
      /// Inserts the given value onto the front of this queue unless the queue has reached
      /// its capacity limit in which case this method would throw an exception. Generally it
      /// is preferable to call the TryEnqueueFront method and check the return value to determine
      /// if the operation succeeded.
      /// </summary>
      /// <param name="value">The value to add to the front of the queue</param>
      /// <exception cref="InvalidOperationException">If the queue is currently at max capacity</exception>
      void EnqueueFront(T value);

      /// <summary>
      /// Attempts to insert the given value onto the front of this queue unless the queue has
      /// reached its capacity limit in which case this method would throw an exception.
      /// </summary>
      /// <param name="value">The value to add to the front of the queue</param>
      /// <returns>True if the value was added to the deque</returns>
      bool TryEnqueueFront(T value);

      /// <summary>
      /// Inserts the given value onto the back of this queue unless the deque has reached
      /// its capacity limit in which case this method would throw an exception. Generally it
      /// is preferable to call the TryEnqueueBack method and check the return value to determine
      /// if the operation succeeded.
      /// </summary>
      /// <param name="value">The value to add to the back of the queue</param>
      /// <exception cref="InvalidOperationException">If the queue is currently at max capacity</exception>
      void EnqueueBack(T value);

      /// <summary>
      /// Attempts to insert the given value onto the back of this queue unless the deque has
      /// reached its capacity limit in which case this method would throw an exception.
      /// </summary>
      /// <param name="value">The value to add to the front of the queue</param>
      /// <returns>True if the value was added to the deque</returns>
      bool TryEnqueueBack(T value);

      /// <summary>
      /// Removes and returns the element at the front of the queue if the queue is currently
      /// not empty, otherwise this method throws an exception to indicate that there is no
      /// value in the queue currently. Generally it is preferable to call the TryDequeue
      /// method and check the return value to see if the operation succeeded.
      /// </summary>
      /// <returns>The element at the front of the queue</returns>
      /// <exception cref="InvalidOperationException">If the queue is currently empty</exception>
      T Dequeue();

      /// <summary>
      /// Attempt to remove and return the element at the front of the queue if there is
      /// any element to return otherwise the method returns false.
      /// </summary>
      /// <param name="front">A reference to store the value at the front of the queue</param>
      /// <returns>True if a value was removed from the queue and returned.</returns>
      bool TryDequeue(out T front);

      /// <summary>
      /// Returns the element at the front of the queue if the queue is currently not empty,
      /// otherwise this method throws an exception to indicate that there is no value in the
      /// queue currently. Generally it is preferable to call the TryPeek method and check
      /// the return value to see if the operation succeeded.
      /// </summary>
      /// <returns>The element at the front of the queue</returns>
      /// <exception cref="InvalidOperationException">If the queue is currently empty</exception>
      T Peek();

      /// <summary>
      /// Attempt to read and return the element at the front of the queue if there is
      /// any element to return otherwise the method returns false.
      /// </summary>
      /// <param name="front">A reference to store the value at the front of the queue</param>
      /// <returns>True if a value was read from the queue and returned.</returns>
      bool TryPeek(out T front);

      /// <summary>
      /// Returns the element at the front of the queue if the queue is currently not empty,
      /// otherwise this method throws an exception to indicate that there is no value in the
      /// queue currently. Generally it is preferable to call the TryPeek method and check
      /// the return value to see if the operation succeeded.
      /// </summary>
      /// <returns>The element at the front of the queue</returns>
      /// <exception cref="InvalidOperationException">If the queue is currently empty</exception>
      T PeekFront();

      /// <summary>
      /// Attempt to read and return the element at the front of the queue if there is
      /// any element to return otherwise the method returns false.
      /// </summary>
      /// <param name="front">A reference to store the value at the front of the queue</param>
      /// <returns>True if a value was read from the queue and returned.</returns>
      bool TryPeekFront(out T front);

      /// <summary>
      /// Returns the element at the front of the queue if the queue is currently not empty,
      /// otherwise this method throws an exception to indicate that there is no value in the
      /// queue currently. Generally it is preferable to call the TryPeek method and check
      /// the return value to see if the operation succeeded.
      /// </summary>
      /// <returns>The element at the front of the queue</returns>
      /// <exception cref="InvalidOperationException">If the queue is currently empty</exception>
      T PeekBack();

      /// <summary>
      /// Attempt to read and return the element at the front of the queue if there is
      /// any element to return otherwise the method returns false.
      /// </summary>
      /// <param name="front">A reference to store the value at the front of the queue</param>
      /// <returns>True if a value was read from the queue and returned.</returns>
      bool TryPeekBack(out T front);

      /// <summary>
      /// Removes and returns the element at the front of the queue if the queue is currently
      /// not empty, otherwise this method throws an exception to indicate that there is no
      /// value in the queue currently. Generally it is preferable to call the TryDequeueFront
      /// method and check the return value to see if the operation succeeded.
      /// </summary>
      /// <returns>The element at the front of the queue</returns>
      /// <exception cref="InvalidOperationException">If the queue is currently empty</exception>
      T DequeueFront();

      /// <summary>
      /// Attempt to remove and return the element at the front of the queue if there is
      /// any element to return otherwise the method returns false.
      /// </summary>
      /// <param name="front">A reference to store the value at the front of the queue</param>
      /// <returns>True if a value was removed from the queue and returned.</returns>
      bool TryDequeueFront(out T front);

      /// <summary>
      /// Removes and returns the element at the back of the queue if the queue is currently
      /// not empty, otherwise this method throws an exception to indicate that there is no
      /// value in the queue currently. Generally it is preferable to call the TryDequeue
      /// method and check the return value to see if the operation succeeded.
      /// </summary>
      /// <returns>The element at the back of the queue</returns>
      /// <exception cref="InvalidOperationException">If the queue is currently empty</exception>
      T DequeueBack();

      /// <summary>
      /// Attempt to remove and return the element at the back of the queue if there is
      /// any element to return otherwise the method returns false.
      /// </summary>
      /// <param name="back">A reference to store the value at the back of the queue</param>
      /// <returns>True if a value was removed from the queue and returned.</returns>
      bool TryDequeueBack(out T back);

      /// <summary>
      /// Inserts the given value onto the back of this queue unless the deque has reached
      /// its capacity limit in which case this method would throw an exception. Generally it
      /// is preferable to call the TryEnqueueBack method and check the return value to determine
      /// if the operation succeeded.
      /// </summary>
      /// <param name="value">The value to add to the back of the queue</param>
      /// <exception cref="InvalidOperationException">If the queue is currently at max capacity</exception>
      void ICollection<T>.Add(T item)
      {
         EnqueueBack(item);
      }

      /// <summary>
      /// Returns the current number of elements contained in the double ended Queue.
      /// </summary>
      new int Count { get; }

   }
}