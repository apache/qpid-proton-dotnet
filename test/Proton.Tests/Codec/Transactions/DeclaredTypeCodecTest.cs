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
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Codec.Decoders.Transactions;
using Apache.Qpid.Proton.Codec.Encoders.Transactions;
using System;

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class DeclaredTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Declared), new DeclaredTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Declared), new DeclaredTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         DeclaredTypeDecoder decoder = new DeclaredTypeDecoder();
         DeclaredTypeEncoder encoder = new DeclaredTypeEncoder();

         Assert.AreEqual(Declared.DescriptorCode, decoder.DescriptorCode);
         Assert.AreEqual(Declared.DescriptorCode, encoder.DescriptorCode);
         Assert.AreEqual(Declared.DescriptorSymbol, decoder.DescriptorSymbol);
         Assert.AreEqual(Declared.DescriptorSymbol, encoder.DescriptorSymbol);
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

         Declared input = new Declared();
         input.TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2, 4, 6, 8 });

         encoder.WriteObject(buffer, encoderState, input);

         Declared result;
         if (fromStream)
         {
            result = (Declared)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Declared)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result.TxnId);
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2, 4, 6, 8 }), result.TxnId);
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

         byte[] txnId = new byte[512];

         Random rand = new Random(Environment.TickCount);
         rand.NextBytes(txnId);

         Declared input = new Declared();

         input.TxnId = ProtonByteBufferAllocator.Instance.Wrap(txnId);

         encoder.WriteObject(buffer, encoderState, input);

         Declared result;
         if (fromStream)
         {
            result = (Declared)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Declared)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(txnId), result.TxnId);
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

         Declared declared = new Declared();

         declared.TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, declared);
         }

         declared.TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 });

         encoder.WriteObject(buffer, encoderState, declared);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Declared), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Declared), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
            }
         }

         Declared result;
         if (fromStream)
         {
            result = (Declared)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Declared)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.IsTrue(result is Declared);

         Declared value = result;
         Assert.AreEqual(ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1, 2 }), value.TxnId);
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
         buffer.WriteUnsignedByte(((byte)Declared.DescriptorCode));
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
            Assert.AreEqual(typeof(Declared), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Declared), typeDecoder.DecodesType);

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
         buffer.WriteUnsignedByte(((byte)Declared.DescriptorCode));
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

         Declared[] array = new Declared[3];

         array[0] = new Declared();
         array[1] = new Declared();
         array[2] = new Declared();

         array[0].TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0 });
         array[1].TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 1 });
         array[2].TxnId = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 2 });

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
         Assert.AreEqual(typeof(Declared), result.GetType().GetElementType());

         Declared[] resultArray = (Declared[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Declared);
            Assert.AreEqual(array[i].TxnId, resultArray[i].TxnId);
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
         buffer.WriteUnsignedByte(((byte)Declared.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)Declared.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt(128);  // Size
            buffer.WriteInt(127);  // Count
         }
         else if (listType == EncodingCodes.List8)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)128);  // Size
            buffer.WriteUnsignedByte((byte)127);  // Count
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