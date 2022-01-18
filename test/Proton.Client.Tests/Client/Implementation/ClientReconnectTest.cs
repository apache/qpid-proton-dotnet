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
using Apache.Qpid.Proton.Test.Driver;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectTest : ClientBaseTestFixture
   {
      [Ignore("Client failover not yet fully operational")]
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
   }
}