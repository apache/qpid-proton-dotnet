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
   public class SaslResponseTest
   {
      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new SaslResponse().ToString());
      }

      [Test]
      public void TestGetDataFromEmptySection()
      {
         Assert.IsNull(new SaslResponse().Response);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(new SaslResponse().Copy().Response);
      }

      [Test]
      public void TestResponseRequired()
      {
         SaslResponse init = new SaslResponse();

         try
         {
            init.Response = null;
            Assert.Fail("Response field is required and should not be cleared");
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestCopy()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);

         SaslResponse value = new SaslResponse();

         value.Response = binary;

         SaslResponse copy = value.Copy();

         Assert.AreNotSame(copy, value);
         Assert.AreEqual(value.Response, copy.Response);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SaslPerformativeType.Response, new SaslResponse().Type);
      }
   }
}
