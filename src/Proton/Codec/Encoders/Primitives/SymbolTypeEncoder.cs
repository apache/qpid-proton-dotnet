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
   /// Type encoder that handles writing Symbol types
   /// </summary>
   public sealed class SymbolTypeEncoder : AbstractPrimitiveTypeEncoder<Symbol>
   {
      public override void WriteType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         Symbol symbol = (Symbol)value;
         int symbolBytes = symbol.Length;

         if (symbolBytes <= 255)
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(byte) + symbolBytes);
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Sym8));
            buffer.WriteUnsignedByte(((byte)symbolBytes));
         }
         else
         {
            buffer.EnsureWritable(sizeof(byte) + sizeof(int) + symbolBytes);
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Sym32));
            buffer.WriteInt(symbolBytes);
         }

         symbol.WriteTo(buffer);
      }

      public override void WriteRawArray(IProtonBuffer buffer, IEncoderState state, Array values)
      {
         buffer.EnsureWritable(sizeof(byte) + sizeof(int) + values.Length);
         buffer.WriteUnsignedByte(((byte)EncodingCodes.Sym32));
         foreach (Symbol symbol in values)
         {
            buffer.EnsureWritable(sizeof(int) + symbol.Length);
            buffer.WriteInt(symbol.Length);
            symbol.WriteTo(buffer);
         }
      }
   }
}