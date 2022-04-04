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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Encoders.Primitives
{
   /// <summary>
   /// Type encoder that handles writing Map types
   /// </summary>
   public sealed class MapTypeEncoder : AbstractPrimitiveTypeEncoder<IDictionary>
   {
      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));
         WriteValue(buffer, state, (IDictionary)value);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Map32));
         foreach (IDictionary value in values)
         {
            WriteValue(buffer, state, (IDictionary)value);
         }
      }

      private static void WriteValue(IProtonBuffer buffer, IEncoderState state, IDictionary value)
      {
         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the element count
         buffer.EnsureWritable(sizeof(long));
         buffer.WriteInt(0);
         buffer.WriteInt(value.Count * 2); // Map encoding count includes both key and value

         // Write the list elements and then compute total size written.
         foreach (DictionaryEntry entry in value)
         {
            object entryKey = entry.Key;
            object entryValue = entry.Value;

            ITypeEncoder keyEncoder = state.Encoder.LookupTypeEncoder(entryKey);
            if (keyEncoder == null)
            {
               throw new EncodeException("Cannot find encoder for type " + entryKey);
            }

            keyEncoder.WriteType(buffer, state, entryKey);

            ITypeEncoder valueEncoder = state.Encoder.LookupTypeEncoder(entryValue);
            if (valueEncoder == null)
            {
               throw new EncodeException("Cannot find encoder for type " + entryValue);
            }

            valueEncoder.WriteType(buffer, state, entryValue);
         }

         // Move back and write the size
         long endIndex = buffer.WriteOffset;
         buffer.SetInt(startIndex, (int)(endIndex - startIndex - sizeof(int)));
      }
   }
}