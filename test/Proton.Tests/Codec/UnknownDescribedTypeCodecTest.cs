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

using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec.Utilities;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec
{
   [TestFixture]
   public class UnknownDescribedTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestDecodeUnknownDescribedType()
      {
         DoTestDecodeUnknownDescribedType(false);
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeFromStream()
      {
         DoTestDecodeUnknownDescribedType(true);
      }

      private void DoTestDecodeUnknownDescribedType(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteObject(buffer, encoderState, NoLocalType.Instance);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is UnknownDescribedType);
         UnknownDescribedType resultTye = (UnknownDescribedType)result;
         Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
      }

      [Test]
      public void TestUnknownDescribedTypeInList()
      {
         DoTestUnknownDescribedTypeInList(false);
      }

      [Test]
      public void TestUnknownDescribedTypeInListFromStream()
      {
         DoTestUnknownDescribedTypeInList(true);
      }

      private void DoTestUnknownDescribedTypeInList(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList<object> listOfUnknowns = new List<object>();

         listOfUnknowns.Add(NoLocalType.Instance);

         encoder.WriteList(buffer, encoderState, listOfUnknowns);

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
         Assert.IsTrue(result is IList<object>);

         IList<object> decodedList = (List<object>)result;
         Assert.AreEqual(1, decodedList.Count);

         object listEntry = decodedList[0];
         Assert.IsTrue(listEntry is UnknownDescribedType);

         UnknownDescribedType resultTye = (UnknownDescribedType)listEntry;
         Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
      }

      [Test]
      public void TestUnknownDescribedTypeInMap()
      {
         DoTestUnknownDescribedTypeInMap(false);
      }

      [Test]
      public void TestUnknownDescribedTypeInMapFromStream()
      {
         DoTestUnknownDescribedTypeInMap(true);
      }

      private void DoTestUnknownDescribedTypeInMap(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<object, object> mapOfUnknowns = new Dictionary<object, object>();

         mapOfUnknowns.Add(NoLocalType.Instance.Descriptor, NoLocalType.Instance);

         encoder.WriteMap(buffer, encoderState, mapOfUnknowns);

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
         Assert.IsTrue(result is IDictionary<object, object>);

         IDictionary<object, object> decodedMap = (IDictionary<object, object>)result;
         Assert.AreEqual(1, decodedMap.Count);

         object mapEntry = decodedMap[NoLocalType.Instance.Descriptor];
         Assert.IsTrue(mapEntry is UnknownDescribedType);

         UnknownDescribedType resultTye = (UnknownDescribedType)mapEntry;
         Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
      }

      [Test]
      public void testUnknownDescribedTypeInArray()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         NoLocalType[] arrayOfUnknown = new NoLocalType[1];

         arrayOfUnknown[0] = NoLocalType.Instance;

         try
         {
            encoder.WriteArray(buffer, encoderState, arrayOfUnknown);
            Assert.Fail("Should not be able to write an array of unregistered described type");
         }
         catch (ArgumentException) { }

         try
         {
            encoder.WriteObject(buffer, encoderState, arrayOfUnknown);
            Assert.Fail("Should not be able to write an array of unregistered described type");
         }
         catch (ArgumentException) { }
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnknownDescribedTypes()
      {
         DoTestDecodeUnknownDescribedTypeSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnknownDescribedTypes()
      {
         DoTestDecodeUnknownDescribedTypeSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfUnknownDescribedTypesFromStream()
      {
         DoTestDecodeUnknownDescribedTypeSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfUnknownDescribedTypesFromStream()
      {
         DoTestDecodeUnknownDescribedTypeSeries(LargeSize, true);
      }

      private void DoTestDecodeUnknownDescribedTypeSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, NoLocalType.Instance);
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
            Assert.IsTrue(result is UnknownDescribedType);

            UnknownDescribedType resultTye = (UnknownDescribedType)result;
            Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
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

      [Test]
      public void TestDecodeFailsWhenArrayOfValuesSizeIsToLargeArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfValuesSizeIsToLarge(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfValuesSizeIsToLargeArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfValuesSizeIsToLarge(EncodingCodes.Array32, true);
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
         buffer.WriteUnsignedByte((byte)NoLocalType.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List8);
         buffer.WriteUnsignedByte((byte)1);  // Size
         buffer.WriteUnsignedByte((byte)0);  // Count
         buffer.WriteUnsignedByte((byte)1);  // Size
         buffer.WriteUnsignedByte((byte)0);  // Count

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is UnknownDescribedType[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is UnknownDescribedType[]);
         }

         // Now check that if we set the array size to big it will not decode
         IProtonBuffer buffer2 = ProtonByteBufferAllocator.Instance.Allocate();
         stream = new ProtonBufferInputStream(buffer2);

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
         buffer2.WriteUnsignedByte((byte)NoLocalType.DescriptorCode);
         buffer2.WriteUnsignedByte((byte)EncodingCodes.List8);
         buffer2.WriteUnsignedByte((byte)1);  // Size
         buffer2.WriteUnsignedByte((byte)0);  // Count
         buffer2.WriteUnsignedByte((byte)1);  // Size
         buffer2.WriteUnsignedByte((byte)0);  // Count

         if (fromStream)
         {
            streamDecoderState.MaxArraySize = 1;
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
         buffer.WriteUnsignedByte((byte)NoLocalType.DescriptorCode);
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
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is UnknownDescribedType[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is UnknownDescribedType[]);
         }
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCountArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCount(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCountArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCount(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCountArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCount(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCountArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCount(EncodingCodes.Array32, true);
      }

      private void DoTestDecodeFailsWhenArrayOfTypeWithListEncodingsHasOverLargeCount(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         streamDecoderState.MaxArraySize = 16;

         // First show that we can do this if the data is correct
         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(10);  // Size
            buffer.WriteInt(17);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)7);  // Size
            buffer.WriteUnsignedByte((byte)17);  // Count
         }
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)NoLocalType.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List8);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)0);

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
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithULong0EncodingsArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, false, EncodingCodes.ULong0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithULong0EncodingsArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, false, EncodingCodes.ULong0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithULong0EncodingsArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, true, EncodingCodes.ULong0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithULong0EncodingsArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, true, EncodingCodes.ULong0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithUInt0EncodingsArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, false, EncodingCodes.UInt0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithUInt0EncodingsArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, false, EncodingCodes.UInt0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithUInt0EncodingsArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, true, EncodingCodes.UInt0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithUInt0EncodingsArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, true, EncodingCodes.UInt0);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithNullEncodingsArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, false, EncodingCodes.Null);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithNullEncodingsArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, false, EncodingCodes.Null);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithNullEncodingsArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, true, EncodingCodes.Null);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithNullEncodingsArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, true, EncodingCodes.Null);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithTrueEncodingsArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, false, EncodingCodes.BooleanTrue);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithTrueEncodingsArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, false, EncodingCodes.BooleanTrue);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithTrueEncodingsArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, true, EncodingCodes.BooleanTrue);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithTrueEncodingsArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, true, EncodingCodes.BooleanTrue);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithFalseEncodingsArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, false, EncodingCodes.BooleanFalse);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithFalseEncodingsArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, false, EncodingCodes.BooleanFalse);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithFalseEncodingsArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array8, true, EncodingCodes.BooleanFalse);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithFalseEncodingsArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes.Array32, true, EncodingCodes.BooleanFalse);
      }

      private void DoTestDecodeFailsWhenArrayOfTypeWithUZeroSizedEncodings(EncodingCodes arrayType, bool fromStream, EncodingCodes code)
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
            buffer.WriteUnsignedByte(5);  // Size
            buffer.WriteUnsignedByte(2);  // Count
         }
         buffer.WriteUnsignedByte(0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)NoLocalType.DescriptorCode);
         buffer.WriteUnsignedByte((byte)code);

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
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is UnknownDescribedType[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is UnknownDescribedType[]);
         }
      }

      [Test]
      public void TestCannotDecodeDescribedTypeWithNonPrimitiveValue()
      {
         DoTestCannotDecodeDescribedTypeWithNonPrimitiveValue(false);
      }

      [Test]
      public void TestCannotDecodeDescribedTypeWithNonPrimitiveValueFS()
      {
         DoTestCannotDecodeDescribedTypeWithNonPrimitiveValue(true);
      }

      private void DoTestCannotDecodeDescribedTypeWithNonPrimitiveValue(bool fromStream)
      {
         TestUnknownDescribedType toEncode =
            new TestUnknownDescribedType(TestUnknownDescribedType.DescriptorCode,
               new TestUnknownDescribedType(TestUnknownDescribedType.DescriptorCode, null));

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteObject(buffer, encoderState, toEncode);

         if (fromStream)
         {
            Assert.Throws<DecodeException>(() => streamDecoder.ReadObject(stream, streamDecoderState));
         }
         else
         {
            Assert.Throws<DecodeException>(() => decoder.ReadObject(buffer, decoderState));
         }
      }

      public class TestUnknownDescribedType : IDescribedType
      {
         public static readonly ulong DescriptorCode = 0xAA00468C00000003UL;

         internal TestUnknownDescribedType(object descriptor, object described)
         {
            Descriptor = descriptor;
            Described = described;
         }

         /// <summary>
         /// Access the descriptor that was used to describe this type
         /// </summary>
         public object Descriptor { get; }

         /// <summary>
         /// Access the object that was conveyed in this described type.
         /// </summary>
         public object Described { get; }

      }
   }
}