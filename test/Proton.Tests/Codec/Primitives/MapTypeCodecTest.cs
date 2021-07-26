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
using System.Text;
using System.Collections;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class MapTypeCodecTest : CodecTestSupport
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
               streamDecoder.ReadMap<string, object>(stream, streamDecoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadMap<string, object>(buffer, decoderState);
               Assert.Fail("Should not allow read of integer type as this type");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeSmallSeriesOfMaps()
      {
         DoTestDecodeMapSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfMaps()
      {
         DoTestDecodeMapSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfMapsFromStream()
      {
         DoTestDecodeMapSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfMapsFromStream()
      {
         DoTestDecodeMapSeries(LargeSize, true);
      }

      private void DoTestDecodeMapSeries(int size, bool fromStream)
      {
         String myBoolKey = "myBool";
         bool myBool = true;
         String myByteKey = "myByte";
         byte myByte = 4;
         String myBytesKey = "myBytes";
         byte[] myBytes = new UTF8Encoding().GetBytes(myBytesKey);
         String myCharKey = "myChar";
         char myChar = 'd';
         String myDoubleKey = "myDouble";
         double myDouble = 1234567890123456789.1234;
         String myFloatKey = "myFloat";
         float myFloat = 1.1F;
         String myIntKey = "myInt";
         int myInt = Int32.MaxValue;
         String myLongKey = "myLong";
         long myLong = Int64.MaxValue;
         String myShortKey = "myShort";
         short myShort = 25;
         String myStringKey = "myString";
         String myString = myStringKey;

         IDictionary<string, object> map = new Dictionary<string, object>();
         map.Add(myBoolKey, myBool);
         map.Add(myByteKey, myByte);
         map.Add(myBytesKey, ProtonByteBufferAllocator.Instance.Wrap(myBytes));
         map.Add(myCharKey, myChar);
         map.Add(myDoubleKey, myDouble);
         map.Add(myFloatKey, myFloat);
         map.Add(myIntKey, myInt);
         map.Add(myLongKey, myLong);
         map.Add(myShortKey, myShort);
         map.Add(myStringKey, myString);

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, map);
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
            Assert.IsTrue(result is IDictionary);

            IDictionary resultMap = (IDictionary)result;

            Assert.AreEqual(map.Count, resultMap.Count);
            foreach (KeyValuePair<string, object> entry in map)
            {
               Assert.IsTrue(resultMap.Contains(entry.Key));
               Assert.AreEqual(map[entry.Key], resultMap[entry.Key]);
            }
         }
      }

      [Test]
      public void TestDecodeSmallSeriesOfMapsUsingReadMap()
      {
         DoTestDecodeMapSeriesUsingReadMap(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfMapsUsingReadMap()
      {
         DoTestDecodeMapSeriesUsingReadMap(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfMapsFromStreamUsingReadMap()
      {
         DoTestDecodeMapSeriesUsingReadMap(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfMapsFromStreamUsingReadMap()
      {
         DoTestDecodeMapSeriesUsingReadMap(LargeSize, true);
      }

      private void DoTestDecodeMapSeriesUsingReadMap(int size, bool fromStream)
      {
         String myBoolKey = "myBool";
         bool myBool = true;
         String myByteKey = "myByte";
         byte myByte = 4;
         String myBytesKey = "myBytes";
         byte[] myBytes = new UTF8Encoding().GetBytes(myBytesKey);
         String myCharKey = "myChar";
         char myChar = 'd';
         String myDoubleKey = "myDouble";
         double myDouble = 1234567890123456789.1234;
         String myFloatKey = "myFloat";
         float myFloat = 1.1F;
         String myIntKey = "myInt";
         int myInt = Int32.MaxValue;
         String myLongKey = "myLong";
         long myLong = Int64.MaxValue;
         String myShortKey = "myShort";
         short myShort = 25;
         String myStringKey = "myString";
         String myString = myStringKey;

         IDictionary<string, object> map = new Dictionary<string, object>();
         map.Add(myBoolKey, myBool);
         map.Add(myByteKey, myByte);
         map.Add(myBytesKey, ProtonByteBufferAllocator.Instance.Wrap(myBytes));
         map.Add(myCharKey, myChar);
         map.Add(myDoubleKey, myDouble);
         map.Add(myFloatKey, myFloat);
         map.Add(myIntKey, myInt);
         map.Add(myLongKey, myLong);
         map.Add(myShortKey, myShort);
         map.Add(myStringKey, myString);

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, map);
         }

         for (int i = 0; i < size; ++i)
         {
            IDictionary<string, object> result;
            if (fromStream)
            {
               result = streamDecoder.ReadMap<string, object>(stream, streamDecoderState);
            }
            else
            {
               result = decoder.ReadMap<string, object>(buffer, decoderState);
            }

            Assert.IsNotNull(result);
            Assert.IsTrue(result is IDictionary);

            Assert.AreEqual(map.Count, result.Count);
            Assert.AreEqual(map, result);
         }
      }

      [Test]
      public void TestArrayOfMApsOfStringToUUIDs()
      {
         TestArrayOfMApsOfStringToUUIDs(false);
      }

      [Test]
      public void TestArrayOfMApsOfStringToUUIDsFS()
      {
         TestArrayOfMApsOfStringToUUIDs(true);
      }

      private void TestArrayOfMApsOfStringToUUIDs(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<string, Guid>[] source = new Dictionary<string, Guid>[2];
         for (int i = 0; i < source.Length; ++i)
         {
            source[i] = new Dictionary<string, Guid>();
            source[i].Add("1", Guid.NewGuid());
            source[i].Add("2", Guid.NewGuid());
            source[i].Add("3", Guid.NewGuid());
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

         IDictionary[] map = (IDictionary[])result;
         Assert.AreEqual(source.Length, map.Length);

         for (int i = 0; i < map.Length; ++i)
         {
            Assert.AreEqual(source[i], map[i]);
         }
      }

      [Test]
      public void TestMapOfArraysOfUUIDsIndexedByString()
      {
         TestMapOfArraysOfUUIDsIndexedByString(false);
      }

      [Test]
      public void TestMapOfArraysOfUUIDsIndexedByStringFS()
      {
         TestMapOfArraysOfUUIDsIndexedByString(true);
      }

      private void TestMapOfArraysOfUUIDsIndexedByString(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[] element1 = new Guid[] { Guid.NewGuid() };
         Guid[] element2 = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
         Guid[] element3 = new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

         Guid[][] expected = new Guid[][] { element1, element2, element3 };

         IDictionary<string, Guid[]> source = new Dictionary<string, Guid[]>();
         source.Add("1", element1);
         source.Add("2", element2);
         source.Add("3", element3);

         encoder.WriteMap(buffer, encoderState, source);

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
         Assert.IsTrue(result is IDictionary);

         IDictionary map = (IDictionary)result;
         Assert.AreEqual(source.Count, map.Count);

         for (int i = 1; i <= map.Count; ++i)
         {
            object entry = map[i.ToString()];
            Assert.IsNotNull(entry);
            Assert.IsTrue(entry.GetType().IsArray);
            Guid[] uuids = (Guid[])entry;
            Assert.AreEqual(i, uuids.Length);
            Assert.AreEqual(expected[i - 1], uuids);
         }
      }

      [Test]
      public void TestSizeToLargeValidationMAP32()
      {
         dotestSizeToLargeValidation(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestSizeToLargeValidationMAP8()
      {
         dotestSizeToLargeValidation(EncodingCodes.Map8, true);
      }

      private void dotestSizeToLargeValidation(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)encodingCode));
         if (encodingCode == EncodingCodes.Map32)
         {
            buffer.WriteInt(Int32.MaxValue);
            buffer.WriteInt(4);
         }
         else
         {
            buffer.WriteUnsignedByte(Byte.MaxValue);
            buffer.WriteUnsignedByte(4);
         }
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str8));
         buffer.WriteUnsignedByte(4);
         buffer.WriteBytes(new UTF8Encoding().GetBytes("test"));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str8));
         buffer.WriteUnsignedByte(5);
         buffer.WriteBytes(new UTF8Encoding().GetBytes("value"));

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(IDictionary), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, encodingCode);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.PeekNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(IDictionary), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder is IPrimitiveTypeDecoder);
            Assert.AreEqual(((IPrimitiveTypeDecoder)typeDecoder).EncodingCode, encodingCode);
         }

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("should throw an ArgumentException");
            }
            catch (DecodeEOFException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("should throw an ArgumentException");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestOddElementCountDetectedMAP32()
      {
         DoTestOddElementCountDetected(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestOddElementCountDetectedMAP8()
      {
         DoTestOddElementCountDetected(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestOddElementCountDetectedMAP32FS()
      {
         DoTestOddElementCountDetected(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestOddElementCountDetectedMAP8FS()
      {
         DoTestOddElementCountDetected(EncodingCodes.Map8, true);
      }

      private void DoTestOddElementCountDetected(EncodingCodes encodingCode, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)encodingCode));
         if (encodingCode == EncodingCodes.Map32)
         {
            buffer.WriteInt(17);
            buffer.WriteInt(1);
         }
         else
         {
            buffer.WriteUnsignedByte(14);
            buffer.WriteUnsignedByte(1);
         }
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str8));
         buffer.WriteUnsignedByte(4);
         buffer.WriteBytes(new UTF8Encoding().GetBytes("test"));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str8));
         buffer.WriteUnsignedByte(5);
         buffer.WriteBytes(new UTF8Encoding().GetBytes("value"));

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("should throw an DecodeException");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("should throw DecodeException");
            }
            catch (DecodeException) { }
         }
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

         IDictionary<string, Guid> skip = new Dictionary<string, Guid>();
         for (int i = 0; i < 10; ++i)
         {
            skip.Add(Guid.NewGuid().ToString(), Guid.NewGuid());
         }

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteMap(buffer, encoderState, skip);
         }

         IDictionary<string, Guid> expected = new Dictionary<string, Guid>();
         expected.Add(Guid.NewGuid().ToString(), Guid.NewGuid());

         encoder.WriteObject(buffer, encoderState, expected);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(IDictionary), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(IDictionary), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is IDictionary);
         IDictionary dictionary = (IDictionary)result;

         Assert.AreEqual(expected.Count, dictionary.Count);
         foreach (KeyValuePair<string, Guid> entry in expected)
         {
            Assert.IsTrue(dictionary.Contains(entry.Key));
            Assert.AreEqual(expected[entry.Key], entry.Value);
         }
      }

      [Test]
      public void TestEncodeMapWithUnknownEntryValueType()
      {
         IDictionary<string, object> map = new Dictionary<string, object>();
         map.Add("unknown", new MyUnknownTestType());

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            encoder.WriteMap(buffer, encoderState, map);
            Assert.Fail("Expected exception to be thrown");
         }
         catch (EncodeException iae)
         {
            Assert.IsTrue(iae.Message.Contains("Cannot find encoder for type"));
            Assert.IsTrue(iae.Message.Contains(typeof(MyUnknownTestType).Name));
         }
      }

      [Test]
      public void TestEncodeSubMapWithUnknownEntryValueType()
      {
         IDictionary<string, object> subMap = new Dictionary<string, object>();
         subMap.Add("unknown", new MyUnknownTestType());

         IDictionary<string, object> map = new Dictionary<string, object>();
         map.Add("submap", subMap);

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            encoder.WriteMap(buffer, encoderState, map);
            Assert.Fail("Expected exception to be thrown");
         }
         catch (EncodeException iae)
         {
            Assert.IsTrue(iae.Message.Contains("Cannot find encoder for type"));
            Assert.IsTrue(iae.Message.Contains(typeof(MyUnknownTestType).Name));
         }
      }

      [Test]
      public void TestEncodeMapWithUnknownEntryKeyType()
      {
         IDictionary<object, string> map = new Dictionary<object, string>();
         map.Add(new MyUnknownTestType(), "unknown");

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            encoder.WriteMap(buffer, encoderState, map);
            Assert.Fail("Expected exception to be thrown");
         }
         catch (EncodeException iae)
         {
            Assert.IsTrue(iae.Message.Contains("Cannot find encoder for type"));
            Assert.IsTrue(iae.Message.Contains(typeof(MyUnknownTestType).Name));
         }
      }

      [Test]
      public void TestEncodeSubMapWithUnknownEntryKeyType()
      {
         IDictionary<object, string> subMap = new Dictionary<object, string>();
         subMap.Add(new MyUnknownTestType(), "unknown");

         IDictionary<string, object> map = new Dictionary<string, object>();
         map.Add("submap", subMap);

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            encoder.WriteMap(buffer, encoderState, map);
            Assert.Fail("Expected exception to be thrown");
         }
         catch (EncodeException iae)
         {
            Assert.IsTrue(iae.Message.Contains("Cannot find encoder for type"));
            Assert.IsTrue(iae.Message.Contains(typeof(MyUnknownTestType).Name));
         }
      }

      internal class MyUnknownTestType
      {

      }
   }
}