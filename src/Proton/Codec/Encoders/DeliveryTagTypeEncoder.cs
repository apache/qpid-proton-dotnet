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

namespace Apache.Qpid.Proton.Codec.Encoders
{
   public sealed class DeliveryTagTypeEncoder : ITypeEncoder
   {
      public Type EncodesType => typeof(IDeliveryTag);

      public static bool IsArrayType => false;

      public void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         IDeliveryTag tag = (IDeliveryTag)value;
         int tagLength = tag.Length;

         if (tagLength > 255)
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.VBin32);
            buffer.WriteInt(tagLength);
         }
         else
         {
            buffer.WriteUnsignedByte((byte)EncodingCodes.VBin8);
            buffer.WriteUnsignedByte((byte) tagLength);
         }

         tag.WriteTo(buffer);
      }

      public void WriteArray(IProtonBuffer buffer, IEncoderState state, Array value)
      {
         throw new NotImplementedException("Cannot encode delivery tags to arrays");
      }

      public void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         throw new NotImplementedException("Cannot encode delivery tags to arrays");
      }
   }
}