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

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public sealed class ProtonDecoder : IDecoder
   {
      private ProtonDecoderState cachedDecoderState;

      public IDecoderState NewDecoderState()
      {
         return new ProtonDecoderState(this);
      }

      public IDecoderState CachedDecoderState()
      {
         return cachedDecoderState ??= new ProtonDecoderState(this);
      }

      public bool? ReadBoolean(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public bool ReadBoolean(IProtonBuffer buffer, IDecoderState state, bool defaultValue)
      {
         return defaultValue;
      }

      public sbyte? ReadByte(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public sbyte ReadByte(IProtonBuffer buffer, IDecoderState state, sbyte defaultValue)
      {
         return defaultValue;
      }

      public byte? ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public byte ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state, byte defaultValue)
      {
         return defaultValue;
      }

      public char? ReadCharacter(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public char ReadCharacter(IProtonBuffer buffer, IDecoderState state, char defaultValue)
      {
         return defaultValue;
      }

      public Decimal32 ReadDecimal32(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public Decimal64 ReadDecimal64(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public Decimal128 ReadDecimal128(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }
 
      public short? ReadShort(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public short ReadShort(IProtonBuffer buffer, IDecoderState state, short defaultValue)
      {
         return defaultValue;
      }

      public ushort? ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public ushort ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state, ushort defaultValue)
      {
         return defaultValue;
      }

      public int? ReadInt(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public int ReadInt(IProtonBuffer buffer, IDecoderState state, int defaultValue)
      {
         return defaultValue;
      }

      public uint? ReadUnsignedInt(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public uint ReadUnsignedInt(IProtonBuffer buffer, IDecoderState state, uint defaultValue)
      {
         return defaultValue;
      }

      public long? ReadLong(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public long ReadLong(IProtonBuffer buffer, IDecoderState state, long defaultValue)
      {
         return defaultValue;
      }

      public ulong? ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public ulong ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state, ulong defaultValue)
      {
         return defaultValue;
      }      

      public float? ReadFloat(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public float ReadFloat(IProtonBuffer buffer, IDecoderState state, float defaultValue)
      {
         return defaultValue;
      }      

      public double? ReadDouble(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public double ReadDouble(IProtonBuffer buffer, IDecoderState state, double defaultValue)
      {
         return defaultValue;
      }      

      public IProtonBuffer ReadBinary(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public string ReadString(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public Symbol ReadSymbol(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public string ReadSymbolAsString(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public ulong? ReadTimestamp(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public Guid? ReadGuid(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public object ReadObject(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public T ReadObject<T>(IProtonBuffer buffer, IDecoderState state, Type type)
      {
         return default(T);
      }

      public T[] ReadMultiple<T>(IProtonBuffer buffer, IDecoderState state, Type type)
      {
         return default(T[]);
      }

      public IDictionary<K, V> ReadMap<K, V>(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public IList<V> ReadList<V>(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public ITypeDecoder<T> ReadNextTypeDecoder<T>(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public ITypeDecoder<T> PeekNextTypeDecoder<T>(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public IDecoder RegisterDescribedTypeDecoder<V>(IDescribedTypeDecoder<V> decoder)
      {
         return this;
      }
  } 
}