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
using Apache.Qpid.Proton.Test.Driver;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientSaslConnectionTest : ClientBaseTestFixture
   {
      private static readonly string ANONYMOUS = "ANONYMOUS";
      private static readonly string PLAIN = "PLAIN";
      private static readonly string CRAM_MD5 = "CRAM-MD5";
      private static readonly string SCRAM_SHA_1 = "SCRAM-SHA-1";
      private static readonly string SCRAM_SHA_256 = "SCRAM-SHA-256";
      private static readonly string SCRAM_SHA_512 = "SCRAM-SHA-512";
      private static readonly string EXTERNAL = "EXTERNAL";
      private static readonly string XOAUTH2 = "XOAUTH2";

      private static readonly byte SASL_FAIL_AUTH = 1;
      private static readonly byte SASL_SYS = 2;
      private static readonly byte SASL_SYS_PERM = 3;
      private static readonly byte SASL_SYS_TEMP = 4;

      private static readonly string BROKER_JKS_KEYSTORE = "src/test/resources/broker-jks.keystore";
      private static readonly string BROKER_JKS_TRUSTSTORE = "src/test/resources/broker-jks.truststore";
      private static readonly string CLIENT_JKS_KEYSTORE = "src/test/resources/client-jks.keystore";
      private static readonly string CLIENT_JKS_TRUSTSTORE = "src/test/resources/client-jks.truststore";
      private static readonly string PASSWORD = "password";

      protected ProtonTestServerOptions ServerOptions()
      {
         return new ProtonTestServerOptions();
      }

      protected ConnectionOptions ConnectionOptions()
      {
         return new ConnectionOptions();
      }

      [Test]
      public void TestSaslLayerDisabledConnection()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            ConnectionOptions clientOptions = ConnectionOptions();
            clientOptions.SaslOptions.SaslEnabled = false;

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort, clientOptions);

            connection.OpenTask.Wait();

            // TODO: Peer requires SSL implementation first
            // Assert.IsFalse(peer.HasSecureConnection);
            // Assert.IsFalse(peer.ConnectionVerified);

            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

      // TODO Port all tests below the above which requires SSL
   }
}