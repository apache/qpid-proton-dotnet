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
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine
{
   [TestFixture]
   public class OutgoingAMQPEnvelopeTest
   {
      [Test]
      public void TestCreateFromDefaultNoPool()
      {
         OutgoingAmqpEnvelope envelope =
            AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool().Take(new Open(), 0, null);

         Assert.Throws<ArgumentException>(() => envelope.HandlePayloadToLarge());

         envelope.PayloadToLargeHandler = null;

         Assert.Throws<ArgumentException>(() => envelope.HandlePayloadToLarge());

         bool toLargeHandlerCalled = false;
         envelope.PayloadToLargeHandler = (perf) => toLargeHandlerCalled = true;

         Assert.DoesNotThrow(() => envelope.HandlePayloadToLarge());
         Assert.IsTrue(toLargeHandlerCalled);

         envelope.HandleOutgoingFrameWriteComplete();
         envelope.Release();

         Assert.IsNotNull(envelope.ToString());
      }

      [Test]
      public void TestInvokeHandlerOnPerformative()
      {
         OutgoingAmqpEnvelope envelope =
            AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool().Take(new Open(), 0, null);

         TestPerformativeHandler handler = new TestPerformativeHandler();

         envelope.Invoke(handler, envelope);

         Assert.IsTrue(handler.openHandlerCalled);
      }

      private class TestPerformativeHandler : IPerformativeHandler<OutgoingAmqpEnvelope>
      {
         public bool openHandlerCalled = false;

         public void HandleOpen(Open open, IProtonBuffer payload, ushort channel, OutgoingAmqpEnvelope context)
         {
            this.openHandlerCalled = true;
         }
      }
   }
}