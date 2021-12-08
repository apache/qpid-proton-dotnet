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
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver
{
   [TestFixture, Timeout(20000)]
   public class ProtonSessionHandlingTest : ProtonBaseTestFixture
   {
      [Test]
      public void TestSessionTrackingWithClientOpensSession()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0).Respond();
            peer.ExpectEnd().OnChannel(0).Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin().OnChannel(0);
            client.ExpectEnd().OnChannel(0);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteEnd().Now();
            client.WaitForScriptToComplete();
            client.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestSessionBeginResponseUsesScriptedChannel()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0).Respond().OnChannel(42);
            peer.ExpectEnd().OnChannel(0).Respond().OnChannel(42);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin().WithRemoteChannel(0).OnChannel(42);
            client.ExpectEnd().OnChannel(42);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteEnd().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestWaitForCompletionFailsWhenRemoteSendEndOnWrongChannel()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0).Respond().OnChannel(42);
            peer.ExpectEnd().OnChannel(0).Respond().OnChannel(43);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin().WithRemoteChannel(0).OnChannel(42);
            client.ExpectEnd().OnChannel(42);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteEnd().Now();

            Assert.Throws<AssertionError>(() => client.WaitForScriptToComplete(TimeSpan.FromSeconds(30)));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestServerEndResponseFillsChannelsAutomaticallyIfNoneSpecified()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0).Respond().OnChannel(42);
            peer.ExpectEnd().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin().WithRemoteChannel(0).OnChannel(42);
            client.ExpectEnd().OnChannel(42);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteEnd().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestServerRespondToLastBeginFeature()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();

            // Wait for the above and then script next steps
            client.WaitForScriptToComplete();
            client.ExpectBegin().WithRemoteChannel(0).OnChannel(42);

            // Now we respond to the last begin we saw at the server side.
            peer.WaitForScriptToComplete();
            peer.ExpectEnd().Respond();
            peer.RespondToLastBegin().OnChannel(42).Now();

            // Wait for the above and then script next steps
            client.WaitForScriptToComplete();
            client.ExpectEnd().OnChannel(42);
            client.RemoteEnd().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenAndCloseMultipleSessionsWithAutoChannelHandlingExpected()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0).Respond();
            peer.ExpectBegin().OnChannel(1).Respond();
            peer.ExpectBegin().OnChannel(2).Respond();
            peer.ExpectEnd().OnChannel(2).Respond();
            peer.ExpectEnd().OnChannel(1).Respond();
            peer.ExpectEnd().OnChannel(0).Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin().OnChannel(0);
            client.ExpectBegin().OnChannel(1);
            client.ExpectBegin().OnChannel(2);
            client.Connect(remoteAddress, remotePort);

            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteBegin().Now();
            client.RemoteBegin().Now();
            client.WaitForScriptToComplete();
            client.ExpectEnd().OnChannel(2);
            client.ExpectEnd().OnChannel(1);
            client.ExpectEnd().OnChannel(0);

            client.RemoteEnd().OnChannel(2).Now();
            client.RemoteEnd().OnChannel(1).Now();
            client.RemoteEnd().OnChannel(0).Now();
            client.WaitForScriptToComplete();
            client.ExpectClose();

            client.RemoteClose().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestPeerEndsConnectionIfRemoteRespondsWithToHighChannelValue()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().WithChannelMax(0).Respond();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().WithChannelMax(0).Now();
            client.RemoteBegin().Now();

            // Wait for the above and then script next steps
            client.WaitForScriptToComplete();
            client.ExpectBegin();

            // Now we respond to the last begin we saw at the server side.
            peer.WaitForScriptToComplete();
            peer.RespondToLastBegin().OnChannel(42).Now();

            Assert.Throws<AssertionError>(() => client.WaitForScriptToComplete());
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestPeerEnforcesChannelMaxOfZeroOnPipelinedOpenBegin()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen();
            peer.ExpectBegin();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().OnChannel(42).Now();

            // Wait for the above and then script next steps
            client.WaitForScriptToComplete();

            Assert.Throws<AssertionError>(() => peer.WaitForScriptToComplete());
         }
      }
   }
}