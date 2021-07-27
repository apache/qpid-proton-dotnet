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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using System.Collections;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class ListTypeCodecTest : CodecTestSupport
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

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadList<object>(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadList<object>(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
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

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List0));

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
         buffer.WriteUnsignedByte(4);
         buffer.WriteUnsignedByte(2);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Byte));
         buffer.WriteUnsignedByte(1);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Byte));
         buffer.WriteUnsignedByte(2);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
         buffer.WriteInt(4);
         buffer.WriteInt(2);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Byte));
         buffer.WriteUnsignedByte(1);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Byte));
         buffer.WriteUnsignedByte(2);

         IList<sbyte> expected = new List<sbyte>();

         expected.Add((sbyte)1);
         expected.Add((sbyte)2);

         if (fromStream)
         {
            Assert.IsNull(streamDecoder.ReadList<sbyte>(stream, streamDecoderState));
            Assert.IsTrue(streamDecoder.ReadList<sbyte>(stream, streamDecoderState).Count == 0);
            Assert.AreEqual(expected, streamDecoder.ReadList<sbyte>(stream, streamDecoderState));
            Assert.AreEqual(expected, streamDecoder.ReadList<sbyte>(stream, streamDecoderState));
         }
         else
         {
            Assert.IsNull(decoder.ReadList<sbyte>(buffer, decoderState));
            Assert.IsTrue(decoder.ReadList<sbyte>(buffer, decoderState).Count == 0);
            Assert.AreEqual(expected, decoder.ReadList<sbyte>(buffer, decoderState));
            Assert.AreEqual(expected, decoder.ReadList<sbyte>(buffer, decoderState));
         }
      }

      [Test]
      public void TestDecodeSmallSeriesOfSymbolLists()
      {
         DoTestDecodeSymbolListSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfSymbolLists()
      {
         DoTestDecodeSymbolListSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfSymbolListsFromStream()
      {
         DoTestDecodeSymbolListSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfSymbolListsFromStream()
      {
         DoTestDecodeSymbolListSeries(LargeSize, true);
      }

      private void DoTestDecodeSymbolListSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList<object> list = new List<object>();

         for (int i = 0; i < 50; ++i)
         {
            list.Add(Symbol.Lookup(i.ToString()));
         }

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, list);
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
            Assert.IsTrue(result is IList);

            IList<object> resultList = (IList<object>)result;

            Assert.AreEqual(list.Count, resultList.Count);
         }
      }

      [Test]
      public void TestDecodeSmallSeriesOfLists()
      {
         DoTestDecodeListSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfLists()
      {
         DoTestDecodeListSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfListsFS()
      {
         DoTestDecodeListSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfListsFS()
      {
         DoTestDecodeListSeries(LargeSize, true);
      }

      private void DoTestDecodeListSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList<object> list = new List<object>();

         DateTime timeNow = DateTime.Now;

         list.Add("ID:Message-1:1:1:0");
         list.Add(ProtonByteBufferAllocator.Instance.Wrap(new byte[1]));
         list.Add("queue:work");
         list.Add(Symbol.Lookup("text/UTF-8"));
         list.Add(Symbol.Lookup("text"));
         list.Add(timeNow);
         list.Add((uint)1);
         list.Add(Guid.NewGuid());

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, list);
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
            Assert.IsTrue(result is IList);

            IList resultList = (IList)result;

            Assert.AreEqual(list.Count, resultList.Count);
         }
      }

      [Test]
      public void TestArrayOfListsOfGuids()
      {
         DoTestArrayOfListsOfGuids(false);
      }

      [Test]
      public void TestArrayOfListsOfGuidsFromStream()
      {
         DoTestArrayOfListsOfGuids(true);
      }

      private void DoTestArrayOfListsOfGuids(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList[] source = new IList[2];
         for (int i = 0; i < source.Length; ++i)
         {
            source[i] = new ArrayList(3);
            source[i].Add(Guid.NewGuid());
            source[i].Add(Guid.NewGuid());
            source[i].Add(Guid.NewGuid());
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

         IList[] list = (IList[])result;
         Assert.AreEqual(source.Length, list.Length);

         for (int i = 0; i < list.Length; ++i)
         {
            Assert.AreEqual(source[i], list[i]);
         }
      }

      [Test]
      public void TestCountExceedsRemainingDetectedList32()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
         buffer.WriteInt(8);
         buffer.WriteInt(Int32.MaxValue);

         try
         {
            decoder.ReadObject(buffer, decoderState);
            Assert.Fail("should throw an IllegalArgumentException");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestCountExceedsRemainingDetectedList8()
      {
         TestCountExceedsRemainingDetectedList8(false);
      }

      [Test]
      public void TestCountExceedsRemainingDetectedList8FS()
      {
         TestCountExceedsRemainingDetectedList8(true);
      }

      private void TestCountExceedsRemainingDetectedList8(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
         buffer.WriteUnsignedByte(4);
         buffer.WriteUnsignedByte(byte.MaxValue);

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("should throw an IllegalArgumentException");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("should throw an IllegalArgumentException");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeEmptyList()
      {
         TestDecodeEmptyList(false);
      }

      [Test]
      public void TestDecodeEmptyListFS()
      {
         TestDecodeEmptyList(true);
      }

      private void TestDecodeEmptyList(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List0));

         object result;
         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(EncodingCodes.List0, ((IPrimitiveTypeDecoder)typeDecoder).EncodingCode);

            result = streamDecoder.ReadList<Guid>(stream, streamDecoderState);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.PeekNextTypeDecoder(buffer, decoderState);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(EncodingCodes.List0, ((IPrimitiveTypeDecoder)typeDecoder).EncodingCode);

            result = decoder.ReadList<Guid>(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.IsTrue(result is IList);

         IList<Guid> value = (IList<Guid>)result;
         Assert.AreEqual(0, value.Count);
      }

      [Test]
      public void TestEncodeEmptyListIsList0()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         encoder.WriteList<object>(buffer, encoderState, new List<object>());

         Assert.AreEqual(1, buffer.ReadableBytes);
         Assert.AreEqual(((byte)EncodingCodes.List0), buffer.ReadByte());
      }

      [Test]
      public void TestDecodeFailsEarlyOnInvliadLengthList8()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
         buffer.WriteUnsignedByte(255);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.AreEqual(typeof(IList), typeDecoder.DecodesType);

         try
         {
            typeDecoder.ReadValue(buffer, decoderState);
            Assert.Fail("Should not be able to read list with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(2, buffer.ReadOffset);
      }

      [Test]
      public void TestDecodeFailsEarlyOnInvliadLengthList32()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
         buffer.WriteInt(int.MaxValue);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.AreEqual(typeof(IList), typeDecoder.DecodesType);

         try
         {
            typeDecoder.ReadValue(buffer, decoderState);
            Assert.Fail("Should not be able to read list with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(5, buffer.ReadOffset);
      }

      [Test]
      public void TestDecodeFailsEarlyOnInvliadElementCountForList8()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
         buffer.WriteUnsignedByte(1);
         buffer.WriteUnsignedByte(255);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.AreEqual(typeof(IList), typeDecoder.DecodesType);
         Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
         Assert.AreEqual(EncodingCodes.List8, ((IPrimitiveTypeDecoder)typeDecoder).EncodingCode);

         try
         {
            typeDecoder.ReadValue(buffer, decoderState);
            Assert.Fail("Should not be able to read list with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(3, buffer.ReadOffset);
      }

      [Test]
      public void TestDecodeFailsEarlyOnInvliadElementLengthList32()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(16, 16);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
         buffer.WriteInt(2);
         buffer.WriteInt(int.MaxValue);

         ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
         Assert.AreEqual(typeof(IList), typeDecoder.DecodesType);
         Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
         Assert.AreEqual(EncodingCodes.List32, ((IPrimitiveTypeDecoder)typeDecoder).EncodingCode);

         try
         {
            typeDecoder.ReadValue(buffer, decoderState);
            Assert.Fail("Should not be able to read list with length greater than readable bytes");
         }
         catch (DecodeException) { }

         Assert.AreEqual(9, buffer.ReadOffset);
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

         IList<Guid> skip = new List<Guid>();
         for (int i = 0; i < 10; ++i)
         {
            skip.Add(Guid.NewGuid());
         }

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteList(buffer, encoderState, skip);
         }

         IList<Guid> expected = new List<Guid>();
         expected.Add(Guid.NewGuid());

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(IList), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(IList), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is IList);

         IList value = (IList)result;
         Assert.AreEqual(expected, value);
      }

      [Test]
      public void TestEncodeListWithUnknownEntryType()
      {
         IList<object> list = new List<object>();
         list.Add(new MyUnknownTestType());

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            encoder.WriteObject(buffer, encoderState, list);
            Assert.Fail("Expected exception to be thrown");
         }
         catch (EncodeException dex)
         {
            Assert.IsTrue(dex.Message.Contains("Cannot find encoder for type"));
            Assert.IsTrue(dex.Message.Contains(typeof(MyUnknownTestType).Name));
         }
      }

      [Test]
      public void TestEncodeSubListWithUnknownEntryType()
      {
         IList<object> subList = new List<object>();
         subList.Add(new MyUnknownTestType());

         IList<object> list = new List<object>();
         list.Add(subList);

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            encoder.WriteObject(buffer, encoderState, list);
            Assert.Fail("Expected exception to be thrown");
         }
         catch (EncodeException dex)
         {
            Assert.IsTrue(dex.Message.Contains("Cannot find encoder for type"));
            Assert.IsTrue(dex.Message.Contains(typeof(MyUnknownTestType).Name));
         }
      }

      internal class MyUnknownTestType
      {

      }
   }
}