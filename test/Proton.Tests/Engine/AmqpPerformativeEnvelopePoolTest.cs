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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine
{
   [TestFixture]
   public class AmqpPerformativeEnvelopePoolTest
   {
      [Test]
      public void TestCreateIncomingFramePool()
      {
         AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>.IncomingEnvelopePool();

         Assert.IsNotNull(pool);
         Assert.AreEqual(AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>.DEFAULT_MAX_POOL_SIZE, pool.MaxPoolSize);

         IncomingAmqpEnvelope frame1 = pool.Take(new Transfer(), 0, null);
         Assert.IsNotNull(frame1);
         IncomingAmqpEnvelope frame2 = pool.Take(new Transfer(), 0, null);
         Assert.IsNotNull(frame2);

         Assert.AreNotSame(frame1, frame2);
      }

      [Test]
      public void TestCreateIncomingFramePoolWithConfiguredMaxSize()
      {
         AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>.IncomingEnvelopePool(32);

         Assert.AreEqual(32, pool.MaxPoolSize);

         IncomingAmqpEnvelope frame1 = pool.Take(new Transfer(), 0, null);

         frame1.Release();

         IncomingAmqpEnvelope frame2 = pool.Take(new Transfer(), 0, null);

         Assert.AreSame(frame1, frame2);
      }

      [Test]
      public void TestIncomingPoolRecyclesReleasedFrames()
      {
         AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>.IncomingEnvelopePool(32);
         IncomingAmqpEnvelope frame1 = pool.Take(new Transfer(), 0, null);

         frame1.Release();

         IncomingAmqpEnvelope frame2 = pool.Take(new Transfer(), 0, null);

         Assert.AreSame(frame1, frame2);
      }

      [Test]
      public void TestIncomingPoolClearsReleasedFramePayloads()
      {
         AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope>.IncomingEnvelopePool(32);
         IncomingAmqpEnvelope frame1 = pool.Take(new Transfer(), 2, ProtonByteBufferAllocator.Instance.Allocate());

         frame1.Release();

         Assert.IsNull(frame1.Body);
         Assert.IsNull(frame1.Payload);
         Assert.AreNotEqual(2, frame1.Channel);
      }

      [Test]
      public void TestCreateOutgoingFramePool()
      {
         AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool();

         Assert.IsNotNull(pool);
         Assert.AreEqual(AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.DEFAULT_MAX_POOL_SIZE, pool.MaxPoolSize);

         OutgoingAmqpEnvelope frame1 = pool.Take(new Transfer(), 0, null);
         Assert.IsNotNull(frame1);
         OutgoingAmqpEnvelope frame2 = pool.Take(new Transfer(), 0, null);
         Assert.IsNotNull(frame2);

         Assert.AreNotSame(frame1, frame2);
      }

      [Test]
      public void TestCreateOutgoingFramePoolWithConfiguredMaxSize()
      {
         AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool(32);

         Assert.AreEqual(32, pool.MaxPoolSize);

         OutgoingAmqpEnvelope frame1 = pool.Take(new Transfer(), 0, null);

         frame1.Release();

         OutgoingAmqpEnvelope frame2 = pool.Take(new Transfer(), 0, null);

         Assert.AreSame(frame1, frame2);
      }

      [Test]
      public void TestOutgoingPoolRecyclesReleasedFrames()
      {
         AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool(32);
         OutgoingAmqpEnvelope frame1 = pool.Take(new Transfer(), 0, null);

         frame1.Release();

         OutgoingAmqpEnvelope frame2 = pool.Take(new Transfer(), 0, null);

         Assert.AreSame(frame1, frame2);
      }

      [Test]
      public void TestOutgoingPoolClearsReleasedFramePayloads()
      {
         AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> pool =
            AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool(32);
         OutgoingAmqpEnvelope frame1 = pool.Take(new Transfer(), 2, ProtonByteBufferAllocator.Instance.Allocate());

         frame1.Release();

         Assert.IsNull(frame1.Body);
         Assert.IsNull(frame1.Payload);
         Assert.AreNotEqual(2, frame1.Channel);
      }
   }
}