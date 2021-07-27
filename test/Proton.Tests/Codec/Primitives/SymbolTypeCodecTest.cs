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
using Apache.Qpid.Proton.Types;
using System;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class SymbolTypeCodecTest : CodecTestSupport
   {
      private static readonly String SmallSymbolValue = "Small String";
      private static readonly String LargeSymbolValue = "Large String: " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog. " +
          "The quick brown fox jumps over the lazy dog.";

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

         if (fromStream)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));

            try
            {
               streamDecoder.ReadSymbol(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadSymbolAsString(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));

            try
            {
               decoder.ReadSymbol(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadSymbolAsString(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadFromNullEncodingCode()
      {
         TestReadFromNullEncodingCode(false);
      }

      [Test]
      public void TestReadFromNullEncodingCodeFS()
      {
         TestReadFromNullEncodingCode(true);
      }

      private void TestReadFromNullEncodingCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadSymbol(stream, streamDecoderState));
            Assert.IsNull(streamDecoder.ReadSymbolAsString(stream, streamDecoderState));
         }
         else
         {
            Assert.IsNull(decoder.ReadSymbol(buffer, decoderState));
            Assert.IsNull(decoder.ReadSymbolAsString(buffer, decoderState));
         }
      }

      [Test]
      public void TestEncodeSmallSymbol()
      {
         DoTestEncodeDecode(Symbol.Lookup(SmallSymbolValue), false);
      }

      [Test]
      public void TestEncodeLargeSymbol()
      {
         DoTestEncodeDecode(Symbol.Lookup(LargeSymbolValue), false);
      }

      [Test]
      public void TestEncodeEmptySymbol()
      {
         DoTestEncodeDecode(Symbol.Lookup(""), false);
      }

      [Test]
      public void TestEncodeNullSymbol()
      {
         DoTestEncodeDecode(null, false);
      }

      [Test]
      public void TestEncodeSmallSymbolFS()
      {
         DoTestEncodeDecode(Symbol.Lookup(SmallSymbolValue), true);
      }

      [Test]
      public void TestEncodeLargeSymbolFS()
      {
         DoTestEncodeDecode(Symbol.Lookup(LargeSymbolValue), true);
      }

      [Test]
      public void TestEncodeEmptySymbolFS()
      {
         DoTestEncodeDecode(Symbol.Lookup(""), true);
      }

      [Test]
      public void TestEncodeNullSymbolFS()
      {
         DoTestEncodeDecode(null, true);
      }

      private void DoTestEncodeDecode(Symbol value, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteSymbol(buffer, encoderState, value);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadSymbol(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadSymbol(buffer, decoderState);
         }

         if (value != null)
         {
            Assert.IsNotNull(result);
            Assert.IsTrue(result is Symbol);
         }
         else
         {
            Assert.IsNull(result);
         }

         Assert.AreEqual(value, result);
      }

      [Test]
      public void TestDecodeSmallSeriesOfSymbols()
      {
         DoTestDecodeSymbolSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfSymbols()
      {
         DoTestDecodeSymbolSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfSymbolsFS()
      {
         DoTestDecodeSymbolSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfSymbolsFS()
      {
         DoTestDecodeSymbolSeries(LargeSize, true);
      }

      private void DoTestDecodeSymbolSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteSymbol(buffer, encoderState, Symbol.Lookup(LargeSymbolValue));
         }

         for (int i = 0; i < size; ++i)
         {
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
            Assert.IsTrue(result is Symbol);
            Assert.AreEqual(LargeSymbolValue, result.ToString());
         }
      }

      [Test]
      public void TestDecodeSmallSymbolArray()
      {
         doTestDecodeSymbolArrayType(SmallArraySize, false);
      }

      [Test]
      public void TestDecodeLargeSymbolArray()
      {
         doTestDecodeSymbolArrayType(LargeArraySize, false);
      }

      [Test]
      public void TestDecodeSmallSymbolArrayFS()
      {
         doTestDecodeSymbolArrayType(SmallArraySize, true);
      }

      [Test]
      public void TestDecodeLargeSymbolArrayFS()
      {
         doTestDecodeSymbolArrayType(LargeArraySize, true);
      }

      private void doTestDecodeSymbolArrayType(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Symbol[] source = new Symbol[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = Symbol.Lookup("test->" + i);
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

         Symbol[] array = (Symbol[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      [Test]
      public void TestEmptyShortSymbolEncode()
      {
         doTestEmptySymbolEncodeAsGivenType(EncodingCodes.Sym8, false);
      }

      [Test]
      public void TestEmptyLargeSymbolEncode()
      {
         doTestEmptySymbolEncodeAsGivenType(EncodingCodes.Sym32, false);
      }

      [Test]
      public void TestEmptyShortSymbolEncodeFS()
      {
         doTestEmptySymbolEncodeAsGivenType(EncodingCodes.Sym8, true);
      }

      [Test]
      public void TestEmptyLargeSymbolEncodeFS()
      {
         doTestEmptySymbolEncodeAsGivenType(EncodingCodes.Sym32, true);
      }

      public void doTestEmptySymbolEncodeAsGivenType(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)encodingCode));
         buffer.WriteInt(0);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Symbol), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, encodingCode);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.PeekNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Symbol), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, encodingCode);
         }

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadSymbol(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadSymbol(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.AreEqual("", result.ToString());
      }

      [Test]
      public void TestEmptyShortSymbolEncodeAsString()
      {
         doTestEmptySymbolEncodeAsGivenTypeReadAsString(EncodingCodes.Sym8, false);
      }

      [Test]
      public void TestEmptyLargeSymbolEncodeAsString()
      {
         doTestEmptySymbolEncodeAsGivenTypeReadAsString(EncodingCodes.Sym32, false);
      }

      [Test]
      public void TestEmptyShortSymbolEncodeAsStringFS()
      {
         doTestEmptySymbolEncodeAsGivenTypeReadAsString(EncodingCodes.Sym8, true);
      }

      [Test]
      public void TestEmptyLargeSymbolEncodeAsStringFS()
      {
         doTestEmptySymbolEncodeAsGivenTypeReadAsString(EncodingCodes.Sym32, true);
      }

      public void doTestEmptySymbolEncodeAsGivenTypeReadAsString(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)encodingCode));
         buffer.WriteInt(0);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadSymbolAsString(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadSymbolAsString(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.AreEqual("", result);
      }

      [Test]
      public void TestEncodedSizeExceedsRemainingDetectedSym32()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Sym32));
         buffer.WriteInt(Int32.MaxValue);

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("should throw an DecodeException");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestEncodedSizeExceedsRemainingDetectedSym8()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Sym8));
         buffer.WriteUnsignedByte(byte.MaxValue);

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("should throw an DecodeException");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestEncodeDecodeSmallSymbolArray50()
      {
         DoEncodeDecodeSmallSymbolArrayTestImpl(50, false);
      }

      [Test]
      public void TestEncodeDecodeSmallSymbolArray100()
      {
         DoEncodeDecodeSmallSymbolArrayTestImpl(100, false);
      }

      [Test]
      public void TestEncodeDecodeSmallSymbolArray384()
      {
         DoEncodeDecodeSmallSymbolArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeSmallSymbolArray50FS()
      {
         DoEncodeDecodeSmallSymbolArrayTestImpl(50, true);
      }

      [Test]
      public void TestEncodeDecodeSmallSymbolArray100FS()
      {
         DoEncodeDecodeSmallSymbolArrayTestImpl(100, true);
      }

      [Test]
      public void TestEncodeDecodeSmallSymbolArray384FS()
      {
         DoEncodeDecodeSmallSymbolArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeSmallSymbolArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         Symbol[] source = createPayloadArraySmallSymbols(count);

         Assert.AreEqual(count, source.Length, "Unexpected source array length");

         int encodingWidth = 4;
         int arrayPayloadSize = encodingWidth + 1 + (count * 5); // variable width for element count + byte type descriptor + (number of elements * size[=length+content-char])
         int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code + variable width for array size + other encoded payload
         byte[] expectedEncoding = new byte[expectedEncodedArraySize];
         IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
         expectedEncodingWrapper.WriteOffset = 0;

         // Write the array encoding code, array size, and element count
         expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
         expectedEncodingWrapper.WriteInt(arrayPayloadSize);
         expectedEncodingWrapper.WriteInt(count);

         // Write the type descriptor
         expectedEncodingWrapper.WriteUnsignedByte((byte)0xb3); // 'sym32' type descriptor code

         // Write the elements
         for (int i = 0; i < count; i++)
         {
            Symbol symbol = source[i];
            Assert.AreEqual(1, symbol.Length, "Unexpected length");

            expectedEncodingWrapper.WriteInt(1); // Length
            expectedEncodingWrapper.WriteUnsignedByte(((byte)symbol.ToString()[0])); // Content
         }

         Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

         // Now verify against the actual encoding of the array
         Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
         encoder.WriteArray(buffer, encoderState, source);
         Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

         byte[] actualEncoding = new byte[expectedEncodedArraySize];
         buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
         Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

         object decoded;
         if (fromStream)
         {
            decoded = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            decoded = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded.GetType().IsArray);

         Assert.AreEqual(source, (Symbol[])decoded, "Unexpected decoding");
      }

      // Creates 1 char Symbols with chars of 0-9, for encoding as sym8
      private static Symbol[] createPayloadArraySmallSymbols(int length)
      {
         Random rand = new Random(Environment.TickCount);

         Symbol[] payload = new Symbol[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = Symbol.Lookup(rand.Next(9).ToString());
         }

         return payload;
      }

      [Test]
      public void TestSkipValue()
      {
         doTestSkipValue(false);
      }

      [Test]
      public void TestSkipValueFromStream()
      {
         doTestSkipValue(true);
      }

      public void doTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteSymbol(buffer, encoderState, Symbol.Lookup("skipMe"));
         }

         Symbol expected = Symbol.Lookup("expected-symbol-value");

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Symbol), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Symbol), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Symbol);

         Symbol value = (Symbol)result;
         Assert.AreEqual(expected, value);
      }
   }
}