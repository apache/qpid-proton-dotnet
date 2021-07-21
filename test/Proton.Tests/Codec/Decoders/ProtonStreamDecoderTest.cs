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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using System.IO;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   [TestFixture]
   public class ProtonStreamDecoderTest : CodecTestSupport
   {
      [Test]
      public void TestGetCachedDecoderStateReturnsCachedState()
      {
         IStreamDecoderState first = streamDecoder.CachedDecoderState;

         Assert.AreSame(first, streamDecoder.CachedDecoderState);
      }

      [Test]
      public void TestReadNullFromReadObjectForNullEncodng()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteByte(((sbyte)EncodingCodes.Null));
         buffer.WriteByte(((sbyte)EncodingCodes.Null));

         Assert.IsNull(streamDecoder.ReadObject(stream, streamDecoderState));
         Assert.Throws(typeof(InvalidCastException), () => streamDecoder.ReadObject<Guid>(stream, streamDecoderState));
      }

      [Test]
      public void TestTryReadFromEmptyStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         try
         {
            streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.Fail("Should fail on read of object from empty stream");
         }
         catch (DecodeEOFException) { }
      }

      [Test]
      public void TestErrorOnReadOfUnknownEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(255);

         Assert.IsNull(streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState));

         try
         {
            streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.Fail("Should throw if no type streamDecoder exists for given type");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestReadFromNullEncodingCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         try
         {
            streamDecoder.ReadObject<string>(stream, streamDecoderState);
            Assert.Fail("Should not allow for conversion to String type");
         }
         catch (InvalidCastException)
         {
         }
      }

      [Test]
      public void TestReadMultipleFromNullEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.IsNull(streamDecoder.ReadMultiple<Guid>(stream, streamDecoderState));
      }

      [Test]
      public void TestReadMultipleFromSingleEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         Guid[] result = streamDecoder.ReadMultiple<Guid>(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.AreEqual(1, result.Length);
      }

      [Test]
      public void TestReadMultipleRequestsWrongTypeForArray()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         try
         {
            streamDecoder.ReadMultiple<string>(stream, streamDecoderState);
            Assert.Fail("Should not be able to convert to wrong resulting array type");
         }
         catch (InvalidCastException) { }
      }

      // TODO: Implement array codec [Test]
      public void TestReadMultipleRequestsWrongTypeForArrayEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[] value = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

         encoder.WriteArray(buffer, encoderState, value);

         try
         {
            streamDecoder.ReadMultiple<string>(stream, streamDecoderState);
            Assert.Fail("Should not be able to convert to wrong resulting array type");
         }
         catch (InvalidCastException) { }
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithNegativeLongDescriptor()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid value = Guid.NewGuid();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(long.MaxValue);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = streamDecoder.ReadObject(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithMaxLongDescriptor()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(long.MaxValue);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = streamDecoder.ReadObject(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithUnknownDescriptorCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = streamDecoder.ReadObject(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void TestReadUnsignedIntegerTypes()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt0));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallUInt));
         buffer.WriteUnsignedByte(127);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.AreEqual(0, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
         Assert.AreEqual(127, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
         Assert.AreEqual(255, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
         Assert.AreEqual(32, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
      }

      [Test]
      public void TestReadStringWithCustomStringDecoder()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str32));
         buffer.WriteInt(16);
         buffer.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

         ((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder = new DummyUtf8StreamDecoder();

         Assert.IsNotNull(((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder);

         String result = streamDecoder.ReadString(stream, streamDecoderState);

         Assert.AreEqual("string-streamDecoder", result);
         Assert.IsTrue(buffer.Readable); // We didn't read anything so buffer was untouched
      }

      [Test]
      public void TestStringReadFromCustomDecoderThrowsDecodeExceptionOnError()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str32));
         buffer.WriteInt(16);
         buffer.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

         ((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder = new FailingUtf8StreamDecoder();

         Assert.IsNotNull(((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder);
         Assert.Throws(typeof(DecodeException), () => streamDecoder.ReadString(stream, streamDecoderState));
      }
   }

   internal class DummyUtf8StreamDecoder : IUtf8StreamDecoder
   {
      public string DecodeUTF8(Stream stream, int utf8length)
      {
         return "string-streamDecoder";
      }
   }

   internal class FailingUtf8StreamDecoder : IUtf8StreamDecoder
   {
      public string DecodeUTF8(Stream stream, int utf8length)
      {
         throw new IndexOutOfRangeException();
      }
   }
}

