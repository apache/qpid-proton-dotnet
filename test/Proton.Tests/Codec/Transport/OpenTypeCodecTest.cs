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
   public class OpenTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Open), new OpenTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Open), new OpenTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Open.DescriptorCode, new OpenTypeDecoder().DescriptorCode);
         Assert.AreEqual(Open.DescriptorCode, new OpenTypeEncoder().DescriptorCode);
         Assert.AreEqual(Open.DescriptorSymbol, new OpenTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Open.DescriptorSymbol, new OpenTypeEncoder().DescriptorSymbol);
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

         Symbol[] offeredCapabilities = new Symbol[] { Symbol.Lookup("Cap-1"), Symbol.Lookup("Cap-2") };
         Symbol[] desiredCapabilities = new Symbol[] { Symbol.Lookup("Cap-3"), Symbol.Lookup("Cap-4") };

         Random random = new Random(Environment.TickCount);

         ushort randomChannelMax = (ushort)random.Next(65535);
         uint randomMaxFrameSize = (uint)random.Next();
         uint randomIdleTimeout = (uint)random.Next();

         Open input = new Open();

         input.ContainerId = "test";
         input.Hostname = "localhost";
         input.ChannelMax = randomChannelMax;
         input.MaxFrameSize = randomMaxFrameSize;
         input.IdleTimeout = randomIdleTimeout;
         input.OfferedCapabilities = offeredCapabilities;
         input.DesiredCapabilities = desiredCapabilities;

         encoder.WriteObject(buffer, encoderState, input);

         Open result;
         if (fromStream)
         {
            result = (Open)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Open)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual("test", result.ContainerId);
         Assert.AreEqual("localhost", result.Hostname);
         Assert.AreEqual(randomChannelMax, result.ChannelMax);
         Assert.AreEqual(randomMaxFrameSize, result.MaxFrameSize);
         Assert.AreEqual(randomIdleTimeout, result.IdleTimeout);
         Assert.AreEqual(offeredCapabilities, result.OfferedCapabilities);
         Assert.AreEqual(desiredCapabilities, result.DesiredCapabilities);
      }

      [Test]
      public void TestOpenEncodesDefaultMaxFrameSizeWhenSet()
      {
         DoTestOpenEncodesDefaultMaxFrameSizeWhenSet(false);
      }

      [Test]
      public void TestOpenEncodesDefaultMaxFrameSizeWhenSetFromStream()
      {
         DoTestOpenEncodesDefaultMaxFrameSizeWhenSet(true);
      }

      private void DoTestOpenEncodesDefaultMaxFrameSizeWhenSet(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Open input = new Open();

         encoder.WriteObject(buffer, encoderState, input);

         Open resultWithDefault = (Open)decoder.ReadObject(buffer, decoderState);

         Assert.IsFalse(resultWithDefault.HasMaxFrameSize());
         Assert.AreEqual(uint.MaxValue, resultWithDefault.MaxFrameSize);

         input.MaxFrameSize = uint.MaxValue;

         encoder.WriteObject(buffer, encoderState, input);

         Open result;
         if (fromStream)
         {
            result = (Open)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Open)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result.HasMaxFrameSize());
         Assert.AreEqual(uint.MaxValue, result.MaxFrameSize);
      }

      [Test]
      public void TestOpenEncodesDefaultIdleTimeoutWhenSet()
      {
         DoTestOpenEncodesDefaultIdleTimeoutWhenSet(false);
      }

      [Test]
      public void TestOpenEncodesDefaultIdleTimeoutWhenSetFromStream()
      {
         DoTestOpenEncodesDefaultIdleTimeoutWhenSet(true);
      }

      private void DoTestOpenEncodesDefaultIdleTimeoutWhenSet(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Open input = new Open();

         encoder.WriteObject(buffer, encoderState, input);

         Open resultWithDefault = (Open)decoder.ReadObject(buffer, decoderState);

         Assert.IsFalse(resultWithDefault.HasIdleTimeout());
         Assert.AreEqual(0u, resultWithDefault.IdleTimeout);

         input.IdleTimeout = 0u;

         encoder.WriteObject(buffer, encoderState, input);

         Open result;
         if (fromStream)
         {
            result = (Open)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Open)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result.HasIdleTimeout());
         Assert.AreEqual(0u, result.IdleTimeout);
      }

      [Test]
      public void TestOpenEncodesDefaultChannelMaxWhenSet()
      {
         DoTestOpenEncodesDefaultChannelMaxWhenSet(false);
      }

      [Test]
      public void TestOpenEncodesDefaultChannelMaxWhenSetFromStream()
      {
         DoTestOpenEncodesDefaultChannelMaxWhenSet(true);
      }

      private void DoTestOpenEncodesDefaultChannelMaxWhenSet(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Open input = new Open();

         encoder.WriteObject(buffer, encoderState, input);

         Open resultWithDefault = (Open)decoder.ReadObject(buffer, decoderState);

         Assert.IsFalse(resultWithDefault.HasChannelMax());
         Assert.AreEqual(ushort.MaxValue, resultWithDefault.ChannelMax);

         input.ChannelMax = ushort.MaxValue;

         encoder.WriteObject(buffer, encoderState, input);

         Open result;
         if (fromStream)
         {
            result = (Open)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Open)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result.HasChannelMax());
         Assert.AreEqual(ushort.MaxValue, result.ChannelMax);
      }

      [Test]
      public void TestEncodeAndDecodeOpenWithMaxMaxFrameSize()
      {
         DoTestEncodeAndDecodeOpenWithMaxMaxFrameSize(false);
      }

      [Test]
      public void TestEncodeAndDecodeOpenWithMaxMaxFrameSizeFromStream()
      {
         DoTestEncodeAndDecodeOpenWithMaxMaxFrameSize(true);
      }

      private void DoTestEncodeAndDecodeOpenWithMaxMaxFrameSize(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Open input = new Open();

         input.ContainerId = "test";
         input.Hostname = "localhost";
         input.ChannelMax = ushort.MaxValue;
         input.MaxFrameSize = uint.MaxValue;

         encoder.WriteObject(buffer, encoderState, input);

         Open result;
         if (fromStream)
         {
            result = (Open)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Open)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual("test", result.ContainerId);
         Assert.AreEqual("localhost", result.Hostname);
         Assert.AreEqual(ushort.MaxValue, result.ChannelMax);
         Assert.AreEqual(uint.MaxValue, result.MaxFrameSize);
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

      private void DoTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Open close = new Open();

         close.ContainerId = "skip";
         close.Hostname = "google";
         close.ChannelMax = 256;
         close.MaxFrameSize = 0u;
         close.IdleTimeout = 1u;

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, close);
         }

         close.ContainerId = "test";
         close.Hostname = "localhost";
         close.ChannelMax = 512;
         close.MaxFrameSize = 1u;
         close.IdleTimeout = 0u;

         encoder.WriteObject(buffer, encoderState, close);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Open), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Open), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
            }
         }

         Open result;
         if (fromStream)
         {
            result = (Open)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Open)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.IsTrue(result is Open);

         Open value = result;
         Assert.AreEqual("test", value.ContainerId);
         Assert.AreEqual("localhost", value.Hostname);
         Assert.AreEqual(512, value.ChannelMax);
         Assert.AreEqual(1u, value.MaxFrameSize);
         Assert.AreEqual(0u, value.IdleTimeout);
         Assert.IsNull(value.OfferedCapabilities);
         Assert.IsNull(value.DesiredCapabilities);
      }

      [Test]
      public void TestTostringWhenEmptyNoNPE()
      {
         Open open = new Open();
         Assert.IsNotNull(open.ToString());
      }

      [Test]
      public void TestPerformativeType()
      {
         Open open = new Open();
         Assert.AreEqual(PerformativeType.Open, open.Type);
      }

      [Test]
      public void TestIsEmpty()
      {
         Open open = new Open();

         // Open defaults to an empty string container ID so not empty
         Assert.IsFalse(open.IsEmpty());
         open.MaxFrameSize = 1024;
         Assert.IsFalse(open.IsEmpty());
      }

      [Test]
      public void TestContainerId()
      {
         Open open = new Open();

         // Open defaults to an empty string container ID
         Assert.IsTrue(open.HasContainerId());
         Assert.AreEqual("", open.ContainerId);
         open.ContainerId = "test";
         Assert.IsTrue(open.HasContainerId());
         Assert.AreEqual("test", open.ContainerId);

         try
         {
            open.ContainerId = null;
            Assert.Fail("Should not be able to set a null container id for mandatory field.");
         }
         catch (NullReferenceException)
         {
         }
      }

      [Test]
      public void TestHostname()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasHostname());
         Assert.IsNull(open.Hostname);
         open.Hostname = "localhost";
         Assert.IsTrue(open.HasHostname());
         Assert.AreEqual("localhost", open.Hostname);
         open.Hostname = null;
         Assert.IsFalse(open.HasHostname());
         Assert.IsNull(open.Hostname);
      }

      [Test]
      public void TestMaxFrameSize()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasMaxFrameSize());
         Assert.AreEqual(uint.MaxValue, open.MaxFrameSize);
         open.MaxFrameSize = 1024;
         Assert.IsTrue(open.HasMaxFrameSize());
         Assert.AreEqual(1024, open.MaxFrameSize);
      }

      [Test]
      public void TestChannelMax()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasChannelMax());
         Assert.AreEqual(ushort.MaxValue, open.ChannelMax);
         open.ChannelMax = 1024;
         Assert.IsTrue(open.HasChannelMax());
         Assert.AreEqual(1024, open.ChannelMax);
      }

      [Test]
      public void TestIdleTimeout()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasIdleTimeout());
         Assert.AreEqual(0, open.IdleTimeout);
         open.IdleTimeout = 1024;
         Assert.IsTrue(open.HasIdleTimeout());
         Assert.AreEqual(1024, open.IdleTimeout);
      }

      [Test]
      public void TestOutgoingLocales()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasOutgoingLocales());
         Assert.IsNull(open.OutgoingLocales);
         open.OutgoingLocales = new Symbol[] { Symbol.Lookup("test") };
         Assert.IsTrue(open.HasOutgoingLocales());
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("test") }, open.OutgoingLocales);
         open.OutgoingLocales = (Symbol[])null;
         Assert.IsFalse(open.HasDesiredCapabilities());
         Assert.IsNull(open.DesiredCapabilities);
      }

      [Test]
      public void TestIncomingLocales()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasIncomingLocales());
         Assert.IsNull(open.IncomingLocales);
         open.IncomingLocales = new Symbol[] { Symbol.Lookup("test") };
         Assert.IsTrue(open.HasIncomingLocales());
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("test") }, open.IncomingLocales);
         open.IncomingLocales = (Symbol[])null;
         Assert.IsFalse(open.HasDesiredCapabilities());
         Assert.IsNull(open.DesiredCapabilities);
      }

      [Test]
      public void TestOfferedCapabilities()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasOfferedCapabilities());
         Assert.IsNull(open.OfferedCapabilities);
         open.OfferedCapabilities = new Symbol[] { Symbol.Lookup("test") };
         Assert.IsTrue(open.HasOfferedCapabilities());
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("test") }, open.OfferedCapabilities);
         open.OfferedCapabilities = (Symbol[])null;
         Assert.IsFalse(open.HasDesiredCapabilities());
         Assert.IsNull(open.DesiredCapabilities);
      }

      [Test]
      public void TestDesiredCapabilities()
      {
         Open open = new Open();

         Assert.IsFalse(open.HasDesiredCapabilities());
         Assert.IsNull(open.DesiredCapabilities);
         open.DesiredCapabilities = new Symbol[] { Symbol.Lookup("test") };
         Assert.IsTrue(open.HasDesiredCapabilities());
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("test") }, open.DesiredCapabilities);
         open.DesiredCapabilities = (Symbol[])null;
         Assert.IsFalse(open.HasDesiredCapabilities());
         Assert.IsNull(open.DesiredCapabilities);
      }

      [Test]
      public void TestProperties()
      {
         Open open = new Open();

         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), false);

         Assert.IsFalse(open.HasProperties());
         Assert.IsNull(open.Properties);
         open.Properties = properties;
         Assert.IsTrue(open.HasProperties());
         Assert.AreEqual(properties, open.Properties);
         open.Properties = null;
         Assert.IsFalse(open.HasProperties());
         Assert.IsNull(open.Properties);
      }

      [Test]
      public void TestCopy()
      {
         IDictionary<Symbol, object> properties = new Dictionary<Symbol, object>();
         properties.Add(Symbol.Lookup("test"), false);

         Symbol outgoingLocale = Symbol.Lookup("outgoing");
         Symbol incomingLocale = Symbol.Lookup("incoming");
         Symbol offeredCapability = Symbol.Lookup("offered");
         Symbol desiredCapability = Symbol.Lookup("desired");

         Open open = new Open();

         open.ContainerId = "test";
         open.Hostname = "localhost";
         open.MaxFrameSize = 1024;
         open.ChannelMax = 64;
         open.IdleTimeout = 360000;
         open.OutgoingLocales = new Symbol[] { outgoingLocale };
         open.IncomingLocales = new Symbol[] { incomingLocale };
         open.OfferedCapabilities = new Symbol[] { offeredCapability };
         open.DesiredCapabilities = new Symbol[] { desiredCapability };
         open.Properties = properties;

         Open copy = (Open)open.Clone();

         Assert.IsNotNull(copy);

         Assert.AreEqual("test", open.ContainerId);
         Assert.AreEqual("localhost", open.Hostname);
         Assert.AreEqual(1024, open.MaxFrameSize);
         Assert.AreEqual(64, open.ChannelMax);
         Assert.AreEqual(360000, open.IdleTimeout);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("outgoing") }, open.OutgoingLocales);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("incoming") }, open.IncomingLocales);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("offered") }, open.OfferedCapabilities);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("desired") }, open.DesiredCapabilities);
         Assert.AreEqual(properties, open.Properties);

         open.OutgoingLocales = (Symbol[])null;
         open.IncomingLocales = (Symbol[])null;
         open.OfferedCapabilities = (Symbol[])null;
         open.DesiredCapabilities = (Symbol[])null;
         open.Properties = null;

         copy = (Open)open.Clone();

         Assert.IsNotNull(copy);

         Assert.AreEqual("test", open.ContainerId);
         Assert.AreEqual("localhost", open.Hostname);
         Assert.AreEqual(1024, open.MaxFrameSize);
         Assert.AreEqual(64, open.ChannelMax);
         Assert.AreEqual(360000, open.IdleTimeout);
         Assert.IsNull(open.OutgoingLocales);
         Assert.IsNull(open.IncomingLocales);
         Assert.IsNull(open.OfferedCapabilities);
         Assert.IsNull(open.DesiredCapabilities);
         Assert.IsNull(open.Properties);
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
         buffer.WriteUnsignedByte(((byte)Open.DescriptorCode));
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
            Assert.AreEqual(typeof(Open), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Open), typeDecoder.DecodesType);

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
         buffer.WriteUnsignedByte(((byte)Open.DescriptorCode));
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

         Open[] array = new Open[3];

         array[0] = new Open();
         array[1] = new Open();
         array[2] = new Open();

         array[0].Hostname = "1";
         array[0].IdleTimeout = 1;
         array[0].MaxFrameSize = 1;
         array[1].Hostname = "2";
         array[1].IdleTimeout = 2;
         array[1].MaxFrameSize = 2;
         array[2].Hostname = "3";
         array[2].IdleTimeout = 3;
         array[2].MaxFrameSize = 3;

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
         Assert.AreEqual(typeof(Open), result.GetType().GetElementType());

         Open[] resultArray = (Open[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Open);
            Assert.AreEqual(array[i].Hostname, resultArray[i].Hostname);
            Assert.AreEqual(array[i].IdleTimeout, resultArray[i].IdleTimeout);
            Assert.AreEqual(array[i].MaxFrameSize, resultArray[i].MaxFrameSize);
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
         buffer.WriteUnsignedByte(((byte)Open.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)Open.DescriptorCode));
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