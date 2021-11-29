/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Threading;

namespace Apache.Qpid.Proton.Client.Concurrent
{
   /// <summary>
   /// Simple Atomic abstraction around the long type to make atomic
   /// operations on long types simpler to manage in code. The default
   /// value of this type is zero.
   /// </summary>
   public class AtomicLong
   {
      private long value;

      /// <summary>
      /// Create a new instance with the given value or default to zero.
      /// </summary>
      /// <param name="initialValue">Initial value for the long </param>
      public AtomicLong(long initialValue = 0)
      {
         this.value = initialValue;
      }

      /// <summary>
      /// Atomically sets the value to the new one if the current value == expected.
      /// </summary>
      /// <param name="expect">The value that is expected</param>
      /// <param name="update">The value to assign if the expectation is met.</param>
      /// <returns></returns>
      public bool CompareAndSet(long expect, long update)
      {
         return expect == Interlocked.CompareExchange(ref value, expect, update);
      }

      /// <summary>
      /// Atomically increments the current value.
      /// </summary>
      /// <returns>The incremented value</returns>
      public long IncrementAndGet()
      {
         return Interlocked.Increment(ref value);
      }

      /// <summary>
      /// Atomically decrements the current value.
      /// </summary>
      /// <returns>The decremented value</returns>
      public long DecrementAndGet()
      {
         return Interlocked.Decrement(ref value);
      }

      /// <summary>
      /// Read the value of the long atomically and return it.
      /// </summary>
      public long Value => Get();

      /// <summary>
      /// Reads the value of the long atomically and returns it.
      /// </summary>
      /// <returns>The value of the long</returns>
      public long Get()
      {
         return Interlocked.Read(ref value);
      }

      /// <summary>
      /// Performs an atomic write of the long value.
      /// </summary>
      /// <param name="newValue">The new value to assign to the long</param>
      public void Set(long newValue)
      {
         Interlocked.Exchange(ref value, newValue);
      }

      public static implicit operator long(AtomicLong atomicLong)
      {
         return atomicLong.value;
      }
   }
}