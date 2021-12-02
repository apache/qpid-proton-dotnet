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
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

using NLog.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver
{
   [Ignore("WIP")]
   [TestFixture]
   public class ProtonTestClientTest
   {
      private ILoggerFactory loggerFactory;
      private ILogger logger;

      [OneTimeSetUp]
      public void OneTimeSetup()
      {
         var config = new NLog.Config.LoggingConfiguration();

         // Targets where to log to: File and Console
         NLog.Targets.Target logfile = new NLog.Targets.FileTarget("logfile")
         {
            FileName = "./target/" + GetType().Name + ".txt"
         };
         NLog.Targets.Target logconsole = new NLog.Targets.ConsoleTarget("logconsole");

         // Rules for mapping loggers to targets
         config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logconsole);
         config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);

         loggerFactory = LoggerFactory.Create(builder =>
            builder.ClearProviders().AddNLog(config)
         );

         logger = loggerFactory.CreateLogger<ProtonTestClientTest>();
      }

      [Test]
      public void TestClientCanConnectAndExchangeAMQPHeaders()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient();

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.WaitForScriptToComplete();
            client.Close();

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestClientDetectsUnexpectedPerformativeResponseToAMQPHeader()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader();
            peer.RemoteOpen().Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient();

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.RemoteHeader(AMQPHeader.Header).Now();

            Assert.Throws<AssertionError>(() => client.WaitForScriptToComplete());

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestClientDetectsUnexpectedPerformativeAndFailsTest()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen();
            peer.RemoteBegin().Queue();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient();

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();

            Assert.Throws<UnexpectedPerformativeError>(() => client.WaitForScriptToComplete());

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}