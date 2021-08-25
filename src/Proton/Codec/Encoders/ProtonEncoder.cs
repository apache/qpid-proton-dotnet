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
using System.Collections;
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Codec.Encoders.Primitives;

namespace Apache.Qpid.Proton.Codec.Encoders
{
   public sealed class ProtonEncoder : IEncoder
   {
      // The encoders for primitives are fixed and cannot be altered by users who want
      // to register custom encoders, these encoders are stateless so they can be safely
      // made static to reduce overhead of creating and destroying this type.
      // The encoders for primitives are fixed and cannot be altered by users who want
      // to register custom encoders, these encoders are stateless so they can be safely
      // made static to reduce overhead of creating and destroying this type.
      private static readonly ArrayTypeEncoder arrayEncoder = new ArrayTypeEncoder();
      private static readonly BinaryTypeEncoder binaryEncoder = new BinaryTypeEncoder();
      private static readonly BooleanTypeEncoder booleanEncoder = new BooleanTypeEncoder();
      private static readonly ByteTypeEncoder sbyteEncoder = new ByteTypeEncoder();
      private static readonly CharacterTypeEncoder charEncoder = new CharacterTypeEncoder();
      private static readonly Decimal32TypeEncoder decimal32Encoder = new Decimal32TypeEncoder();
      private static readonly Decimal64TypeEncoder decimal64Encoder = new Decimal64TypeEncoder();
      private static readonly Decimal128TypeEncoder decimal128Encoder = new Decimal128TypeEncoder();
      private static readonly DoubleTypeEncoder doubleEncoder = new DoubleTypeEncoder();
      private static readonly FloatTypeEncoder floatEncoder = new FloatTypeEncoder();
      private static readonly IntegerTypeEncoder integerEncoder = new IntegerTypeEncoder();
      private static readonly ListTypeEncoder listEncoder = new ListTypeEncoder();
      private static readonly LongTypeEncoder longEncoder = new LongTypeEncoder();
      private static readonly MapTypeEncoder mapEncoder = new MapTypeEncoder();
      private static readonly NullTypeEncoder nullEncoder = new NullTypeEncoder();
      private static readonly ShortTypeEncoder shortEncoder = new ShortTypeEncoder();
      private static readonly StringTypeEncoder stringEncoder = new StringTypeEncoder();
      private static readonly SymbolTypeEncoder symbolEncoder = new SymbolTypeEncoder();
      private static readonly TimestampTypeEncoder timestampEncoder = new TimestampTypeEncoder();
      private static readonly UnknownDescribedTypeEncoder unknownTypeEncoder = new UnknownDescribedTypeEncoder();
      private static readonly UuidTypeEncoder uuidEncoder = new UuidTypeEncoder();
      private static readonly UnsignedByteTypeEncoder byteEncoder = new UnsignedByteTypeEncoder();
      private static readonly UnsignedShortTypeEncoder ushortEncoder = new UnsignedShortTypeEncoder();
      private static readonly UnsignedIntegerTypeEncoder uintEncoder = new UnsignedIntegerTypeEncoder();
      private static readonly UnsignedLongTypeEncoder ulongEncoder = new UnsignedLongTypeEncoder();
      private static readonly DeliveryTagTypeEncoder deliveryTagEncoder = new DeliveryTagTypeEncoder();

      private ProtonEncoderState cachedEncoderState;

      private readonly IDictionary<Type, ITypeEncoder> typeEncoders = new Dictionary<Type, ITypeEncoder>()
      {
         [arrayEncoder.EncodesType] = arrayEncoder,
         [binaryEncoder.EncodesType] = binaryEncoder,
         [booleanEncoder.EncodesType] = booleanEncoder,
         [sbyteEncoder.EncodesType] = sbyteEncoder,
         [charEncoder.EncodesType] = charEncoder,
         [decimal32Encoder.EncodesType] = decimal32Encoder,
         [decimal64Encoder.EncodesType] = decimal64Encoder,
         [decimal128Encoder.EncodesType] = decimal128Encoder,
         [doubleEncoder.EncodesType] = doubleEncoder,
         [floatEncoder.EncodesType] = floatEncoder,
         [integerEncoder.EncodesType] = integerEncoder,
         [listEncoder.EncodesType] = listEncoder,
         [longEncoder.EncodesType] = longEncoder,
         [mapEncoder.EncodesType] = mapEncoder,
         [nullEncoder.EncodesType] = nullEncoder,
         [shortEncoder.EncodesType] = shortEncoder,
         [stringEncoder.EncodesType] = stringEncoder,
         [symbolEncoder.EncodesType] = symbolEncoder,
         [timestampEncoder.EncodesType] = timestampEncoder,
         [unknownTypeEncoder.EncodesType] = unknownTypeEncoder,
         [uuidEncoder.EncodesType] = uuidEncoder,
         [byteEncoder.EncodesType] = byteEncoder,
         [ushortEncoder.EncodesType] = ushortEncoder,
         [uintEncoder.EncodesType] = uintEncoder,
         [ulongEncoder.EncodesType] = ulongEncoder,
         [deliveryTagEncoder.EncodesType] = deliveryTagEncoder
      };

