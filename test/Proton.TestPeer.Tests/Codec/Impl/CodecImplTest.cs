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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   [TestFixture]
   public class CodecTest
   {
      private ICodec codec = CodecFactory.Create();

      [Test]
      public void TestDecodeOpen()
      {
         Open open = new Open();
         open.ContainerId = "test";
         open.Hostname = "localhost";

         long expectedRead;
         Stream encoded = EncodeProtonPerformative(open, out expectedRead);
         Assert.AreNotEqual(0, expectedRead);

         ICodec codec = CodecFactory.Create();

         Assert.AreEqual(expectedRead, codec.Decode(new BinaryReader(encoded)));

         Open described = (Open)codec.GetDescribedType();
         Assert.IsNotNull(described);
         Assert.AreEqual(Open.DESCRIPTOR_SYMBOL, described.Descriptor);

         Assert.AreEqual(open.ContainerId, described.ContainerId);
         Assert.AreEqual(open.Hostname, described.Hostname);
      }

      private Stream EncodeProtonPerformative(IDescribedType performative, out long encodingSize)
      {
         MemoryStream stream = new MemoryStream();
         encodingSize = 0;

         if (performative != null)
         {
            try
            {
               codec.PutDescribedType(performative);
               encodingSize = codec.Encode(new BinaryWriter(stream));
            }
            finally
            {
               codec.Clear();
            }
         }

         stream.Position = 0;  // Reset stream to beginning for decode.

         return encodingSize > 0 ? stream : null;
      }
   }
}