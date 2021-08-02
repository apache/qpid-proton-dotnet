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

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class ModifiedTest
   {
      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new Modified().ToString());
      }

      [Test]
      public void TestCreateWithDeliveryFailed()
      {
         Modified modified = new Modified(true, false);
         Assert.IsTrue(modified.DeliveryFailed);
         Assert.IsFalse(modified.UndeliverableHere);
         Assert.IsNull(modified.MessageAnnotations);
      }

      [Test]
      public void TestCreateWithDeliveryFailedAndUndeliverableHere()
      {
         Modified modified = new Modified(true, true);
         Assert.IsTrue(modified.DeliveryFailed);
         Assert.IsTrue(modified.UndeliverableHere);
         Assert.IsNull(modified.MessageAnnotations);
      }

      [Test]
      public void TestCreateWithDeliveryFailedAndUndeliverableHereAndAnnotations()
      {
         IDictionary<Symbol, object> annotations = new Dictionary<Symbol, object>();
         annotations.Add(Symbol.Lookup("key1"), "value");
         annotations.Add(Symbol.Lookup("key2"), "value");

         Modified modified = new Modified(true, true, annotations);
         Assert.IsTrue(modified.DeliveryFailed);
         Assert.IsTrue(modified.UndeliverableHere);
         Assert.IsNotNull(modified.MessageAnnotations);

         Assert.AreEqual(annotations, modified.MessageAnnotations);
      }

      [Test]
      public void TestAnnotations()
      {
         Modified modified = new Modified();
         Assert.IsNull(modified.MessageAnnotations);
         modified.MessageAnnotations = new Dictionary<Symbol, object>();
         Assert.IsNotNull(modified.MessageAnnotations);
      }

      [Test]
      public void TestDeliveryFailed()
      {
         Modified modified = new Modified();
         Assert.IsFalse(modified.DeliveryFailed);
         modified.DeliveryFailed = true;
         Assert.IsTrue(modified.DeliveryFailed);
      }

      [Test]
      public void TestUndeliverableHere()
      {
         Modified modified = new Modified();
         Assert.IsFalse(modified.UndeliverableHere);
         modified.UndeliverableHere = true;
         Assert.IsTrue(modified.UndeliverableHere);
      }

      [Test]
      public void TestGetAnnotationsFromEmptySection()
      {
         Assert.IsNull(new Modified().MessageAnnotations);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(DeliveryStateType.Modified, new Modified().Type);
      }
   }
}