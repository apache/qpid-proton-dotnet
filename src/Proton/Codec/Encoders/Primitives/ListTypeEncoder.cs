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
   /// Type encoder that handles writing List types
   /// </summary>
   public sealed class ListTypeEncoder : AbstractPrimitiveTypeEncoder<IList>
   {
      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         IList list = (IList)value;

         buffer.EnsureWritable(sizeof(byte));
         if (list.Count == 0)
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List0));
         }
         else
         {
            buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
            WriteValue(buffer, state, list);
         }
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         buffer.EnsureWritable(sizeof(byte) + (sizeof(int) * values.Length));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.List32));
         foreach (IList value in values)
         {
            WriteValue(buffer, state, (IList)value);
         }
      }

      private void WriteValue(IProtonBuffer buffer, IEncoderState state, IList value)
      {
         long startIndex = buffer.WriteOffset;

         // Reserve space for the size and write the element count
         buffer.EnsureWritable(sizeof(int) + sizeof(int));
         buffer.WriteInt(0);
         buffer.WriteInt(value.Count);

         ITypeEncoder encoder = null;

         // Write the list elements and then compute total size written, try not to lookup
         // encoders when the types in the list all match.
         foreach (object entry in value)
         {
            if (encoder == null || !encoder.EncodesType.Equals(entry.GetType()))
            {
               encoder = state.Encoder.LookupTypeEncoder(entry);
            }

            if (encoder == null)
            {
               throw new EncodeException("Cannot find encoder for type " + entry);
            }

            encoder.WriteType(buffer, state, entry);
         }

         // Move back and write the size
         long endIndex = buffer.WriteOffset;
         buffer.SetInt(startIndex, (int)(endIndex - startIndex - sizeof(int)));
      }
   }
}