      public IEncoderState NewEncoderState()
      {
         return new ProtonEncoderState(this);
      }

      public IEncoderState CachedEncoderState => cachedEncoderState ??= new ProtonEncoderState(this);

      public void WriteNull(IProtonBuffer buffer, IEncoderState state)
      {
         nullEncoder.WriteType(buffer, state, null);
      }

      public void WriteBoolean(IProtonBuffer buffer, IEncoderState state, bool value)
      {
         booleanEncoder.WriteType(buffer, state, value);
      }

      public void WriteUnsignedByte(IProtonBuffer buffer, IEncoderState state, byte value)
      {
         byteEncoder.WriteType(buffer, state, value);
      }

      public void WriteUnsignedShort(IProtonBuffer buffer, IEncoderState state, ushort value)
      {
         ushortEncoder.WriteType(buffer, state, value);
      }

      public void WriteUnsignedInteger(IProtonBuffer buffer, IEncoderState state, uint value)
      {
         uintEncoder.WriteType(buffer, state, value);
      }

      public void WriteUnsignedLong(IProtonBuffer buffer, IEncoderState state, ulong value)
      {
         ulongEncoder.WriteType(buffer, state, value);
      }

      public void WriteByte(IProtonBuffer buffer, IEncoderState state, sbyte value)
      {
         sbyteEncoder.WriteType(buffer, state, value);
      }

      public void WriteShort(IProtonBuffer buffer, IEncoderState state, short value)
      {
         shortEncoder.WriteType(buffer, state, value);
      }

      public void WriteInteger(IProtonBuffer buffer, IEncoderState state, int value)
      {
         integerEncoder.WriteType(buffer, state, value);
      }

      public void WriteLong(IProtonBuffer buffer, IEncoderState state, long value)
      {
         longEncoder.WriteType(buffer, state, value);
      }

      public void WriteFloat(IProtonBuffer buffer, IEncoderState state, float value)
      {
         floatEncoder.WriteType(buffer, state, value);
      }

      public void WriteDouble(IProtonBuffer buffer, IEncoderState state, double value)
      {
         doubleEncoder.WriteType(buffer, state, value);
      }

      public void WriteDecimal32(IProtonBuffer buffer, IEncoderState state, Decimal32 value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            decimal32Encoder.WriteType(buffer, state, value);
         }
      }

