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

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class BooleanTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsBoolean()
      {
         TestDecoderThrowsWhenAskedToReadWrongTypeAsBoolean(false);
      }

      [Test]
      public void TestDecoderThrowsWhenAskedToReadWrongTypeAsBooleanFS()
      {
         TestDecoderThrowsWhenAskedToReadWrongTypeAsBoolean(true);
      }

      private void TestDecoderThrowsWhenAskedToReadWrongTypeAsBoolean(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadBoolean(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as bool");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadBoolean(stream, streamDecoderState, false);
               Assert.Fail("Should not allow read of integer type as bool");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadBoolean(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as bool");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadBoolean(buffer, decoderState, false);
               Assert.Fail("Should not allow read of integer type as bool");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeBooleanEncodedBytes()
      {
         TestDecodeBooleanEncodedBytes(false);
      }

      [Test]
      public void TestDecodeBooleanEncodedBytesFS()
      {
         TestDecodeBooleanEncodedBytes(true);
      }

      private void TestDecodeBooleanEncodedBytes(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanTrue));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanFalse));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(1);

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadBoolean(stream, streamDecoderState));

            bool? result1 = streamDecoder.ReadBoolean(stream, streamDecoderState);
            bool? result2 = streamDecoder.ReadBoolean(stream, streamDecoderState);
            bool? result3 = streamDecoder.ReadBoolean(stream, streamDecoderState);
            bool? result4 = streamDecoder.ReadBoolean(stream, streamDecoderState);

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);
         }
         else
         {
            Assert.IsNull(decoder.ReadBoolean(buffer, decoderState));

            bool? result1 = decoder.ReadBoolean(buffer, decoderState);
            bool? result2 = decoder.ReadBoolean(buffer, decoderState);
            bool? result3 = decoder.ReadBoolean(buffer, decoderState);
            bool? result4 = decoder.ReadBoolean(buffer, decoderState);

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);
         }
      }

      [Test]
      public void TestDecodeBooleanEncodedBytesWithTypeDecoder()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(false);
      }

      [Test]
      public void TestDecodeBooleanEncodedBytesWithTypeDecoderFS()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(true);
      }

      private void TestDecodeBooleanEncodedBytesWithTypeDecoder(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanTrue));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanFalse));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(1);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((bool)typeDecoder.ReadValue(stream, streamDecoderState));
            typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((bool)typeDecoder.ReadValue(stream, streamDecoderState));
            typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((bool)typeDecoder.ReadValue(stream, streamDecoderState));
            typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((bool)typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((bool)typeDecoder.ReadValue(buffer, decoderState));
            typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((bool)typeDecoder.ReadValue(buffer, decoderState));
            typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((bool)typeDecoder.ReadValue(buffer, decoderState));
            typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((bool)typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeBooleanTrueArray32()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array32, EncodingCodes.BooleanTrue, false);
      }

      [Test]
      public void TestDecodeBooleanTrueArray32FromStream()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array32, EncodingCodes.BooleanTrue, true);
      }

      [Test]
      public void TestDecodeBooleanFalseArray32()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array32, EncodingCodes.BooleanFalse, false);
      }

      [Test]
      public void TestDecodeBooleanFalseArray32FromStream()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array32, EncodingCodes.BooleanFalse, true);
      }

      [Test]
      public void TestDecodeBooleanTrueArray8()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array8, EncodingCodes.BooleanTrue, false);
      }

      [Test]
      public void TestDecodeBooleanTrueArray8FromStream()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array8, EncodingCodes.BooleanTrue, true);
      }

      [Test]
      public void TestDecodeBooleanFalseArray8()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array8, EncodingCodes.BooleanFalse, false);
      }

      [Test]
      public void TestDecodeBooleanFalseArray8FromStream()
      {
         TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes.Array8, EncodingCodes.BooleanFalse, true);
      }

      private void TestDecodeBooleanEncodedBytesWithTypeDecoder(EncodingCodes arrayType, EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(3);  // Size
            buffer.WriteInt(10); // Count
            buffer.WriteUnsignedByte(((byte)encodingCode));
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array8));
            buffer.WriteUnsignedByte(3);  // Size
            buffer.WriteUnsignedByte(10); // Count
            buffer.WriteUnsignedByte(((byte)encodingCode));
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            bool[] booleans = (bool[])arrayDecoder.ReadValue(stream, streamDecoderState);
            Assert.AreEqual(10, booleans.Length);
            foreach (bool value in booleans)
            {
               Assert.AreEqual(encodingCode == EncodingCodes.BooleanTrue, value);
            }
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveArrayTypeDecoder);
            IPrimitiveArrayTypeDecoder arrayDecoder = (IPrimitiveArrayTypeDecoder)typeDecoder;
            bool[] booleans = (bool[])arrayDecoder.ReadValue(buffer, decoderState);
            Assert.AreEqual(10, booleans.Length);
            foreach (bool value in booleans)
            {
               Assert.AreEqual(encodingCode == EncodingCodes.BooleanTrue, value);
            }
         }
      }

      [Test]
      public void TestPeekNextTypeDecoder()
      {
         TestPeekNextTypeDecoder(false);
      }

      [Test]
      public void TestPeekNextTypeDecoderFS()
      {
         TestPeekNextTypeDecoder(true);
      }

      private void TestPeekNextTypeDecoder(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanTrue));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanFalse));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(1);

         if (fromStream)
         {
            Assert.AreEqual(typeof(bool), streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState).DecodesType);
            Assert.IsTrue(streamDecoder.ReadBoolean(stream, streamDecoderState));
            Assert.AreEqual(typeof(bool), streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState).DecodesType);
            Assert.IsFalse(streamDecoder.ReadBoolean(stream, streamDecoderState));
            Assert.AreEqual(typeof(bool), streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState).DecodesType);
            Assert.IsFalse(streamDecoder.ReadBoolean(stream, streamDecoderState));
            Assert.AreEqual(typeof(bool), streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState).DecodesType);
            Assert.IsTrue(streamDecoder.ReadBoolean(stream, streamDecoderState));
         }
         else
         {
            Assert.AreEqual(typeof(bool), decoder.PeekNextTypeDecoder(buffer, decoderState).DecodesType);
            Assert.IsTrue(decoder.ReadBoolean(buffer, decoderState));
            Assert.AreEqual(typeof(bool), decoder.PeekNextTypeDecoder(buffer, decoderState).DecodesType);
            Assert.IsFalse(decoder.ReadBoolean(buffer, decoderState));
            Assert.AreEqual(typeof(bool), decoder.PeekNextTypeDecoder(buffer, decoderState).DecodesType);
            Assert.IsFalse(decoder.ReadBoolean(buffer, decoderState));
            Assert.AreEqual(typeof(bool), decoder.PeekNextTypeDecoder(buffer, decoderState).DecodesType);
            Assert.IsTrue(decoder.ReadBoolean(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeBooleanEncodedBytesAsPrimtives()
      {
         TestDecodeBooleanEncodedBytesAsPrimtives(false);
      }

      [Test]
      public void TestDecodeBooleanEncodedBytesAsPrimtivesFS()
      {
         TestDecodeBooleanEncodedBytesAsPrimtives(true);
      }

      private void TestDecodeBooleanEncodedBytesAsPrimtives(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanTrue));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanFalse));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(1);

         if (fromStream)
         {
            bool result1 = streamDecoder.ReadBoolean(stream, streamDecoderState, false);
            bool result2 = streamDecoder.ReadBoolean(stream, streamDecoderState, true);
            bool result3 = streamDecoder.ReadBoolean(stream, streamDecoderState, true);
            bool result4 = streamDecoder.ReadBoolean(stream, streamDecoderState, false);

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);
         }
         else
         {
            bool result1 = decoder.ReadBoolean(buffer, decoderState, false);
            bool result2 = decoder.ReadBoolean(buffer, decoderState, true);
            bool result3 = decoder.ReadBoolean(buffer, decoderState, true);
            bool result4 = decoder.ReadBoolean(buffer, decoderState, false);

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
            Assert.IsFalse(result3);
            Assert.IsTrue(result4);
         }
      }

      [Test]
      public void TestDecodeBooleanTrue()
      {
         TestDecodeBooleanTrue(false);
      }

      [Test]
      public void TestDecodeBooleanTrueFS()
      {
         TestDecodeBooleanTrue(true);
      }

      private void TestDecodeBooleanTrue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteBoolean(buffer, encoderState, true);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }
         Assert.IsTrue(result is Boolean);
         Assert.IsTrue((bool)result);

         encoder.WriteBoolean(buffer, encoderState, true);

         bool? booleanResult;
         if (fromStream)
         {
            booleanResult = streamDecoder.ReadBoolean(stream, streamDecoderState);
         }
         else
         {
            booleanResult = decoder.ReadBoolean(buffer, decoderState);
         }
         Assert.IsTrue(booleanResult);
      }

      [Test]
      public void TestDecodeBooleanFalse()
      {
         TestDecodeBooleanFalse(false);
      }

      [Test]
      public void TestDecodeBooleanFalseFS()
      {
         TestDecodeBooleanFalse(true);
      }

      private void TestDecodeBooleanFalse(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteBoolean(buffer, encoderState, false);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is bool);
         Assert.IsFalse((bool)result);
      }

      [Test]
      public void TestDecodeBooleanFromNullEncoding()
      {
         TestDecodeBooleanFromNullEncoding(false);
      }

      [Test]
      public void TestDecodeBooleanFromNullEncodingFS()
      {
         TestDecodeBooleanFromNullEncoding(true);
      }

      private void TestDecodeBooleanFromNullEncoding(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteBoolean(buffer, encoderState, true);
         encoder.WriteNull(buffer, encoderState);

         bool? result;
         if (fromStream)
         {
            result = streamDecoder.ReadBoolean(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadBoolean(buffer, decoderState);
         }
         Assert.IsTrue(result);
         Assert.IsNull(decoder.ReadBoolean(buffer, decoderState));
      }

      [Test]
      public void TestDecodeBooleanAsPrimitiveWithDefault()
      {
         TestDecodeBooleanAsPrimitiveWithDefault(false);
      }

      [Test]
      public void TestDecodeBooleanAsPrimitiveWithDefaultFS()
      {
         TestDecodeBooleanAsPrimitiveWithDefault(true);
      }

      private void TestDecodeBooleanAsPrimitiveWithDefault(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteBoolean(buffer, encoderState, true);
         encoder.WriteNull(buffer, encoderState);

         if (fromStream)
         {
            bool result = streamDecoder.ReadBoolean(stream, streamDecoderState, false);
            Assert.IsTrue(result);
            result = streamDecoder.ReadBoolean(stream, streamDecoderState, false);
            Assert.IsFalse(result);
         }
         else
         {
            bool result = decoder.ReadBoolean(buffer, decoderState, false);
            Assert.IsTrue(result);
            result = decoder.ReadBoolean(buffer, decoderState, false);
            Assert.IsFalse(result);
         }
      }

      [Test]
      public void TestDecodeBooleanFailsForNonBooleanType()
      {
         TestDecodeBooleanFailsForNonBooleanType(false);
      }

      [Test]
      public void TestDecodeBooleanFailsForNonBooleanTypeFS()
      {
         TestDecodeBooleanFailsForNonBooleanType(true);
      }

      private void TestDecodeBooleanFailsForNonBooleanType(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteLong(buffer, encoderState, 1L);

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadBoolean(stream, streamDecoderState);
               Assert.Fail("Should not read long as bool value.");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadBoolean(buffer, decoderState);
               Assert.Fail("Should not read long as bool value.");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeSmallSeriesOfBooleans()
      {
         DoTestDecodeBooleanSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfBooleans()
      {
         DoTestDecodeBooleanSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfBooleansFS()
      {
         DoTestDecodeBooleanSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfBooleansFS()
      {
         DoTestDecodeBooleanSeries(LargeSize, true);
      }

      private void DoTestDecodeBooleanSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteBoolean(buffer, encoderState, i % 2 == 0);
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
            Assert.IsTrue(result is Boolean);

            Boolean boolValue = (Boolean)result;
            Assert.AreEqual(i % 2 == 0, boolValue);
         }
      }

      [Test]
      public void TestArrayOfBooleanObjects()
      {
         TestArrayOfBooleanObjects(false);
      }

      [Test]
      public void TestArrayOfBooleanObjectsFS()
      {
         TestArrayOfBooleanObjects(true);
      }

      private void TestArrayOfBooleanObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         Boolean[] source = new Boolean[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = i % 2 == 0;
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
         Assert.AreEqual(typeof(bool), result.GetType().GetElementType());

         bool[] array = (bool[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      [Test]
      public void TestZeroSizedArrayOfBooleanObjects()
      {
         TestZeroSizedArrayOfBooleanObjects(false);
      }

      [Test]
      public void TestZeroSizedArrayOfBooleanObjectsFS()
      {
         TestZeroSizedArrayOfBooleanObjects(true);
      }

      private void TestZeroSizedArrayOfBooleanObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Boolean[] source = new Boolean[0];

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

         bool[] array = (bool[])result;
         Assert.AreEqual(source.Length, array.Length);
      }

      [Test]
      public void TestDecodeSmallBooleanArray()
      {
         DoTestDecodeBooleanArrayType(SmallArraySize, false);
      }

      [Test]
      public void TestDecodeLargeBooleanArray()
      {
         DoTestDecodeBooleanArrayType(LargeArraySize, false);
      }

      [Test]
      public void TestDecodeSmallBooleanArrayFS()
      {
         DoTestDecodeBooleanArrayType(SmallArraySize, true);
      }

      [Test]
      public void TestDecodeLargeBooleanArrayFS()
      {
         DoTestDecodeBooleanArrayType(LargeArraySize, true);
      }

      private void DoTestDecodeBooleanArrayType(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         bool[] source = new bool[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = i % 2 == 0;
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

         bool[] array = (bool[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      // TODO [Test]
      public void TestArrayOfArraysOfPrimitiveBooleanObjects()
      {
         TestArrayOfArraysOfPrimitiveBooleanObjects(false);
      }

      // TODO [Test]
      public void TestArrayOfArraysOfPrimitiveBooleanObjectsFS()
      {
         TestArrayOfArraysOfPrimitiveBooleanObjects(true);
      }

      private void TestArrayOfArraysOfPrimitiveBooleanObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         bool[][] source = new bool[2][];

         source[0] = new bool[10];
         source[1] = new bool[10];

         for (int i = 0; i < size; ++i)
         {
            source[0][i] = i % 2 == 0;
            source[1][i] = i % 2 == 0;
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

         bool[][] resultArray = (bool[][])result;

         Assert.IsNotNull(resultArray);
         Assert.AreEqual(2, resultArray.Length);

         Assert.IsTrue(resultArray.GetValue(0).GetType().IsArray);
         Assert.IsTrue(resultArray.GetValue(1).GetType().IsArray);

         for (int i = 0; i < resultArray.Length; ++i)
         {
            bool[] nested = (bool[])resultArray.GetValue(i);
            Assert.AreEqual(source[i][0], nested);
         }
      }

      [Test]
      public void TestReadAllBooleanTypeEncodings()
      {
         TestReadAllBooleanTypeEncodings(false);
      }

      [Test]
      public void TestReadAllBooleanTypeEncodingsFS()
      {
         TestReadAllBooleanTypeEncodings(true);
      }

      private void TestReadAllBooleanTypeEncodings(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanTrue));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.BooleanFalse));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(1);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
         buffer.WriteUnsignedByte(0);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((Boolean)typeDecoder.ReadValue(stream, streamDecoderState));
            typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((Boolean)typeDecoder.ReadValue(stream, streamDecoderState));
            typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((Boolean)typeDecoder.ReadValue(stream, streamDecoderState));
            typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((Boolean)typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((Boolean)typeDecoder.ReadValue(buffer, decoderState));
            typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((Boolean)typeDecoder.ReadValue(buffer, decoderState));
            typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsTrue((Boolean)typeDecoder.ReadValue(buffer, decoderState));
            typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
            Assert.IsFalse((Boolean)typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestSkipValueFullBooleanTypeEncodings()
      {
         TestSkipValueFullBooleanTypeEncodings(false);
      }

      [Test]
      public void TestSkipValueFullBooleanTypeEncodingsFS()
      {
         TestSkipValueFullBooleanTypeEncodings(true);
      }

      private void TestSkipValueFullBooleanTypeEncodings(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
            buffer.WriteUnsignedByte(1);
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Boolean));
            buffer.WriteUnsignedByte(0);
         }

         encoder.WriteObject(buffer, encoderState, false);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Boolean);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Boolean);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Boolean);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.Boolean);
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
         Assert.IsTrue(result is Boolean);

         Boolean value = (Boolean)result;
         Assert.AreEqual(false, value);
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
            encoder.WriteBoolean(buffer, encoderState, true);
            encoder.WriteBoolean(buffer, encoderState, false);
         }

         encoder.WriteObject(buffer, encoderState, false);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.BooleanTrue);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.BooleanFalse);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.BooleanTrue);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, EncodingCodes.BooleanFalse);
               Assert.AreEqual(typeof(bool), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Boolean);

         Boolean value = (Boolean)result;
         Assert.AreEqual(false, value);
      }
   }
}