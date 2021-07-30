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
using System.Collections.Generic;

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class ErrorConditionTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(ErrorCondition), new ErrorConditionTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(ErrorCondition), new ErrorConditionTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(ErrorCondition.DescriptorCode, new ErrorConditionTypeDecoder().DescriptorCode);
         Assert.AreEqual(ErrorCondition.DescriptorCode, new ErrorConditionTypeEncoder().DescriptorCode);
         Assert.AreEqual(ErrorCondition.DescriptorSymbol, new ErrorConditionTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(ErrorCondition.DescriptorSymbol, new ErrorConditionTypeEncoder().DescriptorSymbol);
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

         IDictionary<Symbol, object> infoMap = new Dictionary<Symbol, object>();
         infoMap.Add(Symbol.Lookup("1"), true);
         infoMap.Add(Symbol.Lookup("2"), "string");

         ErrorCondition error = new ErrorCondition(Symbol.Lookup("amqp-error"), "Something bad", infoMap);

         encoder.WriteObject(buffer, encoderState, error);

         ErrorCondition result;
         if (fromStream)
         {
            result = (ErrorCondition)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (ErrorCondition)decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.IsNotNull(result.Condition);
         Assert.IsNotNull(result.Description);
         Assert.IsNotNull(result.Info);

         Assert.AreEqual(Symbol.Lookup("amqp-error"), result.Condition);
         Assert.AreEqual("Something bad", result.Description);
         Assert.AreEqual(infoMap, result.Info);
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

         IDictionary<Symbol, object> infoMap = new Dictionary<Symbol, object>();
         infoMap.Add(Symbol.Lookup("1"), true);
         infoMap.Add(Symbol.Lookup("2"), "string");

         ErrorCondition error = new ErrorCondition(Symbol.Lookup("amqp-error"), "Something bad", infoMap);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, error);
         }

         error = new ErrorCondition(Symbol.Lookup("amqp-error-2"), "Something bad also", null);

         encoder.WriteObject(buffer, encoderState, error);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(ErrorCondition), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(ErrorCondition), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is ErrorCondition);

         ErrorCondition value = (ErrorCondition)result;
         Assert.AreEqual(Symbol.Lookup("amqp-error-2"), value.Condition);
         Assert.AreEqual("Something bad also", value.Description);
         Assert.IsNull(value.Info);
      }

      [Test]
      public void TestEqualityOfNewlyConstructed()
      {
         ErrorCondition new1 = new ErrorCondition(null, null, null);
         ErrorCondition new2 = new ErrorCondition(null, null, null);
         AssertErrorConditionsEqual(new1, new2);
      }

      [Test]
      public void TestSameObject()
      {
         ErrorCondition error = new ErrorCondition(null, null, null);
         AssertErrorConditionsEqual(error, error);
      }

      [Test]
      public void TestConditionEquality()
      {
         string symbolValue = "symbol";

         ErrorCondition same1 = new ErrorCondition(Symbol.Lookup(symbolValue), null);
         ErrorCondition same2 = new ErrorCondition(Symbol.Lookup(symbolValue), null);

         AssertErrorConditionsEqual(same1, same2);

         ErrorCondition different = new ErrorCondition(Symbol.Lookup("other"), null);

         AssertErrorConditionsNotEqual(same1, different);
      }

      [Test]
      public void TestConditionAndDescriptionEquality()
      {
         string symbolValue = "symbol";
         string descriptionValue = "description";

         ErrorCondition same1 = new ErrorCondition(Symbol.Lookup(new string(symbolValue)), new string(descriptionValue));
         ErrorCondition same2 = new ErrorCondition(Symbol.Lookup(new string(symbolValue)), new string(descriptionValue));

         AssertErrorConditionsEqual(same1, same2);

         ErrorCondition different = new ErrorCondition(Symbol.Lookup(symbolValue), "other");

         AssertErrorConditionsNotEqual(same1, different);
      }

      [Test]
      public void TestConditionDescriptionInfoEquality()
      {
         string symbolValue = "symbol";
         string descriptionValue = "description";

         IDictionary<Symbol, object> infoMap1 = new Dictionary<Symbol, object>();
         infoMap1.Add(Symbol.Lookup("key"), "value");
         IDictionary<Symbol, object> infoMap2 = new Dictionary<Symbol, object>();
         infoMap2.Add(Symbol.Lookup("key"), "value");
         IDictionary<Symbol, object> infoMap3 = new Dictionary<Symbol, object>();
         infoMap3.Add(Symbol.Lookup("other"), "value");

         ErrorCondition same1 = new ErrorCondition(
             Symbol.Lookup(new string(symbolValue)), new string(descriptionValue), infoMap1);
         ErrorCondition same2 = new ErrorCondition(
             Symbol.Lookup(new string(symbolValue)), new string(descriptionValue), infoMap2);

         AssertErrorConditionsEqual(same1, same2);

         ErrorCondition different = new ErrorCondition(
             Symbol.Lookup(symbolValue), new string(descriptionValue), infoMap3);

         AssertErrorConditionsNotEqual(same1, different);
      }

      private void AssertErrorConditionsNotEqual(ErrorCondition error1, ErrorCondition error2)
      {
         Assert.AreNotEqual(error1, error2);
         Assert.AreNotEqual(error2, error1);
      }

      private void AssertErrorConditionsEqual(ErrorCondition error1, ErrorCondition error2)
      {
         Assert.AreEqual(error1, error2);
         Assert.AreEqual(error2, error1);
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
         buffer.WriteUnsignedByte(((byte)ErrorCondition.DescriptorCode));
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
            Assert.AreEqual(typeof(ErrorCondition), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(ErrorCondition), typeDecoder.DecodesType);

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
         buffer.WriteUnsignedByte(((byte)ErrorCondition.DescriptorCode));
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

         ErrorCondition[] array = new ErrorCondition[3];

         array[0] = new ErrorCondition(AmqpError.DECODE_ERROR, "1");
         array[1] = new ErrorCondition(AmqpError.UNAUTHORIZED_ACCESS, "2");
         array[2] = new ErrorCondition(AmqpError.RESOURCE_LIMIT_EXCEEDED, "3");

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
         Assert.AreEqual(typeof(ErrorCondition), result.GetType().GetElementType());

         ErrorCondition[] resultArray = (ErrorCondition[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is ErrorCondition);
            Assert.AreEqual(array[i], resultArray[i]);
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
         buffer.WriteUnsignedByte(((byte)ErrorCondition.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)ErrorCondition.DescriptorCode));
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