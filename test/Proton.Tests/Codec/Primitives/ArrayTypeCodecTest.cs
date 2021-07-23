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
using System;
using System.Collections;

namespace Apache.Qpid.Proton.Codec.Primitives
{
   [TestFixture]
   public class ArrayTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestWriteOfZeroSizedGenericArrayFails()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         object[] source = new object[0];

         Assert.Throws<ArgumentException>(() => encoder.WriteArray(buffer, encoderState, source));
      }

      [Test]
      public void TestWriteOfGenericArrayOfObjectsFails()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         object[] source = new object[2];

         source[0] = new object();
         source[1] = new object();

         Assert.Throws<ArgumentException>(() => encoder.WriteArray(buffer, encoderState, source));
      }

      [Test]
      public void TestArrayOfArraysOfMixedTypes()
      {
         DoTestArrayOfArraysOfMixedTypes(false);
      }

      [Test]
      public void TestArrayOfArraysOfMixedTypesFromStream()
      {
         DoTestArrayOfArraysOfMixedTypes(true);
      }

      private void DoTestArrayOfArraysOfMixedTypes(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         object[][] source = new object[2][];
         source[0] = new object[size];
         source[1] = new object[size];
         for (int i = 0; i < size; ++i)
         {
            source[0][i] = ((short)i);
            source[1][i] = i;
         }

         encoder.WriteArray(buffer, encoderState, source);

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
         Assert.IsTrue(result.GetType().IsArray);

         object[] resultArray = (object[])result;

         Assert.IsNotNull(resultArray);
         Assert.AreEqual(2, resultArray.Length);

         Assert.IsTrue(resultArray[0].GetType().IsArray);
         Assert.IsTrue(resultArray[1].GetType().IsArray);
      }

      [Test]
      public void TestArrayOfArraysOfArraysOfShortTypes()
      {
         TestArrayOfArraysOfArraysOfShortTypes(false);
      }

      [Test]
      public void TestArrayOfArraysOfArraysOfShortTypesFromStream()
      {
         TestArrayOfArraysOfArraysOfShortTypes(true);
      }

      private void TestArrayOfArraysOfArraysOfShortTypes(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;
         object[][][] source = new object[2][][];
         for (int i = 0; i < 2; ++i)
         {
            source[i] = new object[2][];
            for (int j = 0; j < 2; ++j)
            {
               source[i][j] = new object[size];
               for (int k = 0; k < size; ++k)
               {
                  source[i][j][k] = (short)k;
               }
            }
         }

         encoder.WriteArray(buffer, encoderState, source);

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
         Assert.IsTrue(result.GetType().IsArray);

         object[] resultArray = (object[])result;

         Assert.IsNotNull(resultArray);
         Assert.AreEqual(2, resultArray.Length);

         for (int i = 0; i < resultArray.Length; ++i)
         {
            Assert.IsTrue(resultArray[i].GetType().IsArray);

            object[] dimension2 = (object[])resultArray[i];
            Assert.AreEqual(2, dimension2.Length);

            for (int j = 0; j < dimension2.Length; ++j)
            {
               short[] dimension3 = (short[])dimension2[j];
               Assert.AreEqual(size, dimension3.Length);

               for (int k = 0; k < dimension3.Length; ++k)
               {
                  Assert.AreEqual(source[i][j][k], dimension3[k]);
               }
            }
         }
      }

      [Test]
      public void TestWriteArrayOfArraysStrings()
      {
         TestWriteArrayOfArraysStrings(false);
      }

      [Test]
      public void TestWriteArrayOfArraysStringsFromStream()
      {
         TestWriteArrayOfArraysStrings(true);
      }

      private void TestWriteArrayOfArraysStrings(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         string[][] stringArray = new string[2][];

         stringArray[0] = new string[1];
         stringArray[1] = new string[1];

         stringArray[0][0] = "short-string";
         stringArray[1][0] = "long-string-entry:" + Guid.NewGuid().ToString() + "," +
                                                    Guid.NewGuid().ToString() + "," +
                                                    Guid.NewGuid().ToString() + "," +
                                                    Guid.NewGuid().ToString() + "," +
                                                    Guid.NewGuid().ToString() + "," +
                                                    Guid.NewGuid().ToString() + "," +
                                                    Guid.NewGuid().ToString();

         encoder.WriteArray(buffer, encoderState, stringArray);

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
         Assert.IsTrue(result.GetType().IsArray);

         Object[] array = (Object[])result;
         Assert.AreEqual(2, array.Length);

         Assert.IsTrue(array[0] is string[]);
         Assert.IsTrue(array[1] is string[]);

         string[] element1Array = (string[])array[0];
         string[] element2Array = (string[])array[1];

         Assert.AreEqual(1, element1Array.Length);
         Assert.AreEqual(1, element2Array.Length);

         Assert.AreEqual(stringArray[0][0], element1Array[0]);
         Assert.AreEqual(stringArray[1][0], element2Array[0]);
      }

      [Test]
      public void TestEncodeAndDecodeArrayOfListsUsingReadMultiple()
      {
         TestEncodeAndDecodeArrayOfListsUsingReadMultiple(false);
      }

      [Test]
      public void TestEncodeAndDecodeArrayOfListsUsingReadMultipleFromStream()
      {
         TestEncodeAndDecodeArrayOfListsUsingReadMultiple(true);
      }

      private void TestEncodeAndDecodeArrayOfListsUsingReadMultiple(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList[] lists = new IList[3];

         ArrayList content1 = new ArrayList();
         ArrayList content2 = new ArrayList();
         ArrayList content3 = new ArrayList();

         content1.Add("test-1");
         content2.Add("test-2");
         content3.Add("test-3");

         lists[0] = content1;
         lists[1] = content2;
         lists[2] = content3;

         encoder.WriteObject(buffer, encoderState, lists);

         IList[] decoded;
         if (fromStream)
         {
            decoded = streamDecoder.ReadMultiple<IList>(stream, streamDecoderState);
         }
         else
         {
            decoded = decoder.ReadMultiple<IList>(buffer, decoderState);
         }

         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded.GetType().IsArray);
         Assert.AreEqual(typeof(IList), decoded.GetType().GetElementType());
         Assert.AreEqual(lists, decoded);
      }

      // TODO [Test]
      public void TestEncodeAndDecodeArrayOfMapsUsingReadMultiple()
      {
         TestEncodeAndDecodeArrayOfMapsUsingReadMultiple(false);
      }

      // TODO [Test]
      public void TestEncodeAndDecodeArrayOfMapsUsingReadMultipleFromStream()
      {
         TestEncodeAndDecodeArrayOfMapsUsingReadMultiple(true);
      }

      private void TestEncodeAndDecodeArrayOfMapsUsingReadMultiple(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary[] maps = new IDictionary[3];

         IDictionary content1 = new Hashtable();
         IDictionary content2 = new Hashtable();
         IDictionary content3 = new Hashtable();

         content1.Add("test-1", Guid.NewGuid());
         content2.Add("test-2", "string");
         content3.Add("test-3", false);

         maps[0] = content1;
         maps[1] = content2;
         maps[2] = content3;

         encoder.WriteObject(buffer, encoderState, maps);

         IDictionary[] decoded;
         if (fromStream)
         {
            decoded = streamDecoder.ReadMultiple<IDictionary>(stream, streamDecoderState);
         }
         else
         {
            decoded = decoder.ReadMultiple<IDictionary>(buffer, decoderState);
         }

         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded.GetType().IsArray);
         Assert.AreEqual(typeof(IDictionary), decoded.GetType().GetElementType());
         Assert.AreEqual(maps, decoded);
      }

      [Test]
      public void TestEncodeDecodeBooleanArray100()
      {
         // bool array8 less than 128 bytes
         DoEncodeDecodeBooleanArrayTestImpl(100, false);
      }

      [Test]
      public void TestEncodeDecodeBooleanArray192()
      {
         // bool array8 greater than 128 bytes
         DoEncodeDecodeBooleanArrayTestImpl(192, false);
      }

      [Test]
      public void TestEncodeDecodeBooleanArray384()
      {
         // bool array32
         DoEncodeDecodeBooleanArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeBooleanArray100FS()
      {
         // bool array8 less than 128 bytes
         DoEncodeDecodeBooleanArrayTestImpl(100, true);
      }

      [Test]
      public void TestEncodeDecodeBooleanArray192FS()
      {
         // bool array8 greater than 128 bytes
         DoEncodeDecodeBooleanArrayTestImpl(192, true);
      }

      [Test]
      public void TestEncodeDecodeBooleanArray384FS()
      {
         // bool array32
         DoEncodeDecodeBooleanArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeBooleanArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         bool[] source = CreatePayloadArrayBooleans(count);

         Assert.AreEqual(count, source.Length, "Unexpected source array length");

         int encodingWidth = count < 254 ? 1 : 4; // less than 254 and not 256, since we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
         int arrayPayloadSize = encodingWidth + 1 + count; // variable width for element count + byte type descriptor + number of elements
         int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code +  variable width for array size + other encoded payload
         byte[] expectedEncoding = new byte[expectedEncodedArraySize];
         IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
         expectedEncodingWrapper.WriteOffset = 0;

         // Write the array encoding code, array size, and element count
         if (count < 254)
         {
            expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
            expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
            expectedEncodingWrapper.WriteUnsignedByte((byte)count);
         }
         else
         {
            expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
            expectedEncodingWrapper.WriteInt(arrayPayloadSize);
            expectedEncodingWrapper.WriteInt(count);
         }

         // Write the type descriptor
         expectedEncodingWrapper.WriteUnsignedByte((byte)0x56); // 'bool' type descriptor code

         // Write the elements
         for (int i = 0; i < count; i++)
         {
            byte booleanCode = (byte)(source[i] ? 0x01 : 0x00); //  0x01 true, 0x00 false.
            expectedEncodingWrapper.WriteUnsignedByte(booleanCode);
         }

         Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

         // Now verify against the actual encoding of the array
         Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
         encoder.WriteArray(buffer, encoderState, source);
         Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

         byte[] actualEncoding = new byte[expectedEncodedArraySize];
         buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
         Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");
         Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

         object decoded;
         if (fromStream)
         {
            decoded = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            decoded = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded.GetType().IsArray);
         Assert.AreEqual(typeof(bool), decoded.GetType().GetElementType());

         Assert.AreEqual(source, (bool[])decoded, "Unexpected decoding");
      }

      private static bool[] CreatePayloadArrayBooleans(int length)
      {
         Random rand = new Random(Environment.TickCount);

         bool[] payload = new bool[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = rand.Next(1) == 0 ? false : true;
         }

         return payload;
      }

      [Test]
      public void TestEncodeDecodeByteArray100()
      {
         // byte array8 less than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(100, false);
      }

      [Test]
      public void TestEncodeDecodeByteArray192()
      {
         // byte array8 greater than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(192, false);
      }

      [Test]
      public void TestEncodeDecodeByteArray254()
      {
         // byte array8 greater than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(254, false);
      }

      [Test]
      public void TestEncodeDecodeByteArray255()
      {
         // byte array8 greater than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(255, false);
      }

      [Test]
      public void TestEncodeDecodeByteArray384()
      {
         // byte array32
         DoEncodeDecodeByteArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeByteArray100FS()
      {
         // byte array8 less than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(100, true);
      }

      [Test]
      public void TestEncodeDecodeByteArray192FS()
      {
         // byte array8 greater than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(192, true);
      }

      [Test]
      public void TestEncodeDecodeByteArray254FS()
      {
         // byte array8 greater than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(254, true);
      }

      [Test]
      public void TestEncodeDecodeByteArray255FS()
      {
         // byte array8 greater than 128 bytes
         DoEncodeDecodeByteArrayTestImpl(255, true);
      }

      [Test]
      public void TestEncodeDecodeByteArray384FS()
      {
         // byte array32
         DoEncodeDecodeByteArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeByteArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         sbyte[] source = CreatePayloadArraySignedBytes(count);

         Assert.AreEqual(count, source.Length, "Unexpected source array length");

         int encodingWidth = count < 254 ? 1 : 4; // less than 254 and not 256, since we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
         int arrayPayloadSize = encodingWidth + 1 + count; // variable width for element count + byte type descriptor + number of elements
         int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code + variable width for array size + other encoded payload
         byte[] expectedEncoding = new byte[expectedEncodedArraySize];
         IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
         expectedEncodingWrapper.WriteOffset = 0;

         // Write the array encoding code, array size, and element count
         if (count < 254)
         {
            expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
            expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
            expectedEncodingWrapper.WriteUnsignedByte((byte)count);
         }
         else
         {
            expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
            expectedEncodingWrapper.WriteInt(arrayPayloadSize);
            expectedEncodingWrapper.WriteInt(count);
         }

         // Write the type descriptor
         expectedEncodingWrapper.WriteUnsignedByte((byte)0x51); // 'byte' type descriptor code

         // Write the elements
         for (int i = 0; i < count; i++)
         {
            expectedEncodingWrapper.WriteUnsignedByte((byte)source[i]);
         }

         Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

         // Now verify against the actual encoding of the array
         Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
         encoder.WriteArray(buffer, encoderState, source);
         Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

         byte[] actualEncoding = new byte[expectedEncodedArraySize];
         buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
         Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");

         Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

         object decoded;
         if (fromStream)
         {
            decoded = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            decoded = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(decoded);
         Assert.IsTrue(decoded.GetType().IsArray);
         Assert.AreEqual(typeof(sbyte), decoded.GetType().GetElementType());

         Assert.AreEqual(source, (sbyte[])decoded, "Unexpected decoding");
      }

      private static sbyte[] CreatePayloadArraySignedBytes(int length)
      {
         Random rand = new Random(Environment.TickCount);

         sbyte[] payload = new sbyte[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = (sbyte)(64 + 1 + rand.Next(9));
         }

         return payload;
      }

      [Test]
      public void TestEncodeDecodeShortArray50()
      {
         // short array8 less than 128 bytes
         DoEncodeDecodeShortArrayTestImpl(50, false);
      }

      [Test]
      public void TestEncodeDecodeShortArray100()
      {
         // short array8 greater than 128 bytes
         DoEncodeDecodeShortArrayTestImpl(100, false);
      }

      [Test]
      public void TestEncodeDecodeShortArray384()
      {
         // short array32
         DoEncodeDecodeShortArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeShortArray50FS()
      {
         // short array8 less than 128 bytes
         DoEncodeDecodeShortArrayTestImpl(50, true);
      }

      [Test]
      public void TestEncodeDecodeShortArray100FS()
      {
         // short array8 greater than 128 bytes
         DoEncodeDecodeShortArrayTestImpl(100, true);
      }

      [Test]
      public void TestEncodeDecodeShortArray384FS()
      {
         // short array32
         DoEncodeDecodeShortArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeShortArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         short[] source = CreatePayloadArrayShorts(count);

         try
         {
            Assert.AreEqual(count, source.Length, "Unexpected source array length");

            int encodingWidth = count < 127 ? 1 : 4; // less than 127, since each element is 2 bytes, but we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
            int arrayPayloadSize = encodingWidth + 1 + (count * 2); // variable width for element count + byte type descriptor + (number of elements * size)
            int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code +  variable width for array size + other encoded payload
            byte[] expectedEncoding = new byte[expectedEncodedArraySize];
            IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
            expectedEncodingWrapper.WriteOffset = 0;

            // Write the array encoding code, array size, and element count
            if (count < 254)
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
               expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
               expectedEncodingWrapper.WriteUnsignedByte((byte)count);
            }
            else
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
               expectedEncodingWrapper.WriteInt(arrayPayloadSize);
               expectedEncodingWrapper.WriteInt(count);
            }

            // Write the type descriptor
            expectedEncodingWrapper.WriteUnsignedByte((byte)0x61); // 'short' type descriptor code

            // Write the elements
            for (int i = 0; i < count; i++)
            {
               expectedEncodingWrapper.WriteShort(source[i]);
            }

            Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

            // Now verify against the actual encoding of the array
            Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
            encoder.WriteArray(buffer, encoderState, source);
            Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

            byte[] actualEncoding = new byte[expectedEncodedArraySize];
            buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
            Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");

            Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

            object decoded;
            if (fromStream)
            {
               decoded = streamDecoder.ReadObject(stream, streamDecoderState);
            }
            else
            {
               decoded = decoder.ReadObject(buffer, decoderState);
            }

            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.GetType().IsArray);
            Assert.AreEqual(typeof(short), decoded.GetType().GetElementType());
            Assert.AreEqual(source, (short[])decoded, "Unexpected decoding");
         }
         catch (Exception)
         {
            Console.WriteLine("Error during test, source array: " + source);
            throw;
         }
      }

      private static short[] CreatePayloadArrayShorts(int length)
      {
         Random rand = new Random(Environment.TickCount);

         short[] payload = new short[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = (short)(64 + 1 + rand.Next(9));
         }

         return payload;
      }

      [Test]
      public void TestEncodeDecodeIntArray10()
      {
         // int array8 less than 128 bytes
         DoEncodeDecodeIntArrayTestImpl(10, false);
      }

      [Test]
      public void TestEncodeDecodeIntArray50()
      {
         // int array8 greater than 128 bytes
         DoEncodeDecodeIntArrayTestImpl(50, false);
      }

      [Test]
      public void TestEncodeDecodeIntArray384()
      {
         // int array32
         DoEncodeDecodeIntArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeIntArray10FS()
      {
         // int array8 less than 128 bytes
         DoEncodeDecodeIntArrayTestImpl(10, true);
      }

      [Test]
      public void TestEncodeDecodeIntArray50FS()
      {
         // int array8 greater than 128 bytes
         DoEncodeDecodeIntArrayTestImpl(50, true);
      }

      [Test]
      public void TestEncodeDecodeIntArray384FS()
      {
         // int array32
         DoEncodeDecodeIntArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeIntArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         int[] source = CreatePayloadArrayIntegers(count);

         try
         {
            Assert.AreEqual(count, source.Length, "Unexpected source array length");

            int encodingWidth = count < 63 ? 1 : 4; // less than 63, since each element is 4 bytes, but we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
            int elementWidth = 4;
            int arrayPayloadSize = encodingWidth + 1 + (count * elementWidth); // variable width for element count + byte type descriptor + (number of elements * size)
            int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code +  variable width for array size + other encoded payload
            byte[] expectedEncoding = new byte[expectedEncodedArraySize];
            IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
            expectedEncodingWrapper.WriteOffset = 0;

            // Write the array encoding code, array size, and element count
            if (count < 254)
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
               expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
               expectedEncodingWrapper.WriteUnsignedByte((byte)count);
            }
            else
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
               expectedEncodingWrapper.WriteInt(arrayPayloadSize);
               expectedEncodingWrapper.WriteInt(count);
            }

            // Write the type descriptor
            expectedEncodingWrapper.WriteUnsignedByte((byte)0x71); // 'int' type descriptor code

            // Write the elements
            for (int i = 0; i < count; i++)
            {
               int j = source[i];
               expectedEncodingWrapper.WriteInt(j);
            }

            Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

            // Now verify against the actual encoding of the array
            Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
            encoder.WriteArray(buffer, encoderState, source);
            Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

            byte[] actualEncoding = new byte[expectedEncodedArraySize];
            buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
            Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");

            Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

            object decoded;
            if (fromStream)
            {
               decoded = streamDecoder.ReadObject(stream, streamDecoderState);
            }
            else
            {
               decoded = decoder.ReadObject(buffer, decoderState);
            }

            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.GetType().IsArray);
            Assert.AreEqual(typeof(int), decoded.GetType().GetElementType());
            Assert.AreEqual(source, (int[])decoded, "Unexpected decoding");
         }
         catch (Exception)
         {
            Console.WriteLine("Error during test, source array: " + source);
            throw;
         }
      }

      private static int[] CreatePayloadArrayIntegers(int length)
      {
         Random rand = new Random(Environment.TickCount);

         int[] payload = new int[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = 128 + 1 + rand.Next(9);
         }

         return payload;
      }

      [Test]
      public void TestEncodeDecodeLongArray10()
      {
         // long array8 less than 128 bytes
         DoEncodeDecodeLongArrayTestImpl(10, false);
      }

      [Test]
      public void TestEncodeDecodeLongArray25()
      {
         // long array8 greater than 128 bytes
         DoEncodeDecodeLongArrayTestImpl(25, false);
      }

      [Test]
      public void TestEncodeDecodeLongArray384()
      {
         // long array32
         DoEncodeDecodeLongArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeLongArray10FS()
      {
         // long array8 less than 128 bytes
         DoEncodeDecodeLongArrayTestImpl(10, false);
      }

      [Test]
      public void TestEncodeDecodeLongArray25FS()
      {
         // long array8 greater than 128 bytes
         DoEncodeDecodeLongArrayTestImpl(25, false);
      }

      [Test]
      public void TestEncodeDecodeLongArray384FS()
      {
         // long array32
         DoEncodeDecodeLongArrayTestImpl(384, false);
      }

      private void DoEncodeDecodeLongArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         long[] source = CreatePayloadArrayLongs(count);

         try
         {
            Assert.AreEqual(count, source.Length, "Unexpected source array length");

            int encodingWidth = count < 31 ? 1 : 4; // less than 31, since each element is 8 bytes, but we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
            int elementWidth = 8;

            int arrayPayloadSize = encodingWidth + 1 + (count * elementWidth); // variable width for element count + byte type descriptor + (number of elements * size)
            int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code +  variable width for array size + other encoded payload
            byte[] expectedEncoding = new byte[expectedEncodedArraySize];
            IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
            expectedEncodingWrapper.WriteOffset = 0;

            // Write the array encoding code, array size, and element count
            if (count < 254)
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
               expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
               expectedEncodingWrapper.WriteUnsignedByte((byte)count);
            }
            else
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
               expectedEncodingWrapper.WriteInt(arrayPayloadSize);
               expectedEncodingWrapper.WriteInt(count);
            }

            // Write the type descriptor
            expectedEncodingWrapper.WriteUnsignedByte((byte)0x81); // 'long' type descriptor code

            // Write the elements
            for (int i = 0; i < count; i++)
            {
               long j = source[i];
               expectedEncodingWrapper.WriteLong(j);
            }

            Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

            // Now verify against the actual encoding of the array
            Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
            encoder.WriteArray(buffer, encoderState, source);
            Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

            byte[] actualEncoding = new byte[expectedEncodedArraySize];
            buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
            Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");

            Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

            object decoded;
            if (fromStream)
            {
               decoded = streamDecoder.ReadObject(stream, streamDecoderState);
            }
            else
            {
               decoded = decoder.ReadObject(buffer, decoderState);
            }

            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.GetType().IsArray);
            Assert.AreEqual(typeof(long), decoded.GetType().GetElementType());
            Assert.AreEqual(source, (long[])decoded, "Unexpected decoding");
         }
         catch (Exception)
         {
            Console.WriteLine("Error during test, source array: " + source);
            throw;
         }
      }

      private static long[] CreatePayloadArrayLongs(int length)
      {
         Random rand = new Random(Environment.TickCount);

         long[] payload = new long[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = 128 + 1 + rand.Next(9);
         }

         return payload;
      }

      [Test]
      public void TestEncodeDecodeFloatArray25()
      {
         // float array8 less than 128 bytes
         DoEncodeDecodeFloatArrayTestImpl(25, false);
      }

      [Test]
      public void TestEncodeDecodeFloatArray50()
      {
         // float array8 greater than 128 bytes
         DoEncodeDecodeFloatArrayTestImpl(50, false);
      }

      [Test]
      public void TestEncodeDecodeFloatArray384()
      {
         // float array32
         DoEncodeDecodeFloatArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeFloatArray25FS()
      {
         // float array8 less than 128 bytes
         DoEncodeDecodeFloatArrayTestImpl(25, true);
      }

      [Test]
      public void TestEncodeDecodeFloatArray50FS()
      {
         // float array8 greater than 128 bytes
         DoEncodeDecodeFloatArrayTestImpl(50, true);
      }

      [Test]
      public void TestEncodeDecodeFloatArray384FS()
      {
         // float array32
         DoEncodeDecodeFloatArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeFloatArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         float[] source = CreatePayloadArrayFloats(count);

         try
         {
            Assert.AreEqual(count, source.Length, "Unexpected source array length");

            int encodingWidth = count < 63 ? 1 : 4; // less than 63, since each element is 4 bytes, but we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
            int arrayPayloadSize = encodingWidth + 1 + (count * 4); // variable width for element count + byte type descriptor + (number of elements * size)
            int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code +  variable width for array size + other encoded payload
            byte[] expectedEncoding = new byte[expectedEncodedArraySize];
            IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
            expectedEncodingWrapper.WriteOffset = 0;

            // Write the array encoding code, array size, and element count
            if (count < 254)
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
               expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
               expectedEncodingWrapper.WriteUnsignedByte((byte)count);
            }
            else
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
               expectedEncodingWrapper.WriteInt(arrayPayloadSize);
               expectedEncodingWrapper.WriteInt(count);
            }

            // Write the type descriptor
            expectedEncodingWrapper.WriteUnsignedByte((byte)0x72); // 'float' type descriptor code

            // Write the elements
            for (int i = 0; i < count; i++)
            {
               expectedEncodingWrapper.WriteFloat(source[i]);
            }

            Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

            // Now verify against the actual encoding of the array
            Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
            encoder.WriteArray(buffer, encoderState, source);
            Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

            byte[] actualEncoding = new byte[expectedEncodedArraySize];
            buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
            Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");

            Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

            object decoded;
            if (fromStream)
            {
               decoded = streamDecoder.ReadObject(stream, streamDecoderState);
            }
            else
            {
               decoded = decoder.ReadObject(buffer, decoderState);
            }

            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.GetType().IsArray);
            Assert.AreEqual(typeof(float), decoded.GetType().GetElementType());
            Assert.AreEqual(source, (float[])decoded, "Unexpected decoding");
         }
         catch (Exception)
         {
            Console.WriteLine("Error during test, source array: " + source);
            throw;
         }
      }

      private static float[] CreatePayloadArrayFloats(int length)
      {
         Random rand = new Random(Environment.TickCount);

         float[] payload = new float[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = 64 + 1 + rand.Next(9);
         }

         return payload;
      }

      [Test]
      public void TestEncodeDecodeDoubleArray10()
      {
         // double array8 less than 128 bytes
         DoEncodeDecodeDoubleArrayTestImpl(10, false);
      }

      [Test]
      public void TestEncodeDecodeDoubleArray25()
      {
         // double array8 greater than 128 bytes
         DoEncodeDecodeDoubleArrayTestImpl(25, false);
      }

      [Test]
      public void TestEncodeDecodeDoubleArray384()
      {
         // double array32
         DoEncodeDecodeDoubleArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeDoubleArray10FS()
      {
         // double array8 less than 128 bytes
         DoEncodeDecodeDoubleArrayTestImpl(10, true);
      }

      [Test]
      public void TestEncodeDecodeDoubleArray25FS()
      {
         // double array8 greater than 128 bytes
         DoEncodeDecodeDoubleArrayTestImpl(25, true);
      }

      [Test]
      public void TestEncodeDecodeDoubleArray384FS()
      {
         // double array32
         DoEncodeDecodeDoubleArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeDoubleArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         double[] source = CreatePayloadArrayDoubles(count);

         try
         {
            Assert.AreEqual(count, source.Length, "Unexpected source array length");

            int encodingWidth = count < 31 ? 1 : 4; // less than 31, since each element is 8 bytes, but we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
            int arrayPayloadSize = encodingWidth + 1 + (count * 8); // variable width for element count + byte type descriptor + (number of elements * size)
            int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code +  variable width for array size + other encoded payload
            byte[] expectedEncoding = new byte[expectedEncodedArraySize];
            IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
            expectedEncodingWrapper.WriteOffset = 0;

            // Write the array encoding code, array size, and element count
            if (count < 254)
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
               expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
               expectedEncodingWrapper.WriteUnsignedByte((byte)count);
            }
            else
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
               expectedEncodingWrapper.WriteInt(arrayPayloadSize);
               expectedEncodingWrapper.WriteInt(count);
            }

            // Write the type descriptor
            expectedEncodingWrapper.WriteUnsignedByte((byte)0x82); // 'double' type descriptor code

            // Write the elements
            for (int i = 0; i < count; i++)
            {
               expectedEncodingWrapper.WriteDouble(source[i]);
            }

            Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

            // Now verify against the actual encoding of the array
            Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
            encoder.WriteArray(buffer, encoderState, source);
            Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

            byte[] actualEncoding = new byte[expectedEncodedArraySize];
            buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
            Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");

            Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

            object decoded;
            if (fromStream)
            {
               decoded = streamDecoder.ReadObject(stream, streamDecoderState);
            }
            else
            {
               decoded = decoder.ReadObject(buffer, decoderState);
            }

            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.GetType().IsArray);
            Assert.AreEqual(typeof(double), decoded.GetType().GetElementType());

            Assert.AreEqual(source, (double[])decoded, "Unexpected decoding");
         }
         catch (Exception)
         {
            Console.WriteLine("Error during test, source array: " + source);
            throw;
         }
      }

      private static double[] CreatePayloadArrayDoubles(int length)
      {
         Random rand = new Random(Environment.TickCount);

         double[] payload = new double[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = 64 + 1 + rand.Next(9);
         }

         return payload;
      }

      [Test]
      public void TestEncodeDecodeCharArray25()
      {
         // char array8 less than 128 bytes
         DoEncodeDecodeCharArrayTestImpl(25, false);
      }

      [Test]
      public void TestEncodeDecodeCharArray50()
      {
         // char array8 greater than 128 bytes
         DoEncodeDecodeCharArrayTestImpl(50, false);
      }

      [Test]
      public void TestEncodeDecodeCharArray384()
      {
         // char array32
         DoEncodeDecodeCharArrayTestImpl(384, false);
      }

      [Test]
      public void TestEncodeDecodeCharArray25FS()
      {
         // char array8 less than 128 bytes
         DoEncodeDecodeCharArrayTestImpl(25, true);
      }

      [Test]
      public void TestEncodeDecodeCharArray50FS()
      {
         // char array8 greater than 128 bytes
         DoEncodeDecodeCharArrayTestImpl(50, true);
      }

      [Test]
      public void TestEncodeDecodeCharArray384FS()
      {
         // char array32
         DoEncodeDecodeCharArrayTestImpl(384, true);
      }

      private void DoEncodeDecodeCharArrayTestImpl(int count, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);
         char[] source = CreatePayloadArrayChars(count);

         try
         {
            Assert.AreEqual(count, source.Length, "Unexpected source array length");

            int encodingWidth = count < 63 ? 1 : 4; // less than 63, since each element is 4 bytes, but we also need 1 byte for element count, and (in this case) 1 byte for primitive element type constructor.
            int arrayPayloadSize = encodingWidth + 1 + (count * 4); // variable width for element count + byte type descriptor + (number of elements * size)
            int expectedEncodedArraySize = 1 + encodingWidth + arrayPayloadSize; // array type code +  variable width for array size + other encoded payload
            byte[] expectedEncoding = new byte[expectedEncodedArraySize];
            IProtonBuffer expectedEncodingWrapper = ProtonByteBufferAllocator.Instance.Wrap(expectedEncoding);
            expectedEncodingWrapper.WriteOffset = 0;

            // Write the array encoding code, array size, and element count
            if (count < 254)
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xE0); // 'array8' type descriptor code
               expectedEncodingWrapper.WriteUnsignedByte((byte)arrayPayloadSize);
               expectedEncodingWrapper.WriteUnsignedByte((byte)count);
            }
            else
            {
               expectedEncodingWrapper.WriteUnsignedByte((byte)0xF0); // 'array32' type descriptor code
               expectedEncodingWrapper.WriteInt(arrayPayloadSize);
               expectedEncodingWrapper.WriteInt(count);
            }

            // Write the type descriptor
            expectedEncodingWrapper.WriteUnsignedByte((byte)0x73); // 'char' type descriptor code

            // Write the elements
            for (int i = 0; i < count; i++)
            {
               expectedEncodingWrapper.WriteInt(source[i]); //4 byte encoding
            }

            Assert.IsFalse(expectedEncodingWrapper.IsWritable, "Should have filled expected encoding array");

            // Now verify against the actual encoding of the array
            Assert.AreEqual(0, buffer.ReadOffset, "Unexpected buffer position");
            encoder.WriteArray(buffer, encoderState, source);
            Assert.AreEqual(expectedEncodedArraySize, buffer.ReadableBytes, "Unexpected encoded payload length");

            byte[] actualEncoding = new byte[expectedEncodedArraySize];

            buffer.CopyInto(buffer.ReadOffset, actualEncoding, 0, expectedEncodedArraySize);
            Assert.IsFalse(buffer.ReadableBytes > expectedEncodedArraySize, "Should have drained the encoder buffer contents");

            Assert.AreEqual(expectedEncoding, actualEncoding, "Unexpected actual array encoding");

            object decoded;
            if (fromStream)
            {
               decoded = streamDecoder.ReadObject(stream, streamDecoderState);
            }
            else
            {
               decoded = decoder.ReadObject(buffer, decoderState);
            }

            Assert.IsNotNull(decoded);
            Assert.IsTrue(decoded.GetType().IsArray);
            Assert.AreEqual(typeof(char), decoded.GetType().GetElementType());
            Assert.AreEqual(source, (char[])decoded, "Unexpected decoding");
         }
         catch (Exception)
         {
            Console.WriteLine("Error during test, source array: " + source);
            throw;
         }
      }

      private static char[] CreatePayloadArrayChars(int length)
      {
         Random rand = new Random(Environment.TickCount);

         char[] payload = new char[length];
         for (int i = 0; i < length; i++)
         {
            payload[i] = (char)(64 + 1 + rand.Next(9));
         }

         return payload;
      }

      [Test]
      public void TestSkipValueSmallByteArray()
      {
         DoTestSkipValueOnArrayOfSize(200, false);
      }

      [Test]
      public void TestSkipValueLargeByteArray()
      {
         DoTestSkipValueOnArrayOfSize(1024, false);
      }

      [Test]
      public void TestSkipValueSmallByteArrayFromStream()
      {
         DoTestSkipValueOnArrayOfSize(200, true);
      }

      [Test]
      public void TestSkipValueLargeByteArrayFromStream()
      {
         DoTestSkipValueOnArrayOfSize(1024, true);
      }

      private void DoTestSkipValueOnArrayOfSize(int arraySize, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         Random filler = new Random(Environment.TickCount);

         byte[] bytes = new byte[arraySize];
         filler.NextBytes(bytes);

         for (int i = 0; i < 10; ++i)
         {
            encoder.WriteArray(buffer, encoderState, bytes);
         }

         byte[] expected = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

         encoder.WriteObject(buffer, encoderState, expected);

         object result;

         if (fromStream)
         {
            for (int i = 0; i < 10; ++i)
            {
               IStreamTypeDecoder typeDecoder = streamDecoder.ReadNextTypeDecoder(stream, streamDecoderState);
               Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
               typeDecoder.SkipValue(stream, streamDecoderState);
            }

            result = decoder.ReadObject(buffer, decoderState);
         }
         else
         {
            for (int i = 0; i < 10; ++i)
            {
               ITypeDecoder typeDecoder = decoder.ReadNextTypeDecoder(buffer, decoderState);
               Assert.AreEqual(typeof(Array), typeDecoder.DecodesType);
               typeDecoder.SkipValue(buffer, decoderState);
            }

            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsNotNull(result);
         Assert.IsTrue(result is byte[]);

         byte[] value = (byte[])result;
         Assert.AreEqual(expected, value);
      }

      [Test]
      public void TestArrayOfIntegers()
      {
         DoTestArrayOfIntegers(false);
      }

      [Test]
      public void TestArrayOfIntegersFromStream()
      {
         DoTestArrayOfIntegers(true);
      }

      public void DoTestArrayOfIntegers(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         const int size = 10;

         int[] source = new int[size];
         for (int i = 0; i < size; ++i)
         {
            source[i] = random.Next();
         }

         encoder.WriteArray(buffer, encoderState, source);

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
         Assert.IsTrue(result.GetType().IsArray);

         int[] array = (int[])result;
         Assert.AreEqual(size, array.Length);

         for (int i = 0; i < size; ++i)
         {
            Assert.AreEqual(source[i], array[i]);
         }
      }
   }
}