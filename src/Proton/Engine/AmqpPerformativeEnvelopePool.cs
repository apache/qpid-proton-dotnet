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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine
{
   public sealed class AmqpPerformativeEnvelopePool<T> where T : PerformativeEnvelope<IPerformative>
   {
      /// <summary>
      /// The default maximum pool size to use if not otherwise configured.
      /// </summary>
      public static readonly int DEFAULT_MAX_POOL_SIZE = 10;

      private int maxPoolSize = DEFAULT_MAX_POOL_SIZE;

      private readonly RingQueue<T> pool;
      private readonly Func<AmqpPerformativeEnvelopePool<T>, T> envelopeBuilder;

      /// <summary>
      /// Create a new envelope pool using the default pool size.
      /// </summary>
      /// <param name="envelopeBuilder">A function that can build the envelopes</param>
      public AmqpPerformativeEnvelopePool(Func<AmqpPerformativeEnvelopePool<T>, T> envelopeBuilder) :
         this(envelopeBuilder, AmqpPerformativeEnvelopePool<T>.DEFAULT_MAX_POOL_SIZE)
      {
      }

      /// <summary>
      /// Create a new envelope pool using the given pool size.
      /// </summary>
      /// <param name="envelopeBuilder">A function that can build the envelopes</param>
      /// <param name="maxPoolSize">The maximum number of entries in the pool</param>
      public AmqpPerformativeEnvelopePool(Func<AmqpPerformativeEnvelopePool<T>, T> envelopeBuilder, int maxPoolSize)
      {
         this.pool = new RingQueue<T>(MaxPoolSize);
         this.maxPoolSize = maxPoolSize;
         this.envelopeBuilder = envelopeBuilder;
      }

      /// <summary>
      /// Returns the configured maximum pool size which indicates how many elements can be
      /// pooled before new non-pooled values are created when a request arrives but all the
      /// current elements are on loan.
      /// </summary>
      int MaxPoolSize => maxPoolSize;

      /// <summary>
      /// Request to borrow a pooled object or create a new non-pooled instance if no pooled
      /// values are available.
      /// </summary>
      /// <param name="body"></param>
      /// <param name="channel"></param>
      /// <param name="payload"></param>
      /// <returns></returns>
      public T Take(IPerformative body, ushort channel, IProtonBuffer payload)
      {
         return (T)pool.Poll(SupplyPooledResource).Initialize(body, channel, payload);
      }

      internal void Release(T pooledEnvelope)
      {
         pool.Offer(pooledEnvelope);
      }

      private T SupplyPooledResource()
      {
         return envelopeBuilder.Invoke(this);
      }

      public static AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> IncomingEnvelopePool(int maxPoolSize)
      {
         return new AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>((pool) => new IncomingAmqpEnvelope(pool), maxPoolSize);
      }

      public static AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> IncomingEnvelopePool()
      {
         return new AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>((pool) => new IncomingAmqpEnvelope(pool));
      }

      public static AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> OutgoingEnvelopePool(int maxPoolSize)
      {
         return new AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>((pool) => new OutgoingAmqpEnvelope(pool), maxPoolSize);
      }

      public static AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> OutgoingEnvelopePool()
      {
         return new AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>((pool) => new OutgoingAmqpEnvelope(pool));
      }
   }
}