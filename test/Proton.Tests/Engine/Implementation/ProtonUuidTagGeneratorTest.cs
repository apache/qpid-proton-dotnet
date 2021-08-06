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
using Apache.Qpid.Proton.Types;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture]
   public class ProtonUuidTagGeneratorTest
   {
      [Test]
      public void TestCreateTagGenerator()
      {
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Uuid.NewTagGenerator();
         Assert.IsTrue(generator is ProtonUuidTagGenerator);
         Assert.IsNotNull(generator.ToString());
      }

      [Test]
      public void TestCreateTag()
      {
         ProtonUuidTagGenerator generator = new ProtonUuidTagGenerator();
         Assert.IsNotNull(generator.NextTag());
         IDeliveryTag next = generator.NextTag();
         next.Release();
         Assert.AreNotSame(next, generator.NextTag());
      }

      [Test]
      public void TestCopyTag()
      {
         ProtonUuidTagGenerator generator = new ProtonUuidTagGenerator();
         IDeliveryTag next = generator.NextTag();
         IDeliveryTag copy = (IDeliveryTag)next.Clone();

         Assert.AreNotSame(next, copy);
         Assert.AreEqual(next, copy);
      }

      [Test]
      public void TestTagCreatedHasExpectedUnderlying()
      {
         ProtonUuidTagGenerator generator = new ProtonUuidTagGenerator();

         IDeliveryTag tag = generator.NextTag();

         Assert.AreEqual(16, tag.Length);

         byte[] tagBuffer = new byte[16];
         tag.TagBuffer.CopyInto(0, tagBuffer, 0, 16);

         Guid uuid = new Guid(tagBuffer);

         Assert.IsNotNull(uuid);
         Assert.AreEqual(tag.GetHashCode(), uuid.GetHashCode());
         Assert.AreEqual(uuid.ToString(), tag.ToString());
      }

      [Test]
      public void TestCreateMatchingUUIDFromWrittenBuffer()
      {
         ProtonUuidTagGenerator generator = new ProtonUuidTagGenerator();
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         IDeliveryTag tag = generator.NextTag();

         tag.WriteTo(buffer);

         Assert.AreEqual(16, buffer.ReadableBytes);

         byte[] tagBuffer = new byte[16];
         tag.TagBuffer.CopyInto(0, tagBuffer, 0, 16);

         Guid uuid = new Guid(tagBuffer);

         Assert.IsNotNull(uuid);
         Assert.AreEqual(tag.GetHashCode(), uuid.GetHashCode());
         Assert.AreEqual(uuid.ToString(), tag.ToString());
      }

      [Test]
      public void TestTagEquals()
      {
         ProtonUuidTagGenerator generator = new ProtonUuidTagGenerator();

         IDeliveryTag tag1 = generator.NextTag();
         IDeliveryTag tag2 = generator.NextTag();
         IDeliveryTag tag3 = generator.NextTag();

         Assert.AreEqual(tag1, tag1);
         Assert.AreNotEqual(tag1, tag2);
         Assert.AreNotEqual(tag2, tag3);
         Assert.AreNotEqual(tag1, tag3);

         Assert.AreNotEqual(null, tag1);
         Assert.AreNotEqual(tag1, null);
         Assert.AreNotEqual("something", tag1);
         Assert.AreNotEqual(tag2, "something");
      }

      [Test]
      public void TestCreateTagsAreNotEqual()
      {
         ProtonUuidTagGenerator generator = new ProtonUuidTagGenerator();

         IDeliveryTag tag1 = generator.NextTag();
         IDeliveryTag tag2 = generator.NextTag();

         Assert.AreNotSame(tag1, tag2);
         Assert.AreNotEqual(tag1, tag2);
      }
   }
}