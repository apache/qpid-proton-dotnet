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

using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   [TestFixture, Timeout(20000)]
   public class AnonymousMechanismTest : MechanismTestBase
   {
      [Test]
      public void TestGetInitialResponseWithNullUserAndPassword()
      {
         AnonymousMechanism mech = new AnonymousMechanism();

         IProtonBuffer response = mech.GetInitialResponse(Credentials());
         Assert.IsNotNull(response);
         Assert.IsTrue(response.ReadableBytes == 0);
      }

      [Test]
      public void TestGetChallengeResponse()
      {
         AnonymousMechanism mech = new AnonymousMechanism();

         IProtonBuffer response = mech.GetChallengeResponse(Credentials(), TEST_BUFFER);
         Assert.IsNotNull(response);
         Assert.IsTrue(response.ReadableBytes == 0);
      }

      [Test]
      public void TestIsApplicableWithNoCredentials()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials(null, null, false)),
             "Should be applicable with no credentials");
      }

      [Test]
      public void TestIsNotApplicableWithNoUser()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials(null, "pass", false)),
             "Should be applicable with no username");
      }

      [Test]
      public void TestIsApplicableWithNoPassword()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials("user", null, false)),
             "Should be applicable with no password");
      }

      [Test]
      public void TestIsApplicableWithEmptyUser()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials("", "pass", false)),
             "Should be applicable with empty username");
      }

      [Test]
      public void TestIsApplicableWithEmptyPassword()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials("user", "", false)),
             "Should be applicable with empty password");
      }

      [Test]
      public void TestIsApplicableWithEmptyUserAndPassword()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials("", "", false)),
             "Should be applicable with empty user and password");
      }

      [Test]
      public void TestIsApplicableWithUserAndPassword()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials("user", "password", false)),
             "Should be applicable with user and password");
      }

      [Test]
      public void TestIsApplicableWithUserAndPasswordAndPrincipal()
      {
         Assert.IsTrue(SaslMechanism.Anonymous.CreateMechanism().IsApplicable(Credentials("user", "password", true)),
             "Should be applicable with user and password and principal");
      }
   }
}