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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec;
using Apache.Qpid.Proton.Test.Driver.Codec.Impl;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types.Messaging
{
   public abstract class AbstractMessageSectionMatcher
   {
      private readonly ulong numericDescriptor;
      private readonly Symbol symbolicDescriptor;

      private readonly IDictionary<object, IMatcher> fieldMatchers = new Dictionary<object, IMatcher>();
      private IDictionary receivedFields;

      private readonly bool expectTrailingBytes;

      protected AbstractMessageSectionMatcher(ulong numericDescriptor, Symbol symbolicDescriptor, bool expectTrailingBytes)
      {
         this.numericDescriptor = numericDescriptor;
         this.symbolicDescriptor = symbolicDescriptor;
         this.expectTrailingBytes = expectTrailingBytes;
      }

      protected IDictionary<object, IMatcher> FieldMatchers => fieldMatchers;

      protected IDictionary ReceivedFields => receivedFields;

      /// <summary>
      /// Decodes and verifies the payload of incoming bytes for a given message section type. The
      /// derived class must implement the verification method that inspects the section to determine
      /// if it matches against configured expectations.
      /// </summary>
      /// <param name="receivedBytes">A stream of bytes that contains encoded data</param>
      /// <returns>The number of byte read from the incoming stream</returns>
      /// <exception cref="InvalidOperationException">If the incoming stream would overflow AMQP data types</exception>
      /// <exception cref="ArgumentOutOfRangeException">If the incoming data has trailing bytes when not expected</exception>
      public long Verify(Stream receivedBytes)
      {
         long length = receivedBytes.Length;
         ICodec data = CodecFactory.Create();
         long decoded = data.Decode(receivedBytes);

         if (decoded > UInt32.MaxValue)
         {
            throw new InvalidOperationException("Decoded more bytes than Binary supports holding");
         }

         if (decoded < length && !expectTrailingBytes)
         {
            throw new ArgumentOutOfRangeException(
                "Expected to consume all bytes, but trailing bytes remain: Got " + length + ", consumed " + decoded);
         }

         IDescribedType decodedDescribedType = data.GetDescribedType();
         VerifyReceivedDescribedType(decodedDescribedType);

         return decoded;
      }

      private void VerifyReceivedDescribedType(IDescribedType decodedDescribedType)
      {
         Object descriptor = decodedDescribedType.Descriptor;
         if (!(symbolicDescriptor.Equals(descriptor) || numericDescriptor.Equals(descriptor)))
         {
            throw new ArgumentException(
                "Unexpected section type descriptor. Expected " + symbolicDescriptor +
                " or " + numericDescriptor + ", but got: " + descriptor);
         }

         VerifyReceivedDescribedObject(decodedDescribedType.Described);
      }

      /// <summary>
      /// sub-classes should implement depending on the expected content of the
      /// particular section type.
      /// </summary>
      /// <param name="describedObject">The object conveyed in the described section type</param>
      protected abstract void VerifyReceivedDescribedObject(Object describedObject);

      /// <summary>
      /// Utility method for use by sub-classes that expect field-based sections, i.e lists or maps.
      /// </summary>
      /// <param name="valueMap">Map of values</param>
      protected virtual void VerifyReceivedFields(IDictionary valueMap)
      {
         receivedFields = valueMap;

         // TODO : LOG.debug("About to check the fields of the section." + "\n  Received:" + valueMap + "\n  Expectations: " + fieldMatchers);

         foreach(KeyValuePair<object, IMatcher> entry in FieldMatchers)
         {
            IMatcher matcher = entry.Value;
            object field = entry.Key;
            object fieldValue = null;

            // Get the transmitted value if one exists otherwise matcher must allow nul to match
            if (valueMap.Contains(field))
            {
               fieldValue = valueMap[field];
            }

            MatcherAssert.AssertThat("Field " + field + " value should match", fieldValue, matcher);
         }
      }
   }
}