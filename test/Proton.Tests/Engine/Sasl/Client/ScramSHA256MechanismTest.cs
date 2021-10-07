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
   public class ScramSHA256MechanismTest : AbstractScramSHAMechanismTestBase
   {
      private static readonly string TEST_USERNAME = "user";
      private static readonly string TEST_PASSWORD = "pencil";

      private static readonly string CLIENT_NONCE = "rOprNGfwEbeRWgbNEkqO";

      private static readonly IProtonBuffer EXPECTED_CLIENT_INITIAL_RESPONSE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("n,,n=user,r=rOprNGfwEbeRWgbNEkqO"));
      private static readonly IProtonBuffer SERVER_FIRST_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("r=rOprNGfwEbeRWgbNEkqO%hvYDpWUa2RaTCAfuxFIlj)hNlF$k0,s=W22ZaJ0SNY7soEsUEjb6gQ==,i=4096"));
      private static readonly IProtonBuffer EXPECTED_CLIENT_FINAL_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("c=biws,r=rOprNGfwEbeRWgbNEkqO%hvYDpWUa2RaTCAfuxFIlj)hNlF$k0,p=dHzbZapWIk4jUhN+Ute9ytag9zjfMHgsqmmiz7AndVQ="));
      private static readonly IProtonBuffer SERVER_FINAL_MESSAGE = ProtonByteBufferAllocator.Instance.Wrap(
          Encoding.UTF8.GetBytes("v=6rriTRBi23WpRR/wtup+mMhUZUn/dB5nLTJRsjl95G4="));

      public ScramSHA256MechanismTest() : base(EXPECTED_CLIENT_INITIAL_RESPONSE,
                                               SERVER_FIRST_MESSAGE,
                                               EXPECTED_CLIENT_FINAL_MESSAGE,
                                               SERVER_FINAL_MESSAGE)
      {
      }

      protected override IMechanism GetMechanismForTesting()
      {
         return new ScramSHA256Mechanism(CLIENT_NONCE);
      }

      protected override ISaslCredentialsProvider GetTestCredentials()
      {
         return Credentials(TEST_USERNAME, TEST_PASSWORD);
      }

      [Test]
      public void TestGetNameMatchesValueInSaslMechanismsEnum()
      {
         Assert.AreEqual(SaslMechanism.ScramSHA256.ToSymbol(), GetMechanismForTesting().Name);
      }

      [Test]
      public void TestDifferentClientNonceOnEachInstance()
      {
         ScramSHA256Mechanism mech1 = new ScramSHA256Mechanism();
         ScramSHA256Mechanism mech2 = new ScramSHA256Mechanism();

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

         ScramSHA256Mechanism mech = new ScramSHA256Mechanism(CLIENT_NONCE);

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
             Encoding.UTF8.GetBytes("r=rOprNGfwEbeRWgbNEkqOb291012f-b281-47d3-acbc-fefffaad60f2,s=fQwuXmWB4XES7vNK4oBlLtH9cbWAmtxO+Z+tZ9m5W54=,i=4096"));
         IProtonBuffer expectedClientFinalMessage = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes("c=biws,r=rOprNGfwEbeRWgbNEkqOb291012f-b281-47d3-acbc-fefffaad60f2,p=PNeUNfKwyqBPjMssgF7yk4iLt8W24NS/D99HjBbXwyw="));

         IProtonBuffer clientFinalMessage = mechanism.GetChallengeResponse(credentials, serverFirstMessage);

         Assert.AreEqual(expectedClientFinalMessage, clientFinalMessage);

         IProtonBuffer serverFinalMessage = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.UTF8.GetBytes("v=/N9SY26AOvz2QZkJZkyXpomWknaFWSN6zBGqg5RNG9w="));
         IProtonBuffer expectedFinalChallengeResponse = ProtonByteBufferAllocator.Instance.Wrap(Encoding.UTF8.GetBytes(""));

         Assert.AreEqual(expectedFinalChallengeResponse, mechanism.GetChallengeResponse(credentials, serverFinalMessage));

         mechanism.VerifyCompletion();
      }
   }
}