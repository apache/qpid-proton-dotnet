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
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Codec.Decoders.Messaging;
using Apache.Qpid.Proton.Codec.Encoders.Messaging;
using System.Collections.Generic;
using System;

namespace Apache.Qpid.Proton.Codec.Messaging
{
   [TestFixture]
   public class ApplicationPropertiesTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(ApplicationProperties), new ApplicationPropertiesTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(ApplicationProperties), new ApplicationPropertiesTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(ApplicationProperties.DescriptorCode, new ApplicationPropertiesTypeDecoder().DescriptorCode);
         Assert.AreEqual(ApplicationProperties.DescriptorCode, new ApplicationPropertiesTypeEncoder().DescriptorCode);
         Assert.AreEqual(ApplicationProperties.DescriptorSymbol, new ApplicationPropertiesTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(ApplicationProperties.DescriptorSymbol, new ApplicationPropertiesTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeSmallSeriesOfApplicationProperties()
      {
         DoTestDecodeHeaderSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfApplicationProperties()
      {
         DoTestDecodeHeaderSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfApplicationPropertiesFromStream()
      {
         DoTestDecodeHeaderSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfApplicationPropertiesFromStream()
      {
         DoTestDecodeHeaderSeries(LargeSize, true);
      }

      private void DoTestDecodeHeaderSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<string, object> propertiesMap = new Dictionary<string, object>();
         ApplicationProperties properties = new ApplicationProperties(propertiesMap);

         propertiesMap.Add("key-1", "1");
         propertiesMap.Add("key-2", "2");
         propertiesMap.Add("key-3", "3");
         propertiesMap.Add("key-4", "4");
         propertiesMap.Add("key-5", "5");
         propertiesMap.Add("key-6", "6");
         propertiesMap.Add("key-7", "7");
         propertiesMap.Add("key-8", "8");

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, properties);
         }

         for (int i = 0; i < size; ++i)
         {
            ApplicationProperties result;
            if (fromStream)
            {
               result = streamDecoder.ReadObject<ApplicationProperties>(stream, streamDecoderState);
            }
            else
            {
               result = decoder.ReadObject<ApplicationProperties>(buffer, decoderState);
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(8, result.Value.Count);
            Assert.AreEqual(result.Value, propertiesMap);
         }
      }

      [Test]
      public void TestEncodeDecodeZeroSizedArrayOfApplicationProperties()
      {
         DoTestEncodeDecodeZeroSizedArrayOfApplicationProperties(false);
      }

      [Test]
      public void TestEncodeDecodeZeroSizedArrayOfApplicationPropertiesFromStream()
      {
         DoTestEncodeDecodeZeroSizedArrayOfApplicationProperties(true);
      }

      private void DoTestEncodeDecodeZeroSizedArrayOfApplicationProperties(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         ApplicationProperties[] array = new ApplicationProperties[0];

         encoder.WriteObject(buffer, encoderState, array);

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
         Assert.AreEqual(typeof(ApplicationProperties), result.GetType().GetElementType());

         ApplicationProperties[] resultArray = (ApplicationProperties[])result;
         Assert.AreEqual(0, resultArray.Length);
      }

      [Test]
      public void TestEncodeDecodeArrayOfApplicationProperties()
      {
         TestEncodeDecodeArrayOfApplicationProperties(false);
      }

      [Test]
      public void TestEncodeDecodeArrayOfApplicationPropertiesFromStream()
      {
         TestEncodeDecodeArrayOfApplicationProperties(true);
      }

      private void TestEncodeDecodeArrayOfApplicationProperties(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         ApplicationProperties[] array = new ApplicationProperties[3];

         array[0] = new ApplicationProperties(new Dictionary<string, object>());
         array[1] = new ApplicationProperties(new Dictionary<string, object>());
         array[2] = new ApplicationProperties(new Dictionary<string, object>());

         array[0].Value.Add("key-1", "1");
         array[1].Value.Add("key-1", "2");
         array[2].Value.Add("key-1", "3");

         encoder.WriteObject(buffer, encoderState, array);

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
         Assert.AreEqual(typeof(ApplicationProperties), result.GetType().GetElementType());

         ApplicationProperties[] resultArray = (ApplicationProperties[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is ApplicationProperties);
            Assert.AreEqual(array[i].Value, resultArray[i].Value);
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

      private void doTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<string, object> map = new Dictionary<string, object>();
         map.Add("one", 1);
         map.Add("two", true);
         map.Add("three", "test");

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, new ApplicationProperties(map));
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Modified);
         Modified modified = (Modified)result;
         Assert.IsFalse(modified.UndeliverableHere);
         Assert.IsFalse(modified.DeliveryFailed);
      }

      [Test]
      public void TestEncodeDecodeMessageAnnotationsWithEmptyValue()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         Assert.Throws<EncodeException>(() => encoder.WriteObject(buffer, encoderState, new ApplicationProperties()));
      }

      [Test]
      public void TestSkipValueWithInvalidList32Type()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List32, false);
      }

      [Test]
      public void TestSkipValueWithInvalidList8Type()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List8, false);
      }

      [Test]
      public void TestSkipValueWithInvalidList0Type()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List0, false);
      }

      [Test]
      public void TestSkipValueWithInvalidList32TypeFromStream()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List32, true);
      }

      [Test]
      public void TestSkipValueWithInvalidList8TypeFromStream()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List8, true);
      }

      [Test]
      public void TestSkipValueWithInvalidList0TypeFromStream()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List0, true);
      }

      private void DoTestSkipValueWithInvalidListType(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)ApplicationProperties.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt((byte)0);  // Size
            buffer.WriteInt((byte)0);  // Count
         }
         else if (listType == EncodingCodes.List8)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)0);  // Size
            buffer.WriteUnsignedByte((byte)0);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List0));
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(stream, streamDecoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestSkipValueWithNullMapEncoding()
      {
         DoTestSkipValueWithNullMapEncoding(false);
      }

      [Test]
      public void TestSkipValueWithNullMapEncodingFromStream()
      {
         DoTestSkipValueWithNullMapEncoding(true);
      }

      private void DoTestSkipValueWithNullMapEncoding(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)ApplicationProperties.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.SkipValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.SkipValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestEncodeDecodeArray()
      {
         DoTestEncodeDecodeArray(false);
      }

      [Test]
      public void TestEncodeDecodeArrayFromStream()
      {
         DoTestEncodeDecodeArray(true);
      }

      private void DoTestEncodeDecodeArray(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         ApplicationProperties[] array = new ApplicationProperties[3];

         IDictionary<string, object> map = new Dictionary<string, object>();
         map.Add("1", true);
         map.Add("2", false);

         array[0] = new ApplicationProperties(new Dictionary<string, object>());
         array[1] = new ApplicationProperties(map);
         array[2] = new ApplicationProperties(map);

         encoder.WriteObject(buffer, encoderState, array);

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
         Assert.AreEqual(typeof(ApplicationProperties), result.GetType().GetElementType());

         ApplicationProperties[] resultArray = (ApplicationProperties[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is ApplicationProperties);
            Assert.AreEqual(array[i].Value, resultArray[i].Value);
         }
      }

      [Test]
      public void TestReadTypeWithNullEncoding()
      {
         TestReadTypeWithNullEncoding(false);
      }

      [Test]
      public void TestReadTypeWithNullEncodingFromStream()
      {
         TestReadTypeWithNullEncoding(true);
      }

      private void TestReadTypeWithNullEncoding(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)ApplicationProperties.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.Throws<DecodeException>(() => streamDecoder.ReadObject(stream, streamDecoderState));
         }
         else
         {
            Assert.Throws<DecodeException>(() => decoder.ReadObject(buffer, decoderState));
         }
      }

      [Test]
      public void TestReadTypeWithOverLargeEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)ApplicationProperties.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));
         buffer.WriteInt(int.MaxValue);  // Size
         buffer.WriteInt(4);  // Count

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("Should not decode type with invalid encoding");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedTypeArray8()
      {
         DoTestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedType(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedTypeArray32()
      {
         DoTestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedType(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedTypeArray8FS()
      {
         DoTestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedType(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedTypeArray32FS()
      {
         DoTestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedType(EncodingCodes.Array32, true);
      }

      private void DoTestDecodeProtectsAgainstArrayOfNullEncodingsForMapBasedType(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8);  // Size
            buffer.WriteInt(int.MaxValue);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte(5);  // Size
            buffer.WriteUnsignedByte(byte.MaxValue);  // Count
         }
         buffer.WriteUnsignedByte(0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Null);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeFailsForArraysOfValuesWhereCountIsGreaterThanRemainingBytesArray8()
      {
         DoTestDecodeFailsForArraysOfValuesWhereCountIsGreaterThanRemainingBytes(EncodingCodes.Array8);
      }

      [Test]
      public void TestDecodeFailsForArraysOfValuesWhereCountIsGreaterThanRemainingBytesArray32()
      {
         DoTestDecodeFailsForArraysOfValuesWhereCountIsGreaterThanRemainingBytes(EncodingCodes.Array32);
      }

      private void DoTestDecodeFailsForArraysOfValuesWhereCountIsGreaterThanRemainingBytes(EncodingCodes arrayType)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8);  // Size
            buffer.WriteInt(int.MaxValue);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte(5);  // Size
            buffer.WriteUnsignedByte(byte.MaxValue);  // Count
         }
         buffer.WriteUnsignedByte(0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
         Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanDataArray8()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanData(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanDataArray32()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanData(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanDataArray8FS()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanData(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanDataArray32FS()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanData(EncodingCodes.Array32, true);
      }

      private void DoTestDecodeFailsIfArrayOfDescribedMapTypeWhereCountIsLargerThanData(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         streamDecoderState.MaxArraySize = 20;

         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(16);         // Size
            buffer.WriteInt(25);         // Count
            buffer.WriteUnsignedByte((byte)0);  // Described Type Indicator
            buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
            buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map32);
            buffer.WriteInt((byte)4);
            buffer.WriteInt((byte)0);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)7);    // Size
            buffer.WriteUnsignedByte((byte)25);   // Count
            buffer.WriteUnsignedByte((byte)0);    // Described Type Indicator
            buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
            buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);
            buffer.WriteUnsignedByte((byte)1);
            buffer.WriteUnsignedByte((byte)0);
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanDataArray8()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanData(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanDataArray32()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanData(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanDataArray8FS()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanData(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanDataArray32FS()
      {
         DoTestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanData(EncodingCodes.Array32, true);
      }

      private void DoTestDecodeFailsIfArrayOfDescribedMapTypeOfNullWhereCountIsLargerThanData(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         streamDecoderState.MaxArraySize = 20;

         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(14);         // Size
            buffer.WriteInt(25);         // Count
            buffer.WriteUnsignedByte((byte)0);  // Described Type Indicator
            buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
            buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)5);    // Size
            buffer.WriteUnsignedByte((byte)25);   // Count
            buffer.WriteUnsignedByte((byte)0);    // Described Type Indicator
            buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
            buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap8()
      {
         DoTestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap8(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap32()
      {
         DoTestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap8(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap8FS()
      {
         DoTestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap8(EncodingCodes.Map8, true);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap32FS()
      {
         DoTestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap8(EncodingCodes.Map32, true);
      }

      private void DoTestDecodeFailsForMapEncodingWhereSizeExceedsAllowedLimitsMap8(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         streamDecoderState.MaxMapSize = 16;

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);

         if (arrayType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map32);
            buffer.WriteInt(18);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);
            buffer.WriteUnsignedByte((byte)18);  // Size
            buffer.WriteUnsignedByte((byte)2);  // Count
         }

         buffer.WriteUnsignedByte((byte)EncodingCodes.Str8);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)65);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
         buffer.WriteUnsignedByte((byte)1);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap8()
      {
         DoTestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap8(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap32()
      {
         DoTestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap8(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap8FS()
      {
         DoTestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap8(EncodingCodes.Map8, true);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap32FS()
      {
         DoTestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap8(EncodingCodes.Map32, true);
      }

      private void DoTestDecodeFailsForMapEncodingWhereCountExceedsAllowedLimitsMap8(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         streamDecoderState.MaxMapSize = 16;

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);

         if (arrayType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map32);
            buffer.WriteInt(9);  // Size
            buffer.WriteInt(18);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);
            buffer.WriteUnsignedByte((byte)6);  // Size
            buffer.WriteUnsignedByte((byte)18);  // Count
         }

         buffer.WriteUnsignedByte((byte)EncodingCodes.Str8);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)65);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
         buffer.WriteUnsignedByte((byte)1);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountIsNotEvenMap8()
      {
         TestDecodeFailsForMapEncodingWhereCountIsNotEven(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountIsNotEvenMap32()
      {
         TestDecodeFailsForMapEncodingWhereCountIsNotEven(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountIsNotEvenMap8FS()
      {
         TestDecodeFailsForMapEncodingWhereCountIsNotEven(EncodingCodes.Map8, true);
      }

      [Test]
      public void TestDecodeFailsForMapEncodingWhereCountIsNotEvenMap32FS()
      {
         TestDecodeFailsForMapEncodingWhereCountIsNotEven(EncodingCodes.Map32, true);
      }

      private void TestDecodeFailsForMapEncodingWhereCountIsNotEven(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);

         if (arrayType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map32);
            buffer.WriteInt(9);  // Size
            buffer.WriteInt(1);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);
            buffer.WriteUnsignedByte((byte)6);  // Size
            buffer.WriteUnsignedByte((byte)1);  // Count
         }

         buffer.WriteUnsignedByte((byte)EncodingCodes.Str8);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)65);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
         buffer.WriteUnsignedByte((byte)1);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeMapTypeMap8()
      {
         DoTestDecodeMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeMapTypeMap32()
      {
         DoTestDecodeMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeMapTypeMap8FS()
      {
         DoTestDecodeMapType(EncodingCodes.Map8, true);
      }

      [Test]
      public void TestDecodeMapTypeMap32FS()
      {
         DoTestDecodeMapType(EncodingCodes.Map32, true);
      }

      private void DoTestDecodeMapType(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);

         if (arrayType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map32);
            buffer.WriteInt(9);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);
            buffer.WriteUnsignedByte((byte)6);  // Size
            buffer.WriteUnsignedByte((byte)2);  // Count
         }

         buffer.WriteUnsignedByte((byte)EncodingCodes.Str8);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)65);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
         buffer.WriteUnsignedByte((byte)1);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.IsNotNull(typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.IsNotNull(typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeNestedTypeMap8()
      {
         DoTestDecodeNestedMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeNestedTypeMap32()
      {
         DoTestDecodeNestedMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeNestedTypeMap8FS()
      {
         DoTestDecodeNestedMapType(EncodingCodes.Map8, true);
      }

      [Test]
      public void TestDecodeNestedTypeMap32FS()
      {
         DoTestDecodeNestedMapType(EncodingCodes.Map32, true);
      }

      private void DoTestDecodeNestedMapType(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);

         if (arrayType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map32);
            buffer.WriteInt(24);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);
            buffer.WriteUnsignedByte((byte)15);  // Size
            buffer.WriteUnsignedByte((byte)2);  // Count
         }

         // Nested Application Properties inside another - Key
         buffer.WriteUnsignedByte((byte)EncodingCodes.Str8);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)65);
         // Value
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)ApplicationProperties.DescriptorCode);

         if (arrayType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map32);
            buffer.WriteInt(9);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Map8);
            buffer.WriteUnsignedByte((byte)6);  // Size
            buffer.WriteUnsignedByte((byte)2);  // Count
         }

         buffer.WriteUnsignedByte((byte)EncodingCodes.Str8);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)65);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
         buffer.WriteUnsignedByte((byte)1);

         decoderState.DepthLimit = 1;
         streamDecoderState.DepthLimit = 1;

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }

         buffer.ReadOffset = 0;

         decoderState.DepthLimit = 2;
         streamDecoderState.DepthLimit = 2;

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.IsNotNull(typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(ApplicationProperties), typeDecoder.DecodesType);
            Assert.IsNotNull(typeDecoder.ReadValue(buffer, decoderState));
         }
      }
   }
}