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
using System.Diagnostics;
using System.Threading;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Types.Transport;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectTest : ClientBaseTestFixture
   {
      [Test]
      public void TestConnectionNotifiesReconnectionLifecycleEvents()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().WithContainerId(Test.Driver.Matchers.Matches.Any(typeof(string))).Respond();
            firstPeer.DropAfterLastHandler(5);
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().WithContainerId(Test.Driver.Matchers.Matches.Any(typeof(string))).Respond();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            CountdownEvent connected = new CountdownEvent(1);
            CountdownEvent disconnected = new CountdownEvent(1);
            CountdownEvent reconnected = new CountdownEvent(1);
            CountdownEvent failed = new CountdownEvent(1);

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.MaxReconnectAttempts = 5;
            options.ReconnectOptions.ReconnectDelay = 10;
            options.ReconnectOptions.UseReconnectBackOff = false;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);
            options.ConnectedHandler = (connection, context) =>
            {
               connected.Signal();
            };
            options.InterruptedHandler = (connection, context) =>
            {
               disconnected.Signal();
            };
            options.ReconnectedHandler = (connection, context) =>
            {
               reconnected.Signal();
            };
            options.DisconnectedHandler = (connection, context) =>
            {
               failed.Signal();
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

            firstPeer.WaitForScriptToComplete();

            connection.OpenTask.Wait();

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectBegin().Respond();
            secondPeer.ExpectEnd().Respond();
            secondPeer.DropAfterLastHandler(10);

            ISession session = connection.OpenSession().OpenTask.Result;

            session.Close();

            secondPeer.WaitForScriptToComplete();

            Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(reconnected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(failed.Wait(TimeSpan.FromSeconds(5)));

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectThrowsSecurityViolationOnFailureSaslAuth()
      {
         DoTestConnectThrowsSecurityViolationOnFailureSaslExchange((byte)SaslCode.Auth);
      }

      [Test]
      public void TestConnectThrowsSecurityViolationOnFailureSaslSys()
      {
         DoTestConnectThrowsSecurityViolationOnFailureSaslExchange((byte)SaslCode.Sys);
      }

      [Test]
      public void TestConnectThrowsSecurityViolationOnFailureSaslSysPerm()
      {
         DoTestConnectThrowsSecurityViolationOnFailureSaslExchange((byte)SaslCode.SysPerm);
      }

      private void DoTestConnectThrowsSecurityViolationOnFailureSaslExchange(byte saslCode)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectFailingSASLPlainConnect(saslCode);
            peer.DropAfterLastHandler(10);
            peer.Start();

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.User = "test";
            options.Password = "pass";

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            try
            {
               connection.OpenTask.Wait();
            }
            catch (Exception exe)
            {
               Assert.IsTrue(exe.InnerException is ClientConnectionSecuritySaslException);
            }

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReconnectStopsAfterSaslAuthFailure()
      {
         TestReconnectStopsAfterSaslPermFailure((byte)SaslCode.Auth);
      }

      [Test]
      public void TestReconnectStopsAfterSaslSysFailure()
      {
         TestReconnectStopsAfterSaslPermFailure((byte)SaslCode.Sys);
      }

      [Test]
      public void TestReconnectStopsAfterSaslPermFailure()
      {
         TestReconnectStopsAfterSaslPermFailure((byte)SaslCode.SysPerm);
      }

      private void TestReconnectStopsAfterSaslPermFailure(byte saslCode)
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer thirdPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen();
            secondPeer.DropAfterLastHandler();
            secondPeer.Start();

            thirdPeer.ExpectFailingSASLPlainConnect(saslCode);
            thirdPeer.DropAfterLastHandler();
            thirdPeer.Start();

            CountdownEvent connected = new CountdownEvent(1);
            CountdownEvent disconnected = new CountdownEvent(1);
            CountdownEvent reconnected = new CountdownEvent(1);
            CountdownEvent failed = new CountdownEvent(1);

            string firstAddress = firstPeer.ServerAddress;
            int firstPort = firstPeer.ServerPort;
            string secondAddress = secondPeer.ServerAddress;
            int secondPort = secondPeer.ServerPort;
            string thirdAddress = thirdPeer.ServerAddress;
            int thirdPort = thirdPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", firstAddress, firstPort);
            logger.LogInformation("Test started, second peer listening on: {0}:{1}", secondAddress, secondPort);
            logger.LogInformation("Test started, third peer listening on: {0}:{1}", thirdAddress, thirdPort);

            IClient container = IClient.Create();
            ConnectionOptions options = new ConnectionOptions();
            options.User = "test";
            options.Password = "pass";
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(secondAddress, secondPort)
                                    .AddReconnectLocation(thirdAddress, thirdPort);
            options.ConnectedHandler = (connection, context) =>
            {
               connected.Signal();
            };
            options.InterruptedHandler = (connection, context) =>
            {
               disconnected.Signal();
            };
            options.ReconnectedHandler = (connection, context) =>
            {
               reconnected.Signal();  // This one should not be triggered
            };
            options.DisconnectedHandler = (connection, context) =>
            {
               failed.Signal();
            };

            IConnection connection = container.Connect(firstAddress, firstPort, options).OpenTask.Result;

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
            thirdPeer.WaitForScriptToComplete();

            // Should connect, then fail and attempt to connect to second and third before stopping
            Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(failed.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(1, reconnected.CurrentCount);

            connection.Close();

            thirdPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectHandlesSaslTempFailureAndReconnects()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectFailingSASLPlainConnect((byte)SaslCode.SysTemp);
            firstPeer.DropAfterLastHandler();
            firstPeer.Start();

            secondPeer.ExpectSASLPlainConnect("test", "pass");
            secondPeer.ExpectOpen().Respond();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            CountdownEvent connected = new CountdownEvent(1);
            AtomicReference<string> connectedHost = new AtomicReference<string>();
            AtomicReference<string> connectedPort = new AtomicReference<string>();

            ConnectionOptions options = new ConnectionOptions();
            options.User = "test";
            options.Password = "pass";
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);
            options.ConnectedHandler = (connection, connectedEvent) =>
            {
               connectedHost.Set(connectedEvent.Host);
               connectedPort.Set(connectedEvent.Port.ToString());
               connected.Signal();
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

            firstPeer.WaitForScriptToComplete();

            connection.OpenTask.Wait();

            Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(5)));

            // Should never have connected and exchanged Open performatives with first peer
            // so we won't have had a connection established event there.
            Assert.AreEqual(backupAddress, connectedHost.Get());
            Assert.AreEqual(backupPort.ToString(), connectedPort.Get());

            secondPeer.WaitForScriptToComplete();

            secondPeer.ExpectClose().Respond();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectThrowsSecurityViolationOnFailureFromOpen()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Reject(AmqpError.UNAUTHORIZED_ACCESS.ToString(), "Anonymous connections not allowed");
            peer.ExpectBegin().Optional();  // Could arrive if remote open response not processed in time
            peer.ExpectClose();
            peer.Start();

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, options);

            try
            {
               connection.OpenTask.Wait();
            }
            catch (Exception exe)
            {
               // Possible based on time of rejecting open arrival.
               Assert.IsTrue(exe.InnerException is ClientConnectionSecurityException);
            }

            try
            {
               connection.DefaultSession().OpenTask.Wait();
               Assert.Fail("Should fail connection since remote rejected open with auth error");
            }
            catch (ClientConnectionSecurityException)
            {
            }
            catch (Exception exe)
            {
               Assert.IsTrue(exe.InnerException is ClientConnectionSecurityException);
            }

            connection.Close();

            try
            {
               connection.DefaultSession();
               Assert.Fail("Should fail as illegal state as connection was closed.");
            }
            catch (ClientIllegalStateException)
            {
            }

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReconnectHandlesDropThenRejectionCloseAfterConnect()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer thirdPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Reject(AmqpError.INVALID_FIELD.ToString(), "Connection configuration has invalid field");
            secondPeer.ExpectClose();
            secondPeer.Start();

            thirdPeer.ExpectSASLAnonymousConnect();
            thirdPeer.ExpectOpen().Respond();
            thirdPeer.Start();

            CountdownEvent connected = new CountdownEvent(1);
            CountdownEvent disconnected = new CountdownEvent(2);
            CountdownEvent reconnected = new CountdownEvent(2);
            CountdownEvent failed = new CountdownEvent(1);

            string firstAddress = firstPeer.ServerAddress;
            int firstPort = firstPeer.ServerPort;
            string secondAddress = secondPeer.ServerAddress;
            int secondPort = secondPeer.ServerPort;
            string thirdAddress = thirdPeer.ServerAddress;
            int thirdPort = thirdPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", firstAddress, firstPort);
            logger.LogInformation("Test started, second peer listening on: {0}:{1}", secondAddress, secondPort);
            logger.LogInformation("Test started, third peer listening on: {0}:{1}", thirdAddress, thirdPort);

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(secondAddress, secondPort)
                                    .AddReconnectLocation(thirdAddress, thirdPort);
            options.ConnectedHandler = (connection, context) =>
            {
               connected.Signal();
            };
            options.InterruptedHandler = (connection, context) =>
            {
               disconnected.Signal();
            };
            options.ReconnectedHandler = (connection, context) =>
            {
               reconnected.Signal();
            };
            options.DisconnectedHandler = (connection, context) =>
            {
               failed.Signal();  // Not expecting any failure in this test case
            };

            IConnection connection = IClient.Create().Connect(firstAddress, firstPort, options);

            firstPeer.WaitForScriptToComplete();

            connection.OpenTask.Wait();

            firstPeer.Close();

            secondPeer.WaitForScriptToComplete();

            // Should connect, then fail and attempt to connect to second and be rejected then reconnect to third.
            Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(reconnected.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(1, failed.CurrentCount);

            thirdPeer.ExpectClose().Respond();
            connection.Close();

            thirdPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestClientReconnectsWhenConnectionDropsAfterOpenReceived()
      {
         DoTestClientReconnectsWhenConnectionDropsAfterOpenReceived(0);
      }

      [Test]
      public void TestClientReconnectsWhenConnectionDropsAfterDelayAfterOpenReceived()
      {
         DoTestClientReconnectsWhenConnectionDropsAfterOpenReceived(20);
      }

      private void DoTestClientReconnectsWhenConnectionDropsAfterOpenReceived(int dropDelay)
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen();
            if (dropDelay > 0)
            {
               firstPeer.DropAfterLastHandler(dropDelay);
            }
            else
            {
               firstPeer.DropAfterLastHandler();
            }
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            CountdownEvent connected = new CountdownEvent(1);
            AtomicReference<string> connectedHost = new AtomicReference<string>();
            AtomicReference<string> connectedPort = new AtomicReference<string>();

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);
            options.ConnectedHandler = (connection, connectedEvent) =>
            {
               connectedHost.Set(connectedEvent.Host);
               connectedPort.Set(connectedEvent.Port.ToString());
               connected.Signal();
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

            firstPeer.WaitForScriptToComplete();

            connection.OpenTask.Wait();

            Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(backupAddress, connectedHost.Get());
            Assert.AreEqual(backupPort.ToString(), connectedPort.Get());

            secondPeer.WaitForScriptToComplete();

            secondPeer.ExpectClose().Respond();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestClientReconnectsWhenOpenRejected()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Reject(AmqpError.INVALID_FIELD.ToString(), "Error with client Open performative");
            firstPeer.ExpectClose();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            CountdownEvent connected = new CountdownEvent(1);
            AtomicReference<string> connectedHost = new AtomicReference<string>();
            AtomicReference<string> connectedPort = new AtomicReference<string>();

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);
            options.ConnectedHandler = (connection, connectedEvent) =>
            {
               connectedHost.Set(connectedEvent.Host);
               connectedPort.Set(connectedEvent.Port.ToString());
               connected.Signal();
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

            firstPeer.WaitForScriptToComplete();

            connection.OpenTask.Wait();

            Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(primaryAddress, connectedHost.Get());
            Assert.AreEqual(primaryPort.ToString(), connectedPort.Get());

            secondPeer.WaitForScriptToComplete();

            secondPeer.ExpectClose().Respond();
            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestClientReconnectsWhenConnectionRemotelyClosedWithForced()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin();
            firstPeer.RemoteClose().WithErrorCondition(ConnectionError.CONNECTION_FORCED.ToString(), "Forced disconnect").Queue();
            firstPeer.ExpectClose();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.ExpectBegin().Respond();
            secondPeer.Start();

            string primaryAddress = firstPeer.ServerAddress;
            int primaryPort = firstPeer.ServerPort;
            string backupAddress = secondPeer.ServerAddress;
            int backupPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", primaryAddress, primaryPort);
            logger.LogInformation("Test started, backup peer listening on: {0}:{1}", backupAddress, backupPort);

            CountdownEvent connected = new CountdownEvent(1);
            CountdownEvent disconnected = new CountdownEvent(1);
            CountdownEvent reconnected = new CountdownEvent(1);
            CountdownEvent failed = new CountdownEvent(1);

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);
            options.ConnectedHandler = (connection, context) =>
            {
               connected.Signal();
            };
            options.InterruptedHandler = (connection, context) =>
            {
               disconnected.Signal();
            };
            options.ReconnectedHandler = (connection, context) =>
            {
               reconnected.Signal();
            };
            options.DisconnectedHandler = (connection, context) =>
            {
               failed.Signal();  // Not expecting any failure in this test case
            };

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            ISession session = connection.OpenSession();

            connection.OpenTask.Wait();

            firstPeer.WaitForScriptToComplete();

            try
            {
               session.OpenTask.Wait();
            }
            catch (Exception)
            {
               Assert.Fail("Should eventually succeed in opening this Session");
            }

            // Should connect, then be remotely closed and reconnect to the alternate
            Assert.IsTrue(connected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(disconnected.Wait(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(reconnected.Wait(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(1, failed.CurrentCount);

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectClose().Respond();

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestInitialReconnectDelayDoesNotApplyToInitialConnect()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;

            string primaryAddress = peer.ServerAddress;
            int primaryPort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", primaryAddress, primaryPort);

            int delay = 20000;
            Stopwatch watch = Stopwatch.StartNew();

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

            connection.OpenTask.Wait();

            long taken = watch.ElapsedMilliseconds;

            String message = "Initial connect should not have delayed for the specified initialReconnectDelay." +
                                   "Elapsed=" + taken + ", delay=" + delay;
            Assert.IsTrue(taken < delay, message);
            Assert.IsTrue(taken < 5000, "Connection took longer than reasonable: " + taken);

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestConnectionReportsFailedAfterMaxInitialReconnectAttempts()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.Start();

            string primaryAddress = peer.ServerAddress;
            int primaryPort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", primaryAddress, primaryPort);

            peer.Close();

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.MaxReconnectAttempts = -1; // Try forever if connect succeeds once.
            options.ReconnectOptions.MaxInitialConnectionAttempts = 3;
            options.ReconnectOptions.WarnAfterReconnectAttempts = 5;
            options.ReconnectOptions.ReconnectDelay = 10;
            options.ReconnectOptions.UseReconnectBackOff = false;

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

            try
            {
               connection.OpenTask.Wait();
               Assert.Fail("Should not successfully connect.");
            }
            catch (Exception exe)
            {
               Assert.IsTrue(exe.InnerException is ClientConnectionRemotelyClosedException);
            }

            try
            {
               connection.DefaultSender();
               Assert.Fail("Connection should be in a failed state now.");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
            }

            connection.Close();

            try
            {
               connection.DefaultSender();
               Assert.Fail("Connection should be in a closed state now.");
            }
            catch (ClientIllegalStateException)
            {
            }
         }
      }

      [Test]
      public void TestConnectionReportsFailedAfterMaxInitialReconnectAttemptsWithBackOff()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.Start();

            string primaryAddress = peer.ServerAddress;
            int primaryPort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", primaryAddress, primaryPort);

            peer.Close();

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.MaxReconnectAttempts = -1; // Try forever if connect succeeds once.
            options.ReconnectOptions.MaxInitialConnectionAttempts = 10;
            options.ReconnectOptions.WarnAfterReconnectAttempts = 2;
            options.ReconnectOptions.ReconnectDelay = 10;
            options.ReconnectOptions.UseReconnectBackOff = true;
            options.ReconnectOptions.MaxReconnectDelay = 100;

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);

            try
            {
               connection.OpenTask.Wait();
               Assert.Fail("Should not successfully connect.");
            }
            catch (Exception exe)
            {
               Assert.IsTrue(exe.InnerException is ClientConnectionRemotelyClosedException);
            }

            try
            {
               connection.DefaultSender();
               Assert.Fail("Connection should be in a failed state now.");
            }
            catch (ClientConnectionRemotelyClosedException)
            {
            }

            connection.Close();

            try
            {
               connection.DefaultSender();
               Assert.Fail("Connection should be in a closed state now.");
            }
            catch (ClientIllegalStateException)
            {
            }
         }
      }
   }
}