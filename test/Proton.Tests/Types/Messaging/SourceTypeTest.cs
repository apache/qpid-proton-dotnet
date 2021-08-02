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
using Apache.Qpid.Proton.Types.Transport;
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class SourceTypeTest
   {
      [Test]
      public void TestSetExpiryPolicy()
      {
         Source source = new Source();

         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, source.ExpiryPolicy);
         source.ExpiryPolicy = TerminusExpiryPolicy.ConnectionClose;
         Assert.AreEqual(TerminusExpiryPolicy.ConnectionClose, source.ExpiryPolicy);
         source.ExpiryPolicy = TerminusExpiryPolicy.SessionEnd;
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, source.ExpiryPolicy);
      }

      [Test]
      public void TestTerminusDurability()
      {
         Source source = new Source();

         Assert.AreEqual(TerminusDurability.None, source.Durable);
         source.Durable = TerminusDurability.UnsettledState;
         Assert.AreEqual(TerminusDurability.UnsettledState, source.Durable);
         source.Durable = TerminusDurability.None;
         Assert.AreEqual(TerminusDurability.None, source.Durable);
      }

      [Test]
      public void TestCreate()
      {
         Source source = new Source();

         Assert.IsNull(source.Address);
         Assert.AreEqual(TerminusDurability.None, source.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, source.ExpiryPolicy);
         Assert.AreEqual(0u, source.Timeout);
         Assert.IsFalse(source.Dynamic);
         Assert.IsNull(source.DynamicNodeProperties);
         Assert.IsNull(source.DistributionMode);
         Assert.IsNull(source.Filter);
         Assert.IsNull(source.DefaultOutcome);
         Assert.IsNull(source.Outcomes);
         Assert.IsNull(source.Capabilities);
      }

      [Test]
      public void TestCopyFromDefault()
      {
         Source source = new Source();

         Assert.IsNull(source.Address);
         Assert.AreEqual(TerminusDurability.None, source.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, source.ExpiryPolicy);
         Assert.AreEqual(0u, source.Timeout);
         Assert.IsFalse(source.Dynamic);
         Assert.IsNull(source.DynamicNodeProperties);
         Assert.IsNull(source.DistributionMode);
         Assert.IsNull(source.Filter);
         Assert.IsNull(source.DefaultOutcome);
         Assert.IsNull(source.Outcomes);
         Assert.IsNull(source.Capabilities);

         Source copy = source.Copy();

         Assert.IsNull(copy.Address);
         Assert.AreEqual(TerminusDurability.None, copy.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, copy.ExpiryPolicy);
         Assert.AreEqual(0u, copy.Timeout);
         Assert.IsFalse(copy.Dynamic);
         Assert.IsNull(copy.DynamicNodeProperties);
         Assert.IsNull(copy.DistributionMode);
         Assert.IsNull(copy.Filter);
         Assert.IsNull(copy.DefaultOutcome);
         Assert.IsNull(copy.Outcomes);
         Assert.IsNull(copy.Capabilities);
      }

      [Test]
      public void TestCopyWithValues()
      {
         Source source = new Source();

         IDictionary<Symbol, object> dynamicProperties = new Dictionary<Symbol, object>();
         dynamicProperties.Add(Symbol.Lookup("test"), "test");
         IDictionary<Symbol, object> filter = new Dictionary<Symbol, object>();
         filter.Add(Symbol.Lookup("filter"), "filter");

         Assert.IsNull(source.Address);
         Assert.AreEqual(TerminusDurability.None, source.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, source.ExpiryPolicy);
         Assert.AreEqual(0u, source.Timeout);
         Assert.IsFalse(source.Dynamic);
         source.DynamicNodeProperties = dynamicProperties;
         Assert.IsNotNull(source.DynamicNodeProperties);
         Assert.IsNull(source.DistributionMode);
         source.Filter = filter;
         Assert.IsNotNull(source.Filter);
         Assert.IsNull(source.DefaultOutcome);
         source.Outcomes = new Symbol[] { Symbol.Lookup("accepted") };
         Assert.IsNotNull(source.Outcomes);
         source.Capabilities = new Symbol[] { Symbol.Lookup("test") };
         Assert.IsNotNull(source.Capabilities);

         Source copy = source.Copy();

         Assert.IsNull(copy.Address);
         Assert.AreEqual(TerminusDurability.None, copy.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, copy.ExpiryPolicy);
         Assert.AreEqual(0u, copy.Timeout);
         Assert.IsFalse(copy.Dynamic);
         Assert.IsNotNull(copy.DynamicNodeProperties);
         Assert.AreEqual(dynamicProperties, copy.DynamicNodeProperties);
         Assert.IsNull(copy.DistributionMode);
         Assert.IsNotNull(copy.Filter);
         Assert.AreEqual(filter, copy.Filter);
         Assert.IsNull(copy.DefaultOutcome);
         Assert.IsNotNull(copy.Outcomes);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("accepted") }, source.Outcomes);
         Assert.IsNotNull(copy.Capabilities);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("test") }, source.Capabilities);

         Assert.AreEqual(source.ToString(), copy.ToString());
      }

      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new Source().ToString());
      }
   }
}