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

using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class HeaderTest
   {
      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new Header().ToString());
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.Header, new Header().Type);
      }

      [Test]
      public void TestIsEmpty()
      {
         Header header = new Header();

         Assert.IsTrue(header.IsEmpty());
         header.Durable = true;
         Assert.IsFalse(header.IsEmpty());
         header.ClearDurable();
         Assert.IsTrue(header.IsEmpty());
         header.Durable = false;
         Assert.IsTrue(header.IsEmpty());
      }

      [Test]
      public void TestCreate()
      {
         Header header = new Header();

         Assert.IsFalse(header.HasDurable());
         Assert.IsFalse(header.HasPriority());
         Assert.IsFalse(header.HasTimeToLive());
         Assert.IsFalse(header.HasFirstAcquirer());
         Assert.IsFalse(header.HasDeliveryCount());
         Assert.AreSame(header, header.Value);

         Assert.AreEqual(Header.DEFAULT_DURABILITY, header.Durable);
         Assert.AreEqual(Header.DEFAULT_PRIORITY, header.Priority);
         Assert.AreEqual(Header.DEFAULT_TIME_TO_LIVE, header.TimeToLive);
         Assert.AreEqual(Header.DEFAULT_FIRST_ACQUIRER, header.FirstAcquirer);
         Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT, header.DeliveryCount);
      }

      [Test]
      public void TestCopy()
      {
         Header header = new Header();

         header.Durable = !Header.DEFAULT_DURABILITY;
         header.Priority = (byte)(Header.DEFAULT_PRIORITY + 1);
         header.TimeToLive = Header.DEFAULT_TIME_TO_LIVE - 10;
         header.FirstAcquirer = !Header.DEFAULT_FIRST_ACQUIRER;
         header.DeliveryCount = Header.DEFAULT_DELIVERY_COUNT + 5;

         Assert.IsFalse(header.IsEmpty());

         Header copy = header.Copy();

         Assert.AreEqual(!Header.DEFAULT_DURABILITY, copy.Durable);
         Assert.AreEqual(Header.DEFAULT_PRIORITY + 1, copy.Priority);
         Assert.AreEqual(Header.DEFAULT_TIME_TO_LIVE - 10, copy.TimeToLive);
         Assert.AreEqual(!Header.DEFAULT_FIRST_ACQUIRER, copy.FirstAcquirer);
         Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT + 5, copy.DeliveryCount);

         Assert.IsFalse(header.IsEmpty());
      }

      [Test]
      public void TestReset()
      {
         Header header = new Header();

         header.Durable = !Header.DEFAULT_DURABILITY;
         header.Priority = (byte)(Header.DEFAULT_PRIORITY + 1);
         header.TimeToLive = Header.DEFAULT_TIME_TO_LIVE - 10;
         header.FirstAcquirer = !Header.DEFAULT_FIRST_ACQUIRER;
         header.DeliveryCount = Header.DEFAULT_DELIVERY_COUNT + 5;

         Assert.IsFalse(header.IsEmpty());

         header.Reset();

         Assert.IsTrue(header.IsEmpty());
      }

      [Test]
      public void TestClearDurable()
      {
         Header header = new Header();

         Assert.IsFalse(header.HasDurable());
         Assert.AreEqual(Header.DEFAULT_DURABILITY, header.Durable);

         header.Durable = !Header.DEFAULT_DURABILITY;
         Assert.IsTrue(header.HasDurable());
         Assert.AreNotEqual(Header.DEFAULT_DURABILITY, header.Durable);

         header.ClearDurable();
         Assert.IsFalse(header.HasDurable());
         Assert.AreEqual(Header.DEFAULT_DURABILITY, header.Durable);
      }

      [Test]
      public void TestClearPriority()
      {
         Header header = new Header();

         Assert.IsFalse(header.HasPriority());
         Assert.AreEqual(Header.DEFAULT_PRIORITY, header.Priority);

         header.Priority = (byte)(Header.DEFAULT_PRIORITY + 1);
         Assert.IsTrue(header.HasPriority());
         Assert.AreNotEqual(Header.DEFAULT_PRIORITY, header.Priority);

         header.ClearPriority();
         Assert.IsFalse(header.HasPriority());
         Assert.AreEqual(Header.DEFAULT_PRIORITY, header.Priority);

         header.Priority = Header.DEFAULT_PRIORITY;
         Assert.IsFalse(header.HasPriority());
         Assert.AreEqual(Header.DEFAULT_PRIORITY, header.Priority);
      }

      [Test]
      public void TestClearTimeToLive()
      {
         Header header = new Header();

         Assert.IsFalse(header.HasTimeToLive());
         Assert.AreEqual(Header.DEFAULT_TIME_TO_LIVE, header.TimeToLive);

         header.TimeToLive = Header.DEFAULT_TIME_TO_LIVE - 10;
         Assert.IsTrue(header.HasTimeToLive());
         Assert.AreNotEqual(Header.DEFAULT_TIME_TO_LIVE, header.TimeToLive);

         header.ClearTimeToLive();
         Assert.IsFalse(header.HasTimeToLive());
         Assert.AreEqual(Header.DEFAULT_TIME_TO_LIVE, header.TimeToLive);

         header.TimeToLive = 0;
         Assert.IsTrue(header.HasTimeToLive());
         Assert.AreEqual(0, header.TimeToLive);

         header.TimeToLive = uint.MaxValue;
         Assert.IsTrue(header.HasTimeToLive());
         Assert.AreEqual(uint.MaxValue, header.TimeToLive);
      }

      [Test]
      public void TestClearFirstAcquirer()
      {
         Header header = new Header();

         Assert.IsFalse(header.HasFirstAcquirer());
         Assert.AreEqual(Header.DEFAULT_FIRST_ACQUIRER, header.FirstAcquirer);

         header.FirstAcquirer = !Header.DEFAULT_FIRST_ACQUIRER;
         Assert.IsTrue(header.HasFirstAcquirer());
         Assert.AreNotEqual(Header.DEFAULT_FIRST_ACQUIRER, header.FirstAcquirer);

         header.ClearFirstAcquirer();
         Assert.IsFalse(header.HasFirstAcquirer());
         Assert.AreEqual(Header.DEFAULT_FIRST_ACQUIRER, header.FirstAcquirer);
      }

      [Test]
      public void TestClearDeliveryCount()
      {
         Header header = new Header();

         Assert.IsFalse(header.HasDeliveryCount());
         Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT, header.DeliveryCount);

         header.DeliveryCount = Header.DEFAULT_DELIVERY_COUNT + 10;
         Assert.IsTrue(header.HasDeliveryCount());
         Assert.AreNotEqual(Header.DEFAULT_DELIVERY_COUNT, header.DeliveryCount);

         header.ClearDeliveryCount();
         Assert.IsFalse(header.HasDeliveryCount());
         Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT, header.DeliveryCount);

         header.DeliveryCount = Header.DEFAULT_DELIVERY_COUNT;
         Assert.IsFalse(header.HasDeliveryCount());
         Assert.AreEqual(Header.DEFAULT_DELIVERY_COUNT, header.DeliveryCount);

         header.DeliveryCount = uint.MaxValue;
         Assert.IsTrue(header.HasDeliveryCount());
         Assert.AreEqual(uint.MaxValue, header.DeliveryCount);
      }
   }
}
