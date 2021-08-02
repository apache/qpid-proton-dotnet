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

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class BeginTest
   {
      [Test]
      public void TestGetPerformativeType()
      {
         Assert.AreEqual(PerformativeType.Begin, new Begin().Type);
      }

      [Test]
      public void TestToStringOnFreshInstance()
      {
         Assert.IsNotNull(new Begin().ToString());
      }

      [Test]
      public void TestHasMethods()
      {
         Begin begin = new Begin();

         Assert.IsFalse(begin.HasHandleMax());
         Assert.IsFalse(begin.HasNextOutgoingId());
         Assert.IsFalse(begin.HasDesiredCapabilities());
         Assert.IsFalse(begin.HasOfferedCapabilities());
         Assert.IsFalse(begin.HasOutgoingWindow());
         Assert.IsFalse(begin.HasIncomingWindow());
         Assert.IsFalse(begin.HasProperties());
         Assert.IsFalse(begin.HasRemoteChannel());

         begin.DesiredCapabilities = new Symbol[] { Symbol.Lookup("test") };
         begin.OfferedCapabilities = new Symbol[] { Symbol.Lookup("test") };
         begin.HandleMax = 65535;
         begin.IncomingWindow = 255;
         begin.OutgoingWindow = int.MaxValue;
         begin.RemoteChannel = 1;
         begin.Properties = new Dictionary<Symbol, object>();
         begin.NextOutgoingId = 32767;

         Assert.IsTrue(begin.HasHandleMax());
         Assert.IsTrue(begin.HasNextOutgoingId());
         Assert.IsTrue(begin.HasDesiredCapabilities());
         Assert.IsTrue(begin.HasOfferedCapabilities());
         Assert.IsTrue(begin.HasOutgoingWindow());
         Assert.IsTrue(begin.HasIncomingWindow());
         Assert.IsTrue(begin.HasProperties());
         Assert.IsTrue(begin.HasRemoteChannel());
      }

      [Test]
      public void TestHandleMaxIfSetIsAlwaysPresent()
      {
         Begin begin = new Begin();

         Assert.IsFalse(begin.HasHandleMax());
         begin.HandleMax = 0;
         Assert.IsTrue(begin.HasHandleMax());
         begin.HandleMax = 65535;
         Assert.IsTrue(begin.HasHandleMax());
         begin.HandleMax = uint.MaxValue;
         Assert.IsTrue(begin.HasHandleMax());
      }

      [Test]
      public void TestIsEmpty()
      {
         Begin begin = new Begin();

         Assert.AreEqual(0, begin.GetElementCount());
         Assert.IsTrue(begin.IsEmpty());
         Assert.IsFalse(begin.HasOutgoingWindow());

         begin.OutgoingWindow = 1;

         Assert.IsTrue(begin.GetElementCount() > 0);
         Assert.IsFalse(begin.IsEmpty());
         Assert.IsTrue(begin.HasOutgoingWindow());

         begin.OutgoingWindow = 0;

         Assert.IsTrue(begin.GetElementCount() > 0);
         Assert.IsFalse(begin.IsEmpty());
         Assert.IsTrue(begin.HasOutgoingWindow());
      }

      [Test]
      public void TestCopyFromNew()
      {
         Begin original = new Begin();
         Begin copy = original.Copy();

         Assert.IsTrue(original.IsEmpty());
         Assert.IsTrue(copy.IsEmpty());

         Assert.AreEqual(0, original.GetElementCount());
         Assert.AreEqual(0, copy.GetElementCount());
      }

      [Test]
      public void TestCopyHandlesProperties()
      {
         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test1"), "one");
         properties.Add(Symbol.Lookup("test2"), "two");
         properties.Add(Symbol.Lookup("test3"), "three");

         Begin begin = new Begin();
         begin.Properties = properties;

         Begin copied = begin.Copy();

         Assert.IsTrue(begin.HasProperties());
         Assert.IsTrue(copied.HasProperties());

         Assert.AreEqual(copied.Properties, begin.Properties);
         Assert.AreEqual(copied.Properties, properties);
      }

      [Test]
      public void TestCopyHandlesDesiredCapabilities()
      {
         Symbol[] desiredCapabilities = { Symbol.Lookup("test1"),
                                         Symbol.Lookup("test2"),
                                         Symbol.Lookup("test3") };

         Begin begin = new Begin();
         begin.DesiredCapabilities = desiredCapabilities;

         Begin copied = begin.Copy();

         Assert.IsTrue(begin.HasDesiredCapabilities());
         Assert.IsTrue(copied.HasDesiredCapabilities());

         Assert.AreEqual(copied.DesiredCapabilities, begin.DesiredCapabilities);
         Assert.AreEqual(copied.DesiredCapabilities, desiredCapabilities);
      }

      [Test]
      public void TestCopyHandlesOfferedCapabilities()
      {
         Symbol[] offeredCapabilities = { Symbol.Lookup("test1"),
                                         Symbol.Lookup("test2"),
                                         Symbol.Lookup("test3") };

         Begin begin = new Begin();
         begin.OfferedCapabilities = offeredCapabilities;

         Begin copied = begin.Copy();

         Assert.IsTrue(begin.HasOfferedCapabilities());
         Assert.IsTrue(copied.HasOfferedCapabilities());

         Assert.AreEqual(copied.OfferedCapabilities, begin.OfferedCapabilities);
         Assert.AreEqual(copied.OfferedCapabilities, offeredCapabilities);
      }
   }
}