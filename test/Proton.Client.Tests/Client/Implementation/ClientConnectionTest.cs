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

      protected ConnectionOptions ConnectionOptions(string user, string password)
      {
         ConnectionOptions options = new ConnectionOptions();
         options.User = user;
         options.Password = password;

         return options;
      }
   }
}