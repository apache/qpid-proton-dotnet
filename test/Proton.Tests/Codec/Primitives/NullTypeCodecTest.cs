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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

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
   }
}