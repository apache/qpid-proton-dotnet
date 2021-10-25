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
   /// Provides a boolean value that may be updated and read atomically.
   /// </summary>
   public class AtomicBoolean
   {
      private int value;

      /// <summary>
      /// Creates a new atomic boolean instance with either the given value or
      /// defaults to false.
      /// </summary>
      /// <param name="initialValue">The initial value to provide to the boolean</param>
      public AtomicBoolean(bool initialValue = false)
      {
         value = initialValue ? 1 : 0;
      }

      /// <summary>
      /// Atomically sets the value to the new one if the current value == expected.
      /// </summary>
      /// <param name="expect">The value that is expected</param>
      /// <param name="update">The value to assign if the expectation is met.</param>
      /// <returns></returns>
      public bool CompareAndSet(bool expect, bool update)
      {
         int expectation = expect ? 1 : 0;
         int pending = update ? 1 : 0;
         return expectation == Interlocked.CompareExchange(ref value, pending, expectation);
      }

      /// <summary>
      /// Read the value of the boolean atomically and return it.
      /// </summary>
      public bool Value => value == 1;

      /// <summary>
      /// Reads the value of the boolean atomically and returns it.
      /// </summary>
      /// <returns>The value of the boolean</returns>
      public bool Get()
      {
         return Value;
      }

      /// <summary>
      /// Performs an atomic write of the boolean value.
      /// </summary>
      /// <param name="newValue">The new value to assign to the boolean</param>
      public void Set(bool newValue)
      {
         Interlocked.Exchange(ref value, newValue ? 1 : 0);
      }

      public static implicit operator bool(AtomicBoolean atomicBool)
      {
         return atomicBool.Value;
      }
   }
}