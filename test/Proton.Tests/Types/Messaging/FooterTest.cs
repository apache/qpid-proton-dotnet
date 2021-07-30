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

using System.Collections.Generic;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class FooterTest
   {
      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new Footer().ToString());
      }

      [Test]
      public void TestGetMapFromEmptySection()
      {
         Assert.IsNull(new Footer().Value);
      }

      [Test]
      public void TestCopy()
      {
         IDictionary<Symbol, object> payload = new Dictionary<Symbol, object>();
         payload.Add(Symbol.Lookup("key"), "value");

         Footer original = new Footer(payload);
         Footer copy = original.Copy();

         Assert.AreNotSame(original, copy);
         Assert.AreNotSame(original.Value, copy.Value);
         Assert.AreEqual(original.Value, copy.Value);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(new Footer().Copy().Value);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.Footer, new Footer().Type);
      }

      [Test]
      public void TestHashCode()
      {
         IDictionary<Symbol, object> payload1 = new Dictionary<Symbol, object>();
         payload1.Add(Symbol.Lookup("key"), "value");

         IDictionary<Symbol, object> payload2 = new Dictionary<Symbol, object>();
         payload1.Add(Symbol.Lookup("key1"), "value");
         payload1.Add(Symbol.Lookup("key2"), "value");

         Footer original = new Footer(payload1);
         Footer copy = original.Copy();
         Footer another = new Footer(payload2);

         Assert.AreNotEqual(original.GetHashCode(), copy.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), another.GetHashCode());

         Footer empty = new Footer();
         Footer empty2 = new Footer();

         Assert.AreEqual(empty2.GetHashCode(), empty.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), empty.GetHashCode());
      }

      [Test]
      public void TestEquals()
      {
         IDictionary<Symbol, object> payload1 = new Dictionary<Symbol, object>();
         payload1.Add(Symbol.Lookup("key"), "value");

         IDictionary<Symbol, object> payload2 = new Dictionary<Symbol, object>();
         payload2.Add(Symbol.Lookup("key1"), "value");
         payload2.Add(Symbol.Lookup("key2"), "value");

         Footer original = new Footer(payload1);
         Footer copy = original.Copy();
         Footer another = new Footer(payload2);
         Footer empty = new Footer();
         Footer empty2 = new Footer();

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