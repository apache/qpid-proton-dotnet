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

using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class EndTest
   {
      [Test]
      public void TestGetPerformativeType()
      {
         Assert.AreEqual(PerformativeType.End, new End().Type);
      }

      [Test]
      public void TestToStringOnFreshInstance()
      {
         Assert.IsNotNull(new End().ToString());
      }

      [Test]
      public void TestCopyFromNew()
      {
         End original = new End();
         End copy = original.Copy();

         Assert.AreEqual(original.Error, copy.Error);
      }

      [Test]
      public void TestCopyWithError()
      {
         End original = new End();
         original.Error = new ErrorCondition(AmqpError.DECODE_ERROR, "test");

         End copy = original.Copy();

         Assert.AreNotSame(copy.Error, original.Error);
         Assert.AreEqual(original.Error, copy.Error);
      }
   }
}