      public void WriteDecimal64(IProtonBuffer buffer, IEncoderState state, Decimal64 value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            decimal64Encoder.WriteType(buffer, state, value);
         }
      }

      public void WriteDecimal128(IProtonBuffer buffer, IEncoderState state, Decimal128 value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            decimal128Encoder.WriteType(buffer, state, value);
         }
      }

      public void WriteCharacter(IProtonBuffer buffer, IEncoderState state, char value)
      {
         charEncoder.WriteType(buffer, state, value);
      }

      public void WriteTimestamp(IProtonBuffer buffer, IEncoderState state, long value)
      {
         timestampEncoder.WriteType(buffer, state, value);
      }

      public void WriteTimestamp(IProtonBuffer buffer, IEncoderState state, ulong value)
      {
         timestampEncoder.WriteType(buffer, state, value);
      }

      public void WriteGuid(IProtonBuffer buffer, IEncoderState state, Guid value)
      {
         uuidEncoder.WriteType(buffer, state, value);
      }

      public void WriteBinary(IProtonBuffer buffer, IEncoderState state, byte[] value)
      {
         binaryEncoder.WriteType(buffer, state, value);
      }

      public void WriteBinary(IProtonBuffer buffer, IEncoderState state, IProtonBuffer value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            binaryEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteString(IProtonBuffer buffer, IEncoderState state, string value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            stringEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteSymbol(IProtonBuffer buffer, IEncoderState state, Symbol value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            symbolEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteSymbol(IProtonBuffer buffer, IEncoderState state, string value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            symbolEncoder.WriteType(buffer, state, Symbol.Lookup(value));
         }
      }

      public void WriteList(IProtonBuffer buffer, IEncoderState state, IList value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            listEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteList<T>(IProtonBuffer buffer, IEncoderState state, IList<T> value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            listEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteMap(IProtonBuffer buffer, IEncoderState state, IDictionary value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            mapEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteMap<K, V>(IProtonBuffer buffer, IEncoderState state, IDictionary<K, V> value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            mapEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteMap<K, V>(IProtonBuffer buffer, IEncoderState state, IReadOnlyDictionary<K, V> value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            mapEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteDescribedType(IProtonBuffer buffer, IEncoderState state, IDescribedType value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            unknownTypeEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteDeliveryTag(IProtonBuffer buffer, IEncoderState state, IDeliveryTag value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            deliveryTagEncoder.WriteType(buffer, state, value);
         }
      }

      public void WriteObject(IProtonBuffer buffer, IEncoderState state, Object value)
      {
         if (value != null)
         {
            ITypeEncoder encoder = null;

            if (typeEncoders.TryGetValue(value.GetType(), out encoder))
            {
               encoder.WriteType(buffer, state, value);
            }
            else
            {
               WriteUnregisteredType(buffer, state, value);
            }
         }
         else
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte((byte)EncodingCodes.Null);
         }
      }

      private void WriteUnregisteredType(IProtonBuffer buffer, IEncoderState state, object value)
      {
         if (value.GetType().IsArray)
         {
            WriteArray(buffer, state, value as Array);
         }
         else if (value is IList)
         {
            WriteList(buffer, state, (IList)value);
         }
         else if (value is IDictionary)
         {
            WriteMap(buffer, state, (IDictionary)value);
         }
         else if (value is IDescribedType)
         {
            WriteDescribedType(buffer, state, (IDescribedType)value);
         }
         else
         {
            throw new ArgumentException(
                "Do not know how to write Objects of class " + value.GetType().Name);
         }
      }

      public void WriteArray(IProtonBuffer buffer, IEncoderState state, Array value)
      {
         if (value == null)
         {
            buffer.EnsureWritable(sizeof(byte));
            buffer.WriteUnsignedByte(((byte)EncodingCodes.Null));
         }
         else
         {
            arrayEncoder.WriteType(buffer, state, value);
         }
      }

      public IEncoder RegisterDescribedTypeEncoder(IDescribedTypeEncoder encoder)
      {
         typeEncoders[encoder.EncodesType] = encoder;
         return this;
      }

      public ITypeEncoder LookupTypeEncoder(Object value)
      {
         if (value == null)
         {
            return nullEncoder;
         }
         else
         {
            ITypeEncoder encoder = null;
            if (!typeEncoders.TryGetValue(value.GetType(), out encoder))
            {
               encoder = DeduceTypeEncoder(value.GetType(), value);
            }

            return encoder;
         }
      }

      public ITypeEncoder LookupTypeEncoder(Type typeClass)
      {
         ITypeEncoder encoder = null;

         if (!typeEncoders.TryGetValue(typeClass, out encoder))
         {
            encoder = DeduceTypeEncoder(typeClass, null);
         }

         return encoder;
      }

      private ITypeEncoder DeduceTypeEncoder(Type typeClass, Object instance)
      {
         ITypeEncoder encoder = null;

         if (typeClass.IsArray)
         {
            encoder = arrayEncoder;
         }
         else
         {
            if (typeClass.IsAssignableTo(typeof(IList)))
            {
               encoder = listEncoder;
            }
            else if (typeClass.IsAssignableTo(typeof(IDictionary)))
            {
               encoder = mapEncoder;
            }
            else if (typeClass.IsAssignableTo(typeof(IProtonBuffer)))
            {
               encoder = binaryEncoder;
            }
            else if (typeClass.IsAssignableTo(typeof(IDescribedType)))
            {
               // For instances of a specific DescribedType that we don't know about the
               // generic described type encoder will work.  We don't use that though for
               // class lookups as we don't want to allow arrays of polymorphic types.
               if (encoder == null && instance != null)
               {
                  return unknownTypeEncoder;
               }
            }
         }

         // Ensure that next time we find the encoder immediately and don't need to
         // go through this process again.
         typeEncoders[typeClass] = encoder;

         return encoder;
      }
   }
}