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
using System.Linq;

namespace Apache.Qpid.Proton.Utilities
{
   /// <summary>
   /// A custom dictionary type whose sole purpose is to track unsettled deliveries
   /// and provide resources for processing those deliveries for Dispositions.
   /// </summary>
   public class UnsettledDictionary<Delivery> : IDictionary, ICollection<KeyValuePair<uint, Delivery>>, IDictionary<uint, Delivery>, IEnumerable, IEnumerable<KeyValuePair<uint, Delivery>> where Delivery : class
   {
      private readonly IComparer<uint> keyComparer = Comparer<uint>.Default;

      private static readonly double BucketLoadFactorMultiplier = 0.30;

      private static readonly int UnsettledInitialBuckets = 2;
      private static readonly int UnsettledBucketSize = 256;

      // Allow the free list to grow as needed but establish a maximum size before
      // recycled buckets are allowed to be garbage collected once no longer used.
      private static readonly int FreeListGrowthAmount = 2;
      private static readonly int FreeListSizeLimit = 8;

      // Always full buckets used in operations that needs unwritable bounds
      private readonly UnsettledBucket AlwaysFullBucket = new UnsettledBucket();

      private readonly Func<Delivery, uint> deliveryIdSupplier;
      private readonly int bucketCapacity;
      private readonly int bucketLowWaterMark;

      private uint size;
      private uint modCount;
      private int generations;
      private int freeListSize;

      private UnsettledBucket head; // Where new puts begin from
      private UnsettledBucket tail; // Where all gets start searching from
      private UnsettledBucket free; // Stack of free buckets

      /// <summary>
      /// Cached collection object that provides access to the dictionary keys
      /// </summary>
      protected ICollection<uint> keySet;

      /// <summary>
      /// Cached collection object that provides access to the dictionary values
      /// </summary>
      protected ICollection<Delivery> values;

      public UnsettledDictionary(Func<Delivery, uint> idSupplier) : this(idSupplier, UnsettledInitialBuckets, UnsettledBucketSize)
      {
      }

      public UnsettledDictionary(Func<Delivery, uint> idSupplier, int initialBuckets) : this(idSupplier, initialBuckets, UnsettledBucketSize)
      {
      }

      public UnsettledDictionary(Func<Delivery, uint> idSupplier, int initialBuckets, int bucketSize)
      {
         this.deliveryIdSupplier = idSupplier;
         this.bucketCapacity = bucketSize;
         this.bucketLowWaterMark = (int)(bucketSize * BucketLoadFactorMultiplier);

         if (bucketSize < 1)
         {
            throw new ArgumentException("The bucket size must be greater than zero");
         }

         if (initialBuckets < 1)
         {
            throw new ArgumentException("The initial number of buckets must be at least 1");
         }

         // All initial buckets go onto the free list
         free = new UnsettledBucket(bucketCapacity);
         for (int i = 1; i < initialBuckets; ++i)
         {
            UnsettledBucket newFree = new UnsettledBucket(bucketCapacity);
            newFree.prev = free;
            free = newFree;
         }
      }

      /// <summary>
      /// Create a new Unsettled Dictionary instance that uses the give default comparer
      /// instance to provide relative ordering of keys within the mapping adding all values
      /// into this dictionary from the provided one..
      /// </summary>
      /// <param name="source">The IDictionary whose elements should be added to this one</param>
      public UnsettledDictionary(Func<Delivery, uint> idSupplier, IDictionary<uint, Delivery> source) : this(idSupplier)
      {
         if (source == null)
         {
            throw new ArgumentNullException(nameof(source), "Provided source dictionary cannot be null");
         }

         foreach (KeyValuePair<uint, Delivery> entry in source)
         {
            Add(entry);
         }
      }

      #region Splayed Dictionary API implementations

      public int Count => (int)size;

      public bool IsEmpty => size == 0;

      public bool IsReadOnly => false;

      public IComparer<uint> Comparer => keyComparer;

      public bool IsFixedSize => false;

      public bool IsSynchronized => false;

      public object SyncRoot => AlwaysFullBucket;

      public void Add(uint key, Delivery value)
      {
         TryAdd(key, value, false);
      }

      public void Add(KeyValuePair<uint, Delivery> item)
      {
         TryAdd(item.Key, item.Value, false);
      }

      public void Add(object key, object value)
      {
         TryAdd((uint)key, (Delivery)value, false);
      }

      private void TryAdd(uint deliveryId, Delivery delivery, bool allowUpdates)
      {
         if (deliveryIdSupplier.Invoke(delivery) != deliveryId)
         {
            throw new ArgumentException("The delivery to add must have the same delivery Id as its key");
         }

         UnsettledBucket bucket = head;

         if (bucket != null)
         {
            if (deliveryId <= bucket.highestDeliveryId)
            {
               generations = Math.Max(0, generations + 1);
               bucket = AdvanceHead();
            }
            else if (bucket.FreeSpace == 0)
            {
               bucket = AdvanceHead();
            }
         }
         else
         {
            bucket = AdvanceHead();
         }

         bucket.generation = generations;
         bucket.Put(deliveryId, delivery);
         size++;
         modCount++;
      }

      public virtual void Clear()
      {
         for (UnsettledBucket bucket = tail; bucket != null; bucket = bucket.next)
         {
            bucket.Clear();
            if (freeListSize < FreeListSizeLimit)
            {
               bucket.prev = free;
               free = bucket;
               freeListSize++;
            }
         }

         head = tail = null;
         size = 0;
         modCount++;
      }

