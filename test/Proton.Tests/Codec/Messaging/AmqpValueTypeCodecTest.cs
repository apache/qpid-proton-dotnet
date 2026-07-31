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
   public class AmqpValueTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(AmqpValue), new AmqpValueTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(AmqpValue), new AmqpValueTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(AmqpValue.DescriptorCode, new AmqpValueTypeDecoder().DescriptorCode);
         Assert.AreEqual(AmqpValue.DescriptorCode, new AmqpValueTypeEncoder().DescriptorCode);
         Assert.AreEqual(AmqpValue.DescriptorSymbol, new AmqpValueTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(AmqpValue.DescriptorSymbol, new AmqpValueTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeAmqpValueString()
      {
         DoTestDecodeAmqpValueSeries(1, new AmqpValue("test"), false);
      }

      [Test]
      public void TestDecodeAmqpValueNull()
      {
         DoTestDecodeAmqpValueSeries(1, new AmqpValue((string) null), false);
      }

      [Test]
      public void TestDecodeAmqpValueUUID()
      {
         DoTestDecodeAmqpValueSeries(1, new AmqpValue(Guid.NewGuid()), false);
      }

      [Test]
      public void TestDecodeSmallSeriesOfAmqpValue()
      {
         DoTestDecodeAmqpValueSeries(SmallSize, new AmqpValue("test"), false);
      }

      [Test]
      public void TestDecodeLargeSeriesOfAmqpValue()
      {
         DoTestDecodeAmqpValueSeries(LargeSize, new AmqpValue("test"), false);
      }

      [Test]
      public void TestDecodeAmqpValueStringFromStream()
      {
         DoTestDecodeAmqpValueSeries(1, new AmqpValue("test"), true);
      }

      [Test]
      public void TestDecodeAmqpValueNullFromStream()
      {
         DoTestDecodeAmqpValueSeries(1, new AmqpValue((string)null), true);
      }

      [Test]
      public void TestDecodeAmqpValueUUIDFromStream()
      {
         DoTestDecodeAmqpValueSeries(1, new AmqpValue(Guid.NewGuid()), true);
      }

      [Test]
      public void TestDecodeSmallSeriesOfAmqpValueFromStream()
      {
         DoTestDecodeAmqpValueSeries(SmallSize, new AmqpValue("test"), true);
      }

      [Test]
      public void TestDecodeLargeSeriesOfAmqpValueFromStream()
      {
         DoTestDecodeAmqpValueSeries(LargeSize, new AmqpValue("test"), true);
      }

      private void DoTestDecodeAmqpValueSeries(int size, AmqpValue value, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, value);
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
            Assert.IsTrue(result is AmqpValue);

            AmqpValue decoded = (AmqpValue)result;

            Assert.AreEqual(value.Value, decoded.Value);
         }
      }

      [Test]
      public void TestDecodeAmqpValueWithEmptyValue()
      {
         DoTestDecodeAmqpValueWithEmptyValue(false);
      }

      [Test]
      public void TestDecodeAmqpValueWithEmptyValueFromStream()
      {
         DoTestDecodeAmqpValueWithEmptyValue(true);
      }

      private void DoTestDecodeAmqpValueWithEmptyValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteObject(buffer, encoderState, new AmqpValue(null));

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
         Assert.IsTrue(result is AmqpValue);

         AmqpValue decoded = (AmqpValue) result;

         Assert.IsNull(decoded.Value);
      }

      [Test]
      public void TestEncodeDecodeArrayOfAmqpValue()
      {
         DoTestEncodeDecodeArrayOfAmqpValue(false);
      }

      [Test]
      public void TestEncodeDecodeArrayOfAmqpValueFromStream()
      {
         DoTestEncodeDecodeArrayOfAmqpValue(true);
      }

      private void DoTestEncodeDecodeArrayOfAmqpValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         AmqpValue[] array = new AmqpValue[3];

         array[0] = new AmqpValue("1");
         array[1] = new AmqpValue("2");
         array[2] = new AmqpValue("3");

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
         Assert.AreEqual(typeof(AmqpValue), result.GetType().GetElementType());

         AmqpValue[] resultArray = (AmqpValue[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is AmqpValue);
            Assert.AreEqual(array[i].Value, resultArray[i].Value);
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

      private void DoTestSkipValue(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, new AmqpValue("skipMe"));
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
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

         AmqpValue[] array = new AmqpValue[3];

         array[0] = new AmqpValue("1");
         array[1] = new AmqpValue("2");
         array[2] = new AmqpValue("3");

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
         Assert.AreEqual(typeof(AmqpValue), result.GetType().GetElementType());

         AmqpValue[] resultArray = (AmqpValue[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is AmqpValue);
            Assert.AreEqual(array[i].Value, resultArray[i].Value);
         }
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray8()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array8, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray32()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array32, false);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray8FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array8, true);
      }

      [Test]
      public void TestDecodeFailsWhenArrayOfTypeWithList0EncodingsArray32FS()
      {
         DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes.Array32, true);
      }

      private void DoTestDecodeFailsWhenArrayOfTypeWithList0Encodings(EncodingCodes arrayType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         // First show that we can do this if the data is correct
         if (arrayType == EncodingCodes.Array32)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8);  // Size
            buffer.WriteInt(2);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)5);  // Size
            buffer.WriteUnsignedByte((byte)2);  // Count
         }
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List0);

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }

         buffer.ReadOffset = 0;  // Reset and try with limits lifted

         decoderState.MaxZeroWidthArrayElements = 2;
         streamDecoderState.MaxZeroWidthArrayElements = 2;

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is AmqpValue[]);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is AmqpValue[]);
         }
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValuesReadValueTriggersDepthLimit()
      {
         DoTestNestedAmqpValueOfAmqpValuesReadValueTriggersDepthLimit(false);
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValuesReadValueTriggersDepthLimitFS()
      {
         DoTestNestedAmqpValueOfAmqpValuesReadValueTriggersDepthLimit(true);
      }

      private void DoTestNestedAmqpValueOfAmqpValuesReadValueTriggersDepthLimit(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 1
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 2
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 3
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 4
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List0); // List Encoding - 5

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(stream, streamDecoderState) is AmqpValue);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.IsTrue(typeDecoder.ReadValue(buffer, decoderState) is AmqpValue);
         }

         buffer.ReadOffset = 0;  // Reset and try with limits lifted

         decoderState.DepthLimit = 4;
         streamDecoderState.DepthLimit = 4;

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.ReadValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValuesSkipValueTriggersDepthLimit()
      {
         DoTestNestedAmqpValueOfAmqpValuesSkipValueTriggersDepthLimit(false);
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValuesSkipValueTriggersDepthLimitFS()
      {
         DoTestNestedAmqpValueOfAmqpValuesSkipValueTriggersDepthLimit(true);
      }

      private void DoTestNestedAmqpValueOfAmqpValuesSkipValueTriggersDepthLimit(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 1
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 2
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 3
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 4
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)EncodingCodes.List0); // List Encoding - 5

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            typeDecoder.SkipValue(stream, streamDecoderState);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            typeDecoder.SkipValue(buffer, decoderState);
         }

         buffer.ReadOffset = 0;  // Reset and try with limits lifted

         decoderState.DepthLimit = 4;
         streamDecoderState.DepthLimit = 4;

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.SkipValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.SkipValue(buffer, decoderState));
         }
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValueList8SkipValueTriggersDepthLimit()
      {
         DoTestNestedAmqpValueOfAmqpValueListSkipValueTriggersDepthLimit(false, false);
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValueList8SkipValueTriggersDepthLimitFS()
      {
         DoTestNestedAmqpValueOfAmqpValueListSkipValueTriggersDepthLimit(false, true);
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValueList32SkipValueTriggersDepthLimit()
      {
         DoTestNestedAmqpValueOfAmqpValueListSkipValueTriggersDepthLimit(true, false);
      }

      [Test]
      public void TestNestedAmqpValueOfAmqpValueList32SkipValueTriggersDepthLimitFS()
      {
         DoTestNestedAmqpValueOfAmqpValueListSkipValueTriggersDepthLimit(true, true);
      }

      private void DoTestNestedAmqpValueOfAmqpValueListSkipValueTriggersDepthLimit(bool list8, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(0); // Described Type Indicator - 1
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);
         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator - 2
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte((byte)AmqpValue.DescriptorCode);

         // Level 3 of depth
         if (list8)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.List8);
            buffer.WriteUnsignedByte((byte)5);
            buffer.WriteUnsignedByte((byte)2);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
            buffer.WriteUnsignedByte((byte)1);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
            buffer.WriteUnsignedByte((byte)2);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.List32);
            buffer.WriteInt(8);
            buffer.WriteInt(2);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
            buffer.WriteUnsignedByte((byte)1);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
            buffer.WriteUnsignedByte((byte)2);
         }

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            typeDecoder.SkipValue(stream, streamDecoderState);
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            typeDecoder.SkipValue(buffer, decoderState);
         }

         buffer.ReadOffset = 0;  // Reset and try with limits lifted

         decoderState.DepthLimit = 2;
         streamDecoderState.DepthLimit = 2;

         if (fromStream)
         {
            IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.SkipValue(stream, streamDecoderState));
         }
         else
         {
            ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
            Assert.AreEqual(typeof(AmqpValue), typeDecoder.DecodesType);
            Assert.Throws<DecodeException>(() => typeDecoder.SkipValue(buffer, decoderState));
         }
      }
   }
}