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
using Apache.Qpid.Proton.Types;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture]
   public class ProtonPooledTagGeneratorTest
   {
      [Test]
      public void TestCreateTagGenerator()
      {
         IDeliveryTagGenerator generator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();
         Assert.IsTrue(generator is ProtonPooledTagGenerator);
      }

      [Test]
      public void TestCreateTagGeneratorChecksPoolSze()
      {
         try
         {
            new ProtonPooledTagGenerator(0);
            Assert.Fail("Should not allow non-pooling pool");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestCreateTag()
      {
         ProtonPooledTagGenerator generator = new ProtonPooledTagGenerator();
         Assert.IsNotNull(generator.NextTag());
      }

      [Test]
      public void TestCreateTagsFromPoolAndReturn()
      {
         ProtonPooledTagGenerator generator = new ProtonPooledTagGenerator();

         IList<IDeliveryTag> tags = new List<IDeliveryTag>(ProtonPooledTagGenerator.DEFAULT_MAX_NUM_POOLED_TAGS);

         for (int i = 0; i < ProtonPooledTagGenerator.DEFAULT_MAX_NUM_POOLED_TAGS; ++i)
         {
            tags.Add(generator.NextTag());
         }

         foreach(IDeliveryTag tag in tags)
         {
            tag.Release();
         }

         for (int i = 0; i < ProtonPooledTagGenerator.DEFAULT_MAX_NUM_POOLED_TAGS; ++i)
         {
            Assert.AreSame(tags[i], generator.NextTag());
         }

         IDeliveryTag nonCached = generator.NextTag();
         Assert.IsFalse(tags.Contains(nonCached));
         nonCached.Release();
         Assert.IsFalse(tags.Contains(nonCached));
      }

      [Test]
      public void TestConsumeAllPooledTagsAndThenReleaseAfterCreatingNonPooled()
      {
         ProtonPooledTagGenerator generator = new ProtonPooledTagGenerator();

         IDeliveryTag pooledTag = generator.NextTag();
         IDeliveryTag nonCached = generator.NextTag();

         Assert.AreNotSame(pooledTag, nonCached);

         pooledTag.Release();
         nonCached.Release();

         IDeliveryTag shouldBeCached = generator.NextTag();

         Assert.AreSame(pooledTag, shouldBeCached);
      }

      [Test]
      public void TestPooledTagReleaseIsIdempotent()
      {
         ProtonPooledTagGenerator generator = new ProtonPooledTagGenerator();

         IDeliveryTag pooledTag = generator.NextTag();

         pooledTag.Release();
         pooledTag.Release();
         pooledTag.Release();

         Assert.AreSame(pooledTag, generator.NextTag());
         Assert.AreNotSame(pooledTag, generator.NextTag());
         Assert.AreNotSame(pooledTag, generator.NextTag());
      }

      [Test]
      public void TestCreateTagsThatWrapAroundLimit()
      {
         ProtonPooledTagGenerator generator = new ProtonPooledTagGenerator();

         IList<IDeliveryTag> tags = new List<IDeliveryTag>(ProtonPooledTagGenerator.DEFAULT_MAX_NUM_POOLED_TAGS);

         for (int i = 0; i < ProtonPooledTagGenerator.DEFAULT_MAX_NUM_POOLED_TAGS; ++i)
         {
            tags.Add(generator.NextTag());
         }

         // Test that on wrap the tags start beyond the pooled values.
         generator.NextTagId = 0xFFFFFFFFFFFFFFFFul;

         IDeliveryTag maxUnsignedLong = generator.NextTag();
         IDeliveryTag nextTagAfterWrap = generator.NextTag();

         Assert.AreEqual(sizeof(ulong), maxUnsignedLong.TagBytes.Length);
         Assert.AreEqual(sizeof(ushort), nextTagAfterWrap.TagBytes.Length);

         short tagValue = getShort(nextTagAfterWrap.TagBytes);

         Assert.AreEqual(ProtonPooledTagGenerator.DEFAULT_MAX_NUM_POOLED_TAGS, tagValue);

         tags[0].Release();

         IDeliveryTag tagAfterRelease = generator.NextTag();

         Assert.AreSame(tags[0], tagAfterRelease);
      }

      [Test]
      public void TestTakeAllTagsReturnThemAndTakeThemAgainDefaultSize()
      {
         DoTestTakeAllTagsReturnThemAndTakeThemAgain(0);
      }

      [Test]
      public void TestTakeAllTagsReturnThemAndTakeThemAgain()
      {
         DoTestTakeAllTagsReturnThemAndTakeThemAgain(64);
      }

      private void DoTestTakeAllTagsReturnThemAndTakeThemAgain(ushort poolSize)
      {
         ProtonPooledTagGenerator generator;
         if (poolSize == 0)
         {
            generator = new ProtonPooledTagGenerator();
            poolSize = ProtonPooledTagGenerator.DEFAULT_MAX_NUM_POOLED_TAGS;
         }
         else
         {
            generator = new ProtonPooledTagGenerator(poolSize);
         }

         IList<IDeliveryTag> tags1 = new List<IDeliveryTag>(poolSize);
         IList<IDeliveryTag> tags2 = new List<IDeliveryTag>(poolSize);

         for (int i = 0; i < poolSize; ++i)
         {
            tags1.Add(generator.NextTag());
         }

         for (int i = 0; i < poolSize; ++i)
         {
            tags1[i].Release();
         }

         for (int i = 0; i < poolSize; ++i)
         {
            tags2.Add(generator.NextTag());
         }

         for (int i = 0; i < poolSize; ++i)
         {
            Assert.AreSame(tags1[i], tags2[i]);
         }
      }

      private short getShort(byte[] tagBytes)
      {
         return (short)((tagBytes[0] & 0xFF) << 8 | (tagBytes[1] & 0xFF) << 0);
      }

   }
}