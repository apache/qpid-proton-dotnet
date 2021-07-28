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

namespace Apache.Qpid.Proton.Codec.Messaging
{
   [TestFixture]
   public class DataTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Data), new DataTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Data), new DataTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Data.DescriptorCode, new DataTypeDecoder().DescriptorCode);
         Assert.AreEqual(Data.DescriptorCode, new DataTypeEncoder().DescriptorCode);
         Assert.AreEqual(Data.DescriptorSymbol, new DataTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Data.DescriptorSymbol, new DataTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeData()
      {
         DoTestDecodeDataSeries(1, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfDataSection()
      {
         DoTestDecodeDataSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfDataSection()
      {
         DoTestDecodeDataSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeDataFromStream()
      {
         DoTestDecodeDataSeries(1, true);
      }

      [Test]
      public void TestDecodeSmallSeriesOfDataSectionFromStream()
      {
         DoTestDecodeDataSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfDataSectionFromStream()
      {
         DoTestDecodeDataSeries(LargeSize, true);
      }

      private void DoTestDecodeDataSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Data data = new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2, 3 }));

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, data);
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
            Assert.IsTrue(result is Data);

            Data decoded = (Data)result;

            Assert.AreEqual(data.Value, decoded.Value);
         }
      }

      [Test]
      public void TestDecodeDataWithPayloadInUpperBoundsOfSmallBinaryEncoding()
      {
         DoTestDecodeDataWithPayloadInUpperBoundsOfSmallBinaryEncoding(false);
      }

      [Test]
      public void TestDecodeDataWithPayloadInUpperBoundsOfSmallBinaryEncodingFromStream()
      {
         DoTestDecodeDataWithPayloadInUpperBoundsOfSmallBinaryEncoding(true);
      }

      private void DoTestDecodeDataWithPayloadInUpperBoundsOfSmallBinaryEncoding(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int SIZE = 240;

         Data data = new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[SIZE]));

         for (int i = 0; i < SIZE; ++i)
         {
            data.Value[i] = (byte)i;
         }

         encoder.WriteObject(buffer, encoderState, data);

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
         Assert.IsTrue(result is Data);

         Data decoded = (Data)result;

         Assert.AreEqual(data.Value, decoded.Value);
      }

      [Test]
      public void TestDecodeDataWithPayloadInVBin32BinaryEncoding()
      {
         DoTestDecodeDataWithPayloadInVBin32BinaryEncoding(false);
      }

      [Test]
      public void TestDecodeDataWithPayloadInVBin32BinaryEncodingFromStream()
      {
         DoTestDecodeDataWithPayloadInVBin32BinaryEncoding(true);
      }

      private void DoTestDecodeDataWithPayloadInVBin32BinaryEncoding(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int SIZE = 65535;

         Data data = new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[SIZE]));
         for (int i = 0; i < SIZE; ++i)
         {
            data.Value[i] = (byte)i;
         }

         encoder.WriteObject(buffer, encoderState, data);

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
         Assert.IsTrue(result is Data);

         Data decoded = (Data)result;

         Assert.AreEqual(data.Value, decoded.Value);
      }

      [Test]
      public void TestEncodeDecodeArrayOfDataSections()
      {
         DoTestEncodeDecodeArrayOfDataSections(false);
      }

      [Test]
      public void TestEncodeDecodeArrayOfDataSectionsFromStream()
      {
         DoTestEncodeDecodeArrayOfDataSections(true);
      }

      private void DoTestEncodeDecodeArrayOfDataSections(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Data[] dataArray = new Data[3];

         dataArray[0] = new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2, 3 }));
         dataArray[1] = new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 4, 5, 6 }));
         dataArray[2] = new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 7, 8, 9 }));

         encoder.WriteObject(buffer, encoderState, dataArray);

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
         Assert.AreEqual(typeof(Data), result.GetType().GetElementType());

         Data[] resultArray = (Data[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Data);
            Assert.AreEqual(dataArray[i].Value, resultArray[i].Value);
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

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, new Data(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { (byte)i })));
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Data), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Data), typeDecoder.DecodesType);
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
         buffer.WriteUnsignedByte(((byte)Data.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)Data.DescriptorCode));
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
            Assert.AreEqual(typeof(Data), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Data), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestEncodeDecodeArray()
      {
         TestEncodeDecodeArray(false);
      }

      [Test]
      public void TestEncodeDecodeArrayFromStream()
      {
         TestEncodeDecodeArray(true);
      }

      private void TestEncodeDecodeArray(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Data[] array = new Data[3];

         IProtonBuffer bytes1 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });
         IProtonBuffer bytes2 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         IProtonBuffer bytes3 = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2 });

         array[0] = new Data(bytes1);
         array[1] = new Data(bytes2);
         array[2] = new Data(bytes3);

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
         Assert.AreEqual(typeof(Data), result.GetType().GetElementType());

         Data[] resultArray = (Data[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Data);
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
         buffer.WriteUnsignedByte(((byte)Data.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

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
         Assert.IsTrue(result is Data);

         Data decoded = (Data)result;
         Assert.IsNull(decoded.Value);
      }

      [Test]
      public void TestReadTypeWithOverLargeEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Data.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));
         buffer.WriteInt(int.MaxValue); // Not enough bytes in buffer for this
         buffer.WriteUnsignedByte(0);

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("Should not decode type with invalid encoding");
         }
         catch (DecodeException) { }
      }
   }
}