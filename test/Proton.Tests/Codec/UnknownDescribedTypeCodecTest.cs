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
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec.Utilities;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec
{
   [TestFixture]
   public class UnknownDescribedTypeCodecTest : CodecTestSupport
   {
      [Test]
      public void TestDecodeUnknownDescribedType()
      {
         DoTestDecodeUnknownDescribedType(false);
      }

      [Test]
      public void TestDecodeUnknownDescribedTypeFromStream()
      {
         DoTestDecodeUnknownDescribedType(true);
      }

      private void DoTestDecodeUnknownDescribedType(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         encoder.WriteObject(buffer, encoderState, NoLocalType.Instance);

         object result;
         if (fromStream)
         {
            result = streamDecoder.ReadObject(stream, streamDecoderState);
         }
         else
         {
            result = decoder.ReadObject(buffer, decoderState);
         }

         Assert.IsTrue(result is UnknownDescribedType);
         UnknownDescribedType resultTye = (UnknownDescribedType)result;
         Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
      }

      // TODO [Test]
      public void TestUnknownDescribedTypeInList()
      {
         DoTestUnknownDescribedTypeInList(false);
      }

      // TODO [Test]
      public void TestUnknownDescribedTypeInListFromStream()
      {
         DoTestUnknownDescribedTypeInList(true);
      }

      private void DoTestUnknownDescribedTypeInList(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IList<object> listOfUnknowns = new List<object>();

         listOfUnknowns.Add(NoLocalType.Instance);

         encoder.WriteList(buffer, encoderState, listOfUnknowns);

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
         Assert.IsTrue(result is IList<object>);

         IList<object> decodedList = (List<object>)result;
         Assert.AreEqual(1, decodedList.Count);

         object listEntry = decodedList[0];
         Assert.IsTrue(listEntry is UnknownDescribedType);

         UnknownDescribedType resultTye = (UnknownDescribedType)listEntry;
         Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
      }

      // TODO [Test]
      public void TestUnknownDescribedTypeInMap()
      {
         DoTestUnknownDescribedTypeInMap(false);
      }

      // TODO [Test]
      public void TestUnknownDescribedTypeInMapFromStream()
      {
         DoTestUnknownDescribedTypeInMap(true);
      }

      private void DoTestUnknownDescribedTypeInMap(bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         IDictionary<object, object> mapOfUnknowns = new Dictionary<object, object>();

         mapOfUnknowns.Add(NoLocalType.Instance.Descriptor, NoLocalType.Instance);

         encoder.WriteMap(buffer, encoderState, mapOfUnknowns);

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
         Assert.IsTrue(result is IDictionary<object, object>);

         IDictionary<object, object> decodedMap = (IDictionary<object, object>)result;
         Assert.AreEqual(1, decodedMap.Count);

         object mapEntry = decodedMap[NoLocalType.Instance.Descriptor];
         Assert.IsTrue(mapEntry is UnknownDescribedType);

         UnknownDescribedType resultTye = (UnknownDescribedType)mapEntry;
         Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
      }

      // TODO [Test]
      public void testUnknownDescribedTypeInArray()
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();

         NoLocalType[] arrayOfUnknown = new NoLocalType[1];

         arrayOfUnknown[0] = NoLocalType.Instance;

         try
         {
            encoder.WriteArray(buffer, encoderState, arrayOfUnknown);
            Assert.Fail("Should not be able to write an array of unregistered described type");
         }
         catch (ArgumentException) { }

         try
         {
            encoder.WriteObject(buffer, encoderState, arrayOfUnknown);
            Assert.Fail("Should not be able to write an array of unregistered described type");
         }
         catch (ArgumentException) { }
      }

      // TODO [Test]
      public void TestDecodeSmallSeriesOfUnknownDescribedTypes()
      {
         DoTestDecodeUnknownDescribedTypeSeries(SmallSize, false);
      }

      // TODO [Test]
      public void TestDecodeLargeSeriesOfUnknownDescribedTypes()
      {
         DoTestDecodeUnknownDescribedTypeSeries(LargeSize, false);
      }

      // TODO [Test]
      public void TestDecodeSmallSeriesOfUnknownDescribedTypesFromStream()
      {
         DoTestDecodeUnknownDescribedTypeSeries(SmallSize, true);
      }

      // TODO [Test]
      public void TestDecodeLargeSeriesOfUnknownDescribedTypesFromStream()
      {
         DoTestDecodeUnknownDescribedTypeSeries(LargeSize, true);
      }

      private void DoTestDecodeUnknownDescribedTypeSeries(int size, bool fromStream)
      {
         IProtonBuffer buffer = ProtonByteBufferAllocator.Instance.Allocate();
         Stream stream = new ProtonBufferInputStream(buffer);

         for (int i = 0; i < size; ++i)
         {
            encoder.WriteObject(buffer, encoderState, NoLocalType.Instance);
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
            Assert.IsTrue(result is UnknownDescribedType);

            UnknownDescribedType resultTye = (UnknownDescribedType)result;
            Assert.AreEqual(NoLocalType.Instance.Descriptor, resultTye.Descriptor);
         }
      }
   }
}