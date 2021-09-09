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

namespace Apache.Qpid.Proton.Test.Driver.Matchers
{
   [TestFixture]
   public class AllOfMatcherTest
   {
      [Test]
      public void TestAllOfMatchesWithOneEquals()
      {
         IMatcher allOf = Matches.AllOf(Is.EqualTo("test"));

         Assert.IsTrue(allOf.Matches("test"));
         Assert.IsFalse(allOf.Matches("foo"));
      }

      [Test]
      public void TestAllOfDoesNotMatchWithOneEquals()
      {
         IMatcher allOf = Matches.AllOf(Is.EqualTo("test"));

         Assert.IsTrue(allOf.Matches("test"));
         Assert.IsFalse(allOf.Matches("fail"));
      }

      [Test]
      public void TestAllOfMatchesWithMultipleNots()
      {
         IMatcher allOf = Matches.AllOf(Is.Not("test"), Is.Not("other"), Is.Not(1));

         Assert.IsTrue(allOf.Matches("foo"));
         Assert.IsFalse(allOf.Matches("test"));
      }
   }
}