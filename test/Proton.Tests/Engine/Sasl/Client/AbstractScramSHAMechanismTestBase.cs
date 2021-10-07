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
using System.Text;
using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   public abstract class AbstractScramSHAMechanismTestBase : MechanismTestBase
   {
      private readonly IProtonBuffer expectedClientInitialResponse;
      private readonly IProtonBuffer serverFirstMessage;
      private readonly IProtonBuffer expectedClientFinalMessage;
      private readonly IProtonBuffer serverFinalMessage;

      public AbstractScramSHAMechanismTestBase(IProtonBuffer expectedClientInitialResponse,
                                               IProtonBuffer serverFirstMessage,
                                               IProtonBuffer expectedClientFinalMessage,
                                               IProtonBuffer serverFinalMessage)
      {

         this.expectedClientInitialResponse = expectedClientInitialResponse;
         this.serverFirstMessage = serverFirstMessage;
         this.expectedClientFinalMessage = expectedClientFinalMessage;
         this.serverFinalMessage = serverFinalMessage;
      }

      protected abstract IMechanism GetMechanismForTesting();

      protected abstract ISaslCredentialsProvider GetTestCredentials();

      [Test]
      public void TestSuccessfulAuthentication()
      {
         IMechanism mechanism = GetMechanismForTesting();

         IProtonBuffer clientInitialResponse = mechanism.GetInitialResponse(GetTestCredentials());
         Assert.AreEqual(expectedClientInitialResponse, clientInitialResponse);

         IProtonBuffer clientFinalMessage = mechanism.GetChallengeResponse(GetTestCredentials(), serverFirstMessage);
         Assert.AreEqual(expectedClientFinalMessage, clientFinalMessage);

         IProtonBuffer expectedFinalChallengeResponse = ProtonByteBufferAllocator.Instance.Wrap(Encoding.ASCII.GetBytes(""));
         Assert.AreEqual(expectedFinalChallengeResponse, mechanism.GetChallengeResponse(GetTestCredentials(), serverFinalMessage));

         mechanism.VerifyCompletion();
      }

      [Test]
      public void TestServerFirstMessageMalformed()
      {
         IMechanism mechanism = GetMechanismForTesting();

         mechanism.GetInitialResponse(GetTestCredentials());

         IProtonBuffer challenge = ProtonByteBufferAllocator.Instance.Wrap(
            Encoding.ASCII.GetBytes("badserverfirst"));

         try
         {
            mechanism.GetChallengeResponse(GetTestCredentials(), challenge);
            Assert.Fail("Exception not thrown");
         }
         catch (SaslException)
         {
            // PASS
         }
      }

      /**
       * 5.1.  SCRAM Attributes
       * "m: This attribute is reserved for future extensibility.  In this
       * version of SCRAM, its presence in a client or a server message
       * MUST cause authentication failure when the attribute is parsed by
       * the other end."
       *
       * @ if an unexpected exception is thrown.
       */
      [Test]
      public void TestServerFirstMessageMandatoryExtensionRejected()
      {
         IMechanism mechanism = GetMechanismForTesting();

         mechanism.GetInitialResponse(GetTestCredentials());

         IProtonBuffer challenge = ProtonByteBufferAllocator.Instance.Wrap(
            Encoding.ASCII.GetBytes("m=notsupported,s=,i="));

         try
         {
            mechanism.GetChallengeResponse(GetTestCredentials(), challenge);
            Assert.Fail("Exception not thrown");
         }
         catch (SaslException)
         {
            // PASS
         }
      }

      /**
       * 5.  SCRAM Authentication Exchange
       * "In [the server first] response, the server sends a "server-first-message" containing the
       * user's iteration count i and the user's salt, and appends its own
       * nonce to the client-specified one."
       *
       * @ if an unexpected exception is thrown.
       */
      [Test]
      public void TestServerFirstMessageInvalidNonceRejected()
      {
         IMechanism mechanism = GetMechanismForTesting();

         mechanism.GetInitialResponse(GetTestCredentials());

         IProtonBuffer challenge = ProtonByteBufferAllocator.Instance.Wrap(
             Encoding.ASCII.GetBytes("r=invalidnonce,s=W22ZaJ0SNY7soEsUEjb6gQ==,i=4096"));

         try
         {
            mechanism.GetChallengeResponse(GetTestCredentials(), challenge);
            Assert.Fail("Exception not thrown");
         }
         catch (SaslException)
         {
            // PASS
         }
      }

      /**
       * 5.  SCRAM Authentication Exchange
       * "The client then authenticates the server by computing the
       * ServerSignature and comparing it to the value sent by the server.  If
       * the two are different, the client MUST consider the authentication
       * exchange to be unsuccessful, and it might have to drop the
       * connection."
       *
       * @ if an unexpected exception is thrown.
       */
      [Test]
      public void TestServerSignatureDiffer()
      {
         IMechanism mechanism = GetMechanismForTesting();

         mechanism.GetInitialResponse(GetTestCredentials());
         mechanism.GetChallengeResponse(GetTestCredentials(), serverFirstMessage);

         IProtonBuffer challenge = ProtonByteBufferAllocator.Instance.Wrap(
            Encoding.ASCII.GetBytes("v=" + Convert.ToBase64String(Encoding.ASCII.GetBytes("badserver"))));

         try
         {
            mechanism.GetChallengeResponse(GetTestCredentials(), challenge);
            Assert.Fail("Exception not thrown");
         }
         catch (SaslException)
         {
            // PASS
         }
      }

      [Test]
      public void TestIncompleteExchange()
      {
         IMechanism mechanism = GetMechanismForTesting();

         IProtonBuffer clientInitialResponse = mechanism.GetInitialResponse(GetTestCredentials());
         Assert.AreEqual(expectedClientInitialResponse, clientInitialResponse);

         IProtonBuffer clientFinalMessage = mechanism.GetChallengeResponse(GetTestCredentials(), serverFirstMessage);
         Assert.AreEqual(expectedClientFinalMessage, clientFinalMessage);

         try
         {
            mechanism.VerifyCompletion();
            Assert.Fail("Exception not thrown");
         }
         catch (SaslException)
         {
            // PASS
         }
      }
   }
}