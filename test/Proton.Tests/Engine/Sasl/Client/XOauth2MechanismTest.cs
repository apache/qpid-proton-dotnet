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
   public class XOauth2MechanismTest : MechanismTestBase
   {
      [Test]
      public void TestGetInitialResponseWithNullUserAndPassword()
      {
         XOauth2Mechanism mech = new XOauth2Mechanism();

         IProtonBuffer response = mech.GetInitialResponse(Credentials());
         Assert.IsNotNull(response);
         Assert.IsTrue(response.ReadableBytes != 0);
      }

      [Test]
      public void TestGetChallengeResponse()
      {
         XOauth2Mechanism mech = new XOauth2Mechanism();

         IProtonBuffer response = mech.GetChallengeResponse(Credentials(), TEST_BUFFER);
         Assert.IsNotNull(response);
         Assert.IsTrue(response.ReadableBytes == 0);
      }

      [Test]
      public void TestIsNotApplicableWithNoCredentials()
      {
         Assert.IsFalse(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials(null, null, false)),
             "Should not be applicable with no credentials");
      }

      [Test]
      public void TestIsNotApplicableWithNoUser()
      {
         Assert.IsFalse(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials(null, "pass", false)),
             "Should not be applicable with no username");
      }

      [Test]
      public void TestIsNotApplicableWithNoToken()
      {
         Assert.IsFalse(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials("user", null, false)),
             "Should not be applicable with no token");
      }

      [Test]
      public void TestIsNotApplicableWithEmptyUser()
      {
         Assert.IsFalse(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials("", "pass", false)),
             "Should not be applicable with empty username");
      }

      [Test]
      public void TestIsNotApplicableWithEmptyToken()
      {
         Assert.IsFalse(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials("user", "", false)),
             "Should not be applicable with empty token");
      }

      /** RFC6749 defines the OAUTH2 an access token as comprising VSCHAR elements (\x20-7E) */
      [Test]
      public void TestIsNotApplicableWithIllegalAccessToken()
      {
         Assert.IsFalse(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials("user", "illegalChar\000", false)),
             "Should not be applicable with non vschars");
      }

      [Test]
      public void TestIsNotApplicableWithEmtpyUserAndToken()
      {
         Assert.IsFalse(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials("", "", false)),
             "Should not be applicable with empty user and token");
      }

      [Test]
      public void TestIsApplicableWithUserAndToken()
      {
         Assert.IsTrue(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials("user", "2YotnFZFEjr1zCsicMWpAA", false)),
             "Should be applicable with user and token");
      }

      [Test]
      public void TestIsApplicableWithUserAndPasswordAndPrincipal()
      {
         Assert.IsTrue(SaslMechanism.XOAuth2.CreateMechanism().IsApplicable(Credentials("user", "2YotnFZFEjr1zCsicMWpAA", true)),
             "Should be applicable with user and token and principal");
      }
   }
}