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
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class ApplicationPropertiesTest
   {
      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new ApplicationProperties((IDictionary<string, object>)null).ToString());
      }

      [Test]
      public void TestGetPropertiesFromEmptySection()
      {
         Assert.IsNull(new ApplicationProperties().Value);
      }

      [Test]
      public void TestCannotCreateCopyFromNullSource()
      {
         Assert.Throws<NullReferenceException>(() => new ApplicationProperties((ApplicationProperties)null));
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(((ApplicationProperties)new ApplicationProperties((IDictionary<string, object>)null).Clone()).Value);
      }

      [Test]
      public void TestCopy()
      {
         IDictionary<string, object> payload = new Dictionary<string, object>();
         payload.Add("key", "value");

         ApplicationProperties original = new ApplicationProperties(payload);
         ApplicationProperties copy = (ApplicationProperties)original.Clone();

         Assert.AreNotSame(original, copy);
         Assert.AreNotSame(original.Value, copy.Value);
         Assert.AreEqual(original.Value, copy.Value);
      }

      [Test]
      public void TestHashCode()
      {
         IDictionary<string, object> payload1 = new Dictionary<string, object>();
         payload1.Add("key", "value");

         IDictionary<string, object> payload2 = new Dictionary<string, object>();
         payload2.Add("key1", "value");
         payload2.Add("key2", "value");

         ApplicationProperties original = new ApplicationProperties(payload1);
         ApplicationProperties copy = (ApplicationProperties)original.Clone();
         ApplicationProperties another = new ApplicationProperties(payload2);

         Assert.AreNotEqual(original.GetHashCode(), copy.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), another.GetHashCode());

         ApplicationProperties empty = new ApplicationProperties();
         ApplicationProperties empty2 = new ApplicationProperties();

         Assert.AreEqual(empty2.GetHashCode(), empty.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), empty.GetHashCode());
      }

      [Test]
      public void TestEquals()
      {
         IDictionary<string, object> payload1 = new Dictionary<string, object>();
         payload1.Add("key", "value");

         IDictionary<string, object> payload2 = new Dictionary<string, object>();
         payload2.Add("key1", "value");
         payload2.Add("key2", "value");

         ApplicationProperties original = new ApplicationProperties(payload1);
         ApplicationProperties copy = original.Copy();
         ApplicationProperties another = new ApplicationProperties(payload2);
         ApplicationProperties empty = new ApplicationProperties();
         ApplicationProperties empty2 = new ApplicationProperties();

         Assert.AreEqual(original, original);
         Assert.AreEqual(original, copy);
         Assert.AreNotEqual(original, another);
         Assert.AreNotEqual(original, "test");
         Assert.AreNotEqual(original, empty);
         Assert.AreNotEqual(empty, original);
         Assert.AreEqual(empty, empty2);

         Assert.IsFalse(original.Equals(null));
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.ApplicationProperties, new ApplicationProperties().Type);
      }
   }
}