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
   public class AmqpValueTypeTest
   {
      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new AmqpValue(null).ToString());
      }

      [Test]
      public void TestGetValueFromEmptySection()
      {
         Assert.IsNull(new AmqpValue(null).Value);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(((AmqpValue)new AmqpValue(null).Clone()).Value);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.AmqpValue, new AmqpValue(null).Type);
      }

      [Test]
      public void TestHashCode()
      {
         string first = new string("first");
         string second = new string("second");

         AmqpValue original = new AmqpValue(first);
         AmqpValue copy = (AmqpValue)original.Clone();
         AmqpValue another = new AmqpValue(second);

         Assert.AreEqual(original.GetHashCode(), copy.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), another.GetHashCode());

         AmqpValue empty = new AmqpValue(null);
         AmqpValue empty2 = new AmqpValue(null);

         Assert.AreEqual(empty2.GetHashCode(), empty.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), empty.GetHashCode());
      }

      [Test]
      public void TestEquals()
      {
         string first = new string("first");
         string second = new string("second");

         AmqpValue original = new AmqpValue(first);
         AmqpValue copy = (AmqpValue)original.Clone();
         AmqpValue another = new AmqpValue(second);
         AmqpValue empty = new AmqpValue(null);
         AmqpValue empty2 = new AmqpValue(null);

         Assert.AreEqual(original, original);
         Assert.AreEqual(original, copy);
         Assert.AreNotEqual(original, another);
         Assert.AreNotEqual(original, "test");
         Assert.AreNotEqual(original, empty);
         Assert.AreNotEqual(empty, original);
         Assert.AreEqual(empty, empty2);

         Assert.IsFalse(original.Equals(null));
      }
   }
}