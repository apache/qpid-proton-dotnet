
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
using Microsoft.Extensions.Logging;

using System.Net.Security;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientSslConnectionTest : ClientBaseTestFixture
   {
      [Ignore("Failed in CI due to server certificate not found.")]
      [Test]
      public void TestCreateConnectionString()
      {
         ProtonTestServerOptions serverOptions = new ProtonTestServerOptions();
         serverOptions.SslEnabled = true;
         serverOptions.ServerCertificatePath = "./Certificates/server.pfx";
         serverOptions.ServerCertificatePassword = "password";

         using (ProtonTestServer peer = new ProtonTestServer(serverOptions, loggerFactory))
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
            IConnection connection = container.Connect("localhost", remotePort, options);

            _ = connection.OpenTask.Wait(TimeSpan.FromSeconds(10));
            _ = connection.CloseAsync().Wait(TimeSpan.FromSeconds(10));

            peer.WaitForScriptToComplete();
         }
      }

      protected ConnectionOptions ConnectionOptions(string user, string password)
      {
         ConnectionOptions options = new ConnectionOptions();
         options.User = user;
         options.Password = password;
         options.SslOptions.SslEnabled = true;
         options.SslOptions.AllowedSslPolicyErrorsOverride = SslPolicyErrors.RemoteCertificateChainErrors;

         return options;
      }
   }
}