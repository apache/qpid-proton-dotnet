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
using System.Threading;
using System.Linq;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client.Impl
{
   [TestFixture, Timeout(20000)]
   public class ClientMessageTest
   {
      [Test]
      public void TestCreateEmpty()
      {
         ClientMessage<String> message = ClientMessage<string>.Create();

         Assert.IsNull(message.Body);
         Assert.IsNotNull(message.GetBodySections());
         Assert.AreEqual(0, message.GetBodySections().Count());

         Assert.IsFalse(message.HasProperties);
         Assert.IsFalse(message.HasFooters);
         Assert.IsFalse(message.HasAnnotations);
      }
   }
}