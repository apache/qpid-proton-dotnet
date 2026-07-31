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

using System;
using NUnit.Framework;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using System.IO;
using System.Collections.Generic;
using Apache.Qpid.Proton.Test.Driver.Codec.Impl;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   [TestFixture]
   public class ProtonStreamDecoderTest : CodecTestSupport
   {
      [Test]
      public void TestGetCachedDecoderStateReturnsCachedState()
      {
         IStreamDecoderState first = streamDecoder.CachedDecoderState;

         Assert.AreSame(first, streamDecoder.CachedDecoderState);
      }

      [Test]
      public void TestReadNullFromReadObjectForNullEncodng()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteByte(((sbyte)EncodingCodes.Null));
         buffer.WriteByte(((sbyte)EncodingCodes.Null));

         Assert.IsNull(streamDecoder.ReadObject(stream, streamDecoderState));
         Assert.Throws(typeof(DecodeException), () => streamDecoder.ReadObject<Guid>(stream, streamDecoderState));
      }

      [Test]
      public void TestTryReadFromEmptyStream()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         try
         {
            streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.Fail("Should fail on read of object from empty stream");
         }
         catch (DecodeEOFException) { }
      }

      [Test]
      public void TestErrorOnReadOfUnknownEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(255);

         Assert.IsNull(streamDecoder.PeekNextTypeDecoder(stream, streamDecoderState));

         try
         {
            streamDecoder.ReadObject(stream, streamDecoderState);
            Assert.Fail("Should throw if no type streamDecoder exists for given type");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestReadFromNullEncodingCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         try
         {
            streamDecoder.ReadObject<string>(stream, streamDecoderState);
            Assert.Fail("Should not allow for conversion to String type");
         }
         catch (DecodeException)
         {
         }
      }

      [Test]
      public void TestReadMultipleFromNullEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.Throws<DecodeException>(() => streamDecoder.ReadMultiple<Guid>(stream, streamDecoderState));
      }

      [Test]
      public void TestReadMultipleFromSingleEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         Guid[] result = streamDecoder.ReadMultiple<Guid>(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.AreEqual(1, result.Length);
      }

      [Test]
      public void TestReadObjectRequestsWrongType()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         Assert.Throws<DecodeException>(() => streamDecoder.ReadObject<string>(stream, streamDecoderState));
      }

      [Test]
      public void TestReadMultipleRequestsWrongTypeForArray()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         try
         {
            streamDecoder.ReadMultiple<string>(stream, streamDecoderState);
            Assert.Fail("Should not be able to convert to wrong resulting array type");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestReadMultipleRequestsWrongTypeForArrayEncoding()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid[] value = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

         encoder.WriteArray(buffer, encoderState, value);

         try
         {
            streamDecoder.ReadMultiple<string>(stream, streamDecoderState);
            Assert.Fail("Should not be able to convert to wrong resulting array type");
         }
         catch (DecodeException) { }
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithNegativeLongDescriptor()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Guid value = Guid.NewGuid();

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(long.MaxValue);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = streamDecoder.ReadObject(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithMaxLongDescriptor()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         buffer.WriteLong(long.MaxValue);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = streamDecoder.ReadObject(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithUnknownDescriptorCode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.DescribedTypeIndicator));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Uuid));
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         object result = streamDecoder.ReadObject(stream, streamDecoderState);

         Assert.IsNotNull(result);
         Assert.IsTrue(result is UnknownDescribedType);

         UnknownDescribedType type = (UnknownDescribedType)result;
         Assert.IsTrue(type.Described is Guid);
      }

      [Test]
      public void TestReadUnsignedIntegerTypes()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt0));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallUInt));
         buffer.WriteUnsignedByte(127);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(0);
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));

         Assert.AreEqual(0, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
         Assert.AreEqual(127, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
         Assert.AreEqual(255, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
         Assert.AreEqual(32, streamDecoder.ReadUnsignedInteger(stream, streamDecoderState, 32));
      }

      [Test]
      public void TestReadStringWithCustomStringDecoder()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte(((byte)EncodingCodes.Str32));
         buffer.WriteInt(16);
         buffer.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

         ((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder = new DummyUtf8StreamDecoder();

         Assert.IsNotNull(((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder);

         string result = streamDecoder.ReadString(stream, streamDecoderState);

         Assert.AreEqual("string-streamDecoder", result);
         Assert.IsTrue(buffer.IsReadable); // We didn't read anything so buffer was untouched
      }

      [Test]
      public void TestStringReadFromCustomDecoderThrowsDecodeExceptionOnError()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)EncodingCodes.Str32);
         buffer.WriteInt(16);
         buffer.WriteBytes(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 });

         ((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder = new FailingUtf8StreamDecoder();

         Assert.IsNotNull(((ProtonStreamDecoderState)streamDecoderState).Utf8Decoder);
         Assert.Throws(typeof(DecodeException), () => streamDecoder.ReadString(stream, streamDecoderState));
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeFailsWhenInSASLMode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         ProtonStreamDecoder streamDecoder = ProtonStreamDecoderFactory.CreateSasl();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)EncodingCodes.DescribedTypeIndicator);
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallULong);
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Uuid);
         buffer.WriteLong(250L);
         buffer.WriteLong(128L);

         Assert.Throws(typeof(DecodeException), () => streamDecoder.ReadObject(stream, streamDecoderState));

         streamDecoder = ProtonStreamDecoderFactory.Create();
         buffer.ReadOffset = 0;

         Assert.DoesNotThrow(() => streamDecoder.ReadObject(stream, streamDecoderState));
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeWithRestrictedDescriptorFailsWhenInSASLMode()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         ProtonStreamDecoder streamDecoder = ProtonStreamDecoderFactory.CreateSasl();
         IStreamDecoderState state = streamDecoder.NewDecoderState();
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)EncodingCodes.DescribedTypeIndicator);
         buffer.WriteUnsignedByte((byte)EncodingCodes.SmallUInt);
         buffer.WriteUnsignedByte(255);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Uuid);
         buffer.WriteLong(256L);
         buffer.WriteLong(128L);

         Assert.Throws<DecodeException>(() => streamDecoder.ReadObject(stream, state));
      }

      [Test]
      public void TestLargeSymbolDescriptorsAreNotPutInUnknownTypeCache()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(8192);
         ProtonStreamDecoder decoder = ProtonStreamDecoderFactory.Create();
         IStreamDecoderState state = streamDecoder.NewDecoderState();
         Stream stream = new ProtonBufferInputStream(buffer);

         int descriptorLength = ProtonStreamDecoder.UnknownDescribedTypeDescriptorSizeLimit + 1;

         for (int i = 0; i < ProtonStreamDecoder.UnknownDescribedTypeCacheLimit; ++i)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.DescribedTypeIndicator);
            buffer.WriteUnsignedByte((byte)EncodingCodes.Sym8);
            buffer.WriteUnsignedByte((byte)descriptorLength);
            for (int j = 0; j < descriptorLength; ++j)
            {
               buffer.WriteUnsignedByte(65);
            }
            buffer.WriteUnsignedByte((byte)EncodingCodes.Uuid);
            buffer.WriteLong(256L);
            buffer.WriteLong(128L);
         }

         ISet<IStreamTypeDecoder> typeDecoders = new HashSet<IStreamTypeDecoder>();

         for (int i = 0; i < ProtonStreamDecoder.UnknownDescribedTypeCacheLimit; ++i)
         {
            IStreamTypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(stream, state);
            Assert.IsTrue(typeDecoder is UnknownDescribedTypeDecoder);
            Assert.IsTrue(typeDecoders.Add(typeDecoder));
            UnknownDescribedType result = (UnknownDescribedType)typeDecoder.ReadValue(stream, state);
            Assert.IsTrue(result.Descriptor is Symbol);
            Assert.IsTrue(result.Described is Guid);
         }

         Assert.AreEqual(ProtonStreamDecoder.UnknownDescribedTypeCacheLimit, typeDecoders.Count);
      }

      [Test]
      public void TestReadObjectArray8FailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter()
      {
         TestReadObjectFailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter(EncodingCodes.Array8);
      }

      [Test]
      public void TestReadObjectArray32FailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter()
      {
         TestReadObjectFailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter(EncodingCodes.Array32);
      }

      private void TestReadObjectFailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter(EncodingCodes arrayType)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(8192);
         Stream stream = new ProtonBufferInputStream(buffer);

         if (EncodingCodes.Array32 == arrayType)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8); // Size
            buffer.WriteInt(3);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)5);
            buffer.WriteUnsignedByte((byte)3);
         }

         buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)2);
         buffer.WriteUnsignedByte((byte)3);

         Assert.Throws<DecodeException>(() => streamDecoder.ReadObject<Symbol>(stream, streamDecoderState));

         Assert.IsTrue(stream.Length > 0); // Should not have read array contents
      }

      [Test]
      public void TestReadMultipleArray8FailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter()
      {
         TestReadMultipleFailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter(EncodingCodes.Array8);
      }

      [Test]
      public void TestReadMultipleArray32FailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter()
      {
         TestReadMultipleFailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter(EncodingCodes.Array32);
      }

      private void TestReadMultipleFailsBeforeDecodingContentsIfEncodingDoesNotMatchFilter(EncodingCodes arrayType)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(8192);
         Stream stream = new ProtonBufferInputStream(buffer);

         if (EncodingCodes.Array32 == arrayType)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array32);
            buffer.WriteInt(8); // Size
            buffer.WriteInt(3);  // Count
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.Array8);
            buffer.WriteUnsignedByte((byte)5);
            buffer.WriteUnsignedByte((byte)3);
         }

         buffer.WriteUnsignedByte((byte)EncodingCodes.Byte);
         buffer.WriteUnsignedByte((byte)1);
         buffer.WriteUnsignedByte((byte)2);
         buffer.WriteUnsignedByte((byte)3);

         Assert.Throws<DecodeException>(() => streamDecoder.ReadMultiple<Symbol>(stream, streamDecoderState));

         Assert.IsTrue(stream.Length > 0); // Should not have read array contents
      }

      [Test]
      public void TestReadObjectForObjectDoesNotDecodeIfFilterDoesNotMatch()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(8192);
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)EncodingCodes.Uuid);
         buffer.WriteLong(255);
         buffer.WriteLong(512);

         Assert.Throws<DecodeException>(() => streamDecoder.ReadObject<string>(stream, streamDecoderState));

         Assert.IsTrue(stream.CanRead); // Should not have read array contents
      }

      [Test]
      public void TestReadMultipleForObjectDoesNotDecodeIfFilterDoesNotMatch()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate(8192);
         Stream stream = new ProtonBufferInputStream(buffer);

         buffer.WriteUnsignedByte((byte)EncodingCodes.Uuid);
         buffer.WriteLong(127);
         buffer.WriteLong(128);

         Assert.Throws<DecodeException>(() => streamDecoder.ReadMultiple<string>(stream, streamDecoderState));

         Assert.IsTrue(stream.CanRead); // Should not have read array contents
      }
   }

   internal class DummyUtf8StreamDecoder : IUtf8StreamDecoder
   {
      public string DecodeUTF8(Stream stream, int utf8length)
      {
         return "string-streamDecoder";
      }
   }

   internal class FailingUtf8StreamDecoder : IUtf8StreamDecoder
   {
      public string DecodeUTF8(Stream stream, int utf8length)
      {
         throw new IndexOutOfRangeException();
      }
   }
}
