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
using System.IO;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   internal interface ITypeConstructor
   {
      DataType DataType { get; }

      uint Size(BinaryReader reader);

      void Parse(BinaryReader reader, Codec data);

   }

   internal static class TypeDecoder
   {
      private static readonly ITypeConstructor[] constructors = new ITypeConstructor[256];

      static TypeDecoder()
      {
         constructors[0x00] = new DescribedTypeConstructor();
         constructors[0x40] = new NullConstructor();
         constructors[0x41] = new TrueConstructor();
         constructors[0x42] = new FalseConstructor();
         constructors[0x43] = new UInt0Constructor();
         constructors[0x44] = new ULong0Constructor();
         constructors[0x45] = new EmptyListConstructor();
         constructors[0x50] = new UByteConstructor();
         constructors[0x51] = new ByteConstructor();
         constructors[0x52] = new SmallUIntConstructor();
         constructors[0x53] = new SmallULongConstructor();
         constructors[0x54] = new SmallIntConstructor();
         constructors[0x55] = new SmallLongConstructor();
         constructors[0x56] = new BooleanConstructor();
         constructors[0x60] = new UShortConstructor();
         constructors[0x61] = new ShortConstructor();
         constructors[0x70] = new UIntConstructor();
         constructors[0x71] = new IntConstructor();
         constructors[0x72] = new FloatConstructor();
         constructors[0x73] = new CharConstructor();
         constructors[0x74] = new Decimal32Constructor();
         constructors[0x80] = new ULongConstructor();
         constructors[0x81] = new LongConstructor();
         constructors[0x82] = new DoubleConstructor();
         constructors[0x83] = new TimestampConstructor();
         constructors[0x84] = new Decimal64Constructor();
         constructors[0x94] = new Decimal128Constructor();
         constructors[0x98] = new UuidConstructor();
         constructors[0xa0] = new SmallBinaryConstructor();
         constructors[0xa1] = new SmallStringConstructor();
         constructors[0xa3] = new SmallSymbolConstructor();
         constructors[0xb0] = new BinaryConstructor();
         constructors[0xb1] = new StringConstructor();
         constructors[0xb3] = new SymbolConstructor();
         constructors[0xc0] = new SmallListConstructor();
         constructors[0xc1] = new SmallMapConstructor();
         constructors[0xd0] = new ListConstructor();
         constructors[0xd1] = new MapConstructor();
         constructors[0xe0] = new SmallArrayConstructor();
         constructors[0xf0] = new ArrayConstructor();
      }

      internal static long Decode(BinaryReader reader, Codec codec)
      {
         Stream baseStream = reader.BaseStream;
         if (baseStream.Position < baseStream.Length)
         {
            long position = baseStream.Position;
            ITypeConstructor c = ReadConstructor(reader);
            uint size = c.Size(reader);

            if (baseStream.ReadableBytes() >= size)
            {
               c.Parse(reader, codec);
               return 1 + size;
            }
            else
            {
               baseStream.Position = position;
               return -4;
            }
         }

         return 0;
      }

      private static ITypeConstructor ReadConstructor(BinaryReader reader)
      {
         byte index = reader.ReadByte();
         ITypeConstructor tc = constructors[index];
         if (tc == null)
         {
            throw new ArgumentException("No constructor for type " + index);
         }
         return tc;
      }

      private static void ParseChildren(Codec codec, byte[] buffer, int count)
      {
         BinaryReader reader = new BinaryReader(new MemoryStream(buffer));

         codec.Enter();
         for (int i = 0; i < count; i++)
         {
            ITypeConstructor c = ReadConstructor(reader);
            uint size = c.Size(reader);
            long getReadableBytes = reader.ReadableBytes();

            if (size <= getReadableBytes)
            {
               c.Parse(reader, codec);
            }
            else
            {
               throw new ArgumentException("Malformed data");
            }

         }
         codec.Exit();
      }

      private static void ParseArray(Codec codec, byte[] buffer, int count)
      {
         BinaryReader reader = new BinaryReader(new MemoryStream(buffer));

         byte type = reader.ReadByte();
         bool isDescribed = type == (byte)0x00;
         long descriptorPosition = reader.ReadIndex();

         if (isDescribed)
         {
            ITypeConstructor descriptorTc = ReadConstructor(reader);
            reader.ReadIndex(descriptorPosition + descriptorTc.Size(reader));
            type = reader.ReadByte();
            if (type == (byte)0x00)
            {
               throw new ArgumentException("Malformed array data");
            }

         }

         ITypeConstructor tc = constructors[type & 0xff];

         codec.PutArray(isDescribed, tc.DataType);
         codec.Enter();

         if (isDescribed)
         {
            long position = reader.ReadIndex();
            reader.ReadIndex(descriptorPosition);
            ITypeConstructor descriptorTc = ReadConstructor(reader);
            descriptorTc.Parse(reader, codec);
            reader.ReadIndex(position);
         }

         for (int i = 0; i < count; i++)
         {
            tc.Parse(reader, codec);
         }

         codec.Exit();
      }

      #region Type Constructors for all AMQP types

      private class NullConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Null;

         public uint Size(BinaryReader reader)
         {
            return 0;
         }

         public void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutNull();
         }
      }

      private class TrueConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Bool;

         public uint Size(BinaryReader reader)
         {
            return 0;
         }

         public void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutBoolean(true);
         }
      }

      private class FalseConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Bool;

         public uint Size(BinaryReader reader)
         {
            return 0;
         }

         public void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutBoolean(false);
         }
      }

      private class UInt0Constructor : ITypeConstructor
      {
         public DataType DataType => DataType.UInt;

         public uint Size(BinaryReader reader)
         {
            return 0;
         }

         public void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedInteger(0);
         }
      }

      private class ULong0Constructor : ITypeConstructor
      {
         public DataType DataType => DataType.ULong;

         public uint Size(BinaryReader reader)
         {
            return 0;
         }

         public void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedLong(0);
         }
      }

      private class EmptyListConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.List;

         public uint Size(BinaryReader reader)
         {
            return 0;
         }

         public void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutList();
         }
      }

      private abstract class Fixed0SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            return 0;
         }
      }

      private abstract class Fixed1SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            return 1;
         }
      }

      private abstract class Fixed2SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            return 2;
         }
      }

      private abstract class Fixed4SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            return 4;
         }
      }

      private abstract class Fixed8SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            return 8;
         }
      }

      private abstract class Fixed16SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            return 16;
         }
      }

      private class UByteConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.UByte;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedByte(reader.ReadByte());
         }
      }

      private class ByteConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Byte;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutByte((sbyte)reader.ReadByte());
         }
      }

      private class SmallUIntConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.UInt;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedInteger(reader.ReadByte());
         }
      }

      private class SmallIntConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Int;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutInt(reader.ReadByte());
         }
      }

      private class SmallULongConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.ULong;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedLong(reader.ReadByte());
         }
      }

      private class SmallLongConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Long;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutLong(reader.ReadByte());
         }
      }

      private class BooleanConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Bool;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            byte i = reader.ReadByte();
            if (i != 0 && i != 1)
            {
               throw new ArgumentOutOfRangeException("Illegal value " + i + " for boolean");
            }

            codec.PutBoolean(i == 1);
         }
      }

      private class UShortConstructor : Fixed2SizeConstructor
      {
         public override DataType DataType => DataType.UShort;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedShort(reader.ReadUInt16());
         }
      }

      private class ShortConstructor : Fixed2SizeConstructor
      {
         public override DataType DataType => DataType.Short;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutShort(reader.ReadInt16());
         }
      }

      private class UIntConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.UInt;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedInteger(reader.ReadUInt32());
         }
      }

      private class IntConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Int;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutInt(reader.ReadInt32());
         }
      }

      private class FloatConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Float;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutFloat(reader.ReadSingle());
         }
      }

      private class CharConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Char;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutChar(reader.ReadChar());
         }
      }

      private class Decimal32Constructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Decimal32;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutDecimal32(new Decimal32(reader.ReadUInt32()));
         }
      }

      private class ULongConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.ULong;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUnsignedLong(reader.ReadUInt64());
         }
      }

      private class LongConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Long;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutLong(reader.ReadInt64());
         }
      }

      private class DoubleConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Double;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutDouble(reader.ReadDouble());
         }
      }

      private class TimestampConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Timestamp;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutTimestamp(new DateTime(reader.ReadInt64(), DateTimeKind.Utc));
         }
      }

      private class Decimal64Constructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Decimal64;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutDecimal64(new Decimal64(reader.ReadUInt64()));
         }
      }

      private class Decimal128Constructor : Fixed16SizeConstructor
      {
         public override DataType DataType => DataType.Decimal128;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutDecimal128(new Decimal128(reader.ReadUInt64(), reader.ReadUInt64()));
         }
      }

      private class UuidConstructor : Fixed16SizeConstructor
      {
         private static readonly int BYTES = sizeof(long) * 2;

         public override DataType DataType => DataType.Uuid;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutUUID(new Guid(reader.ReadBytes(BYTES)));
         }
      }

      private abstract class SmallVariableConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            long position = reader.ReadIndex();
            if (reader.IsReadable())
            {
               byte size = reader.ReadByte();
               reader.ReadIndex(position);

               return (uint)(size + 1);
            }
            else
            {
               return 1;
            }
         }
      }

      private abstract class VariableConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(BinaryReader reader, Codec data);

         public uint Size(BinaryReader reader)
         {
            long position = reader.ReadIndex();
            if (reader.IsReadable())
            {
               uint size = reader.ReadUInt32();
               reader.ReadIndex(position);

               return size + 4u;
            }
            else
            {
               return 4;
            }
         }
      }

      private class SmallBinaryConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Binary;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            byte size = reader.ReadByte();
            byte[] bytes = reader.ReadBytes(size);
            codec.PutBinary(bytes);
         }
      }

      private class SmallSymbolConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Symbol;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            byte size = reader.ReadByte();
            byte[] bytes = reader.ReadBytes(size);
            codec.PutSymbol(new Symbol(bytes));
         }
      }

      private class SmallStringConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.String;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            byte size = reader.ReadByte();
            byte[] bytes = reader.ReadBytes(size);
            codec.PutString(System.Text.Encoding.UTF8.GetString(bytes));
         }
      }

      private class BinaryConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Binary;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            int size = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(size);
            codec.PutBinary(bytes);
         }
      }

      private class SymbolConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Symbol;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            int size = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(size);
            codec.PutSymbol(new Symbol(bytes));
         }
      }

      private class StringConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.String;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            int size = reader.ReadInt32();
            byte[] bytes = reader.ReadBytes(size);
            codec.PutString(System.Text.Encoding.UTF8.GetString(bytes));
         }
      }

      private class SmallListConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.List;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            byte size = reader.ReadByte();
            byte count = reader.ReadByte();

            byte[] bytes = reader.ReadBytes(size - 1);

            codec.PutList();

            ParseChildren(codec, bytes, count);
         }
      }

      private class SmallMapConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Map;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            byte size = reader.ReadByte();
            byte count = reader.ReadByte();

            byte[] bytes = reader.ReadBytes(size - 1);

            codec.PutMap();

            ParseChildren(codec, bytes, count);
         }
      }

      private class ListConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.List;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            uint size = reader.ReadUInt32();
            int count = reader.ReadInt32();

            byte[] bytes = reader.ReadBytes((int)(size - 4));

            codec.PutList();

            ParseChildren(codec, bytes, count);
         }
      }

      private class MapConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Map;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            uint size = reader.ReadUInt32();
            int count = reader.ReadInt32();

            byte[] bytes = reader.ReadBytes((int)(size - 4));

            codec.PutMap();

            ParseChildren(codec, bytes, count);
         }
      }

      private class DescribedTypeConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Described;

         public uint Size(BinaryReader reader)
         {
            if (reader.IsReadable())
            {
               long position = reader.ReadIndex();
               try
               {
                  ITypeConstructor c = ReadConstructor(reader);
                  uint size = c.Size(reader);
                  if (reader.ReadableBytes() > size)
                  {
                     reader.ReadIndex(size + 1);
                     c = ReadConstructor(reader);
                     return size + 2 + c.Size(reader);
                  }
                  else
                  {
                     return size + 2;
                  }
               }
               finally
               {
                  reader.ReadIndex(position);
               }
            }
            else
            {
               return 1;
            }
         }

         public void Parse(BinaryReader reader, Codec codec)
         {
            codec.PutDescribed();
            codec.Enter();
            ITypeConstructor constructor = ReadConstructor(reader);
            constructor.Parse(reader, codec);
            constructor = ReadConstructor(reader);
            constructor.Parse(reader, codec);
            codec.Exit();
         }
      }

      private class SmallArrayConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Array;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            byte size = reader.ReadByte();
            byte count = reader.ReadByte();

            byte[] bytes = reader.ReadBytes(size - 1);

            ParseArray(codec, bytes, count);
         }
      }

      private class ArrayConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Array;

         public override void Parse(BinaryReader reader, Codec codec)
         {
            int size = reader.ReadInt32();
            int count = reader.ReadInt32();

            byte[] bytes = reader.ReadBytes(size - 4);

            ParseArray(codec, bytes, count);
         }
      }

      #endregion
   }
}