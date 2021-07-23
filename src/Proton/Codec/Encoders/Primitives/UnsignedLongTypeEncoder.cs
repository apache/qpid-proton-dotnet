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
   /// Type encoder that handles writing UnsignedLong types
   /// </summary>
   public sealed class UnsignedLongTypeEncoder : AbstractPrimitiveTypeEncoder<ulong>
   {
      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         ulong target = (ulong)value;

         if (target == 0)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong0));
         }
         else if (target <= 255)
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallULong));
            buffer.WriteUnsignedByte((byte)target);
         }
         else
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(ulong));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
            buffer.WriteUnsignedLong((ulong)value);
         }
      }

      public override void WriteArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         if (values.GetType().GetElementType() != typeof(ulong))
         {
            throw new EncodeException("Unsigned Long encoder given array of incorrect type:" + values.GetType());
         }

         if (values.Length < 32)
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
         buffer.EnsureWritable(sizeof(byte) + (sizeof(ulong) * values.Length));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.ULong));
         foreach (ulong value in values)
         {
            buffer.WriteUnsignedLong(value);
         }
      }
   }
}