      public bool ContainsValue(Delivery value)
      {
         if (value != null && size > 0)
         {
            for (UnsettledBucket bucket = tail; bucket != null; bucket = bucket.next)
            {
               int writeOffset = bucket.writeOffset;
               Delivery[] deliveries = bucket.deliveries;

               for (int j = bucket.readOffset; j < writeOffset; ++j)
               {
                  if (value.Equals(deliveries[j]))
                  {
                     return true;
                  }
               }
            }
         }

         return false;
      }

      public bool Contains(object key)
      {
         return ContainsKey((uint)key);
      }

      public bool Contains(KeyValuePair<uint, Delivery> item)
      {
         if (size <= 0 && deliveryIdSupplier.Invoke(item.Value) != item.Key)
         {
            return false;
         }

         return Contains(item.Key);
      }

      public bool ContainsKey(uint deliveryId)
      {
         return TryFindDelivery(deliveryId, false, out _);
      }

      public virtual void CopyTo(KeyValuePair<uint, Delivery>[] array, int arrayIndex)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
         }

         if (arrayIndex < 0)
         {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
         }

         if (array.Length - arrayIndex > size)
         {
            throw new ArgumentException("Not enough space in array to store the dictionary entries");
         }

         if (size > 0)
         {
            for (UnsettledBucket bucket = tail; bucket != null; bucket = bucket.next)
            {
               int writeOffset = bucket.writeOffset;

               for (int j = bucket.readOffset; j < writeOffset; ++j)
               {
                  array[arrayIndex++] = bucket.KeyValuePairAt(j);
               }
            }
         }
      }

      public virtual void CopyTo(Array array, int arrayIndex)
      {
         if (array == null)
         {
            throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
         }

         if (arrayIndex < 0)
         {
            throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
         }

         if (array.Length - arrayIndex > size)
         {
            throw new ArgumentException("Not enough space in array to store the dictionary entries");
         }

         if (size > 0)
         {
            for (UnsettledBucket bucket = tail; bucket != null; bucket = bucket.next)
            {
               int writeOffset = bucket.writeOffset;

               for (int j = bucket.readOffset; j < writeOffset; ++j)
               {
                  array.SetValue(bucket.KeyValuePairAt(j), arrayIndex++);
               }
            }
         }
      }

      public void Remove(object key)
      {
         Remove((uint)key);
      }

      public bool Remove(uint key)
      {
         return TryFindDelivery(key, true, out _);
      }

      public bool Remove(KeyValuePair<uint, Delivery> item)
      {
         if (size <= 0 || deliveryIdSupplier.Invoke(item.Value) != item.Key)
         {
            return false;
         }

         return TryFindDelivery(item.Key, true, out _);
      }

      public bool TryGetValue(uint deliveryId, [MaybeNullWhen(false)] out Delivery value)
      {
         return TryFindDelivery(deliveryId, false, out value);
      }

      public Delivery this[uint key]
      {
         get
         {
            if (TryGetValue(key, out Delivery result))
            {
               return result;
            }

            throw new KeyNotFoundException("No entry exists for given key: " + key);
         }

         set => TryAdd(key, value, true);
      }

      public object this[object key]
      {
         get => this[(uint)key];
         set => this[(uint)key] = (Delivery)value;
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return this.GetEnumerator();
      }

      IDictionaryEnumerator IDictionary.GetEnumerator()
      {
         return (IDictionaryEnumerator)this.GetEnumerator();
      }

      public virtual IEnumerator<KeyValuePair<uint, Delivery>> GetEnumerator()
      {
         return new UnsettledDictionaryEntryEnumerator(this, tail);
      }

      public virtual ICollection<uint> Keys
      {
         get
         {
            if (keySet == null)
            {
               keySet = new UnsettledDictionaryKeys(this);
            }

            return keySet;
         }
      }

      public virtual ICollection<Delivery> Values
      {
         get
         {
            if (values == null)
            {
               values = new UnsettledDictionaryValues(this);
            }

            return values;
         }
      }

      ICollection IDictionary.Keys => (ICollection)this.Keys;

      ICollection IDictionary.Values => (ICollection)this.Values;

      #endregion

      #region UnsettledDictionary Specific APIs

      /// <summary>
      /// Removes the first entry from the dictionary that contains the specified value
      /// and returns true, if there is no entry with the given value this method returns
      /// false.
      /// </summary>
      /// <param name="target">The value to search for in the entries</param>
      /// <returns>true if an entry was removed or false if no match found</returns>
      public bool Remove(Delivery target)
      {
         if (target != null)
         {
            return TryFindDelivery(deliveryIdSupplier.Invoke(target), true, out _);
         }

         return false;
      }

      /// <summary>
      /// Visits each entry in the UnsettledDictionary and invokes the given action
      /// with each entry.
      /// </summary>
      /// <param name="action">The action to run on each element in the dictionary</param>
      /// <exception cref="ArgumentNullException"></exception>
      public void ForEach(Action<Delivery> action)
      {
         if (action == null)
         {
            throw new ArgumentNullException("The for each action cannot be null");
         }

         if (size > 0)
         {
            for (UnsettledBucket bucket = tail; bucket != null; bucket = bucket.next)
            {
               int writeOffset = bucket.writeOffset;

               for (int j = bucket.readOffset; j < writeOffset; ++j)
               {
                  action.Invoke(bucket.EntryAt(j));
               }
            }
         }
      }

      /// <summary>
      /// Visits each entry in the UnsettledDictionary and invokes the given action
      /// with each entry.
      /// </summary>
      /// <param name="action">The action to run on each element in the dictionary</param>
      /// <exception cref="ArgumentNullException"></exception>
      public void ForEach(Action<uint, Delivery> action)
      {
         if (action == null)
         {
            throw new ArgumentNullException("The for each action cannot be null");
         }

         if (size > 0)
         {
            for (UnsettledBucket bucket = tail; bucket != null; bucket = bucket.next)
            {
               int writeOffset = bucket.writeOffset;

               for (int j = bucket.readOffset; j < writeOffset; ++j)
               {
                  action.Invoke(bucket.EntryIdAt(j), bucket.EntryAt(j));
               }
            }
         }
      }

      /// <summary>
      /// Visits each entry within the given range and invokes the provided action on
      /// each delivery in the tracker. The lower and upper bounds are limits which are
      /// not exclusive meaning entries within this range are visited regards of there
      /// being an exact match on the lower and upper boundaries.
      /// </summary>
      /// <param name="first"></param>
      /// <param name="last"></param>
      /// <param name="action"></param>
      /// <exception cref="ArgumentNullException">If the provided action is null</exception>
      public void ForEach(uint first, uint last, Action<Delivery> action)
      {
         if (action == null)
         {
            throw new ArgumentNullException("The for each action cannot be null");
         }

         if (size == 0)
         {
            return;
         }

         int readStart = -1;
         bool foundFirst = false;
         bool foundLast = false;

         for (UnsettledBucket bucket = tail; bucket != null && !foundLast; bucket = bucket.next)
         {
            int writeOffset = bucket.writeOffset;

            readStart = bucket.readOffset;

            if (!foundFirst && bucket.IsCapturedByRange(first, last))
            {
               int result = bucket.Search(first);
               int ceiling = result >= 0 ? result : ~result;

               if (ceiling < writeOffset)
               {
                  foundFirst = true;
                  readStart = ceiling;
               }
            }

            if (foundFirst)
            {
               Delivery[] deliveries = bucket.deliveries;
               uint[] deliveryIds = bucket.deliveryIds;

               for (int j = readStart; j < writeOffset && !foundLast;)
               {
                  uint candidate = deliveryIds[j];

                  if (candidate <= last)
                  {
                     action.Invoke(bucket.EntryAt(j++));
                  }

                  foundLast = candidate >= last;
               }
            }
         }
      }

      /// <summary>
      /// Remove each entry within the given range of delivery IDs. For each entry
      /// removed the provided action is triggered allowing the caller to be notified
      /// of each removal.
      /// </summary>
      /// <param name="first"></param>
      /// <param name="last"></param>
      /// <param name="action"></param>
      /// <exception cref="ArgumentNullException">If the provided action is null</exception>
      public void RemoveEach(uint first, uint last, Action<Delivery> action)
      {
         if (action == null)
         {
            throw new ArgumentNullException("The for each action cannot be null");
         }

         if (size == 0)
         {
            return;
         }

         bool foundFirst = false;
         bool foundLast = false;
         int removeStart = 0;
         int removeEnd = 0;

         for (UnsettledBucket bucket = tail; bucket != null && !foundLast;)
         {
            int writeOffset = bucket.writeOffset;

            removeStart = bucket.readOffset;

            if (!foundFirst && bucket.IsCapturedByRange(first, last))
            {
               int result = bucket.Search(first);
               int ceiling = result >= 0 ? result : ~result;

               if (ceiling < writeOffset)
               {
                  foundFirst = true;
                  removeStart = ceiling;
               }
            }

            if (foundFirst)
            {
               Delivery[] deliveries = bucket.deliveries;
               uint[] deliveryIds = bucket.deliveryIds;

               for (removeEnd = removeStart; removeEnd < writeOffset && !foundLast;)
               {
                  uint candidate = deliveryIds[removeEnd];

                  if (candidate <= last)
                  {
                     action.Invoke(bucket.EntryAt(removeEnd++));
                  }

                  foundLast = candidate >= last;
               }

               bucket = RemoveRange(bucket, removeStart, removeEnd, foundLast);
            }
            else
            {
               bucket = bucket.next;
            }
         }
      }

      #endregion

      #region The private methods used to implement the dictionary

      private UnsettledBucket AdvanceHead()
      {
         if (free == null)
         {
            for (int i = 0; i < FreeListGrowthAmount; i++)
            {
               UnsettledBucket bucket = new UnsettledBucket(bucketCapacity);

               bucket.prev = free;
               free = bucket;
            }

            freeListSize = FreeListGrowthAmount;
         }

         // Pop a bucket off the free list
         UnsettledBucket popped = free;
         free = popped.prev;
         popped.prev = null;
         freeListSize--;

         if (head == null)
         {
            head = popped;
            tail = popped;
         }
         else if (head == tail)
         {
            head = popped;
            head.prev = tail;
            tail.next = head;
         }
         else
         {
            head.next = popped;
            popped.prev = head;
            head = popped;
         }

         return head;
      }

      private bool TryFindDelivery(uint deliveryId, bool remove, out Delivery delivery)
      {
         if (size == 0)
         {
            delivery = default(Delivery);
            return false;
         }

         bool hasNotOverflowed = generations == 0;
         uint globalLow = tail.lowestDeliveryId;
         uint globalHigh = head.highestDeliveryId;

         if (hasNotOverflowed)
         {
            // When there is no overflow in the map we can fast path check for the delivery
            // being outside the global range and exit early. All other cases of overflow
            // requires some caution and we search all the buckets for a match.
            if (deliveryId < globalLow || deliveryId > globalHigh)
            {
               delivery = default(Delivery);
               return false;
            }
         }

         if (tail.IsInRange(deliveryId))
         {
            int deliveryIndex = tail.Search(deliveryId);
            if (deliveryIndex >= 0)
            {
               if (remove)
               {
                  delivery = GetAndRemove(tail, deliveryIndex);
               }
               else
               {
                  delivery = tail.deliveries[deliveryIndex];
               }

               return true;
            }
         }

         if (head == tail)
         {
            delivery = default(Delivery);
            return false;
         }

         if (hasNotOverflowed)
         {
            uint distFromTail = deliveryId - globalLow;
            uint distToHead = globalHigh - deliveryId;

            // We can only search from head if there are no active overflows, which is indicated
            // by generations being greater than zero.
            if (distFromTail > distToHead)
            {
               return SearchBackwards(deliveryId, head, remove, out delivery);
            }
         }

         return SearchForwards(deliveryId, tail.next, remove, hasNotOverflowed, out delivery);
      }

      private bool SearchForwards(uint deliveryId, UnsettledBucket target, bool remove, bool canStopEarly, out Delivery delivery)
      {
         for (; target != null; target = target.next)
         {
            if (canStopEarly && target.lowestDeliveryId > deliveryId)
            {
               break;
            }

            if (deliveryId < target.lowestDeliveryId)
            {
               continue;
            }

            if (deliveryId > target.highestDeliveryId)
            {
               continue;
            }

            int index = target.Search(deliveryId);

            if (index >= 0)
            {
               if (remove)
               {
                  delivery = GetAndRemove(target, index);
               }
               else
               {
                  delivery = target.deliveries[index];
               }

               return true;
            }
         }

         delivery = default(Delivery);

         return false;
      }

      private bool SearchBackwards(uint deliveryId, UnsettledBucket target, bool remove, out Delivery delivery)
      {
         for (; target != null; target = target.prev)
         {
            if (target.highestDeliveryId < deliveryId)
            {
               break;
            }

            if (deliveryId < target.lowestDeliveryId)
            {
               continue;
            }

            if (deliveryId > target.highestDeliveryId)
            {
               continue;
            }

            int index = target.Search(deliveryId);

            if (index >= 0)
            {
               if (remove)
               {
                  delivery = GetAndRemove(target, index);
               }
               else
               {
                  delivery = target.deliveries[index];
               }

               return true;
            }
         }

         delivery = default(Delivery);

         return false;
      }

      private Delivery GetAndRemove(UnsettledBucket bucket, int index)
      {
         Delivery delivery = bucket.deliveries[index];

         bucket.RemoveIndex(index);

         size--;
         modCount++;

         if (bucket.entries <= bucketLowWaterMark)
         {
            TryCompact(bucket);
         }

         return delivery;
      }

      private UnsettledBucket RemoveRange(UnsettledBucket bucket, int start, int end, bool compact)
      {
         UnsettledBucket next = bucket.next;

         int removals = end - start;

         this.size -= (uint)removals;
         this.modCount++;

         if (removals == bucket.entries)
         {
            RecycleBucket(bucket);
         }
         else
         {
            Array.Copy(bucket.deliveries, end, bucket.deliveries, start, bucket.writeOffset - end);
            Array.Copy(bucket.deliveryIds, end, bucket.deliveryIds, start, bucket.writeOffset - end);
            Array.Fill(bucket.deliveries, null, bucket.writeOffset - removals, removals);

            bucket.writeOffset = bucket.writeOffset - removals;
            bucket.entries -= removals;
            bucket.highestDeliveryId = bucket.EntryIdAt(bucket.writeOffset - 1);
            bucket.lowestDeliveryId = bucket.EntryIdAt(bucket.readOffset);

            if (compact)
            {
               TryCompact(bucket);
            }
         }

         return next;
      }

      #endregion

      #region Bucket Type used to hold a chunk of the unsettled delivery data

      protected sealed class UnsettledBucket
      {
         public UnsettledBucket next;
         public UnsettledBucket prev;

         public int readOffset;
         public int writeOffset;
         public int entries;
         public uint lowestDeliveryId = 0;
         public uint highestDeliveryId = 0;
         public int generation;

         public readonly Delivery[] deliveries;
         public readonly uint[] deliveryIds;

         public UnsettledBucket()
         {
            this.deliveries = new Delivery[0];
            this.deliveryIds = new uint[0];
            this.highestDeliveryId = uint.MaxValue;
         }

         public UnsettledBucket(int bucketCapacity)
         {
            this.deliveries = new Delivery[bucketCapacity];
            this.deliveryIds = new uint[bucketCapacity];
         }

         public Delivery[] Deliveries => deliveries;

         public bool IsReadable => entries > 0;

         public int FreeSpace => deliveries.Length - entries;

         public bool IsFull => writeOffset == deliveries.Length;

         public bool IsInRange(uint deliveryId) => deliveryId >= lowestDeliveryId && deliveryId <= highestDeliveryId;

         public bool IsCapturedByRange(uint lowest, uint highest) => lowestDeliveryId <= highest && highestDeliveryId >= lowest;

         public void Put(uint deliveryId, Delivery delivery)
         {
            if (writeOffset == deliveryIds.Length)
            {
               Compact();
            }

            if (entries == 0)
            {
               lowestDeliveryId = deliveryId;
            }

            highestDeliveryId = deliveryId;
            deliveryIds[writeOffset] = deliveryId;
            deliveries[writeOffset++] = delivery;
            entries++;
         }

         public Delivery EntryAt(int index)
         {
            return (Delivery)deliveries[index];
         }

         public uint EntryIdAt(int index)
         {
            return deliveryIds[index];
         }

         public KeyValuePair<uint, Delivery> KeyValuePairAt(int index)
         {
            return new KeyValuePair<uint, Delivery>(deliveryIds[index], deliveries[index]);
         }

         public DictionaryEntry DictionaryEntryAt(int index)
         {
            return new DictionaryEntry(deliveryIds[index], deliveries[index]);
         }

         public int RemoveIndex(int deliveryIndex)
         {
            entries--;

            // If not the readOffset we compact the entries to avoid null gaps in the entries
            // which complicates searches and makes bulk assignments or copies impossible. If
            // at the read offset then we either advance the lowest Id seen or we've consumed
            // all the entries and we reset the value to ensure range checks fail
            if (deliveryIndex == readOffset)
            {
               deliveries[readOffset++] = null;
               deliveryIndex++;
               // We removed the first element meaning we now must increase the lowest entry to
               // avoid false positives when accessing randomly unless unordered since there
               // could be duplicates
               if (entries > 0)
               {
                  lowestDeliveryId = deliveryIds[readOffset];
               }
               else
               {
                  lowestDeliveryId = uint.MaxValue;
                  highestDeliveryId = 0;
                  readOffset = 0;
                  writeOffset = 0;
               }
            }
            else if (deliveryIndex == writeOffset - 1)
            {
               deliveries[--writeOffset] = null;
               // If we remove the last entry then we can reduce the highest delivery ID in this
               // bucket to avoid false positive matches when randomly accessing elements unless
               // unordered in which case there could be duplicate entries
               highestDeliveryId = deliveryIds[writeOffset - 1];
            }
            else
            {
               int prefixSize = deliveryIndex - readOffset;
               int suffixSize = (writeOffset - 1) - deliveryIndex;

               if (prefixSize <= suffixSize)
               {
                  Array.Copy(deliveries, readOffset, deliveries, readOffset + 1, prefixSize);
                  Array.Copy(deliveryIds, readOffset, deliveryIds, readOffset + 1, prefixSize);
                  deliveries[readOffset++] = null;
                  deliveryIndex++;
               }
               else
               {
                  Array.Copy(deliveries, deliveryIndex + 1, deliveries, deliveryIndex, suffixSize);
                  Array.Copy(deliveryIds, deliveryIndex + 1, deliveryIds, deliveryIndex, suffixSize);
                  deliveries[--writeOffset] = null;
               }
            }

            return deliveryIndex;
         }

         public Delivery RemoveAt(int bucketEntry)
         {
            Delivery delivery = (Delivery)deliveries[bucketEntry];

            entries--;

            // If not the readOffset we compact the entries to avoid null gaps in the entries
            // which complicates searches and makes bulk assignments or copies impossible. If
            // at the read offset then we either advance the lowest Id seen or we've consumed
            // all the entries and we reset the value to ensure range checks fail
            if (bucketEntry == readOffset)
            {
               deliveries[readOffset++] = null;
               // We removed the first element meaning we now must increase the lowest entry to
               // avoid false positives when accessing randomly unless unordered since there could
               // be duplicates
               if (entries > 0)
               {
                  lowestDeliveryId = deliveryIds[readOffset];
               }
               else
               {
                  lowestDeliveryId = uint.MaxValue;
                  highestDeliveryId = 0;
                  readOffset = 0;
                  writeOffset = 0;
               }
            }
            else if (bucketEntry == writeOffset - 1)
            {
               deliveries[--writeOffset] = null;
               // If we remove the last entry then we can reduce the highest delivery ID in this
               // bucket to avoid false positive matches when randomly accessing elements unless
               // unordered in which case there could be duplicate entries
               highestDeliveryId = deliveryIds[writeOffset - 1];
            }
            else
            {
               int prefixSize = bucketEntry - readOffset;
               int suffixSize = (writeOffset - 1) - bucketEntry;

               if (prefixSize <= suffixSize)
               {
                  Array.Copy(deliveries, readOffset, deliveries, readOffset + 1, prefixSize);
                  Array.Copy(deliveryIds, readOffset, deliveryIds, readOffset + 1, prefixSize);
                  deliveries[readOffset++] = null;
                  bucketEntry++;
               }
               else
               {
                  Array.Copy(deliveries, bucketEntry + 1, deliveries, bucketEntry, suffixSize);
                  Array.Copy(deliveryIds, bucketEntry + 1, deliveryIds, bucketEntry, suffixSize);
                  deliveries[--writeOffset] = null;
               }
            }

            return delivery;
         }

         public void Clear()
         {
            if (entries != 0)
            {
               Array.Fill(deliveries, default(Delivery));
            }

            // Ensures the first put always assigns this
            lowestDeliveryId = uint.MaxValue;
            highestDeliveryId = 0;
            readOffset = writeOffset = entries = 0;
            next = prev = null;
         }

         public override string ToString()
         {
            return "UnsettledBucket { size=" + entries +
                                    " roff=" + readOffset +
                                    " woff=" + writeOffset +
                                    " lowID=" + lowestDeliveryId +
                                    " highID=" + highestDeliveryId + " }";
         }

         private static readonly int BinarySearchThreshold = 64;

         public int Search(uint deliveryId)
         {
            if (deliveryIds[readOffset] == deliveryId)
            {
               return readOffset;
            }
            else if (entries < BinarySearchThreshold)
            {
               return LinearSearch(deliveryId, readOffset, writeOffset);
            }
            else
            {
               return BinarySearch(deliveryId, readOffset, writeOffset);
            }
         }

         private int BinarySearch(uint deliveryId, int fromIndex, int toIndex)
         {
            int low = fromIndex;
            int high = toIndex - 1;

            while (low <= high)
            {
               int mid = (low + high) >> 1;
               uint midDeliveryId = deliveryIds[mid];

               int cmp = midDeliveryId.CompareTo(deliveryId);

               if (cmp < 0)
               {
                  low = mid + 1;
               }
               else if (cmp > 0)
               {
                  high = mid - 1;
               }
               else
               {
                  return mid;  // first matching delivery
               }
            }

            return ~low; // signal that delivery ID is not in this bucket also gives insertion point
         }

         private int LinearSearch(uint deliveryId, int fromIndex, int toIndex)
         {
            for (int i = fromIndex; i < toIndex; ++i)
            {
               uint idAtIndex = deliveryIds[i];
               int comp = idAtIndex.CompareTo(deliveryId);

               if (comp == 0)
               {
                  return i;
               }
               else if (comp > 0)
               {
                  // Can't be in this bucket because we already found a larger value
                  return ~i;
               }
            }

            return ~toIndex;
         }

         private void Compact()
         {
            if (readOffset != 0)
            {
               Array.Copy(deliveries, readOffset, deliveries, 0, entries);
               Array.Copy(deliveryIds, readOffset, deliveryIds, 0, entries);
               Array.Fill(deliveries, null, entries, entries);

               writeOffset = entries;
               readOffset = 0;
            }
            else
            {
               throw new Exception("Put called when no space in the bucket for new entries");
            }
         }
      }

      #endregion

      #region Internal utility methods

      private void RecycleBucket(UnsettledBucket bucket)
      {
         UnsettledBucket bucketNext = bucket.next;
         UnsettledBucket bucketPrev = bucket.prev;

         int nextGeneration = bucketNext == null ? -1 : bucketNext.generation;
         int prevGeneration = bucketPrev == null ? -1 : bucketPrev.generation;

         // This is the last of its generation so we decrease the tracked generations which can
         // allow search operations to optimize if they know there isn't any overflow in the map
         // when they are running.
         if (prevGeneration != bucket.generation && nextGeneration != bucket.generation)
         {
            generations = Math.Max(0, generations - 1);
         }

         if (bucket == head)
         {
            head = bucketPrev;
         }

         if (bucket == tail)
         {
            tail = bucketNext;
         }

         if (bucketNext != null)
         {
            bucketNext.prev = bucketPrev;
         }

         if (bucketPrev != null)
         {
            bucketPrev.next = bucketNext;
         }

         bucket.Clear();  // Drop all content and reset as empty bucket

         if (freeListSize < FreeListSizeLimit)
         {
            bucket.prev = free;
            free = bucket;
            freeListSize++;
         }
      }

      // This variant is called from the Map API remove methods and doesn't need to track bucket
      // compaction results or next element locations which increases performance in this case.
      private void TryCompact(UnsettledBucket bucket)
      {
         UnsettledBucket next = bucket.next;
         UnsettledBucket prev = bucket.prev;

         if (bucket.IsReadable)
         {
            UnsettledBucket nextBucket =
               (next == null || bucket == head) ||
               bucket.highestDeliveryId > next.lowestDeliveryId ? AlwaysFullBucket : next;
            UnsettledBucket prevBucket =
               (prev == null || bucket == tail) ||
               bucket.lowestDeliveryId < prev.highestDeliveryId ? AlwaysFullBucket : prev;

            // As soon as compaction is possible move elements from this bucket into previous and next
            // which reduces search times as there are fewer buckets to traverse/
            if (nextBucket.FreeSpace + prevBucket.FreeSpace >= bucket.entries)
            {
               DoCompaction(bucket, prevBucket, nextBucket);
               RecycleBucket(bucket);
            }
         }
         else
         {
            RecycleBucket(bucket);
         }
      }

      private void DoCompaction(UnsettledBucket bucket, UnsettledBucket prev, UnsettledBucket next)
      {
         int srcEntries = bucket.entries;
         int srcReadOffset = bucket.readOffset;
         Object[] srcDeliveries = bucket.deliveries;
         uint[] srcDeliveryIds = bucket.deliveryIds;

         if (prev.FreeSpace > 0)
         {
            int toCopy = Math.Min(srcEntries, prev.FreeSpace);
            int prevTailSpace = prev.deliveries.Length - prev.writeOffset;

            if (prevTailSpace < toCopy && prev.readOffset != 0)
            {
               Array.Copy(prev.deliveries, prev.readOffset, prev.deliveries, 0, prev.entries);
               Array.Copy(prev.deliveryIds, prev.readOffset, prev.deliveryIds, 0, prev.entries);
               if (prev.writeOffset > prev.entries + toCopy)
               {
                  int startIndex = prev.entries + toCopy;
                  Array.Fill(prev.deliveries, null, prev.entries + toCopy, prev.writeOffset - startIndex);
               }

               prev.writeOffset -= prev.readOffset;
               prev.readOffset = 0;
            }

            Array.Copy(srcDeliveries, srcReadOffset, prev.deliveries, prev.writeOffset, toCopy);
            Array.Copy(srcDeliveryIds, srcReadOffset, prev.deliveryIds, prev.writeOffset, toCopy);

            prev.entries += toCopy;
            prev.writeOffset += toCopy;
            prev.highestDeliveryId = prev.EntryIdAt(prev.writeOffset - 1);

            srcEntries -= toCopy;
            srcReadOffset += toCopy;
         }

         // We didn't get them all into the previous bucket but we know that if we are
         // here then there must be space ahead to accept the rest as we already checked.
         if (srcEntries > 0)
         {
            if (next.entries != 0)
            {
               if (next.readOffset < srcEntries)
               {
                  Array.Copy(next.deliveries, next.readOffset, next.deliveries, srcEntries, next.entries);
                  Array.Copy(next.deliveryIds, next.readOffset, next.deliveryIds, srcEntries, next.entries);

                  next.readOffset = 0;
                  next.writeOffset = srcEntries + next.entries;
               }
               else
               {
                    next.readOffset -= srcEntries;
               }
            }
            else
            {
               next.writeOffset = srcEntries;
            }

            Array.Copy(srcDeliveries, srcReadOffset, next.deliveries, next.readOffset, srcEntries);
            Array.Copy(srcDeliveryIds, srcReadOffset, next.deliveryIds, next.readOffset, srcEntries);

            next.entries += srcEntries;
            next.lowestDeliveryId = next.EntryIdAt(next.readOffset);
            next.highestDeliveryId = next.EntryIdAt(next.writeOffset - 1);
         }
      }

      #endregion

      #region Collection Enumerators

      private abstract class UnsettledDictionaryEnumerator<TResult> : IEnumerator<TResult>, IEnumerator
      {
         protected readonly UnsettledDictionary<Delivery> parent;
         protected readonly UnsettledBucket initialBucket;
         protected readonly uint expectedModCount;

         protected UnsettledBucket currentBucket;
         protected int readOffset;

         public UnsettledDictionaryEnumerator(UnsettledDictionary<Delivery> parent, UnsettledBucket bucket)
         {
            this.parent = parent;
            this.initialBucket = bucket;
            this.expectedModCount = parent.modCount;
            this.currentBucket = bucket;
            this.readOffset = -1;
         }

         object IEnumerator.Current => this.Current;

         public abstract TResult Current { get; }

         public void Dispose()
         {
            readOffset = -1;
            currentBucket = null;
         }

         public bool MoveNext()
         {
            CheckNotModified();

            if (readOffset == -1)
            {
               FirstEntry();
            }
            else
            {
               Successor();
            }

            return readOffset != -1;
         }

         public void Reset()
         {
            currentBucket = initialBucket;
            readOffset = -1;
         }

         protected void CheckNotModified()
         {
            if (expectedModCount != parent.modCount)
            {
               throw new InvalidOperationException("Parent Dictionary was modified during enumeration.");
            }
         }

         private void FirstEntry()
         {
            this.readOffset = currentBucket != null ? currentBucket.readOffset : -1;
         }

         private void Successor()
         {
            if (++readOffset == currentBucket.writeOffset)
            {
               currentBucket = currentBucket.next;

               if (currentBucket != null && currentBucket.IsReadable)
               {
                  readOffset = currentBucket.readOffset;
               }
               else
               {
                  readOffset = -1;
               }
            }
         }
      }

      private sealed class UnsettledDictionaryKeyEnumerator : UnsettledDictionaryEnumerator<uint>
      {
         public UnsettledDictionaryKeyEnumerator(UnsettledDictionary<Delivery> parent, UnsettledBucket bucket) : base(parent, bucket)
         {
         }

         public override uint Current
         {
            get
            {
               if (readOffset != -1)
               {
                  return currentBucket.EntryIdAt(readOffset);
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      private sealed class UnsettledDictionaryValueEnumerator : UnsettledDictionaryEnumerator<Delivery>
      {
         public UnsettledDictionaryValueEnumerator(UnsettledDictionary<Delivery> parent, UnsettledBucket bucket) : base(parent, bucket)
         {
         }

         public override Delivery Current
         {
            get
            {
               if (readOffset != -1)
               {
                  return currentBucket.EntryAt(readOffset);
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      private sealed class UnsettledDictionaryEntryEnumerator : UnsettledDictionaryEnumerator<KeyValuePair<uint, Delivery>>, IDictionaryEnumerator
      {
         public UnsettledDictionaryEntryEnumerator(UnsettledDictionary<Delivery> parent, UnsettledBucket bucket) : base(parent, bucket)
         {
         }

         public override KeyValuePair<uint, Delivery> Current
         {
            get
            {
               if (readOffset != -1)
               {
                  return currentBucket.KeyValuePairAt(readOffset);
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }

         public DictionaryEntry Entry
         {
            get
            {
               if (readOffset != -1)
               {
                  return currentBucket.DictionaryEntryAt(readOffset);
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }

         public object Key
         {
            get
            {
               if (readOffset != -1)
               {
                  return currentBucket.EntryIdAt(readOffset);
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }

         public object Value
         {
            get
            {
               if (readOffset != -1)
               {
                  return currentBucket.EntryAt(readOffset);
               }

               throw new InvalidOperationException("Current position is undefined.");
            }
         }
      }

      #endregion

      #region Dictionary Collections for Keys and Values

      private sealed class UnsettledDictionaryValues : ICollection<Delivery>, ICollection
      {
         private readonly UnsettledDictionary<Delivery> parent;

         public UnsettledDictionaryValues(UnsettledDictionary<Delivery> parent) : base()
         {
            this.parent = parent;
         }

         public int Count => parent.Count;

         public bool IsReadOnly => parent.IsReadOnly;

         public bool IsSynchronized => false;

         public object SyncRoot => parent.SyncRoot;

         public void Add(Delivery item)
         {
            throw new NotSupportedException("Cannot add a value only entry to the parent Dictionary");
         }

         public void Clear()
         {
            parent.Clear();
         }

         public bool Contains(Delivery item)
         {
            return parent.ContainsValue(item);
         }

         public bool Remove(Delivery item)
         {
            return parent.Remove(item);
         }

         public void CopyTo(Delivery[] array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary values");
            }

            if (parent.size > 0)
            {
               for (UnsettledBucket bucket = parent.tail; bucket != null; bucket = bucket.next)
               {
                  int writeOffset = bucket.writeOffset;
                  Delivery[] deliveries = bucket.deliveries;

                  for (int j = bucket.readOffset; j < writeOffset; ++j)
                  {
                     array[arrayIndex++] = bucket.EntryAt(j);
                  }
               }
            }
         }

         public void CopyTo(Array array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary values");
            }

            if (parent.size > 0)
            {
               for (UnsettledBucket bucket = parent.tail; bucket != null; bucket = bucket.next)
               {
                  int writeOffset = bucket.writeOffset;
                  Delivery[] deliveries = bucket.deliveries;

                  for (int j = bucket.readOffset; j < writeOffset; ++j)
                  {
                     array.SetValue(bucket.EntryAt(j), arrayIndex++);
                  }
               }
            }
         }

         public IEnumerator<Delivery> GetEnumerator()
         {
            return new UnsettledDictionaryValueEnumerator(parent, parent.tail);
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            return new UnsettledDictionaryValueEnumerator(parent, parent.tail);
         }
      }

      private sealed class UnsettledDictionaryKeys : ICollection<uint>, ICollection
      {
         private readonly UnsettledDictionary<Delivery> parent;

         public UnsettledDictionaryKeys(UnsettledDictionary<Delivery> parent) : base()
         {
            this.parent = parent;
         }

         public int Count => parent.Count;

         public bool IsReadOnly => parent.IsReadOnly;

         public bool IsSynchronized => false;

         public object SyncRoot => parent.SyncRoot;

         public void Add(uint item)
         {
            throw new NotSupportedException("Cannot add a key only entry to the parent Dictionary");
         }

         public void Clear()
         {
            parent.Clear();
         }

         public bool Contains(uint item)
         {
            return parent.ContainsKey(item);
         }

         public void CopyTo(uint[] array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary keys");
            }

            if (parent.size > 0)
            {
               for (UnsettledBucket bucket = parent.tail; bucket != null; bucket = bucket.next)
               {
                  int writeOffset = bucket.writeOffset;
                  Delivery[] deliveries = bucket.deliveries;

                  for (int j = bucket.readOffset; j < writeOffset; ++j)
                  {
                     array[arrayIndex++] = bucket.EntryIdAt(j);
                  }
               }
            }
         }

         public void CopyTo(Array array, int arrayIndex)
         {
            if (array == null)
            {
               throw new ArgumentNullException(nameof(array), "The provided array cannot be null");
            }

            if (arrayIndex < 0)
            {
               throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be greater than zero");
            }

            if (array.Length - arrayIndex > parent.size)
            {
               throw new ArgumentException("Not enough space in array to store the dictionary keys");
            }

            if (parent.size > 0)
            {
               for (UnsettledBucket bucket = parent.tail; bucket != null; bucket = bucket.next)
               {
                  int writeOffset = bucket.writeOffset;
                  Delivery[] deliveries = bucket.deliveries;

                  for (int j = bucket.readOffset; j < writeOffset; ++j)
                  {
                     array.SetValue(bucket.EntryIdAt(j), arrayIndex++);
                  }
               }
            }
         }

         public bool Remove(uint item)
         {
            return parent.Remove(item);
         }

         public IEnumerator<uint> GetEnumerator()
         {
            return new UnsettledDictionaryKeyEnumerator(parent, parent.tail);
         }

         IEnumerator IEnumerable.GetEnumerator()
         {
            return new UnsettledDictionaryKeyEnumerator(parent, parent.tail);
         }
      }

      #endregion
   }
}