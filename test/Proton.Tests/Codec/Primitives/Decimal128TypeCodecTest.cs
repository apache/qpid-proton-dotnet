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
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Codec.Encoders.Primitives;
using Apache.Qpid.Proton.Types;
using System;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class Decimal128TypeCodecTest : CodecTestSupport
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

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadDecimal128(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadDecimal128(buffer, decoderState);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Decimal128));
         buffer.WriteLong(42);
         buffer.WriteLong(43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Decimal128 result;
         if (fromStream)
         {
            result = streamDecoder.ReadDecimal128(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadDecimal128(buffer, decoderState);
         }

         Assert.AreEqual(42, result.MostSignificantBits);
         Assert.AreEqual(43, result.LeastSignificantBits);

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadDecimal128(stream, streamDecoderState));
         }
         else
         {
            Assert.IsNull(decoder.ReadDecimal128(buffer, decoderState));
         }
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Decimal128, new Decimal128TypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(Decimal128), new Decimal128TypeEncoder().EncodesType);
         Assert.AreEqual(typeof(Decimal128), new Decimal128TypeDecoder().DecodesType);
      }

      [Test]
      public void TestReadFromEncodingCode()
      {
         TestReadFromEncodingCode(false);
      }

      [Test]
      public void TestReadFromEncodingCodeFS()
      {
         TestReadFromEncodingCode(true);
      }

      private void TestReadFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Decimal128));
         buffer.WriteLong(42);
         buffer.WriteLong(84);

         Decimal128 result;
         if (fromStream)
         {
            result = streamDecoder.ReadDecimal128(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadDecimal128(buffer, decoderState);
         }

         Assert.AreEqual(42, result.MostSignificantBits);
         Assert.AreEqual(84, result.LeastSignificantBits);
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

         for (ulong i = 0; i < 10; ++i)
         {
            encoder.WriteDecimal128(buffer, encoderState, new Decimal128(ulong.MaxValue - i, 42));
            encoder.WriteDecimal128(buffer, encoderState, new Decimal128(i, i));
         }

         Decimal128 expected = new Decimal128(42, 42);

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Decimal128), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Decimal128), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Decimal128), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Decimal128), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Decimal128);

         Decimal128 value = (Decimal128)result;
         Assert.AreEqual(expected, value);
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
         Random random = new Random(Environment.TickCount);

         const int size = 10;

         Decimal128[] source = new Decimal128[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = new Decimal128((ulong)random.Next(), (ulong)random.Next());
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

         Decimal128[] array = (Decimal128[])result;
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

         Decimal128[] source = new Decimal128[0];

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

         Decimal128[] array = (Decimal128[])result;
         Assert.AreEqual(source.Length, array.Length);
      }
   }
}