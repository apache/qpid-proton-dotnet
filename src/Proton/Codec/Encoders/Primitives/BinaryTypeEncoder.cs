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
   /// Type encoder that handles writing Binary types
   /// </summary>
   public sealed class BinaryTypeEncoder : AbstractPrimitiveTypeEncoder<IProtonBuffer>
   {
      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         IProtonBuffer binary = (IProtonBuffer)value;

         if (binary.ReadableBytes > byte.MaxValue)
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(uint) + binary.ReadableBytes);
            buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));
            buffer.WriteUnsignedInt((uint)binary.ReadableBytes);
         }
         else
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(byte) + binary.ReadableBytes);
            buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin8));
            buffer.WriteUnsignedByte((byte)binary.ReadableBytes);
         }

         binary.CopyInto(binary.ReadOffset, buffer, buffer.WriteOffset, binary.ReadableBytes);

         buffer.WriteOffset += binary.ReadableBytes;
      }

      public static void WriteType(IProtonBuffer buffer, IEncoderState state, byte[] value)
      {
         if (value.Length > byte.MaxValue)
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(uint) + value.Length);
            buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));
            buffer.WriteUnsignedInt((uint)value.Length);
         }
         else
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(byte) + value.Length);
            buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin8));
            buffer.WriteUnsignedByte((byte)value.Length);
         }

         buffer.WriteBytes(value);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         IProtonBuffer[] buffers = (IProtonBuffer[])values;

         buffer.EnsureWritable(sizeof(byte));
         buffer.WriteUnsignedByte(((byte)EncodingCodes.VBin32));

         foreach (IProtonBuffer value in buffers)
         {
            buffer.EnsureWritable(sizeof(uint) + value.ReadableBytes);
            buffer.WriteUnsignedInt((uint)value.ReadableBytes);

            value.CopyInto(value.ReadOffset, buffer, buffer.WriteOffset, value.ReadableBytes);
            buffer.WriteOffset += value.ReadableBytes;
         }
      }
   }
}