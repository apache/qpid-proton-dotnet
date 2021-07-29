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
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Codec.Decoders.Messaging;
using Apache.Qpid.Proton.Codec.Encoders.Messaging;
using System;

namespace Apache.Qpid.Proton.Codec.Messaging
{
   [TestFixture]
   public class PropertiesTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Properties), new PropertiesTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Properties), new PropertiesTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Properties.DescriptorCode, new PropertiesTypeDecoder().DescriptorCode);
         Assert.AreEqual(Properties.DescriptorCode, new PropertiesTypeEncoder().DescriptorCode);
         Assert.AreEqual(Properties.DescriptorSymbol, new PropertiesTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Properties.DescriptorSymbol, new PropertiesTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeSmallSeriesOfProperties()
      {
         DoTestDecodePropertiesSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfProperties()
      {
         DoTestDecodePropertiesSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfPropertiesFromStream()
      {
         DoTestDecodePropertiesSeries(SmallSize, true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfPropertiesStream()
      {
         DoTestDecodePropertiesSeries(LargeSize, true);
      }

      private void DoTestDecodePropertiesSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Random random = new Random(Environment.TickCount);

         uint randomGroupSequence = (uint)random.Next();
         uint randomAbsoluteExpiry = (uint)random.Next();
         uint randomCreateTime = (uint)random.Next();

         Properties properties = new Properties();

         properties.MessageId = "ID:Message-1:1:1:0";
         properties.UserId = ProtonByteBufferAllocator.Instance.Wrap(new byte[1]);
         properties.To = "queue:work";
         properties.Subject = "help";
         properties.ReplyTo = "queue:temp:me";
         properties.ContentEncoding = "text/UTF-8";
         properties.ContentType = "text";
         properties.CorrelationId = "correlation-id";
         properties.AbsoluteExpiryTime = randomAbsoluteExpiry;
         properties.CreationTime = randomCreateTime;
         properties.GroupId = "group-1";
         properties.GroupSequence = randomGroupSequence;
         properties.ReplyToGroupId = "group-1";

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, properties);
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
            Assert.IsTrue(result is Properties);

            Properties decoded = (Properties)result;

            Assert.IsNotNull(decoded.AbsoluteExpiryTime);
            Assert.AreEqual(randomAbsoluteExpiry, decoded.AbsoluteExpiryTime);
            Assert.AreEqual("text/UTF-8", decoded.ContentEncoding);
            Assert.AreEqual("text", decoded.ContentType);
            Assert.AreEqual("correlation-id", decoded.CorrelationId);
            Assert.AreEqual(randomCreateTime, decoded.CreationTime);
            Assert.AreEqual("group-1", decoded.GroupId);
            Assert.AreEqual(randomGroupSequence, decoded.GroupSequence);
            Assert.AreEqual("ID:Message-1:1:1:0", decoded.MessageId);
            Assert.AreEqual("queue:temp:me", decoded.ReplyTo);
            Assert.AreEqual("group-1", decoded.ReplyToGroupId);
            Assert.AreEqual("help", decoded.Subject);
            Assert.AreEqual("queue:work", decoded.To);
            Assert.IsTrue(decoded.UserId is IProtonBuffer);
         }
      }

      [Test]
      public void TestEncodeAndDecodeWithMaxUnsignedValuesFromLongs()
      {
         DoTestEncodeAndDecodeWithMaxUnsignedValuesFromLongs(false);
      }

      [Test]
      public void TestEncodeAndDecodeWithMaxUnsignedValuesFromLongsFromStream()
      {
         DoTestEncodeAndDecodeWithMaxUnsignedValuesFromLongs(true);
      }

      private void DoTestEncodeAndDecodeWithMaxUnsignedValuesFromLongs(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         Properties properties = new Properties();

         properties.AbsoluteExpiryTime = uint.MaxValue;
         properties.CreationTime = (uint.MaxValue);
         properties.GroupSequence = (uint.MaxValue);

         encoder.WriteObject(buffer, encoderState, properties);

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
         Assert.IsTrue(result is Properties);

         Properties decoded = (Properties)result;

         Assert.AreEqual(uint.MaxValue, decoded.AbsoluteExpiryTime);
         Assert.AreEqual(uint.MaxValue, decoded.CreationTime);
         Assert.AreEqual(uint.MaxValue, decoded.GroupSequence);
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

         Properties properties = new Properties();
         properties.AbsoluteExpiryTime = 100;
         properties.ContentEncoding = "UTF8";

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, properties);
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Properties), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Properties), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Modified);
         Modified modified = (Modified)result;
         Assert.IsFalse(modified.UndeliverableHere);
         Assert.IsFalse(modified.DeliveryFailed);
      }

      [Test]
      public void TestDecodeWithInvalidMap32Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap8Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap32TypeFromStream()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestDecodeWithInvalidMap8TypeFromStream()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map8, true);
      }

      private void DoTestDecodeWithInvalidMapType(EncodingCodes mapType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Properties.DescriptorCode));
         if (mapType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));
            buffer.WriteInt((byte)0);  // Size
            buffer.WriteInt((byte)0);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map8));
            buffer.WriteUnsignedByte((byte)0);  // Size
            buffer.WriteUnsignedByte((byte)0);  // Count
         }

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("Should not decode type with invalid encoding");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("Should not decode type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestSkipValueWithInvalidMap32Type()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestSkipValueWithInvalidMap8Type()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestSkipValueWithInvalidMap32TypeFromStream()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestSkipValueWithInvalidMap8TypeFromStream()
      {
         DoTestSkipValueWithInvalidMapType(EncodingCodes.Map8, true);
      }

      private void DoTestSkipValueWithInvalidMapType(EncodingCodes mapType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Properties.DescriptorCode));
         if (mapType == EncodingCodes.Map32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));
            buffer.WriteInt((byte)0);  // Size
            buffer.WriteInt((byte)0);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Map8));
            buffer.WriteUnsignedByte((byte)0);  // Size
            buffer.WriteUnsignedByte((byte)0);  // Count
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Properties), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(stream, streamDecoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Properties), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestEncodeDecodeArray()
      {
         DoTestEncodeDecodeArray(false);
      }

      [Test]
      public void TestEncodeDecodeArrayFromStream()
      {
         DoTestEncodeDecodeArray(true);
      }

      private void DoTestEncodeDecodeArray(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Properties[] array = new Properties[3];

         array[0] = new Properties();
         array[1] = new Properties();
         array[2] = new Properties();

         array[0].AbsoluteExpiryTime = 1;
         array[1].AbsoluteExpiryTime = 2;
         array[2].AbsoluteExpiryTime = 3;

         encoder.WriteObject(buffer, encoderState, array);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result.GetType().IsArray);
         Assert.AreEqual(typeof(Properties), result.GetType().GetElementType());

         Properties[] resultArray = (Properties[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Properties);
            Assert.AreEqual(array[i].AbsoluteExpiryTime, resultArray[i].AbsoluteExpiryTime);
         }
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList8()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List8, false);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList32()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List32, false);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList8FromStream()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List8, true);
      }

      [Test]
      public void TestDecodeWithNotEnoughListEntriesList32FromStream()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List32, true);
      }

      private void DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Properties.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt(128);  // Size
            buffer.WriteInt(-1);  // Count, reads as negative as encoder treats these as signed ints.
         }
         else if (listType == EncodingCodes.List8)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)128);  // Size
            buffer.WriteUnsignedByte((byte)0xFF);  // Count
         }

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList8()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List8, false);
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList32()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List32, false);
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList8FromStream()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List8, true);
      }

      [Test]
      public void TestDecodeWithToManyListEntriesList32FromStream()
      {
         DoTestDecodeWithToManyListEntriesList32(EncodingCodes.List32, true);
      }

      private void DoTestDecodeWithToManyListEntriesList32(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Properties.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt(128);  // Size
            buffer.WriteInt(127);  // Count
         }
         else if (listType == EncodingCodes.List8)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)128);  // Size
            buffer.WriteUnsignedByte((byte)127);  // Count
         }

         if (fromStream)
         {
            try
            {
               streamDecoder.ReadObject(stream, streamDecoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
         }
         else
         {
            try
            {
               decoder.ReadObject(buffer, decoderState);
               Assert.Fail("Should not decode type with invalid min entries");
            }
            catch (DecodeException) { }
         }
      }
   }
}