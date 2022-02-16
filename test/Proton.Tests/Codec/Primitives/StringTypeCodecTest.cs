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
using System;
using System.IO;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class StringTypeCodecTest : CodecTestSupport
   {
      private readonly string SmallStringValue = "Small String";
      private readonly string LargeStringValue = "Large String: " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog.";

      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType()
      {
         TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType(false);
      }

      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsThisTypeFS()
      {
         TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType(true);
      }

      private void TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadString(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadString(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadFromNullEncodingCode()
      {
         TestReadFromNullEncodingCode(false);
      }

      [Test]
      public void TestReadFromNullEncodingCodeFS()
      {
         TestReadFromNullEncodingCode(true);
      }

      private void TestReadFromNullEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadString(stream, streamDecoderState));
         }
         else
         {
            Assert.IsNull(decoder.ReadString(buffer, decoderState));
         }
      }

      [Test]
      public void TestEncodeSmallString()
      {
         doTestEncodeDecode(SmallStringValue, false);
      }

      [Test]
      public void TestEncodeLargeString()
      {
         doTestEncodeDecode(LargeStringValue, false);
      }

      [Test]
      public void TestEncodeEmptyString()
      {
         doTestEncodeDecode("", false);
      }

      [Test]
      public void TestEncodeNullString()
      {
         doTestEncodeDecode(null, false);
      }

      [Test]
      public void TestEncodeSmallStringFS()
      {
         doTestEncodeDecode(SmallStringValue, true);
      }

      [Test]
      public void TestEncodeLargeStringFS()
      {
         doTestEncodeDecode(LargeStringValue, true);
      }

      [Test]
      public void TestEncodeEmptyStringFS()
      {
         doTestEncodeDecode("", true);
      }

      [Test]
      public void TestEncodeNullStringFS()
      {
         doTestEncodeDecode(null, true);
      }

      private void doTestEncodeDecode(String value, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteObject(buffer, encoderState, value);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         if (value != null)
         {
            Assert.IsNotNull(result);
            Assert.IsTrue(result is string);
         }
         else
         {
            Assert.IsNull(result);
         }

         Assert.AreEqual(value, result);
      }

      [Test]
      public void TestDecodeSmallSeriesOfStrings()
      {
         doTestDecodeStringSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfStrings()
      {
         doTestDecodeStringSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfStringsFS()
      {
         doTestDecodeStringSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfStringsFS()
      {
         doTestDecodeStringSeries(LargeSize, true);
      }

      private void doTestDecodeStringSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteString(buffer, encoderState, LargeStringValue);
         }

         for (int i = 0; i < size; ++i)
         {
            object result;
            if (fromStream)
            {
               result = streamDecoder.ReadObject(stream, streamDecoderState);
            }
            else
            {
               result = decoder.ReadObject(buffer, decoderState);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result is String);
            Assert.AreEqual(LargeStringValue, result);
         }
      }

      [Test]
      public void TestDecodeStringOfZeroLengthWithLargeEncoding()
      {
         DoTestDecodeStringOfZeroLengthWithGivenEncoding(EncodingCodes.Str32, false);
      }

      [Test]
      public void TestDecodeStringOfZeroLengthWithSmallEncoding()
      {
         DoTestDecodeStringOfZeroLengthWithGivenEncoding(EncodingCodes.Str8, false);
      }

      [Test]
      public void TestDecodeStringOfZeroLengthWithLargeEncodingFS()
      {
         DoTestDecodeStringOfZeroLengthWithGivenEncoding(EncodingCodes.Str32, true);
      }

      [Test]
      public void TestDecodeStringOfZeroLengthWithSmallEncodingFS()
      {
         DoTestDecodeStringOfZeroLengthWithGivenEncoding(EncodingCodes.Str8, true);
      }

      private void DoTestDecodeStringOfZeroLengthWithGivenEncoding(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         // Manually encode the type we want.
         if (encodingCode == EncodingCodes.Str32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Str32));
            buffer.WriteInt(0);
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Str8));
            buffer.WriteUnsignedByte(0);
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(string), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, encodingCode);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.PeekNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(string), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, encodingCode);
         }

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.AreEqual("", result);
      }

      [Test]
      public void TestEncodedSizeExceedsRemainingDetectedStr32()
      {
         TestEncodedSizeExceedsRemainingDetectedStr32(false);
      }

      [Test]
      public void TestEncodedSizeExceedsRemainingDetectedStr32FS()
      {
         TestEncodedSizeExceedsRemainingDetectedStr32(true);
      }

      private void TestEncodedSizeExceedsRemainingDetectedStr32(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str32));
         buffer.WriteInt(8);
         buffer.WriteInt(Int32.MaxValue);

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("should throw an DecodeException");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("should throw an DecodeException");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestEncodedSizeExceedsRemainingDetectedStr8()
      {
         TestEncodedSizeExceedsRemainingDetectedStr8(false);
      }

      [Test]
      public void TestEncodedSizeExceedsRemainingDetectedStr8FS()
      {
         TestEncodedSizeExceedsRemainingDetectedStr8(true);
      }

      private void TestEncodedSizeExceedsRemainingDetectedStr8(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str8));
         buffer.WriteUnsignedByte(4);
         buffer.WriteUnsignedByte(byte.MaxValue);

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("should throw an DecodeException");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("should throw an DecodeException");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestSkipValue()
      {
         TestSkipValue(false);
      }

      [Test]
      public void TestSkipValueFS()
      {
         TestSkipValue(true);
      }

      private void TestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteString(buffer, encoderState, "skipMe");
         }

         String expected = "expected-string-value";

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(string), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Str8);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(string), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Str8);
               typeDecoder.SkipValue(buffer, decoderState);
            }
         }

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.IsTrue(result is String);

         String value = (String)result;
         Assert.AreEqual(expected, value);
      }

      [Test]
      public void TestDecodeNonStringWhenStringExpectedReportsUsefulError()
      {
         TestDecodeNonStringWhenStringExpectedReportsUsefulError(false);
      }

      [Test]
      public void TestDecodeNonStringWhenStringExpectedReportsUsefulErrorFS()
      {
         TestDecodeNonStringWhenStringExpectedReportsUsefulError(true);
      }

      private void TestDecodeNonStringWhenStringExpectedReportsUsefulError(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Guid encoded = Guid.NewGuid();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteBytes(encoded.ToByteArray());

         ITypeDecoder nextType = decoder.PeekNextTypeDecoder(buffer, decoderState);
         Assert.AreEqual(typeof(Guid), nextType.DecodesType);

         long readOffset = buffer.ReadOffset;

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadString(stream, streamDecoderState);
            }
            catch (DecodeException ex)
            {
               // Should indicate the type that it found in the error
               Assert.IsTrue(ex.Message.Contains(EncodingCodes.Uuid.ToString()));
            }
         }
         else
         {
            try
            {
               decoder.ReadString(buffer, decoderState);
            }
            catch (DecodeException ex)
            {
               // Should indicate the type that it found in the error
               Assert.IsTrue(ex.Message.Contains(EncodingCodes.Uuid.ToString()));
            }
         }

         buffer.ReadOffset = readOffset;
         Guid? actual = decoder.ReadGuid(buffer, decoderState);
         Assert.AreEqual(encoded, actual);
      }

      [Test]
      public void TestDecodeUnknownTypeWhenStringExpectedReportsUsefulError()
      {
         TestDecodeUnknownTypeWhenStringExpectedReportsUsefulError(false);
      }

      [Test]
      public void TestDecodeUnknownTypeWhenStringExpectedReportsUsefulErrorFS()
      {
         TestDecodeUnknownTypeWhenStringExpectedReportsUsefulError(true);
      }

      private void TestDecodeUnknownTypeWhenStringExpectedReportsUsefulError(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0x01);

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadString(stream, streamDecoderState);
            }
            catch (DecodeException ex)
            {
               // Should indicate the type that it found in the error
               Assert.IsTrue(ex.Message.Contains("Expected String"));
            }
         }
         else
         {
            try
            {
               decoder.ReadString(buffer, decoderState);
            }
            catch (DecodeException ex)
            {
               // Should indicate the type that it found in the error
               Assert.IsTrue(ex.Message.Contains("Expected String"));
            }
         }
      }
   }
}