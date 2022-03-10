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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Security;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonSaslServerTest : ProtonEngineTestSupport
   {
      [Test]
      public void TestEngineFailsIfAMQPHeaderArrivesWhenSASLHeaderExpected()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler(result => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Setup basic SASL server which only allows ANONYMOUS
         engine.SaslDriver.Server().Authenticator = CreateAnonymousSaslServerListener();
         engine.Connection.OpenHandler((conn) => conn.Open());
         engine.Connection.CloseHandler((conn) => conn.Close());
         engine.Start();

         peer.ExpectSASLHeader();

         try
         {
            peer.RemoteHeader(AMQPHeader.Header.ToArray()).Now();
         }
         catch (Exception pve)
         {
            Assert.IsTrue(pve.InnerException is ProtocolViolationException);
         }

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestSaslAnonymousConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler(result => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Setup basic SASL server which only allows ANONYMOUS
         engine.SaslDriver.Server().Authenticator = CreateAnonymousSaslServerListener();
         engine.Connection.OpenHandler((conn) => conn.Open());
         engine.Connection.CloseHandler((conn) => conn.Close());
         engine.Start();

         peer.ExpectSASLHeader();
         peer.ExpectSaslMechanisms().WithSaslServerMechanisms("ANONYMOUS");
         peer.RemoteHeader(AMQPHeader.SASLHeader.ToArray()).Now();
         peer.WaitForScriptToComplete();

         peer.ExpectSaslOutcome().WithCode(((byte)SaslCode.Ok));
         peer.RemoteSaslInit().WithMechanism("ANONYMOUS").Now();
         peer.WaitForScriptToComplete();

         peer.ExpectAMQPHeader();
         peer.ExpectOpen();
         peer.RemoteHeader(AMQPHeader.Header.ToArray()).Now();
         peer.RemoteOpen().Now();
         peer.WaitForScriptToComplete();

         peer.ExpectClose();
         peer.RemoteClose().Now();
         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSaslPlainConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler(result => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Setup basic SASL server which only allows ANONYMOUS
         engine.SaslDriver.Server().Authenticator = CreatePlainSaslServerListener();
         engine.Connection.OpenHandler((conn) => conn.Open());
         engine.Connection.CloseHandler((conn) => conn.Close());
         engine.Start();

         peer.ExpectSASLHeader();
         peer.ExpectSaslMechanisms().WithSaslServerMechanisms("PLAIN");
         peer.RemoteHeader(AMQPHeader.SASLHeader.ToArray()).Now();
         peer.WaitForScriptToComplete();

         peer.ExpectSaslOutcome().WithCode(((byte)SaslCode.Ok));
         peer.RemoteSaslInit().WithMechanism("PLAIN")
                              .WithInitialResponse(SaslPlainInitialResponse("user", "pass")).Now();
         peer.WaitForScriptToComplete();

         peer.ExpectAMQPHeader();
         peer.ExpectOpen();
         peer.RemoteHeader(AMQPHeader.Header.ToArray()).Now();
         peer.RemoteOpen().Now();
         peer.WaitForScriptToComplete();

         peer.ExpectClose();
         peer.RemoteClose().Now();
         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestSaslPlainConnectionFailedWhenAnonymousOffered()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler(result => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Setup basic SASL server which only allows ANONYMOUS
         engine.SaslDriver.Server().Authenticator = CreatePlainSaslServerListener();
         engine.Connection.OpenHandler((conn) => conn.Open());
         engine.Connection.CloseHandler((conn) => conn.Close());
         engine.Start();

         peer.ExpectSASLHeader();
         peer.ExpectSaslMechanisms().WithSaslServerMechanisms("PLAIN");
         peer.RemoteHeader(AMQPHeader.SASLHeader.ToArray()).Now();
         peer.WaitForScriptToComplete();

         peer.ExpectSaslOutcome().WithCode(((byte)SaslCode.SysPerm));
         peer.RemoteSaslInit().WithMechanism("ANONYMOUS").Now();
         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEngineFailsForUnexpectedNonSaslFrameDuringSaslExchange()
      {
         IEngine engine = IEngineFactory.Proton.CreateEngine();
         engine.ErrorHandler(result => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         // Setup basic SASL server which only allows ANONYMOUS
         engine.SaslDriver.Server().Authenticator = CreatePlainSaslServerListener();
         engine.Connection.OpenHandler((conn) => conn.Open());
         engine.Connection.CloseHandler((conn) => conn.Close());
         engine.Start();

         peer.ExpectSASLHeader();
         peer.ExpectSaslMechanisms().WithSaslServerMechanisms("PLAIN");
         peer.RemoteHeader(AMQPHeader.SASLHeader.ToArray()).Now();
         peer.WaitForScriptToComplete();

         try
         {
            peer.RemoteOpen().Now();
         }
         catch (Exception pve)
         {
            Assert.IsTrue(pve.InnerException is ProtocolViolationException);
         }

         peer.WaitForScriptToCompleteIgnoreErrors();

         Assert.IsNotNull(failure);
      }

      public byte[] SaslPlainInitialResponse(string username, string password)
      {
         byte[] usernameBytes = Encoding.UTF8.GetBytes(username);
         byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
         byte[] initialResponse = new byte[usernameBytes.Length + passwordBytes.Length + 2];

         Array.Copy(usernameBytes, 0, initialResponse, 1, usernameBytes.Length);
         Array.Copy(passwordBytes, 0, initialResponse, 2 + usernameBytes.Length, passwordBytes.Length);

         return initialResponse;
      }

      private ISaslServerAuthenticator CreateAnonymousSaslServerListener()
      {
         return new SASLServerAnonymousAuthenticator();
      }

      private ISaslServerAuthenticator CreatePlainSaslServerListener()
      {
         return new SASLServerPlainAuthenticator();
      }

      private sealed class SASLServerAnonymousAuthenticator : ISaslServerAuthenticator
      {
         public void HandleSaslHeader(ISaslServerContext context, AmqpHeader header)
         {
            context.SendMechanisms(new Symbol[] { Symbol.Lookup("ANONYMOUS") });
         }

         public void HandleSaslInit(ISaslServerContext context, Symbol mechanism, IProtonBuffer initResponse)
         {
            if (mechanism.Equals(Symbol.Lookup("ANONYMOUS")))
            {
               context.SendOutcome(SaslAuthOutcome.SaslOk, null);
            }
            else
            {
               context.SendOutcome(SaslAuthOutcome.SaslPermError, null);
            }
         }

         public void HandleSaslResponse(ISaslServerContext context, IProtonBuffer response)
         {
            throw new NotSupportedException("Not expected that a response is sent for SASL Anonymous");
         }
      }

      private sealed class SASLServerPlainAuthenticator : ISaslServerAuthenticator
      {
         public void HandleSaslHeader(ISaslServerContext context, AmqpHeader header)
         {
            context.SendMechanisms(new Symbol[] { Symbol.Lookup("PLAIN") });
         }

         public void HandleSaslInit(ISaslServerContext context, Symbol mechanism, IProtonBuffer initResponse)
         {
            if (mechanism.Equals(Symbol.Lookup("PLAIN")))
            {
               context.SendOutcome(SaslAuthOutcome.SaslOk, null);
            }
            else
            {
               context.SendOutcome(SaslAuthOutcome.SaslPermError, null);
            }
         }

         public void HandleSaslResponse(ISaslServerContext context, IProtonBuffer response)
         {
            throw new NotSupportedException("Not expected that a response is sent for SASL Plain");
         }
      }
   }
}