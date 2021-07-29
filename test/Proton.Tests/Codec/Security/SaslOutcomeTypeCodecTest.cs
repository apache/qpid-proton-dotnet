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

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class SaslOutcomeTypeCodecTest : CodecTestSupport
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
         Assert.AreEqual(typeof(SaslOutcome), new SaslOutcomeTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(SaslOutcome), new SaslOutcomeTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         SaslOutcomeTypeDecoder decoder = new SaslOutcomeTypeDecoder();
         SaslOutcomeTypeEncoder encoder = new SaslOutcomeTypeEncoder();

         Assert.AreEqual(SaslOutcome.DescriptorCode, decoder.DescriptorCode);
         Assert.AreEqual(SaslOutcome.DescriptorCode, encoder.DescriptorCode);
         Assert.AreEqual(SaslOutcome.DescriptorSymbol, decoder.DescriptorSymbol);
         Assert.AreEqual(SaslOutcome.DescriptorSymbol, encoder.DescriptorSymbol);
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

         byte[] data = new byte[] { 1, 2, 3, 4 };
         SaslCode code = SaslCode.Auth;

         SaslOutcome input = new SaslOutcome();
         input.AdditionalData = ProtonByteBufferAllocator.Instance.Wrap(data);
         input.Code = code;

         encoder.WriteObject(buffer, encoderState, input);

         SaslOutcome result;
         if (fromStream)
         {
            result = (SaslOutcome)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslOutcome)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(code, result.Code);
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(data), result.AdditionalData);
      }

      [Test]
      public void TestAdditionalDataHandlesNullBinaryWithoutNPEAndUpdates()
      {
         DoTestAdditionalDataHandlesNullBinaryWithoutNPEAndUpdates(false);
      }

      [Test]
      public void TestAdditionalDataHandlesNullBinaryWithoutNPEAndUpdatesFromStream()
      {
         DoTestAdditionalDataHandlesNullBinaryWithoutNPEAndUpdates(true);
      }

      private void DoTestAdditionalDataHandlesNullBinaryWithoutNPEAndUpdates(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         byte[] data = new byte[] { 1, 2, 3, 4 };
         SaslCode code = SaslCode.Auth;

         SaslOutcome input = new SaslOutcome();
         input.AdditionalData = ProtonByteBufferAllocator.Instance.Wrap(data);
         input.AdditionalData = null;
         input.Code = code;

         encoder.WriteObject(buffer, encoderState, input);

         SaslOutcome result;
         if (fromStream)
         {
            result = (SaslOutcome)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslOutcome)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(code, result.Code);
         Assert.IsNull(result.AdditionalData);
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

         SaslOutcome outcome = new SaslOutcome();

         outcome.AdditionalData = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });
         outcome.Code = SaslCode.Auth;

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, outcome);
         }

         outcome.AdditionalData = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 });
         outcome.Code = SaslCode.SysTemp;

         encoder.WriteObject(buffer, encoderState, outcome);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(SaslOutcome), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(SaslOutcome), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is SaslOutcome);

         SaslOutcome value = (SaslOutcome)result;
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 }), value.AdditionalData);
         Assert.AreEqual(SaslCode.SysTemp, value.Code);
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
         buffer.WriteUnsignedByte(((byte)SaslOutcome.DescriptorCode));
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
            Assert.AreEqual(typeof(SaslOutcome), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(SaslOutcome), typeDecoder.DecodesType);

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
         buffer.WriteUnsignedByte(((byte)SaslOutcome.DescriptorCode));
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

         SaslOutcome[] array = new SaslOutcome[3];

         array[0] = new SaslOutcome();
         array[1] = new SaslOutcome();
         array[2] = new SaslOutcome();

         array[0].Code = SaslCode.Ok;
         array[0].AdditionalData = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });
         array[1].Code = SaslCode.SysTemp;
         array[1].AdditionalData = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         array[2].Code = SaslCode.Auth;
         array[2].AdditionalData = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2 });

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
         Assert.AreEqual(typeof(SaslOutcome), result.GetType().GetElementType());

         SaslOutcome[] resultArray = (SaslOutcome[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is SaslOutcome);
            Assert.AreEqual(array[i].Code, resultArray[i].Code);
            Assert.AreEqual(array[i].AdditionalData, resultArray[i].AdditionalData);
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
         buffer.WriteUnsignedByte(((byte)SaslOutcome.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)SaslOutcome.DescriptorCode));
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
   }
}