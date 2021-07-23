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
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// Simple Ring Queue implementation that has an enforced max size value.
   /// </summary>
   /// <typeparam name="T">The type contained in the Queue</typeparam>
   public class SplayedDictionary<K, V> : ICollection<KeyValuePair<K, V>>, IDictionary<K, V>, IEnumerable, IEnumerable<KeyValuePair<K, V>>
   {
      public V this[K key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public ICollection<K> Keys => throw new NotImplementedException();

      public ICollection<V> Values => throw new NotImplementedException();

      public int Count => throw new NotImplementedException();

      public bool IsReadOnly => throw new NotImplementedException();

      public void Add(K key, V value)
      {
         throw new NotImplementedException();
      }

      public void Add(KeyValuePair<K, V> item)
      {
         throw new NotImplementedException();
      }

      public void Clear()
      {
         throw new NotImplementedException();
      }

      public bool Contains(KeyValuePair<K, V> item)
      {
         throw new NotImplementedException();
      }

      public bool ContainsKey(K key)
      {
         throw new NotImplementedException();
      }

      public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
      {
         throw new NotImplementedException();
      }

      public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
      {
         throw new NotImplementedException();
      }

      public bool Remove(K key)
      {
         throw new NotImplementedException();
      }

      public bool Remove(KeyValuePair<K, V> item)
      {
         throw new NotImplementedException();
      }

      public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
      {
         throw new NotImplementedException();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         throw new NotImplementedException();
      }
   }
}