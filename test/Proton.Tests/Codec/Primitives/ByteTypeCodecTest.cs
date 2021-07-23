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
using System;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Codec.Encoders.Primitives;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class ByteTypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadByte(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadByte(stream, streamDecoderState, (sbyte)0);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadByte(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadByte(buffer, decoderState, (sbyte)0);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Byte, new ByteTypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(sbyte), new ByteTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(sbyte), new ByteTypeDecoder().DecodesType);
      }

      [Test]
      public void TestPeekNextTypeDecoder()
      {
         TestPeekNextTypeDecoder(false);
      }

      [Test]
      public void TestPeekNextTypeDecoderFS()
      {
         TestPeekNextTypeDecoder(true);
      }

      private void TestPeekNextTypeDecoder(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Byte));
         buffer.WriteByte((sbyte)42);

         if (fromStream)
         {
            Assert.AreEqual(typeof(sbyte), streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState).DecodesType);
            Assert.AreEqual(42, streamDecoder.ReadByte(stream, streamDecoderState));
         }
         else
         {
            Assert.AreEqual(typeof(sbyte), decoder.PeekNextTypeDecoder(buffer, decoderState).DecodesType);
            Assert.AreEqual(42, decoder.ReadByte(buffer, decoderState));
         }
      }

      [Test]
      public void TestReadByteFromEncodingCode()
      {
         TestReadByteFromEncodingCode(false);
      }

      [Test]
      public void TestReadByteFromEncodingCodeFS()
      {
         TestReadByteFromEncodingCode(true);
      }

      private void TestReadByteFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Byte));
         buffer.WriteByte((sbyte)42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Byte));
         buffer.WriteByte((sbyte)43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadByte(stream, streamDecoderState));
            Assert.AreEqual(43, streamDecoder.ReadByte(stream, streamDecoderState, 42));
            Assert.IsNull(streamDecoder.ReadByte(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadByte(stream, streamDecoderState, 42));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadByte(buffer, decoderState));
            Assert.AreEqual(43, decoder.ReadByte(buffer, decoderState, 42));
            Assert.IsNull(decoder.ReadByte(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadByte(buffer, decoderState, 42));
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
            encoder.WriteByte(buffer, encoderState, sbyte.MaxValue);
            encoder.WriteByte(buffer, encoderState, (sbyte)16);
         }

         sbyte expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(sbyte), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(sbyte), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(sbyte), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(sbyte), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is sbyte);

         sbyte value = (sbyte)result;
         Assert.AreEqual(expected, value);
      }

      // TODO [Test]
      public void TestArrayOfSignedBytes()
      {
         TestArrayOfSignedBytes(false);
      }

      // TODO [Test]
      public void TestArrayOfSignedBytesFS()
      {
         TestArrayOfSignedBytes(true);
      }

      private void TestArrayOfSignedBytes(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Random random = new Random(Environment.TickCount);

         const int size = 10;

         sbyte[] source = new sbyte[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = (sbyte)(random.Next() & 0xFF);
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

         Byte[] source = new Byte[0];

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

         byte[] array = (byte[])result;
         Assert.AreEqual(source.Length, array.Length);
      }
   }
}