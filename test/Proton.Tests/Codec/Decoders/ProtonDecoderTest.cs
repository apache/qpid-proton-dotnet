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
using Apache.Qpid.Proton.Codec.Decoders;
using Apache.Qpid.Proton.Codec;

namespace Apache.Qpid.Proton.Codec
{
   [TestFixture]
   public class ProtonDecoderTest : CodecTestSupport
   {
      [Test]
      public void TestGetCachedDecoderStateReturnsCachedState()
      {
         IDecoderState first = decoder.CachedDecoderState;

         Assert.AreSame(first, decoder.CachedDecoderState);
      }

      [Test]
      public void TestReadNullFromReadObjectForNullEncodng()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteByte(((sbyte)EncodingCodes.Null));
         buffer.WriteByte(((sbyte)EncodingCodes.Null));

         Assert.IsNull(decoder.ReadObject(buffer, decoderState));
         Assert.Throws(typeof(InvalidCastException), () => decoder.ReadObject<Guid>(buffer, decoderState));
      }

      [Test]
      public void TestTryReadFromEmptyBuffer()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("Should fail on read of object from empty buffer");
         }
         catch (DecodeEOFException) { }
      }

      [Test]
      public void TestErrorOnReadOfUnknownEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(255);

         Assert.IsNull(decoder.PeekNextTypeDecoder(buffer, decoderState));

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("Should throw if no type decoder exists for given type");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void testReadFromNullEncodingCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         try
         {
            decoder.ReadObject<string>(buffer, decoderState);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.IsNull(decoder.ReadMultiple<Guid>(buffer, decoderState));
      }

      [Test]
      public void TestReadMultipleFromSingleEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         Guid[] result = decoder.ReadMultiple<Guid>(buffer, decoderState);

         Assert.IsNotNull(result);
         Assert.AreEqual(1, result.Length);
      }

      [Test]
      public void TestReadMultipleRequestsWrongTypeForArray()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         try
         {
            decoder.ReadMultiple<string>(buffer, decoderState);
            Assert.Fail("Should not be able to convert to wrong resulting array type");
         }
         catch (InvalidCastException) { }
      }

      // TODO: Implement array codec [Test]
      public void TestReadMultipleRequestsWrongTypeForArrayEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         Guid[] value = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

         encoder.WriteArray(buffer, encoderState, value);

         try
         {
            decoder.ReadMultiple<string>(buffer, decoderState);
            Assert.Fail("Should not be able to convert to wrong resulting array type");
         }
         catch (InvalidCastException) { }
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithNegativeLongDescriptor()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         Guid value = Guid.NewGuid();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(long.MaxValue);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = decoder.ReadObject(buffer, decoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void testDecodeUnknownDescribedTypeWithMaxLongDescriptor()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(long.MaxValue);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = decoder.ReadObject(buffer, decoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void testDecodeUnknownDescribedTypeWithUnknownDescriptorCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = decoder.ReadObject(buffer, decoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void testReadUnsignedIntegerTypes()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt0));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallUInt));
         buffer.WriteUnsignedByte(127);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.AreEqual(0, decoder.ReadUnsignedInteger(buffer, decoderState, 32));
         Assert.AreEqual(127, decoder.ReadUnsignedInteger(buffer, decoderState, 32));
         Assert.AreEqual(255, decoder.ReadUnsignedInteger(buffer, decoderState, 32));
         Assert.AreEqual(32, decoder.ReadUnsignedInteger(buffer, decoderState, 32));
      }

      [Test]
      public void testReadStringWithCustomStringDecoder()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str32));
         buffer.WriteInt(16);
         buffer.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

         ((ProtonDecoderState)decoderState).Utf8Decoder = new DummyUtf8Decoder();

         Assert.IsNotNull(((ProtonDecoderState)decoderState).Utf8Decoder);

         String result = decoder.ReadString(buffer, decoderState);

         Assert.AreEqual("string-decoder", result);
         Assert.IsFalse(buffer.Readable);
      }

      [Test]
      public void testStringReadFromCustomDecoderThrowsDecodeExceptionOnError()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str32));
         buffer.WriteInt(16);
         buffer.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

         ((ProtonDecoderState)decoderState).Utf8Decoder = new FailingUtf8Decoder();

         Assert.IsNotNull(((ProtonDecoderState)decoderState).Utf8Decoder);
         Assert.Throws(typeof(DecodeException), () => decoder.ReadString(buffer, decoderState));
      }
   }

   internal class DummyUtf8Decoder : IUtf8Decoder
   {
      public string DecodeUTF8(IProtonBuffer buffer, int utf8length)
      {
         return "string-decoder";
      }
   }

   internal class FailingUtf8Decoder : IUtf8Decoder
   {
      public string DecodeUTF8(IProtonBuffer buffer, int utf8length)
      {
         throw new IndexOutOfRangeException();
      }
   }
}

