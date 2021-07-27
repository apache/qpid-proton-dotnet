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
using System;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class LongTypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadLong(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadLong(stream, streamDecoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadLong(stream, streamDecoderState, 0L);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadLong(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadLong(buffer, decoderState, 0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadLong(buffer, decoderState, 0L);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Long));
         buffer.WriteLong(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Long));
         buffer.WriteLong(44);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallLong));
         buffer.WriteUnsignedByte(43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadLong(stream, streamDecoderState));
            Assert.AreEqual(44, streamDecoder.ReadLong(stream, streamDecoderState, 42));
            Assert.AreEqual(43, streamDecoder.ReadLong(stream, streamDecoderState, 42));
            Assert.IsNull(streamDecoder.ReadLong(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadLong(stream, streamDecoderState, 42L));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadLong(buffer, decoderState));
            Assert.AreEqual(44, decoder.ReadLong(buffer, decoderState, 42));
            Assert.AreEqual(43, decoder.ReadLong(buffer, decoderState, 42));
            Assert.IsNull(decoder.ReadLong(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadLong(buffer, decoderState, 42L));
         }
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Long, new Long32TypeDecoder().EncodingCode);
         Assert.AreEqual(EncodingCodes.SmallLong, new Long8TypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(long), new LongTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(long), new Long8TypeDecoder().DecodesType);
      }

      [Test]
      public void TestReadLongFromEncodingCodeLong()
      {
         TestReadLongFromEncodingCodeLong(false);
      }

      [Test]
      public void TestReadLongFromEncodingCodeLongFS()
      {
         TestReadLongFromEncodingCodeLong(true);
      }

      private void TestReadLongFromEncodingCodeLong(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Long));
         buffer.WriteLong(42);

         if (fromStream)
         {
            Assert.AreEqual(42L, streamDecoder.ReadLong(stream, streamDecoderState));
         }
         else
         {
            Assert.AreEqual(42L, decoder.ReadLong(buffer, decoderState));
         }
      }

      [Test]
      public void TestReadLongFromEncodingCodeSmallLong()
      {
         TestReadLongFromEncodingCodeSmallLong(false);
      }

      [Test]
      public void TestReadLongFromEncodingCodeSmallLongFS()
      {
         TestReadLongFromEncodingCodeSmallLong(true);
      }

      private void TestReadLongFromEncodingCodeSmallLong(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallLong));
         buffer.WriteUnsignedByte(42);

         if (fromStream)
         {
            Assert.AreEqual(42L, streamDecoder.ReadLong(stream, streamDecoderState));
         }
         else
         {
            Assert.AreEqual(42L, decoder.ReadLong(buffer, decoderState));
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
            encoder.WriteLong(buffer, encoderState, long.MaxValue);
            encoder.WriteLong(buffer, encoderState, 16);
         }

         long expected = 42L;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(long), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(long), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(long), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(long), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is long);

         long value = (long)result;
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

         long[] source = new long[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = random.Next();
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
         Assert.AreEqual(typeof(long), result.GetType().GetElementType());

         long[] array = (long[])result;
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

         long[] source = new long[0];

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
         Assert.AreEqual(typeof(long), result.GetType().GetElementType());

         long[] array = (long[])result;
         Assert.AreEqual(source.Length, array.Length);
      }

      [Test]
      public void TestReadLongArray()
      {
         doTestReadLongArray(EncodingCodes.Long, false);
      }

      [Test]
      public void TestReadLongArrayFromStream()
      {
         doTestReadLongArray(EncodingCodes.Long, true);
      }

      [Test]
      public void TestReadSmallLongArray()
      {
         doTestReadLongArray(EncodingCodes.SmallLong, false);
      }

      [Test]
      public void TestReadSmallLongArrayFromStream()
      {
         doTestReadLongArray(EncodingCodes.SmallLong, true);
      }

      public void doTestReadLongArray(EncodingCodes encoding, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         if (encoding == EncodingCodes.Long)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(25);  // Size
            buffer.WriteInt(2);   // Count
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Long));
            buffer.WriteLong(1L);   // [0]
            buffer.WriteLong(2L);   // [1]
         }
         else if (encoding == EncodingCodes.SmallLong)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(11);  // Size
            buffer.WriteInt(2);   // Count
            buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallLong));
            buffer.WriteUnsignedByte(1);   // [0]
            buffer.WriteUnsignedByte(2);   // [1]
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
         Assert.AreEqual(typeof(long), result.GetType().GetElementType());

         long[] array = (long[])result;

         Assert.AreEqual(2, array.Length);
         Assert.AreEqual(1, array[0]);
         Assert.AreEqual(2, array[1]);
      }
   }
}