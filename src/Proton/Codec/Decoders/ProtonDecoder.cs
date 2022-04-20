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
         primitiveDecoders[(int)EncodingCodes.Boolean] = new BooleanTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.BooleanTrue] = new BooleanTrueTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.BooleanFalse] = new BooleanFalseTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.VBin8] = new Binary8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.VBin32] = new Binary32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Byte] = new ByteTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Char] = new CharacterTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Decimal32] = new Decimal32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Decimal64] = new Decimal64TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Decimal128] = new Decimal128TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Double] = new DoubleTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Float] = new FloatTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Null] = new NullTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Short] = new ShortTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.SmallInt] = new Integer8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Int] = new Integer32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.SmallLong] = new Long8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Long] = new Long32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.UByte] = new UnsignedByteTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.UShort] = new UnsignedShortTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.UInt0] = new UnsignedInteger0TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.SmallUInt] = new UnsignedInteger8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.UInt] = new UnsignedInteger32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.ULong0] = new UnsignedLong0TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.SmallULong] = new UnsignedLong8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.ULong] = new UnsignedLong64TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Str8] = new String8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Str32] = new String32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Sym8] = new Symbol8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Sym32] = new Symbol32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Uuid] = new UuidTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Timestamp] = new TimestampTypeDecoder();
         primitiveDecoders[(int)EncodingCodes.List0] = new List0TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.List8] = new List8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.List32] = new List32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Map8] = new Map8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Map32] = new Map32TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Array8] = new Array8TypeDecoder();
         primitiveDecoders[(int)EncodingCodes.Array32] = new Array32TypeDecoder();

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
      private readonly IDictionary<object, IDescribedTypeDecoder> describedTypeDecoders =
         new Dictionary<object, IDescribedTypeDecoder>();

      /// <summary>
      /// Quick access to decoders that handle AMQP types like Transfer, Properties etc.
      /// </summary>
      private readonly IDescribedTypeDecoder[] amqpTypeDecoders = new IDescribedTypeDecoder[256];

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

      public IDecoderState CachedDecoderState => cachedDecoderState ??= new ProtonDecoderState(this);

      public bool? ReadBoolean(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.BooleanTrue => true,
            EncodingCodes.BooleanFalse => false,
            EncodingCodes.Boolean => buffer.ReadByte() != 0,
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Boolean type but found encoding: " + encodingCode),
         };
      }

      public bool ReadBoolean(IProtonBuffer buffer, IDecoderState state, bool defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.BooleanTrue => true,
            EncodingCodes.BooleanFalse => false,
            EncodingCodes.Boolean => buffer.ReadByte() != 0,
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Boolean type but found encoding: " + encodingCode),
         };
      }

      public sbyte? ReadByte(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Byte => buffer.ReadByte(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Byte type but found encoding: " + encodingCode),
         };
      }

      public sbyte ReadByte(IProtonBuffer buffer, IDecoderState state, sbyte defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Byte => buffer.ReadByte(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Byte type but found encoding: " + encodingCode),
         };
      }

      public byte? ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.UByte => buffer.ReadUnsignedByte(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Unsigned Byte type but found encoding: " + encodingCode),
         };
      }

      public byte ReadUnsignedByte(IProtonBuffer buffer, IDecoderState state, byte defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.UByte => buffer.ReadUnsignedByte(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Unsigned Byte type but found encoding: " + encodingCode),
         };
      }

      public char? ReadCharacter(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Char => (char)buffer.ReadInt(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Char type but found encoding: " + encodingCode),
         };
      }

      public char ReadCharacter(IProtonBuffer buffer, IDecoderState state, char defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Char => (char)buffer.ReadInt(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Char type but found encoding: " + encodingCode),
         };
      }

      public Decimal32 ReadDecimal32(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Decimal32 => new Decimal32(buffer.ReadUnsignedInt()),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Decimal32 type but found encoding: " + encodingCode),
         };
      }

      public Decimal64 ReadDecimal64(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Decimal64 => new Decimal64(buffer.ReadUnsignedLong()),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Decimal64 type but found encoding: " + encodingCode),
         };
      }

      public Decimal128 ReadDecimal128(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Decimal128 => new Decimal128(buffer.ReadUnsignedLong(), buffer.ReadUnsignedLong()),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Decimal128 type but found encoding: " + encodingCode),
         };
      }

      public short? ReadShort(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Short => buffer.ReadShort(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Short type but found encoding: " + encodingCode),
         };
      }

      public short ReadShort(IProtonBuffer buffer, IDecoderState state, short defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Short => buffer.ReadShort(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Short type but found encoding: " + encodingCode),
         };
      }

      public ushort? ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.UShort => buffer.ReadUnsignedShort(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Unsigned Short type but found encoding: " + encodingCode),
         };
      }

      public ushort ReadUnsignedShort(IProtonBuffer buffer, IDecoderState state, ushort defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.UShort => buffer.ReadUnsignedShort(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Unsigned Short type but found encoding: " + encodingCode),
         };
      }

      public int? ReadInteger(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.SmallInt => buffer.ReadByte(),
            EncodingCodes.Int => buffer.ReadInt(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Integer type but found encoding: " + encodingCode),
         };
      }

      public int ReadInteger(IProtonBuffer buffer, IDecoderState state, int defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.SmallInt => buffer.ReadByte(),
            EncodingCodes.Int => buffer.ReadInt(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Integer type but found encoding: " + encodingCode),
         };
      }

      public uint? ReadUnsignedInteger(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.UInt0 => 0u,
            EncodingCodes.SmallUInt => buffer.ReadUnsignedByte(),
            EncodingCodes.UInt => buffer.ReadUnsignedInt(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Unsigned Integer type but found encoding: " + encodingCode),
         };
      }

      public uint ReadUnsignedInteger(IProtonBuffer buffer, IDecoderState state, uint defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.UInt0 => 0u,
            EncodingCodes.SmallUInt => buffer.ReadUnsignedByte(),
            EncodingCodes.UInt => buffer.ReadUnsignedInt(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Unsigned Integer type but found encoding: " + encodingCode),
         };
      }

      public long? ReadLong(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.SmallLong => buffer.ReadByte(),
            EncodingCodes.Long => buffer.ReadLong(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Long type but found encoding: " + encodingCode),
         };
      }

      public long ReadLong(IProtonBuffer buffer, IDecoderState state, long defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.SmallLong => buffer.ReadByte(),
            EncodingCodes.Long => buffer.ReadLong(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Long type but found encoding: " + encodingCode),
         };
      }

      public ulong? ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.ULong0 => 0ul,
            EncodingCodes.SmallULong => buffer.ReadUnsignedByte(),
            EncodingCodes.ULong => buffer.ReadUnsignedLong(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Unsigned Long type but found encoding: " + encodingCode),
         };
      }

      public ulong ReadUnsignedLong(IProtonBuffer buffer, IDecoderState state, ulong defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.ULong0 => 0ul,
            EncodingCodes.SmallULong => buffer.ReadUnsignedByte(),
            EncodingCodes.ULong => buffer.ReadUnsignedLong(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Unsigned Long type but found encoding: " + encodingCode),
         };
      }

      public float? ReadFloat(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Float => buffer.ReadFloat(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Float type but found encoding: " + encodingCode),
         };
      }

      public float ReadFloat(IProtonBuffer buffer, IDecoderState state, float defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Float => buffer.ReadFloat(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Float type but found encoding: " + encodingCode),
         };
      }

      public double? ReadDouble(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Double => buffer.ReadDouble(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Double type but found encoding: " + encodingCode),
         };
      }

      public double ReadDouble(IProtonBuffer buffer, IDecoderState state, double defaultValue)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Double => buffer.ReadDouble(),
            EncodingCodes.Null => defaultValue,
            _ => throw new DecodeException("Expected Double type but found encoding: " + encodingCode),
         };
      }

      public IProtonBuffer ReadBinary(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.VBin8 => (IProtonBuffer)binary8Decoder.ReadValue(buffer, state),
            EncodingCodes.VBin32 => (IProtonBuffer)binary32Decoder.ReadValue(buffer, state),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Binary type but found encoding: " + encodingCode),
         };
      }

      public string ReadString(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Str8 => (string)string8Decoder.ReadValue(buffer, state),
            EncodingCodes.Str32 => (string)string32Decoder.ReadValue(buffer, state),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected String type but found encoding: " + encodingCode),
         };
      }

      public Symbol ReadSymbol(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Sym8 => symbol8Decoder.ReadValue(buffer, state),
            EncodingCodes.Sym32 => symbol32Decoder.ReadValue(buffer, state),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Symbol type but found encoding: " + encodingCode),
         };
      }

      public string ReadSymbolAsString(IProtonBuffer buffer, IDecoderState state)
      {
         return ReadSymbol(buffer, state)?.ToString();
      }

      public ulong? ReadTimestamp(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Timestamp => buffer.ReadUnsignedLong(),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Timestamp type but found encoding: " + encodingCode),
         };
      }

      public Guid? ReadGuid(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Uuid => (Guid?)UuidTypeDecoder.ReadUuid(buffer),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Uuid type but found encoding: " + encodingCode),
         };
      }

      public IDictionary<K, V> ReadMap<K, V>(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.Map8 => map8Decoder.ReadMap<K, V>(buffer, state),
            EncodingCodes.Map32 => map32Decoder.ReadMap<K, V>(buffer, state),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Map type but found encoding: " + encodingCode),
         };
      }

      public IList<T> ReadList<T>(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.List0 => Array.Empty<T>(),
            EncodingCodes.List8 => list8Decoder.ReadList<T>(buffer, state),
            EncodingCodes.List32 => list32Decoder.ReadList<T>(buffer, state),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected List type but found encoding: " + encodingCode),
         };
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

         if (result == null)
         {
            if (typeof(T).IsValueType)
            {
               throw SignalUnexpectedType(typeof(T));
            }
            else
            {
               return default;
            }
         }
         else if (result.GetType().IsAssignableTo(typeof(T)))
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
         object val = ReadObject(buffer, state);

         if (val == null)
         {
            return null;
         }
         else if (val.GetType().IsArray)
         {
            if (typeof(T).IsAssignableFrom(val.GetType().GetElementType()))
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

      public IDeliveryTag ReadDeliveryTag(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         return encodingCode switch
         {
            EncodingCodes.VBin8 => new DeliveryTag((IProtonBuffer)binary8Decoder.ReadValue(buffer, state)),
            EncodingCodes.VBin32 => new DeliveryTag((IProtonBuffer)binary32Decoder.ReadValue(buffer, state)),
            EncodingCodes.Null => null,
            _ => throw new DecodeException("Expected Binary type but found encoding: " + encodingCode),
         };
      }

      public ITypeDecoder ReadNextTypeDecoder(IProtonBuffer buffer, IDecoderState state)
      {
         EncodingCodes encodingCode = ReadEncodingCode(buffer);

         if (encodingCode == EncodingCodes.DescribedTypeIndicator)
         {
            long readPos = buffer.ReadOffset;
            try
            {
               ulong result = ReadUnsignedLong(buffer, state) ?? byte.MaxValue;

               if (result > 0 && result < byte.MaxValue && amqpTypeDecoders[(int)result] != null)
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
         object descriptor;
         long readPos = buffer.ReadOffset;

         try
         {
            descriptor = ReadUnsignedLong(buffer, state);
         }
         catch (Exception)
         {
            buffer.ReadOffset = readPos;
            descriptor = ReadObject(buffer, state);
         }

         if (!describedTypeDecoders.TryGetValue(descriptor, out IDescribedTypeDecoder typeDecoder))
         {
            typeDecoder = HandleUnknownDescribedType(descriptor);
         }

         return typeDecoder;
      }

      public ITypeDecoder PeekNextTypeDecoder(IProtonBuffer buffer, IDecoderState state)
      {
         long offset = buffer.ReadOffset;
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
         if (decoder.DescriptorCode.CompareTo((ulong)amqpTypeDecoders.Length) < 0)
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

      private static InvalidCastException SignalUnexpectedType(in Type type)
      {
         return new InvalidCastException(
            "Unexpected null decoding, Expected " + type.Name + ".");
      }

      private static InvalidCastException SignalUnexpectedType(in object val, in Type type)
      {
         return new InvalidCastException(
            "Unexpected type " + val.GetType().Name + ". Expected " + type.Name + ".");
      }

      private IDescribedTypeDecoder HandleUnknownDescribedType(in object descriptor)
      {
         UnknownDescribedTypeDecoder typeDecoder = new(descriptor);
         describedTypeDecoders.Add(descriptor, typeDecoder);

         return typeDecoder;
      }
   }
}