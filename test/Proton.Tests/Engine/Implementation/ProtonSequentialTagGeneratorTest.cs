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
using Apache.Qpid.Proton.Types;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture]
   public class ProtonSequentialTagGeneratorTest
   {
      [Test]
      public void TestCreateTagGenerator()
      {
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Sequential.NewTagGenerator();
         Assert.IsTrue(generator is ProtonSequentialTagGenerator);
         Assert.IsNotNull(generator.ToString());
      }

      [Test]
      public void TestCreateTag()
      {
         ProtonSequentialTagGenerator generator = new ProtonSequentialTagGenerator();

         Assert.IsNotNull(generator.NextTag());
      }

      [Test]
      public void TestCopyTag()
      {
         ProtonSequentialTagGenerator generator = new ProtonSequentialTagGenerator();
         IDeliveryTag next = generator.NextTag();
         IDeliveryTag copy = (IDeliveryTag)next.Clone();

         Assert.AreNotSame(next, copy);
         Assert.AreEqual(next, copy);
      }

      [Test]
      public void TestTagEquals()
      {
         ProtonSequentialTagGenerator generator = new ProtonSequentialTagGenerator();

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
      public void TestCreateTagsThatAreEqual()
      {
         ProtonSequentialTagGenerator generator = new ProtonSequentialTagGenerator();

         generator.NextTagId = 42;
         IDeliveryTag tag1 = generator.NextTag();

         generator.NextTagId = 42;
         IDeliveryTag tag2 = generator.NextTag();

         Assert.AreNotSame(tag1, tag2);
         Assert.AreEqual(tag1, tag2);

         Assert.AreEqual(tag1.GetHashCode(), tag2.GetHashCode());
      }

      [Test]
      public void TestCreateTagsThatWrapAroundLimit()
      {
         ProtonSequentialTagGenerator generator = new ProtonSequentialTagGenerator();

         // Test that on wrap the tags start beyond the cached values.
         generator.NextTagId = 0xFFFFFFFFFFFFFFFFul;

         IDeliveryTag maxUnsignedLong = generator.NextTag();
         IDeliveryTag NextTagAfterWrap = generator.NextTag();

         Assert.AreEqual(sizeof(ulong), maxUnsignedLong.TagBytes.Length);
         Assert.AreEqual(sizeof(byte), NextTagAfterWrap.TagBytes.Length);
      }

      [Test]
      public void TestCreateMatchingValuesFromWrittenBuffer()
      {
         ProtonSequentialTagGenerator generator = new ProtonSequentialTagGenerator();
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(64);

         generator.NextTagId = ulong.MaxValue;       // Long
         generator.NextTag().WriteTo(buffer);
         generator.NextTagId = 127;                  // Byte
         generator.NextTag().WriteTo(buffer);
         generator.NextTagId = 256;                  // Short
         generator.NextTag().WriteTo(buffer);
         generator.NextTagId = 65536;                // Int
         generator.NextTag().WriteTo(buffer);
         generator.NextTagId = 0x00000001FFFFFFFFul; // Long
         generator.NextTag().WriteTo(buffer);

         Assert.AreEqual(23, buffer.ReadableBytes);

         Assert.AreEqual(ulong.MaxValue, buffer.ReadUnsignedLong());
         Assert.AreEqual(127, buffer.ReadUnsignedByte());
         Assert.AreEqual(256, buffer.ReadUnsignedShort());
         Assert.AreEqual(65536, buffer.ReadUnsignedInt());
         Assert.AreEqual(0x00000001FFFFFFFFul, buffer.ReadUnsignedLong());
      }

      [Test]
      public void TestTagSizeMatchesValueRange()
      {
         ProtonSequentialTagGenerator generator = new ProtonSequentialTagGenerator();

         generator.NextTagId = ulong.MaxValue - 10;
         Assert.AreEqual(sizeof(ulong), generator.NextTag().Length);
         Assert.AreEqual(sizeof(ulong), generator.NextTag().TagBytes.Length);
         Assert.AreEqual(sizeof(ulong), generator.NextTag().TagBuffer.ReadableBytes);

         generator.NextTagId = 127;
         Assert.AreEqual(sizeof(byte), generator.NextTag().Length);
         Assert.AreEqual(sizeof(byte), generator.NextTag().TagBytes.Length);
         Assert.AreEqual(sizeof(byte), generator.NextTag().TagBuffer.ReadableBytes);

         generator.NextTagId = 256;
         Assert.AreEqual(sizeof(ushort), generator.NextTag().Length);
         Assert.AreEqual(sizeof(ushort), generator.NextTag().TagBytes.Length);
         Assert.AreEqual(sizeof(ushort), generator.NextTag().TagBuffer.ReadableBytes);

         generator.NextTagId = 65536;
         Assert.AreEqual(sizeof(uint), generator.NextTag().Length);
         Assert.AreEqual(sizeof(uint), generator.NextTag().TagBytes.Length);
         Assert.AreEqual(sizeof(uint), generator.NextTag().TagBuffer.ReadableBytes);

         generator.NextTagId = 0x00000001FFFFFFFFul;
         Assert.AreEqual(sizeof(ulong), generator.NextTag().Length);
         Assert.AreEqual(sizeof(ulong), generator.NextTag().TagBytes.Length);
         Assert.AreEqual(sizeof(ulong), generator.NextTag().TagBuffer.ReadableBytes);
      }
   }
}