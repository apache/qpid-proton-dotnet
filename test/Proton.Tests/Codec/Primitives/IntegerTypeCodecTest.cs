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
   public class IntegerTypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadInt(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               streamDecoder.ReadInt(stream, streamDecoderState, (short)0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadInteger(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }

            try
            {
               decoder.ReadInteger(buffer, decoderState, (short)0);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestReadUByteFromEncodingCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Int));
         buffer.WriteInt(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Int));
         buffer.WriteInt(44);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallInt));
         buffer.WriteUnsignedByte(43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.AreEqual(42, decoder.ReadInteger(buffer, decoderState));
         Assert.AreEqual(44, decoder.ReadInteger(buffer, decoderState, 42));
         Assert.AreEqual(43, decoder.ReadInteger(buffer, decoderState, 42));
         Assert.IsNull(decoder.ReadInteger(buffer, decoderState));
         Assert.AreEqual(42, decoder.ReadInteger(buffer, decoderState, 42));
      }

      [Test]
      public void TestReadUByteFromEncodingCodeFromStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Int));
         buffer.WriteInt(42);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Int));
         buffer.WriteInt(44);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallInt));
         buffer.WriteUnsignedByte(43);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.AreEqual(42, streamDecoder.ReadInt(stream, streamDecoderState));
         Assert.AreEqual(44, streamDecoder.ReadInt(stream, streamDecoderState, 42));
         Assert.AreEqual(43, streamDecoder.ReadInt(stream, streamDecoderState, 42));
         Assert.IsNull(streamDecoder.ReadInt(stream, streamDecoderState));
         Assert.AreEqual(42, streamDecoder.ReadInt(stream, streamDecoderState, 42));
      }

      [Test]
      public void TestGetTypeCode()
      {
         Assert.AreEqual(EncodingCodes.Int, new Integer32TypeDecoder().EncodingCode);
         Assert.AreEqual(EncodingCodes.SmallInt, new Integer8TypeDecoder().EncodingCode);
      }

      [Test]
      public void TestGetTypeClass()
      {
         Assert.AreEqual(typeof(int), new IntegerTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(int), new Integer8TypeDecoder().DecodesType);
         Assert.AreEqual(typeof(int), new Integer32TypeDecoder().DecodesType);
      }

      [Test]
      public void TestReadIntegerFromEncodingCodeInt()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Int));
         buffer.WriteInt(42);

         Assert.AreEqual(42, decoder.ReadInteger(buffer, decoderState));
      }

      [Test]
      public void TestReadIntegerFromEncodingCodeSmallInt()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallInt));
         buffer.WriteUnsignedByte(42);

         Assert.AreEqual(42, decoder.ReadInteger(buffer, decoderState));
      }

      [Test]
      public void TestReadIntegerFromEncodingCodeIntFromStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Int));
         buffer.WriteInt(42);

         Assert.AreEqual(42, streamDecoder.ReadInt(stream, streamDecoderState));
      }

      [Test]
      public void TestReadIntegerFromEncodingCodeSmallIntFromStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallInt));
         buffer.WriteUnsignedByte(42);

         Assert.AreEqual(42, streamDecoder.ReadInt(stream, streamDecoderState));
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

      public void DoTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteInteger(buffer, encoderState, int.MaxValue);
            encoder.WriteInteger(buffer, encoderState, 16);
         }

         int expected = 42;

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(int), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
               typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(int), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(int), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
               typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(int), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is int);

         int value = (int)result;
         Assert.AreEqual(expected, value);
      }

      [Test]
      public void TestArrayOfObjects()
      {
         doTestArrayOfObjects(false);
      }

      [Test]
      public void TestArrayOfObjectsFromStream()
      {
         doTestArrayOfObjects(true);
      }

      protected void doTestArrayOfObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         int[] source = new int[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = random.Next();
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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(int));

         int[] array = (int[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }

      [Test]
      public void TestZeroSizedArrayOfObjects()
      {
         TestZeroSizedArrayOfObjects(false);
      }

      [Test]
      public void TestZeroSizedArrayOfObjectsFS()
      {
         TestZeroSizedArrayOfObjects(true);
      }

      private void TestZeroSizedArrayOfObjects(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         int[] source = new int[0];

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
         Assert.AreEqual(result.GetType().GetElementType(), typeof(int));

         int[] array = (int[])result;
         Assert.AreEqual(source.Length, array.Length);
      }

      [Test]
      public void TestReadIntegerArrayInt32()
      {
         doTestReadIntegerArray(EncodingCodes.Int, false);
      }

      [Test]
      public void TestReadIntegerArrayInt32FromStream()
      {
         doTestReadIntegerArray(EncodingCodes.Int, true);
      }

      [Test]
      public void TestReadIntegerArrayInt8()
      {
         doTestReadIntegerArray(EncodingCodes.SmallInt, false);
      }

      [Test]
      public void TestReadIntegerArrayInt8FromStream()
      {
         doTestReadIntegerArray(EncodingCodes.SmallInt, true);
      }

      public void doTestReadIntegerArray(EncodingCodes encoding, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         if (encoding == EncodingCodes.Int)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(17);  // Size
            buffer.WriteInt(2);   // Count
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Int));
            buffer.WriteInt(1);   // [0]
            buffer.WriteInt(2);   // [1]
         }
         else if (encoding == EncodingCodes.SmallInt)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Array32));
            buffer.WriteInt(11);  // Size
            buffer.WriteInt(2);   // Count
            buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallInt));
            buffer.WriteUnsignedByte(1);   // [0]
            buffer.WriteUnsignedByte(2);   // [1]
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
         Assert.IsTrue(result.GetType().IsArray);
         Assert.AreEqual(result.GetType().GetElementType(), typeof(int));

         int[] array = (int[])result;

         Assert.AreEqual(2, array.Length);
         Assert.AreEqual(1, array[0]);
         Assert.AreEqual(2, array[1]);
      }
   }
}