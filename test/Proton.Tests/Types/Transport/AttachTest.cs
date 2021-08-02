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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class AttachTest
   {
      [Test]
      public void TestGetPerformativeType()
      {
         Assert.AreEqual(PerformativeType.Attach, new Attach().Type);
      }

      [Test]
      public void TestToStringOnFreshInstance()
      {
         Assert.IsNotNull(new Attach().ToString());
      }

      [Test]
      public void TestInitialState()
      {
         Attach attach = new Attach();

         Assert.AreEqual(0, attach.GetElementCount());
         Assert.IsTrue(attach.IsEmpty());
         Assert.IsFalse(attach.HasDesiredCapabilities());
         Assert.IsFalse(attach.HasHandle());
         Assert.IsFalse(attach.HasIncompleteUnsettled());
         Assert.IsFalse(attach.HasInitialDeliveryCount());
         Assert.IsFalse(attach.HasMaxMessageSize());
         Assert.IsFalse(attach.HasName());
         Assert.IsFalse(attach.HasOfferedCapabilities());
         Assert.IsFalse(attach.HasProperties());
         Assert.IsFalse(attach.HasReceiverSettleMode());
         Assert.IsFalse(attach.HasRole());
         Assert.IsFalse(attach.HasSenderSettleMode());
         Assert.IsFalse(attach.HasSource());
         Assert.IsFalse(attach.HasTarget());
      }

      [Test]
      public void TestIsEmpty()
      {
         Attach attach = new Attach();

         Assert.AreEqual(0, attach.GetElementCount());
         Assert.IsTrue(attach.IsEmpty());
         Assert.IsFalse(attach.HasHandle());

         attach.Handle = 0;

         Assert.IsTrue(attach.GetElementCount() > 0);
         Assert.IsFalse(attach.IsEmpty());
         Assert.IsTrue(attach.HasHandle());

         attach.Handle = 1;

         Assert.IsTrue(attach.GetElementCount() > 0);
         Assert.IsFalse(attach.IsEmpty());
         Assert.IsTrue(attach.HasHandle());
      }

      [Test]
      public void TestSetNameRefusesNull()
      {
         try
         {
            new Attach().Name = null;
            Assert.Fail("Link name is mandatory");
         }
         catch (ArgumentNullException)
         {
         }
      }

      [Test]
      public void TestHasTargetOrCoordinator()
      {
         Attach attach = new Attach();

         Assert.IsFalse(attach.HasCoordinator());
         Assert.IsFalse(attach.HasTarget());
         Assert.IsFalse(attach.HasTargetOrCoordinator());

         attach.Target = new Target();

         Assert.IsFalse(attach.HasCoordinator());
         Assert.IsTrue(attach.HasTarget());
         Assert.IsTrue(attach.HasTargetOrCoordinator());

         attach.Target = new Coordinator();

         Assert.IsTrue(attach.HasCoordinator());
         Assert.IsFalse(attach.HasTarget());
         Assert.IsTrue(attach.HasTargetOrCoordinator());

         attach.Target = (Target)null;

         Assert.IsFalse(attach.HasCoordinator());
         Assert.IsFalse(attach.HasTarget());
         Assert.IsFalse(attach.HasTargetOrCoordinator());

         attach.Target = new Coordinator();

         Assert.IsTrue(attach.HasCoordinator());
         Assert.IsFalse(attach.HasTarget());
         Assert.IsTrue(attach.HasTargetOrCoordinator());
      }

      [Test]
      public void TestCopyAttachWithTarget()
      {
         Attach original = new Attach();

         original.Target = new Target();

         Attach copy = original.Copy();

         Assert.IsNotNull(copy.Target);
         Assert.AreNotSame(original.Target, copy.Target);
      }

      [Test]
      public void TestCopy()
      {
         IDictionary<IProtonBuffer, IDeliveryState> unsettled = new Dictionary<IProtonBuffer, IDeliveryState>();
         unsettled.Add(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 }), Accepted.Instance);

         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), "test1");

         Attach original = new Attach();

         original.DesiredCapabilities = new Symbol[] { Symbol.Lookup("queue") };
         original.OfferedCapabilities = new Symbol[] { Symbol.Lookup("queue"), Symbol.Lookup("topic") };
         original.Handle = 1;
         original.IncompleteUnsettled = true;
         original.Unsettled = unsettled;
         original.InitialDeliveryCount = 12;
         original.Name = "test";
         original.Target = new Target();
         original.Source = new Source();
         original.Role = Role.Receiver;
         original.SenderSettleMode = SenderSettleMode.Settled;
         original.ReceiverSettleMode = ReceiverSettleMode.Second;
         original.MaxMessageSize = 1024;
         original.Properties = properties;

         Assert.IsNotNull(original.ToString());  // Check no fumble on full populated fields.

         Attach copy = original.Copy();

         Assert.AreEqual(copy.DesiredCapabilities, copy.DesiredCapabilities);
         Assert.AreEqual(copy.OfferedCapabilities, copy.OfferedCapabilities);
         Assert.AreEqual(original.Target, copy.Target);
         Assert.AreEqual(original.IncompleteUnsettled, copy.IncompleteUnsettled);
         Assert.AreEqual(original.Unsettled, copy.Unsettled);
         Assert.AreEqual(original.InitialDeliveryCount, copy.InitialDeliveryCount);
         Assert.AreEqual(original.Name, copy.Name);
         Assert.AreEqual(original.Source, copy.Source);
         Assert.AreEqual(original.Role, copy.Role);
         Assert.AreEqual(original.SenderSettleMode, copy.SenderSettleMode);
         Assert.AreEqual(original.ReceiverSettleMode, copy.ReceiverSettleMode);
         Assert.AreEqual(original.MaxMessageSize, copy.MaxMessageSize);
         Assert.AreEqual(original.Properties, copy.Properties);
      }

      [Test]
      public void TestHasFields()
      {
         IDictionary<IProtonBuffer, IDeliveryState> unsettled = new Dictionary<IProtonBuffer, IDeliveryState>();
         unsettled.Add(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 }), Accepted.Instance);
         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), "test1");

         Attach original = new Attach();

         original.DesiredCapabilities = new Symbol[] { Symbol.Lookup("queue") };
         original.OfferedCapabilities = new Symbol[] { Symbol.Lookup("queue"), Symbol.Lookup("topic") };
         original.Handle = 1;
         original.IncompleteUnsettled = true;
         original.Unsettled = unsettled;
         original.InitialDeliveryCount = 12;
         original.Name = "test";
         original.Target = new Target();
         original.Source = new Source();
         original.Role = Role.Receiver;
         original.SenderSettleMode = SenderSettleMode.Settled;
         original.ReceiverSettleMode = ReceiverSettleMode.Second;
         original.MaxMessageSize = 1024;
         original.Properties = properties;

         Assert.IsTrue(original.HasDesiredCapabilities());
         Assert.IsTrue(original.HasOfferedCapabilities());
         Assert.IsTrue(original.HasHandle());
         Assert.IsTrue(original.HasIncompleteUnsettled());
         Assert.IsTrue(original.HasUnsettled());
         Assert.IsTrue(original.HasInitialDeliveryCount());
         Assert.IsTrue(original.HasName());
         Assert.IsTrue(original.HasTarget());
         Assert.IsFalse(original.HasCoordinator());
         Assert.IsTrue(original.HasSource());
         Assert.IsTrue(original.HasRole());
         Assert.IsTrue(original.HasSenderSettleMode());
         Assert.IsTrue(original.HasReceiverSettleMode());
         Assert.IsTrue(original.HasMaxMessageSize());
         Assert.IsTrue(original.HasProperties());

         original.Properties = null;
         original.Source = null;
         original.Target = (Coordinator)null;
         original.MaxMessageSize = 1;
         original.Unsettled = null;
         original.OfferedCapabilities = (Symbol[])null;
         original.DesiredCapabilities = (Symbol[])null;

         Assert.IsFalse(original.HasTarget());
         Assert.IsFalse(original.HasSource());
         Assert.IsTrue(original.HasMaxMessageSize());
         Assert.IsFalse(original.HasProperties());
         Assert.IsFalse(original.HasUnsettled());

         original.Target = new Coordinator();
         Assert.IsFalse(original.HasTarget());
         Assert.IsTrue(original.HasCoordinator());
         original.Target = null;
         Assert.IsFalse(original.HasTarget());
         Assert.IsFalse(original.HasCoordinator());
         Assert.IsFalse(original.HasDesiredCapabilities());
         Assert.IsFalse(original.HasOfferedCapabilities());
      }

      [Test]
      public void TestSetTargetAndCoordinatorThrowIllegalArguementErrorOnBadInAdd()
      {
         Attach original = new Attach();
         Assert.Throws<ArgumentException>(() => original.Target = new Source());
      }

      [Test]
      public void TestReplaceTargetWithCoordinator()
      {
         Attach original = new Attach();

         Assert.IsFalse(original.HasTarget());
         Assert.IsFalse(original.HasCoordinator());

         original.Target = new Target();

         Assert.IsTrue(original.HasTarget());
         Assert.IsFalse(original.HasCoordinator());

         original.Target = new Coordinator();

         Assert.IsFalse(original.HasTarget());
         Assert.IsTrue(original.HasCoordinator());
      }

      [Test]
      public void TestReplaceCoordinatorWithTarget()
      {
         Attach original = new Attach();

         Assert.IsFalse(original.HasTarget());
         Assert.IsFalse(original.HasCoordinator());

         original.Target = new Coordinator();

         Assert.IsFalse(original.HasTarget());
         Assert.IsTrue(original.HasCoordinator());

         original.Target = new Target();

         Assert.IsTrue(original.HasTarget());
         Assert.IsFalse(original.HasCoordinator());
      }

      [Test]
      public void TestCopyAttachWithCoordinator()
      {
         Attach original = new Attach();

         original.Target = new Coordinator();

         Attach copy = original.Copy();

         Assert.IsNotNull(copy.Target);
         Assert.AreEqual(original.Target, copy.Target, "Should be equal");

         Coordinator coordinator = (Coordinator)copy.Target;

         Assert.IsNotNull(coordinator);
         Assert.AreEqual(original.Target, coordinator);
      }

      [Test]
      public void TestCopyFromNew()
      {
         Attach original = new Attach();
         Attach copy = original.Copy();

         Assert.IsTrue(original.IsEmpty());
         Assert.IsTrue(copy.IsEmpty());

         Assert.AreEqual(0, original.GetElementCount());
         Assert.AreEqual(0, copy.GetElementCount());
      }
   }
}