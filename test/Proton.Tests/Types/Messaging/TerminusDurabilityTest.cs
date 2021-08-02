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

using NUnit.Framework;
using System;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class TerminusDurabilityTest
   {
      [Test]
      public void TestValueOf()
      {
         Assert.AreEqual(TerminusDurabilityExtension.Lookup(0u), TerminusDurability.None);
         Assert.AreEqual(TerminusDurabilityExtension.Lookup(1u), TerminusDurability.Configuration);
         Assert.AreEqual(TerminusDurabilityExtension.Lookup(2u), TerminusDurability.UnsettledState);

         try
         {
            TerminusDurabilityExtension.Lookup(100);
            Assert.Fail("Should not accept null value");
         }
         catch (ArgumentException) { }
      }
   }
}