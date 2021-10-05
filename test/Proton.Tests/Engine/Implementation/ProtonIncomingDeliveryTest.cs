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

using System.Text;
using Apache.Qpid.Proton.Test.Driver;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ProtonIncomingDeliveryTest : ProtonEngineTestSupport
   {
      public static int DEFAULT_MESSAGE_FORMAT = 0;

      [Test]
      public void TestToStringOnEmptyDeliveryDoesNotNPE()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test");
         IIncomingDelivery receivedDelivery = null;

         receiver.DeliveryReadHandler(delivery =>
         {
            receivedDelivery = delivery;
         });

         receiver.Open();
         receiver.AddCredit(1);

         Assert.IsNotNull(receivedDelivery.ToString());
      }

      [Test]
      public void TestDefaultMessageFormat()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test");
         IIncomingDelivery receivedDelivery = null;

         receiver.DeliveryReadHandler(delivery =>
         {
            receivedDelivery = delivery;
         });

         receiver.Open();
         receiver.AddCredit(1);

         Assert.AreEqual(0L, DEFAULT_MESSAGE_FORMAT, "Unexpected value");
         Assert.AreEqual(DEFAULT_MESSAGE_FORMAT, receivedDelivery.MessageFormat, "Unexpected message format");
      }

      [Test]
      public void TestAvailable()
      {
         byte[] data = Encoding.UTF8.GetBytes("test-data");

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithPayload(data)
                              .Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test");
         IIncomingDelivery receivedDelivery = null;

         receiver.DeliveryReadHandler(delivery =>
         {
            receivedDelivery = delivery;
         });

         receiver.Open();
         receiver.AddCredit(1);

         Assert.IsNotNull(receivedDelivery, "expected the delivery to be present");
         Assert.AreEqual(data.Length, receivedDelivery.Available, "unexpected available count");

         // Extract some of the data as the receiver link will, check available gets reduced accordingly.
         int partLength = 2;
         int remainderLength = data.Length - partLength;
         Assert.IsTrue(partLength < data.Length);

         byte[] myReceivedData1 = new byte[partLength];

         receivedDelivery.ReadBytes(myReceivedData1, 0, myReceivedData1.Length);
         Assert.AreEqual(remainderLength, receivedDelivery.Available, "Unexpected data.Length available");

         // Extract remainder of the data as the receiver link will, check available hits 0.
         byte[] myReceivedData2 = new byte[remainderLength];

         receivedDelivery.ReadBytes(myReceivedData2, 0, remainderLength);
         Assert.AreEqual(0, receivedDelivery.Available, "Expected no data to remain available");
      }

      [Test]
      public void TestAvailableWhenEmpty()
      {
         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false).Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test");
         IIncomingDelivery receivedDelivery = null;

         receiver.DeliveryReadHandler(delivery =>
         {
            receivedDelivery = delivery;
         });

         receiver.Open();
         receiver.AddCredit(1);

         Assert.AreEqual(0, receivedDelivery.Available);
      }

      [Test]
      public void TestClaimAvailableBytesIndicatesAllBytesRead()
      {
         byte[] data = new byte[1024];
         rand.NextBytes(data);

         IEngine engine = IEngineFactory.Proton.CreateNonSaslEngine();
         engine.ErrorHandler((result) => failure = result.FailureCause);
         ProtonTestConnector peer = CreateTestPeer(engine);

         peer.ExpectAMQPHeader().RespondWithAMQPHeader();
         peer.ExpectOpen().Respond().WithContainerId("driver");
         peer.ExpectBegin().Respond();
         peer.ExpectAttach().Respond();
         peer.ExpectFlow().WithLinkCredit(1);
         peer.RemoteTransfer().WithDeliveryId(0)
                              .WithDeliveryTag(new byte[] { 0 })
                              .WithMore(false)
                              .WithPayload(data)
                              .Queue();

         IConnection connection = engine.Start().Open();
         ISession session = connection.Session().Open();
         IReceiver receiver = session.Receiver("test");
         IIncomingDelivery receivedDelivery = null;

         receiver.DeliveryReadHandler(delivery =>
         {
            receivedDelivery = delivery;
         });

         receiver.Open();
         receiver.AddCredit(1);

         Assert.AreEqual(1024, receivedDelivery.Available);
         Assert.AreSame(receivedDelivery, receivedDelivery.ClaimAvailableBytes());
         Assert.AreEqual(1024, receivedDelivery.Available);
      }
   }
}