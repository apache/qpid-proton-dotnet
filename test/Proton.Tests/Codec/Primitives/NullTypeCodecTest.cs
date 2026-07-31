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

using NUnit.Framework;
using System;
using System.IO;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec.Encoders.Primitives;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Codec.Decoders;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class NullTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Null, new NullTypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(void), new NullTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(void), new NullTypeDecoder().DecodesType);
      }

      [Test]
      public void TestWriteOfArrayThrowsException()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(1, 1);

         try
         {
            new NullTypeEncoder().WriteArray(buffer, encoderState, new object[1]);
            Assert.Fail("Null encoder cannot write array types");
         }
         catch (InvalidOperationException) { }
      }

      [Test]
      public void TestWriteRawOfArrayThrowsException()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(1, 1);

         try
         {
            new NullTypeEncoder().WriteRawArray(buffer, encoderState, new object[1]);
            Assert.Fail("Null encoder cannot write array types");
         }
         catch (InvalidOperationException) { }
      }

      [Test]
      public void TestReadNullDoesNotTouchBuffer()
      {
         TestReadNullDoesNotTouchBuffer(false);
      }

      [Test]
      public void TestReadNullDoesNotTouchBufferFS()
      {
         TestReadNullDoesNotTouchBuffer(true);
      }

      private void TestReadNullDoesNotTouchBuffer(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(1, 1);
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadObject(stream, streamDecoderState));
         }
         else
         {
            Assert.IsNull(decoder.ReadObject(buffer, decoderState));
         }
      }

      [Test]
      public void TestSkipNullDoesNotTouchBuffer()
      {
         DoTestSkipNullDoesNotTouchBuffer(false);
      }

      [Test]
      public void TestSkipNullDoesNotTouchStream()
      {
         DoTestSkipNullDoesNotTouchBuffer(true);
      }

      private void DoTestSkipNullDoesNotTouchBuffer(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)EncodingCodes.Null);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(void), typeDecoder.DecodesType);
            long index = buffer.ReadOffset;
            typeDecoder.SkipValue(stream, streamDecoderState);
            Assert.AreEqual(index, buffer.ReadOffset);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(void), typeDecoder.DecodesType);
            long index = buffer.ReadOffset;
            typeDecoder.SkipValue(buffer, decoderState);
            Assert.AreEqual(index, buffer.ReadOffset);
         }
      }

      [Test]
      public void TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray32()
      {
         TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray32FromStream()
      {
         TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray(EncodingCodes.Array32, true);
      }

      [Test]
      public void TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray8()
      {
         TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray8FromStream()
      {
         TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray(EncodingCodes.Array8, true);
      }

      private void TestDefaultsDecodeFailsForAnyNonZeroSizedNullArray(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         if (encodingCode == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(5);  // Size
            buffer.WriteInt(1);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte(2);  // Size
            buffer.WriteUnsignedByte(1);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            Assert.Throws<DecodeException>(() => arrayDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            Assert.Throws<DecodeException>(() => arrayDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeWorksForInConfiguredLimitsNullArray32()
      {
         TestDecodeWorksForInConfiguredLimitsNullArray(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeWorksForInConfiguredLimitsNullArray32FromStream()
      {
         TestDecodeWorksForInConfiguredLimitsNullArray(EncodingCodes.Array32, true);
      }

      [Test]
      public void TestDecodeWorksForInConfiguredLimitsNullArray8()
      {
         TestDecodeWorksForInConfiguredLimitsNullArray(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeWorksForInConfiguredLimitsNullArray8FromStream()
      {
         TestDecodeWorksForInConfiguredLimitsNullArray(EncodingCodes.Array8, true);
      }

      private void TestDecodeWorksForInConfiguredLimitsNullArray(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         decoderState.MaxZeroWidthArrayElements = 20;
         streamDecoderState.MaxZeroWidthArrayElements = 20;

         if (encodingCode == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(5);   // Size
            buffer.WriteInt(10);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte(2);   // Size
            buffer.WriteUnsignedByte(10);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            Assert.DoesNotThrow(() => arrayDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            Assert.DoesNotThrow(() => arrayDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeFailsForToLargeNullArray32()
      {
         TestDecodeFailsForToLargeForConfigurationNullArray(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeFailsForToLargeNullArray32FromStream()
      {
         TestDecodeFailsForToLargeForConfigurationNullArray(EncodingCodes.Array32, true);
      }

      [Test]
      public void TestDecodeFailsForToLargeNullArray8()
      {
         TestDecodeFailsForToLargeForConfigurationNullArray(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsForToLargeNullArray8FromStream()
      {
         TestDecodeFailsForToLargeForConfigurationNullArray(EncodingCodes.Array8, true);
      }

      private void TestDecodeFailsForToLargeForConfigurationNullArray(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         decoderState.MaxZeroWidthArrayElements = 9;
         streamDecoderState.MaxZeroWidthArrayElements = 9;

         if (encodingCode == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(5);  // Size
            buffer.WriteInt(10);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)2);  // Size
            buffer.WriteUnsignedByte((byte)10);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            Assert.Throws<DecodeException>(() => arrayDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            Assert.Throws<DecodeException>(() => arrayDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeForArrayWithCountToLargeFailsArray32()
      {
         DoTestDecodeForArrayWithCountToLargeFails(EncodingCodes.Array32);
      }

      [Test]
      public void TestDecodeForArrayWithCountToLargeFailsArray8()
      {
         DoTestDecodeForArrayWithCountToLargeFails(EncodingCodes.Array8);
      }

      private void DoTestDecodeForArrayWithCountToLargeFails(EncodingCodes encodingCode)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         decoderState.MaxZeroWidthArrayElements = 20;
         streamDecoderState.MaxZeroWidthArrayElements = 20;

         if (encodingCode == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(5);  // Size
            buffer.WriteInt(int.MaxValue);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)2);  // Size
            buffer.WriteUnsignedByte(byte.MaxValue);  // Count
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
         IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
         Assert.Throws<DecodeException>(() => arrayDecoder.ReadValue(buffer, decoderState));
      }
   }
}