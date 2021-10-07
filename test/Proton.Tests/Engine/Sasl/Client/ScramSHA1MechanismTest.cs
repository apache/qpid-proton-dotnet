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
   public class ScramSHA1MechanismTest : AbstractScramSHAMechanismTestBase
   {
      private static readonly string TEST_USERNAME = "user";
      private static readonly string TEST_PASSWORD = "pencil";

      private static readonly string CLIENT_NONCE = "fyko+d2lbbFgONRv9qkxdawL";

      private static readonly IProtonBuffer EXPECTED_CLIENT_INITIAL_RESPONSE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("n,,n=user,r=fyko+d2lbbFgONRv9qkxdawL"));
      private static readonly IProtonBuffer SERVER_FIRST_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("r=fyko+d2lbbFgONRv9qkxdawL3rfcNHYJY1ZVvWVs7j,s=QSXCR+Q6sek8bf92,i=4096"));
      private static readonly IProtonBuffer EXPECTED_CLIENT_FINAL_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("c=biws,r=fyko+d2lbbFgONRv9qkxdawL3rfcNHYJY1ZVvWVs7j,p=v0X8v3Bz2T0CJGbJQyF0X+HI4Ts="));
      private static readonly IProtonBuffer SERVER_FINAL_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("v=rmF9pqV8S7suAoZWja4dJRkFsKQ="));

      public ScramSHA1MechanismTest() : base(EXPECTED_CLIENT_INITIAL_RESPONSE,
                                             SERVER_FIRST_MESSAGE,
                                             EXPECTED_CLIENT_FINAL_MESSAGE,
                                             SERVER_FINAL_MESSAGE)
      {
      }

      protected override IMechanism GetMechanismForTesting()
      {
         return new ScramSHA1Mechanism(CLIENT_NONCE);
      }

      protected override ISaslCredentialsProvider GetTestCredentials()
      {
         return Credentials(TEST_USERNAME, TEST_PASSWORD);
      }

      [Test]
      public void TestGetNameMatchesValueInSaslMechanismsEnum()
      {
         Assert.AreEqual(SaslMechanism.ScramSHA1.ToSymbol(), GetMechanismForTesting().Name);
      }

      [Test]
      public void TestDifferentClientNonceOnEachInstance()
      {
         ScramSHA1Mechanism mech1 = new ScramSHA1Mechanism();
         ScramSHA1Mechanism mech2 = new ScramSHA1Mechanism();

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

         ScramSHA1Mechanism mech = new ScramSHA1Mechanism(CLIENT_NONCE);

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
             Encoding.UTF8.GetBytes("r=fyko+d2lbbFgONRv9qkxdawLdcbfa301-1618-46ee-96c1-2bf60139dc7f,s=Q0zM1qzKMOmI0sAzE7dXt6ru4ZIXhAzn40g4mQXKQdw=,i=4096"));
         IProtonBuffer expectedClientFinalMessage = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes("c=biws,r=fyko+d2lbbFgONRv9qkxdawLdcbfa301-1618-46ee-96c1-2bf60139dc7f,p=quRNWvZqGUvPXoazebZe0ZYsjQI="));

         IProtonBuffer clientFinalMessage = mechanism.GetChallengeResponse(credentials, serverFirstMessage);

         Assert.AreEqual(expectedClientFinalMessage.ToString(Encoding.UTF8), clientFinalMessage.ToString(Encoding.UTF8));
         Assert.AreEqual(expectedClientFinalMessage, clientFinalMessage);

         IProtonBuffer serverFinalMessage = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes("v=dnJDHm3fp6WwVrl5yjZuqKp03lQ="));
         IProtonBuffer expectedFinalChallengeResponse = ProtonByteBufferAllocator.Instance.Wrap(Encoding.UTF8.GetBytes(""));

         Assert.AreEqual(expectedFinalChallengeResponse, mechanism.GetChallengeResponse(credentials, serverFinalMessage));

         mechanism.VerifyCompletion();
      }
   }
}