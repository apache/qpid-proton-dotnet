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
   public class UnsignedLongTypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadUnsignedLong(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadUnsignedLong(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadUnsignedLong(buffer, decoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadUByteFromEncodingCode()
      {
         TestReadUByteFromEncodingCode(false);
      }

      [Test]
      public void TestReadUByteFromEncodingCodeFS()
      {
         TestReadUByteFromEncodingCode(true);
      }

      private void TestReadUByteFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong0));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte((byte)43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(0, streamDecoder.ReadUnsignedLong(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadUnsignedLong(stream, streamDecoderState));
            Assert.AreEqual(43, streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 42));
            Assert.IsNull(streamDecoder.ReadUnsignedLong(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 42));
         }
         else
         {
            Assert.AreEqual(0, decoder.ReadUnsignedLong(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadUnsignedLong(buffer, decoderState));
            Assert.AreEqual(43, decoder.ReadUnsignedLong(buffer, decoderState, 42));
            Assert.IsNull(decoder.ReadUnsignedLong(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadUnsignedLong(buffer, decoderState, 42));
         }
      }

      [Test]
      public void TestEncodeDecodeUnsignedLong()
      {
         TestEncodeDecodeUnsignedLong(false);
      }

      [Test]
      public void TestEncodeDecodeUnsignedLongFS()
      {
         TestEncodeDecodeUnsignedLong(true);
      }

      private void TestEncodeDecodeUnsignedLong(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedLong(buffer, encoderState, 640ul);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is ulong);
         Assert.AreEqual(640, (ulong)result);
      }

      [Test]
      public void TestEncodeDecodePrimitive()
      {
         TestEncodeDecodePrimitive(false);
      }

      [Test]
      public void TestEncodeDecodePrimitiveFS()
      {
         TestEncodeDecodePrimitive(true);
      }

      private void TestEncodeDecodePrimitive(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedLong(buffer, encoderState, 640ul);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is ulong);
         Assert.AreEqual(640, (ulong)result);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedLongs()
      {
         doTestDecodeUnsignedLongSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedLongs()
      {
         doTestDecodeUnsignedLongSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedLongsFS()
      {
         doTestDecodeUnsignedLongSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedLongsFS()
      {
         doTestDecodeUnsignedLongSeries(LargeSize, true);
      }

      private void doTestDecodeUnsignedLongSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (uint i = 0; i < size; ++i)
         {
            encoder.WriteUnsignedLong(buffer, encoderState, i);
         }

         for (uint i = 0; i < size; ++i)
         {
            ulong? result;
            if (fromStream)
            {
               result = streamDecoder.ReadUnsignedLong(stream, streamDecoderState);
            }
            else
            {
               result = decoder.ReadUnsignedLong(buffer, decoderState);
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(i, result);
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

         ulong[] source = new ulong[size];
         for (uint i = 0; i < size; ++i)
         {
            source[i] = i;
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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(ulong));

         ulong[] array = (ulong[])result;
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

         ulong[] source = new ulong[0];

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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(ulong));

         ulong[] array = (ulong[])result;
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong0));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(127);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(255);

         if (fromStream)
         {
            ulong? result1 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState);
            ulong? result2 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState);
            ulong? result3 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState);

            Assert.AreEqual(0, result1);
            Assert.AreEqual(127, result2);
            Assert.AreEqual(255, result3);
         }
         else
         {
            ulong? result1 = decoder.ReadUnsignedLong(buffer, decoderState);
            ulong? result2 = decoder.ReadUnsignedLong(buffer, decoderState);
            ulong? result3 = decoder.ReadUnsignedLong(buffer, decoderState);

            Assert.AreEqual(0, result1);
            Assert.AreEqual(127, result2);
            Assert.AreEqual(255, result3);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong0));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(127);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(255);

         if (fromStream)
         {
            ulong result1 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 1);
            ulong result2 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 105);
            ulong result3 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 200);

            Assert.AreEqual(0, result1);
            Assert.AreEqual(127, result2);
            Assert.AreEqual(255, result3);
         }
         else
         {
            ulong result1 = decoder.ReadUnsignedLong(buffer, decoderState, 1);
            ulong result2 = decoder.ReadUnsignedLong(buffer, decoderState, 105);
            ulong result3 = decoder.ReadUnsignedLong(buffer, decoderState, 200);

            Assert.AreEqual(0, result1);
            Assert.AreEqual(127, result2);
            Assert.AreEqual(255, result3);
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

         encoder.WriteUnsignedLong(buffer, encoderState, (byte)1);
         encoder.WriteNull(buffer, encoderState);

         if (fromStream)
         {
            ulong? result1 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState);
            ulong? result2 = streamDecoder.ReadUnsignedLong(stream, streamDecoderState);

            Assert.AreEqual(1, result1);
            Assert.IsNull(result2);
         }
         else
         {
            ulong? result1 = decoder.ReadUnsignedLong(buffer, decoderState);
            ulong? result2 = decoder.ReadUnsignedLong(buffer, decoderState);

            Assert.AreEqual(1, result1);
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

      private void TestDecodeBooleanAsPrimitiveWithDefault(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedLong(buffer, encoderState, 27);
         encoder.WriteNull(buffer, encoderState);

         if (fromStream)
         {
            ulong result = streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 0);
            Assert.AreEqual(27, result);
            result = streamDecoder.ReadUnsignedLong(stream, streamDecoderState, 127);
            Assert.AreEqual(127, result);
         }
         else
         {
            ulong result = decoder.ReadUnsignedLong(buffer, decoderState, 0);
            Assert.AreEqual(27, result);
            result = decoder.ReadUnsignedLong(buffer, decoderState, 127);
            Assert.AreEqual(127, result);
         }
      }

      [Test]
      public void TestWriteLongZeroEncodesAsOneByte()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedLong(buffer, encoderState, 0ul);
         Assert.AreEqual(1, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.ULong0), buffer.ReadUnsignedByte());
      }

      [Test]
      public void TestWriteLongValuesInSmallULongRange()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedLong(buffer, encoderState, 1ul);
         Assert.AreEqual(2, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.SmallULong), buffer.ReadUnsignedByte());
         Assert.AreEqual((byte)1, buffer.ReadByte());
         encoder.WriteUnsignedLong(buffer, encoderState, 64ul);
         Assert.AreEqual(2, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.SmallULong), buffer.ReadUnsignedByte());
         Assert.AreEqual((byte)64, buffer.ReadByte());
         encoder.WriteUnsignedLong(buffer, encoderState, 255ul);
         Assert.AreEqual(2, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.SmallULong), buffer.ReadUnsignedByte());
         Assert.AreEqual((byte)255, buffer.ReadUnsignedByte());
      }

      [Test]
      public void TestWriteLongValuesOutsideOfSmallULongRange()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedLong(buffer, encoderState, 314ul);
         Assert.AreEqual(9, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.ULong), buffer.ReadUnsignedByte());
         Assert.AreEqual(314ul, buffer.ReadUnsignedLong());
      }

      [Test]
      public void TestWriteByteZeroEncodesAsOneByte()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedLong(buffer, encoderState, (byte)0);
         Assert.AreEqual(1, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.ULong0), buffer.ReadUnsignedByte());
      }

      [Test]
      public void TestWriteByteInSmallULongRange()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedLong(buffer, encoderState, (byte)64);
         Assert.AreEqual(2, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.SmallULong), buffer.ReadUnsignedByte());
         Assert.AreEqual((byte)64, buffer.ReadByte());
      }

      [Test]
      public void TestWriteByteAsZeroULong()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedLong(buffer, encoderState, (byte)0);
         Assert.AreEqual(1, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.ULong0), buffer.ReadUnsignedByte());
         Assert.IsFalse(buffer.IsReadable);
      }

      [Test]
      public void TestReadULongZeroDoesNotTouchBuffer()
      {
         TestReadULongZeroDoesNotTouchBuffer(false);
      }

      [Test]
      public void TestReadULongZeroDoesNotTouchBufferFS()
      {
         TestReadULongZeroDoesNotTouchBuffer(true);
      }

      private void TestReadULongZeroDoesNotTouchBuffer(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(1, 1);
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong0));

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
            Assert.IsFalse(stream.Length > 0);
            Assert.AreEqual(0ul, typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
            Assert.IsFalse(buffer.IsReadable);
            Assert.AreEqual(0ul, typeDecoder.ReadValue(buffer, decoderState));
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

         encoder.WriteUnsignedLong(buffer, encoderState, 0);

         for (ulong i = 1; i <= 10; ++i)
         {
            encoder.WriteUnsignedLong(buffer, encoderState, ulong.MaxValue - i);
            encoder.WriteUnsignedLong(buffer, encoderState, i);
         }

         ulong expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.ULong0);
            typeDecoder.SkipValue(stream, streamDecoderState);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.ULong0);
            typeDecoder.SkipValue(buffer, decoderState);
         }

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.ULong);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.SmallULong);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.ULong);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(ulong), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.SmallULong);
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
         Assert.IsTrue(result is ulong);

         ulong value = (ulong)result;
         Assert.AreEqual(expected, value);
      }

      [Test]
      public void TestReadULongArray()
      {
         DoTestReadULongArray(EncodingCodes.ULong, false);
      }

      [Test]
      public void TestReadULongArrayFromStream()
      {
         DoTestReadULongArray(EncodingCodes.ULong, true);
      }

      [Test]
      public void TestReadSmallULongArray()
      {
         DoTestReadULongArray(EncodingCodes.SmallULong, false);
      }

      [Test]
      public void TestReadSmallULongArrayFromStream()
      {
         DoTestReadULongArray(EncodingCodes.SmallULong, true);
      }

      [Test]
      public void TestReadULong0Array()
      {
         DoTestReadULongArray(EncodingCodes.ULong0, false);
      }

      [Test]
      public void TestReadULong0ArrayFromStream()
      {
         DoTestReadULongArray(EncodingCodes.ULong0, true);
      }

      public void DoTestReadULongArray(EncodingCodes encoding, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         if (encoding == EncodingCodes.ULong)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(25);  // Size
            buffer.WriteInt(2);   // Count
            buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
            buffer.WriteUnsignedLong(1ul);   // [0]
            buffer.WriteUnsignedLong(2ul);   // [1]
         }
         else if (encoding == EncodingCodes.SmallULong)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(11);  // Size
            buffer.WriteInt(2);   // Count
            buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
            buffer.WriteUnsignedByte(1);   // [0]
            buffer.WriteUnsignedByte(2);   // [1]
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(9);  // Size
            buffer.WriteInt(2);   // Count
            buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong0));
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
         Assert.IsTrue(result.GetType().IsArray);
         Assert.AreEqual(result.GetType().GetElementType(), typeof(ulong));

         ulong[] array = (ulong[])result;

         Assert.AreEqual(2, array.Length);

         if (encoding == EncodingCodes.ULong0)
         {
            Assert.AreEqual(0ul, array[0]);
            Assert.AreEqual(0ul, array[1]);
         }
         else
         {
            Assert.AreEqual(1ul, array[0]);
            Assert.AreEqual(2ul, array[1]);
         }
      }
   }
}