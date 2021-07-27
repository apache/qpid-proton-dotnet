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
   public class UnsignedShortTypeCodecTest : CodecTestSupport
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
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadUnsignedShort(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadUnsignedShort(stream, streamDecoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadUnsignedShort(stream, streamDecoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadUnsignedShort(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadUnsignedShort(buffer, decoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadUnsignedShort(buffer, decoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestTypeFromEncodingCode()
      {
         TestTypeFromEncodingCode(false);
      }

      [Test]
      public void TestTypeFromEncodingCodeFS()
      {
         TestTypeFromEncodingCode(true);
      }

      public void TestTypeFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UShort));
         buffer.WriteUnsignedShort(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UShort));
         buffer.WriteUnsignedShort(43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadUnsignedShort(stream, streamDecoderState));
            Assert.AreEqual(43, streamDecoder.ReadUnsignedShort(stream, streamDecoderState, 42));
            Assert.IsNull(streamDecoder.ReadUnsignedShort(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadUnsignedShort(stream, streamDecoderState, 42));
            Assert.AreEqual(43, streamDecoder.ReadUnsignedShort(stream, streamDecoderState, 43));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadUnsignedShort(buffer, decoderState));
            Assert.AreEqual(43, decoder.ReadUnsignedShort(buffer, decoderState, 42));
            Assert.IsNull(decoder.ReadUnsignedShort(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadUnsignedShort(buffer, decoderState, 42));
            Assert.AreEqual(43, decoder.ReadUnsignedShort(buffer, decoderState, 43));
         }
      }

      [Test]
      public void TestEncodeDecodeUnsignedShort()
      {
         TestEncodeDecodeUnsignedShort(false);
      }

      [Test]
      public void TestEncodeDecodeUnsignedShortFS()
      {
         TestEncodeDecodeUnsignedShort(true);
      }

      public void TestEncodeDecodeUnsignedShort(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedShort(buffer, encoderState, 64);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is ushort);
         Assert.AreEqual(64, (ushort)result);
      }

      [Test]
      public void TestEncodeDecodeUnsignedShortAbove32k()
      {
         TestEncodeDecodeUnsignedShortAbove32k(false);
      }

      [Test]
      public void TestEncodeDecodeUnsignedShortAbove32kFS()
      {
         TestEncodeDecodeUnsignedShortAbove32k(true);
      }

      private void TestEncodeDecodeUnsignedShortAbove32k(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedShort(buffer, encoderState, 33565);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is ushort);
         Assert.AreEqual(33565, (ushort)result);
      }

      [Test]
      public void TestEncodeDecodeShort()
      {
         TestEncodeDecodeShort(false);
      }

      [Test]
      public void TestEncodeDecodeShortFS()
      {
         TestEncodeDecodeShort(true);
      }

      private void TestEncodeDecodeShort(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedShort(buffer, encoderState, 64);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is ushort);
         Assert.AreEqual(64, (ushort)result);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedShorts()
      {
         doTestDecodeUnsignedShortSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedShorts()
      {
         doTestDecodeUnsignedShortSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedShortsFS()
      {
         doTestDecodeUnsignedShortSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedShortsFS()
      {
         doTestDecodeUnsignedShortSeries(LargeSize, true);
      }

      private void doTestDecodeUnsignedShortSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteUnsignedShort(buffer, encoderState, (byte)(i % 255));
         }

         for (int i = 0; i < size; ++i)
         {
            ushort? result;
            if (fromStream)
            {
               result = streamDecoder.ReadUnsignedShort(stream, streamDecoderState);
            }
            else
            {
               result = decoder.ReadUnsignedShort(buffer, decoderState);
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

         ushort[] source = new ushort[size];
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
         Assert.AreEqual(typeof(ushort), result.GetType().GetElementType());

         ushort[] array = (ushort[])result;
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

         ushort[] source = new ushort[0];

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
         Assert.AreEqual(typeof(ushort), result.GetType().GetElementType());

         ushort[] array = (ushort[])result;
         Assert.AreEqual(source.Length, array.Length);
      }

      [Test]
      public void TestSkipValue()
      {
         DoTestSkipValue(false);
      }

      [Test]
      public void TestSkipValueFromStream()
      {
         DoTestSkipValue(true);
      }

      public void DoTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (ushort i = 0; i < 10; ++i)
         {
            encoder.WriteUnsignedShort(buffer, encoderState, i);
         }

         ushort expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(ushort), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UShort);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(ushort), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UShort);
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
         Assert.IsTrue(result is ushort);

         ushort value = (ushort)result;
         Assert.AreEqual(expected, value);
      }
   }
}