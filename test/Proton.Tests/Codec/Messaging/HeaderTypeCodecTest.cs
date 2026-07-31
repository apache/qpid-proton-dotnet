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
using System;

namespace Apache.Qpid.Proton.Codec.Messaging
{
   [TestFixture]
   public class HeaderTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Header), new HeaderTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Header), new HeaderTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Header.DescriptorCode, new HeaderTypeDecoder().DescriptorCode);
         Assert.AreEqual(Header.DescriptorCode, new HeaderTypeEncoder().DescriptorCode);
         Assert.AreEqual(Header.DescriptorSymbol, new HeaderTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Header.DescriptorSymbol, new HeaderTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeHeader()
      {
         DoTestDecodeHeaderSeries(1, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfHeaders()
      {
         DoTestDecodeHeaderSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfHeaders()
      {
         DoTestDecodeHeaderSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeHeaderFromStream()
      {
         DoTestDecodeHeaderSeries(1, true);
      }

      [Test]
      public void TestDecodeSmallSeriesOfHeadersFromStream()
      {
         DoTestDecodeHeaderSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfHeadersFromStream()
      {
         DoTestDecodeHeaderSeries(LargeSize, true);
      }

      private void DoTestDecodeHeaderSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Header header = new Header();

         Random random = new Random(Environment.TickCount);

         uint randomDeliveryCount = (uint)random.Next();
         uint randomTimeToLive = (uint)random.Next();

         header.Durable = true;
         header.Priority = (byte)3;
         header.DeliveryCount = randomDeliveryCount;
         header.FirstAcquirer = true;
         header.TimeToLive = randomTimeToLive;

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, header);
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
            Assert.IsTrue(result is Header);

            Header decoded = (Header)result;

            Assert.AreEqual(3, decoded.Priority);
            Assert.AreEqual(randomTimeToLive, decoded.TimeToLive);
            Assert.AreEqual(randomDeliveryCount, decoded.DeliveryCount);
            Assert.IsTrue(decoded.Durable);
            Assert.IsTrue(decoded.FirstAcquirer);
         }
      }

      [Test]
      public void TestEncodeAndDecodeWithMaxUnsignedValuesFromLongs()
      {
         DoTestEncodeAndDecodeWithMaxUnsignedValuesFromLongs(false);
      }

      [Test]
      public void TestEncodeAndDecodeWithMaxUnsignedValuesFromLongsFromStream()
      {
         DoTestEncodeAndDecodeWithMaxUnsignedValuesFromLongs(true);
      }

      private void DoTestEncodeAndDecodeWithMaxUnsignedValuesFromLongs(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         Header header = new Header();

         header.DeliveryCount = uint.MaxValue;
         header.TimeToLive = uint.MaxValue;

         encoder.WriteObject(buffer, encoderState, header);

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
         Assert.IsTrue(result is Header);

         Header decoded = (Header)result;

         Assert.AreEqual(uint.MaxValue, decoded.DeliveryCount);
         Assert.AreEqual(uint.MaxValue, decoded.TimeToLive);
      }

      [Test]
      public void TestEncodeDecodeZeroSizedArrayOfHeaders()
      {
         dotestEncodeDecodeZeroSizedArrayOfHeaders(false);
      }

      [Test]
      public void TestEncodeDecodeZeroSizedArrayOfHeadersFromStream()
      {
         dotestEncodeDecodeZeroSizedArrayOfHeaders(true);
      }

      private void dotestEncodeDecodeZeroSizedArrayOfHeaders(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Header[] headerArray = new Header[0];

         encoder.WriteObject(buffer, encoderState, headerArray);

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
         Assert.AreEqual(typeof(Header), result.GetType().GetElementType());

         Header[] resultArray = (Header[])result;
         Assert.AreEqual(0, resultArray.Length);
      }

      [Test]
      public void TestEncodeDecodeArrayOfHeaders()
      {
         DoTestEncodeDecodeArrayOfHeaders(false);
      }

      [Test]
      public void TestEncodeDecodeArrayOfHeadersFromStream()
      {
         DoTestEncodeDecodeArrayOfHeaders(true);
      }

      private void DoTestEncodeDecodeArrayOfHeaders(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Header[] headerArray = new Header[3];

         headerArray[0] = new Header();
         headerArray[1] = new Header();
         headerArray[2] = new Header();

         headerArray[0].Durable = (true);
         headerArray[1].Durable = (true);
         headerArray[2].Durable = (true);

         encoder.WriteObject(buffer, encoderState, headerArray);

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
         Assert.AreEqual(typeof(Header), result.GetType().GetElementType());

         Header[] resultArray = (Header[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Header);
            Assert.AreEqual(headerArray[i].Durable, resultArray[i].Durable);
         }
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

      private void DoTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Header header = new Header();

         header.Durable = true;
         header.Priority = 3;
         header.DeliveryCount = 10;
         header.FirstAcquirer = false;
         header.TimeToLive = 500;

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, new Header());
         }

         encoder.WriteObject(buffer, encoderState, header);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Header);

         Header value = (Header)result;
         Assert.AreEqual(3, value.Priority);
         Assert.IsTrue(value.Durable);
         Assert.IsFalse(value.FirstAcquirer);
         Assert.AreEqual(500, header.TimeToLive);
         Assert.AreEqual(10, header.DeliveryCount);
      }

      [Test]
      public void TestDecodeWithInvalidMap32Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap8Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap32TypeFromStream()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestDecodeWithInvalidMap8TypeFromStream()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map8, true);
      }

      private void DoTestDecodeWithInvalidMapType(EncodingCodes mapType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Header.DescriptorCode));
         if (mapType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));
            buffer.WriteInt((byte)0);  // Size
            buffer.WriteInt((byte)0);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map8));
            buffer.WriteUnsignedByte((byte)0);  // Size
            buffer.WriteUnsignedByte((byte)0);  // Count
         }

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("Should not decode type with invalid encoding");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("Should not decode type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestSkipValueWithInvalidMap32Type()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestSkipValueWithInvalidMap8Type()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestSkipValueWithInvalidMap32TypeFromStream()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestSkipValueWithInvalidMap8TypeFromStream()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map8, true);
      }

      private void DoTestSkipValueWithInvalidMapType(EncodingCodes mapType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Header.DescriptorCode));
         if (mapType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));
            buffer.WriteInt((byte)0);  // Size
            buffer.WriteInt((byte)0);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map8));
            buffer.WriteUnsignedByte((byte)0);  // Size
            buffer.WriteUnsignedByte((byte)0);  // Count
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeFailsWhenList8IndicateToManyEntries()
      {
         DoTestDecodeFailsWhenListIndicateToManyEntries(EncodingCodes.List8, true);
      }

      [Test]
      public void TestDecodeFailsWhenList32IndicateToManyEntries()
      {
         DoTestDecodeFailsWhenListIndicateToManyEntries(EncodingCodes.List32, true);
      }

      [Test]
      public void TestDecodeFailsWhenList8IndicateToManyEntriesFromStream()
      {
         DoTestDecodeFailsWhenListIndicateToManyEntries(EncodingCodes.List8, true);
      }

      [Test]
      public void TestDecodeFailsWhenList32IndicateToManyEntriesFromStream()
      {
         DoTestDecodeFailsWhenListIndicateToManyEntries(EncodingCodes.List32, true);
      }

      private void DoTestDecodeFailsWhenListIndicateToManyEntries(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Header.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt((byte)20);  // Size
            buffer.WriteInt((byte)10);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)20);  // Size
            buffer.WriteUnsignedByte((byte)10);  // Count
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);

            try
            {
               typeDecoder.ReadValue(stream, streamDecoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);

            try
            {
               typeDecoder.ReadValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeFailsWhenList8IndicateOverflowedEntries()
      {
         DoTestDecodeFailsWhenListIndicateOverflowedEntries(EncodingCodes.List8, false);
      }

      [Test]
      public void TestDecodeFailsWhenList32IndicateOverflowedEntries()
      {
         DoTestDecodeFailsWhenListIndicateOverflowedEntries(EncodingCodes.List32, false);
      }

      [Test]
      public void TestDecodeFailsWhenList8IndicateOverflowedEntriesFromStream()
      {
         DoTestDecodeFailsWhenListIndicateOverflowedEntries(EncodingCodes.List8, true);
      }

      [Test]
      public void TestDecodeFailsWhenList32IndicateOverflowedEntriesFromStream()
      {
         DoTestDecodeFailsWhenListIndicateOverflowedEntries(EncodingCodes.List32, true);
      }

      private void DoTestDecodeFailsWhenListIndicateOverflowedEntries(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Header.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt(20);  // Size
            buffer.WriteInt(-1);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)20);  // Size
            buffer.WriteUnsignedByte((byte)255);  // Count
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);

            try
            {
               typeDecoder.ReadValue(stream, streamDecoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Header), typeDecoder.DecodesType);

            try
            {
               typeDecoder.ReadValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestEncodeDecodeArray()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         Header[] array = new Header[3];

         array[0] = new Header();
         array[1] = new Header();
         array[2] = new Header();

         array[0].Priority = 1;
         array[0].DeliveryCount = 1;
         array[1].Priority = 2;
         array[1].DeliveryCount = 2;
         array[2].Priority = 3;
         array[2].DeliveryCount = 3;

         encoder.WriteObject(buffer, encoderState, array);

         object result = decoder.ReadObject(buffer, decoderState);

         Assert.IsTrue(result.GetType().IsArray);
         Assert.AreEqual(typeof(Header), result.GetType().GetElementType());

         Header[] resultArray = (Header[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Header);
            Assert.AreEqual(array[i].Priority, resultArray[i].Priority);
            Assert.AreEqual(array[i].DeliveryCount, resultArray[i].DeliveryCount);
         }
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfValuesSizeIsToLargeArray8()
      {
         DoTestDecodeFailsWhenArrayOfValuesSizeIsToLarge(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfValuesSizeIsToLargeArray32()
      {
         DoTestDecodeFailsWhenArrayOfValuesSizeIsToLarge(EncodingCodes.Array32, false);
      }

      private void DoTestDecodeFailsWhenArrayOfValuesSizeIsToLarge(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         // First show that we can do this if the data is correct
         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(12);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)9);  // Size
            buffer.WriteUnsignedByte((byte)2);  // Count
         }
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)Header.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List8);
         buffer.WriteUnsignedByte((byte)1);  // Size
         buffer.WriteUnsignedByte((byte)0);  // Count
         buffer.WriteUnsignedByte((byte)1);  // Size
         buffer.WriteUnsignedByte((byte)0);  // Count

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is Header[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is Header[]);
         }

         // Now check that if we set the array size to big it will not decode
         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Allocate();
         stream = new ProtonBufferInputStream(buffer2);

         if (arrayType == EncodingCodes.Array32)
         {
            buffer2.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer2.WriteInt(13);  // Size
            buffer2.WriteInt(2);   // Count
         }
         else
         {
            buffer2.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer2.WriteUnsignedByte((byte)10);  // Size
            buffer2.WriteUnsignedByte((byte)2);  // Count
         }
         buffer2.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer2.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer2.WriteUnsignedByte((byte)Header.DescriptorCode);
         buffer2.WriteUnsignedByte((byte)EncodingCodes.List8);
         buffer2.WriteUnsignedByte((byte)1);  // Size
         buffer2.WriteUnsignedByte((byte)0);  // Count
         buffer2.WriteUnsignedByte((byte)1);  // Size
         buffer2.WriteUnsignedByte((byte)0);  // Count

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer2, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer2, decoderState));
         }
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array32, true);
      }

      private void DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         // First show that we can do this if the data is correct
         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)5);  // Size
            buffer.WriteUnsignedByte((byte)2);  // Count
         }
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)Header.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List0);

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

         buffer.ReadOffset = 0;  // Reset and try with limits lifted

         decoderState.MaxZeroWidthArrayElements = 2;
         streamDecoderState.MaxZeroWidthArrayElements = 2;

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is Header[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is Header[]);
         }
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLargeArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLarge(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLargeArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLarge(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLargeArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLarge(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLargeArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLarge(EncodingCodes.Array32, true);
      }

      private void DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsCountToLarge(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         streamDecoderState.MaxArraySize = 16;

         // First show that we can do this if the data is correct
         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8);  // Size
            buffer.WriteInt(int.MaxValue);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)5);  // Size
            buffer.WriteUnsignedByte(byte.MaxValue);  // Count
         }
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)Header.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List32);

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
   }
}