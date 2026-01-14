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

using System.Collections.Generic;
using System.Threading;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Implementation
{
   [TestFixture, Timeout(20000)]
   public class ClientDeliveryStateTest
   {
      [Test]
      public void TestAcceptedType()
      {
         IDeliveryState deliveryState = IDeliveryState.Accepted();

         Assert.NotNull(deliveryState);
         Assert.IsTrue(deliveryState.IsAccepted);
         Assert.IsFalse(deliveryState.IsModified);
         Assert.IsFalse(deliveryState.IsRejected);
         Assert.IsFalse(deliveryState.IsReleased);
         Assert.IsFalse(deliveryState.IsTransactional);
         Assert.AreEqual(DeliveryStateType.Accepted, deliveryState.Type);

         Assert.IsInstanceOf<IAccepted>(deliveryState);
      }

      [Test]
      public void TestReleasedType()
      {
         IDeliveryState deliveryState = IDeliveryState.Released();

         Assert.NotNull(deliveryState);
         Assert.IsFalse(deliveryState.IsAccepted);
         Assert.IsFalse(deliveryState.IsModified);
         Assert.IsFalse(deliveryState.IsRejected);
         Assert.IsTrue(deliveryState.IsReleased);
         Assert.IsFalse(deliveryState.IsTransactional);
         Assert.AreEqual(DeliveryStateType.Released, deliveryState.Type);

         Assert.IsInstanceOf<IReleased>(deliveryState);
      }

      [Test]
      public void TestRejectedType()
      {
         IDeliveryState deliveryState = IDeliveryState.Rejected("amqp:error", "error");

         Assert.NotNull(deliveryState);
         Assert.IsFalse(deliveryState.IsAccepted);
         Assert.IsFalse(deliveryState.IsModified);
         Assert.IsTrue(deliveryState.IsRejected);
         Assert.IsFalse(deliveryState.IsReleased);
         Assert.IsFalse(deliveryState.IsTransactional);
         Assert.AreEqual(DeliveryStateType.Rejected, deliveryState.Type);

         IRejected rejected = deliveryState as IRejected;

         Assert.AreEqual("amqp:error", rejected.Condition);
         Assert.AreEqual("error", rejected.Description);
         Assert.IsNull(rejected.Info);

         Assert.IsInstanceOf<IRejected>(deliveryState);
      }

      [Test]
      public void TestModifiedType()
      {
         IDeliveryState deliveryState = IDeliveryState.Modified(true, true);

         Assert.NotNull(deliveryState);
         Assert.IsFalse(deliveryState.IsAccepted);
         Assert.IsTrue(deliveryState.IsModified);
         Assert.IsFalse(deliveryState.IsRejected);
         Assert.IsFalse(deliveryState.IsReleased);
         Assert.IsFalse(deliveryState.IsTransactional);
         Assert.AreEqual(DeliveryStateType.Modified, deliveryState.Type);

         Assert.IsInstanceOf<IModified>(deliveryState);
      }

      [Test]
      public void TestTransactionalWithReleased()
      {
         ITransactional state = new ClientTransactional(CreateTransactional(Released.Instance));

         Assert.IsNotNull(state);
         Assert.AreEqual(state.Type, DeliveryStateType.Transactional);
         Assert.IsFalse(state.IsAccepted);
         Assert.IsFalse(state.IsRejected);
         Assert.IsTrue(state.IsReleased);
         Assert.IsFalse(state.IsModified);
         Assert.IsTrue(state.IsTransactional);
         Assert.IsTrue(state.Outcome is IReleased);
      }

      [Test]
      public void TestTransactionalWithModified()
      {
         IDictionary<Symbol, object> symbolicAnnotations = new Dictionary<Symbol, object>();
         symbolicAnnotations.Add("test", "value");
         IDictionary<string, object> annotations = new Dictionary<string, object>();
         annotations.Add("test", "value");

         ITransactional state = new ClientTransactional(CreateTransactional(new Modified(true, true, symbolicAnnotations)));

         Assert.IsNotNull(state);
         Assert.AreEqual(state.Type, DeliveryStateType.Transactional);
         Assert.IsFalse(state.IsAccepted);
         Assert.IsFalse(state.IsRejected);
         Assert.IsFalse(state.IsReleased);
         Assert.IsTrue(state.IsModified);
         Assert.IsTrue(state.IsTransactional);
         Assert.IsTrue(state.Outcome is IModified);

         IModified modifiedState = state.Outcome as IModified;

         Assert.IsNotNull(modifiedState);
         Assert.IsTrue(modifiedState.DeliveryFailed);
         Assert.IsTrue(modifiedState.UndeliverableHere);
         Assert.AreEqual(annotations, modifiedState.MessageAnnotations);
      }

      [Test]
      public void TestTransactionalWithRejected()
      {
         IDictionary<Symbol, object> symbolicInfo = new Dictionary<Symbol, object>();
         symbolicInfo.Add("test", "value");
         IDictionary<string, object> info = new Dictionary<string, object>();
         info.Add("test", "value");

         ITransactional state = new ClientTransactional(CreateTransactional(new Rejected(new ErrorCondition("test", "data", symbolicInfo))));

         Assert.IsNotNull(state);
         Assert.AreEqual(state.Type, DeliveryStateType.Transactional);
         Assert.IsFalse(state.IsAccepted);
         Assert.IsTrue(state.IsRejected);
         Assert.IsFalse(state.IsReleased);
         Assert.IsFalse(state.IsModified);
         Assert.IsTrue(state.IsTransactional);
         Assert.IsTrue(state.Outcome is IRejected);

         IRejected rejectedState = state.Outcome as IRejected;

         Assert.IsNotNull(rejectedState);
         Assert.AreEqual("test", rejectedState.Condition);
         Assert.AreEqual("data", rejectedState.Description);
         Assert.AreEqual(info, rejectedState.Info);
      }

      private TransactionalState CreateTransactional(IOutcome outcome)
      {
         TransactionalState txnState = new TransactionalState();

         txnState.TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3 });
         txnState.Outcome = outcome;

         return txnState;
      }
   }
}