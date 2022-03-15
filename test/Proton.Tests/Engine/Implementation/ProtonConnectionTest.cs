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
using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;
using Is = Apache.Qpid.Proton.Test.Driver.Matchers.Is;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonConnectionTest : ProtonEngineTestSupport
   {
      [Test]
      public void TestNegotiateSendsAMQPHeader()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader();

         IConnection connection = engine.Start();

         connection.Negotiate();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestNegotiateSendsAMQPHeaderAndFireRemoteHeaderEvent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();

         IConnection connection = engine.Start();

         connection.Negotiate();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestNegotiateSendsAMQPHeaderEnforcesNotNullEventHandler()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Start();

         try
         {
            connection.Negotiate(null);
            Assert.Fail("Should not allow null event handler");
         }
         catch (ArgumentNullException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestNegotiateDoesNotSendAMQPHeaderAfterOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         uint headerReceivedCallback = 0;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.Open();
         connection.Negotiate((header) => headerReceivedCallback++);
         Assert.AreEqual(1, headerReceivedCallback);
         connection.Negotiate((header) => headerReceivedCallback++);
         Assert.AreEqual(2, headerReceivedCallback);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionSyncStateAfterEngineStarted()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Connection;
         ISession session = connection.Session().Open();
         ISender sender = session.Sender("test").Open();

         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().OfSender().Respond();
         peer.ExpectDetach().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose().Respond();

         engine.Start();

         sender.Close();
         session.Close();
         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionEmitsOpenAndCloseEvents()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool connectionLocalOpen = false;
         bool connectionLocalClose = false;
         bool connectionRemoteOpen = false;
         bool connectionRemoteClose = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.LocalOpenHandler((result) => connectionLocalOpen = true)
                   .LocalCloseHandler((result) => connectionLocalClose = true)
                   .OpenHandler((result) => connectionRemoteOpen = true)
                   .CloseHandler((result) => connectionRemoteClose = true);

         connection.Open();
         connection.Close();

         Assert.IsTrue(connectionLocalOpen, "Connection should have reported local open");
         Assert.IsTrue(connectionLocalClose, "Connection should have reported local close");
         Assert.IsTrue(connectionRemoteOpen, "Connection should have reported remote open");
         Assert.IsTrue(connectionRemoteClose, "Connection should have reported remote close");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionPopulatesRemoteData()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         Symbol[] offeredCapabilities = new Symbol[] { Symbol.Lookup("one"), Symbol.Lookup("two") };
         Symbol[] desiredCapabilities = new Symbol[] { Symbol.Lookup("three"), Symbol.Lookup("four") };

         IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
         expectedProperties.Add("test", "value");

         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), "value");

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("test")
                                    .WithHostname("localhost")
                                    .WithIdleTimeOut(60000)
                                    .WithOfferedCapabilities(new String[] { "one", "two" })
                                    .WithDesiredCapabilities(new String[] { "three", "four" })
                                    .WithProperties(expectedProperties);
         peer.ExpectClose();

         IConnection connection = engine.Start().Open();

         Assert.AreEqual("test", connection.RemoteContainerId);
         Assert.AreEqual("localhost", connection.RemoteHostname);
         Assert.AreEqual(60000, connection.RemoteIdleTimeout);

         Assert.AreEqual(offeredCapabilities, connection.RemoteOfferedCapabilities);
         Assert.AreEqual(desiredCapabilities, connection.RemoteDesiredCapabilities);
         Assert.AreEqual(properties, connection.RemoteProperties);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenAndCloseConnectionWithNullSetsOnConnectionOptions()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose();

         IConnection connection = engine.Start();

         connection.Properties = null;
         connection.OfferedCapabilities = (Symbol[])null;
         connection.DesiredCapabilities = (Symbol[])null;
         connection.ErrorCondition = null;
         connection.Open();

         Assert.IsNotNull(connection.Attachments);
         Assert.IsNull(connection.Properties);
         Assert.IsNull(connection.OfferedCapabilities);
         Assert.IsNull(connection.DesiredCapabilities);
         Assert.IsNull(connection.ErrorCondition);

         Assert.IsNull(connection.RemoteProperties);
         Assert.IsNull(connection.RemoteOfferedCapabilities);
         Assert.IsNull(connection.RemoteDesiredCapabilities);
         Assert.IsNull(connection.RemoteErrorCondition);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEndpointEmitsEngineShutdownEvent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool engineShutdown = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.EngineShutdownHandler((result) => engineShutdown = true);

         connection.Open();
         connection.Close();

         engine.Shutdown();

         Assert.IsTrue(engineShutdown, "Connection should have reported engine shutdown");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionOpenAndCloseAreIdempotent()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         // Default engine should Start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.Open();

         // Should not emit another open frame
         connection.Open();

         connection.Close();

         // Should not emit another close frame
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionRemoteOpenTriggeredWhenRemoteOpenArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");

         bool RemoteOpened = false;

         IConnection connection = engine.Start();

         // Default engine should Start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.OpenHandler((result) => RemoteOpened = true);
         connection.Open();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(RemoteOpened);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionRemoteOpenTriggeredWhenRemoteOpenArrivesBeforeLocalOpen()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool RemoteOpened = false;

         IConnection connection = engine.Start();

         // Default engine should Start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.OpenHandler((result) => RemoteOpened = true);

         peer.ExpectAMQPHeader();

         // Remote Header will prompt local response and then remote open should trigger
         // the connection handler to fire so that user knows remote opened.
         peer.RemoteHeader(AmqpHeader.GetAMQPHeader().ToArray()).Now();
         peer.RemoteOpen().Now();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(RemoteOpened);
         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionRemoteCloseTriggeredWhenRemoteCloseArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectClose().Respond();

         bool connectionOpenedSignaled = false;
         bool connectionClosedSignaled = false;

         IConnection connection = engine.Start();

         // Default engine should Start and return a connection immediately
         Assert.IsNotNull(connection);

         connection.OpenHandler((result) => connectionOpenedSignaled = true);
         connection.CloseHandler((result) => connectionClosedSignaled = true);

         connection.Open();

         Assert.AreEqual(ConnectionState.Active, connection.ConnectionState);
         Assert.AreEqual(ConnectionState.Active, connection.RemoteConnectionState);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsTrue(connectionOpenedSignaled);
         Assert.IsTrue(connectionClosedSignaled);

         Assert.AreEqual(ConnectionState.Closed, connection.ConnectionState);
         Assert.AreEqual(ConnectionState.Closed, connection.RemoteConnectionState);

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionOpenCarriesAllSetValues()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(true, true, true, true, true);
      }

      [Test]
      public void TestConnectionOpenCarriesDefaultMaxFrameSize()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, false, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesSetMaxFrameSize()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(true, false, false, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesDefaultContainerId()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, false, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesSetContainerId()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, true, false, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesDefaultChannelMax()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, false, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesSetChannelMax()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, true, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesNoHostname()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, false, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesSetHostname()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, false, true, false);
      }

      [Test]
      public void TestConnectionOpenCarriesNoIdleTimeout()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, false, false, false);
      }

      [Test]
      public void TestConnectionOpenCarriesSetIdleTimeout()
      {
         DoTestConnectionOpenPopulatesOpenCorrectly(false, false, false, false, true);
      }

      private void DoTestConnectionOpenPopulatesOpenCorrectly(bool setMaxFrameSize, bool setContainerId, bool setChannelMax,
                                                              bool setHostname, bool setIdleTimeout)
      {
         uint expectedMaxFrameSize = 32767;
         IMatcher expectedMaxFrameSizeMatcher;
         if (setMaxFrameSize)
         {
            expectedMaxFrameSizeMatcher = Is.EqualTo(expectedMaxFrameSize);
         }
         else
         {
            expectedMaxFrameSizeMatcher = Is.EqualTo(ProtonConstants.DefaultMaxAmqpFrameSize);
         }

         String expectedContainerId = "";
         if (setContainerId)
         {
            expectedContainerId = "test";
         }

         ushort expectedChannelMax = 512;
         IMatcher expectedChannelMaxMatcher;
         if (setChannelMax)
         {
            expectedChannelMaxMatcher = Is.EqualTo(expectedChannelMax);
         }
         else
         {
            expectedChannelMaxMatcher = Is.NullValue();
         }

         String expectedHostname = null;
         if (setHostname)
         {
            expectedHostname = "localhost";
         }
         uint expectedIdleTimeout = 60000;
         IMatcher expectedIdleTimeoutMatcher;
         if (setIdleTimeout)
         {
            expectedIdleTimeoutMatcher = Is.EqualTo(expectedIdleTimeout);
         }
         else
         {
            expectedIdleTimeoutMatcher = Is.NullValue();
         }

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithMaxFrameSize(expectedMaxFrameSizeMatcher)
                          .WithChannelMax(expectedChannelMaxMatcher)
                          .WithContainerId(expectedContainerId)
                          .WithHostname(expectedHostname)
                          .WithIdleTimeOut(expectedIdleTimeoutMatcher)
                          .WithIncomingLocales(Is.NullValue())
                          .WithOutgoingLocales(Is.NullValue())
                          .WithDesiredCapabilities(Is.NullValue())
                          .WithOfferedCapabilities(Is.NullValue())
                          .WithProperties(Is.NullValue())
                          .Respond().WithContainerId("driver");
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         if (setMaxFrameSize)
         {
            connection.MaxFrameSize = expectedMaxFrameSize;
         }
         if (setContainerId)
         {
            connection.ContainerId = expectedContainerId;
         }
         if (setChannelMax)
         {
            connection.ChannelMax = expectedChannelMax;
         }
         if (setHostname)
         {
            connection.Hostname = expectedHostname;
         }
         if (setIdleTimeout)
         {
            connection.IdleTimeout = expectedIdleTimeout;
         }

         Assert.AreEqual(expectedContainerId, connection.ContainerId);
         Assert.AreEqual(expectedHostname, connection.Hostname);

         connection.Open();
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCapabilitiesArePopulatedAndAccessible()
      {
         Symbol clientOfferedSymbol = Symbol.Lookup("clientOfferedCapability");
         Symbol clientDesiredSymbol = Symbol.Lookup("clientDesiredCapability");
         Symbol serverOfferedSymbol = Symbol.Lookup("serverOfferedCapability");
         Symbol serverDesiredSymbol = Symbol.Lookup("serverDesiredCapability");

         Symbol[] clientOfferedCapabilities = new Symbol[] { clientOfferedSymbol };
         Symbol[] clientDesiredCapabilities = new Symbol[] { clientDesiredSymbol };
         Symbol[] clientExpectedOfferedCapabilities = new Symbol[] { serverOfferedSymbol };
         Symbol[] clientExpectedDesiredCapabilities = new Symbol[] { serverDesiredSymbol };

         bool remotelyOpened = false;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithOfferedCapabilities(new String[] { clientOfferedSymbol.ToString() })
                          .WithDesiredCapabilities(new String[] { clientDesiredSymbol.ToString() })
                          .Respond()
                          .WithDesiredCapabilities(new String[] { serverDesiredSymbol.ToString() })
                          .WithOfferedCapabilities(new String[] { serverOfferedSymbol.ToString() });
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.DesiredCapabilities = clientDesiredCapabilities;
         connection.OfferedCapabilities = clientOfferedCapabilities;
         connection.OpenHandler((result) => remotelyOpened = true);
         connection.Open();

         Assert.IsTrue(remotelyOpened, "Connection remote opened event did not fire");

         Assert.AreEqual(clientOfferedCapabilities, connection.OfferedCapabilities);
         Assert.AreEqual(clientDesiredCapabilities, connection.DesiredCapabilities);
         Assert.AreEqual(clientExpectedOfferedCapabilities, connection.RemoteOfferedCapabilities);
         Assert.AreEqual(clientExpectedDesiredCapabilities, connection.RemoteDesiredCapabilities);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestPropertiesArePopulatedAndAccessible()
      {
         Symbol clientPropertyName = Symbol.Lookup("ClientPropertyName");
         int clientPropertyValue = 1234;
         Symbol serverPropertyName = Symbol.Lookup("ServerPropertyName");
         int serverPropertyValue = 5678;

         IDictionary<string, object> clientPropertiesMap = new Dictionary<string, object>();
         clientPropertiesMap.Add(clientPropertyName.ToString(), clientPropertyValue);

         IDictionary<string, object> serverPropertiesMap = new Dictionary<string, object>();
         serverPropertiesMap.Add(serverPropertyName.ToString(), serverPropertyValue);

         bool remotelyOpened = false;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithProperties(clientPropertiesMap)
                          .Respond()
                          .WithProperties(serverPropertiesMap);
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         Dictionary<Symbol, object> clientProperties = new Dictionary<Symbol, object>();
         clientProperties.Add(clientPropertyName, clientPropertyValue);

         connection.Properties = clientProperties;
         connection.OpenHandler((result) => remotelyOpened = true);
         connection.Open();

         Assert.IsTrue(remotelyOpened, "Connection remote opened event did not fire");

         Assert.IsNotNull(connection.Properties);
         Assert.IsNotNull(connection.RemoteProperties);

         Assert.AreEqual(clientPropertyValue, connection.Properties[clientPropertyName]);
         Assert.AreEqual(serverPropertyValue, connection.RemoteProperties[serverPropertyName]);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenedCarriesRemoteErrorCondition()
      {
         IDictionary<string, object> errorInfoExpectation = new Dictionary<string, object>();
         errorInfoExpectation.Add("error", "value");
         errorInfoExpectation.Add("error-list", new List<string>(new String[] { "entry-1", "entry-2", "entry-3" }));

         Dictionary<Symbol, object> errorInfo = new Dictionary<Symbol, object>();
         errorInfo.Add(Symbol.Lookup("error"), "value");
         errorInfo.Add(Symbol.Lookup("error-list"), new List<string>(new string[] { "entry-1", "entry-2", "entry-3" }));
         ErrorCondition remoteCondition = new ErrorCondition(Symbol.Lookup("myerror"), "mydescription", errorInfo);

         bool remotelyOpened = false;
         bool remotelyClosed = false;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.RemoteClose().WithErrorCondition("myerror", "mydescription", errorInfoExpectation).Queue();
         peer.ExpectClose();

         IConnection connection = engine.Start();

         connection.OpenHandler((result) => remotelyOpened = true);
         connection.CloseHandler((result) => remotelyClosed = true);
         connection.Open();

         Assert.IsTrue(connection.IsLocallyOpen);
         Assert.IsFalse(connection.IsLocallyClosed);
         Assert.IsFalse(connection.IsRemotelyOpen);
         Assert.IsTrue(connection.IsRemotelyClosed);

         Assert.IsTrue(remotelyOpened, "Connection remote opened event did not fire");
         Assert.IsTrue(remotelyClosed, "Connection remote closed event did not fire");

         Assert.IsNull(connection.ErrorCondition);
         Assert.IsNotNull(connection.RemoteErrorCondition);

         Assert.AreEqual(remoteCondition.Condition, connection.RemoteErrorCondition.Condition);
         Assert.AreEqual(remoteCondition.Description, connection.RemoteErrorCondition.Description);
         Assert.AreEqual(remoteCondition.Info, connection.RemoteErrorCondition.Info);

         connection.Close();

         Assert.IsFalse(connection.IsLocallyOpen);
         Assert.IsTrue(connection.IsLocallyClosed);
         Assert.IsFalse(connection.IsRemotelyOpen);
         Assert.IsTrue(connection.IsRemotelyClosed);

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestEmptyFrameBeforeOpenDoesNotCauseError()
      {
         bool remotelyOpened = false;
         bool remotelyClosed = false;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen();
         peer.RemoteEmptyFrame().Queue();
         peer.RemoteOpen().Queue();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.OpenHandler((result) => remotelyOpened = true);
         connection.CloseHandler((result) => remotelyClosed = true);
         connection.Open();

         Assert.IsTrue(connection.IsLocallyOpen);
         Assert.IsFalse(connection.IsLocallyClosed);
         Assert.IsTrue(connection.IsRemotelyOpen);
         Assert.IsFalse(connection.IsRemotelyClosed);

         Assert.IsTrue(remotelyOpened, "Connection remote opened event did not fire");

         connection.Close();

         Assert.IsTrue(remotelyClosed, "Connection remote closed event did not fire");

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestChannelMaxDefaultsToMax()
      {
         bool remotelyOpened = false;

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithChannelMax(Is.NullValue()).Respond();
         peer.ExpectClose().Respond();

         IConnection connection = engine.Start();

         connection.OpenHandler((result) => remotelyOpened = true);
         connection.Open();

         Assert.IsTrue(remotelyOpened, "Connection remote opened event did not fire");

         Assert.AreEqual(65535, connection.ChannelMax);

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCloseConnectionAfterShutdownDoesNotThrowExceptionOpenWrittenAndResponse()
      {
         TestCloseConnectionAfterShutdownNoOutputAndNoException(true, true);
      }

      [Test]
      public void TestCloseConnectionAfterShutdownDoesNotThrowExceptionOpenWrittenButNoResponse()
      {
         TestCloseConnectionAfterShutdownNoOutputAndNoException(true, false);
      }

      [Test]
      public void TestCloseConnectionAfterShutdownDoesNotThrowExceptionOpenNotWritten()
      {
         TestCloseConnectionAfterShutdownNoOutputAndNoException(false, false);
      }

      private void TestCloseConnectionAfterShutdownNoOutputAndNoException(bool respondToHeader, bool respondToOpen)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         if (respondToHeader)
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            if (respondToOpen)
            {
               peer.ExpectOpen().Respond();
            }
            else
            {
               peer.ExpectOpen();
            }
         }
         else
         {
            peer.ExpectAMQPHeader();
         }

         IConnection connection = engine.Start();
         connection.Open();

         engine.Shutdown();

         // Should clean up and not throw as we knowingly shutdown engine operations.
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCloseConnectionAfterFailureThrowsEngineStateExceptionOpenWrittenAndResponse()
      {
         TestCloseConnectionAfterEngineFailedThrowsAndNoOutputWritten(true, true);
      }

      [Test]
      public void TestCloseConnectionAfterFailureThrowsEngineStateExceptionOpenWrittenButNoResponse()
      {
         TestCloseConnectionAfterEngineFailedThrowsAndNoOutputWritten(true, false);
      }

      [Test]
      public void TestCloseConnectionAfterFailureThrowsEngineStateExceptionOpenNotWritten()
      {
         TestCloseConnectionAfterEngineFailedThrowsAndNoOutputWritten(false, false);
      }

      private void TestCloseConnectionAfterEngineFailedThrowsAndNoOutputWritten(bool respondToHeader, bool respondToOpen)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         if (respondToHeader)
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            if (respondToOpen)
            {
               peer.ExpectOpen().Respond();
               peer.ExpectClose();
            }
            else
            {
               peer.ExpectOpen();
               peer.ExpectClose();
            }
         }
         else
         {
            peer.ExpectAMQPHeader();
         }

         IConnection connection = engine.Start().Open();

         engine.EngineFailed(new IOException());

         try
         {
            connection.Close();
            Assert.Fail("Should throw exception indicating engine is in a failed state.");
         }
         catch (EngineFailedException)
         {
         }

         engine.Shutdown();  // Explicit shutdown now allows local close to complete

         // Should clean up and not throw as we knowingly shutdown engine operations.
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestOpenAndCloseWhileWaitingForHeaderResponseDoesNotWriteUntilHeaderArrives()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader();

         IConnection connection = engine.Start();
         connection.Open();  // Trigger write of AMQP Header, we don't respond here.
         connection.Close();

         peer.WaitForScriptToComplete();

         // Now respond and Connection should open and close
         peer.ExpectOpen();
         peer.ExpectClose();
         peer.RemoteHeader(AmqpHeader.GetAMQPHeader().ToArray()).Now();

         peer.WaitForScriptToComplete();

         engine.Shutdown();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestOpenWhileWaitingForHeaderResponseDoesNotWriteThenWritesFlowAsExpected()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader();

         IConnection connection = engine.Start();
         connection.Open();  // Trigger write of AMQP Header, we don't respond here.

         peer.WaitForScriptToComplete();

         // Now respond and Connection should open and close
         peer.ExpectOpen();
         peer.ExpectClose().WithError(Is.NotNullValue());
         peer.RemoteHeader(AmqpHeader.GetAMQPHeader().ToArray()).Now();

         connection.ErrorCondition = new ErrorCondition(ConnectionError.CONNECTION_FORCED, "something about errors");
         connection.Close();

         peer.WaitForScriptToComplete();

         engine.Shutdown();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCloseOrDetachWithErrorCondition()
      {
         string condition = "amqp:connection:forced";
         string description = "something bad happened.";

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().WithError(condition, description).Respond();

         IConnection connection = engine.Start();

         connection.Open();
         connection.ErrorCondition = new ErrorCondition(Symbol.Lookup(condition), description);
         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotCreateSessionFromLocallyClosedConnection()
      {
         TestCannotCreateSessionFromClosedConnection(true);
      }

      [Test]
      public void TestCannotCreateSessionFromRemotelyClosedConnection()
      {
         TestCannotCreateSessionFromClosedConnection(false);
      }

      private void TestCannotCreateSessionFromClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start();
         connection.Open();
         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.Session();
            Assert.Fail("Should not create new Session from closed Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetContainerIdOnOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start();
         connection.Open();

         try
         {
            connection.ContainerId = "test";
            Assert.Fail("Should not be able to set container ID from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetContainerIdOnLocallyClosedConnection()
      {
         TestCannotSetContainerIdOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetContainerIdOnRemotelyClosedConnection()
      {
         TestCannotSetContainerIdOnClosedConnection(false);
      }

      private void TestCannotSetContainerIdOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start();
         connection.Open();
         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.ContainerId = "test";
            Assert.Fail("Should not be able to set container ID from closed Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetHostnameOnOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start();
         connection.Open();

         try
         {
            connection.Hostname = "test";
            Assert.Fail("Should not be able to set host name from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetHostnameOnLocallyClosedConnection()
      {
         TestCannotSetHostnameOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetHostnameOnRemotelyClosedConnection()
      {
         TestCannotSetHostnameOnClosedConnection(false);
      }

      private void TestCannotSetHostnameOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start();
         connection.Open();
         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.Hostname = "test";
            Assert.Fail("Should not be able to set host name from closed Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetChannelMaxOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start();
         connection.Open();

         try
         {
            connection.ChannelMax = 0;
            Assert.Fail("Should not be able to set channel max from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetChannelMaxOnLocallyClosedConnection()
      {
         TestCannotSetChannelMaxOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetChannelMaxOnRemotelyClosedConnection()
      {
         TestCannotSetChannelMaxOnClosedConnection(false);
      }

      private void TestCannotSetChannelMaxOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start();
         connection.Open();
         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.ChannelMax = 0;
            Assert.Fail("Should not be able to set channel max from closed Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetMaxFrameSizeOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start();
         connection.Open();

         try
         {
            connection.MaxFrameSize = 65535u;
            Assert.Fail("Should not be able to set max frame size from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetMaxFrameSizeOnLocallyClosedConnection()
      {
         TestCannotSetMaxFrameSizeOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetMaxFrameSizeOnRemotelyClosedConnection()
      {
         TestCannotSetMaxFrameSizeOnClosedConnection(false);
      }

      private void TestCannotSetMaxFrameSizeOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start();
         connection.Open();
         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.MaxFrameSize = 65535u;
            Assert.Fail("Should not be able to set max frame size from closed Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetIdleTimeoutOnOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start();

         connection.Open();

         Assert.AreEqual(0, connection.IdleTimeout);

         try
         {
            connection.IdleTimeout = 65535;
            Assert.Fail("Should not be able to set idle timeout from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         Assert.AreEqual(0, connection.IdleTimeout);

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetIdleTimeoutOnLocallyClosedConnection()
      {
         TestCannotSetIdleTimeoutOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetIdleTimeoutOnRemotelyClosedConnection()
      {
         TestCannotSetIdleTimeoutOnClosedConnection(false);
      }

      private void TestCannotSetIdleTimeoutOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start();
         connection.Open();
         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.IdleTimeout = 65535;
            Assert.Fail("Should not be able to set idle timeout from closed Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetOfferedCapabilitiesOnOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start().Open();

         try
         {
            connection.OfferedCapabilities = new Symbol[] { Symbol.Lookup("ANONYMOUS_RELAY") };
            Assert.Fail("Should not be able to set offered capabilities from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetOfferedCapabilitiesOnLocallyClosedConnection()
      {
         TestCannotSetOfferedCapabilitiesOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetOfferedCapabilitiesOnRemotelyClosedConnection()
      {
         TestCannotSetOfferedCapabilitiesOnClosedConnection(false);
      }

      private void TestCannotSetOfferedCapabilitiesOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start().Open();

         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.OfferedCapabilities = new Symbol[] { Symbol.Lookup("ANONYMOUS_RELAY") };
            Assert.Fail("Should not be able to set offered capabilities from closed Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetDesiredCapabilitiesOnOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start().Open();

         try
         {
            connection.DesiredCapabilities = new Symbol[] { Symbol.Lookup("ANONYMOUS_RELAY") };
            Assert.Fail("Should not be able to set desired capabilities from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetDesiredCapabilitiesOnLocallyClosedConnection()
      {
         TestCannotSetDesiredCapabilitiesOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetDesiredCapabilitiesOnRemotelyClosedConnection()
      {
         TestCannotSetDesiredCapabilitiesOnClosedConnection(false);
      }

      private void TestCannotSetDesiredCapabilitiesOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start().Open();

         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.DesiredCapabilities = new Symbol[] { Symbol.Lookup("ANONYMOUS_RELAY") };
            Assert.Fail("Should not be able to set desired capabilities from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetPropertiesOnOpenConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();

         IConnection connection = engine.Start().Open();

         try
         {
            connection.Properties = new Dictionary<Symbol, object>();
            Assert.Fail("Should not be able to set properties from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestCannotSetPropertiesOnLocallyClosedConnection()
      {
         TestCannotSetPropertiesOnClosedConnection(true);
      }

      [Test]
      public void TestCannotSetPropertiesOnRemotelyClosedConnection()
      {
         TestCannotSetPropertiesOnClosedConnection(false);
      }

      private void TestCannotSetPropertiesOnClosedConnection(bool localClose)
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         if (localClose)
         {
            peer.ExpectClose().Respond();
         }
         else
         {
            peer.RemoteClose().Queue();
         }

         IConnection connection = engine.Start().Open();

         if (localClose)
         {
            connection.Close();
         }

         try
         {
            connection.Properties = new Dictionary<Symbol, object>();
            Assert.Fail("Should not be able to set properties from open Connection");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestIterateAndCloseSessionsFromSessionsAPI()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectBegin().Respond();
         peer.ExpectBegin().Respond();

         IConnection connection = engine.Start().Open();

         connection.Session().Open();
         connection.Session().Open();
         connection.Session().Open();

         peer.WaitForScriptToComplete();

         peer.ExpectEnd().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectEnd().Respond();
         peer.ExpectClose();

         foreach (ISession session in connection.Sessions)
         {
            session.Close();
         }

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.AreEqual(0, connection.Sessions.Count);

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionClosedWhenChannelMaxExceeded()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         bool closed = false;

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithChannelMax(16).Respond();
         peer.ExpectClose().WithError(ConnectionError.FRAMING_ERROR.ToString(), "Channel Max Exceeded for session Begin").Respond();

         IConnection connection = engine.Start();

         connection.ChannelMax = 16;
         connection.LocalCloseHandler((conn) => closed = true);
         connection.Open();

         peer.RemoteBegin().OnChannel(32).Now();

         peer.WaitForScriptToComplete();

         Assert.AreEqual(0, connection.Sessions.Count);
         Assert.IsTrue(closed);

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestConnectionThrowsWhenLocalChannelMaxExceeded()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().WithChannelMax(1).Respond();
         peer.ExpectBegin().OnChannel(0).Respond().OnChannel(1);
         peer.ExpectBegin().OnChannel(1).Respond().OnChannel(0);
         peer.ExpectEnd().OnChannel(1).Respond().OnChannel(0);

         IConnection connection = engine.Start();

         connection.ChannelMax = 1;
         connection.Open();

         ISession session1 = connection.Session().Open();
         ISession session2 = connection.Session().Open();

         try
         {
            connection.Session().Open();
            Assert.Fail("Should not be able to exceed local channel max");
         }
         catch (InvalidOperationException)
         {
            // Expected
         }

         session2.Close();

         peer.WaitForScriptToComplete();
         peer.ExpectBegin().OnChannel(1).Respond().OnChannel(0);
         peer.ExpectEnd().OnChannel(0).Respond().OnChannel(1);
         peer.ExpectEnd().OnChannel(1).Respond().OnChannel(0);
         peer.ExpectClose().Respond();

         session2 = connection.Session().Open();
         session1.Close();
         session2.Close();

         connection.Close();

         Assert.IsNull(failure);
      }

      [Test]
      public void TestNoOpenWrittenAfterEncodeErrorFromConnectionProperties()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();

         Dictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), engine);

         IConnection connection = engine.Start();
         connection.Properties = properties;

         // Ensures that open is synchronous as header exchange will be complete.
         connection.Negotiate();

         try
         {
            connection.Open();
            Assert.Fail("Should not have been able to open with invalid type in properties");
         }
         catch (FrameEncodingException fee)
         {
            Assert.IsTrue(fee.InnerException is EncodeException);
            engine.Shutdown();
         }

         connection.Close();

         peer.WaitForScriptToComplete();

         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestPipelinedResourceOpenAllowsForReturningResponsesAfterCloseOfConnection()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin();
         peer.ExpectEnd();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();

         session.Close();

         peer.WaitForScriptToComplete();
         peer.ExpectClose();
         peer.RemoteBegin().WithRemoteChannel(0)
                           .WithNextOutgoingId(0).Queue();
         peer.RemoteClose().Queue();

         connection.Close();

         peer.WaitForScriptToComplete();
         Assert.IsNull(failure);
      }

      [Test]
      public void TestSecondOpenAfterReceiptOfFirstFailsEngine()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Connection;

         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().WithError(Is.NotNullValue());

         engine.Start();

         peer.RemoteOpen().OnChannel(0).Now();

         peer.WaitForScriptToComplete();
         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestUnexpectedEndFrameFailsEngine()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Connection;

         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().WithError(Is.NotNullValue());

         engine.Start();

         peer.RemoteEnd().OnChannel(10).Now();

         peer.WaitForScriptToComplete();
         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestUnexpectedAttachForUnknownChannelFrameFailsEngine()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Connection;

         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().WithError(Is.NotNullValue());

         engine.Start();

         peer.RemoteAttach().OfSender().WithName("test").WithHandle(0).OnChannel(10).Now();

         peer.WaitForScriptToComplete();
         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestUnexpectedDetachForUnknownChannelFrameFailsEngine()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Connection;

         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectClose().WithError(Is.NotNullValue());

         engine.Start();

         peer.RemoteDetach().WithHandle(0).OnChannel(10).Now();

         peer.WaitForScriptToComplete();
         Assert.IsNotNull(failure);
      }

      [Test]
      public void TestSecondBeginForAlreadyBegunSessionTriggerEngineFailure()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((error) => failure = error.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         IConnection connection = engine.Connection;
         connection.Session().Open();
         connection.Open();

         peer.WaitForScriptToComplete();
         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond();
         peer.ExpectBegin().OnChannel(0).Respond().OnChannel(0);
         peer.ExpectClose().WithError(Is.NotNullValue());

         engine.Start();

         peer.RemoteBegin().OnChannel(0).Now();

         peer.WaitForScriptToComplete();
         Assert.IsNotNull(failure);
      }
   }
}