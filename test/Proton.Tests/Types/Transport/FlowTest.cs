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
   public class FlowTest
   {
      [Test]
      public void TestGetPerformativeType()
      {
         Assert.AreEqual(PerformativeType.Flow, new Flow().Type);
      }

      [Test]
      public void TestToStringOnFreshInstance()
      {
         Assert.IsNotNull(new Flow().ToString());
      }

      [Test]
      public void TestCopy()
      {
         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), "test1");

         Flow flow = new Flow();

         flow.Available = 1024;
         flow.DeliveryCount = 5;
         flow.Drain = true;
         flow.Echo = true;
         flow.Handle = 3;
         flow.IncomingWindow = 1024;
         flow.LinkCredit = 255;
         flow.NextIncomingId = 12;
         flow.NextOutgoingId = 13;
         flow.OutgoingWindow = 2048;
         flow.Properties = properties;

         Flow copy = flow.Copy();

         Assert.AreEqual(flow.Available, copy.Available);
         Assert.AreEqual(flow.DeliveryCount, copy.DeliveryCount);
         Assert.AreEqual(flow.Drain, copy.Drain);
         Assert.AreEqual(flow.Echo, copy.Echo);
         Assert.AreEqual(flow.Handle, copy.Handle);
         Assert.AreEqual(flow.IncomingWindow, copy.IncomingWindow);
         Assert.AreEqual(flow.OutgoingWindow, copy.OutgoingWindow);
         Assert.AreEqual(flow.NextIncomingId, copy.NextIncomingId);
         Assert.AreEqual(flow.NextOutgoingId, copy.NextOutgoingId);
         Assert.AreEqual(flow.Properties, copy.Properties);
         Assert.AreEqual(flow.LinkCredit, copy.LinkCredit);
      }

      [Test]
      public void TestClearFieldsAPI()
      {
         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), "test1");

         Flow flow = new Flow();

         flow.Available = 1024;
         flow.DeliveryCount = 5;
         flow.Drain = true;
         flow.Echo = true;
         flow.Handle = 3;
         flow.IncomingWindow = 1024;
         flow.LinkCredit = 255;
         flow.NextIncomingId = 12;
         flow.NextOutgoingId = 13;
         flow.OutgoingWindow = 2048;
         flow.Properties = properties;

         Assert.AreEqual(11, flow.GetElementCount());
         Assert.IsFalse(flow.IsEmpty());
         Assert.IsTrue(flow.HasAvailable());
         Assert.IsTrue(flow.HasDeliveryCount());
         Assert.IsTrue(flow.HasDrain());
         Assert.IsTrue(flow.HasEcho());
         Assert.IsTrue(flow.HasHandle());
         Assert.IsTrue(flow.HasIncomingWindow());
         Assert.IsTrue(flow.HasLinkCredit());
         Assert.IsTrue(flow.HasNextIncomingId());
         Assert.IsTrue(flow.HasNextOutgoingId());
         Assert.IsTrue(flow.HasOutgoingWindow());
         Assert.IsTrue(flow.HasProperties());

         Assert.IsNotNull(flow.ToString()); // Ensure fully populated toString does not error

         flow.ClearAvailable();
         flow.ClearDeliveryCount();
         flow.ClearDrain();
         flow.ClearEcho();
         flow.ClearHandle();
         flow.ClearIncomingWindow();
         flow.ClearLinkCredit();
         flow.ClearNextIncomingId();
         flow.ClearNextOutgoingId();
         flow.ClearOutgoingWindow();
         flow.ClearProperties();

         Assert.AreEqual(0, flow.GetElementCount());
         Assert.IsTrue(flow.IsEmpty());
         Assert.IsFalse(flow.HasAvailable());
         Assert.IsFalse(flow.HasDeliveryCount());
         Assert.IsFalse(flow.HasDrain());
         Assert.IsFalse(flow.HasEcho());
         Assert.IsFalse(flow.HasHandle());
         Assert.IsFalse(flow.HasIncomingWindow());
         Assert.IsFalse(flow.HasLinkCredit());
         Assert.IsFalse(flow.HasNextIncomingId());
         Assert.IsFalse(flow.HasNextOutgoingId());
         Assert.IsFalse(flow.HasOutgoingWindow());
         Assert.IsFalse(flow.HasProperties());

         flow.Properties = properties;
         Assert.IsTrue(flow.HasProperties());
         flow.Properties = null;
         Assert.IsFalse(flow.HasProperties());
      }

      [Test]
      public void TestInitialState()
      {
         Flow flow = new Flow();

         Assert.AreEqual(0, flow.GetElementCount());
         Assert.IsTrue(flow.IsEmpty());
         Assert.IsFalse(flow.HasAvailable());
         Assert.IsFalse(flow.HasDeliveryCount());
         Assert.IsFalse(flow.HasDrain());
         Assert.IsFalse(flow.HasEcho());
         Assert.IsFalse(flow.HasHandle());
         Assert.IsFalse(flow.HasIncomingWindow());
         Assert.IsFalse(flow.HasLinkCredit());
         Assert.IsFalse(flow.HasNextIncomingId());
         Assert.IsFalse(flow.HasNextOutgoingId());
         Assert.IsFalse(flow.HasOutgoingWindow());
         Assert.IsFalse(flow.HasProperties());
      }

      [Test]
      public void TestIsEmpty()
      {
         Flow flow = new Flow();

         Assert.AreEqual(0, flow.GetElementCount());
         Assert.IsTrue(flow.IsEmpty());
         Assert.IsFalse(flow.HasLinkCredit());

         flow.LinkCredit = 10;

         Assert.IsNotNull(flow.ToString()); // Ensure partially populated toString does not error
         Assert.IsTrue(flow.GetElementCount() > 0);
         Assert.IsFalse(flow.IsEmpty());
         Assert.IsTrue(flow.HasLinkCredit());

         flow.LinkCredit = 0;

         Assert.IsTrue(flow.GetElementCount() > 0);
         Assert.IsFalse(flow.IsEmpty());
         Assert.IsTrue(flow.HasLinkCredit());
      }

      [Test]
      public void TestCopyFromNew()
      {
         Flow original = new Flow();
         Flow copy = original.Copy();

         Assert.IsTrue(original.IsEmpty());
         Assert.IsTrue(copy.IsEmpty());

         Assert.AreEqual(0, original.GetElementCount());
         Assert.AreEqual(0, copy.GetElementCount());
      }
   }
}
