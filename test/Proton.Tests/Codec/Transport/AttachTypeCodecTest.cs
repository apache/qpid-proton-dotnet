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
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using System;

namespace Apache.Qpid.Proton.Codec.Security
{
   [TestFixture]
   public class AttachTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestTypeClassReturnsCorrectType()
      {
         Assert.AreEqual(typeof(Attach), new AttachTypeDecoder().DecodesType);
         Assert.AreEqual(typeof(Attach), new AttachTypeEncoder().EncodesType);
      }

      [Test]
      public void TestDescriptors()
      {
         Assert.AreEqual(Attach.DescriptorCode, new AttachTypeDecoder().DescriptorCode);
         Assert.AreEqual(Attach.DescriptorCode, new AttachTypeEncoder().DescriptorCode);
         Assert.AreEqual(Attach.DescriptorSymbol, new AttachTypeDecoder().DescriptorSymbol);
         Assert.AreEqual(Attach.DescriptorSymbol, new AttachTypeEncoder().DescriptorSymbol);
      }

      [Test]
      public void TestCannotEncodeEmptyPerformative()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         Attach input = new Attach();

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
      public void TestEncodeDecodeTypeWithTarget()
      {
         DoTestEncodeDecodeType(new Target(), false);
      }

      [Test]
      public void TestEncodeDecodeTypeWithCoordinator()
      {
         DoTestEncodeDecodeType(new Coordinator(), false);
      }

      [Test]
      public void TestEncodeDecodeTypeWithTargetFromStream()
      {
         DoTestEncodeDecodeType(new Target(), true);
      }

      [Test]
      public void TestEncodeDecodeTypeWithCoordinatorFromStream()
      {
         DoTestEncodeDecodeType(new Coordinator(), true);
      }

      private void DoTestEncodeDecodeType(ITerminus target, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Symbol[] offeredCapabilities = new Symbol[] { Symbol.Lookup("Cap-1"), Symbol.Lookup("Cap-2") };
         Symbol[] desiredCapabilities = new Symbol[] { Symbol.Lookup("Cap-3"), Symbol.Lookup("Cap-4") };

         Random random = new Random(Environment.TickCount);

         uint randomHandle = (uint)random.Next();
         uint randomInitialDeliveryCount = (uint)random.Next();

         Attach input = new Attach();

         input.Name = "name";
         input.OfferedCapabilities = offeredCapabilities;
         input.DesiredCapabilities = desiredCapabilities;
         input.Handle = randomHandle;
         input.Role = Role.Receiver;
         input.SenderSettleMode = SenderSettleMode.Unsettled;
         input.ReceiverSettleMode = ReceiverSettleMode.Second;
         input.Source = new Source();
         input.Target = target;
         input.IncompleteUnsettled = false;
         input.InitialDeliveryCount = randomInitialDeliveryCount;
         input.MaxMessageSize = 1024u;

         encoder.WriteObject(buffer, encoderState, input);

         Attach result;
         if (fromStream)
         {
            result = (Attach)streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = (Attach)decoder.ReadObject(buffer, decoderState);
         }

         Assert.AreEqual("name", result.Name);
         Assert.AreEqual(randomHandle, result.Handle);
         Assert.AreEqual(Role.Receiver, result.Role);
         Assert.AreEqual(SenderSettleMode.Unsettled, result.SenderSettleMode);
         Assert.AreEqual(ReceiverSettleMode.Second, result.ReceiverSettleMode);
         Assert.AreEqual(randomInitialDeliveryCount, result.InitialDeliveryCount);
         Assert.AreEqual(1024u, result.MaxMessageSize);
         Assert.IsNotNull(result.Source);
         Assert.IsNotNull(result.Target);
         Assert.IsFalse(result.IncompleteUnsettled);
         Assert.IsNull(result.Unsettled);
         Assert.IsNull(result.Properties);
         Assert.AreEqual(offeredCapabilities, result.OfferedCapabilities);
         Assert.AreEqual(desiredCapabilities, result.DesiredCapabilities);
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

         Attach attach = new Attach();

         attach.Handle = 1;
         attach.Role = Role.Receiver;
         attach.Name = "skip";

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteObject(buffer, encoderState, attach);
         }

         attach.Handle = 2;
         attach.Role = Role.Sender;
         attach.Name = "test";

         encoder.WriteObject(buffer, encoderState, attach);

         for (int i = 0; i < 10; ++i)
         {
            if (fromStream)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Attach), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }
            else
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Attach), typeDecoder.DecodesType);
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
         Assert.IsTrue(result is Attach);

         Attach value = (Attach)result;
         Assert.AreEqual(Role.Sender, value.Role);
         Assert.AreEqual(2, value.Handle);
         Assert.AreEqual("test", value.Name);
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
         buffer.WriteUnsignedByte(((byte)Attach.DescriptorCode));
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
            Assert.AreEqual(typeof(Attach), typeDecoder.DecodesType);

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
            Assert.AreEqual(typeof(Attach), typeDecoder.DecodesType);

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
         buffer.WriteUnsignedByte(((byte)Attach.DescriptorCode));
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

         Attach[] array = new Attach[3];

         array[0] = new Attach();
         array[1] = new Attach();
         array[2] = new Attach();

         array[0].Handle = 0;
         array[0].Name = "0";
         array[0].InitialDeliveryCount = 0;
         array[0].Role = Role.Sender;
         array[1].Handle = 1;
         array[1].Name = "1";
         array[1].InitialDeliveryCount = 1;
         array[1].Role = Role.Sender;
         array[2].Handle = 2;
         array[2].Name = "2";
         array[2].InitialDeliveryCount = 2;
         array[2].Role = Role.Sender;

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
         Assert.AreEqual(typeof(Attach), result.GetType().GetElementType());

         Attach[] resultArray = (Attach[])result;

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsNotNull(resultArray[i]);
            Assert.IsTrue(resultArray[i] is Attach);
            Assert.AreEqual(array[i].Handle, resultArray[i].Handle);
            Assert.AreEqual(array[i].Name, resultArray[i].Name);
            Assert.AreEqual(array[i].InitialDeliveryCount, resultArray[i].InitialDeliveryCount);
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
         buffer.WriteUnsignedByte(((byte)Attach.DescriptorCode));
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
         buffer.WriteUnsignedByte(((byte)Attach.DescriptorCode));
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