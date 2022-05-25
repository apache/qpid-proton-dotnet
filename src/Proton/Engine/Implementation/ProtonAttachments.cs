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

using System.Collections.Generic;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   public sealed class ProtonAttachments : IAttachments
   {
      private readonly IDictionary<string, object> entries = new Dictionary<string, object>();

      public object this[string key]
      {
         get => entries[key];
         set => entries[key] = value;
      }

      public IAttachments Clear()
      {
         entries.Clear();
         return this;
      }

      public bool Contains(in string key)
      {
         return entries.ContainsKey(key);
      }

      public T Get<T>(in string key, in T defaultValue)
      {
         if (entries.ContainsKey(key))
         {
            return (T)entries[key];
         }
         else
         {
            return defaultValue;
         }
      }

      public IAttachments Set(in string key, in object value)
      {
         entries.Add(key, value);
         return this;
      }

      public bool TryGet(in string key, out object value)
      {
         if (entries.ContainsKey(key))
         {
            value = entries[key];
            return true;
         }
         else
         {
            value = null;
            return false;
         }
      }
   }
}