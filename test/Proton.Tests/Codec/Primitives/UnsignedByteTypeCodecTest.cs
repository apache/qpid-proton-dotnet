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

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class UnsignedByteTypeCodecTest : CodecTestSupport
   {
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
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadUnsignedByte(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadUnsignedByte(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadUnsignedByte(buffer, decoderState, (byte)0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadTypeFromEncodingCode()
      {
         TestReadTypeFromEncodingCode(false);
      }

      [Test]
      public void TestReadTypeFromEncodingCodeFS()
      {
         TestReadTypeFromEncodingCode(true);
      }

      public void TestReadTypeFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte((byte)42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte((byte)43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadUnsignedByte(stream, streamDecoderState));
            Assert.AreEqual(43, streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)42));
            Assert.IsNull(streamDecoder.ReadUnsignedByte(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)42));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadUnsignedByte(buffer, decoderState));
            Assert.AreEqual(43, decoder.ReadUnsignedByte(buffer, decoderState, (byte)42));
            Assert.IsNull(decoder.ReadUnsignedByte(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadUnsignedByte(buffer, decoderState, (byte)42));
         }
      }

      [Test]
      public void TestEncodeDecodeUnsignedByte()
      {
         TestEncodeDecodeUnsignedByte(false);
      }

      [Test]
      public void TestEncodeDecodeUnsignedByteFS()
      {
         TestEncodeDecodeUnsignedByte(true);
      }

      public void TestEncodeDecodeUnsignedByte(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedByte(buffer, encoderState, (byte)64);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is byte);
         Assert.AreEqual(64, (byte)result);
      }

      [Test]
      public void TestEncodeDecodeByte()
      {
         TestEncodeDecodeByte(false);
      }

      [Test]
      public void TestEncodeDecodeByteFS()
      {
         TestEncodeDecodeByte(true);
      }

      private void TestEncodeDecodeByte(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedByte(buffer, encoderState, (byte)64);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is byte);
         Assert.AreEqual(64, (byte)result);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedBytes()
      {
         doTestDecodeUnsignedByteSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedBytes()
      {
         doTestDecodeUnsignedByteSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedBytesFS()
      {
         doTestDecodeUnsignedByteSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedBytesFS()
      {
         doTestDecodeUnsignedByteSeries(LargeSize, true);
      }

      private void doTestDecodeUnsignedByteSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteUnsignedByte(buffer, encoderState, (byte)(i % 255));
         }

         for (int i = 0; i < size; ++i)
         {
            byte? result;
            if (fromStream)
            {
               result = streamDecoder.ReadUnsignedByte(stream, streamDecoderState);
            }
            else
            {
               result = decoder.ReadUnsignedByte(buffer, decoderState);
            }

            Assert.IsNotNull(result);
            Assert.AreEqual((byte)(i % 255), result);
         }
      }

      [Test]
      public void TestArrayOfObjects()
      {
         TestArrayOfObjects(false);
      }

      [Test]
      public void TestArrayOfObjectsFS()
      {
         TestArrayOfObjects(true);
      }

      private void TestArrayOfObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         byte[] source = new byte[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = (byte)(i % 255);
         }

         encoder.WriteArray(buffer, encoderState, source);

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
         Assert.IsTrue(result.GetType().IsArray);
         Assert.AreEqual(result.GetType().GetElementType(), typeof(byte));

         byte[] array = (byte[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      [Test]
      public void TestZeroSizedArrayOfObjects()
      {
         TestZeroSizedArrayOfObjects(false);
      }

      [Test]
      public void TestZeroSizedArrayOfObjectsFS()
      {
         TestZeroSizedArrayOfObjects(true);
      }

      private void TestZeroSizedArrayOfObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         byte[] source = new byte[0];

         encoder.WriteArray(buffer, encoderState, source);

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
         Assert.IsTrue(result.GetType().IsArray);
         Assert.AreEqual(result.GetType().GetElementType(), typeof(byte));

         byte[] array = (byte[])result;
         Assert.AreEqual(source.Length, array.Length);
      }

      [Test]
      public void TestDecodeEncodedBytes()
      {
         TestDecodeEncodedBytes(false);
      }

      [Test]
      public void TestDecodeEncodedBytesFS()
      {
         TestDecodeEncodedBytes(true);
      }

      private void TestDecodeEncodedBytes(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte(127);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte(255);

         if (fromStream)
         {
            byte? result1 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState);
            byte? result2 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState);
            byte? result3 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState);

            Assert.AreEqual((byte)0, result1);
            Assert.AreEqual((byte)127, result2);
            Assert.AreEqual((byte)255, result3);
         }
         else
         {
            byte? result1 = decoder.ReadUnsignedByte(buffer, decoderState);
            byte? result2 = decoder.ReadUnsignedByte(buffer, decoderState);
            byte? result3 = decoder.ReadUnsignedByte(buffer, decoderState);

            Assert.AreEqual((byte)0, result1);
            Assert.AreEqual((byte)127, result2);
            Assert.AreEqual((byte)255, result3);
         }
      }

      [Test]
      public void TestDecodeEncodedBytesAsPrimitive()
      {
         TestDecodeEncodedBytesAsPrimitive(false);
      }

      [Test]
      public void TestDecodeEncodedBytesAsPrimitiveFS()
      {
         TestDecodeEncodedBytesAsPrimitive(true);
      }

      private void TestDecodeEncodedBytesAsPrimitive(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte(127);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte(255);

         if (fromStream)
         {
            byte result1 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)1);
            byte result2 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)105);
            byte result3 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)200);

            Assert.AreEqual((byte)0, result1);
            Assert.AreEqual((byte)127, result2);
            Assert.AreEqual((byte)255, result3);
         }
         else
         {
            byte result1 = decoder.ReadUnsignedByte(buffer, decoderState, (byte)1);
            byte result2 = decoder.ReadUnsignedByte(buffer, decoderState, (byte)105);
            byte result3 = decoder.ReadUnsignedByte(buffer, decoderState, (byte)200);

            Assert.AreEqual((byte)0, result1);
            Assert.AreEqual((byte)127, result2);
            Assert.AreEqual((byte)255, result3);
         }
      }

      [Test]
      public void TestDecodeBooleanFromNullEncoding()
      {
         TestDecodeBooleanFromNullEncoding(false);
      }

      [Test]
      public void TestDecodeBooleanFromNullEncodingFS()
      {
         TestDecodeBooleanFromNullEncoding(true);
      }

      private void TestDecodeBooleanFromNullEncoding(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedByte(buffer, encoderState, (byte)1);
         encoder.WriteNull(buffer, encoderState);

         if (fromStream)
         {
            byte? result1 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState);
            byte? result2 = streamDecoder.ReadUnsignedByte(stream, streamDecoderState);

            Assert.AreEqual((byte)1, result1);
            Assert.IsNull(result2);
         }
         else
         {
            byte? result1 = decoder.ReadUnsignedByte(buffer, decoderState);
            byte? result2 = decoder.ReadUnsignedByte(buffer, decoderState);

            Assert.AreEqual((byte)1, result1);
            Assert.IsNull(result2);
         }
      }

      [Test]
      public void TestDecodeBooleanAsPrimitiveWithDefault()
      {
         TestDecodeBooleanAsPrimitiveWithDefault(false);
      }

      [Test]
      public void TestDecodeBooleanAsPrimitiveWithDefaultFS()
      {
         TestDecodeBooleanAsPrimitiveWithDefault(true);
      }

      public void TestDecodeBooleanAsPrimitiveWithDefault(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedByte(buffer, encoderState, (byte)27);
         encoder.WriteNull(buffer, encoderState);

         if (fromStream)
         {
            byte result = streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)0);
            Assert.AreEqual((byte)27, result);
            result = streamDecoder.ReadUnsignedByte(stream, streamDecoderState, (byte)127);
            Assert.AreEqual((byte)127, result);
         }
         else
         {
            byte result = decoder.ReadUnsignedByte(buffer, decoderState, (byte)0);
            Assert.AreEqual((byte)27, result);
            result = decoder.ReadUnsignedByte(buffer, decoderState, (byte)127);
            Assert.AreEqual((byte)127, result);
         }
      }

      [Test]
      public void TestSkipValue()
      {
         doTestSkipValue(false);
      }

      [Test]
      public void TestSkipValueFromStream()
      {
         doTestSkipValue(true);
      }

      public void doTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteUnsignedByte(buffer, encoderState, (byte)i);
         }

         byte expected = (byte)42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(byte), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UByte);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(byte), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UByte);
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
         Assert.IsTrue(result is byte);

         byte value = (byte)result;
         Assert.AreEqual(expected, value);
      }
   }
}