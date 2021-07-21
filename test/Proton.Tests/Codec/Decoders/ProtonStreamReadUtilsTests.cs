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
using System.IO;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   [TestFixture]
   public class ProtonStreamReadUtilsTests
   {
      [Test]
      public void TestReadBytes()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         MemoryStream stream = new MemoryStream(source);

         byte[] result = ProtonStreamReadUtils.ReadBytes(stream, source.Length);

         Assert.AreEqual(source, result);
      }

      [Test]
      public void TestReadBytesWithLengthZero()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         MemoryStream stream = new MemoryStream(source);

         byte[] result = ProtonStreamReadUtils.ReadBytes(stream, 0);

         Assert.IsNotNull(result);
         Assert.AreNotEqual(source, result);
         Assert.AreEqual(0, result.Length);
      }

      [Test]
      public void TestReadBytesWithLengthGreaterThanAvailable()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeException>(() => ProtonStreamReadUtils.ReadBytes(stream, 100));
      }

      [Test]
      public void TestReadEncodingCode()
      {
         byte[] source = new byte[] { ((byte)EncodingCodes.ULong) };
         MemoryStream stream = new MemoryStream(source);

         EncodingCodes result = ProtonStreamReadUtils.ReadEncodingCode(stream);

         Assert.AreEqual(EncodingCodes.ULong, result);
      }

      [Test]
      public void TestReadEncodingCodeEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadEncodingCode(stream));
      }

      [Test]
      public void TestReadByte()
      {
         byte[] source = new byte[] { 127, 128 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual((sbyte)source[0], ProtonStreamReadUtils.ReadByte(stream));
         Assert.AreEqual((sbyte)source[1], ProtonStreamReadUtils.ReadByte(stream));
      }

      [Test]
      public void TestReadByteEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadByte(stream));
      }

      [Test]
      public void TestReadUnsignedByte()
      {
         byte[] source = new byte[] { 127, 128 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual(source[0], ProtonStreamReadUtils.ReadUnsignedByte(stream));
         Assert.AreEqual(source[1], ProtonStreamReadUtils.ReadUnsignedByte(stream));
      }

      [Test]
      public void TestReadUnsignedByteEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadUnsignedByte(stream));
      }

      [Test]
      public void TestReadShort()
      {
         byte[] source = new byte[] { 255, 255, 0, 1 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual(-1, ProtonStreamReadUtils.ReadShort(stream));
         Assert.AreEqual(1, ProtonStreamReadUtils.ReadShort(stream));
      }

      [Test]
      public void TestReadShortEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadShort(stream));
      }

      [Test]
      public void TestReadUnsignedShort()
      {
         byte[] source = new byte[] { 255, 255, 0, 1 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual(ushort.MaxValue, ProtonStreamReadUtils.ReadUnsignedShort(stream));
         Assert.AreEqual(1, ProtonStreamReadUtils.ReadUnsignedShort(stream));
      }

      [Test]
      public void TestReadUnsignedShortEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadUnsignedShort(stream));
      }

      [Test]
      public void TestReadInt()
      {
         byte[] source = new byte[] { 255, 255, 255, 255, 0, 0, 0, 1 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual(-1, ProtonStreamReadUtils.ReadInt(stream));
         Assert.AreEqual(1, ProtonStreamReadUtils.ReadInt(stream));
      }

      [Test]
      public void TestReadIntEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadInt(stream));
      }

      [Test]
      public void TestReadUnsignedInt()
      {
         byte[] source = new byte[] { 255, 255, 255, 255, 0, 0, 0, 1 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual(uint.MaxValue, ProtonStreamReadUtils.ReadUnsignedInt(stream));
         Assert.AreEqual(1, ProtonStreamReadUtils.ReadUnsignedInt(stream));
      }

      [Test]
      public void TestReadUnsignedIntEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadUnsignedInt(stream));
      }

      [Test]
      public void TestReadLong()
      {
         byte[] source = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 1 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual(-1, ProtonStreamReadUtils.ReadLong(stream));
         Assert.AreEqual(1, ProtonStreamReadUtils.ReadLong(stream));
      }

      [Test]
      public void TestReadLongEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadLong(stream));
      }

      [Test]
      public void TestReadUnsignedLong()
      {
         byte[] source = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 1 };
         MemoryStream stream = new MemoryStream(source);

         Assert.AreEqual(ulong.MaxValue, ProtonStreamReadUtils.ReadUnsignedLong(stream));
         Assert.AreEqual(1, ProtonStreamReadUtils.ReadUnsignedLong(stream));
      }

      [Test]
      public void TestReadUnsignedLongWithSeeks()
      {
         byte[] source = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 1 };
         MemoryStream stream = new MemoryStream(source);

         stream.Seek(8, SeekOrigin.Begin);
         Assert.AreEqual(1, ProtonStreamReadUtils.ReadUnsignedLong(stream));
         stream.Seek(0, SeekOrigin.Begin);
         Assert.AreEqual(ulong.MaxValue, ProtonStreamReadUtils.ReadUnsignedLong(stream));
      }

      [Test]
      public void TestReadUnsignedLongEOF()
      {
         byte[] source = new byte[0];
         MemoryStream stream = new MemoryStream(source);

         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadUnsignedLong(stream));
      }

      [Test]
      public void TestSkipBytes()
      {
         byte[] source = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
         MemoryStream stream = new MemoryStream(source);

         ProtonStreamReadUtils.SkipBytes(stream, source.LongLength);

         Assert.IsTrue(stream.CanRead);
         Assert.Throws<DecodeEOFException>(() => ProtonStreamReadUtils.ReadByte(stream));

         ProtonStreamReadUtils.SkipBytes(stream, -1);

         Assert.IsTrue(stream.CanRead);
         Assert.DoesNotThrow(() => ProtonStreamReadUtils.ReadByte(stream));
      }
   }
}