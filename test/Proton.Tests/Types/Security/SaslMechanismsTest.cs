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
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Security
{
   public class SaslMechanismsTest
   {
      [Test]
      public void TestToStringOnNonEmptyObject()
      {
         Symbol[] mechanisms = new Symbol[] { Symbol.Lookup("EXTERNAL"), Symbol.Lookup("PLAIN") };
         SaslMechanisms value = new SaslMechanisms();

         value.Mechanisms = mechanisms;

         Assert.IsNotNull(value.ToString());
      }

      [Test]
      public void TestToStringOnEmptyObject()
      {
         Assert.IsNotNull(new SaslMechanisms().ToString());
      }

      [Test]
      public void TestGetDataFromEmptySection()
      {
         Assert.IsNull(new SaslMechanisms().Mechanisms);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(new SaslMechanisms().Copy().Mechanisms);
      }

      [Test]
      public void TestMechanismsRequired()
      {
         SaslMechanisms init = new SaslMechanisms();

         try
         {
            init.Mechanisms = (Symbol[])null;
            Assert.Fail("Server Mechanisms field is required and should not be cleared");
         }
         catch (ArgumentNullException) { }
      }

      [Test]
      public void TestCopy()
      {
         Symbol[] mechanisms = new Symbol[] { Symbol.Lookup("EXTERNAL"), Symbol.Lookup("PLAIN") };
         SaslMechanisms value = new SaslMechanisms();

         value.Mechanisms = mechanisms;

         SaslMechanisms copy = value.Copy();

         Assert.AreNotSame(copy, value);
         Assert.AreEqual(value.Mechanisms, copy.Mechanisms);
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SaslPerformativeType.Mechanisms, new SaslMechanisms().Type);
      }
   }
}