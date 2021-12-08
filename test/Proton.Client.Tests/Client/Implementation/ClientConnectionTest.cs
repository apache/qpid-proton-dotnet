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
   public class ClientConnectionTest : ClientBaseTestFixture
   {
      [Ignore("Not yet ready for these to work.")]
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
               logger.LogInformation("Connection create failed due to: ", ex);
               Assert.IsTrue(ex.InnerException is ClientException);
            }

            peer.WaitForScriptToComplete();
         }
      }
   }
}