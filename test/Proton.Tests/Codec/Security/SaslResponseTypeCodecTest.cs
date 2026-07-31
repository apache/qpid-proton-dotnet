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
using Apache.Qpid.Proton.Codec.Decoders;
using Apache.Qpid.Proton.Codec.Encoders;
using Apache.Qpid.Proton.Types.Security;
using Apache.Qpid.Proton.Codec.Decoders.Security;
using Apache.Qpid.Proton.Codec.Encoders.Security;
using System;

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class SaslResponseTypeCodecTest : CodecTestSupport
   {
      public override void SetUp()
      {
         decoder = ProtonDecoderFactory.CreateSasl();
         decoderState = decoder.NewDecoderState();

         encoder = ProtonEncoderFactory.CreateSasl();
         encoderState = encoder.NewEncoderState();

         streamDecoder = ProtonStreamDecoderFactory.CreateSasl();
         streamDecoderState = streamDecoder.NewDecoderState();
      }

      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(SaslResponse), new SaslResponseTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(SaslResponse), new SaslResponseTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         SaslResponseTypeDecoder decoder = new SaslResponseTypeDecoder();
         SaslResponseTypeEncoder encoder = new SaslResponseTypeEncoder();

         Assert.AreEqual(SaslResponse.DescriptorCode, decoder.DescriptorCode);
         Assert.AreEqual(SaslResponse.DescriptorCode, encoder.DescriptorCode);
         Assert.AreEqual(SaslResponse.DescriptorSymbol, decoder.DescriptorSymbol);
         Assert.AreEqual(SaslResponse.DescriptorSymbol, encoder.DescriptorSymbol);
      }

      [Test]
      public void TestEncodeDecodeType()
      {
         DoTestEncodeDecodeType(false);
      }

      [Test]
      public void TestEncodeDecodeTypeFromStream()
      {
         DoTestEncodeDecodeType(true);
      }

      private void DoTestEncodeDecodeType(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         byte[] response = new byte[] { 1, 2, 3, 4 };

         SaslResponse input = new SaslResponse();
         input.Response = ProtonByteBufferAllocator.Instance.Wrap(response);

         encoder.WriteObject(buffer, encoderState, input);

         SaslResponse result;
         if (fromStream)
         {
            result = (SaslResponse)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslResponse)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(response), result.Response);
      }

      [Test]
      public void TestEncodeDecodeTypeWithLargeResponseBlob()
      {
         DoTestEncodeDecodeTypeWithLargeResponseBlob(false);
      }

      [Test]
      public void TestEncodeDecodeTypeWithLargeResponseBlobFromStream()
      {
         DoTestEncodeDecodeTypeWithLargeResponseBlob(true);
      }

      private void DoTestEncodeDecodeTypeWithLargeResponseBlob(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         byte[] response = new byte[512];

         Random rand = new Random(Environment.TickCount);
         rand.NextBytes(response);

         SaslResponse input = new SaslResponse();
         input.Response = ProtonByteBufferAllocator.Instance.Wrap(response);

         encoder.WriteObject(buffer, encoderState, input);

         SaslResponse result;
         if (fromStream)
         {
            result = (SaslResponse)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslResponse)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(response), result.Response);
      }

      [Test]
      public void TestSkipValue()
      {
         DoTestSkipValueFromStream(false);
      }

      [Test]
      public void TestSkipValueFromStream()
      {
         DoTestSkipValueFromStream(true);
      }

      private void DoTestSkipValueFromStream(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         SaslResponse response = new SaslResponse();

         response.Response = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, response);
         }

         response.Response = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 });

         encoder.WriteObject(buffer, encoderState, response);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(SaslResponse), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(SaslResponse), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is SaslResponse);

         SaslResponse value = (SaslResponse)result;
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 }), value.Response);
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
         buffer.WriteUnsignedByte(((byte)SaslResponse.DescriptorCode));
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
            Assert.AreEqual(typeof(SaslResponse), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(SaslResponse), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodedWithInvalidMap32Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap8Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodedWithInvalidMap32TypeFromStream()
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
         buffer.WriteUnsignedByte(((byte)SaslResponse.DescriptorCode));
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

         SaslResponse[] array = new SaslResponse[3];

         array[0] = new SaslResponse();
         array[1] = new SaslResponse();
         array[2] = new SaslResponse();

         array[0].Response = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });
         array[1].Response = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         array[2].Response = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2 });

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
         Assert.AreEqual(typeof(SaslResponse), result.GetType().GetElementType());

         SaslResponse[] resultArray = (SaslResponse[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is SaslResponse);
            Assert.AreEqual(array[i].Response, resultArray[i].Response);
         }
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList0()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List0, false);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList8()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List8, false);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList32()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List32, false);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList0FromStream()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List0, true);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList8FromStream()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List8, true);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList32FromStream()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List32, true);
      }

      private void DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)SaslResponse.DescriptorCode));
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
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList8()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List8, false);
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList32()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List32, false);
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList8FromStream()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List8, true);
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList32FromStream()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List32, true);
      }

      private void DoTestDecodeWithToManyListEntriesList32(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)SaslResponse.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt((byte)64);  // Size
            buffer.WriteInt((byte)8);  // Count
         }
         else if (listType == EncodingCodes.List8)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)64);  // Size
            buffer.WriteUnsignedByte((byte)8);  // Count
         }

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
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
         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer2);

         if (arrayType == EncodingCodes.Array32)
         {
            buffer2.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer2.WriteInt(13);  // Size
            buffer2.WriteInt(2);   // Count
         }
         else
         {
            buffer2.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer2.WriteUnsignedByte(10);  // Size
            buffer2.WriteUnsignedByte(2);  // Count
         }
         buffer2.WriteUnsignedByte(0); // Described Type Indicator
         buffer2.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer2.WriteUnsignedByte((byte)SaslResponse.DescriptorCode);
         buffer2.WriteUnsignedByte((byte)EncodingCodes.List8);
         buffer2.WriteUnsignedByte(1);  // Size
         buffer2.WriteUnsignedByte(0);  // Count
         buffer2.WriteUnsignedByte(1);  // Size
         buffer2.WriteUnsignedByte(0);  // Count

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.IsNotNull(typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer2, decoderState);
            Assert.IsNotNull(typeDecoder.DecodesType);
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

         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte(5);  // Size
            buffer.WriteUnsignedByte(2);  // Count
         }
         buffer.WriteUnsignedByte(0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)SaslResponse.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List0);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.IsNotNull(typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.IsNotNull(typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }
   }
}