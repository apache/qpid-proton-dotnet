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

using Apache.Qpid.Proton.Types;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   [TestFixture, Timeout(20000)]
   public class SaslMechanismSelectorTest : MechanismTestBase
   {
      private static readonly Symbol[] TEST_MECHANISMS_ARRAY = { ExternalMechanism.EXTERNAL,
                                                                 CramMD5Mechanism.CRAM_MD5,
                                                                 PlainMechanism.PLAIN,
                                                                 AnonymousMechanism.ANONYMOUS };

      [Test]
      public void TestSelectAnonymousFromAll()
      {
         SaslMechanismSelector selector = new SaslMechanismSelector();

         IMechanism mech = selector.Select(TEST_MECHANISMS_ARRAY, EmptyCredentials());

         Assert.IsNotNull(mech);
         Assert.AreEqual(AnonymousMechanism.ANONYMOUS, mech.Name);
      }

      [Test]
      public void TestSelectPlain()
      {
         SaslMechanismSelector selector = new SaslMechanismSelector();

         IMechanism mech = selector.Select(new Symbol[] { PlainMechanism.PLAIN, AnonymousMechanism.ANONYMOUS }, Credentials());

         Assert.IsNotNull(mech);
         Assert.AreEqual(PlainMechanism.PLAIN, mech.Name);
      }

      [Test]
      public void TestSelectCramMD5()
      {
         SaslMechanismSelector selector = new SaslMechanismSelector();

         IMechanism mech = selector.Select(TEST_MECHANISMS_ARRAY, Credentials(USERNAME, PASSWORD));

         Assert.IsNotNull(mech);
         Assert.AreEqual(CramMD5Mechanism.CRAM_MD5, mech.Name);
      }

      [Test]
      public void TestSelectExternalIfPrincipalAvailable()
      {
         SaslMechanismSelector selector = new SaslMechanismSelector();

         IMechanism mech = selector.Select(TEST_MECHANISMS_ARRAY, Credentials());

         Assert.IsNotNull(mech);
         Assert.AreEqual(ExternalMechanism.EXTERNAL, mech.Name);
      }
   }
}