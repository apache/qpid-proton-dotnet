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

namespace Apache.Qpid.Proton.Types.Security
{
   public class SaslChallengeTest
   {
      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new SaslChallenge().ToString());
      }

      [Test]
      public void TestGetDataFromEmptySection()
      {
         Assert.IsNull(new SaslChallenge().Challenge);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(new SaslChallenge().Copy().Challenge);
      }

      [Test]
      public void TestMechanismRequired()
      {
         SaslChallenge init = new SaslChallenge();

         try
         {
            init.Challenge = null;
            Assert.Fail("Challenge field is required and should not be cleared");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestCopy()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);

         SaslChallenge value = new SaslChallenge();

         value.Challenge = binary;

         SaslChallenge copy = value.Copy();

         Assert.AreNotSame(copy, value);
         Assert.AreEqual(value.Challenge, copy.Challenge);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SaslPerformativeType.Challenge, new SaslChallenge().Type);
      }
   }
}