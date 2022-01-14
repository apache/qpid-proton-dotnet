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
using Apache.Qpid.Proton.Utilities;

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

      private int state = STOPPED;

      private readonly IDeque<T> queue = new ArrayDeque<T>();

      public bool IsRunning => Volatile.Read(ref state) == RUNNING;

      public bool IsClosed => Volatile.Read(ref state) == CLOSED;

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
                  if (timeout == TimeSpan.MaxValue)
                  {
                     Monitor.Wait(this.mutex);
                  }
                  else
                  {
                     Monitor.Wait(this.mutex, timeout);
                  }
               }

               if (!IsEmpty && IsRunning)
               {
                  value = queue.Dequeue();
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
               value = queue.Dequeue();
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

            queue.Enqueue(delivery);

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

            queue.EnqueueFront(delivery);

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