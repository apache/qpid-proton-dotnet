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
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class ErrorConditionTest
   {
      [Test]
      public void TestToStringOnFreshInstance()
      {
         Assert.IsNotNull(new ErrorCondition(AmqpError.DECODE_ERROR, (string)null).ToString());
      }

      [Test]
      public void TestEqualsWithCreateFromStringVsCreateFromSymbolCondition()
      {
         ErrorCondition fromString = new ErrorCondition(AmqpError.DECODE_ERROR.ToString(), "error");
         ErrorCondition fromSymbol = new ErrorCondition(AmqpError.DECODE_ERROR, "error");

         Assert.AreEqual(fromString, fromSymbol);
      }

      [Test]
      public void TestEquals()
      {
         ErrorCondition original = new ErrorCondition(AmqpError.DECODE_ERROR, "error");
         ErrorCondition copy = original.Copy();

         Assert.AreEqual(original, copy);

         IDictionary<Symbol, object> infoMap = new Dictionary<Symbol, object>();
         ErrorCondition other1 = new ErrorCondition(null, "error", infoMap);
         ErrorCondition other2 = new ErrorCondition(AmqpError.DECODE_ERROR, null, infoMap);
         ErrorCondition other3 = new ErrorCondition(AmqpError.DECODE_ERROR, "error", infoMap);
         ErrorCondition other4 = new ErrorCondition(null, null, infoMap);
         ErrorCondition other5 = new ErrorCondition(null, null, null);

         Assert.AreNotEqual(original, other1);
         Assert.AreNotEqual(original, other2);
         Assert.AreNotEqual(original, other3);
         Assert.AreNotEqual(original, other4);
         Assert.AreNotEqual(original, other5);

         Assert.AreNotEqual(other1, original);
         Assert.AreNotEqual(other2, original);
         Assert.AreNotEqual(other3, original);
         Assert.AreNotEqual(other4, original);
         Assert.AreNotEqual(other5, original);

         Assert.IsFalse(original.Equals(null));
         Assert.IsFalse(original.Equals(true));
      }

      [Test]
      public void TestCopyFromNew()
      {
         ErrorCondition original = new ErrorCondition(AmqpError.DECODE_ERROR, "error");
         ErrorCondition copy = original.Copy();

         Assert.AreEqual(original.Condition, copy.Condition);
         Assert.AreEqual(original.Description, copy.Description);
         Assert.AreEqual(original.Info, copy.Info);
      }
   }
}