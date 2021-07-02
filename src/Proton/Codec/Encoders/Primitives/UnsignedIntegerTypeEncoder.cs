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
   /// Type encoder that handles writing UnsignedInteger types
   /// </summary>
   public sealed class UnsignedIntegerTypeEncoder : AbstractPrimitiveTypeEncoder
   {
      public override Type EncodesType => typeof(uint);

      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         uint target = (uint)value;

         if (target == 0)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt0));
         }
         else if (target <= 255)
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.SmallUInt));
            buffer.WriteUnsignedByte((byte)target);
         }
         else
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(uint));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
            buffer.WriteUnsignedInt((uint)value);
         }
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, object[] values)
      {
         buffer.EnsureWritable(sizeof(byte) + (sizeof(uint) * values.Length));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.UInt));
         foreach (uint value in values)
         {
            buffer.WriteUnsignedInt(value);
         }
      }
   }
}