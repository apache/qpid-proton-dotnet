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
using System.IO;
using NUnit.Framework;

namespace Apache.Qpid.Proton.Buffer
{
   [TestFixture]
   public class ProtonBufferOutputStreamTest
   {
      [Test]
      public void TestBufferWrappedExposesWrittenBytes()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(payload.Length);
         ProtonBufferOutputStream stream = new ProtonBufferOutputStream(buffer);
         Assert.AreEqual(0, stream.BytesWritten);

         stream.Write(payload);

         Assert.AreEqual(payload.Length, stream.BytesWritten);

         stream.Close();
      }

      [Test]
      public void TestBufferWritesGivenArrayBytes()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(payload.Length);
         ProtonBufferOutputStream stream = new ProtonBufferOutputStream(buffer);
         Assert.AreEqual(0, stream.BytesWritten);

         stream.Write(payload);

         for (int i = 0; i < payload.Length; ++i)
         {
            Assert.AreEqual(payload[i], buffer.GetByte(i));
         }

         stream.Close();
      }

      [Test]
      public void TestZeroLengthWriteBytesDoesNotWriteOrThrow()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(payload.Length);
         ProtonBufferOutputStream stream = new ProtonBufferOutputStream(buffer);

         stream.Write(payload, 0, 0);

         Assert.AreEqual(0, stream.BytesWritten);

         stream.Close();
      }

      [Test]
      public void TestCannotWriteToClosedStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         ProtonBufferOutputStream stream = new ProtonBufferOutputStream(buffer);

         stream.Close();
         Assert.Throws(typeof(ObjectDisposedException), () => stream.WriteByte(byte.MaxValue));
      }

      [Test]
      public void TestWriteValuesAndReadWithDataInputStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         using ProtonBufferOutputStream stream = new ProtonBufferOutputStream(buffer);
         using BinaryWriter streamWriter = new BinaryWriter(stream);

         streamWriter.Write((byte) 32);
         streamWriter.Write(1024);
         streamWriter.Write(new byte[] { 0, 1, 2, 3 });
         streamWriter.Write(false);
         streamWriter.Write(true);
         streamWriter.Write((byte) 255);
         streamWriter.Write((short)32767);
         streamWriter.Write(long.MaxValue);
         streamWriter.Write((char)65535);
         streamWriter.Write(3.14f);
         streamWriter.Write(3.14);

         byte[] array = new byte[buffer.ReadableBytes];
         buffer.CopyInto(0, array, 0, buffer.ReadableBytes);
         using MemoryStream memStream = new MemoryStream(array);
         using BinaryReader streamReader = new BinaryReader(memStream);

         byte[] sink = new byte[4];

         Assert.AreEqual(32, streamReader.ReadByte());
         Assert.AreEqual(1024, streamReader.ReadInt32());
         streamReader.Read(sink);
         Assert.AreEqual(new byte[] { 0, 1, 2, 3 }, sink);
         Assert.AreEqual(false, streamReader.ReadBoolean());
         Assert.AreEqual(true, streamReader.ReadBoolean());
         Assert.AreEqual(255, streamReader.ReadByte());
         Assert.AreEqual(32767, streamReader.ReadUInt16());
         Assert.AreEqual(long.MaxValue, streamReader.ReadInt64());
         Assert.AreEqual(65535, streamReader.ReadChar());
         Assert.AreEqual(3.14f, streamReader.ReadSingle(), 0.01f);
         Assert.AreEqual(3.14, streamReader.ReadDouble(), 0.01);
      }
   }
}
