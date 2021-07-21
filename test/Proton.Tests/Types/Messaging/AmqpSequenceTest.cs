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

using System.Collections;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class AmqpSequenceTypeTest
   {
      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new AmqpSequence((IList)null).ToString());
      }

      [Test]
      public void TestGetSequenceFromEmptySection()
      {
         Assert.IsNull(new AmqpSequence().Value);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(((AmqpSequence) new AmqpSequence().Copy()).Value);
      }

      [Test]
      public void TestCopy()
      {
         IList payload = new ArrayList();
         payload.Add("test");

         AmqpSequence original = new AmqpSequence(payload);
         AmqpSequence copy = original.Copy();

         Assert.AreNotSame(original, copy);
         Assert.AreNotSame(original.Value, copy.Value);
         Assert.AreEqual(original.Value, copy.Value);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.AmqpSequence, new AmqpSequence().Type);
      }

      [Test]
      public void TestHashCode()
      {
         IList first = new ArrayList();
         first.Add("first");

         IList second = new ArrayList();
         second.Add("second");

         AmqpSequence original = new AmqpSequence(first);
         AmqpSequence copy = original.Copy();
         AmqpSequence another = new AmqpSequence(second);

         Assert.AreEqual(original.GetHashCode(), original.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), copy.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), another.GetHashCode());

         AmqpSequence empty = new AmqpSequence();
         AmqpSequence empty2 = new AmqpSequence();

         Assert.AreEqual(empty2.GetHashCode(), empty.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), empty.GetHashCode());
      }

      [Test]
      public void TestEquals()
      {
         IList first = new ArrayList();
         first.Add("first");

         IList second = new ArrayList();
         second.Add("second");

         AmqpSequence original = new AmqpSequence(first);
         AmqpSequence copy = original.Copy();
         AmqpSequence another = new AmqpSequence(second);
         AmqpSequence empty = new AmqpSequence();
         AmqpSequence empty2 = new AmqpSequence();

         Assert.AreEqual(original, original);
         Assert.AreNotEqual(original, copy);
         Assert.AreNotEqual(original, another);
         Assert.AreNotEqual(original, "test");
         Assert.AreNotEqual(original, empty);
         Assert.AreNotEqual(empty, original);
         Assert.AreEqual(empty, empty2);

         Assert.IsFalse(original.Equals(null));
      }
   }
}