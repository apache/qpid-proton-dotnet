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
   public class SaslInitTest
   {
      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new SaslInit().ToString());
      }

      [Test]
      public void TestGetDataFromEmptySection()
      {
         Assert.IsNull(new SaslInit().Hostname);
         Assert.IsNull(new SaslInit().InitialResponse);
         Assert.IsNull(new SaslInit().Mechanism);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(new SaslInit().Copy().Hostname);
      }

      [Test]
      public void TestMechanismRequired()
      {
         SaslInit init = new SaslInit();

         try
         {
            init.Mechanism = null;
            Assert.Fail("Mechanism field is required and should not be cleared");
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestCopy()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);

         SaslInit init = new SaslInit();

         init.Hostname = "localhost";
         init.InitialResponse = binary;
         init.Mechanism = Symbol.Lookup("ANONYMOUS");

         SaslInit copy = init.Copy();

         Assert.AreNotSame(copy, init);
         Assert.AreEqual(init.Hostname, copy.Hostname);
         Assert.AreEqual(init.InitialResponse, copy.InitialResponse);
         Assert.AreEqual(init.Mechanism, copy.Mechanism);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SaslPerformativeType.Init, new SaslInit().Type);
      }
   }
}