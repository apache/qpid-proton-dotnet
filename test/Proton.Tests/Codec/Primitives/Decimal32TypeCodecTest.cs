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
   public class Decimal32TypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadDecimal32(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadDecimal32(buffer, decoderState);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Decimal32));
         buffer.WriteInt(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Decimal32 result;
         if (fromStream)
         {
            result = streamDecoder.ReadDecimal32(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadDecimal32(buffer, decoderState);
         }

         Assert.AreEqual(42, result.Bits);

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadDecimal32(stream, streamDecoderState));
         }
         else
         {
            Assert.IsNull(decoder.ReadDecimal32(buffer, decoderState));
         }
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Decimal32, new Decimal32TypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(Decimal32), new Decimal32TypeEncoder().EncodesType);
         Assert.AreEqual(typeof(Decimal32), new Decimal32TypeDecoder().DecodesType);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Decimal32));
         buffer.WriteInt(42);

         Decimal32 result;
         if (fromStream)
         {
            result = streamDecoder.ReadDecimal32(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadDecimal32(buffer, decoderState);
         }

         Assert.AreEqual(42, result.Bits);
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
            encoder.WriteDecimal32(buffer, encoderState, new Decimal32(41));
            encoder.WriteDecimal32(buffer, encoderState, new Decimal32(40));
         }

         Decimal32 expected = new Decimal32(42);

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Decimal32), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Decimal32), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Decimal32), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Decimal32), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Decimal32);

         Decimal32 value = (Decimal32)result;
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

         Decimal32[] source = new Decimal32[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = new Decimal32((uint)random.Next());
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

         Decimal32[] array = (Decimal32[])result;
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

         Decimal32[] source = new Decimal32[0];

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

         Decimal32[] array = (Decimal32[])result;
         Assert.AreEqual(source.Length, array.Length);
      }
   }
}