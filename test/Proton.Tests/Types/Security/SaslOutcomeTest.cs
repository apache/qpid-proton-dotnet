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

using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Security
{
   public class SaslOutcomeTest
   {
      [Test]
      public void TestToStringOnNonEmptyObject()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);

         SaslOutcome value = new SaslOutcome();

         value.Code = SaslCode.Ok;
         value.AdditionalData = binary;

         Assert.IsNotNull(value.ToString());
      }

      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new SaslOutcome().ToString());
      }

      [Test]
      public void TestGetDataFromEmptySection()
      {
         Assert.IsNotNull(new SaslOutcome().Code);
         Assert.IsNull(new SaslOutcome().AdditionalData);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNotNull(new SaslOutcome().Copy().Code);
      }

      [Test]
      public void TestCopy()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);

         SaslOutcome value = new SaslOutcome();

         value.Code = SaslCode.Ok;
         value.AdditionalData = binary;

         SaslOutcome copy = value.Copy();

         Assert.AreNotSame(copy, value);
         Assert.AreEqual(copy.Code, value.Code);
         Assert.AreEqual(value.AdditionalData, copy.AdditionalData);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SaslPerformativeType.Outcome, new SaslOutcome().Type);
      }
   }
}