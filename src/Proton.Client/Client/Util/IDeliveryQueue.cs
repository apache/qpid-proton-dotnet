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
using System.Threading;

namespace Apache.Qpid.Proton.Client.Util
{
   /// <summary>
   /// A generic delivery queue used to hold messages or other delivery objects
   /// that need to be consumed in order and also provides put-back options for
   /// redelivery scenarios.
   /// </summary>
   /// <typeparam name="T">The type of delivery object this Queue manages</typeparam>
   public interface IDeliveryQueue<T> where T : class
   {
      /// <summary>
      /// Retrieves the number of queued deliveries current held in this delivery queue.
      /// </summary>
      int Count { get; }

      /// <summary>
      /// Checks if the delivery queue is currently empty or not.
      /// </summary>
      /// <returns>true if there are no currently queued deliveries</returns>
      bool IsEmpty { get; }

      /// <summary>
      /// Checks if the queue has been stopped or closed, if not then the queue
      /// is in a running state.
      /// </summary>
      /// <returns>true if the queue is currently not stopped or closed</returns>
      bool IsRunning { get; }

      /// <summary>
      /// Checks if the queue has been closed or is still available for use. When
      /// this method returns false this queue can still be in a stopped state and
      /// may not be returning deliveries.
      /// </summary>
      /// <returns>true if the queue is currently not closed</returns>
      bool IsClosed { get; }

      /// <summary>
      /// Adds the given delivery to the end of the delivery queue.
      /// </summary>
      /// <param name="">The Delivery to add to the Queue</param>
      void Enqueue(T delivery);

      /// <summary>
      /// Adds the given delivery to the front of the delivery queue.
      /// </summary>
      /// <param name="">The Delivery to add to the Queue</param>
      void EnqueueFront(T delivery);

      /// <summary>
      /// Used to get the next available Delivery. The amount of time this method blocks is based
      /// on the timeout value that is supplied to it.
      /// </summary>
      /// <param name="timeout">The time to wait for a delivery to arrive</param>
      /// <returns>The next delivery in the Queue if one arrives within the time span</returns>
      /// <exception cref="ThreadInterruptedException">If the waiting thread is interrupted</exception>
      T Dequeue(TimeSpan timeout);

      /// <summary>
      /// Dequeue and return the next available delivery if one is available without the
      /// need to block, otherwise returns null.
      /// </summary>
      /// <returns>The next available delivery or null if none ready</returns>
      T DequeueNoWait();

      /// <summary>
      /// Starts the Queue and makes it available for dequeue operations, a non-started
      /// queue will always return null for any dequeue operations but will accept new
      /// queued deliveries for later dequeue when started.
      /// </summary>
      void Start();

      /// <summary>
      /// Stops the delivery Queue which wakes any current waiters and prevents any
      /// future calls to dequeue a delivery from blocking. New incoming deliveries
      /// can still be queued in this state.
      /// </summary>
      void Stop();

      /// <summary>
      /// Close the delivery Queue and purge any current deliveries and wakes all
      /// waiters currently blocked on a dequeue call. Once closed a delivery queue
      /// cannot be started again and no new deliveries can be queued.
      /// </summary>
      void Close();

      /// <summary>
      /// Purge all currently queued deliveries from this delivery queue.
      /// </summary>
      void Clear();

   }
}