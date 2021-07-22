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

using System.IO;
using NUnit.Framework;
using Apache.Qpid.Proton.Buffer;
using System;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class ArrayTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestNetArrayTypes()
      {
         Array array = new int[1];
         Array multiDarray = new int[2][];

         Assert.IsNotNull(array);
         Assert.IsNotNull(multiDarray);
         Assert.AreEqual(1, array.Length);
         Assert.AreEqual(2, multiDarray.Length);

         Assert.AreEqual(0, multiDarray.GetLowerBound(0));
         Assert.AreEqual(1, multiDarray.GetUpperBound(0));
      }
   }
}