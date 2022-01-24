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

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientContainerTest : ClientBaseTestFixture
   {
      [Test]
      public void TestCreateWithNoContainerIdIsAllowed()
      {
         ClientOptions options = new ClientOptions();
         Assert.IsNull(options.Id);

         try
         {
            IClient instance = IClient.Create(options);

            Assert.IsNull(instance.ContainerId);
         }
         catch (Exception)
         {
            Assert.Fail("Should not enforce user supplied container Id");
         }
      }

      [Test]
      public void TestCreateWithContainerId()
      {
         string id = "test-id";

         ClientOptions options = new ClientOptions();
         options.Id = id;
         Assert.IsNotNull(options.Id);

         IClient client = IClient.Create(options);
         Assert.IsNotNull(client.ContainerId);
         Assert.AreEqual(id, client.ContainerId);
      }

      [Test]
      public void TestCloseClientAndConnectShouldFail()
      {
         IClient client = IClient.Create();
         Assert.IsTrue(client.CloseAsync().IsCompleted);

         try
         {
            client.Connect("localhost");
            Assert.Fail("Should enforce no new connections on Client close");
         }
         catch (ClientIllegalStateException)
         {
            // Expected
         }

         try
         {
            client.Connect("localhost", new ConnectionOptions());
            Assert.Fail("Should enforce no new connections on Client close");
         }
         catch (ClientIllegalStateException)
         {
            // Expected
         }

         try
         {
            client.Connect("localhost", 5672);
            Assert.Fail("Should enforce no new connections on Client close");
         }
         catch (ClientIllegalStateException)
         {
            // Expected
         }

         try
         {
            client.Connect("localhost", 5672, new ConnectionOptions());
            Assert.Fail("Should enforce no new connections on Client close");
         }
         catch (ClientIllegalStateException)
         {
            // Expected
         }
      }

      [Test]
      public void TestCloseAllConnectionWhenNonCreatedDoesNotBlock()
      {
         IClient.Create().Close();
      }

      [Ignore("Bug in how connection close async blocks instead of being async")]
      [Test]
      public void TestCloseAllConnectionAndWait()
      {
         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.Start();

            secondPeer.ExpectSASLAnonymousConnect();
            secondPeer.ExpectOpen().Respond();
            secondPeer.Start();

            string firstAddress = firstPeer.ServerAddress;
            int firstPort = firstPeer.ServerPort;
            string secondAddress = secondPeer.ServerAddress;
            int secondPort = secondPeer.ServerPort;

            logger.LogInformation("Test started, first peer listening on: {0}:{1}", firstAddress, firstPort);
            logger.LogInformation("Test started, second peer listening on: {0}:{1}", secondAddress, secondPort);

            IClient container = IClient.Create();
            IConnection connection1 = container.Connect(firstAddress, firstPort);
            IConnection connection2 = container.Connect(secondAddress, secondPort);

            connection1.OpenTask.Wait();
            connection2.OpenTask.Wait();

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();

            firstPeer.ExpectClose().Respond().AfterDelay(10);
            secondPeer.ExpectClose().Respond().AfterDelay(11);

            container.CloseAsync().Wait();

            firstPeer.WaitForScriptToComplete();
            secondPeer.WaitForScriptToComplete();
         }
      }
   }
}