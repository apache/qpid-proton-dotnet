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

using System.Text;
using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   [TestFixture, Timeout(20000)]
   public class ScramSHA512MechanismTest : AbstractScramSHAMechanismTestBase
   {
      private static readonly string TEST_USERNAME = "user";
      private static readonly string TEST_PASSWORD = "pencil";

      private static readonly string CLIENT_NONCE = "rOprNGfwEbeRWgbNEkqO";

      private static readonly IProtonBuffer EXPECTED_CLIENT_INITIAL_RESPONSE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("n,,n=user,r=rOprNGfwEbeRWgbNEkqO"));
      private static readonly IProtonBuffer SERVER_FIRST_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("r=rOprNGfwEbeRWgbNEkqO02431b08-2f89-4bad-a4e6-80c0564ec865,s=Yin2FuHTt/M0kJWb0t9OI32n2VmOGi3m+JfjOvuDF88=,i=4096"));
      private static readonly IProtonBuffer EXPECTED_CLIENT_FINAL_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("c=biws,r=rOprNGfwEbeRWgbNEkqO02431b08-2f89-4bad-a4e6-80c0564ec865,p=Hc5yec3NmCD7t+kFRw4/3yD6/F3SQHc7AVYschRja+Bc3sbdjlA0eH1OjJc0DD4ghn1tnXN5/Wr6qm9xmaHt4A=="));
      private static readonly IProtonBuffer SERVER_FINAL_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("v=BQuhnKHqYDwQWS5jAw4sZed+C9KFUALsbrq81bB0mh+bcUUbbMPNNmBIupnS2AmyyDnG5CTBQtkjJ9kyY4kzmw=="));

      public ScramSHA512MechanismTest() : base(EXPECTED_CLIENT_INITIAL_RESPONSE,
                                               SERVER_FIRST_MESSAGE,
                                               EXPECTED_CLIENT_FINAL_MESSAGE,
                                               SERVER_FINAL_MESSAGE)
      {
      }

      protected override IMechanism GetMechanismForTesting()
      {
         return new ScramSHA512Mechanism(CLIENT_NONCE);
      }

      protected override ISaslCredentialsProvider GetTestCredentials()
      {
         return Credentials(TEST_USERNAME, TEST_PASSWORD);
      }

      [Test]
      public void TestGetNameMatchesValueInSaslMechanismsEnum()
      {
         Assert.AreEqual(SaslMechanism.ScramSHA512.ToSymbol(), GetMechanismForTesting().Name);
      }

      [Test]
      public void TestDifferentClientNonceOnEachInstance()
      {
         ScramSHA512Mechanism mech1 = new ScramSHA512Mechanism();
         ScramSHA512Mechanism mech2 = new ScramSHA512Mechanism();

         IProtonBuffer clientInitialResponse1 = mech1.GetInitialResponse(GetTestCredentials());
         IProtonBuffer clientInitialResponse2 = mech2.GetInitialResponse(GetTestCredentials());

         Assert.IsTrue(clientInitialResponse1.ToString(Encoding.UTF8).StartsWith("n,,n=user,r="));
         Assert.IsTrue(clientInitialResponse2.ToString(Encoding.UTF8).StartsWith("n,,n=user,r="));

         Assert.AreNotEqual(clientInitialResponse1, clientInitialResponse2);
      }

      [Test]
      public void TestUsernameCommaEqualsCharactersEscaped()
      {
         string originalUsername = "user,name=";
         string escapedUsername = "user=2Cname=3D";

         string expectedInitialResponseString = "n,,n=" + escapedUsername + ",r=" + CLIENT_NONCE;
         IProtonBuffer expectedInitialResponseBuffer = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes(expectedInitialResponseString));

         ScramSHA512Mechanism mech = new ScramSHA512Mechanism(CLIENT_NONCE);

         IProtonBuffer clientInitialResponse = mech.GetInitialResponse(Credentials(originalUsername, "password"));
         Assert.AreEqual(expectedInitialResponseBuffer, clientInitialResponse);
      }

      [Test]
      public void TestPasswordCommaEqualsCharactersNotEscaped()
      {
         IMechanism mechanism = GetMechanismForTesting();
         ISaslCredentialsProvider credentials = Credentials(TEST_USERNAME, TEST_PASSWORD + ",=");

         IProtonBuffer clientInitialResponse = mechanism.GetInitialResponse(credentials);
         Assert.AreEqual(EXPECTED_CLIENT_INITIAL_RESPONSE, clientInitialResponse);

         IProtonBuffer serverFirstMessage = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes("r=rOprNGfwEbeRWgbNEkqOf0f492bc-13cc-4050-8461-59f74f24e989,s=g2nOdJkyb5SlvqLbJb6S5+ckZpYFJ+AkJqxlmDAZYbY=,i=4096"));
         IProtonBuffer expectedClientFinalMessage = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes("c=biws,r=rOprNGfwEbeRWgbNEkqOf0f492bc-13cc-4050-8461-59f74f24e989,p=vxWDY/qwIhNPGnYvGKxRESmP9nP4bmOSssNLVN6sWo1cAatr3HAxIogJ9qe2kxLdrmQcyCkW7sgq+8ybSgPphQ=="));

         IProtonBuffer clientFinalMessage = mechanism.GetChallengeResponse(credentials, serverFirstMessage);

         Assert.AreEqual(expectedClientFinalMessage, clientFinalMessage);

         IProtonBuffer serverFinalMessage = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes("v=l/icAMt3q4ym4Yh7syjjekFZ3r3L3+l+e08WmS3m3pMXCXhPf865+9bfRRprO6xPhFWKyuD+PPh+jQf8JBVojQ=="));
         IProtonBuffer expectedFinalChallengeResponse = ProtonByteBufferAllocator.Instance.Wrap(Encoding.UTF8.GetBytes(""));

         Assert.AreEqual(expectedFinalChallengeResponse, mechanism.GetChallengeResponse(credentials, serverFinalMessage));

         mechanism.VerifyCompletion();
      }
   }
}