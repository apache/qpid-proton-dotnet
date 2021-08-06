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

using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class TransferTest
   {
      [Test]
      public void TestGetPerformativeType()
      {
         Assert.AreEqual(PerformativeType.Transfer, new Transfer().Type);
      }

      [Test]
      public void TestToStringOnFreshInstance()
      {
         Assert.IsNotNull(new Transfer().ToString());
      }

      [Test]
      public void TestInitialState()
      {
         Transfer transfer = new Transfer();

         Assert.AreEqual(0, transfer.GetElementCount());
         Assert.IsTrue(transfer.IsEmpty());
         Assert.IsFalse(transfer.HasAborted());
         Assert.IsFalse(transfer.HasBatchable());
         Assert.IsFalse(transfer.HasDeliveryId());
         Assert.IsFalse(transfer.HasDeliveryTag());
         Assert.IsFalse(transfer.HasHandle());
         Assert.IsFalse(transfer.HasMessageFormat());
         Assert.IsFalse(transfer.HasMore());
         Assert.IsFalse(transfer.HasReceiverSettleMode());
         Assert.IsFalse(transfer.HasResume());
         Assert.IsFalse(transfer.HasSettled());
         Assert.IsFalse(transfer.HasState());
      }

      [Test]
      public void TestClearFieldsAPI()
      {
         Transfer transfer = new Transfer();

         transfer.Aborted = true;
         transfer.Batchable = true;
         transfer.DeliveryId = 1;
         transfer.DeliveryTag = new DeliveryTag(new byte[] { 1 });
         transfer.Handle = 2;
         transfer.MessageFormat = 12;
         transfer.More = true;
         transfer.ReceiverSettleMode = ReceiverSettleMode.Second;
         transfer.Resume = true;
         transfer.Settled = true;
         transfer.DeliveryState = Accepted.Instance;

         Assert.IsNotNull(transfer.ToString());
         Assert.AreEqual(11, transfer.GetElementCount());
         Assert.IsFalse(transfer.IsEmpty());
         Assert.IsTrue(transfer.HasAborted());
         Assert.IsTrue(transfer.HasBatchable());
         Assert.IsTrue(transfer.HasDeliveryId());
         Assert.IsTrue(transfer.HasDeliveryTag());
         Assert.IsTrue(transfer.HasHandle());
         Assert.IsTrue(transfer.HasMessageFormat());
         Assert.IsTrue(transfer.HasMore());
         Assert.IsTrue(transfer.HasReceiverSettleMode());
         Assert.IsTrue(transfer.HasResume());
         Assert.IsTrue(transfer.HasSettled());
         Assert.IsTrue(transfer.HasState());

         transfer.ClearAborted();
         transfer.ClearBatchable();
         transfer.ClearDeliveryId();
         transfer.ClearDeliveryTag();
         transfer.ClearHandle();
         transfer.ClearMessageFormat();
         transfer.ClearMore();
         transfer.ClearReceiverSettleMode();
         transfer.ClearResume();
         transfer.ClearSettled();
         transfer.ClearState();

         Assert.AreEqual(0, transfer.GetElementCount());
         Assert.IsTrue(transfer.IsEmpty());
         Assert.IsFalse(transfer.HasAborted());
         Assert.IsFalse(transfer.HasBatchable());
         Assert.IsFalse(transfer.HasDeliveryId());
         Assert.IsFalse(transfer.HasDeliveryTag());
         Assert.IsFalse(transfer.HasHandle());
         Assert.IsFalse(transfer.HasMessageFormat());
         Assert.IsFalse(transfer.HasMore());
         Assert.IsFalse(transfer.HasReceiverSettleMode());
         Assert.IsFalse(transfer.HasResume());
         Assert.IsFalse(transfer.HasSettled());
         Assert.IsFalse(transfer.HasState());

         transfer.DeliveryTag = new DeliveryTag(new byte[] { 1 });
         Assert.IsTrue(transfer.HasDeliveryTag());
         transfer.DeliveryTag = null;
         Assert.IsFalse(transfer.HasDeliveryTag());

         transfer.ReceiverSettleMode = ReceiverSettleMode.Second;
         Assert.IsTrue(transfer.HasReceiverSettleMode());
         transfer.ClearReceiverSettleMode();
         Assert.IsFalse(transfer.HasReceiverSettleMode());
      }

      [Test]
      public void TestCopy()
      {
         Transfer transfer = new Transfer();

         transfer.Aborted = true;
         transfer.Batchable = true;
         transfer.DeliveryId = 1;
         transfer.DeliveryTag = new DeliveryTag(new byte[] { 1 });
         transfer.Handle = 2;
         transfer.MessageFormat = 12;
         transfer.More = true;
         transfer.ReceiverSettleMode = ReceiverSettleMode.Second;
         transfer.Resume = true;
         transfer.Settled = true;
         transfer.DeliveryState = Accepted.Instance;

         Transfer copy = transfer.Copy();

         Assert.AreEqual(transfer.Aborted, copy.Aborted);
         Assert.AreEqual(transfer.Batchable, copy.Batchable);
         Assert.AreEqual(transfer.DeliveryId, copy.DeliveryId);
         Assert.AreEqual(transfer.DeliveryTag, copy.DeliveryTag);
         Assert.AreEqual(transfer.Handle, copy.Handle);
         Assert.AreEqual(transfer.MessageFormat, copy.MessageFormat);
         Assert.AreEqual(transfer.More, copy.More);
         Assert.AreEqual(transfer.ReceiverSettleMode, copy.ReceiverSettleMode);
         Assert.AreEqual(transfer.Resume, copy.Resume);
         Assert.AreEqual(transfer.Settled, copy.Settled);
         Assert.AreEqual(transfer.DeliveryState, copy.DeliveryState);
      }

      [Test]
      public void TestIsEmpty()
      {
         Transfer transfer = new Transfer();

         Assert.AreEqual(0, transfer.GetElementCount());
         Assert.IsTrue(transfer.IsEmpty());
         Assert.IsFalse(transfer.HasAborted());

         transfer.Aborted = true;

         Assert.IsTrue(transfer.GetElementCount() > 0);
         Assert.IsFalse(transfer.IsEmpty());
         Assert.IsTrue(transfer.HasAborted());
         Assert.IsTrue(transfer.Aborted);

         transfer.Aborted = false;

         Assert.IsNotNull(transfer.ToString());
         Assert.IsTrue(transfer.GetElementCount() > 0);
         Assert.IsFalse(transfer.IsEmpty());
         Assert.IsTrue(transfer.HasAborted());
         Assert.IsFalse(transfer.Aborted);
      }

      [Test]
      public void TestCopyFromNew()
      {
         Transfer original = new Transfer();
         Transfer copy = original.Copy();

         Assert.IsTrue(original.IsEmpty());
         Assert.IsTrue(copy.IsEmpty());

         Assert.AreEqual(0, original.GetElementCount());
         Assert.AreEqual(0, copy.GetElementCount());
      }
   }
}