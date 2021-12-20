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
using System.Threading;
using NUnit.Framework;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Client.Exceptions;
using Microsoft.Extensions.Logging;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Matchers;
using Apache.Qpid.Proton.Types.Transport;
using System.Threading.Tasks;
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using System.Linq;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientConnectionTest : ClientBaseTestFixture
   {
      [Test]
      public void TestConnectFailsDueToServerStopped()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            peer.Close();

            IClient container = IClient.Create();

            try
            {
               IConnection connection = container.Connect(remoteAddress, remotePort, new ConnectionOptions());
               _ = connection.OpenTask.Result;
               Assert.Fail("Should fail to connect");
            }
            catch (Exception ex)
            {
               logger.LogInformation(ex, "Connection create failed due to: {0}", ex.Message);
               Assert.IsTrue(ex.InnerException is ClientException);
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateTwoDistinctConnectionsFromSingleClientInstance()
      {
         using ProtonTestServer firstPeer = new ProtonTestServer(TestServerOptions(), loggerFactory);
         using ProtonTestServer secondPeer = new ProtonTestServer(TestServerOptions(), loggerFactory);
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().WithContainerId(Matches.Any(typeof(string))).Respond();
            firstPeer.ExpectClose().Respond();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().WithContainerId(Matches.Any(typeof(string))).Respond();
            secondPeer.ExpectClose().Respond();
            secondPeer.Start();

            string firstAddress = firstPeer.ServerAddress;
            int firstPort = firstPeer.ServerPort;
            string secondAddress = secondPeer.ServerAddress;
            int secondPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, peer 1 listening on: {0}:{1}", firstAddress, firstPort);
            logger.LogInformation("Test started, peer 2 listening on: {0}:{1}", secondAddress, secondPort);

            IClient container = IClient.Create();
            IConnection connection1 = container.Connect(firstAddress, firstPort, ConnectionOptions());
            IConnection connection2 = container.Connect(secondAddress, secondPort, ConnectionOptions());

            _ = connection1.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection2.OpenTask.Wait(TimeSpan.FromSeconds(10));

            _ = connection1.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            _ = connection2.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionToNonSaslPeer()
      {
         DoConnectionWithUnexpectedHeaderTestImpl(AMQPHeader.Header.ToArray());
      }

      [Test]
      public void TestCreateConnectionToNonAmqpPeer()
      {
         DoConnectionWithUnexpectedHeaderTestImpl(
            new byte[] { (byte)'N', (byte)'O', (byte)'T', (byte)'-', (byte)'A', (byte)'M', (byte)'Q', (byte)'P' });
      }

      private void DoConnectionWithUnexpectedHeaderTestImpl(byte[] responseHeader)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLHeader().RespondWithBytes(responseHeader);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = ConnectionOptions("guest", "guest");
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            try
            {
               _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            }
            catch (Exception ex)
            {
               logger.LogInformation(ex, "Open task threw error: {0}", ex.Message);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionString()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = ConnectionOptions("guest", "guest");
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionSignalsEvent()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            CountdownEvent connected = new CountdownEvent(1);

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = ConnectionOptions();

            options.ConnectedHandler = (conn, eventArgs) => connected.Signal();

            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            Assert.IsTrue(connected.Wait(5000));

            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionWithConfiguredContainerId()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithContainerId("container-id-test").Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ClientOptions options = new ClientOptions();
            options.Id = "container-id-test";

            IClient container = IClient.Create(options);
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionWithUnconfiguredContainerId()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithContainerId(Matches.Any(typeof(string))).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ClientOptions options = new ClientOptions();
            IClient container = IClient.Create(options);
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionStringWithDefaultTcpPort()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = ConnectionOptions();

            options.TransportOptions.DefaultTcpPort = remotePort;

            IConnection connection = container.Connect(remoteAddress, options);

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionEstablishedHandlerGetsCalled()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            CountdownEvent established = new CountdownEvent(1);
            ConnectionOptions options = ConnectionOptions();

            options.ConnectedHandler = (connection, connectEvent) =>
            {
               logger.LogInformation("Connection signaled that it was established");
               established.Signal();
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            Assert.IsTrue(established.Wait(TimeSpan.FromSeconds(10)));

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionFailedHandlerGetsCalled()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            CountdownEvent failed = new CountdownEvent(1);
            ConnectionOptions options = ConnectionOptions();

            options.DisconnectedHandler = (connection, location) =>
            {
               logger.LogInformation("Connection signaled that it has failed");
               failed.Signal();
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.OpenSession();

            Assert.IsTrue(failed.Wait(TimeSpan.FromSeconds(10)));

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionWithCredentialsChoosesSASLPlainIfOffered()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLPlainConnect("user", "pass");
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            CountdownEvent established = new CountdownEvent(1);
            ConnectionOptions options = ConnectionOptions("user", "pass");

            options.ConnectedHandler = (connection, connectEvent) =>
            {
               logger.LogInformation("Connection signaled that it was established");
               established.Signal();
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            Assert.IsTrue(established.Wait(TimeSpan.FromSeconds(10)));

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateConnectionWithSASLDisabledToSASLEnabledHost()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithSASLHeader();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ConnectionOptions options = ConnectionOptions();
            options.SaslOptions.SaslEnabled = false;

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            try
            {
               _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
               Assert.Fail("Should not successfully connect to remote");
            }
            catch (Exception ex)
            {
               Assert.IsTrue(ex.InnerException is ClientConnectionRemotelyClosedException);
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionCloseGetsResponseWithErrorDoesNotThrowTimedGet()
      {
         DoTestConnectionCloseGetsResponseWithErrorDoesNotThrow(true);
      }

      [Test]
      public void TestConnectionCloseGetsResponseWithErrorDoesNotThrowUntimedGet()
      {
         DoTestConnectionCloseGetsResponseWithErrorDoesNotThrow(false);
      }

      protected void DoTestConnectionCloseGetsResponseWithErrorDoesNotThrow(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond().WithErrorCondition(ConnectionError.CONNECTION_FORCED.ToString(), "Not accepting connections");
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            if (timeout)
            {
               // Should close normally and not throw error as we initiated the close.
               _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
               _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            }
            else
            {
               // Should close normally and not throw error as we initiated the close.
               _ = connection.OpenTask.Result;
               // TODO Passes if given time: Task.Delay(100).Wait();
               _ = connection.CloseAsync().Result;
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestRemotelyCloseConnectionWithRedirect()
      {
         string redirectVhost = "vhost";
         string redirectNetworkHost = "localhost";
         int redirectPort = 5677;
         string redirectScheme = "wss";
         string redirectPath = "/websockets";

         // Tell the test peer to close the connection when executing its last handler
         IDictionary<string, object> errorInfo = new Dictionary<string, object>();
         errorInfo.Add(ClientConstants.OPEN_HOSTNAME.ToString(), redirectVhost);
         errorInfo.Add(ClientConstants.NETWORK_HOST.ToString(), redirectNetworkHost);
         errorInfo.Add(ClientConstants.PORT.ToString(), redirectPort);
         errorInfo.Add(ClientConstants.SCHEME.ToString(), redirectScheme);
         errorInfo.Add(ClientConstants.PATH.ToString(), redirectPath);

         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Reject(ConnectionError.REDIRECT.ToString(), "Not accepting connections", errorInfo);
            peer.ExpectBegin().Optional();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            try
            {
               _ = connection.DefaultSession().OpenTask.Result;
               Assert.Fail("Should not be able to connect since the connection is redirected.");
            }
            catch (Exception ex)
            {
               logger.LogDebug("Received expected exception from session open: {0}", ex.Message);
               Exception cause = ex.InnerException;
               Assert.IsTrue(cause is ClientConnectionRedirectedException);

               ClientConnectionRedirectedException connectionRedirect = (ClientConnectionRedirectedException)ex.InnerException;

               Assert.AreEqual(redirectVhost, connectionRedirect.Hostname);
               Assert.AreEqual(redirectNetworkHost, connectionRedirect.NetworkHostname);
               Assert.AreEqual(redirectPort, connectionRedirect.Port);
               Assert.AreEqual(redirectScheme, connectionRedirect.Scheme);
               Assert.AreEqual(redirectPath, connectionRedirect.Path);
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionBlockingCloseGetsResponseWithErrorDoesNotThrow()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond().WithErrorCondition(ConnectionError.CONNECTION_FORCED.ToString(), "Not accepting connections");
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            // Should close normally and not throw error as we initiated the close.
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionClosedWithErrorToRemoteSync()
      {
         DoTestConnectionClosedWithErrorToRemote(false);
      }

      [Test]
      public void TestConnectionClosedWithErrorToRemoteAsync()
      {
         DoTestConnectionClosedWithErrorToRemote(true);
      }

      private void DoTestConnectionClosedWithErrorToRemote(bool async)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().WithError(ConnectionError.CONNECTION_FORCED.ToString(), "Closed").Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            _ = connection.OpenTask.Result;

            if (async)
            {
               connection.CloseAsync(IErrorCondition.Create(ConnectionError.CONNECTION_FORCED.ToString(), "Closed")).Wait();
            }
            else
            {
               connection.Close(IErrorCondition.Create(ConnectionError.CONNECTION_FORCED.ToString(), "Closed"));
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionRemoteClosedAfterOpened()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Reject(ConnectionError.CONNECTION_FORCED.ToString(), "Not accepting connections");
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionRemoteClosedAfterOpenedWithEmptyErrorConditionDescription()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Reject(ConnectionError.CONNECTION_FORCED.ToString(), (String)null);
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionRemoteClosedAfterOpenedWithNoRemoteErrorCondition()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Reject();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();

            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionOpenFutureWaitCancelledOnConnectionDropWithTimeout()
      {
         DoTestConnectionOpenFutureWaitCancelledOnConnectionDrop(true);
      }

      [Test]
      public void TestConnectionOpenFutureWaitCancelledOnConnectionDropNoTimeout()
      {
         DoTestConnectionOpenFutureWaitCancelledOnConnectionDrop(false);
      }

      protected void DoTestConnectionOpenFutureWaitCancelledOnConnectionDrop(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            peer.WaitForScriptToComplete();
            peer.Close();

            try
            {
               if (timeout)
               {
                  connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  connection.OpenTask.Wait();
               }
               Assert.Fail("Should have thrown an execution error due to connection drop");
            }
            catch (Exception error)
            {
               logger.LogInformation(error, "connection open failed with error: {0}", error.Message);
            }

            try
            {
               if (timeout)
               {
                  connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  connection.CloseAsync().Wait();
               }
            }
            catch (Exception error)
            {
               logger.LogInformation(error, "connection close failed with error: {0}", error.Message);
               Assert.Fail("Close should ignore connect error and complete without error.");
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestRemotelyCloseConnectionDuringSessionCreation()
      {
         String BREAD_CRUMB = "ErrorMessageBreadCrumb";

         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin();
            peer.RemoteClose().WithErrorCondition(AmqpError.NOT_ALLOWED.ToString(), BREAD_CRUMB).Queue();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            connection.OpenTask.Wait();

            ISession session = connection.OpenSession();

            try
            {
               session.OpenTask.Wait();
               Assert.Fail("Open should throw error when waiting for remote open and connection remotely closed.");
            }
            catch (Exception error)
            {
               logger.LogInformation(error, "Session open failed with error: {0}", error.Message);
               Assert.IsNotNull(error.Message, "Expected exception to have a message");
               Assert.IsTrue(error.Message.Contains(BREAD_CRUMB), "Expected breadcrumb to be present in message");
               Assert.IsNotNull(error.InnerException, "Execution error should convey the cause");
               Assert.IsTrue(error.InnerException is ClientConnectionRemotelyClosedException);
            }

            session.CloseAsync().Wait();

            peer.WaitForScriptToComplete();

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionOpenTimeoutWhenNoRemoteOpenArrivesTimeout()
      {
         DoTestConnectionOpenTimeoutWhenNoRemoteOpenArrives(true);
      }

      [Test]
      public void TestConnectionOpenTimeoutWhenNoRemoteOpenArrivesNoTimeout()
      {
         DoTestConnectionOpenTimeoutWhenNoRemoteOpenArrives(false);
      }

      private void DoTestConnectionOpenTimeoutWhenNoRemoteOpenArrives(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ConnectionOptions options = ConnectionOptions();
            options.OpenTimeout = 75;
            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            try
            {
               if (timeout)
               {
                  connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  connection.OpenTask.Wait();
               }

               Assert.Fail("Open should timeout when no open response and complete future with error.");
            }
            catch (Exception error)
            {
               logger.LogInformation(error, "Connection open failed with error: {0}", error.Message);
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionOpenWaitWithTimeoutCanceledWhenConnectionDrops()
      {
         DoTestConnectionOpenWaitCanceledWhenConnectionDrops(true);
      }

      [Test]
      public void TestConnectionOpenWaitWithNoTimeoutCanceledWhenConnectionDrops()
      {
         DoTestConnectionOpenWaitCanceledWhenConnectionDrops(false);
      }

      private void DoTestConnectionOpenWaitCanceledWhenConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            try
            {
               if (timeout)
               {
                  connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  connection.OpenTask.Wait();
               }

               Assert.Fail("Open should timeout when no open response and complete future with error.");
            }
            catch (Exception error)
            {
               logger.LogInformation(error, "Connection open failed with error: {0}", error.Message);
               Assert.IsTrue(error.InnerException is ClientIOException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionCloseTimeoutWhenNoRemoteCloseArrivesTimeout()
      {
         DoTestConnectionCloseTimeoutWhenNoRemoteCloseArrives(true);
      }

      [Test]
      public void TestConnectionCloseTimeoutWhenNoRemoteCloseArrivesNoTimeout()
      {
         DoTestConnectionCloseTimeoutWhenNoRemoteCloseArrives(false);
      }

      private void DoTestConnectionCloseTimeoutWhenNoRemoteCloseArrives(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ConnectionOptions options = ConnectionOptions();
            options.CloseTimeout = 75;
            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            // Shouldn't throw from close, nothing to be done anyway.
            try
            {
               if (timeout)
               {
                  connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  connection.CloseAsync().Wait();
               }
            }
            catch (Exception error)
            {
               logger.LogInformation(error, "Connection open failed with error: {0}", error.Message);
               Assert.Fail("Close should ignore lack of close response and complete without error.");
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionCloseWaitWithTimeoutCompletesAfterRemoteConnectionDrops()
      {
         DoTestConnectionCloseWaitCompletesAfterRemoteConnectionDrops(true);
      }

      [Test]
      public void TestConnectionCloseWaitWithNoTimeoutCompletesAfterRemoteConnectionDrops()
      {
         DoTestConnectionCloseWaitCompletesAfterRemoteConnectionDrops(false);
      }

      private void DoTestConnectionCloseWaitCompletesAfterRemoteConnectionDrops(bool timeout)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose();
            peer.DropAfterLastHandler(10);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            // Shouldn't throw from close, nothing to be done anyway.
            try
            {
               if (timeout)
               {
                  connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));
               }
               else
               {
                  connection.CloseAsync().Wait();
               }
            }
            catch (Exception error)
            {
               logger.LogInformation(error, "Connection open failed with error: {0}", error.Message);
               Assert.Fail("Close should treat Connection drop as success and complete without error.");
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateDefaultSenderFailsOnConnectionWithoutSupportForAnonymousRelay()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions()).OpenTask.Result;

            try
            {
               connection.DefaultSender();
               Assert.Fail("Should not be able to get the default sender when remote does not offer anonymous relay");
            }
            catch (ClientUnsupportedOperationException unsupported)
            {
               logger.LogInformation(unsupported, "Caught expected error: {0}", unsupported.Message);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCreateDefaultSenderOnConnectionWithSupportForAnonymousRelay()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithDesiredCapabilities(ClientConstants.ANONYMOUS_RELAY.ToString())
                             .Respond()
                             .WithOfferedCapabilities(ClientConstants.ANONYMOUS_RELAY.ToString());
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions()).OpenTask.Result;

            ISender defaultSender = connection.DefaultSender();
            defaultSender.OpenTask.Wait(TimeSpan.FromSeconds(10));

            Assert.IsNotNull(defaultSender);

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionRecreatesAnonymousRelaySenderAfterRemoteCloseOfSender()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().WithDesiredCapabilities(ClientConstants.ANONYMOUS_RELAY.ToString())
                             .Respond()
                             .WithOfferedCapabilities(ClientConstants.ANONYMOUS_RELAY.ToString());
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteDetach().Queue();
            peer.ExpectDetach();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();

            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ISender defaultSender = connection.DefaultSender().OpenTask.GetAwaiter().GetResult();
            Assert.IsNotNull(defaultSender);

            peer.WaitForScriptToComplete();
            peer.ExpectAttach().OfSender().Respond();
            peer.ExpectClose().Respond();

            ISender defaultSender2 = connection.DefaultSender();

            Assert.IsNotNull(defaultSender2);
            Assert.AreNotSame(defaultSender, defaultSender2);

            defaultSender2.OpenTask.Wait(TimeSpan.FromSeconds(10));

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Connection doesn't create dynamic receiver yet")]
      [Test]
      public void TestCreateDynamicReceiver()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource(new SourceMatcher().WithDynamic(true).WithAddress(Matches.Is(null)))
                               .Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            IReceiver receiver = connection.OpenDynamicReceiver();
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            Assert.IsNotNull(receiver.Address, "Remote should have assigned the address for the dynamic receiver");

            receiver.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Connection doesn't create dynamic receiver yet")]
      [Test]
      public void TestCreateDynamicReceiverWithNodeProperties()
      {
         IDictionary<string, object> dynamicNodeProperties = new Dictionary<string, object>();
         dynamicNodeProperties.Add("test", "vale");

         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithSource()
                               .WithDynamic(true)
                               .WithAddress(Matches.Is(null))
                               .WithDynamicNodeProperties(dynamicNodeProperties)
                               .Also()
                               .Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            IReceiver receiver = connection.OpenDynamicReceiver(dynamicNodeProperties);
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            Assert.IsNotNull(receiver.Address, "Remote should have assigned the address for the dynamic receiver");

            receiver.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Connection doesn't create dynamic receiver yet")]
      [Test]
      public void TestCreateDynamicReceiverWithReceiverOptions()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithDesiredCapabilities("queue")
                               .Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            connection.OpenTask.Wait(TimeSpan.FromSeconds(10));

            ReceiverOptions options = new ReceiverOptions();
            options.DesiredCapabilities = new string[] { "queue" };
            IReceiver receiver = connection.OpenDynamicReceiver(options);
            receiver.OpenTask.Wait(TimeSpan.FromSeconds(10));

            Assert.IsNotNull(receiver.Address, "Remote should have assigned the address for the dynamic receiver");

            receiver.CloseAsync().Wait(TimeSpan.FromSeconds(10));
            connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionSenderOpenHeldUntilConnectionOpenedAndRelaySupportConfirmed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            ISender sender = connection.DefaultSender();

            peer.WaitForScriptToComplete();

            // This should happen after we inject the held open and attach
            peer.ExpectAttach().OfSender().WithTarget().WithAddress(Test.Driver.Matchers.Is.NullValue()).And().Respond();
            peer.ExpectClose().Respond();

            // Inject held responses to get the ball rolling again
            peer.RemoteOpen().WithOfferedCapabilities("ANONYMOUS-RELAY").Now();
            peer.RespondToLastBegin().Now();

            try
            {
               sender.OpenTask.Wait();
            }
            catch (Exception ex)
            {
               Assert.Fail("Open of Sender failed waiting for response: " + ex.InnerException.Message);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionSenderIsSingleton()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond().WithOfferedCapabilities("ANONYMOUS-RELAY");
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithTarget().WithAddress(Test.Driver.Matchers.Is.NullValue()).And().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            ISender sender1 = connection.DefaultSender();
            ISender sender2 = connection.DefaultSender();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            try
            {
               sender1.OpenTask.Wait();
            }
            catch (Exception ex)
            {
               Assert.Fail("Open of Sender 1 failed waiting for response: " + ex.InnerException.Message);
            }

            try
            {
               sender2.OpenTask.Wait();
            }
            catch (Exception ex)
            {
               Assert.Fail("Open of Sender 2 failed waiting for response: " + ex.InnerException.Message);
            }

            Assert.AreSame(sender1, sender2);

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionSenderOpenFailsWhenAnonymousRelayNotSupported()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            ISender sender = connection.DefaultSender();

            peer.WaitForScriptToComplete();
            peer.ExpectClose().Respond();

            try
            {
               sender.OpenTask.Wait();
               Assert.Fail("Open of Sender should have failed waiting for response when anonymous relay not supported");
            }
            catch (Exception)
            {
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionGetRemotePropertiesWaitsForRemoteBegin()
      {
         TryReadConnectionRemoteProperties(true);
      }

      [Test]
      public void TestConnectionGetRemotePropertiesFailsAfterOpenTimeout()
      {
         TryReadConnectionRemoteProperties(false);
      }

      private void TryReadConnectionRemoteProperties(bool openResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = ConnectionOptions();
            options.OpenTimeout = 100;
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            peer.WaitForScriptToComplete();

            IDictionary<string, object> expectedProperties = new Dictionary<string, object>();
            expectedProperties.Add("TEST", "test-property");

            if (openResponse)
            {
               peer.ExpectClose().Respond();
               peer.RemoteOpen().WithProperties(expectedProperties).Later(10);
            }
            else
            {
               peer.ExpectClose();
            }

            if (openResponse)
            {
               Assert.IsNotNull(connection.Properties, "Remote should have responded with a remote properties value");
               Assert.AreEqual(expectedProperties, connection.Properties);
            }
            else
            {
               try
               {
                  _ = connection.Properties;
                  Assert.Fail("Should failed to get remote state due to no open response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex.Message);
               }
            }

            connection.CloseAsync().GetAwaiter().GetResult();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionGetRemoteOfferedCapabilitiesWaitsForRemoteBegin()
      {
         TryReadConnectionRemoteOfferedCapabilities(true);
      }

      [Test]
      public void TestConnectionGetRemoteOfferedCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadConnectionRemoteOfferedCapabilities(false);
      }

      private void TryReadConnectionRemoteOfferedCapabilities(bool openResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = ConnectionOptions();
            options.OpenTimeout = 100;
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            peer.WaitForScriptToComplete();

            if (openResponse)
            {
               peer.ExpectClose().Respond();
               peer.RemoteOpen().WithOfferedCapabilities("transactions").Later(10);
            }
            else
            {
               peer.ExpectClose();
            }

            if (openResponse)
            {
               Assert.IsNotNull(connection.OfferedCapabilities, "Remote should have responded with a remote offered Capabilities value");
               Assert.AreEqual(1, connection.OfferedCapabilities.Count);
               Assert.AreEqual("transactions", connection.OfferedCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = connection.OfferedCapabilities;
                  Assert.Fail("Should failed to get remote state due to no open response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex.Message);
               }
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionGetRemoteDesiredCapabilitiesWaitsForRemoteBegin()
      {
         TryReadConnectionRemoteDesiredCapabilities(true);
      }

      [Test]
      public void TestConnectionGetRemoteDesiredCapabilitiesFailsAfterOpenTimeout()
      {
         TryReadConnectionRemoteDesiredCapabilities(false);
      }

      private void TryReadConnectionRemoteDesiredCapabilities(bool openResponse)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            ConnectionOptions options = ConnectionOptions();
            options.OpenTimeout = 100;
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            peer.WaitForScriptToComplete();

            if (openResponse)
            {
               peer.ExpectClose().Respond();
               peer.RemoteOpen().WithDesiredCapabilities("Error-Free").Later(10);
            }
            else
            {
               peer.ExpectClose();
            }

            if (openResponse)
            {
               Assert.IsNotNull(connection.DesiredCapabilities, "Remote should have responded with a remote desired Capabilities value");
               Assert.AreEqual(1, connection.DesiredCapabilities.Count);
               Assert.AreEqual("Error-Free", connection.DesiredCapabilities.ElementAt(0));
            }
            else
            {
               try
               {
                  _ = connection.DesiredCapabilities;
                  Assert.Fail("Should failed to get remote state due to no open response");
               }
               catch (ClientException ex)
               {
                  logger.LogDebug("Caught expected exception from blocking call", ex.Message);
               }
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestCloseWithErrorCondition()
      {
         String condition = "amqp:precondition-failed";
         String description = "something bad happened.";

         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().WithError(condition, description).Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            connection.OpenTask.Wait();

            connection.Close(IErrorCondition.Create(condition, description, null));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAnonymousSenderOpenHeldUntilConnectionOpenedAndSupportConfirmed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());
            ISender sender = connection.OpenAnonymousSender();

            peer.WaitForScriptToComplete();

            // This should happen after we inject the held open and attach
            peer.ExpectAttach().OfSender().WithTarget().WithAddress(Test.Driver.Matchers.Is.NullValue()).And().Respond();
            peer.ExpectClose().Respond();

            // Inject held responses to get the ball rolling again
            peer.RemoteOpen().WithOfferedCapabilities("ANONYMOUS-RELAY").Now();
            peer.RespondToLastBegin().Now();

            try
            {
               sender.OpenTask.Wait();
            }
            catch (Exception ex)
            {
               Assert.Fail("Open of Sender failed waiting for response: " + ex.InnerException.Message);
            }

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Sender doesn't yet implement the send operation")]
      [Test]
      public void TestSendHeldUntilConnectionOpenedAndSupportConfirmed()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond().WithOfferedCapabilities("ANONYMOUS-RELAY");
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().WithTarget().WithAddress(Test.Driver.Matchers.Is.NullValue()).And().Respond();
            peer.RemoteFlow().WithLinkCredit(1).Queue();
            peer.ExpectTransfer().WithNonNullPayload()
                                 .WithDeliveryTag(new byte[] { 0 }).Respond().WithSettled(true).WithState().Accepted();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, ConnectionOptions());

            try
            {
               ITracker tracker = connection.Send(IMessage<string>.Create("Hello World"));
               Assert.IsNotNull(tracker);
               _ = tracker.AwaitAccepted();
            }
            catch (ClientException ex)
            {
               Assert.Fail("Open of Sender failed waiting for response: " + ex.Message);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Connection doesn't yet implement the send operation")]
      [Test]
      public void TestConnectionLevelSendFailsWhenAnonymousRelayNotAdvertisedByRemote()
      {
         DoTestConnectionLevelSendFailsWhenAnonymousRelayNotAdvertisedByRemote(false);
      }

      [Ignore("Connection doesn't yet implement the send operation")]
      [Test]
      public void TestConnectionLevelSendFailsWhenAnonymousRelayNotAdvertisedByRemoteAfterAlreadyOpened()
      {
         DoTestConnectionLevelSendFailsWhenAnonymousRelayNotAdvertisedByRemote(true);
      }

      private void DoTestConnectionLevelSendFailsWhenAnonymousRelayNotAdvertisedByRemote(bool openWait)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            // Ensures that the Begin arrives regard of a race on open without anonymous relay support
            connection.DefaultSession();

            if (openWait)
            {
               connection.OpenTask.Wait();
            }

            try
            {
               connection.Send(IMessage<string>.Create("Hello World"));
               Assert.Fail("Open of Sender should fail as remote did not advertise anonymous relay support: ");
            }
            catch (ClientUnsupportedOperationException)
            {
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenAnonymousSenderFailsWhenAnonymousRelayNotAdvertisedByRemote()
      {
         DoTestOpenAnonymousSenderFailsWhenAnonymousRelayNotAdvertisedByRemote(false);
      }

      [Test]
      public void TestOpenAnonymousSenderFailsWhenAnonymousRelayNotAdvertisedByRemoteAfterAlreadyOpened()
      {
         DoTestOpenAnonymousSenderFailsWhenAnonymousRelayNotAdvertisedByRemote(true);
      }

      private void DoTestOpenAnonymousSenderFailsWhenAnonymousRelayNotAdvertisedByRemote(bool openWait)
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            // Ensures that the Begin arrives regard of a race on open without anonymous relay support
            connection.DefaultSession();

            if (openWait)
            {
               connection.OpenTask.Wait();
            }

            try
            {
               connection.OpenAnonymousSender().OpenTask.Wait();
               Assert.Fail("Open of Sender should fail as remote did not advertise anonymous relay support: ");
            }
            catch (ClientUnsupportedOperationException)
            {
            }
            catch (Exception ex)
            {
               Assert.IsTrue(ex.InnerException is ClientUnsupportedOperationException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Connection doesn't yet implement the full open durable receiver features")]
      [Test]
      public void TestOpenDurableReceiverFromConnection()
      {
         String address = "test-topic";
         String subscriptionName = "mySubscriptionName";

         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver()
                               .WithName(subscriptionName)
                               .WithSource()
                                   .WithAddress(address)
                                   .WithDurable(TerminusDurability.UnsettledState)
                                   .WithExpiryPolicy(TerminusExpiryPolicy.Never)
                                   .WithDistributionMode("copy")
                               .And().Respond();
            peer.ExpectFlow();
            peer.ExpectDetach().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IReceiver receiver = connection.OpenDurableReceiver(address, subscriptionName);

            receiver.OpenTask.Wait();
            receiver.CloseAsync().Wait();

            connection.CloseAsync().Wait();

            peer.WaitForScriptToComplete();
         }
      }

      [Ignore("Client local bind not yet implemented")]
      [Test]
      public void TestLocalPortOption()
      {
         using (ProtonTestServer peer = new ProtonTestServer(TestServerOptions(), loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            int localPort = 5671;

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions();
            options.TransportOptions.LocalPort = localPort;
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            connection.OpenTask.Wait();

            Assert.AreEqual(localPort, peer.ClientPort);

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      protected ProtonTestServerOptions TestServerOptions()
      {
         return new ProtonTestServerOptions();
      }

      protected ConnectionOptions ConnectionOptions()
      {
         return new ConnectionOptions();
      }

      protected ConnectionOptions ConnectionOptions(string user, string password)
      {
         ConnectionOptions options = new ConnectionOptions();
         options.User = user;
         options.Password = password;

         return options;
      }
   }
}