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
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Encoders.Primitives
{
   /// <summary>
   /// Type encoder that handles writing Decimal64 types
   /// </summary>
   public sealed class Decimal64TypeEncoder : AbstractPrimitiveTypeEncoder
   {
      public override Type EncodesType => typeof(Decimal64);

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         Decimal64 decimal64 = (Decimal64)value;

         buffer.EnsureWritable(sizeof(byte) + (sizeof(ulong)));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Decimal64));
         buffer.WriteUnsignedLong(decimal64.Bits);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         Decimal64[] buffers = (Decimal64[])values;

         buffer.EnsureWritable(sizeof(byte) + (sizeof(ulong) * values.Length));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Decimal64));
         foreach (Decimal64 value in values)
         {
            buffer.WriteUnsignedLong(value.Bits);
         }
      }
   }
}