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
   public class UnsignedIntegerTypeCodecTest : CodecTestSupport
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadUnsignedInteger(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 0u);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadUnsignedInteger(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadUnsignedInteger(buffer, decoderState, 0u);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt0));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
         buffer.WriteInt(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallUInt));
         buffer.WriteUnsignedByte((byte)43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(0, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState));
            Assert.AreEqual(43, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 42));
            Assert.IsNull(streamDecoder.ReadUnsignedInteger(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 42));
         }
         else
         {
            Assert.AreEqual(0, decoder.ReadUnsignedInteger(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadUnsignedInteger(buffer, decoderState));
            Assert.AreEqual(43, decoder.ReadUnsignedInteger(buffer, decoderState, 42));
            Assert.IsNull(decoder.ReadUnsignedInteger(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadUnsignedInteger(buffer, decoderState, 42));
         }
      }

      [Test]
      public void TestEncodeDecodeUnsignedInteger()
      {
         TestEncodeDecodeUnsignedInteger(false);
      }

      [Test]
      public void TestEncodeDecodeUnsignedIntegerFS()
      {
         TestEncodeDecodeUnsignedInteger(true);
      }

      private void TestEncodeDecodeUnsignedInteger(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedInteger(buffer, encoderState, 640);

         if (fromStream)
         {
            object result = streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(640, result);
         }
         else
         {
            object result = decoder.ReadObject(buffer, decoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(640, result);
         }
      }

      [Test]
      public void TestEncodeDecodeInteger()
      {
         TestEncodeDecodeInteger(false);
      }

      [Test]
      public void TestEncodeDecodeIntegerFS()
      {
         TestEncodeDecodeInteger(true);
      }

      private void TestEncodeDecodeInteger(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteUnsignedInteger(buffer, encoderState, 640);
         encoder.WriteUnsignedInteger(buffer, encoderState, 0);
         encoder.WriteUnsignedInteger(buffer, encoderState, 255);
         encoder.WriteUnsignedInteger(buffer, encoderState, 254);

         if (fromStream)
         {
            object result = streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(640, (uint)result);
            result = streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(0, (uint)result);
            result = streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(255, (uint)result);
            result = streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(254, (uint)result);
         }
         else
         {
            object result = decoder.ReadObject(buffer, decoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(640, (uint)result);
            result = decoder.ReadObject(buffer, decoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(0, (uint)result);
            result = decoder.ReadObject(buffer, decoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(255, (uint)result);
            result = decoder.ReadObject(buffer, decoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(254, (uint)result);
         }
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

         encoder.WriteUnsignedInteger(buffer, encoderState, (byte)64);
         encoder.WriteUnsignedInteger(buffer, encoderState, (byte)0);

         if (fromStream)
         {
            object result = streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(64, ((uint)result));
            result = streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(0, ((uint)result));
         }
         else
         {
            object result = decoder.ReadObject(buffer, decoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(64, (uint)result);
            result = decoder.ReadObject(buffer, decoderState);
            Assert.IsTrue(result is uint);
            Assert.AreEqual(0, (uint)result);
         }
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedIntegers()
      {
         DoTestDecodeUnsignedIntegerSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedIntegers()
      {
         DoTestDecodeUnsignedIntegerSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnsignedIntegersFS()
      {
         DoTestDecodeUnsignedIntegerSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnsignedIntegersFS()
      {
         DoTestDecodeUnsignedIntegerSeries(LargeSize, true);
      }

      private void DoTestDecodeUnsignedIntegerSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (uint i = 0; i < size; ++i)
         {
            encoder.WriteUnsignedInteger(buffer, encoderState, i);
         }

         for (int i = 0; i < size; ++i)
         {
            uint? result;
            if (fromStream)
            {
               result = streamDecoder.ReadUnsignedInteger(stream, streamDecoderState);
            }
            else
            {
               result = decoder.ReadUnsignedInteger(buffer, decoderState);
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

         uint[] source = new uint[size];
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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(uint));

         uint[] array = (uint[])result;
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

         uint[] source = new uint[0];

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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(uint));

         uint[] array = (uint[])result;
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

         encoder.WriteUnsignedInteger(buffer, encoderState, 0u);

         for (uint i = 1; i <= 10; ++i)
         {
            encoder.WriteUnsignedInteger(buffer, encoderState, (uint)(uint.MaxValue - i));
            encoder.WriteUnsignedInteger(buffer, encoderState, i);
         }

         uint expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(uint), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UInt0);
            typeDecoder.SkipValue(stream, streamDecoderState);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(uint), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UInt0);
            typeDecoder.SkipValue(buffer, decoderState);
         }

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(uint), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UInt);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(uint), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.SmallUInt);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(uint), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.UInt);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(uint), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.SmallUInt);
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
         Assert.IsTrue(result is uint);

         uint value = (uint)result;
         Assert.AreEqual(expected, value);
      }

   }
}