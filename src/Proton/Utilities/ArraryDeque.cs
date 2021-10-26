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
   /// A resizable-array implementation of the IDeque interface which has no capacity restrictions.
   /// The backing array will grow as necessary to support usage. This collection is not thread-safe.
   /// Null elements are prohibited in this collection.
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public sealed class ArrayDeque<T> : IDeque<T>
   {
      public int Count => throw new NotImplementedException();

      public bool IsReadOnly => throw new NotImplementedException();

      public bool IsSynchronized => throw new NotImplementedException();

      public object SyncRoot => throw new NotImplementedException();

      public void Add(T item)
      {
         throw new NotImplementedException();
      }

      public void Clear()
      {
         throw new NotImplementedException();
      }

      public bool Contains(T item)
      {
         throw new NotImplementedException();
      }

      public void CopyTo(T[] array, int arrayIndex)
      {
         throw new NotImplementedException();
      }

      public void CopyTo(Array array, int index)
      {
         throw new NotImplementedException();
      }

      public IEnumerator<T> GetEnumerator()
      {
         throw new NotImplementedException();
      }

      public bool Remove(T item)
      {
         throw new NotImplementedException();
      }

      public bool TryAddFirst(T value)
      {
         throw new NotImplementedException();
      }

      public bool TryAddLast(T value)
      {
         throw new NotImplementedException();
      }

      public bool TryPeekFirst(out T value)
      {
         throw new NotImplementedException();
      }

      public bool TryPeekLast(out T value)
      {
         throw new NotImplementedException();
      }

      public bool TryRemoveFirst(out T value)
      {
         throw new NotImplementedException();
      }

      public bool TryRemoveLast(out T value)
      {
         throw new NotImplementedException();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         throw new NotImplementedException();
      }
   }
}