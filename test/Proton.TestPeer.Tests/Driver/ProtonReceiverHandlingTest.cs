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
   public class ProtonReceiverHandlingTest : ProtonBaseTestFixture
   {
      [Test]
      public void TestReceiverTrackingWithClientOpensReceiver()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0).Respond();
            peer.ExpectAttach().OfReceiver().WithHandle(0).OnChannel(0).Respond();
            peer.ExpectEnd().OnChannel(0).Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin().OnChannel(0);
            client.ExpectAttach().OfSender().OnChannel(0).WithHandle(0);
            client.ExpectEnd().OnChannel(0);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfReceiver().Now();
            client.RemoteEnd().Now();
            client.WaitForScriptToComplete();
            client.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestReceiverTrackingWithClientOpensReceiverWithDelayedResponses()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().OnChannel(0).Respond();
            peer.ExpectAttach().OfReceiver().WithHandle(0).OnChannel(0).Respond();
            peer.ExpectEnd().OnChannel(0).Respond().AfterDelay(50);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin().OnChannel(0);
            client.ExpectAttach().OfSender().OnChannel(0).WithHandle(0);
            client.ExpectEnd().OnChannel(0);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfReceiver().Now();
            client.RemoteEnd().Now();
            client.WaitForScriptToComplete();
            client.Close();

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestAttachResponseUsesScriptedChannel()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond().WithHandle(42);
            peer.ExpectEnd().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin();
            client.ExpectAttach().OfReceiver().WithHandle(42);
            client.ExpectEnd();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfSender().Now();
            client.RemoteEnd().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestWaitForCompletionFailsWhenRemoteSendDetachWithWrongHandle()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond().WithHandle(42);
            peer.ExpectDetach().Respond().WithHandle(43);
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin();
            client.ExpectAttach().OfSender().WithHandle(42);
            client.ExpectDetach().WithHandle(42);
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfReceiver().Now();
            client.RemoteDetach().Now();

            Assert.Throws<AssertionError>(() => client.WaitForScriptToComplete(TimeSpan.FromSeconds(30)));

            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestServerDetachResponseFillsHandlesAutomaticallyIfNoneSpecified()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond().WithHandle(42);
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin();
            client.ExpectAttach().OfSender().WithHandle(42);
            client.ExpectDetach().WithHandle(42);
            client.ExpectEnd();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfReceiver().Now();
            client.RemoteDetach().Now();
            client.RemoteEnd().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestServerRespondToLastAttachFeature()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfReceiver().Now();

            // Wait for the above and then script next steps
            client.WaitForScriptToComplete();
            client.ExpectAttach().OfSender();

            // Now we respond to the last begin we saw at the server side.
            peer.WaitForScriptToComplete();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.RespondToLastAttach().Now();

            // Wait for the above and then script next steps
            client.WaitForScriptToComplete();
            client.ExpectDetach();
            client.ExpectEnd();
            client.RemoteDetach().Now();
            client.RemoteEnd().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestOpenAndCloseMultipleLinksWithAutoChannelHandlingExpected()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().WithHandle(0).Respond();
            peer.ExpectAttach().OfReceiver().WithHandle(1).Respond();
            peer.ExpectAttach().OfReceiver().WithHandle(2).Respond();
            peer.ExpectDetach().WithHandle(2).Respond();
            peer.ExpectDetach().WithHandle(1).Respond();
            peer.ExpectDetach().WithHandle(0).Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin();
            client.ExpectAttach().OfSender().WithHandle(0);
            client.ExpectAttach().OfSender().WithHandle(1);
            client.ExpectAttach().OfSender().WithHandle(2);
            client.Connect(remoteAddress, remotePort);

            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfReceiver().Now();
            client.RemoteAttach().OfReceiver().Now();
            client.RemoteAttach().OfReceiver().Now();
            client.WaitForScriptToComplete();
            client.ExpectDetach().WithHandle(2);
            client.ExpectDetach().WithHandle(1);
            client.ExpectDetach().WithHandle(0);
            client.ExpectEnd();

            client.RemoteDetach().WithHandle(2).Now();
            client.RemoteDetach().WithHandle(1).Now();
            client.RemoteDetach().WithHandle(0).Now();
            client.RemoteEnd().Now();
            client.WaitForScriptToComplete();
            client.ExpectClose();

            client.RemoteClose().Now();

            client.WaitForScriptToComplete();
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestPeerEndsConnectionIfRemoteRespondsWithToHighHandleValue()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().WithHandleMax(0).Respond();
            peer.ExpectAttach().OfReceiver();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.ExpectOpen();
            client.ExpectBegin();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().WithHandleMax(0).Now();
            client.RemoteAttach().OfReceiver().Now();

            // Wait for the above and then script next steps
            client.WaitForScriptToComplete();
            client.ExpectAttach().OfSender();

            // Now we respond to the last attach we saw at the server side.
            peer.WaitForScriptToComplete();
            peer.RespondToLastAttach().WithHandle(42).Now();

            Assert.Throws<AssertionError>(() => client.WaitForScriptToComplete());
            peer.WaitForScriptToComplete();
         }
      }

      [Test]
      public void TestPeerEnforcesHandleMaxOfZeroOnPipelinedOpenBeginAttach()
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         using (ProtonTestClient client = new ProtonTestClient(loggerFactory))
         {
            peer.ExpectAMQPHeader().RespondWithAMQPHeader();
            peer.ExpectOpen();
            peer.ExpectBegin();
            peer.ExpectAttach().OfReceiver();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            client.Connect(remoteAddress, remotePort);
            client.ExpectAMQPHeader();
            client.RemoteHeader(AMQPHeader.Header).Now();
            client.RemoteOpen().Now();
            client.RemoteBegin().Now();
            client.RemoteAttach().OfReceiver().WithHandle(42)
                                              .WithSource().WithCapabilities("QUEUE")
                                              .And().Now();

            // Wait for the above and then script next steps
            Assert.DoesNotThrow(() => client.WaitForScriptToComplete());

            logger.LogInformation("Test finished with client expectations, now awaiting server fail");

            Assert.Throws<AssertionError>(() => peer.WaitForScriptToComplete());
         }
      }
   }
}