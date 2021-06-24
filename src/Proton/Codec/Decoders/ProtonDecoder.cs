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
using Apache.Qpid.Proton.Codec.Decoders.Primitives;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public sealed class ProtonDecoder : IDecoder
   {
      /// <summary>
      /// The decoders for primitives are fixed and cannot be altered by users who want
      /// to register custom decoders. The decoders created here are stateless and can be
      /// made static to reduce overhead of creating Decoder instances.
      /// </summary>
      private static readonly IPrimitiveTypeDecoder[] primitiveDecoders = new IPrimitiveTypeDecoder[256];

      static ProtonDecoder()
      {
         primitiveDecoders[((int)EncodingCodes.Boolean)] = new BooleanTypeDecoder();
      }

      /// <summary>
      /// Registry of decoders for described types which can be updated with user defined
      /// decoders as well as the default decoders.
      /// </summary>
      private IDictionary<object, IDescribedTypeDecoder> describedTypeDecoders =
         new Dictionary<object, IDescribedTypeDecoder>();

      /// <summary>
      /// Quick access to decoders that handle AMQP types like Transfer, Properties etc.
      /// </summary>
      private IDescribedTypeDecoder[] amqpTypeDecoders = new IDescribedTypeDecoder[256];

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
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.BooleanTrue:
               return true;
            case EncodingCodes.BooleanFalse:
               return false;
            case EncodingCodes.Boolean:
               return buffer.ReadByte() == 0 ? false : true;
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Boolean type but found encoding: " + encodingCode);
         }
      }

      public bool ReadBoolean(IProtonBuffer buffer, IDecoderState state, bool defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.BooleanTrue:
               return true;
            case EncodingCodes.BooleanFalse:
               return false;
            case EncodingCodes.Boolean:
               return buffer.ReadByte() == 0 ? false : true;
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Boolean type but found encoding: " + encodingCode);
         }
      }

      public sbyte? ReadByte(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Byte:
               return buffer.ReadByte();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Byte type but found encoding: " + encodingCode);
         }
      }

      public sbyte ReadByte(IProtonBuffer buffer, IDecoderState state, sbyte defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Byte:
               return buffer.ReadByte();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Byte type but found encoding: " + encodingCode);
         }
      }

      public byte? ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.UByte:
               return buffer.ReadUnsignedByte();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected UByte type but found encoding: " + encodingCode);
         }
      }

      public byte ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state, byte defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.UByte:
               return buffer.ReadUnsignedByte();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected UByte type but found encoding: " + encodingCode);
         }
      }

      public char? ReadCharacter(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Char:
               return (char)buffer.ReadInt();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Char type but found encoding: " + encodingCode);
         }
      }

      public char ReadCharacter(IProtonBuffer buffer, IDecoderState state, char defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Char:
               return (char)buffer.ReadInt();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Char type but found encoding: " + encodingCode);
         }
      }

      public Decimal32 ReadDecimal32(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Decimal32:
               return new Decimal32(buffer.ReadUnsignedInt());
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Decimal32 type but found encoding: " + encodingCode);
         }
      }

      public Decimal64 ReadDecimal64(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Decimal64:
               return new Decimal64(buffer.ReadUnsignedLong());
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Decimal64 type but found encoding: " + encodingCode);
         }
      }

      public Decimal128 ReadDecimal128(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Decimal128:
               return new Decimal128(buffer.ReadUnsignedLong(), buffer.ReadUnsignedLong());
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Decimal128 type but found encoding: " + encodingCode);
         }
      }

      public short? ReadShort(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Short:
               return buffer.ReadShort();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Short type but found encoding: " + encodingCode);
         }
      }

      public short ReadShort(IProtonBuffer buffer, IDecoderState state, short defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Short:
               return buffer.ReadShort();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Short type but found encoding: " + encodingCode);
         }
      }

      public ushort? ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.UShort:
               return buffer.ReadUnsignedShort();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected UShort type but found encoding: " + encodingCode);
         }
      }

      public ushort ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state, ushort defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.UShort:
               return buffer.ReadUnsignedShort();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected UShort type but found encoding: " + encodingCode);
         }
      }

      public int? ReadInt(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.SmallInt:
               return buffer.ReadByte();
            case EncodingCodes.Int:
               return buffer.ReadInt();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Int type but found encoding: " + encodingCode);
         }
      }

      public int ReadInt(IProtonBuffer buffer, IDecoderState state, int defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.SmallInt:
               return buffer.ReadByte();
            case EncodingCodes.Int:
               return buffer.ReadInt();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Int type but found encoding: " + encodingCode);
         }
      }

      public uint? ReadUnsignedInt(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.UInt0:
               return 0u;
            case EncodingCodes.SmallUInt:
               return buffer.ReadUnsignedByte();
            case EncodingCodes.UInt:
               return buffer.ReadUnsignedInt();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected UInt type but found encoding: " + encodingCode);
         }
      }

      public uint ReadUnsignedInt(IProtonBuffer buffer, IDecoderState state, uint defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.UInt0:
               return 0u;
            case EncodingCodes.SmallUInt:
               return buffer.ReadUnsignedByte();
            case EncodingCodes.UInt:
               return buffer.ReadUnsignedInt();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected UInt type but found encoding: " + encodingCode);
         }
      }

      public long? ReadLong(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.SmallLong:
               return buffer.ReadByte();
            case EncodingCodes.Long:
               return buffer.ReadLong();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Long type but found encoding: " + encodingCode);
         }
      }

      public long ReadLong(IProtonBuffer buffer, IDecoderState state, long defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.SmallLong:
               return buffer.ReadByte();
            case EncodingCodes.Long:
               return buffer.ReadLong();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Long type but found encoding: " + encodingCode);
         }
      }

      public ulong? ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.ULong0:
               return 0ul;
            case EncodingCodes.SmallULong:
               return buffer.ReadUnsignedByte();
            case EncodingCodes.ULong:
               return buffer.ReadUnsignedLong();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected ULong type but found encoding: " + encodingCode);
         }
      }

      public ulong ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state, ulong defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.ULong0:
               return 0ul;
            case EncodingCodes.SmallULong:
               return buffer.ReadUnsignedByte();
            case EncodingCodes.ULong:
               return buffer.ReadUnsignedLong();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected ULong type but found encoding: " + encodingCode);
         }
      }

      public float? ReadFloat(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Float:
               return buffer.ReadFloat();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Float type but found encoding: " + encodingCode);
         }
      }

      public float ReadFloat(IProtonBuffer buffer, IDecoderState state, float defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Float:
               return buffer.ReadFloat();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Float type but found encoding: " + encodingCode);
         }
      }

      public double? ReadDouble(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Double:
               return buffer.ReadDouble();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Double type but found encoding: " + encodingCode);
         }
      }

      public double ReadDouble(IProtonBuffer buffer, IDecoderState state, double defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Double:
               return buffer.ReadDouble();
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Double type but found encoding: " + encodingCode);
         }
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

      public ITypeDecoder ReadNextTypeDecoder(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public ITypeDecoder PeekNextTypeDecoder(IProtonBuffer buffer, IDecoderState state)
      {
         return null;
      }

      public IDecoder RegisterDescribedTypeDecoder(IDescribedTypeDecoder decoder)
      {
         return this;
      }

      private static EncodingCodes ReadEncodingCode(IProtonBuffer buffer)
      {
         try
         {
            return (EncodingCodes)buffer.ReadByte();
         }
         catch (IndexOutOfRangeException error)
         {
            throw new DecodeEOFException("Read of new type failed because buffer exhausted.", error);
         }
      }

      private InvalidCastException SignalUnexpectedType(in object val, in Type type)
      {
         return new InvalidCastException(
            "Unexpected type " + val.GetType().Name + ". Expected " + type.Name + ".");
      }
   }
}