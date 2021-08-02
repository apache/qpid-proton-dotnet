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
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class ReceiverSettleModeTest
   {
      [Test]
      public void TestValueOf()
      {
         Assert.AreEqual(ReceiverSettleMode.First, ReceiverSettleModeExtension.Lookup(0));
         Assert.AreEqual(ReceiverSettleMode.Second, ReceiverSettleModeExtension.Lookup(1));
      }

      [Test]
      public void TestEquality()
      {
         ReceiverSettleMode first = ReceiverSettleMode.First;
         ReceiverSettleMode second = ReceiverSettleMode.Second;

         Assert.AreEqual(first, ReceiverSettleModeExtension.Lookup((byte)0));
         Assert.AreEqual(second, ReceiverSettleModeExtension.Lookup((byte)1));

         Assert.AreEqual(first.ToByteValue(), (byte)0);
         Assert.AreEqual(second.ToByteValue(), (byte)1);
      }

      [Test]
      public void TestNotEquality()
      {
         ReceiverSettleMode first = ReceiverSettleMode.First;
         ReceiverSettleMode second = ReceiverSettleMode.Second;

         Assert.AreNotEqual(first, ReceiverSettleModeExtension.Lookup(1));
         Assert.AreNotEqual(second, ReceiverSettleModeExtension.Lookup(0));

         Assert.AreNotEqual(first.ToByteValue(), (byte)1);
         Assert.AreNotEqual(second.ToByteValue(), (byte)0);
      }

      [Test]
      public void TestIllegalArgument()
      {
         Assert.Throws<ArgumentOutOfRangeException>(() => ReceiverSettleModeExtension.Lookup((byte)2));
      }
   }
}