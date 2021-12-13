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

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class FloatTypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadFloat(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadFloat(stream, streamDecoderState, 0f);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadFloat(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadFloat(buffer, decoderState, 0f);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadPrimitiveTypeFromEncodingCode()
      {
         TestReadPrimitiveTypeFromEncodingCode(false);
      }

      [Test]
      public void TestReadPrimitiveTypeFromEncodingCodeFS()
      {
         TestReadPrimitiveTypeFromEncodingCode(true);
      }

      private void TestReadPrimitiveTypeFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Float));
         buffer.WriteFloat(42.0f);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Float));
         buffer.WriteFloat(43.0f);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(42f, streamDecoder.ReadFloat(stream, streamDecoderState));
            Assert.AreEqual(43f, streamDecoder.ReadFloat(stream, streamDecoderState, (short)42));
            Assert.IsNull(streamDecoder.ReadFloat(stream, streamDecoderState));
            Assert.AreEqual(43f, streamDecoder.ReadFloat(stream, streamDecoderState, 43f));
         }
         else
         {
            Assert.AreEqual(42f, decoder.ReadFloat(buffer, decoderState));
            Assert.AreEqual(43f, decoder.ReadFloat(buffer, decoderState, (short)42));
            Assert.IsNull(decoder.ReadFloat(buffer, decoderState));
            Assert.AreEqual(43f, decoder.ReadFloat(buffer, decoderState, 43f));
         }
      }

      [Test]
      public void TestEncodeAndDecodeArrayOfPrimitiveFlosts()
      {
         DoTestEncodeAndDecodeArrayOfPrimitiveFlosts(false);
      }

      [Test]
      public void TestEncodeAndDecodeArrayOfPrimitiveFlostsFromStream()
      {
         DoTestEncodeAndDecodeArrayOfPrimitiveFlosts(true);
      }

      private void DoTestEncodeAndDecodeArrayOfPrimitiveFlosts(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         float[] floats = new float[] { 0.1f, 0.2f, 1.1f, 1.2f };

         encoder.WriteArray(buffer, encoderState, floats);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result.GetType().IsArray);
         Assert.AreEqual(typeof(float), result.GetType().GetElementType());

         float[] resultArray = (float[])result;

         Assert.AreEqual(floats, resultArray);
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Float, new FloatTypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(float), new FloatTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(float), new FloatTypeDecoder().DecodesType);
      }

      [Test]
      public void TestReadFloatFromEncodingCode()
      {
         TestReadFloatFromEncodingCode(false);
      }

      [Test]
      public void TestReadFloatFromEncodingCodeFS()
      {
         TestReadFloatFromEncodingCode(true);
      }

      private void TestReadFloatFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Float));
         buffer.WriteFloat(42);

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadFloat(stream, streamDecoderState));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadFloat(buffer, decoderState));
         }
      }

      [Test]
      public void TestSkipValue()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteFloat(buffer, encoderState, float.MaxValue);
            encoder.WriteFloat(buffer, encoderState, 16.1f);
         }

         float expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(float), typeDecoder.DecodesType);
            typeDecoder.SkipValue(buffer, decoderState);
            typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(float), typeDecoder.DecodesType);
            typeDecoder.SkipValue(buffer, decoderState);
         }

         object result = decoder.ReadObject(buffer, decoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is float);

         float value = (float)result;
         Assert.AreEqual(expected, value, 0.1f);
      }

      [Test]
      public void TestSkipValueFromStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteFloat(buffer, encoderState, float.MaxValue);
            encoder.WriteFloat(buffer, encoderState, 16.1f);
         }

         float expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(float), typeDecoder.DecodesType);
            typeDecoder.SkipValue(stream, streamDecoderState);
            typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(float), typeDecoder.DecodesType);
            typeDecoder.SkipValue(stream, streamDecoderState);
         }

         object result = streamDecoder.ReadObject(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is float);

         float value = (float)result;
         Assert.AreEqual(expected, value, 0.1f);
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

         float[] source = new float[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = (float)random.Next();
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
         Assert.AreEqual(typeof(float), result.GetType().GetElementType());

         float[] array = (float[])result;
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

         float[] source = new float[0];

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
         Assert.AreEqual(typeof(float), result.GetType().GetElementType());

         float[] array = (float[])result;
         Assert.AreEqual(source.Length, array.Length);
      }
   }
}