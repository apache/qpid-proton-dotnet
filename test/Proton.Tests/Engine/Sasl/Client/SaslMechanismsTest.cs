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
using Apache.Qpid.Proton.Types;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   [TestFixture, Timeout(20000)]
   public class SaslMechanismsTest : MechanismTestBase
   {
      [Test]
      public void TestValueOfAnonymous()
      {
         SaslMechanism mech = SaslMechanisms.Lookup(AnonymousMechanism.ANONYMOUS);
         Assert.IsNotNull(mech);
         Assert.AreEqual(SaslMechanism.Anonymous, mech);
      }

      [Test]
      public void TestRequestInvalidMechanismName()
      {
         try
         {
            SaslMechanisms.Lookup(Symbol.Lookup("TEST"));
            Assert.Fail("Should throw when invalid mechanism name given.");
         }
         catch (ArgumentException)
         {
            // Expected
         }
      }
   }
}