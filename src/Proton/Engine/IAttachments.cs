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

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Attachments API used to provide additional state data to live alongside
   /// specific Proton AMQP resources.
   /// </summary>
   public interface IAttachments
   {
      /// <summary>
      /// Gets or sets the attachment with the given key. If the element being
      /// retrieved is not in the collection an exception is thrown.
      /// </summary>
      /// <param name="key"></param>
      /// <returns></returns>
      /// <exception cref="KeyNotFoundException">If the given key is not in the attachments</exception>
      /// <exception cref="ArgumentNullException">If the given key is null</exception>
      object this[string key] { get; set; }

      /// <summary>
      /// Gets the user attached value that is associated with the given key, or null
      /// if no data is mapped to the key.
      /// </summary>
      /// <param name="key">The key to search for in the attachments</param>
      /// <param name="value">The value that result if the key exists</param>
      /// <returns>True if the value was in the attachments and retrieved</returns>
      bool TryGet(in string key, out object value);

      /// <summary>
      /// Gets the user attached value that is associated with the given key, or null
      /// if no data is mapped to the key.
      /// </summary>
      /// <typeparam name="T">The type to attempt to convert the attachment to</typeparam>
      /// <param name="key">The key to search for in the attachments</param>
      /// <param name="defaultValue">The default to return if the key is not present</param>
      /// <returns>A user set attachment for this given key or return the default</returns>
      /// <exception cref="InvalidCastException">If the attachment cannot be converted</exception>
      T Get<T>(in string key, in T defaultValue);

      /// <summary>
      /// Maps a given object to the given key in this Attachments instance.
      /// </summary>
      /// <param name="key">The key used to add or replace a value</param>
      /// <param name="value">The value to store with the given key</param>
      /// <returns>This Attachments instance</returns>
      IAttachments Set(in string key, in object value);

      /// <summary>
      /// Returns if the given key has a value mapped to it in this Attachments instance.
      /// </summary>
      /// <param name="key">The key to search in the attachments</param>
      /// <returns>true if the key is present or false if not.</returns>
      bool Contains(in string key);

      /// <summary>
      /// Removes all attachments from this instance.
      /// </summary>
      /// <returns>This Attachments instance</returns>
      IAttachments Clear();

   }
}