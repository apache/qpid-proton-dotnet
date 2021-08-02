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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class ReceivedTypeTest
   {
      [Test]
      public void testToStringOnEmptyObject()
      {
         Assert.IsNotNull(new Received().ToString());
      }

      [Test]
      public void testGetType()
      {
         Assert.AreEqual(DeliveryStateType.Received, new Received().Type);
      }

      [Test]
      public void testSectionNumber()
      {
         Received received = new Received();

         Assert.AreEqual(0, received.SectionNumber);
         received.SectionNumber = 20u;
         Assert.IsNotNull(received.SectionNumber);
      }

      [Test]
      public void testSectionOffset()
      {
         Received received = new Received();

         Assert.AreEqual(0, received.SectionOffset);
         received.SectionOffset = 20u;
         Assert.IsNotNull(received.SectionOffset);
      }
   }
}