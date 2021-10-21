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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Client.Utilities
{
   /// <summary>
   /// Simple first in / first out delivery queue with no reordering for
   /// priority or other criteria.
   /// </summary>
   /// <typeparam name="T">The type of delivery object this Queue manages</typeparam>
   public sealed class FifoDeliveryQueue<T> : IDeliveryQueue<T> where T : class
   {
      private readonly Mutex mutex = new Mutex();

      private static readonly int CLOSED = 0;
      private static readonly int STOPPED = 1;
      private static readonly int RUNNING = 2;

      private volatile int state = STOPPED;

      // TODO: This is slower and more garbage heavy than an array based double ended
      //       queue but .NET doesn't have that so for now use this and implement something
      //       more performant later.
      private readonly LinkedList<T> queue = new LinkedList<T>();

      public bool IsRunning => state == RUNNING;

      public bool IsClosed => state == CLOSED;

      public bool IsEmpty
      {
         get
         {
            lock (mutex)
            {
               return queue.Count == 0;
            }
         }
      }

      public int Count
      {
         get
         {
            lock (mutex)
            {
               return queue.Count;
            }
         }
      }

      public void Clear()
      {
         lock (mutex)
         {
            queue.Clear();
         }
      }

      public void Close()
      {
         if (Interlocked.Exchange(ref state, CLOSED) > CLOSED)
         {
            lock (mutex)
            {
               queue.Clear();

               Monitor.PulseAll(mutex);
            }
         }
      }

      public T Dequeue(TimeSpan timeout)
      {
         T value = default(T);

         lock (mutex)
         {
            if (IsRunning)
            {
               if (IsEmpty && timeout != TimeSpan.Zero)
               {
                  Monitor.Wait(this.mutex, timeout);
               }

               if (!IsEmpty && IsRunning)
               {
                  value = queue.First.ValueRef;
                  queue.RemoveFirst();
               }
            }
         }

         return value;
      }

      public T DequeueNoWait()
      {
         T value = null;

         lock (mutex)
         {
            if (IsRunning && queue.Count > 0)
            {
               value = queue.First.ValueRef;
               queue.RemoveFirst();
            }
         }

         return value;
      }

      public void Enqueue(T delivery)
      {
         lock (mutex)
         {
            if (IsClosed)
            {
               return;
            }

            queue.AddLast(delivery);

            Monitor.Pulse(mutex);
         }
      }

      public void EnqueueFront(T delivery)
      {
         lock (mutex)
         {
            if (IsClosed)
            {
               return;
            }

            queue.AddFirst(delivery);

            Monitor.Pulse(mutex);
         }
      }

      public void Start()
      {
         if (Interlocked.CompareExchange(ref state, RUNNING, STOPPED) == STOPPED)
         {
            lock (mutex)
            {
               Monitor.PulseAll(mutex);
            }
         }
      }

      public void Stop()
      {
         if (Interlocked.CompareExchange(ref state, STOPPED, RUNNING) == RUNNING)
         {
            lock (mutex)
            {
               Monitor.PulseAll(mutex);
            }
         }
      }
   }
}