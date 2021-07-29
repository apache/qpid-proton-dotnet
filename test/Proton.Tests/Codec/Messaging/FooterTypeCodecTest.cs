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
using System;

namespace Apache.Qpid.Proton.Codec.Messaging
{
   [TestFixture]
   public class FooterTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Footer), new FooterTypeEncoder().EncodesType);
         Assert.AreEqual(typeof(Footer), new FooterTypeDecoder().DecodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Footer.DescriptorCode, new FooterTypeEncoder().DescriptorCode);
         Assert.AreEqual(Footer.DescriptorCode, new FooterTypeDecoder().DescriptorCode);
         Assert.AreEqual(Footer.DescriptorSymbol, new FooterTypeEncoder().DescriptorSymbol);
         Assert.AreEqual(Footer.DescriptorSymbol, new FooterTypeDecoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeSmallSeriesOfFooter()
      {
         DoTestDecodeHeaderSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfFooter()
      {
         DoTestDecodeHeaderSeries(LargeSize, false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfFooterFromStream()
      {
         DoTestDecodeHeaderSeries(SmallSize, false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfFooterFromStream()
      {
         DoTestDecodeHeaderSeries(LargeSize, false);
      }

      private void DoTestDecodeHeaderSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<Symbol, object> propertiesMap = new Dictionary<Symbol, object>();
         Footer properties = new Footer(propertiesMap);

         propertiesMap.Add(Symbol.Lookup("key-1"), "1");
         propertiesMap.Add(Symbol.Lookup("key-2"), "2");
         propertiesMap.Add(Symbol.Lookup("key-3"), "3");
         propertiesMap.Add(Symbol.Lookup("key-4"), "4");
         propertiesMap.Add(Symbol.Lookup("key-5"), "5");
         propertiesMap.Add(Symbol.Lookup("key-6"), "6");
         propertiesMap.Add(Symbol.Lookup("key-7"), "7");
         propertiesMap.Add(Symbol.Lookup("key-8"), "8");

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
            Assert.IsTrue(result is Footer);

            Footer decoded = (Footer)result;

            Assert.AreEqual(8, decoded.Value.Count);
            Assert.AreEqual(decoded.Value, propertiesMap);
         }
      }

      [Test]
      public void TestDecodeFailsWhenDescriedValueIsNotMapType()
      {
         DoTestDecodeFailsWhenDescriedValueIsNotMapType(false);
      }

      [Test]
      public void TestDecodeFailsWhenDescriedValueIsNotMapTypeFromStream()
      {
         DoTestDecodeFailsWhenDescriedValueIsNotMapType(true);
      }

      private void DoTestDecodeFailsWhenDescriedValueIsNotMapType(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Footer.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
         buffer.WriteInt(0);
         buffer.WriteInt(0);

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
      public void TestDecodeWithNullBodyUsingDescriptorCode()
      {
         DoTestDecodeWithNullBodyUsingDescriptorCode(false);
      }

      [Test]
      public void TestDecodeWithNullBodyUsingDescriptorCodeFromStream()
      {
         DoTestDecodeWithNullBodyUsingDescriptorCode(true);
      }

      private void DoTestDecodeWithNullBodyUsingDescriptorCode(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Footer.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Footer result;
         if (fromStream)
         {
            result = (Footer)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Footer)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNull(result.Value);
      }

      [Test]
      public void TestDecodeWithNullBodyUsingDescriptorSymbol()
      {
         TestDecodeWithNullBodyUsingDescriptorSymbol(false);
      }

      [Test]
      public void TestDecodeWithNullBodyUsingDescriptorSymbolFromStream()
      {
         TestDecodeWithNullBodyUsingDescriptorSymbol(true);
      }

      private void TestDecodeWithNullBodyUsingDescriptorSymbol(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Sym8));
         buffer.WriteUnsignedByte((byte)Footer.DescriptorSymbol.Length);
         Footer.DescriptorSymbol.WriteTo(buffer);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Footer result;
         if (fromStream)
         {
            result = (Footer)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Footer)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNull(result.Value);
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

         IDictionary<Symbol, object> map = new Dictionary<Symbol, object>();
         map.Add(Symbol.Lookup("one"), 1);
         map.Add(Symbol.Lookup("two"), true);
         map.Add(Symbol.Lookup("three"), "test");

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, new Footer(map));
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Footer), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Footer), typeDecoder.DecodesType);
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
      public void TestEncodeDecodeMessageAnnotationsWithEmptyValue()
      {
         DoTestEncodeDecodeMessageAnnotationsWithEmptyValue(false);
      }

      [Test]
      public void TestEncodeDecodeMessageAnnotationsWithEmptyValueFromStream()
      {
         DoTestEncodeDecodeMessageAnnotationsWithEmptyValue(true);
      }

      private void DoTestEncodeDecodeMessageAnnotationsWithEmptyValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteObject(buffer, encoderState, new Footer());

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
         Assert.IsTrue(result is Footer);

         Footer readAnnotations = (Footer)result;
         Assert.IsNull(readAnnotations.Value);
      }

      [Test]
      public void TestSkipValueWithInvalidList32Type()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List32, false);
      }

      [Test]
      public void TestSkipValueWithInvalidList8Type()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List8, false);
      }

      [Test]
      public void TestSkipValueWithInvalidList0Type()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List0, false);
      }

      [Test]
      public void TestSkipValueWithInvalidList32TypeFromStream()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List32, true);
      }

      [Test]
      public void TestSkipValueWithInvalidList8TypeFromStream()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List8, true);
      }

      [Test]
      public void TestSkipValueWithInvalidList0TypeFromStream()
      {
         DoTestSkipValueWithInvalidListType(EncodingCodes.List0, true);
      }

      private void DoTestSkipValueWithInvalidListType(EncodingCodes listType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Footer.DescriptorCode));
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
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Footer), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Footer), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      [Test]
      public void TestSkipValueWithNullMapEncoding()
      {
         DoTestSkipValueWithNullMapEncoding(false);
      }

      [Test]
      public void TestSkipValueWithNullMapEncodingFromStream()
      {
         DoTestSkipValueWithNullMapEncoding(true);
      }

      private void DoTestSkipValueWithNullMapEncoding(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)Footer.DescriptorCode));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Footer), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            catch (DecodeException)
            {
               Assert.Fail("Should be able to skip type with null inner encoding");
            }
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Footer), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
            }
            catch (DecodeException)
            {
               Assert.Fail("Should be able to skip type with null inner encoding");
            }
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

         Footer[] array = new Footer[3];

         IDictionary<Symbol, object> map = new Dictionary<Symbol, object>();
         map.Add(Symbol.Lookup("1"), true);
         map.Add(Symbol.Lookup("2"), false);

         array[0] = new Footer(new Dictionary<Symbol, object>());
         array[1] = new Footer(map);
         array[2] = new Footer(map);

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
         Assert.AreEqual(typeof(Footer), result.GetType().GetElementType());

         Footer[] resultArray = (Footer[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Footer);
            Assert.AreEqual(array[i].Value, resultArray[i].Value);
         }
      }

      [Test]
      public void TestEncodeAndDecodeAnnoationsWithEmbeddedMaps()
      {
         DoTestEncodeAndDecodeAnnoationsWithEmbeddedMaps(false);
      }

      [Test]
      public void TestEncodeAndDecodeAnnoationsWithEmbeddedMapsFromStream()
      {
         DoTestEncodeAndDecodeAnnoationsWithEmbeddedMaps(true);
      }

      private void DoTestEncodeAndDecodeAnnoationsWithEmbeddedMaps(bool fromStream)
      {
         Symbol SYMBOL_1 = Symbol.Lookup("x-opt-test1");
         Symbol SYMBOL_2 = Symbol.Lookup("x-opt-test2");

         string VALUE_1 = "string";
         uint VALUE_2 = 42;
         Guid VALUE_3 = Guid.NewGuid();

         IDictionary<string, object> stringKeyedMap = new Dictionary<string, object>();
         stringKeyedMap.Add("key1", VALUE_1);
         stringKeyedMap.Add("key2", VALUE_2);
         stringKeyedMap.Add("key3", VALUE_3);

         IDictionary<Symbol, object> symbolKeyedMap = new Dictionary<Symbol, object>();
         symbolKeyedMap.Add(Symbol.Lookup("key1"), VALUE_1);
         symbolKeyedMap.Add(Symbol.Lookup("key2"), VALUE_2);
         symbolKeyedMap.Add(Symbol.Lookup("key3"), VALUE_3);

         Footer annotations = new Footer(new Dictionary<Symbol, object>());
         annotations.Value.Add(SYMBOL_1, stringKeyedMap);
         annotations.Value.Add(SYMBOL_2, symbolKeyedMap);

         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteObject(buffer, encoderState, annotations);

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
         Assert.IsTrue(result is Footer);

         Footer readAnnotations = (Footer)result;

         IDictionary<Symbol, object> resultMap = readAnnotations.Value;

         Assert.AreEqual(annotations.Value.Count, resultMap.Count);
         Assert.AreEqual(resultMap[SYMBOL_1], stringKeyedMap);
         Assert.AreEqual(resultMap[SYMBOL_2], symbolKeyedMap);
      }
   }
}