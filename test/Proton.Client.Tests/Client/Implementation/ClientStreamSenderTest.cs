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
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging;
using Apache.Qpid.Proton.Test.Driver.Matchers.Types.Transport;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientStreamSenderTest : ClientBaseTestFixture
   {
      [Test]
      public void TestSendWhenCreditIsAvailable()
      {
         DoTestSendWhenCreditIsAvailable(false, false);
      }

      [Test]
      public void TestTrySendWhenCreditIsAvailable()
      {
         DoTestSendWhenCreditIsAvailable(true, false);
      }

      [Test]
      public void TestSendWhenCreditIsAvailableWithDeliveryAnnotations()
      {
         DoTestSendWhenCreditIsAvailable(false, true);
      }

      [Test]
      public void TestTrySendWhenCreditIsAvailableWithDeliveryAnnotations()
      {
         DoTestSendWhenCreditIsAvailable(true, true);
      }

      private void DoTestSendWhenCreditIsAvailable(bool trySend, bool addDeliveryAnnotations)
      {
         using (ProtonTestServer peer = new ProtonTestServer(loggerFactory))
         {
            peer.ExpectSASLAnonymousConnect();
            peer.ExpectOpen().Respond();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfSender().Respond();
            peer.RemoteFlow().WithDeliveryCount(0)
                             .WithLinkCredit(10)
                             .WithIncomingWindow(1024)
                             .WithOutgoingWindow(10)
                             .WithNextIncomingId(0)
                             .WithNextOutgoingId(1).Queue();
            peer.ExpectBegin().Respond();
            peer.ExpectAttach().OfReceiver().Respond();
            peer.Start();

            string remoteAddress = peer.ServerAddress;
            int remotePort = peer.ServerPort;

            logger.LogInformation("Test started, peer listening on: {0}:{1}", remoteAddress, remotePort);

            IClient container = IClient.Create();
            IConnection connection = container.Connect(remoteAddress, remotePort);
            IStreamSender sender = connection.OpenStreamSender("test-queue");
            sender.OpenTask.Wait();

            // This ensures that the flow to sender is processed before we try-send
            IReceiver receiver = connection.OpenReceiver("test-queue", new ReceiverOptions()
            {
               CreditWindow = 0
            });
            receiver.OpenTask.Wait();

            IDictionary<string, object> deliveryAnnotations = new Dictionary<string, object>();
            deliveryAnnotations.Add("da1", 1);
            deliveryAnnotations.Add("da2", 2);
            deliveryAnnotations.Add("da3", 3);
            DeliveryAnnotationsMatcher daMatcher = new DeliveryAnnotationsMatcher(true);
            daMatcher.WithEntry("da1", Test.Driver.Matchers.Is.EqualTo(1));
            daMatcher.WithEntry("da2", Test.Driver.Matchers.Is.EqualTo(2));
            daMatcher.WithEntry("da3", Test.Driver.Matchers.Is.EqualTo(3));
            AmqpValueMatcher bodyMatcher = new AmqpValueMatcher("Hello World");
            TransferPayloadCompositeMatcher payloadMatcher = new TransferPayloadCompositeMatcher();
            if (addDeliveryAnnotations)
            {
               payloadMatcher.DeliveryAnnotationsMatcher = daMatcher;
            }
            payloadMatcher.MessageContentMatcher = bodyMatcher;

            peer.WaitForScriptToComplete();
            peer.ExpectTransfer().WithNonNullPayload();
            peer.ExpectDetach().Respond();
            peer.ExpectEnd().Respond();
            peer.ExpectClose().Respond();

            IMessage<string> message = IMessage<string>.Create("Hello World");

            ITracker tracker;
            if (trySend)
            {
               if (addDeliveryAnnotations)
               {
                  tracker = sender.TrySend(message, deliveryAnnotations);
               }
               else
               {
                  tracker = sender.TrySend(message);
               }
            }
            else
            {
               if (addDeliveryAnnotations)
               {
                  tracker = sender.Send(message, deliveryAnnotations);
               }
               else
               {
                  tracker = sender.Send(message);
               }
            }

            Assert.IsNotNull(tracker);

            sender.Close();
            connection.Close();

            peer.WaitForScriptToComplete();
         }
      }

   }
}