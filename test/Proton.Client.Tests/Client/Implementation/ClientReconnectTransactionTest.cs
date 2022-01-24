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

using System.Threading;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Test.Driver;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientReconnectTransactionTest : ClientBaseTestFixture
   {
      [Test]
      public void TestDeclareTransactionAfterConnectionDropsAndReconnects()
      {
         byte[] txnId = new byte[] { 0, 1, 2, 3 };

         using (ProtonTestServer firstPeer = new ProtonTestServer(loggerFactory))
         using (ProtonTestServer secondPeer = new ProtonTestServer(loggerFactory))
         {
            firstPeer.ExpectSASLAnonymousConnect();
            firstPeer.ExpectOpen().Respond();
            firstPeer.ExpectBegin().Respond();
            firstPeer.DropAfterLastHandler();
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

            ConnectionOptions options = new ConnectionOptions();
            options.ReconnectOptions.ReconnectEnabled = true;
            options.ReconnectOptions.AddReconnectLocation(backupAddress, backupPort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(primaryAddress, primaryPort, options);
            ISession session = connection.OpenSession().OpenTask.Result;

            firstPeer.WaitForScriptToComplete();

            secondPeer.WaitForScriptToComplete();
            secondPeer.ExpectCoordinatorAttach().Respond();
            secondPeer.RemoteFlow().WithLinkCredit(2).Queue();
            secondPeer.ExpectDeclare().Accept(txnId);
            secondPeer.ExpectClose().Respond();

            try
            {
               session.BeginTransaction();
            }
            catch (ClientException cliEx)
            {
               logger.LogWarning("Caught unexpected error from test: {0}", cliEx);
               Assert.Fail("Should not have failed to declare transaction");
            }

            connection.Close();

            secondPeer.WaitForScriptToComplete();
         }
      }
   }
}