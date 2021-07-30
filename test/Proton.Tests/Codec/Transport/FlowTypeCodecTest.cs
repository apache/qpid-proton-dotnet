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
using System;

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class FlowTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Flow), new FlowTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Flow), new FlowTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Flow.DescriptorCode, new FlowTypeDecoder().DescriptorCode);
         Assert.AreEqual(Flow.DescriptorCode, new FlowTypeEncoder().DescriptorCode);
         Assert.AreEqual(Flow.DescriptorSymbol, new FlowTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Flow.DescriptorSymbol, new FlowTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestCannotEncodeEmptyPerformative()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         Flow input = new Flow();

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
      public void TestEncodeAndDecode()
      {
         DoTestEncodeAndDecode(false);
      }

      [Test]
      public void TestEncodeAndDecodeFromStream()
      {
         DoTestEncodeAndDecode(true);
      }

      private void DoTestEncodeAndDecode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Random random = new Random(Environment.TickCount);

         uint randomeNextIncomingId = (uint)random.Next();
         uint randomeNextOutgoingId = (uint)random.Next();
         uint randomeNextIncomingWindow = (uint)random.Next();
         uint randomeNextOutgoingWindow = (uint)random.Next();
         uint randomeHandle = (uint)random.Next();
         uint randomLinkCredit = (uint)random.Next();
         uint randomeAvailable = (uint)random.Next();
         uint randomeDeliveryCount = (uint)random.Next();

         Flow input = new Flow();

         input.NextIncomingId = randomeNextIncomingId;
         input.IncomingWindow = randomeNextIncomingWindow;
         input.NextOutgoingId = randomeNextOutgoingId;
         input.OutgoingWindow = randomeNextOutgoingWindow;
         input.Handle = randomeHandle;
         input.DeliveryCount = randomeDeliveryCount;
         input.LinkCredit = randomLinkCredit;
         input.Available = randomeAvailable;
         input.Drain = true;
         input.Echo = true;

         encoder.WriteObject(buffer, encoderState, input);

         Flow result;
         if (fromStream)
         {
            result = (Flow)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Flow)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual(randomeNextIncomingId, result.NextIncomingId);
         Assert.AreEqual(randomeNextIncomingWindow, result.IncomingWindow);
         Assert.AreEqual(randomeNextOutgoingId, result.NextOutgoingId);
         Assert.AreEqual(randomeNextOutgoingWindow, result.OutgoingWindow);
         Assert.AreEqual(randomeHandle, result.Handle);
         Assert.AreEqual(randomeDeliveryCount, result.DeliveryCount);
         Assert.AreEqual(randomLinkCredit, result.LinkCredit);
         Assert.AreEqual(randomeAvailable, result.Available);
         Assert.IsTrue(input.Drain);
         Assert.IsTrue(input.Echo);
         Assert.IsNull(input.Properties);
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

         Flow flow = new Flow();

         flow.NextIncomingId = 1;
         flow.IncomingWindow = 2;
         flow.NextOutgoingId = 3;
         flow.OutgoingWindow = 4;
         flow.Handle = 10;
         flow.DeliveryCount = 5;
         flow.LinkCredit = 6;
         flow.Available = 7;
         flow.Drain = false;
         flow.Echo = false;

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, flow);
         }

         flow.NextIncomingId = 10;
         flow.IncomingWindow = 20;
         flow.NextOutgoingId = 30;
         flow.OutgoingWindow = 40;
         flow.Handle = uint.MaxValue;
         flow.DeliveryCount = 50;
         flow.LinkCredit = 60;
         flow.Available = 70;
         flow.Drain = true;
         flow.Echo = true;

         encoder.WriteObject(buffer, encoderState, flow);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Flow), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Flow), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Flow);

         Flow value = (Flow)result;
         Assert.AreEqual(10, value.NextIncomingId);
         Assert.AreEqual(20, value.IncomingWindow);
         Assert.AreEqual(30, value.NextOutgoingId);
         Assert.AreEqual(40, value.OutgoingWindow);
         Assert.AreEqual(uint.MaxValue, value.Handle);
         Assert.AreEqual(50, value.DeliveryCount);
         Assert.AreEqual(60, value.LinkCredit);
         Assert.AreEqual(70, value.Available);
         Assert.IsTrue(value.Drain);
         Assert.IsTrue(value.Echo);
         Assert.IsNull(value.Properties);
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
         buffer.WriteUnsignedByte(((byte)Flow.DescriptorCode));
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
            Assert.AreEqual(typeof(Flow), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Flow), typeDecoder.DecodesType);

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
         buffer.WriteUnsignedByte(((byte)Flow.DescriptorCode));
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

         Flow[] array = new Flow[3];

         array[0] = new Flow();
         array[1] = new Flow();
         array[2] = new Flow();

         array[0].Handle = 0;
         array[0].LinkCredit = 0;
         array[0].DeliveryCount = 1;
         array[0].IncomingWindow = 1024;
         array[0].NextOutgoingId = 1;
         array[0].OutgoingWindow = 128;
         array[1].Handle = 1;
         array[1].LinkCredit = 1;
         array[1].DeliveryCount = 1;
         array[1].IncomingWindow = 2048;
         array[1].NextOutgoingId = 2;
         array[1].OutgoingWindow = 256;
         array[2].Handle = 2;
         array[2].LinkCredit = 2;
         array[2].DeliveryCount = 1;
         array[2].IncomingWindow = 4096;
         array[2].NextOutgoingId = 3;
         array[2].OutgoingWindow = 512;

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
         Assert.AreEqual(typeof(Flow), result.GetType().GetElementType());

         Flow[] resultArray = (Flow[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Flow);
            Assert.AreEqual(array[i].Handle, resultArray[i].Handle);
            Assert.AreEqual(array[i].LinkCredit, resultArray[i].LinkCredit);
            Assert.AreEqual(array[i].DeliveryCount, resultArray[i].DeliveryCount);
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
         buffer.WriteUnsignedByte(((byte)Flow.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)Flow.DescriptorCode));
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