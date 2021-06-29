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
         primitiveDecoders[((int)EncodingCodes.BooleanTrue)] = new BooleanTrueTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.BooleanFalse)] = new BooleanFalseTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.VBin8)] = new Binary8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.VBin32)] = new Binary32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Byte)] = new ByteTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Char)] = new CharacterTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Decimal32)] = new Decimal32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Decimal64)] = new Decimal64TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Decimal128)] = new Decimal128TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Double)] = new DoubleTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Float)] = new FloatTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Null)] = new NullTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Short)] = new ShortTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.SmallInt)] = new Integer8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Int)] = new Integer32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.SmallLong)] = new Long8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Long)] = new Long32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.UByte)] = new UnsignedByteTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.UShort)] = new UnsignedShortTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.UInt0)] = new UnsignedInteger0TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.SmallUInt)] = new UnsignedInteger8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.UInt)] = new UnsignedInteger32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.ULong0)] = new UnsignedLong0TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.SmallULong)] = new UnsignedLong8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.ULong)] = new UnsignedLong64TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Str8)] = new String8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Str32)] = new String32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Sym8)] = new Symbol8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Sym32)] = new Symbol32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Uuid)] = new UuidTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Timestamp)] = new TimestampTypeDecoder();
         primitiveDecoders[((int)EncodingCodes.List0)] = new List0TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.List8)] = new List8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.List32)] = new List32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Map8)] = new Map8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Map32)] = new Map32TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Array8)] = new Array8TypeDecoder();
         primitiveDecoders[((int)EncodingCodes.Array32)] = new Array32TypeDecoder();

         // Initialize the locally used primitive type decoders for the main API
         symbol8Decoder = (Symbol8TypeDecoder)primitiveDecoders[(int)EncodingCodes.Sym8];
         symbol32Decoder = (Symbol32TypeDecoder)primitiveDecoders[(int)EncodingCodes.Sym32];
         binary8Decoder = (Binary8TypeDecoder)primitiveDecoders[(int)EncodingCodes.VBin8];
         binary32Decoder = (Binary32TypeDecoder)primitiveDecoders[(int)EncodingCodes.VBin32];
         list8Decoder = (List8TypeDecoder)primitiveDecoders[(int)EncodingCodes.List8];
         list32Decoder = (List32TypeDecoder)primitiveDecoders[(int)EncodingCodes.List32];
         map8Decoder = (Map8TypeDecoder)primitiveDecoders[(int)EncodingCodes.Map8];
         map32Decoder = (Map32TypeDecoder)primitiveDecoders[(int)EncodingCodes.Map32];
         string8Decoder = (String8TypeDecoder)primitiveDecoders[(int)EncodingCodes.Str8];
         string32Decoder = (String32TypeDecoder)primitiveDecoders[(int)EncodingCodes.Str32];
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

      // Internal Decoders used to prevent user to access Proton specific decoding methods
      private static readonly Symbol8TypeDecoder symbol8Decoder;
      private static readonly Symbol32TypeDecoder symbol32Decoder;
      private static readonly Binary8TypeDecoder binary8Decoder;
      private static readonly Binary32TypeDecoder binary32Decoder;
      private static readonly List8TypeDecoder list8Decoder;
      private static readonly List32TypeDecoder list32Decoder;
      private static readonly Map8TypeDecoder map8Decoder;
      private static readonly Map32TypeDecoder map32Decoder;
      private static readonly String8TypeDecoder string8Decoder;
      private static readonly String32TypeDecoder string32Decoder;

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
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.VBin8:
               return (IProtonBuffer)binary8Decoder.ReadValue(buffer, state);
            case EncodingCodes.VBin32:
               return (IProtonBuffer)binary32Decoder.ReadValue(buffer, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Binary type but found encoding: " + encodingCode);
         }
      }

      public string ReadString(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Str8:
               return (string)string8Decoder.ReadValue(buffer, state);
            case EncodingCodes.Str32:
               return (string)string32Decoder.ReadValue(buffer, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected string type but found encoding: " + encodingCode);
         }
      }

      public Symbol ReadSymbol(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Sym8:
               return (Symbol)symbol8Decoder.ReadValue(buffer, state);
            case EncodingCodes.Sym32:
               return (Symbol)symbol32Decoder.ReadValue(buffer, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected symbol type but found encoding: " + encodingCode);
         }
      }

      public string ReadSymbolAsString(IProtonBuffer buffer, IDecoderState state)
      {
         return ReadSymbol(buffer, state).ToString();
      }

      public ulong? ReadTimestamp(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Timestamp:
               return buffer.ReadUnsignedLong();
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Timestamp type but found encoding: " + encodingCode);
         }
      }

      public Guid? ReadGuid(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Uuid:
               return UuidTypeDecoder.ReadUuid(buffer);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Uuid type but found encoding: " + encodingCode);
         }
      }

      public IDictionary<K, V> ReadMap<K, V>(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.Map8:
               return (IDictionary<K, V>)map8Decoder.ReadValue(buffer, state);
            case EncodingCodes.Map32:
               return (IDictionary<K, V>)map32Decoder.ReadValue(buffer, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Map type but found encoding: " + encodingCode);
         }
      }

      public IList<V> ReadList<V>(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         switch (encodingCode)
         {
            case EncodingCodes.List0:
               return (IList<V>)Array.Empty<V>();
            case EncodingCodes.List8:
               return (IList<V>)list8Decoder.ReadValue(buffer, state);
            case EncodingCodes.List32:
               return (IList<V>)list32Decoder.ReadValue(buffer, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected List type but found encoding: " + encodingCode);
         }
      }

      public object ReadObject(IProtonBuffer buffer, IDecoderState state)
      {
         ITypeDecoder decoder = ReadNextTypeDecoder(buffer, state);

         if (decoder == null)
         {
            throw new DecodeException("Unknown type constructor in encoded bytes");
         }

         return decoder.ReadValue(buffer, state);
      }

      public T ReadObject<T>(IProtonBuffer buffer, IDecoderState state)
      {
         object result = ReadObject(buffer, state);

         if (result.GetType().IsAssignableTo(typeof(T)))
         {
            return (T)result;
         }
         else
         {
            throw SignalUnexpectedType(result, typeof(T));
         }
      }

      public T[] ReadMultiple<T>(IProtonBuffer buffer, IDecoderState state)
      {
         Object val = ReadObject(buffer, state);

         if (val == null)
         {
            return null;
         }
         else if (val.GetType().IsArray)
         {
            if (typeof(T).IsAssignableFrom(val.GetType()))
            {
               return (T[])val;
            }
            else
            {
               throw SignalUnexpectedType(val, typeof(T).MakeArrayType());
            }
         }
         else if (typeof(T).IsAssignableFrom(val.GetType()))
         {
            T[] array = (T[])Array.CreateInstance(typeof(T), 1);
            array[0] = (T)val;
            return array;
         }
         else
         {
            throw SignalUnexpectedType(val, typeof(T).MakeArrayType());
         }
      }

      public ITypeDecoder ReadNextTypeDecoder(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         if (encodingCode == EncodingCodes.DescribedTypeIndicator)
         {
            int readPos = buffer.ReadOffset;
            try
            {
               ulong result = ReadUnsignedLong(buffer, state) ?? Byte.MaxValue;

               if (result > 0 && result < Byte.MaxValue && amqpTypeDecoders[(int)result] != null)
               {
                  return amqpTypeDecoders[(int)result];
               }
               else
               {
                  buffer.ReadOffset = readPos;
                  return SlowReadNextTypeDecoder(buffer, state);
               }
            }
            catch (Exception)
            {
               buffer.ReadOffset = readPos;
               return SlowReadNextTypeDecoder(buffer, state);
            }
         }
         else
         {
            return primitiveDecoders[(byte)encodingCode];
         }
      }

      private ITypeDecoder SlowReadNextTypeDecoder(IProtonBuffer buffer, IDecoderState state)
      {
         Object descriptor;
         int readPos = buffer.ReadOffset;

         try
         {
            descriptor = ReadUnsignedLong(buffer, state);
         }
         catch (Exception)
         {
            buffer.ReadOffset = readPos;
            descriptor = ReadObject(buffer, state);
         }

         ITypeDecoder typeDecoder = describedTypeDecoders[descriptor];
         if (typeDecoder == null)
         {
            typeDecoder = HandleUnknownDescribedType(descriptor);
         }

         return typeDecoder;
      }

      public ITypeDecoder PeekNextTypeDecoder(IProtonBuffer buffer, IDecoderState state)
      {
         int offset = buffer.ReadOffset;
         try
         {
            return ReadNextTypeDecoder(buffer, state);
         }
         finally
         {
            buffer.ReadOffset = offset;
         }
      }

      public IDecoder RegisterDescribedTypeDecoder(IDescribedTypeDecoder decoder)
      {
         IDescribedTypeDecoder describedTypeDecoder = decoder;

         // Cache AMQP type decoders in the quick lookup array.
         if (decoder.DescriptorCode.CompareTo(amqpTypeDecoders.Length) < 0)
         {
            amqpTypeDecoders[decoder.DescriptorCode] = decoder;
         }

         describedTypeDecoders[describedTypeDecoder.DescriptorCode] = describedTypeDecoder;
         describedTypeDecoders[describedTypeDecoder.DescriptorSymbol] = describedTypeDecoder;

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

      private ITypeDecoder HandleUnknownDescribedType(in Object descriptor)
      {
         // ITypeDecoder typeDecoder = new UnknownDescribedTypeDecoder(descriptor);
         // describedTypeDecoders.Add(descriptor, (UnknownDescribedTypeDecoder)typeDecoder);

         // return typeDecoder;

         return null; // TODO
      }
   }
}