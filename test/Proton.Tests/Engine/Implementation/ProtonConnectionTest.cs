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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Security;
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
      [Ignore("Fixes needed for link creation bits that aren't tested yet")]
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

         // Default engine should start and return a connection immediately
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

         // Default engine should start and return a connection immediately
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

         // Default engine should start and return a connection immediately
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

         // Default engine should start and return a connection immediately
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
      [Ignore("Failing test needs investigation")]
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
      [Ignore("Test failure needs investigation")]
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

         Assert.AreEqual(remoteCondition, connection.RemoteErrorCondition);

         connection.Close();

         Assert.IsFalse(connection.IsLocallyOpen);
         Assert.IsTrue(connection.IsLocallyClosed);
         Assert.IsFalse(connection.IsRemotelyOpen);
         Assert.IsTrue(connection.IsRemotelyClosed);

         peer.WaitForScriptToComplete();

         Assert.IsNull(failure);
      }

      [Test]
      [Ignore("Empty frame or action processing breaks and remote open doesn't arrive")]
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
   }
}