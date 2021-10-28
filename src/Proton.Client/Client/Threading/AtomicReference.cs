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
   /// Provides an object reference that may be updated and read atomically.
   /// </summary>
   public class AtomicReference<T> where T : class
   {
      private T value;

      /// <summary>
      /// Creates a new AtomicReference instance where the contained value
      /// is default to null.
      /// </summary>
      public AtomicReference()
      {
      }

      /// <summary>
      /// Creates a new AtomicReference instance where the contained value
      /// is set to the provided value.
      /// </summary>
      /// <param name="value">The value to initialize the reference with</param>
      public AtomicReference(T value)
      {
         Set(value);
      }

      /// <summary>
      /// Atomic access to the contained value.
      /// </summary>
      public T Value
      {
         get => Get();
         set => Set(value);
      }

      /// <summary>
      /// Atomic read of the contained value.
      /// </summary>
      /// <returns>The value that was read</returns>
      public T Get()
      {
         return Volatile.Read(ref value);
      }

      /// <summary>
      /// Atomic write of the given value into the contained object reference.
      /// </summary>
      /// <param name="target">The object reference value to set.</param>
      public void Set(T target)
      {
         Volatile.Write(ref value, target);
      }

      /// <summary>
      /// Atomically set the value to the new value if and only if the value that is currently
      /// set matches the target expected value.
      /// </summary>
      /// <param name="expectedValue">The value that is required before the new value is applied</param>
      /// <param name="newValue">The new value to set if the current value matches the expectation</param>
      /// <returns></returns>
      public bool CompareAndSet(T expectedValue, T newValue)
      {
         T result = Interlocked.CompareExchange(ref value, newValue, expectedValue);
         return ReferenceEquals(expectedValue, result);
      }

      /// <summary>
      /// Compare the current value to the expected value and if they match set the current
      /// value to the new value as an atomic operation.
      /// </summary>
      /// <param name="expectedValue">the value that must currently be set</param>
      /// <param name="newValue">the value to set if the current value matches the expectation</param>
      /// <returns>The value that was set when the compare occurred which equals the expected if successful</returns>
      public T CompareAndExchange(T expectedValue, T newValue)
      {
         return Interlocked.CompareExchange(ref value, newValue, expectedValue);
      }

      /// <summary>
      /// Performs a to string operation on the currently set value.
      /// </summary>
      /// <returns>The stringified version of the current value</returns>
      public override string ToString()
      {
         return Get()?.ToString() ?? "<null>";
      }

      /// <summary>
      /// Implicit conversion of an atomic reference type to the contined value
      /// using a volatile read operation.
      /// </summary>
      /// <param name="reference">The atomic reference to read from</param>
      public static implicit operator T(AtomicReference<T> reference)
      {
         return reference.Value;
      }
   }
}