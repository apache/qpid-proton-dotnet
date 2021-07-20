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

using System.Text;
using NUnit.Framework;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Encoders
{
   [TestFixture]
   public class ProtonEncoderTest : CodecTestSupport
   {
      [Test]
      public void TestCachedEncoderStateIsCached()
      {
         IEncoderState state1 = encoder.CachedEncoderState;
         IEncoderState state2 = encoder.CachedEncoderState;

         Assert.IsTrue(state1 is ProtonEncoderState);
         Assert.IsTrue(state1 is ProtonEncoderState);

         Assert.AreSame(state1, state2);
      }

      [Test]
      public void TestProtonEncoderStateHasNoStringEncoderByDefault()
      {
         ProtonEncoderState state = (ProtonEncoderState)encoder.CachedEncoderState;

         Assert.IsNull(state.Utf8Encoder);
      }

      [Test]
      public void TestUseCustomUTF8EncoderInEncoderState()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         string expected = "test-encoding-string";

         ((ProtonEncoderState)encoderState).Utf8Encoder = new TestUtf8CustomEncoder();

         encoder.WriteString(buffer, encoderState, expected);

         string result = decoder.ReadString(buffer, decoderState);

         Assert.AreEqual(expected, result);
      }

      [Test]
      public void TestWriteBooleanPrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteBoolean(buffer, encoderState, true);
         encoder.WriteBoolean(buffer, encoderState, false);

         Assert.AreEqual(2, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.BooleanTrue));
         Assert.AreEqual(buffer.GetUnsignedByte(1), ((byte)EncodingCodes.BooleanFalse));
      }

      [Test]
      public void TestWriteUnsignedBytePrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedByte(buffer, encoderState, (byte)0);
         encoder.WriteUnsignedByte(buffer, encoderState, (byte)255);

         Assert.AreEqual(4, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.UByte));
         Assert.AreEqual(buffer.GetUnsignedByte(1), 0);
         Assert.AreEqual(buffer.GetUnsignedByte(2), ((byte)EncodingCodes.UByte));
         Assert.AreEqual(buffer.GetUnsignedByte(3), (byte)255);
      }

      [Test]
      public void TestWriteUnsignedShortPrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedShort(buffer, encoderState, (ushort)0);
         encoder.WriteUnsignedShort(buffer, encoderState, (ushort)65535);

         Assert.AreEqual(6, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.UShort));
         Assert.AreEqual(buffer.GetUnsignedByte(1), 0);
         Assert.AreEqual(buffer.GetUnsignedByte(2), 0);
         Assert.AreEqual(buffer.GetUnsignedByte(3), ((byte)EncodingCodes.UShort));
         Assert.AreEqual(buffer.GetUnsignedByte(4), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(5), (byte)255);
      }

      [Test]
      public void TestWriteUnsignedIntegerPrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedInteger(buffer, encoderState, 0);
         encoder.WriteUnsignedInteger(buffer, encoderState, 255);
         encoder.WriteUnsignedInteger(buffer, encoderState, uint.MaxValue);

         Assert.AreEqual(8, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.UInt0));
         Assert.AreEqual(buffer.GetUnsignedByte(1), ((byte)EncodingCodes.SmallUInt));
         Assert.AreEqual(buffer.GetUnsignedByte(2), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(3), ((byte)EncodingCodes.UInt));
         Assert.AreEqual(buffer.GetUnsignedByte(4), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(5), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(6), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(7), (byte)255);
      }

      [Test]
      public void TestWriteUnsignedLongPrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteUnsignedLong(buffer, encoderState, 0ul);
         encoder.WriteUnsignedLong(buffer, encoderState, 255ul);
         encoder.WriteUnsignedLong(buffer, encoderState, ulong.MaxValue);

         Assert.AreEqual(12, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.ULong0));
         Assert.AreEqual(buffer.GetUnsignedByte(1), ((byte)EncodingCodes.SmallULong));
         Assert.AreEqual(buffer.GetUnsignedByte(2), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(3), ((byte)EncodingCodes.ULong));
         Assert.AreEqual(buffer.GetUnsignedByte(4), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(5), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(6), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(7), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(8), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(9), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(10), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(11), (byte)255);
      }

      [Test]
      public void TestWriteBytePrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteByte(buffer, encoderState, 0);
         encoder.WriteByte(buffer, encoderState, -1);

         Assert.AreEqual(4, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.Byte));
         Assert.AreEqual(buffer.GetUnsignedByte(1), 0);
         Assert.AreEqual(buffer.GetUnsignedByte(2), ((byte)EncodingCodes.Byte));
         Assert.AreEqual(buffer.GetUnsignedByte(3), (byte)255);
      }

      [Test]
      public void TestWriteShortPrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteShort(buffer, encoderState, (short)0);
         encoder.WriteShort(buffer, encoderState, (short)-1);

         Assert.AreEqual(6, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.Short));
         Assert.AreEqual(buffer.GetUnsignedByte(1), 0);
         Assert.AreEqual(buffer.GetUnsignedByte(2), 0);
         Assert.AreEqual(buffer.GetUnsignedByte(3), ((byte)EncodingCodes.Short));
         Assert.AreEqual(buffer.GetUnsignedByte(4), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(5), (byte)255);
      }

      [Test]
      public void TestWriteIntegerPrimitive()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteInteger(buffer, encoderState, 0);
         encoder.WriteInteger(buffer, encoderState, int.MaxValue);

         Assert.AreEqual(7, buffer.ReadableBytes);
         Assert.AreEqual(buffer.GetUnsignedByte(0), ((byte)EncodingCodes.SmallInt));
         Assert.AreEqual(buffer.GetUnsignedByte(1), 0);
         Assert.AreEqual(buffer.GetUnsignedByte(2), ((byte)EncodingCodes.Int));
         Assert.AreEqual(buffer.GetUnsignedByte(3), (byte)127);
         Assert.AreEqual(buffer.GetUnsignedByte(4), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(5), (byte)255);
         Assert.AreEqual(buffer.GetUnsignedByte(6), (byte)255);
      }
   }

   internal class TestUtf8CustomEncoder : IUtf8Encoder
   {
      public IProtonBuffer EncodeUTF8(IProtonBuffer buffer, string sequence)
      {
         return buffer.WriteBytes(new UTF8Encoding().GetBytes(sequence));
      }
   }
}