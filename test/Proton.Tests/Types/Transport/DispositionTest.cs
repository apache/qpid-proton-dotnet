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
   public class DispositionTest
   {
      [Test]
      public void TestGetPerformativeType()
      {
         Assert.AreEqual(PerformativeType.Disposition, new Disposition().Type);
      }

      [Test]
      public void TestToStringOnFreshInstance()
      {
         Assert.IsNotNull(new Disposition().ToString());
      }

      [Test]
      public void TestInitialState()
      {
         Disposition disposition = new Disposition();

         Assert.AreEqual(0, disposition.GetElementCount());
         Assert.IsTrue(disposition.IsEmpty());
         Assert.IsFalse(disposition.HasBatchable());
         Assert.IsFalse(disposition.HasFirst());
         Assert.IsFalse(disposition.HasLast());
         Assert.IsFalse(disposition.HasRole());
         Assert.IsFalse(disposition.HasSettled());
         Assert.IsFalse(disposition.HasState());
      }

      [Test]
      public void TestClearPayloadAPI()
      {
         Disposition disposition = new Disposition();

         disposition.Batchable = true;
         disposition.First = 1;
         disposition.Last = 2;
         disposition.Role = Role.Sender;
         disposition.Settled = true;
         disposition.State = Accepted.Instance;

         Assert.IsFalse(disposition.IsEmpty());
         Assert.IsTrue(disposition.HasBatchable());
         Assert.IsTrue(disposition.HasFirst());
         Assert.IsTrue(disposition.HasLast());
         Assert.IsTrue(disposition.HasRole());
         Assert.IsTrue(disposition.HasSettled());
         Assert.IsTrue(disposition.HasState());

         disposition.ClearBatchable();
         disposition.ClearFirst();
         disposition.ClearLast();
         disposition.ClearRole();
         disposition.ClearSettled();
         disposition.ClearState();

         Assert.AreEqual(0, disposition.GetElementCount());
         Assert.IsTrue(disposition.IsEmpty());
         Assert.IsFalse(disposition.HasBatchable());
         Assert.IsFalse(disposition.HasFirst());
         Assert.IsFalse(disposition.HasLast());
         Assert.IsFalse(disposition.HasRole());
         Assert.IsFalse(disposition.HasSettled());
         Assert.IsFalse(disposition.HasState());
      }

      [Test]
      public void TestIsEmpty()
      {
         Disposition disposition = new Disposition();

         Assert.AreEqual(0, disposition.GetElementCount());
         Assert.IsTrue(disposition.IsEmpty());
         Assert.IsFalse(disposition.HasFirst());

         disposition.First = 0;

         Assert.IsTrue(disposition.GetElementCount() > 0);
         Assert.IsFalse(disposition.IsEmpty());
         Assert.IsTrue(disposition.HasFirst());

         disposition.First = 1;

         Assert.IsTrue(disposition.GetElementCount() > 0);
         Assert.IsFalse(disposition.IsEmpty());
         Assert.IsTrue(disposition.HasFirst());
      }

      [Test]
      public void TestCopyFromNew()
      {
         Disposition original = new Disposition();
         Disposition copy = original.Copy();

         Assert.IsTrue(original.IsEmpty());
         Assert.IsTrue(copy.IsEmpty());

         Assert.AreEqual(0, original.GetElementCount());
         Assert.AreEqual(0, copy.GetElementCount());
      }
   }
}