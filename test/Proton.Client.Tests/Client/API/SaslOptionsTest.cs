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

using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Client
{
   [TestFixture, Timeout(20000)]
   public class SaslOptionsTest
   {
      [Test]
      public void TestCreate()
      {
         SaslOptions options = new SaslOptions();

         Assert.IsNotNull(options.AllowedMechanisms);
         Assert.AreEqual(0, options.AllowedMechanisms.Count);
         Assert.IsTrue(options.SaslEnabled);
      }

      [Test]
      public void TestCopy()
      {
         SaslOptions options = new SaslOptions();

         options.AddAllowedMechanism("PLAIN");
         options.AddAllowedMechanism("ANONYMOUS");
         options.SaslEnabled = false;

         SaslOptions copy = (SaslOptions)options.Clone();

         Assert.AreEqual(options.AllowedMechanisms, copy.AllowedMechanisms);
         Assert.AreEqual(options.SaslEnabled, copy.SaslEnabled);
      }

      [Test]
      public void TestAllowedOptions()
      {
         SaslOptions options = new SaslOptions();

         Assert.IsNotNull(options.AllowedMechanisms);
         Assert.IsTrue(options.AllowedMechanisms.Count == 0);

         options.AddAllowedMechanism("PLAIN");
         options.AddAllowedMechanism("ANONYMOUS");

         Assert.AreEqual(2, options.AllowedMechanisms.Count);

         Assert.IsTrue(options.AllowedMechanisms.Contains("PLAIN"));
         Assert.IsTrue(options.AllowedMechanisms.Contains("ANONYMOUS"));
      }
   }
}