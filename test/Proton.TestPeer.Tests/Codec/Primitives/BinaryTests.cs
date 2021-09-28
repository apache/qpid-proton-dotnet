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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Primitives
{
   [TestFixture]
   public class CodecTest
   {
      [Test]
      public void TestCreateEmptyBinary()
      {
         Binary binary = new Binary();

         Assert.IsFalse(binary.HasArray);
         Assert.AreEqual(0, binary.Length);
         Assert.IsNull(binary.Array);
      }

      [Test]
      public void TestCreateNonEmptyBinary()
      {
         Binary binary = new Binary(new byte[] { 0 });

         Assert.IsTrue(binary.HasArray);
         Assert.AreEqual(1, binary.Length);
         Assert.AreEqual(1, binary.Array.Length);
      }

      [Test]
      public void TestBinaryEquals()
      {
         Binary binary1 = new Binary(new byte[] { 0, 0, 0, 0 });
         Binary binary2 = new Binary(new byte[] { 0, 0, 0, 0 });
         Binary binary3 = new Binary(new byte[] { 1, 0, 0, 1 });

         Assert.AreEqual(binary1, binary1);
         Assert.AreEqual(binary1, binary2);
         Assert.AreNotEqual(binary1, binary3);
      }
   }
}
