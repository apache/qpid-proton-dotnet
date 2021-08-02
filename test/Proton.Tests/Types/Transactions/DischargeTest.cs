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
using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class DischargeTests
   {
      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new Discharge().ToString());
      }

      [Test]
      public void TestTxnId()
      {
         IProtonBuffer txnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         Discharge discharge = new Discharge();

         Assert.IsNull(discharge.TxnId);
         discharge.TxnId = txnId;
         Assert.IsNotNull(discharge.TxnId);

         try
         {
            discharge.TxnId = null;
            Assert.Fail("The TXN field is mandatory and cannot be set to null");
         }
         catch (ArgumentNullException) { }
      }
   }
}