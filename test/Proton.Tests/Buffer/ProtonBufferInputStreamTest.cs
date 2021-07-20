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
   public class ProtonBufferInputStreamTest
   {
      [Test]
      public void TestCannotReadFromClosedStream()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.AreEqual(payload.Length, stream.Length);
         stream.Close();

         Assert.Throws(typeof(ObjectDisposedException), () => stream.ReadByte());
      }

      [Test]
      public void TestBufferWrappedExposesAvailableBytes()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.AreEqual(payload.Length, stream.Length);

         stream.Close();
      }

      [Test]
      public void TestReadReturnsMinusOneAfterAllBytesRead()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.AreEqual(payload.Length, stream.Length);

         for (int i = 0; i < payload.Length; ++i)
         {
            Assert.AreEqual(stream.ReadByte(), payload[i]);
         }

         Assert.AreEqual(-1, stream.ReadByte());

         stream.Close();
      }

      [Test]
      public void TestReadArrayReturnsMinusOneAfterAllBytesRead()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.AreEqual(payload.Length, stream.Length);
         byte[] target = new byte[payload.Length];
         Assert.AreEqual(payload.Length, stream.Read(target));
         Assert.AreEqual(0, stream.Read(target));

         stream.Close();
      }

      [Test]
      public void TestAdjustReadPosition()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.IsTrue(stream.CanRead);
         Assert.IsTrue(stream.CanSeek);
         Assert.AreEqual(payload.Length, stream.Length);
         Assert.AreEqual(0, stream.Position);
         Assert.AreEqual(payload[0], stream.ReadByte());
         Assert.AreEqual(1, stream.Position);

         long pos = stream.Position;

         Assert.AreEqual(payload[1], stream.ReadByte());

         Assert.AreEqual(2, stream.Position);
         stream.Position = pos;
         Assert.AreEqual(1, stream.Position);

         Assert.AreEqual(payload[1], stream.ReadByte());
         Assert.AreEqual(payload[2], stream.ReadByte());

         stream.Close();
      }

      [Test]
      public void TestGetBytesRead()
      {
         MemoryStream memStream = new MemoryStream();
         BinaryWriter outStream = new BinaryWriter(memStream);

         outStream.Write(1024);
         outStream.Write(new byte[] { 0, 1, 2, 3 });
         outStream.Write(false);
         outStream.Write(true);
         outStream.Write((byte)255);
         outStream.Write((char)128);
         outStream.Write(long.MaxValue);
         outStream.Write(3.14f);
         outStream.Write(3.14);

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(memStream.GetBuffer());
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         BinaryReader inStream = new BinaryReader(stream);

         byte[] sink = new byte[4];

         Assert.AreEqual(0, stream.BytesRead);
         Assert.AreEqual(1024, inStream.ReadInt32());
         Assert.AreEqual(4, stream.BytesRead);
         inStream.Read(sink);
         Assert.AreEqual(new byte[] { 0, 1, 2, 3 }, sink);
         Assert.AreEqual(8, stream.BytesRead);
         Assert.AreEqual(false, inStream.ReadBoolean());
         Assert.AreEqual(9, stream.BytesRead);
         Assert.AreEqual(true, inStream.ReadBoolean());
         Assert.AreEqual(10, stream.BytesRead);
         Assert.AreEqual(255, inStream.ReadByte());
         Assert.AreEqual(11, stream.BytesRead);
         Assert.AreEqual(128, inStream.ReadChar());
         Assert.AreEqual(13, stream.BytesRead);
         Assert.AreEqual(long.MaxValue, inStream.ReadInt64());
         Assert.AreEqual(21, stream.BytesRead);
         Assert.AreEqual(3.14f, inStream.ReadSingle(), 0.01f);
         Assert.AreEqual(25, stream.BytesRead);
         Assert.AreEqual(3.14, inStream.ReadDouble(), 0.01);
         Assert.AreEqual(33, stream.BytesRead);

         stream.Close();
      }

      [Test]
      public void TestSkip()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.AreEqual(payload.Length, stream.Length);
         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(Int32.MaxValue, SeekOrigin.Begin));
         Assert.AreEqual(payload.Length, stream.Length);
         Assert.AreEqual(payload.Length, stream.Seek(payload.Length, SeekOrigin.Begin));
         Assert.AreEqual(0, stream.Length);
         Assert.AreEqual(0, stream.Seek(0, SeekOrigin.Begin));
         Assert.AreEqual(payload.Length, stream.Length);
         Assert.AreEqual(payload.Length - 3, stream.Seek(3, SeekOrigin.Current));
         Assert.AreEqual(payload.Length - 3, stream.Length);
         Assert.AreEqual(0, stream.Seek(-3, SeekOrigin.Current));
         Assert.AreEqual(payload.Length, stream.Length);

         stream.Close();
      }

      [Test]
      public void TestSkipOvershoot()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.AreEqual(payload.Length, stream.Length);

         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(10, SeekOrigin.Begin));
         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-1, SeekOrigin.Begin));

         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-10, SeekOrigin.End));
         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(1, SeekOrigin.End));

         stream.Position = 3;

         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(-10, SeekOrigin.Current));
         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(10, SeekOrigin.Current));
      }

      [Test]
      public void TestSkipLong()
      {
         byte[] payload = new byte[] { 0, 1, 2, 3, 4, 5 };

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Wrap(payload);
         ProtonBufferInputStream stream = new ProtonBufferInputStream(buffer);
         Assert.AreEqual(payload.LongLength, stream.Length);
         Assert.Throws<ArgumentOutOfRangeException>(() => stream.Seek(Int64.MaxValue, SeekOrigin.Begin));
         Assert.AreEqual(payload.LongLength, stream.Length);
         Assert.AreEqual(payload.LongLength, stream.Seek(payload.LongLength, SeekOrigin.Begin));
         Assert.AreEqual(0, stream.Length);
         stream.Close();
      }
   }
}
