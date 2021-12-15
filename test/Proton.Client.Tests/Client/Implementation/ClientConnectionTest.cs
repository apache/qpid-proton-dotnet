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

      [Ignore("Test fails as events are yet implemented")]
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

            options.Connected += (sender, eventArgs) => connected.AddCount();

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