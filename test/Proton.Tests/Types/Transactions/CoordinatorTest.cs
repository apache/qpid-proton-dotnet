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

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class CoordinatorTests
   {
      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new Coordinator().ToString());
      }

      [Test]
      public void TestCopyOnEmpty()
      {
         Assert.IsNotNull(new Coordinator().Copy());
      }

      [Test]
      public void TestCopy()
      {
         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };

         Coordinator copy = coordinator.Copy();

         Assert.AreNotSame(copy.Capabilities, coordinator.Capabilities);
         Assert.AreEqual(copy.Capabilities, coordinator.Capabilities);

         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN, TxnCapability.PROMOTABLE_TXN };

         copy = coordinator.Copy();

         Assert.AreNotSame(copy.Capabilities, coordinator.Capabilities);
         Assert.AreEqual(copy.Capabilities, coordinator.Capabilities);
      }

      [Test]
      public void TestCapabilities()
      {
         Coordinator coordinator = new Coordinator();

         Assert.IsNull(coordinator.Capabilities);
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Assert.IsNotNull(coordinator.Capabilities);
         Assert.IsNotNull(coordinator.ToString());

         Assert.AreEqual(new Symbol[] { TxnCapability.LOCAL_TXN }, coordinator.Capabilities);
      }

      [Test]
      public void TestToStringWithCapabilities()
      {
         Coordinator coordinator = new Coordinator();
         coordinator.Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN };
         Assert.IsNotNull(new Coordinator().ToString());
      }
   }
}