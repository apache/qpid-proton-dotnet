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
   public class DeleteOnCloseTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(DeleteOnClose), new DeleteOnCloseTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(DeleteOnClose), new DeleteOnCloseTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(DeleteOnClose.DescriptorCode, new DeleteOnCloseTypeDecoder().DescriptorCode);
         Assert.AreEqual(DeleteOnClose.DescriptorCode, new DeleteOnCloseTypeEncoder().DescriptorCode);
         Assert.AreEqual(DeleteOnClose.DescriptorSymbol, new DeleteOnCloseTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(DeleteOnClose.DescriptorSymbol, new DeleteOnCloseTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeDeleteOnClose()
      {
         DoTestDecodeDeleteOnClose(false);
      }

      [Test]
      public void TestDecodeDeleteOnCloseFromStream()
      {
         DoTestDecodeDeleteOnClose(true);
      }

      private void DoTestDecodeDeleteOnClose(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         DeleteOnClose value = DeleteOnClose.Instance;

         encoder.WriteObject(buffer, encoderState, value);

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
         Assert.IsTrue(result is DeleteOnClose);
      }

      [Test]
      public void TestDecodeDeleteOnCloseWithList8()
      {
         DoTestDecodeDeleteOnCloseWithList8(false);
      }

      [Test]
      public void TestDecodeDeleteOnCloseWithList8FromStream()
      {
         DoTestDecodeDeleteOnCloseWithList8(true);
      }

      private void DoTestDecodeDeleteOnCloseWithList8(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)DeleteOnClose.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
         buffer.WriteUnsignedByte((byte)0);  // Size
         buffer.WriteUnsignedByte((byte)0);  // Count

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
         Assert.IsTrue(result is DeleteOnClose);
      }

      [Test]
      public void TestDecodeDeleteOnCloseWithList32()
      {
         DoTestDecodeDeleteOnCloseWithList32(false);
      }

      [Test]
      public void TestDecodeDeleteOnCloseWithList32FromStream()
      {
         DoTestDecodeDeleteOnCloseWithList32(true);
      }

      private void DoTestDecodeDeleteOnCloseWithList32(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)DeleteOnClose.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
         buffer.WriteInt((byte)0);  // Size
         buffer.WriteInt((byte)0);  // Count

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
         Assert.IsTrue(result is DeleteOnClose);
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
            encoder.WriteObject(buffer, encoderState, DeleteOnClose.Instance);
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(DeleteOnClose), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(DeleteOnClose), typeDecoder.DecodesType);
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
         buffer.WriteUnsignedByte(((byte)DeleteOnClose.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)DeleteOnClose.DescriptorCode));
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
            Assert.AreEqual(typeof(DeleteOnClose), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(DeleteOnClose), typeDecoder.DecodesType);

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

         DeleteOnClose[] array = new DeleteOnClose[3];

         array[0] = DeleteOnClose.Instance;
         array[1] = DeleteOnClose.Instance;
         array[2] = DeleteOnClose.Instance;

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
         Assert.AreEqual(typeof(DeleteOnClose), result.GetType().GetElementType());

         DeleteOnClose[] resultArray = (DeleteOnClose[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is DeleteOnClose);
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
         buffer.WriteUnsignedByte((byte)DeleteOnClose.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List8);
         buffer.WriteUnsignedByte((byte)1);  // Size
         buffer.WriteUnsignedByte((byte)0);  // Count
         buffer.WriteUnsignedByte((byte)1);  // Size
         buffer.WriteUnsignedByte((byte)0);  // Count

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is DeleteOnClose[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is DeleteOnClose[]);
         }

         // Now check that if we set the array size to big it will not decode
         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Allocate();

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
         buffer2.WriteUnsignedByte((byte)DeleteOnNoLinks.DescriptorCode);
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
         buffer.WriteUnsignedByte((byte)DeleteOnClose.DescriptorCode);
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
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is DeleteOnClose[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is DeleteOnClose[]);
         }
      }
   }
}
