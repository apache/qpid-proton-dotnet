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
using System.Collections;
using System;

namespace Apache.Qpid.Proton.Codec.Messaging
{
   [TestFixture]
   public class AmqpSequenceTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(AmqpSequence), new AmqpSequenceTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(AmqpSequence), new AmqpSequenceTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(AmqpSequence.DescriptorCode, new AmqpSequenceTypeDecoder().DescriptorCode);
         Assert.AreEqual(AmqpSequence.DescriptorCode, new AmqpSequenceTypeEncoder().DescriptorCode);
         Assert.AreEqual(AmqpSequence.DescriptorSymbol, new AmqpSequenceTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(AmqpSequence.DescriptorSymbol, new AmqpSequenceTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestDecodeAmqpValueString()
      {
         DoTestDecodeAmqpValueString(false);
      }

      [Test]
      public void TestDecodeAmqpValueStringFromStream()
      {
         DoTestDecodeAmqpValueString(true);
      }

      private void DoTestDecodeAmqpValueString(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList list = new ArrayList();

         list.Add(Guid.NewGuid());
         list.Add("string");

         AmqpSequence value = new AmqpSequence(list);

         encoder.WriteObject(buffer, encoderState, value);

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
         Assert.IsTrue(result is AmqpSequence);

         AmqpSequence decoded = (AmqpSequence)result;

         Assert.AreEqual(value.Value, decoded.Value);
      }

      // TODO: [Test]
      public void TestEncodeDecodeArrayOfAmqpSequence()
      {
         DoTestEncodeDecodeArrayOfAmqpSequence(false);
      }

      // TODO: [Test]
      public void TestEncodeDecodeArrayOfAmqpSequenceFromStream()
      {
         DoTestEncodeDecodeArrayOfAmqpSequence(true);
      }

      private void DoTestEncodeDecodeArrayOfAmqpSequence(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList list = new ArrayList();

         list.Add("test-1");
         list.Add("test-2");

         AmqpSequence[] array = new AmqpSequence[3];

         array[0] = new AmqpSequence(list);
         array[1] = new AmqpSequence(list);
         array[2] = new AmqpSequence(list);

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
         Assert.AreEqual(typeof(AmqpSequence), result.GetType().GetElementType());

         AmqpSequence[] resultArray = (AmqpSequence[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is AmqpSequence);
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

         IList list = new ArrayList();
         list.Add("one");
         list.Add("two");
         list.Add("three");

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, new AmqpSequence(list));
         }

         encoder.WriteObject(buffer, encoderState, new Modified());

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(AmqpSequence), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(AmqpSequence), typeDecoder.DecodesType);
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
         doTestDecodeWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap8Type()
      {
         doTestDecodeWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestDecodeWithInvalidMap32TypeFromStream()
      {
         doTestDecodeWithInvalidMapType(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestDecodeWithInvalidMap8TypeFromStream()
      {
         doTestDecodeWithInvalidMapType(EncodingCodes.Map8, true);
      }

      private void doTestDecodeWithInvalidMapType(EncodingCodes mapType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)AmqpSequence.DescriptorCode));
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
         doTestSkipValueWithInvalidMapType(EncodingCodes.Map32, false);
      }

      [Test]
      public void TestSkipValueWithInvalidMap8Type()
      {
         doTestSkipValueWithInvalidMapType(EncodingCodes.Map8, false);
      }

      [Test]
      public void TestSkipValueWithInvalidMap32TypeFromStream()
      {
         doTestSkipValueWithInvalidMapType(EncodingCodes.Map32, true);
      }

      [Test]
      public void TestSkipValueWithInvalidMap8TypeFromStream()
      {
         doTestSkipValueWithInvalidMapType(EncodingCodes.Map8, true);
      }

      private void doTestSkipValueWithInvalidMapType(EncodingCodes mapType, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)0); // Described Type Indicator
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(((byte)AmqpSequence.DescriptorCode));
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
            Assert.AreEqual(typeof(AmqpSequence), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(AmqpSequence), typeDecoder.DecodesType);

            try
            {
               typeDecoder.SkipValue(buffer, decoderState);
               Assert.Fail("Should not be able to skip type with invalid encoding");
            }
            catch (DecodeException) { }
         }
      }

      // TODO: [Test]
      public void TestEncodeDecodeArray()
      {
         DoTestEncodeDecodeArray(false);
      }

      // TODO: [Test]
      public void TestEncodeDecodeArrayFromStream()
      {
         DoTestEncodeDecodeArray(true);
      }

      private void DoTestEncodeDecodeArray(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         AmqpSequence[] array = new AmqpSequence[3];

         IList list = new ArrayList();
         list.Add("1");
         list.Add("2");

         array[0] = new AmqpSequence(new ArrayList());
         array[1] = new AmqpSequence(list);
         array[2] = new AmqpSequence(list);

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
         Assert.AreEqual(typeof(AmqpSequence), result.GetType().GetElementType());

         AmqpSequence[] resultArray = (AmqpSequence[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is AmqpSequence);
            Assert.AreEqual(array[i].Value, resultArray[i].Value);
         }
      }
   }
}