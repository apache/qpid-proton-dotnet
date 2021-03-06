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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Encoders.Primitives
{
   /// <summary>
   /// Type encoder that handles writing String types
   /// </summary>
   public sealed class StringTypeEncoder : AbstractPrimitiveTypeEncoder<string>
   {
      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         string target = (string)value;

         // We are pessimistic and assume larger strings will encode
         // at the max 4 bytes per character instead of calculating
         if (target.Length > 64)
         {
            WriteString(buffer, state, target);
         }
         else
         {
            WriteSmallString(buffer, state, target);
         }
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         buffer.EnsureWritable(sizeof(byte) + sizeof(int));
         buffer.WriteUnsignedByte((byte)EncodingCodes.Str32);

         foreach (string value in values)
         {
            // Reserve space for the size
            buffer.WriteInt(0);

            long stringStart = buffer.WriteOffset;

            // Write the full string value
            state.EncodeUtf8(buffer, value);

            // Move back and write the string size
            buffer.SetInt(stringStart - sizeof(int), (int)(buffer.WriteOffset - stringStart));
         }
      }

      private static void WriteSmallString(IProtonBuffer buffer, IEncoderState state, string value)
      {
         buffer.EnsureWritable(sizeof(byte) + sizeof(byte) + value.Length);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Str8);
         buffer.WriteByte(0);

         long startIndex = buffer.WriteOffset;

         // Write the full string value
         state.EncodeUtf8(buffer, value);

         // Move back and write the size into the size slot
         buffer.SetUnsignedByte(startIndex - sizeof(byte), (byte)(buffer.WriteOffset - startIndex));
      }

      private static void WriteString(IProtonBuffer buffer, IEncoderState state, string value)
      {
         buffer.EnsureWritable(sizeof(byte) + sizeof(int) + value.Length);
         buffer.WriteUnsignedByte((byte)EncodingCodes.Str32);
         buffer.WriteInt(0);

         long startIndex = buffer.WriteOffset;

         // Write the full string value
         state.EncodeUtf8(buffer, value);

         // Move back and write the size into the size slot
         buffer.SetInt(startIndex - sizeof(int), (int)(buffer.WriteOffset - startIndex));
      }
   }
}