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
using Apache.Qpid.Proton.Codec.Decoders.Primitives;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class BinaryTypeCodecTest : CodecTestSupport
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
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadBinary(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadBinary(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadFromNullEncodingCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         Assert.IsNull(decoder.ReadBinary(buffer, decoderState));
      }

      [Test]
      public void TestReadFromNullEncodingCodeFromStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         Assert.IsNull(streamDecoder.ReadBinary(stream, streamDecoderState));
      }

      [Test]
      public void TestEncodeDecodeEmptyArrayBinary()
      {
         TestEncodeDecodeEmptyArrayBinary(false);
      }

      [Test]
      public void TestEncodeDecodeEmptyArrayBinaryFromStream()
      {
         TestEncodeDecodeEmptyArrayBinary(true);
      }

      private void TestEncodeDecodeEmptyArrayBinary(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         IProtonBuffer input = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteBinary(buffer, encoderState, input);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is IProtonBuffer);
         IProtonBuffer output = (IProtonBuffer)result;

         Assert.AreEqual(0, output.ReadableBytes);
         Assert.AreEqual(0, output.Capacity);
      }

      [Test]
      public void TestEncodeDecodeBinary()
      {
         TestEncodeDecodeBinary(false);
      }

      [Test]
      public void TestEncodeDecodeBinaryFromStream()
      {
         TestEncodeDecodeBinary(true);
      }

      private void TestEncodeDecodeBinary(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         IProtonBuffer input = ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0, 1, 2, 3, 4 });

         encoder.WriteBinary(buffer, encoderState, input);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is IProtonBuffer);
         IProtonBuffer output = (IProtonBuffer)result;

         input.ReadOffset = 0;
         Assert.AreEqual(5, input.ReadableBytes);
         Assert.AreEqual(5, output.ReadableBytes);
         Assert.AreEqual(input, output);
      }

      [Test]
      public void TestEncodeDecodeBinaryUsingRawBytesWithSmallArray()
      {
         TestEncodeDecodeBinaryUsingRawBytesWithSmallArray(false);
      }

      [Test]
      public void TestEncodeDecodeBinaryUsingRawBytesWithSmallArrayFromStream()
      {
         TestEncodeDecodeBinaryUsingRawBytesWithSmallArray(true);
      }

      private void TestEncodeDecodeBinaryUsingRawBytesWithSmallArray(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         Random filler = new Random(Environment.TickCount);

         byte[] input = new byte[16];
         filler.NextBytes(input);

         encoder.WriteBinary(buffer, encoderState, input);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is IProtonBuffer);
         IProtonBuffer output = (IProtonBuffer)result;

         Assert.AreEqual(input.Length, output.ReadableBytes);
      }

      [Test]
      public void TestEncodeDecodeBinaryUsingRawBytesWithLargeArray()
      {
         TestEncodeDecodeBinaryUsingRawBytesWithLargeArray(false);
      }

      [Test]
      public void TestEncodeDecodeBinaryUsingRawBytesWithLargeArrayFromStream()
      {
         TestEncodeDecodeBinaryUsingRawBytesWithLargeArray(true);
      }

      private void TestEncodeDecodeBinaryUsingRawBytesWithLargeArray(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         Random filler = new Random(Environment.TickCount);

         byte[] input = new byte[512];
         filler.NextBytes(input);

         encoder.WriteBinary(buffer, encoderState, input);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is IProtonBuffer);
         IProtonBuffer output = (IProtonBuffer)result;

         Assert.AreEqual(input.Length, output.ReadableBytes);
      }

      [Test]
      public void TestDecodeFailsEarlyOnInvliadBinaryLengthVBin8()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin8));
         buffer.WriteUnsignedByte(255);

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("Should not be able to read binary with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(2, buffer.ReadOffset);
      }

      [Test]
      public void TestDecodeFailsEarlyOnInvliadBinaryLengthVBin32()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));
         buffer.WriteInt(int.MaxValue);

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("Should not be able to read binary with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(5, buffer.ReadOffset);
      }

      [Test]
      public void TestDecodeOfBinaryTagFailsEarlyOnInvliadBinaryLengthVBin32()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));
         buffer.WriteInt(int.MaxValue);
         buffer.WriteInt(int.MaxValue);

         try
         {
            decoder.ReadDeliveryTag(buffer, decoderState);
            Assert.Fail("Should not be able to read binary with length greater than readable bytes");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestSkipFailsEarlyOnInvliadBinaryLengthVBin8()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin8));
         buffer.WriteUnsignedByte(255);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
         Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.VBin8);
         Assert.AreEqual(typeof(IProtonBuffer), typeDecoder.DecodesType);

         try
         {
            typeDecoder.SkipValue(buffer, decoderState);
            Assert.Fail("Should not be able to skip binary with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(2, buffer.ReadOffset);
      }

      [Test]
      public void TestSkipFailsEarlyOnInvliadBinaryLengthVBin8FromStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin8));
         buffer.WriteUnsignedByte(255);

         IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
         Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
         Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.VBin8);
         Assert.AreEqual(typeof(IProtonBuffer), typeDecoder.DecodesType);

         try
         {
            typeDecoder.SkipValue(stream, streamDecoderState);
            Assert.Fail("Should not be able to skip binary with length greater than readable bytes");
         }
         catch (DecodeException)
         {
         }

         Assert.AreEqual(2, buffer.ReadOffset);
      }

      [Test]
      public void TestSkipFailsEarlyOnInvliadBinaryLengthVBin32()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));
         buffer.WriteInt(int.MaxValue);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
         Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.VBin32);
         Assert.AreEqual(typeof(IProtonBuffer), typeDecoder.DecodesType);

         try
         {
            typeDecoder.SkipValue(buffer, decoderState);
            Assert.Fail("Should not be able to skip binary with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(5, buffer.ReadOffset);
      }

      [Test]
      public void TestSkipFailsEarlyOnInvliadBinaryLengthVBin32FromStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));
         buffer.WriteInt(int.MaxValue);

         IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
         Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
         Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.VBin32);
         Assert.AreEqual(typeof(IProtonBuffer), typeDecoder.DecodesType);

         try
         {
            typeDecoder.SkipValue(stream, streamDecoderState);
            Assert.Fail("Should not be able to skip binary with length greater than readable bytes");
         }
         catch (DecodeException)
         {
         }

         Assert.AreEqual(5, buffer.ReadOffset);
      }

      [Test]
      public void TestReadEncodedSizeFromVBin8Encoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin8));
         buffer.WriteUnsignedByte(255);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.AreEqual(typeof(IProtonBuffer), typeDecoder.DecodesType);
         IBinaryTypeDecoder binaryDecoder = (IBinaryTypeDecoder)typeDecoder;
         Assert.AreEqual(255, binaryDecoder.ReadSize(buffer, decoderState));

         Assert.AreEqual(2, buffer.ReadOffset);
      }

      [Test]
      public void TestReadEncodedSizeFromVBin8EncodingUsingStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin8));
         buffer.WriteUnsignedByte(255);

         IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
         Assert.AreEqual(typeof(IProtonBuffer), typeDecoder.DecodesType);
         IBinaryTypeDecoder binaryDecoder = (IBinaryTypeDecoder)typeDecoder;
         Assert.AreEqual(255, binaryDecoder.ReadSize(stream, streamDecoderState));

         Assert.AreEqual(2, buffer.ReadOffset);
      }

      [Test]
      public void TestZeroSizedArrayOfBinaryObjects()
      {
         TestZeroSizedArrayOfBinaryObjects(false);
      }

      [Test]
      public void TestZeroSizedArrayOfBinaryObjectsFromStream()
      {
         TestZeroSizedArrayOfBinaryObjects(true);
      }

      private void TestZeroSizedArrayOfBinaryObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IProtonBuffer[] source = new IProtonBuffer[0];

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

         IProtonBuffer[] array = (IProtonBuffer[])result;
         Assert.AreEqual(source.Length, array.Length);
      }

      [Test]
      public void TestArrayOfBinaryObjects()
      {
         TestArrayOfBinaryObjects(false);
      }

      [Test]
      public void TestArrayOfBinaryObjectsFromStream()
      {
         TestArrayOfBinaryObjects(true);
      }

      private void TestArrayOfBinaryObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         Random filler = new Random(Environment.TickCount);

         IProtonBuffer[] source = new IProtonBuffer[5];
         for (int i = 0; i < source.Length; ++i)
         {
            byte[] data = new byte[16 * i];
            filler.NextBytes(data);

            source[i] = ProtonByteBufferAllocator.Instance.Wrap(data);
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

         IProtonBuffer[] array = (IProtonBuffer[])result;
         Assert.AreEqual(source.LongLength, array.Length);

         for (int i = 0; i < source.Length; ++i)
         {
            IProtonBuffer decoded = ((IProtonBuffer[])result)[i];
            Assert.AreEqual(source[i], decoded);
         }
      }
   }
}