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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class TargetTypeTest
   {
      [Test]
      public void TestCreate()
      {
         Target target = new Target();

         Assert.IsNull(target.Address);
         Assert.AreEqual(TerminusDurability.None, target.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, target.ExpiryPolicy);
         Assert.AreEqual(0u, target.Timeout);
         Assert.IsFalse(target.Dynamic);
         Assert.IsNull(target.DynamicNodeProperties);
         Assert.IsNull(target.Capabilities);
      }

      [Test]
      public void TestCopyFromDefault()
      {
         Target target = new Target();

         Assert.IsNull(target.Address);
         Assert.AreEqual(TerminusDurability.None, target.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, target.ExpiryPolicy);
         Assert.AreEqual(0u, target.Timeout);
         Assert.IsFalse(target.Dynamic);
         Assert.IsNull(target.DynamicNodeProperties);
         Assert.IsNull(target.Capabilities);

         Target copy = target.Copy();

         Assert.IsNull(copy.Address);
         Assert.AreEqual(TerminusDurability.None, copy.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, copy.ExpiryPolicy);
         Assert.AreEqual(0u, copy.Timeout);
         Assert.IsFalse(copy.Dynamic);
         Assert.IsNull(copy.DynamicNodeProperties);
         Assert.IsNull(copy.Capabilities);
      }

      [Test]
      public void TestCopyWithValues()
      {
         Target target = new Target();

         IDictionary<Symbol, object> dynamicProperties = new Dictionary<Symbol, object>();
         dynamicProperties.Add(Symbol.Lookup("test"), "test");

         Assert.IsNull(target.Address);
         Assert.AreEqual(TerminusDurability.None, target.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, target.ExpiryPolicy);
         Assert.AreEqual(0u, target.Timeout);
         Assert.IsFalse(target.Dynamic);
         target.DynamicNodeProperties = dynamicProperties;
         Assert.IsNotNull(target.DynamicNodeProperties);
         target.Capabilities = new Symbol[] { Symbol.Lookup("test") };
         Assert.IsNotNull(target.Capabilities);

         Target copy = target.Copy();

         Assert.IsNull(copy.Address);
         Assert.AreEqual(TerminusDurability.None, copy.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, copy.ExpiryPolicy);
         Assert.AreEqual(0u, copy.Timeout);
         Assert.IsFalse(copy.Dynamic);
         Assert.IsNotNull(copy.DynamicNodeProperties);
         Assert.AreEqual(dynamicProperties, copy.DynamicNodeProperties);
         Assert.IsNotNull(copy.Capabilities);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("test") }, target.Capabilities);

         Assert.AreEqual(target.ToString(), copy.ToString());
      }

      [Test]
      public void TestSetExpiryPolicy()
      {
         Target target = new Target();

         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, target.ExpiryPolicy);
         target.ExpiryPolicy = TerminusExpiryPolicy.ConnectionClose;
         Assert.AreEqual(TerminusExpiryPolicy.ConnectionClose, target.ExpiryPolicy);
         target.ExpiryPolicy = TerminusExpiryPolicy.SessionEnd;
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, target.ExpiryPolicy);
      }

      [Test]
      public void TestTerminusDurability()
      {
         Target target = new Target();

         Assert.AreEqual(TerminusDurability.None, target.Durable);
         target.Durable = TerminusDurability.UnsettledState;
         Assert.AreEqual(TerminusDurability.UnsettledState, target.Durable);
         target.Durable = TerminusDurability.None;
         Assert.AreEqual(TerminusDurability.None, target.Durable);
      }

      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new Target().ToString());
      }
   }
}