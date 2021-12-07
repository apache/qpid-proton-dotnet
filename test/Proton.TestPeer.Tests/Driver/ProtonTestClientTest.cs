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

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture, Timeout(20000)]
   public class ProtonTestClientTest : ProtonBaseTestFixture
   {
      [Test]
      public void TestClientCanConnectAndExchangeAMQPHeaders()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient(loggerFactory);

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

            ProtonTestClient client = new ProtonTestClient(loggerFactory);

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

            ProtonTestClient client = new ProtonTestClient(loggerFactory);

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

      [Test]
      public void TestClientCanConnectAndOpenExchanged()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient(loggerFactory);

            client.Connect(remoteAddress, remotePort);
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

      [Test]
      public void TestClientFailsTestIfFrameSizeExpectationNotMet()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient(loggerFactory);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen().WithFrameSize(4096);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            Assert.Throws<AssertionError>(() => client.WaitForScriptToComplete());

            client.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestClientSendPipelinedHeaderAndOpen()
      {
         byte[] basicOpen = new byte[] {(byte)'A', (byte)'M', (byte)'Q', (byte)'P', 0, 1, 0, 0, // HEADER
                                        0, 0, 0, 49, 2, 0, 0, 0, 0, 83, 16, 192, 36, 5, 161, 9, 99, 111,
                                        110, 116, 97, 105, 110, 101, 114, 161, 9, 108, 111, 99, 97, 108,
                                        104, 111, 115, 116, 112, 0, 0, 64, 0, 96, 255, 255, 112, 0, 0, 117, 48};

         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            ProtonTestClient client = new ProtonTestClient(loggerFactory);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.RemoteBytes().WithBytes(basicOpen).Now();

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            Assert.DoesNotThrow(() => client.WaitForScriptToComplete());

            client.Close();

            peer.WaitForScriptToComplete();
         }
      }
   }
}