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

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class UUIDTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType()
      {
         TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType(false);
      }

      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsThisTypeFromStream()
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
               streamDecoder.ReadGuid(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadGuid(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadFromNullEncodingCode()
      {
         TestReadFromNullEncodingCode(false);
      }

      [Test]
      public void TestReadFromNullEncodingCodeFromStream()
      {
         TestReadFromNullEncodingCode(true);
      }

      private void TestReadFromNullEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid value = Guid.NewGuid();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteBytes(value.ToByteArray());

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadGuid(stream, streamDecoderState));
            Assert.AreEqual(value, streamDecoder.ReadGuid(stream, streamDecoderState));
         }
         else
         {
            Assert.IsNull(decoder.ReadGuid(buffer, decoderState));
            Assert.AreEqual(value, decoder.ReadGuid(buffer, decoderState));
         }
      }

      [Test]
      public void TestEncodeDecodeUUID()
      {
         DoTestEncodeDecodeUUIDSeries(1, false);
      }

      [Test]
      public void TestEncodeDecodeSmallSeriesOfUUIDs()
      {
         DoTestEncodeDecodeUUIDSeries(SmallSize, false);
      }

      [Test]
      public void TestEncodeDecodeLargeSeriesOfUUIDs()
      {
         DoTestEncodeDecodeUUIDSeries(LargeSize, false);
      }

      [Test]
      public void TestEncodeDecodeUUIDFromStream()
      {
         DoTestEncodeDecodeUUIDSeries(1, true);
      }

      [Test]
      public void TestEncodeDecodeSmallSeriesOfUUIDsFromStream()
      {
         DoTestEncodeDecodeUUIDSeries(SmallSize, true);
      }

      [Test]
      public void TestEncodeDecodeLargeSeriesOfUUIDsFromStream()
      {
         DoTestEncodeDecodeUUIDSeries(LargeSize, true);
      }

      private void DoTestEncodeDecodeUUIDSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[] source = new Guid[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = Guid.NewGuid();
         }

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, source[i]);
         }

         for (int i = 0; i < size; ++i)
         {
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
            Assert.IsTrue(result is Guid);

            Guid decoded = (Guid)result;

            Assert.AreEqual(source[i], decoded);
         }
      }

      [Test]
      public void TestDecodeSmallUUIDArray()
      {
         DoTestDecodeUUIDArrayType(SmallArraySize, false);
      }

      [Test]
      public void TestDecodeLargeUUIDArray()
      {
         DoTestDecodeUUIDArrayType(LargeArraySize, false);
      }

      [Test]
      public void TestDecodeSmallUUIDArrayFromStream()
      {
         DoTestDecodeUUIDArrayType(SmallArraySize, true);
      }

      [Test]
      public void TestDecodeLargeUUIDArrayFromStream()
      {
         DoTestDecodeUUIDArrayType(LargeArraySize, true);
      }

      private void DoTestDecodeUUIDArrayType(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[] source = new Guid[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = Guid.NewGuid();
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

         Guid[] array = (Guid[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      [Test]
      public void TestWriteUUIDArrayWithMixedNullAndNotNullValues()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         object[] source = new object[2];
         source[0] = Guid.NewGuid();
         source[1] = null;

         try
         {
            encoder.WriteArray(buffer, encoderState, source);
            Assert.Fail("Should not be able to encode array with mixed null and non-null values");
         }
         catch (Exception) { }

         source = new object[2];
         source[0] = null;
         source[1] = Guid.NewGuid();

         try
         {
            encoder.WriteArray(buffer, encoderState, source);
            Assert.Fail("Should not be able to encode array with mixed null and non-null values");
         }
         catch (Exception) { }
      }

      [Test]
      public void TestWriteUUIDArrayWithZeroSize()
      {
         TestWriteUUIDArrayWithZeroSize(false);
      }

      [Test]
      public void TestWriteUUIDArrayWithZeroSizeFromStream()
      {
         TestWriteUUIDArrayWithZeroSize(true);
      }

      private void TestWriteUUIDArrayWithZeroSize(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[] source = new Guid[0];
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

         Guid[] array = (Guid[])result;
         Assert.AreEqual(0, array.Length);
      }

      [Test]
      public void TestObjectArrayContainingUUID()
      {
         TestObjectArrayContainingUUID(false);
      }

      [Test]
      public void TestObjectArrayContainingUUIDFromStream()
      {
         TestObjectArrayContainingUUID(true);
      }

      private void TestObjectArrayContainingUUID(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Object[] source = new Object[10];
         for (int i = 0; i < 10; ++i)
         {
            source[i] = Guid.NewGuid();
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

         Guid[] array = (Guid[])result;
         Assert.AreEqual(10, array.Length);

         for (int i = 0; i < 10; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      [Test]
      public void TestWriteArrayOfUUIDArrayWithZeroSize()
      {
         TestWriteArrayOfUUIDArrayWithZeroSize(false);
      }

      [Test]
      public void TestWriteArrayOfUUIDArrayWithZeroSizeFromStream()
      {
         TestWriteArrayOfUUIDArrayWithZeroSize(true);
      }

      private void TestWriteArrayOfUUIDArrayWithZeroSize(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[][] source = new Guid[2][];

         source[0] = new Guid[0];
         source[1] = new Guid[0];

         try
         {
            encoder.WriteArray(buffer, encoderState, source);
         }
         catch (Exception)
         {
            Assert.Fail("Should be able to encode array with no size");
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

         Object[] resultArray = (Object[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Object nested = resultArray[i];
            Assert.IsNotNull(result);
            Assert.IsTrue(result.GetType().IsArray);

            Guid[] uuids = (Guid[])nested;
            Assert.AreEqual(0, uuids.Length);
         }
      }

      [Test]
      public void TestSkipValue()
      {
         TestSkipValue(false);
      }

      [Test]
      public void TestSkipValueFromStream()
      {
         TestSkipValue(true);
      }

      private void TestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteGuid(buffer, encoderState, Guid.NewGuid());
         }

         Guid expected = Guid.NewGuid();

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Uuid);
               Assert.AreEqual(typeof(Guid), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Uuid);
               Assert.AreEqual(typeof(Guid), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Guid);

         Guid value = (Guid)result;
         Assert.AreEqual(expected, value);
      }

      [Test]
      public void TestArrayOfObjects()
      {
         TestArrayOfObjects(false);
      }

      [Test]
      public void TestArrayOfObjectsFromStream()
      {
         TestArrayOfObjects(true);
      }

      private void TestArrayOfObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         Guid[] source = new Guid[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = Guid.NewGuid();
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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(Guid));

         Guid[] array = (Guid[])result;
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
      public void TestZeroSizedArrayOfObjectsFromStream()
      {
         TestZeroSizedArrayOfObjects(true);
      }

      private void TestZeroSizedArrayOfObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[] source = new Guid[0];

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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(Guid));

         Guid[] array = (Guid[])result;
         Assert.AreEqual(source.Length, array.Length);
      }
   }
}