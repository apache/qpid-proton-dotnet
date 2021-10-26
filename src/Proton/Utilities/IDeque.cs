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
   /// insert operation is designed specifically for use with capacity-restricted Deque
   /// implementations; in most implementations, insert operations cannot fail.
   /// </remarks>
   /// <typeparam name="T">The type that is carried in this collection</typeparam>
   public interface IDeque<T> : ICollection<T>, IEnumerable<T>, IReadOnlyCollection<T>, IEnumerable, ICollection
   {
      bool TryAddFirst(T value);

      bool TryAddLast(T value);

      bool TryRemoveFirst(out T value);

      bool TryRemoveLast(out T value);

      bool TryPeekFirst(out T value);

      bool TryPeekLast(out T value);

   }
}