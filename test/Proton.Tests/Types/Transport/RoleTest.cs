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

using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Types.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Transactions
{
   [TestFixture]
   public class RoleTest
   {
      [Test]
      public void TestValueOf()
      {
         Assert.AreEqual(Role.Sender, RoleExtension.Lookup(false));
         Assert.AreEqual(Role.Receiver, RoleExtension.Lookup(true));
         Assert.AreEqual(Role.Sender, RoleExtension.Lookup(false));
         Assert.AreEqual(Role.Receiver, RoleExtension.Lookup(true));
      }

      [Test]
      public void TestEquality()
      {
         Role sender = Role.Sender;
         Role receiver = Role.Receiver;

         Assert.AreEqual(sender, RoleExtension.Lookup(false));
         Assert.AreEqual(receiver, RoleExtension.Lookup(true));

         Assert.AreEqual(sender.ToBooleanEncoding(), EncodingCodes.BooleanFalse);
         Assert.AreEqual(receiver.ToBooleanEncoding(), EncodingCodes.BooleanTrue);
      }

      [Test]
      public void TestNotEquality()
      {
         Role sender = Role.Sender;
         Role receiver = Role.Receiver;

         Assert.AreNotEqual(sender, RoleExtension.Lookup(true));
         Assert.AreNotEqual(receiver, RoleExtension.Lookup(false));

         Assert.AreNotEqual(sender.ToBooleanEncoding(), true);
         Assert.AreNotEqual(receiver.ToBooleanEncoding(), false);
      }
   }
}