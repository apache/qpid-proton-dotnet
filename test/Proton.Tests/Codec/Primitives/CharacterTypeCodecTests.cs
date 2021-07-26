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
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using Apache.Qpid.Proton.Codec.Encoders.Primitives;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class CharacterTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType()
      {
         TestDecoderThrowsWhenAskedToReadWrongTypeAsThisType(false);
      }

      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsThisTypeFS()
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
               streamDecoder.ReadCharacter(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadCharacter(stream, streamDecoderState, (char)0);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadCharacter(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadCharacter(buffer, decoderState, (char)0);
               Assert.Fail("Should not allow read of integer type as byte");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestTypeFromEncodingCode()
      {
         TestTypeFromEncodingCode(false);
      }

      [Test]
      public void TestTypeFromEncodingCodeFS()
      {
         TestTypeFromEncodingCode(true);
      }

      public void TestTypeFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Char));
         buffer.WriteInt(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Char));
         buffer.WriteInt(43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadCharacter(stream, streamDecoderState));
            Assert.AreEqual(43, streamDecoder.ReadCharacter(stream, streamDecoderState, (char)42));
            Assert.IsNull(streamDecoder.ReadCharacter(stream, streamDecoderState));
            Assert.AreEqual(42, streamDecoder.ReadCharacter(stream, streamDecoderState, (char)42));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadCharacter(buffer, decoderState));
            Assert.AreEqual(43, decoder.ReadCharacter(buffer, decoderState, (char)42));
            Assert.IsNull(decoder.ReadCharacter(buffer, decoderState));
            Assert.AreEqual(42, decoder.ReadCharacter(buffer, decoderState, (char)42));
         }
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Char, new CharacterTypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(char), new CharacterTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(char), new CharacterTypeDecoder().DecodesType);
      }

      [Test]
      public void TestReadCharFromEncodingCode()
      {
         TestReadCharFromEncodingCode(false);
      }

      [Test]
      public void TestReadCharFromEncodingCodeFS()
      {
         TestReadCharFromEncodingCode(true);
      }

      private void TestReadCharFromEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Char));
         buffer.WriteInt(42);

         if (fromStream)
         {
            Assert.AreEqual(42, streamDecoder.ReadCharacter(stream, streamDecoderState));
         }
         else
         {
            Assert.AreEqual(42, decoder.ReadCharacter(buffer, decoderState));
         }
      }

      [Test]
      public void TestSkipValue()
      {
         TestSkipValue(false);
      }

      [Test]
      public void TestSkipValueFS()
      {
         TestSkipValue(true);
      }

      private void TestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteCharacter(buffer, encoderState, char.MaxValue);
            encoder.WriteCharacter(buffer, encoderState, (char)16);
         }

         char expected = (char)42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(char), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(char), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(char), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(char), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is char);

         char value = (char)result;
         Assert.AreEqual(expected, value);
      }

      [Test]
      public void TestArrayOfCharacterObjects()
      {
         TestArrayOfCharacterObjects(false);
      }

      [Test]
      public void TestArrayOfCharacterObjectsFS()
      {
         TestArrayOfCharacterObjects(true);
      }

      private void TestArrayOfCharacterObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         char[] source = new char[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = (char)i;
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

         char[] array = (char[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      [Test]
      public void TestZeroSizedArrayOfCharacterObjects()
      {
         TestZeroSizedArrayOfCharacterObjects(false);
      }

      [Test]
      public void TestZeroSizedArrayOfCharacterObjectsFS()
      {
         TestZeroSizedArrayOfCharacterObjects(true);
      }

      private void TestZeroSizedArrayOfCharacterObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         char[] source = new char[0];

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

         char[] array = (char[])result;
         Assert.AreEqual(source.Length, array.Length);
      }

      [Test]
      public void TestDecodeSmallCharArray()
      {
         DoTestDecodeCharArrayType(SmallArraySize, false);
      }

      [Test]
      public void TestDecodeLargeCharArray()
      {
         DoTestDecodeCharArrayType(LargeArraySize, false);
      }

      [Test]
      public void TestDecodeSmallCharArrayFS()
      {
         DoTestDecodeCharArrayType(SmallArraySize, true);
      }

      [Test]
      public void TestDecodeLargeCharArrayFS()
      {
         DoTestDecodeCharArrayType(LargeArraySize, true);
      }

      private void DoTestDecodeCharArrayType(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         char[] source = new char[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = (char)i;
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
         Assert.IsTrue(result.GetType().GetElementType() == typeof(char));

         char[] array = (char[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }
   }
}