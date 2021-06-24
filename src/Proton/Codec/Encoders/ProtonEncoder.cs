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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Codec.Encoders
{
   public sealed class ProtonEncoder : IEncoder
   {
      // The encoders for primitives are fixed and cannot be altered by users who want
      // to register custom encoders, these encoders are stateless so they can be safely
      // made static to reduce overhead of creating and destroying this type.
      // TODO - Create static type encoders

      private ProtonEncoderState cachedEncoderState;

      private readonly IDictionary<Type, ITypeEncoder> typeEncoders = new Dictionary<Type, ITypeEncoder>();

      public IEncoderState NewEncoderState()
      {
         return new ProtonEncoderState(this);
      }

      public IEncoderState CachedEncoderState()
      {
         return cachedEncoderState ??= new ProtonEncoderState(this);
      }

      public void WriteNull(IProtonBuffer buffer, IEncoderState state)
      {

      }

      public void WriteBoolean(IProtonBuffer buffer, IEncoderState state, bool value)
      {

      }

      public void WriteUnsignedByte(IProtonBuffer buffer, IEncoderState state, byte value)
      {

      }

      public void WriteUnsignedShort(IProtonBuffer buffer, IEncoderState state, ushort value)
      {

      }

      public void WriteUnsignedInteger(IProtonBuffer buffer, IEncoderState state, uint value)
      {

      }

      public void WriteUnsignedLong(IProtonBuffer buffer, IEncoderState state, ulong value)
      {

      }

      public void WriteByte(IProtonBuffer buffer, IEncoderState state, byte value)
      {

      }

      public void WriteShort(IProtonBuffer buffer, IEncoderState state, short value)
      {

      }

      public void WriteInteger(IProtonBuffer buffer, IEncoderState state, int value)
      {

      }

      public void WriteLong(IProtonBuffer buffer, IEncoderState state, long value)
      {

      }

      public void WriteFloat(IProtonBuffer buffer, IEncoderState state, float value)
      {

      }

      public void WriteDouble(IProtonBuffer buffer, IEncoderState state, double value)
      {

      }

      public void WriteDecimal32(IProtonBuffer buffer, IEncoderState state, Decimal32 value)
      {

      }

      public void WriteDecimal64(IProtonBuffer buffer, IEncoderState state, Decimal64 value)
      {

      }

      public void WriteDecimal128(IProtonBuffer buffer, IEncoderState state, Decimal128 value)
      {

      }

      public void WriteCharacter(IProtonBuffer buffer, IEncoderState state, char value)
      {

      }

      public void WriteTimestamp(IProtonBuffer buffer, IEncoderState state, long value)
      {

      }

      public void WriteGuid(IProtonBuffer buffer, IEncoderState state, Guid value)
      {

      }

      public void WriteBinary(IProtonBuffer buffer, IEncoderState state, IProtonBuffer value)
      {

      }

      public void WriteString(IProtonBuffer buffer, IEncoderState state, String value)
      {

      }

      public void WriteSymbol(IProtonBuffer buffer, IEncoderState state, Symbol value)
      {

      }

      public void WriteSymbol(IProtonBuffer buffer, IEncoderState state, String value)
      {

      }

      public void WriteList<T>(IProtonBuffer buffer, IEncoderState state, IList<T> value)
      {

      }

      public void WriteMap<K, V>(IProtonBuffer buffer, IEncoderState state, IDictionary<K, V> value)
      {

      }

      public void WriteDescribedType(IProtonBuffer buffer, IEncoderState state, IDescribedType value)
      {

      }

      public void WriteObject(IProtonBuffer buffer, IEncoderState state, Object value)
      {

      }

      public void WriteArray(IProtonBuffer buffer, IEncoderState state, object[] value)
      {

      }

      public IEncoder RegisterDescribedTypeEncoder(IDescribedTypeEncoder encoder)
      {
         return this;
      }

      public ITypeEncoder LookupTypeEncoder<V>(Object value)
      {
         return null;
      }

      public ITypeEncoder LookupTypeEncoder<V>(Type typeClass)
      {
         return null;
      }
   }
}