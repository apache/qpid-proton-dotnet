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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Client.Utilities
{
   public sealed class ReconnectLocationPool
   {
      private readonly LinkedList<ReconnectLocation> entries = new LinkedList<ReconnectLocation>();

      /// <summary>
      /// Create a default empty reconnect location pool
      /// </summary>
      public ReconnectLocationPool()
      {
      }

      /// <summary>
      /// Create a new reconnection location pool with the provided initial entries.
      /// </summary>
      /// <param name="locations">The initial collection of entries to add to the pool</param>
      public ReconnectLocationPool(ICollection<ReconnectLocation> locations) : this()
      {
         if (locations != null)
         {
            foreach (ReconnectLocation item in locations)
            {
               entries.AddLast(item);
            }
         }
      }

      /// <summary>
      /// Gets the current number of entries that are stored in the pool.
      /// </summary>
      public int Count
      {
         get
         {
            lock (entries)
            {
               return entries.Count;
            }
         }
      }

      /// <summary>
      /// Checks if the reconnection location pool is empty or not
      /// </summary>
      public bool IsEmpty
      {
         get
         {
            lock (entries)
            {
               return entries.Count == 0;
            }
         }
      }

      /// <summary>
      /// Returns the next reconnection location from the pool and moves its place in
      /// the pool to the end of the list meaning it will eventually be returned again
      /// unless removed from the pool.
      /// </summary>
      public ReconnectLocation? Next
      {
         get
         {
            lock (entries)
            {
               if (entries.Count > 0)
               {
                  ReconnectLocation entry = entries.First.Value;
                  entries.RemoveFirst();
                  entries.AddLast(entry);

                  return entry;
               }
            }

            return null;
         }
      }

      /// <summary>
      /// Shuffle the elements in the pool producing a new randmoized sequence of
      /// reconnection locations based on the original set.
      /// </summary>
      public void Shuffle()
      {
         lock (entries)
         {
            // TODO
         }
      }

      /// <summary>
      /// Adds the given location to this pool if it is not already contained within.
      /// </summary>
      /// <param name="location">The new location to be added to the pool</param>
      /// <returns>This reconnect locations pool</returns>
      public ReconnectLocationPool Add(ReconnectLocation location)
      {
         lock (entries)
         {
            if (!entries.Contains(location))
            {
               entries.AddLast(location);
            }
         }

         return this;
      }

      /// <summary>
      /// Adds the given location to this pool at the front if it is not already contained within.
      /// </summary>
      /// <param name="location">The new location to be added to the pool</param>
      /// <returns>This reconnect locations pool</returns>
      public ReconnectLocationPool AddFirst(ReconnectLocation location)
      {
         lock (entries)
         {
            if (!entries.Contains(location))
            {
               entries.AddFirst(location);
            }
         }

         return this;
      }

      /// <summary>
      /// Removes the given location from the pool if present.  If a value was removed
      /// this method return true otherwise it returns false.
      /// </summary>
      /// <param name="location">The location that should be removed from the pool</param>
      /// <returns>true if a value was removed from the pool, and false otherwise</returns>
      public bool Remove(ReconnectLocation location)
      {
         lock (entries)
         {
            return entries.Remove(location);
         }
      }

      /// <summary>
      /// Removes all current entries from this pool leaving it in an empty state.
      /// </summary>
      /// <param name="location">The location to remove</param>
      /// <returns>This reconnection location pool instance</returns>
      public ReconnectLocationPool RemoveAll(ReconnectLocation location)
      {
         lock (entries)
         {
            entries.Clear();
         }

         return this;
      }

      /// <summary>
      /// Returns a list that contains a copy of each element contained in this
      /// reconnect location pool which can be empty if the pool is empty.
      /// </summary>
      /// <returns></returns>
      public IList<ReconnectLocation> ToList()
      {
         IList<ReconnectLocation> copy = new List<ReconnectLocation>();

         lock (entries)
         {
            if (!IsEmpty)
            {
               foreach (ReconnectLocation location in entries)
               {
                  copy.Add(location);
               }
            }
         }

         return copy;
      }

      public override string ToString()
      {
         lock (entries)
         {
            return "ReconnectLocationPool { " + entries + " }";
         }
      }

      public static void ShuffleMe(IList<ReconnectLocation> list)
      {
         Random random = new Random();
         int n = list.Count;

         for (int i = list.Count - 1; i > 1; i--)
         {
            int rnd = random.Next(i + 1);

            ReconnectLocation value = list[rnd];
            list[rnd] = list[i];
            list[i] = value;
         }
      }
   }
}