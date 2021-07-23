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
   /// Type encoder that handles writing UnsignedByte types
   /// </summary>
   public sealed class UnsignedByteTypeEncoder : AbstractPrimitiveTypeEncoder<byte>
   {
      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         buffer.EnsureWritable(sizeof(byte) + sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         buffer.WriteUnsignedByte((byte)value);
      }

      public override void WriteArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         if (values.GetType().GetElementType() != typeof(byte))
         {
            throw new EncodeException("Unsigned Byte encoder given array of incorrect type:" + values.GetType());
         }

         if (values.Length < 254)
         {
            WriteAsArray8(buffer, state, values);
         }
         else
         {
            WriteAsArray32(buffer, state, values);
         }
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         buffer.EnsureWritable(sizeof(byte) + (sizeof(byte) * values.Length));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UByte));
         foreach (byte value in values)
         {
            buffer.WriteUnsignedByte(value);
         }
      }
   }
}