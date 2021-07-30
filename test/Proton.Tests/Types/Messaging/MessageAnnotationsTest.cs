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
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class MessageAnnotationsTest
   {
      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new MessageAnnotations().ToString());
      }

      [Test]
      public void TestGetSequenceFromEmptySection()
      {
         Assert.IsNull(new MessageAnnotations().Value);
      }

      [Test]
      public void TestCopy()
      {
         IDictionary<Symbol, object> payload = new Dictionary<Symbol, object>();
         payload.Add(AmqpError.DECODE_ERROR, "value");

         MessageAnnotations original = new MessageAnnotations(payload);
         MessageAnnotations copy = original.Copy();

         Assert.AreNotSame(original, copy);
         Assert.AreNotSame(original.Value, copy.Value);
         Assert.AreEqual(original.Value, copy.Value);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(new MessageAnnotations().Copy().Value);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.MessageAnnotations, new MessageAnnotations().Type);
      }

      [Test]
      public void TestHashCode()
      {
         IDictionary<Symbol, object> payload1 = new Dictionary<Symbol, object>();
         payload1.Add(Symbol.Lookup("key"), "value");

         IDictionary<Symbol, object> payload2 = new Dictionary<Symbol, object>();
         payload2.Add(Symbol.Lookup("key1"), "value");
         payload2.Add(Symbol.Lookup("key2"), "value");

         MessageAnnotations original = new MessageAnnotations(payload1);
         MessageAnnotations copy = original.Copy();
         MessageAnnotations another = new MessageAnnotations(payload2);

         Assert.AreNotEqual(original.GetHashCode(), copy.GetHashCode());
         Assert.AreNotEqual(original.GetHashCode(), another.GetHashCode());

         MessageAnnotations empty = new MessageAnnotations();
         MessageAnnotations empty2 = new MessageAnnotations();

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

         MessageAnnotations original = new MessageAnnotations(payload1);
         MessageAnnotations copy = original.Copy();
         MessageAnnotations another = new MessageAnnotations(payload2);
         MessageAnnotations empty = new MessageAnnotations();
         MessageAnnotations empty2 = new MessageAnnotations();

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