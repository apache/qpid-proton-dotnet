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
using System.Threading;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientErrorConditionTest
   {
      [Test]
      public void TestCreateWithNullProtonError()
      {
         Assert.IsNull(ClientErrorCondition.AsProtonErrorCondition(null));
      }

      [Test]
      public void TestCreateWithErrorConditionThatOnlyHasConditionData()
      {
         IErrorCondition clientCondition = IErrorCondition.Create("amqp:error", null);

         Assert.AreEqual("amqp:error", clientCondition.Condition);
         Assert.IsNull(clientCondition.Description);
         Assert.IsNotNull(clientCondition.Info);

         ErrorCondition protonError = ((ClientErrorCondition)clientCondition).ProtonErrorCondition;

         Assert.IsNotNull(protonError);
         Assert.AreEqual("amqp:error", protonError.Condition.ToString());
         Assert.IsNull(protonError.Description);
         Assert.IsNull(protonError.Info);
      }

      [Test]
      public void TestCreateWithErrorConditionWithConditionAndDescription()
      {
         IErrorCondition clientCondition = IErrorCondition.Create("amqp:error", "example");

         Assert.AreEqual("amqp:error", clientCondition.Condition);
         Assert.AreEqual("example", clientCondition.Description);
         Assert.IsNotNull(clientCondition.Info);

         ErrorCondition protonError = ((ClientErrorCondition)clientCondition).ProtonErrorCondition;

         Assert.IsNotNull(protonError);
         Assert.AreEqual("amqp:error", protonError.Condition.ToString());
         Assert.AreEqual("example", protonError.Description);
         Assert.IsNull(protonError.Info);
      }

      [Test]
      public void TestCreateWithErrorConditionWithConditionAndDescriptionAndInfo()
      {
         IDictionary<string, object> infoMap = new Dictionary<string, object>();
         infoMap.Add("test", "value");

         IErrorCondition condition = IErrorCondition.Create("amqp:error", "example", infoMap);

         Assert.AreEqual("amqp:error", condition.Condition);
         Assert.AreEqual("example", condition.Description);
         Assert.AreEqual(infoMap, condition.Info);

         ErrorCondition protonError = ((ClientErrorCondition)condition).ProtonErrorCondition;

         Assert.IsNotNull(protonError);
         Assert.AreEqual("amqp:error", protonError.Condition.ToString());
         Assert.AreEqual("example", protonError.Description);
         Assert.AreEqual(infoMap, ClientConversionSupport.ToStringKeyedMap(protonError.Info));
      }

      [Test]
      public void TestCreateFromForeignErrorCondition()
      {
         Dictionary<string, object> infoMap = new Dictionary<string, object>();
         infoMap.Add("test", "value");

         IErrorCondition condition = new TestErrorCondition(infoMap);

         ErrorCondition protonError = ClientErrorCondition.AsProtonErrorCondition(condition);

         Assert.IsNotNull(protonError);
         Assert.AreEqual("amqp:error", protonError.Condition.ToString());
         Assert.AreEqual("example", protonError.Description);
         Assert.AreEqual(infoMap, ClientConversionSupport.ToStringKeyedMap(protonError.Info));
      }

      internal class TestErrorCondition : IErrorCondition
      {
         private IReadOnlyDictionary<string, object> infoMap;

         public TestErrorCondition(IReadOnlyDictionary<string, object> infoMap)
         {
            this.infoMap = infoMap;
         }

         public string Condition => "amqp:error";

         public string Description => "example";

         public IReadOnlyDictionary<string, object> Info => infoMap;
      }
   }
}