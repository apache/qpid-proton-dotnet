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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Engine.Sasl.Client;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Types.Security;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonSaslClientTest : ProtonEngineTestSupport
   {
      [Test]
      public void TestSaslAnonymousConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectSASLAnonymousConnect();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         engine.SaslDriver.Client().Authenticator = CreateSaslPlainAuthenticator(null, null);

         IConnection connection = engine.Start().Open();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestDriverThrowsIfServerStateRequestedAfterClientStateActivated()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectSASLAnonymousConnect();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         engine.SaslDriver.Client().Authenticator = CreateSaslPlainAuthenticator(null, null);

         Assert.Throws<InvalidOperationException>(() => engine.SaslDriver.Server());

         IConnection connection = engine.Start().Open();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSaslAnonymousConnectionWhenPlainAlsoOfferedButNoCredentialsGiven()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectSASLHeader().RespondWithSASLHeader();
         peer.RemoteSaslMechanisms().WithMechanisms("PLAIN", "ANONYMOUS").Queue();
         peer.ExpectSaslInit().WithMechanism("ANONYMOUS");
         peer.RemoteSaslOutcome().WithCode(Test.Driver.Codec.Security.SaslCode.Ok).Queue();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         engine.SaslDriver.Client().Authenticator = CreateSaslPlainAuthenticator(null, null);

         IConnection connection = engine.Start().Open();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSaslPlainConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Expect a PLAIN connection
         String user = "user";
         String pass = "qwerty123456";

         peer.ExpectSASLPlainConnect(user, pass);
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         engine.SaslDriver.Client().Authenticator = CreateSaslPlainAuthenticator(user, pass);

         IConnection connection = engine.Start().Open();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSaslPlainConnectionWhenUnknownMechanismsOfferedBeforeIt()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Expect a PLAIN connection
         String user = "user";
         String pass = "qwerty123456";

         peer.ExpectSASLHeader().RespondWithSASLHeader();
         peer.RemoteSaslMechanisms().WithMechanisms("UNKNOWN", "PLAIN", "ANONYMOUS").Queue();
         peer.ExpectSaslInit().WithMechanism("PLAIN").WithInitialResponse(peer.SaslPlainInitialResponse(user, pass));
         peer.RemoteSaslOutcome().WithCode(Test.Driver.Codec.Security.SaslCode.Ok).Queue();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();

         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         engine.SaslDriver.Client().Authenticator = CreateSaslPlainAuthenticator(user, pass);

         IConnection connection = engine.Start().Open();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      [Ignore("Test fails with NPE")]
      public void TestDefaultClientSaslMismatchBetweenClientAndServer()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectSASLHeader().RespondWithSASLHeader();
         peer.RemoteSaslMechanisms().WithMechanisms("PLAIN").Queue();

         // Default client only know about ANONYMOUS
         engine.SaslDriver.Client();

         IConnection connection = engine.Start().Open();

         try
         {
            connection.Close();
            Assert.Fail("Engine should have failed");
         }
         catch (EngineFailedException)
         {
            // Expected as engine failed but was not shutdown
         }

         peer.WaitForScriptToComplete();

         Assert.IsTrue(engine.IsFailed);
         Assert.IsNotNull(failure);
      }

      [Test]
      [Ignore("Test failres due to apparent issue in test peer comparison of expectation data")]
      public void TestSaslXOauth2Connection()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Expect a XOAUTH2 connection
         String user = "user";
         String pass = "eyB1c2VyPSJ1c2VyIiB9";

         peer.ExpectSaslXOauth2Connect(user, pass);
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         engine.SaslDriver.Client().Authenticator = CreateSaslPlainAuthenticator(user, pass);

         IConnection connection = engine.Start().Open();

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSaslFailureCodesFailEngine()
      {
         DoSaslFailureCodesTestImpl(SaslCode.Auth);
         DoSaslFailureCodesTestImpl(SaslCode.Sys);
         DoSaslFailureCodesTestImpl(SaslCode.SysPerm);
         DoSaslFailureCodesTestImpl(SaslCode.SysTemp);
      }

      private void DoSaslFailureCodesTestImpl(SaslCode saslFailureCode)
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectSASLHeader().RespondWithSASLHeader();
         peer.RemoteSaslMechanisms().WithMechanisms("PLAIN", "ANONYMOUS").Queue();
         peer.ExpectSaslInit().WithMechanism("PLAIN");
         peer.RemoteSaslOutcome().WithCode(((byte)saslFailureCode)).Queue();

         engine.SaslDriver.Client().Authenticator = CreateSaslPlainAuthenticator("user", "pass");

         engine.Start().Open();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
         Assert.IsFalse(engine.IsShutdown);
         Assert.IsTrue(engine.IsFailed);
         Assert.AreEqual(failure, engine.FailureCause);
         Assert.IsTrue(failure is SaslException);
      }

      private SaslAuthenticator CreateSaslPlainAuthenticator(string user, string password)
      {
         ISaslCredentialsProvider credentials =
            new DelegatedSaslCredentialsProvider().UsernameSupplier(() => user)
                                                  .PasswordSupplier(() => password);

         return new SaslAuthenticator(credentials);
      }
   }
}