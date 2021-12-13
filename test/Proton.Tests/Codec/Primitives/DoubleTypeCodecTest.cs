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
   public class DoubleTypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadDouble(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadDouble(stream, streamDecoderState, 0.0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadDouble(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadDouble(buffer, decoderState, 0.0);
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Double));
         buffer.WriteDouble(42.0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Double));
         buffer.WriteDouble(43.0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(42.0, streamDecoder.ReadDouble(stream, streamDecoderState));
            Assert.AreEqual(43.0, streamDecoder.ReadDouble(stream, streamDecoderState, 42.0));
            Assert.IsNull(streamDecoder.ReadDouble(stream, streamDecoderState));
            Assert.AreEqual(43.0, streamDecoder.ReadDouble(stream, streamDecoderState, 43.0));
         }
         else
         {
            Assert.AreEqual(42.0, decoder.ReadDouble(buffer, decoderState));
            Assert.AreEqual(43.0, decoder.ReadDouble(buffer, decoderState, 42.0));
            Assert.IsNull(decoder.ReadDouble(buffer, decoderState));
            Assert.AreEqual(43.0, decoder.ReadDouble(buffer, decoderState, 43.0));
         }
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Double, new DoubleTypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(double), new DoubleTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(double), new DoubleTypeDecoder().DecodesType);
      }

      [Test]
      public void TestReadDoubleFromEncodingCode()
      {
         TestReadDoubleFromEncodingCode(false);
      }

      [Test]
      public void TestReadDoubleFromEncodingCodeFS()
      {
         TestReadDoubleFromEncodingCode(true);
      }

      private void TestReadDoubleFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Double));
         buffer.WriteDouble(42);

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadDouble(stream, streamDecoderState));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadDouble(buffer, decoderState));
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
            encoder.WriteDouble(buffer, encoderState, double.MaxValue);
            encoder.WriteDouble(buffer, encoderState, 16.1);
         }

         double expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(double), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(double), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(double), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(double), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Double);

         double value = (double)result;
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

         Double[] source = new Double[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = (double)i;
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
         Assert.AreEqual(typeof(double), result.GetType().GetElementType());

         double[] array = (double[])result;
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

         Double[] source = new Double[0];

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
         Assert.IsTrue(result.GetType().GetElementType() == typeof(double));

         double[] array = (double[])result;
         Assert.AreEqual(source.Length, array.Length);
      }
   }
}