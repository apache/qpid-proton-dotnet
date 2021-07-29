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
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class SaslInitTypeCodecTest : CodecTestSupport
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
         Assert.AreEqual(typeof(SaslInit), new SaslInitTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(SaslInit), new SaslInitTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         SaslInitTypeDecoder decoder = new SaslInitTypeDecoder();
         SaslInitTypeEncoder encoder = new SaslInitTypeEncoder();

         Assert.AreEqual(SaslInit.DescriptorCode, decoder.DescriptorCode);
         Assert.AreEqual(SaslInit.DescriptorCode, encoder.DescriptorCode);
         Assert.AreEqual(SaslInit.DescriptorSymbol, decoder.DescriptorSymbol);
         Assert.AreEqual(SaslInit.DescriptorSymbol, encoder.DescriptorSymbol);
      }

      [Test]
      public void TestEncodeDecodeTypeMechanismOnly()
      {
         DoTestEncodeDecodeTypeMechanismOnly(false);
      }

      [Test]
      public void TestEncodeDecodeTypeMechanismOnlyFromStream()
      {
         DoTestEncodeDecodeTypeMechanismOnly(true);
      }

      private void DoTestEncodeDecodeTypeMechanismOnly(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         SaslInit input = new SaslInit();
         input.Mechanism = Symbol.Lookup("ANONYMOUS");

         encoder.WriteObject(buffer, encoderState, input);

         SaslInit result;
         if (fromStream)
         {
            result = (SaslInit)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslInit)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), result.Mechanism);
         Assert.IsNull(result.Hostname);
         Assert.IsNull(result.InitialResponse);
      }

      [Test]
      public void TestEncodeDecodeTypeWithoutHostname()
      {
         DoTestEncodeDecodeTypeWithoutHostname(false);
      }

      [Test]
      public void TestEncodeDecodeTypeWithoutHostnameFromStream()
      {
         DoTestEncodeDecodeTypeWithoutHostname(true);
      }

      private void DoTestEncodeDecodeTypeWithoutHostname(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         byte[] initialResponse = new byte[] { 1, 2, 3, 4 };

         SaslInit input = new SaslInit();
         input.Mechanism = Symbol.Lookup("ANONYMOUS");
         input.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(initialResponse);

         encoder.WriteObject(buffer, encoderState, input);

         SaslInit result;
         if (fromStream)
         {
            result = (SaslInit)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslInit)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), result.Mechanism);
         Assert.IsNull(result.Hostname);
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(initialResponse), result.InitialResponse);
      }

      [Test]
      public void TestInitialResponseHandlesNullBinarySet()
      {
         DoTestInitialResponseHandlesNullBinarySet(false);
      }

      [Test]
      public void TestInitialResponseHandlesNullBinarySetFromStream()
      {
         DoTestInitialResponseHandlesNullBinarySet(true);
      }

      private void DoTestInitialResponseHandlesNullBinarySet(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         byte[] initialResponse = new byte[] { 1, 2, 3, 4 };

         SaslInit input = new SaslInit();
         input.Mechanism = Symbol.Lookup("ANONYMOUS");

         // Ensure that a null is handled without NPE and that it does indeed clear old value.
         input.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(initialResponse);
         input.InitialResponse = null;

         encoder.WriteObject(buffer, encoderState, input);

         SaslInit result;
         if (fromStream)
         {
            result = (SaslInit)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslInit)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), result.Mechanism);
         Assert.IsNull(result.Hostname);
         Assert.IsNull(result.InitialResponse);
      }

      [Test]
      public void TestEncodeDecodeTypeMechanismAndHostname()
      {
         DoTestEncodeDecodeTypeMechanismAndHostname(false);
      }

      [Test]
      public void TestEncodeDecodeTypeMechanismAndHostnameFromStream()
      {
         DoTestEncodeDecodeTypeMechanismAndHostname(true);
      }

      private void DoTestEncodeDecodeTypeMechanismAndHostname(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         SaslInit input = new SaslInit();
         input.Mechanism = Symbol.Lookup("ANONYMOUS");
         input.Hostname = "test";

         encoder.WriteObject(buffer, encoderState, input);

         SaslInit result;
         if (fromStream)
         {
            result = (SaslInit)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslInit)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), result.Mechanism);
         Assert.AreEqual("test", result.Hostname);
         Assert.IsNull(result.InitialResponse);
      }

      [Test]
      public void TestEncodeDecodeTypeAllFieldsSet()
      {
         DoTestEncodeDecodeTypeAllFieldsSet(false);
      }

      [Test]
      public void TestEncodeDecodeTypeAllFieldsSetFromStream()
      {
         DoTestEncodeDecodeTypeAllFieldsSet(true);
      }

      private void DoTestEncodeDecodeTypeAllFieldsSet(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         byte[] initialResponse = new byte[] { 1, 2, 3, 4 };

         SaslInit input = new SaslInit();
         input.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(initialResponse);
         input.Hostname = "test";
         input.Mechanism = Symbol.Lookup("ANONYMOUS");

         encoder.WriteObject(buffer, encoderState, input);

         SaslInit result;
         if (fromStream)
         {
            result = (SaslInit)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslInit)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual("test", result.Hostname);
         Assert.AreEqual(Symbol.Lookup("ANONYMOUS"), result.Mechanism);
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(initialResponse), result.InitialResponse);
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

         SaslInit init = new SaslInit();

         init.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });
         init.Hostname = "skip";

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, init);
         }

         init.InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 });
         init.Hostname = "localhost";
         init.Mechanism = Symbol.Lookup("PLAIN");

         encoder.WriteObject(buffer, encoderState, init);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(SaslInit), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(SaslInit), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
            }
         }

         SaslInit result;
         if (fromStream)
         {
            result = (SaslInit)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (SaslInit)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.IsTrue(result is SaslInit);

         SaslInit value = result;
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 }), value.InitialResponse);
         Assert.AreEqual("localhost", value.Hostname);
         Assert.AreEqual(Symbol.Lookup("PLAIN"), value.Mechanism);
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
         buffer.WriteUnsignedByte(((byte)SaslInit.DescriptorCode));
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
            Assert.AreEqual(typeof(SaslInit), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(SaslInit), typeDecoder.DecodesType);

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
         buffer.WriteUnsignedByte(((byte)SaslInit.DescriptorCode));
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

         SaslInit[] array = new SaslInit[3];

         array[0] = new SaslInit();
         array[1] = new SaslInit();
         array[2] = new SaslInit();

         array[0].InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });
         array[0].Hostname = "test-1";
         array[0].Mechanism = Symbol.Lookup("ANONYMOUS");
         array[1].InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         array[1].Hostname = "test-2";
         array[1].Mechanism = Symbol.Lookup("PLAIN");
         array[2].InitialResponse = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2 });
         array[2].Hostname = "test-2";
         array[2].Mechanism = Symbol.Lookup("EXTERNAL");

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
         Assert.AreEqual(typeof(SaslInit), result.GetType().GetElementType());

         SaslInit[] resultArray = (SaslInit[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is SaslInit);
            Assert.AreEqual(array[i].Mechanism, resultArray[i].Mechanism);
            Assert.AreEqual(array[i].Hostname, resultArray[i].Hostname);
            Assert.AreEqual(array[i].InitialResponse, resultArray[i].InitialResponse);
         }
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
         buffer.WriteUnsignedByte(((byte)SaslInit.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt(64);  // Size
            buffer.WriteInt(-1);  // Count
         }
         else if (listType == EncodingCodes.List8)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)64);  // Size
            buffer.WriteUnsignedByte((byte)0xFF);  // Count
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
         buffer.WriteUnsignedByte(((byte)SaslInit.DescriptorCode));
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