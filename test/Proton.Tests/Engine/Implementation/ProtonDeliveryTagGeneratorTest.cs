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

using Apache.Qpid.Proton.Types;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   [TestFixture]
   public class ProtonDeliveryTagGeneratorTest
   {
      [Test]
      public void TestEmptyTagGenerator()
      {
         IDeliveryTagGenerator tagGen1 = ProtonDeliveryTagTypes.Empty.NewTagGenerator();
         IDeliveryTagGenerator tagGen2 = ProtonDeliveryTagTypes.Empty.NewTagGenerator();

         Assert.AreSame(tagGen1, tagGen2);

         IDeliveryTag tag1 = tagGen1.NextTag();
         IDeliveryTag tag2 = tagGen2.NextTag();

         Assert.AreSame(tag1, tag2);

         Assert.AreEqual(0, tag1.Length);
         Assert.IsNotNull(tag1.TagBytes);

         Assert.IsNotNull(tagGen1.ToString());
         Assert.IsNotNull(tagGen2.ToString());
      }
   }
}