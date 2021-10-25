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

namespace Apache.Qpid.Proton.Client.Threading
{
   /// <summary>
   /// Simple Atomic abstraction around the integer type to make atomic
   /// operations on integer types simpler to manage in code. The default
   /// value of this type is zero.
   /// </summary>
   public class AtomicInteger
   {
      private volatile int value;

      /// <summary>
      /// Create a new instance with the given value or default to zero.
      /// </summary>
      /// <param name="initialValue">Initial value for the int </param>
      public AtomicInteger(int initialValue = 0)
      {
         this.value = initialValue;
      }

      /// <summary>
      /// Atomically sets the value to the new one if the current value == expected.
      /// </summary>
      /// <param name="expect">The value that is expected</param>
      /// <param name="update">The value to assign if the expectation is met.</param>
      /// <returns></returns>
      public bool CompareAndSet(int expect, int update)
      {
         return expect == Interlocked.CompareExchange(ref value, expect, update);
      }

      /// <summary>
      /// Atomically increments the current value.
      /// </summary>
      /// <returns>The incremented value</returns>
      public int IncrementAndGet()
      {
         return Interlocked.Increment(ref value);
      }

      /// <summary>
      /// Atomically decrements the current value.
      /// </summary>
      /// <returns>The decremented value</returns>
      public int DecrementAndGet()
      {
         return Interlocked.Decrement(ref value);
      }

      /// <summary>
      /// Read the value of the integer atomically and return it.
      /// </summary>
      public int Value => value;

      /// <summary>
      /// Reads the value of the integer atomically and returns it.
      /// </summary>
      /// <returns>The value of the integer</returns>
      public int Get()
      {
         return value;
      }

      /// <summary>
      /// Performs an atomic write of the integer value.
      /// </summary>
      /// <param name="newValue">The new value to assign to the integer</param>
      public void Set(int newValue)
      {
         Interlocked.Exchange(ref value, newValue);
      }

      public static implicit operator int(AtomicInteger atomic)
      {
         return atomic.value;
      }
   }
}