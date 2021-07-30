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
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Codec.Decoders.Transport;
using Apache.Qpid.Proton.Codec.Encoders.Transport;
using Apache.Qpid.Proton.Types;
using System;
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class BeginTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Begin), new BeginTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Begin), new BeginTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Begin.DescriptorCode, new BeginTypeDecoder().DescriptorCode);
         Assert.AreEqual(Begin.DescriptorCode, new BeginTypeEncoder().DescriptorCode);
         Assert.AreEqual(Begin.DescriptorSymbol, new BeginTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Begin.DescriptorSymbol, new BeginTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestCannotEncodeEmptyPerformative()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         Begin input = new Begin();

         try
         {
            encoder.WriteObject(buffer, encoderState, input);
            Assert.Fail("Cannot omit required fields.");
         }
         catch (EncodeException)
         {
         }
      }

      [Test]
      public void TestEncodeDecodeType()
      {
         DoTestEncodeAndDecode(false);
      }

      [Test]
      public void TestEncodeDecodeTypeFromStream()
      {
         DoTestEncodeAndDecode(true);
      }

      private void DoTestEncodeAndDecode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Symbol[] offeredCapabilities = new Symbol[] { Symbol.Lookup("Cap-1"), Symbol.Lookup("Cap-2") };
         Symbol[] desiredCapabilities = new Symbol[] { Symbol.Lookup("Cap-3"), Symbol.Lookup("Cap-4") };
         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("property"), "value");

         Random random = new Random(Environment.TickCount);

         ushort randomChannel = (ushort)random.Next(65535);
         uint randomeNextOutgoingId = (uint)random.Next();
         uint randomeNextIncomingWindow = (uint)random.Next();
         uint randomeNextOutgoingWindow = (uint)random.Next();
         uint randomeHandleMax = (uint)random.Next();

         Begin input = new Begin();

         input.RemoteChannel = randomChannel;
         input.NextOutgoingId = randomeNextOutgoingId;
         input.IncomingWindow = randomeNextIncomingWindow;
         input.OutgoingWindow = randomeNextOutgoingWindow;
         input.HandleMax = randomeHandleMax;
         input.OfferedCapabilities = offeredCapabilities;
         input.DesiredCapabilities = desiredCapabilities;
         input.Properties = properties;

         encoder.WriteObject(buffer, encoderState, input);

         Begin result;
         if (fromStream)
         {
            result = (Begin)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Begin)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(randomChannel, result.RemoteChannel);
         Assert.AreEqual(randomeNextOutgoingId, result.NextOutgoingId);
         Assert.AreEqual(randomeNextIncomingWindow, result.IncomingWindow);
         Assert.AreEqual(randomeNextOutgoingWindow, result.OutgoingWindow);
         Assert.AreEqual(randomeHandleMax, result.HandleMax);
         Assert.IsNotNull(result.Properties);
         Assert.AreEqual(1, properties.Count);
         Assert.IsTrue(properties.ContainsKey(Symbol.Lookup("property")));
         Assert.AreEqual(offeredCapabilities, result.OfferedCapabilities);
         Assert.AreEqual(desiredCapabilities, result.DesiredCapabilities);
      }

      [Test]
      public void TestEncodeDecodeFailsOnMissingIncomingWindow()
      {
         TestEncodeDecodeFailsOnMissingIncomingWindow(false);
      }

      [Test]
      public void TestEncodeDecodeFailsOnMissingIncomingWindowFromStream()
      {
         TestEncodeDecodeFailsOnMissingIncomingWindow(true);
      }

      private void TestEncodeDecodeFailsOnMissingIncomingWindow(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Random random = new Random(Environment.TickCount);

         ushort randomChannel = (ushort)random.Next(65535);
         uint randomeNextOutgoingId = (uint)random.Next();
         uint randomeNextOutgoingWindow = (uint)random.Next();
         uint randomeHandleMax = (uint)random.Next();

         Begin input = new Begin();

         input.RemoteChannel = randomChannel;
         input.NextOutgoingId = randomeNextOutgoingId;
         input.OutgoingWindow = randomeNextOutgoingWindow;
         input.HandleMax = randomeHandleMax;

         encoder.WriteObject(buffer, encoderState, input);

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
      public void TestSkipValue()
      {
         TestSkipValue(false);
      }

      [Test]
      public void TestSkipValueFromStream()
      {
         TestSkipValue(true);
      }

      private void TestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Begin begin = new Begin();

         begin.RemoteChannel = 1;
         begin.NextOutgoingId = 0;
         begin.IncomingWindow = 1024;
         begin.OutgoingWindow = 1024;
         begin.HandleMax = 25;

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, begin);
         }

         begin.RemoteChannel = 2;
         begin.NextOutgoingId = 0;
         begin.IncomingWindow = 1024;
         begin.OutgoingWindow = 1024;
         begin.HandleMax = 50;

         encoder.WriteObject(buffer, encoderState, begin);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Begin), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Begin), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Begin);

         Begin value = (Begin)result;
         Assert.AreEqual(2, value.RemoteChannel);
         Assert.AreEqual(50, value.HandleMax);
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
         buffer.WriteUnsignedByte(((byte)Begin.DescriptorCode));
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
            Assert.AreEqual(typeof(Begin), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Begin), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestDecodedWithInvalidMap32Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap8Type()
      {
         DoTestDecodeWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodedWithInvalidMap32TypeFromStream()
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
         buffer.WriteUnsignedByte(((byte)Begin.DescriptorCode));
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
      public void TestEncodeDecodeArray()
      {
         TestEncodeDecodeArray(false);
      }

      [Test]
      public void TestEncodeDecodeArrayFromStream()
      {
         TestEncodeDecodeArray(true);
      }

      private void TestEncodeDecodeArray(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Begin[] array = new Begin[3];

         array[0] = new Begin();
         array[1] = new Begin();
         array[2] = new Begin();

         array[0].NextOutgoingId = 0;
         array[0].RemoteChannel = 0;
         array[0].IncomingWindow = 0;
         array[0].OutgoingWindow = 0;
         array[1].NextOutgoingId = 1;
         array[1].RemoteChannel = 1;
         array[1].IncomingWindow = 1;
         array[1].OutgoingWindow = 1;
         array[2].NextOutgoingId = 2;
         array[2].RemoteChannel = 2;
         array[2].IncomingWindow = 2;
         array[2].OutgoingWindow = 2;

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
         Assert.AreEqual(typeof(Begin), result.GetType().GetElementType());

         Begin[] resultArray = (Begin[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Begin);
            Assert.AreEqual(array[i].NextOutgoingId, resultArray[i].NextOutgoingId);
            Assert.AreEqual(array[i].OutgoingWindow, resultArray[i].OutgoingWindow);
            Assert.AreEqual(array[i].IncomingWindow, resultArray[i].IncomingWindow);
            Assert.AreEqual(array[i].RemoteChannel, resultArray[i].RemoteChannel);
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
      public void TestDecodeWithNotEnoughListEntriesList0FromStream()
      {
         DoTestDecodeWithNotEnoughListEntriesList32(EncodingCodes.List0, true);
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
         buffer.WriteUnsignedByte(((byte)Begin.DescriptorCode));
         if (listType == EncodingCodes.List32)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            buffer.WriteInt((byte)0);  // Size
            buffer.WriteInt((byte)0);  // Count
         }
         else if (listType == EncodingCodes.List8)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List8));
            buffer.WriteUnsignedByte((byte)0);  // Size
            buffer.WriteUnsignedByte((byte)0);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List0));
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
         buffer.WriteUnsignedByte(((byte)Begin.DescriptorCode));
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