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

      [Ignore("Sender is not fully implemented yet")]
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