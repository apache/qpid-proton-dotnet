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

using Apache.Qpid.Proton.Buffer;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Types.Messaging
{
   [TestFixture]
   public class DataTest
   {
      [Test]
      public void TestTostringOnEmptyObject()
      {
         Assert.IsNotNull(new Data((IProtonBuffer)null).ToString());
      }

      [Test]
      public void TestGetDataFromEmptySection()
      {
         Assert.IsNull(new Data((byte[])null).Value);
      }

      [Test]
      public void TestCopyFromEmpty()
      {
         Assert.IsNull(new Data((IProtonBuffer)null).Copy().Value);
      }

      [Test]
      public void TestCopy()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);
         Data data = new Data(binary);
         Data copy = data.Copy();

         Assert.IsNotNull(copy.Value);
         Assert.AreNotSame(data.Value, copy.Value);
      }

      [Test]
      public void TestHashCode()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);
         Data data = new Data(binary);
         Data copy = data.Copy();

         Assert.IsNotNull(copy.Value);
         Assert.AreNotSame(data.Value, copy.Value);

         Assert.AreEqual(data.GetHashCode(), copy.GetHashCode());

         Data second = new Data(new byte[] { 1, 2, 3 });

         Assert.AreNotEqual(data.GetHashCode(), second.GetHashCode());

         Assert.AreNotEqual(new Data((IProtonBuffer)null).GetHashCode(), data.GetHashCode());
         Assert.AreEqual(new Data((IProtonBuffer)null).GetHashCode(), new Data((IProtonBuffer)null).GetHashCode());
      }

      [Test]
      public void TestEquals()
      {
         byte[] bytes = new byte[] { 1 };
         IProtonBuffer binary = ProtonByteBufferAllocator.Instance.Wrap(bytes);
         Data data = new Data(binary);
         Data copy = data.Copy();

         Assert.IsNotNull(copy.Value);
         Assert.AreNotSame(data.Value, copy.Value);

         Assert.AreEqual(data, data);
         Assert.AreEqual(data, copy);

         Data second = new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2, 3 }));
         Data third = new Data(new byte[] { 1, 2, 3 });
         Data fourth = new Data((byte[]) null);

         Assert.AreNotEqual(data, second);
         Assert.AreNotEqual(data, third);
         Assert.AreNotEqual(data, fourth);
         Assert.IsFalse(data.Equals(null));
         Assert.AreNotEqual(data, "not a data");
         Assert.AreNotEqual(data, new Data((IProtonBuffer)null));
         Assert.AreNotEqual(new Data((IProtonBuffer)null), data);
         Assert.AreEqual(new Data((IProtonBuffer)null), new Data((IProtonBuffer)null));
      }

      [Test]
      public void TestGetType()
      {
         Assert.AreEqual(SectionType.Data, new Data((byte[])null).Type);
      }
   }
}