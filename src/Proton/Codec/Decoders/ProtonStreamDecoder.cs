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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Codec.Decoders.Primitives;
using System.IO;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Codec.Decoders
{
   public sealed class ProtonStreamDecoder : IStreamDecoder
   {
      /// <summary>
      /// The decoders for primitives are fixed and cannot be altered by users who want
      /// to register custom decoders. The decoders created here are stateless and can be
      /// made static to reduce overhead of creating Decoder instances.
      /// </summary>
      private static readonly IPrimitiveTypeDecoder[] primitiveDecoders = new IPrimitiveTypeDecoder[256];

      static ProtonStreamDecoder()
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
      private IDictionary<object, IStreamDescribedTypeDecoder> describedTypeDecoders =
         new Dictionary<object, IStreamDescribedTypeDecoder>();

      /// <summary>
      /// Quick access to decoders that handle AMQP types like Transfer, Properties etc.
      /// </summary>
      private IStreamDescribedTypeDecoder[] amqpTypeDecoders = new IStreamDescribedTypeDecoder[256];

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

      private ProtonStreamDecoderState cachedDecoderState;

      public IStreamDecoderState NewDecoderState()
      {
         return new ProtonStreamDecoderState(this);
      }

      public IStreamDecoderState CachedDecoderState()
      {
         return cachedDecoderState ??= new ProtonStreamDecoderState(this);
      }

      public bool? ReadBoolean(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.BooleanTrue:
               return true;
            case EncodingCodes.BooleanFalse:
               return false;
            case EncodingCodes.Boolean:
               return stream.ReadByte() == 0 ? false : true;
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Boolean type but found encoding: " + encodingCode);
         }
      }

      public bool ReadBoolean(Stream stream, IStreamDecoderState state, bool defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.BooleanTrue:
               return true;
            case EncodingCodes.BooleanFalse:
               return false;
            case EncodingCodes.Boolean:
               return stream.ReadByte() == 0 ? false : true;
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Boolean type but found encoding: " + encodingCode);
         }
      }

      public sbyte? ReadByte(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Byte:
               return ProtonStreamReadUtils.ReadByte(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Byte type but found encoding: " + encodingCode);
         }
      }

      public sbyte ReadByte(Stream stream, IStreamDecoderState state, sbyte defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Byte:
               return ProtonStreamReadUtils.ReadByte(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Byte type but found encoding: " + encodingCode);
         }
      }

      public byte? ReadUnsignedByte(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.UByte:
               return ProtonStreamReadUtils.ReadUnsignedByte(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected UByte type but found encoding: " + encodingCode);
         }
      }

      public byte ReadUnsignedByte(Stream stream, IStreamDecoderState state, byte defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.UByte:
               return ProtonStreamReadUtils.ReadUnsignedByte(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected UByte type but found encoding: " + encodingCode);
         }
      }

      public char? ReadCharacter(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Char:
               return (char)ProtonStreamReadUtils.ReadInt(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Char type but found encoding: " + encodingCode);
         }
      }

      public char ReadCharacter(Stream stream, IStreamDecoderState state, char defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Char:
               return (char)ProtonStreamReadUtils.ReadInt(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Char type but found encoding: " + encodingCode);
         }
      }

      public Decimal32 ReadDecimal32(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Decimal32:
               return new Decimal32(ProtonStreamReadUtils.ReadUnsignedInt(stream));
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Decimal32 type but found encoding: " + encodingCode);
         }
      }

      public Decimal64 ReadDecimal64(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Decimal64:
               return new Decimal64(ProtonStreamReadUtils.ReadUnsignedLong(stream));
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Decimal64 type but found encoding: " + encodingCode);
         }
      }

      public Decimal128 ReadDecimal128(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Decimal128:
               return new Decimal128(ProtonStreamReadUtils.ReadUnsignedLong(stream), ProtonStreamReadUtils.ReadUnsignedLong(stream));
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Decimal128 type but found encoding: " + encodingCode);
         }
      }

      public short? ReadShort(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Short:
               return ProtonStreamReadUtils.ReadShort(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Short type but found encoding: " + encodingCode);
         }
      }

      public short ReadShort(Stream stream, IStreamDecoderState state, short defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Short:
               return ProtonStreamReadUtils.ReadShort(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Short type but found encoding: " + encodingCode);
         }
      }

      public ushort? ReadUnsignedShort(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.UShort:
               return ProtonStreamReadUtils.ReadUnsignedShort(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected UShort type but found encoding: " + encodingCode);
         }
      }

      public ushort ReadUnsignedShort(Stream stream, IStreamDecoderState state, ushort defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.UShort:
               return ProtonStreamReadUtils.ReadUnsignedShort(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected UShort type but found encoding: " + encodingCode);
         }
      }

      public int? ReadInt(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.SmallInt:
               return ProtonStreamReadUtils.ReadByte(stream);
            case EncodingCodes.Int:
               return ProtonStreamReadUtils.ReadInt(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Int type but found encoding: " + encodingCode);
         }
      }

      public int ReadInt(Stream stream, IStreamDecoderState state, int defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.SmallInt:
               return ProtonStreamReadUtils.ReadByte(stream);
            case EncodingCodes.Int:
               return ProtonStreamReadUtils.ReadInt(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Int type but found encoding: " + encodingCode);
         }
      }

      public uint? ReadUnsignedInteger(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.UInt0:
               return 0u;
            case EncodingCodes.SmallUInt:
               return ProtonStreamReadUtils.ReadUnsignedByte(stream);
            case EncodingCodes.UInt:
               return ProtonStreamReadUtils.ReadUnsignedInt(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected UInt type but found encoding: " + encodingCode);
         }
      }

      public uint ReadUnsignedInteger(Stream stream, IStreamDecoderState state, uint defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.UInt0:
               return 0u;
            case EncodingCodes.SmallUInt:
               return ProtonStreamReadUtils.ReadUnsignedByte(stream);
            case EncodingCodes.UInt:
               return ProtonStreamReadUtils.ReadUnsignedInt(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected UInt type but found encoding: " + encodingCode);
         }
      }

      public long? ReadLong(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.SmallLong:
               return ProtonStreamReadUtils.ReadByte(stream);
            case EncodingCodes.Long:
               return ProtonStreamReadUtils.ReadLong(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Long type but found encoding: " + encodingCode);
         }
      }

      public long ReadLong(Stream stream, IStreamDecoderState state, long defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.SmallLong:
               return ProtonStreamReadUtils.ReadByte(stream);
            case EncodingCodes.Long:
               return ProtonStreamReadUtils.ReadLong(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Long type but found encoding: " + encodingCode);
         }
      }

      public ulong? ReadUnsignedLong(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.ULong0:
               return 0ul;
            case EncodingCodes.SmallULong:
               return ProtonStreamReadUtils.ReadUnsignedByte(stream);
            case EncodingCodes.ULong:
               return ProtonStreamReadUtils.ReadUnsignedLong(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected ULong type but found encoding: " + encodingCode);
         }
      }

      public ulong ReadUnsignedLong(Stream stream, IStreamDecoderState state, ulong defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.ULong0:
               return 0ul;
            case EncodingCodes.SmallULong:
               return ProtonStreamReadUtils.ReadUnsignedByte(stream);
            case EncodingCodes.ULong:
               return ProtonStreamReadUtils.ReadUnsignedLong(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected ULong type but found encoding: " + encodingCode);
         }
      }

      public float? ReadFloat(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Float:
               return ProtonStreamReadUtils.ReadFloat(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Float type but found encoding: " + encodingCode);
         }
      }

      public float ReadFloat(Stream stream, IStreamDecoderState state, float defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Float:
               return ProtonStreamReadUtils.ReadFloat(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Float type but found encoding: " + encodingCode);
         }
      }

      public double? ReadDouble(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Double:
               return ProtonStreamReadUtils.ReadDouble(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Double type but found encoding: " + encodingCode);
         }
      }

      public double ReadDouble(Stream stream, IStreamDecoderState state, double defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Double:
               return ProtonStreamReadUtils.ReadDouble(stream);
            case EncodingCodes.Null:
               return defaultValue;
            default:
               throw new DecodeException("Expected Double type but found encoding: " + encodingCode);
         }
      }

      public IProtonBuffer ReadBinary(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.VBin8:
               return (IProtonBuffer)binary8Decoder.ReadValue(stream, state);
            case EncodingCodes.VBin32:
               return (IProtonBuffer)binary32Decoder.ReadValue(stream, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Binary type but found encoding: " + encodingCode);
         }
      }

      public string ReadString(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Str8:
               return (string)string8Decoder.ReadValue(stream, state);
            case EncodingCodes.Str32:
               return (string)string32Decoder.ReadValue(stream, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected string type but found encoding: " + encodingCode);
         }
      }

      public Symbol ReadSymbol(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Sym8:
               return (Symbol)symbol8Decoder.ReadValue(stream, state);
            case EncodingCodes.Sym32:
               return (Symbol)symbol32Decoder.ReadValue(stream, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected symbol type but found encoding: " + encodingCode);
         }
      }

      public string ReadSymbolAsString(Stream stream, IStreamDecoderState state)
      {
         return ReadSymbol(stream, state).ToString();
      }

      public ulong? ReadTimestamp(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Timestamp:
               return ProtonStreamReadUtils.ReadUnsignedLong(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Timestamp type but found encoding: " + encodingCode);
         }
      }

      public Guid? ReadGuid(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Uuid:
               return UuidTypeDecoder.ReadUuid(stream);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Uuid type but found encoding: " + encodingCode);
         }
      }

      public IDictionary<K, V> ReadMap<K, V>(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.Map8:
               return (IDictionary<K, V>)map8Decoder.ReadValue(stream, state);
            case EncodingCodes.Map32:
               return (IDictionary<K, V>)map32Decoder.ReadValue(stream, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Map type but found encoding: " + encodingCode);
         }
      }

      public IList<V> ReadList<V>(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.List0:
               return (IList<V>)Array.Empty<V>();
            case EncodingCodes.List8:
               return (IList<V>)list8Decoder.ReadValue(stream, state);
            case EncodingCodes.List32:
               return (IList<V>)list32Decoder.ReadValue(stream, state);
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected List type but found encoding: " + encodingCode);
         }
      }

      public object ReadObject(Stream stream, IStreamDecoderState state)
      {
         IStreamTypeDecoder decoder = ReadNextTypeDecoder(stream, state);

         if (decoder == null)
         {
            throw new DecodeException("Unknown type constructor in encoded bytes");
         }

         return decoder.ReadValue(stream, state);
      }

      public T ReadObject<T>(Stream stream, IStreamDecoderState state)
      {
         object result = ReadObject(stream, state);

         if (result.GetType().IsAssignableTo(typeof(T)))
         {
            return (T)result;
         }
         else
         {
            throw SignalUnexpectedType(result, typeof(T));
         }
      }

      public T[] ReadMultiple<T>(Stream stream, IStreamDecoderState state)
      {
         Object val = ReadObject(stream, state);

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

      public IDeliveryTag ReadDeliveryTag(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         switch (encodingCode)
         {
            case EncodingCodes.VBin8:
               return new DeliveryTag((IProtonBuffer)binary8Decoder.ReadValue(stream, state));
            case EncodingCodes.VBin32:
               return new DeliveryTag((IProtonBuffer)binary32Decoder.ReadValue(stream, state));
            case EncodingCodes.Null:
               return null;
            default:
               throw new DecodeException("Expected Binary type but found encoding: " + encodingCode);
         }
      }

      public IStreamTypeDecoder ReadNextTypeDecoder(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(stream);

         if (encodingCode == EncodingCodes.DescribedTypeIndicator)
         {
            if (stream.CanSeek)
            {
               long position = stream.Position;
               try
               {
                  ulong result = ReadUnsignedLong(stream, state) ?? 255;

                  if (result > 0 && result < Byte.MaxValue && amqpTypeDecoders[(int)result] != null)
                  {
                     return amqpTypeDecoders[(int)result];
                  }
                  else
                  {
                     stream.Seek(position, SeekOrigin.Begin);
                     return SlowReadNextTypeDecoder(stream, state);
                  }
               }
               catch (Exception)
               {
                  stream.Seek(position, SeekOrigin.Begin);
                  return SlowReadNextTypeDecoder(stream, state);
               }
            }
            else
            {
               return SlowReadNextTypeDecoder(stream, state);
            }
         }
         else
         {
            return primitiveDecoders[(byte)encodingCode];
         }
      }

      private IStreamTypeDecoder SlowReadNextTypeDecoder(Stream stream, IStreamDecoderState state)
      {
         EncodingCodes encodingCode = ProtonStreamReadUtils.ReadEncodingCode(stream);
         object descriptor;

         switch (encodingCode)
         {
            case EncodingCodes.SmallULong:
               descriptor = ProtonStreamReadUtils.ReadUnsignedByte(stream);
               break;
            case EncodingCodes.ULong:
               descriptor = ProtonStreamReadUtils.ReadUnsignedLong(stream);
               break;
            case EncodingCodes.Sym8:
               descriptor = symbol8Decoder.ReadValue(stream, state);
               break;
            case EncodingCodes.Sym32:
               descriptor = symbol32Decoder.ReadValue(stream, state);
               break;
            default:
               throw new DecodeException("Expected Descriptor type but found encoding: " + encodingCode);
         }

         IStreamTypeDecoder streamTypeDecoder = describedTypeDecoders[descriptor];
         if (streamTypeDecoder == null)
         {
            streamTypeDecoder = HandleUnknownDescribedType(descriptor);
         }

         return streamTypeDecoder;
      }

      public IStreamTypeDecoder PeekNextTypeDecoder(Stream stream, IStreamDecoderState state)
      {
         if (stream.CanSeek)
         {
            long position = stream.Position;
            try
            {
               return ReadNextTypeDecoder(stream, state);
            }
            finally
            {
               try
               {
                  stream.Seek(position, SeekOrigin.Begin);
               }
               catch (IOException e)
               {
                  throw new DecodeException("Error while reseting marked stream", e);
               }
            }
         }
         else
         {
            throw new InvalidOperationException("The provided stream doesn't support stream marks");
         }
      }

      public IStreamDecoder RegisterDescribedTypeDecoder(IStreamDescribedTypeDecoder decoder)
      {
         IStreamDescribedTypeDecoder describedTypeDecoder = decoder;

         // Cache AMQP type decoders in the quick lookup array.
         if (decoder.DescriptorCode.CompareTo((ulong)amqpTypeDecoders.Length) < 0)
         {
            amqpTypeDecoders[decoder.DescriptorCode] = decoder;
         }

         describedTypeDecoders[describedTypeDecoder.DescriptorCode] = describedTypeDecoder;
         describedTypeDecoders[describedTypeDecoder.DescriptorSymbol] = describedTypeDecoder;

         return this;
      }

      private static EncodingCodes ReadEncodingCode(Stream stream)
      {
         try
         {
            return (EncodingCodes)stream.ReadByte();
         }
         catch (IndexOutOfRangeException error)
         {
            throw new DecodeEOFException("Read of new type failed because stream exhausted.", error);
         }
      }

      private InvalidCastException SignalUnexpectedType(in object val, in Type type)
      {
         return new InvalidCastException(
            "Unexpected type " + val.GetType().Name + ". Expected " + type.Name + ".");
      }

      private IStreamTypeDecoder HandleUnknownDescribedType(in Object descriptor)
      {
         IStreamTypeDecoder typeDecoder = new UnknownDescribedTypeDecoder(descriptor);
         describedTypeDecoders.Add(descriptor, (UnknownDescribedTypeDecoder)typeDecoder);

         return typeDecoder;
      }
   }
}