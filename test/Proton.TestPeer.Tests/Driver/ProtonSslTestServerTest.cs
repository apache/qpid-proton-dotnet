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

using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using NUnit.Framework;
using Microsoft.Extensions.Logging;
using System.Net.Security;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture, Timeout(20000)]
   public class ProtonSslTestserverTest : ProtonBaseTestFixture
   {
      [Ignore("Tests in CI can't find files")]
      [Test]
      public void TestClientCanConnectToSecureServer()
      {
         ProtonTestServerOptions serverOptions = new ProtonTestServerOptions();

         serverOptions.SslEnabled = true;
         serverOptions.ServerCertificatePath = "./Certificates/server.pfx";
         serverOptions.ServerCertificatePassword = "password";

         ProtonTestClientOptions clientOptions = new ProtonTestClientOptions();
         clientOptions.SslEnabled = true;
         clientOptions.ClientCertificatePath = "./Certificates/client.pfx";
         clientOptions.ClientCertificatePassword = "password";
         clientOptions.AllowedSslPolicyErrorsOverride = SslPolicyErrors.RemoteCertificateChainErrors; // Self signed

         using (ProtonTestServer peer = new ProtonTestServer(serverOptions, loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient(clientOptions, loggerFactory);

            // localhost matches name in server certificate
            client.Connect("localhost", remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectClose();

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteClose().Now();
            client.WaitForScriptToComplete();

            client.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}