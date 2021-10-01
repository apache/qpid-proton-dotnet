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
using Apache.Qpid.Proton.Types;
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Codec.Messaging
{
   [TestFixture]
   public class SourceTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Source), new SourceTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Source), new SourceTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Source.DescriptorCode, new SourceTypeDecoder().DescriptorCode);
         Assert.AreEqual(Source.DescriptorCode, new SourceTypeEncoder().DescriptorCode);
         Assert.AreEqual(Source.DescriptorSymbol, new SourceTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Source.DescriptorSymbol, new SourceTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestEncodeDecodeSourceType()
      {
         DoTestEncodeDecodeSourceType(false);
      }

      [Test]
      public void TestEncodeDecodeSourceTypeFromStream()
      {
         DoTestEncodeDecodeSourceType(true);
      }

      private void DoTestEncodeDecodeSourceType(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Source value = new Source();
         value.Address = "test";
         value.Durable = TerminusDurability.UnsettledState;
         value.Timeout = uint.MaxValue;

         encoder.WriteObject(buffer, encoderState, value);

         Source result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject<Source>(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject<Source>(buffer, decoderState);
         }

         Assert.AreEqual("test", result.Address);
         Assert.AreEqual(TerminusDurability.UnsettledState, result.Durable);
         Assert.AreEqual(uint.MaxValue, result.Timeout);
      }

      [Test]
      public void TestFullyPopulatedSource()
      {
         DoTestFullyPopulatedSource(false);
      }

      [Test]
      public void TestFullyPopulatedSourceFromStream()
      {
         DoTestFullyPopulatedSource(true);
      }

      private void DoTestFullyPopulatedSource(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<Symbol, object> nodeProperties = new Dictionary<Symbol, object>();
         nodeProperties.Add(Symbol.Lookup("property-1"), "value-1");
         nodeProperties.Add(Symbol.Lookup("property-2"), "value-2");
         nodeProperties.Add(Symbol.Lookup("property-3"), "value-3");

         IDictionary<Symbol, object> filters = new Dictionary<Symbol, object>();
         nodeProperties.Add(Symbol.Lookup("filter-1"), "value-1");
         nodeProperties.Add(Symbol.Lookup("filter-2"), "value-2");
         nodeProperties.Add(Symbol.Lookup("filter-3"), "value-3");

         Source value = new Source();
         value.Address = "test";
         value.Durable = TerminusDurability.UnsettledState;
         value.ExpiryPolicy = TerminusExpiryPolicy.SessionEnd;
         value.Timeout = 255u;
         value.Dynamic = true;
         value.DynamicNodeProperties = nodeProperties;
         value.DistributionMode = Symbol.Lookup("mode");
         value.Filter = filters;
         value.DefaultOutcome = Released.Instance;
         value.Outcomes = new Symbol[] { Symbol.Lookup("ACCEPTED"), Symbol.Lookup("REJECTED") };
         value.Capabilities = new Symbol[] { Symbol.Lookup("RELEASED"), Symbol.Lookup("MODIFIED") };

         encoder.WriteObject(buffer, encoderState, value);

         Source result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject<Source>(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject<Source>(buffer, decoderState);
         }

         Assert.AreEqual("test", result.Address);
         Assert.AreEqual(TerminusDurability.UnsettledState, result.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, result.ExpiryPolicy);
         Assert.AreEqual(255, result.Timeout);
         Assert.AreEqual(true, result.Dynamic);
         Assert.AreEqual(nodeProperties, result.DynamicNodeProperties);
         Assert.AreEqual(Symbol.Lookup("mode"), result.DistributionMode);
         Assert.AreEqual(filters, result.Filter);
         Assert.AreEqual(Released.Instance, result.DefaultOutcome);

         Assert.AreEqual(new Symbol[] { Symbol.Lookup("ACCEPTED"), Symbol.Lookup("REJECTED") }, result.Outcomes);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("RELEASED"), Symbol.Lookup("MODIFIED") }, result.Capabilities);
      }

      [Test]
      public void TestSourceWithNoDefaultOutcome()
      {
         DoTestSourceWithNoDefaultOutcome(false);
      }

      [Test]
      public void TestSourceWithNoDefaultOutcomeFromStream()
      {
         DoTestSourceWithNoDefaultOutcome(true);
      }

      private void DoTestSourceWithNoDefaultOutcome(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<Symbol, object> nodeProperties = new Dictionary<Symbol, object>();
         nodeProperties.Add(Symbol.Lookup("property-1"), "value-1");
         nodeProperties.Add(Symbol.Lookup("property-2"), "value-2");
         nodeProperties.Add(Symbol.Lookup("property-3"), "value-3");

         IDictionary<Symbol, object> filters = new Dictionary<Symbol, object>();
         nodeProperties.Add(Symbol.Lookup("filter-1"), "value-1");
         nodeProperties.Add(Symbol.Lookup("filter-2"), "value-2");
         nodeProperties.Add(Symbol.Lookup("filter-3"), "value-3");

         Source value = new Source();
         value.Address = "test";
         value.Durable = TerminusDurability.UnsettledState;
         value.ExpiryPolicy = TerminusExpiryPolicy.SessionEnd;
         value.Timeout = 255u;
         value.Dynamic = true;
         value.DynamicNodeProperties = nodeProperties;
         value.DistributionMode = Symbol.Lookup("mode");
         value.Filter = filters;
         value.Outcomes = new Symbol[] { Symbol.Lookup("ACCEPTED"), Symbol.Lookup("REJECTED") };
         value.Capabilities = new Symbol[] { Symbol.Lookup("RELEASED"), Symbol.Lookup("MODIFIED") };

         encoder.WriteObject(buffer, encoderState, value);

         Source result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject<Source>(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject<Source>(buffer, decoderState);
         }

         Assert.AreEqual("test", result.Address);
         Assert.AreEqual(TerminusDurability.UnsettledState, result.Durable);
         Assert.AreEqual(TerminusExpiryPolicy.SessionEnd, result.ExpiryPolicy);
         Assert.AreEqual(255, result.Timeout);
         Assert.AreEqual(true, result.Dynamic);
         Assert.AreEqual(nodeProperties, result.DynamicNodeProperties);
         Assert.AreEqual(Symbol.Lookup("mode"), result.DistributionMode);
         Assert.AreEqual(filters, result.Filter);
         Assert.IsNull(result.DefaultOutcome);

         Assert.AreEqual(new Symbol[] { Symbol.Lookup("ACCEPTED"), Symbol.Lookup("REJECTED") }, result.Outcomes);
         Assert.AreEqual(new Symbol[] { Symbol.Lookup("RELEASED"), Symbol.Lookup("MODIFIED") }, result.Capabilities);
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

         Source source = new Source();
         source.Address = "address";
         source.Capabilities = new Symbol[] { Symbol.Lookup("QUEUE") };

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, source);
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Source), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Source), typeDecoder.DecodesType);
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

      private void DoTestDecodeWithInvalidMapType(EncodingCodes mapType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Source.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)Source.DescriptorCode));
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
            Assert.AreEqual(typeof(Source), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Source), typeDecoder.DecodesType);

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

         Source[] array = new Source[3];

         array[0] = new Source();
         array[1] = new Source();
         array[2] = new Source();

         array[0].Address = "test-1";
         array[0].Dynamic = true;
         array[0].DefaultOutcome = Accepted.Instance;
         array[0].Address = "test-2";
         array[0].Dynamic = false;
         array[0].DefaultOutcome = Released.Instance;
         array[0].Address = "test-3";
         array[0].Dynamic = true;
         array[0].DefaultOutcome = Accepted.Instance;

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
         Assert.AreEqual(typeof(Source), result.GetType().GetElementType());

         Source[] resultArray = (Source[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Source);
            Assert.AreEqual(array[i].Address, resultArray[i].Address);
            Assert.AreEqual(array[i].Dynamic, resultArray[i].Dynamic);
            Assert.AreEqual(array[i].DefaultOutcome, resultArray[i].DefaultOutcome);
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
         buffer.WriteUnsignedByte(((byte)Source.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)Source.DescriptorCode));
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