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

      uint Size(Stream stream);

      void Parse(Stream stream, Codec data);

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

      internal static long Decode(Stream input, Codec codec)
      {
         if (input.Position < input.Length)
         {
            long position = input.Position;
            ITypeConstructor c = ReadConstructor(input);
            uint size = c.Size(input);

            if (input.ReadableBytes() >= size)
            {
               c.Parse(input, codec);
               return 1 + size;
            }
            else
            {
               input.Position = position;
               return -4;
            }
         }

         return 0;
      }

      private static ITypeConstructor ReadConstructor(Stream stream)
      {
         byte index = stream.ReadUnsignedByte();
         ITypeConstructor tc = constructors[index];
         if (tc == null)
         {
            throw new ArgumentException("No constructor for type " + index);
         }
         return tc;
      }

      private static void ParseChildren(Codec codec, byte[] buffer, int count)
      {
         Stream stream = new MemoryStream(buffer);

         codec.Enter();
         for (int i = 0; i < count; i++)
         {
            ITypeConstructor c = ReadConstructor(stream);
            uint size = c.Size(stream);
            long getReadableBytes = stream.ReadableBytes();

            if (size <= getReadableBytes)
            {
               c.Parse(stream, codec);
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
         Stream stream = new MemoryStream(buffer);

         byte type = stream.ReadUnsignedByte();
         bool isDescribed = type == 0x00;
         long descriptorPosition = stream.ReadIndex();

         if (isDescribed)
         {
            ITypeConstructor descriptorTc = ReadConstructor(stream);
            stream.ReadIndex(descriptorPosition + descriptorTc.Size(stream));
            type = stream.ReadUnsignedByte();
            if (type == 0x00)
            {
               throw new ArgumentException("Malformed array data");
            }

         }

         ITypeConstructor tc = constructors[type & 0xff];

         codec.PutArray(isDescribed, tc.DataType);
         codec.Enter();

         if (isDescribed)
         {
            long position = stream.ReadIndex();
            stream.ReadIndex(descriptorPosition);
            ITypeConstructor descriptorTc = ReadConstructor(stream);
            descriptorTc.Parse(stream, codec);
            stream.ReadIndex(position);
         }

         for (int i = 0; i < count; i++)
         {
            tc.Parse(stream, codec);
         }

         codec.Exit();
      }

      #region Type Constructors for all AMQP types

      private class NullConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Null;

         public uint Size(Stream stream)
         {
            return 0;
         }

         public void Parse(Stream stream, Codec codec)
         {
            codec.PutNull();
         }
      }

      private class TrueConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Bool;

         public uint Size(Stream stream)
         {
            return 0;
         }

         public void Parse(Stream stream, Codec codec)
         {
            codec.PutBoolean(true);
         }
      }

      private class FalseConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Bool;

         public uint Size(Stream stream)
         {
            return 0;
         }

         public void Parse(Stream stream, Codec codec)
         {
            codec.PutBoolean(false);
         }
      }

      private class UInt0Constructor : ITypeConstructor
      {
         public DataType DataType => DataType.UInt;

         public uint Size(Stream stream)
         {
            return 0;
         }

         public void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedInteger(0);
         }
      }

      private class ULong0Constructor : ITypeConstructor
      {
         public DataType DataType => DataType.ULong;

         public uint Size(Stream stream)
         {
            return 0;
         }

         public void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedLong(0);
         }
      }

      private class EmptyListConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.List;

         public uint Size(Stream stream)
         {
            return 0;
         }

         public void Parse(Stream stream, Codec codec)
         {
            codec.PutList();
         }
      }

      private abstract class Fixed0SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            return 0;
         }
      }

      private abstract class Fixed1SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            return 1;
         }
      }

      private abstract class Fixed2SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            return 2;
         }
      }

      private abstract class Fixed4SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            return 4;
         }
      }

      private abstract class Fixed8SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            return 8;
         }
      }

      private abstract class Fixed16SizeConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            return 16;
         }
      }

      private class UByteConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.UByte;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedByte(stream.ReadUnsignedByte());
         }
      }

      private class ByteConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Byte;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutByte(stream.ReadSignedByte());
         }
      }

      private class SmallUIntConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.UInt;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedInteger(stream.ReadUnsignedByte());
         }
      }

      private class SmallIntConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Int;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutInt(stream.ReadSignedByte());
         }
      }

      private class SmallULongConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.ULong;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedLong(stream.ReadUnsignedByte());
         }
      }

      private class SmallLongConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Long;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutLong(stream.ReadSignedByte());
         }
      }

      private class BooleanConstructor : Fixed1SizeConstructor
      {
         public override DataType DataType => DataType.Bool;

         public override void Parse(Stream stream, Codec codec)
         {
            byte i = stream.ReadUnsignedByte();
            if (i is not 0 and not 1)
            {
               throw new ArgumentOutOfRangeException("Illegal value " + i + " for boolean");
            }

            codec.PutBoolean(i == 1);
         }
      }

      private class UShortConstructor : Fixed2SizeConstructor
      {
         public override DataType DataType => DataType.UShort;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedShort(stream.ReadUnsignedShort());
         }
      }

      private class ShortConstructor : Fixed2SizeConstructor
      {
         public override DataType DataType => DataType.Short;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutShort(stream.ReadShort());
         }
      }

      private class UIntConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.UInt;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedInteger(stream.ReadUnsignedInt());
         }
      }

      private class IntConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Int;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutInt(stream.ReadInt());
         }
      }

      private class FloatConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Float;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutFloat(stream.ReadFloat());
         }
      }

      private class CharConstructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Char;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutChar((char)stream.ReadShort());
         }
      }

      private class Decimal32Constructor : Fixed4SizeConstructor
      {
         public override DataType DataType => DataType.Decimal32;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutDecimal32(new Decimal32(stream.ReadUnsignedInt()));
         }
      }

      private class ULongConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.ULong;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutUnsignedLong(stream.ReadUnsignedLong());
         }
      }

      private class LongConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Long;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutLong(stream.ReadLong());
         }
      }

      private class DoubleConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Double;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutDouble(stream.ReadDouble());
         }
      }

      private class TimestampConstructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Timestamp;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutTimestamp(new DateTime(stream.ReadLong(), DateTimeKind.Utc));
         }
      }

      private class Decimal64Constructor : Fixed8SizeConstructor
      {
         public override DataType DataType => DataType.Decimal64;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutDecimal64(new Decimal64(stream.ReadUnsignedLong()));
         }
      }

      private class Decimal128Constructor : Fixed16SizeConstructor
      {
         public override DataType DataType => DataType.Decimal128;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutDecimal128(new Decimal128(stream.ReadUnsignedLong(), stream.ReadUnsignedLong()));
         }
      }

      private class UuidConstructor : Fixed16SizeConstructor
      {
         private static readonly int BYTES = sizeof(long) * 2;

         public override DataType DataType => DataType.Uuid;

         public override void Parse(Stream stream, Codec codec)
         {
            codec.PutUUID(new Guid(stream.ReadBytes(BYTES)));
         }
      }

      private abstract class SmallVariableConstructor : ITypeConstructor
      {
         public abstract DataType DataType { get; }

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            long position = stream.ReadIndex();
            if (stream.IsReadable())
            {
               byte size = stream.ReadUnsignedByte();
               stream.ReadIndex(position);

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

         public abstract void Parse(Stream stream, Codec data);

         public uint Size(Stream stream)
         {
            long position = stream.ReadIndex();
            if (stream.IsReadable())
            {
               uint size = stream.ReadUnsignedInt();
               stream.ReadIndex(position);

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

         public override void Parse(Stream stream, Codec codec)
         {
            byte size = stream.ReadUnsignedByte();
            byte[] bytes = stream.ReadBytes(size);
            codec.PutBinary(bytes);
         }
      }

      private class SmallSymbolConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Symbol;

         public override void Parse(Stream stream, Codec codec)
         {
            byte size = stream.ReadUnsignedByte();
            byte[] bytes = stream.ReadBytes(size);
            codec.PutSymbol(new Symbol(bytes));
         }
      }

      private class SmallStringConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.String;

         public override void Parse(Stream stream, Codec codec)
         {
            byte size = stream.ReadUnsignedByte();
            byte[] bytes = stream.ReadBytes(size);
            codec.PutString(System.Text.Encoding.UTF8.GetString(bytes));
         }
      }

      private class BinaryConstructor : VariableConstructor
      {
         public override DataType DataType => DataType.Binary;

         public override void Parse(Stream stream, Codec codec)
         {
            int size = stream.ReadInt();
            byte[] bytes = stream.ReadBytes(size);
            codec.PutBinary(bytes);
         }
      }

      private class SymbolConstructor : VariableConstructor
      {
         public override DataType DataType => DataType.Symbol;

         public override void Parse(Stream stream, Codec codec)
         {
            int size = stream.ReadInt();
            byte[] bytes = stream.ReadBytes(size);
            codec.PutSymbol(new Symbol(bytes));
         }
      }

      private class StringConstructor : VariableConstructor
      {
         public override DataType DataType => DataType.String;

         public override void Parse(Stream stream, Codec codec)
         {
            int size = stream.ReadInt();
            byte[] bytes = stream.ReadBytes(size);
            codec.PutString(System.Text.Encoding.UTF8.GetString(bytes));
         }
      }

      private class SmallListConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.List;

         public override void Parse(Stream stream, Codec codec)
         {
            byte size = stream.ReadUnsignedByte();
            byte count = stream.ReadUnsignedByte();

            byte[] bytes = stream.ReadBytes(size - 1);

            codec.PutList();

            ParseChildren(codec, bytes, count);
         }
      }

      private class SmallMapConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Map;

         public override void Parse(Stream stream, Codec codec)
         {
            byte size = stream.ReadUnsignedByte();
            byte count = stream.ReadUnsignedByte();

            byte[] bytes = stream.ReadBytes(size - 1);

            codec.PutMap();

            ParseChildren(codec, bytes, count);
         }
      }

      private class ListConstructor : VariableConstructor
      {
         public override DataType DataType => DataType.List;

         public override void Parse(Stream stream, Codec codec)
         {
            uint size = stream.ReadUnsignedInt();
            int count = stream.ReadInt();

            byte[] bytes = stream.ReadBytes((int)(size - 4));

            codec.PutList();

            ParseChildren(codec, bytes, count);
         }
      }

      private class MapConstructor : VariableConstructor
      {
         public override DataType DataType => DataType.Map;

         public override void Parse(Stream stream, Codec codec)
         {
            uint size = stream.ReadUnsignedInt();
            int count = stream.ReadInt();

            byte[] bytes = stream.ReadBytes((int)(size - 4));

            codec.PutMap();

            ParseChildren(codec, bytes, count);
         }
      }

      private class DescribedTypeConstructor : ITypeConstructor
      {
         public DataType DataType => DataType.Described;

         public uint Size(Stream stream)
         {
            if (stream.IsReadable())
            {
               long position = stream.ReadIndex();
               try
               {
                  ITypeConstructor c = ReadConstructor(stream);
                  uint size = c.Size(stream);
                  if (stream.ReadableBytes() > size)
                  {
                     stream.ReadIndex(position + size + 1);
                     c = ReadConstructor(stream);
                     return size + 2 + c.Size(stream);
                  }
                  else
                  {
                     return size + 2;
                  }
               }
               finally
               {
                  stream.ReadIndex(position);
               }
            }
            else
            {
               return 1;
            }
         }

         public void Parse(Stream stream, Codec codec)
         {
            codec.PutDescribed();
            codec.Enter();
            ITypeConstructor constructor = ReadConstructor(stream);
            constructor.Parse(stream, codec);
            constructor = ReadConstructor(stream);
            constructor.Parse(stream, codec);
            codec.Exit();
         }
      }

      private class SmallArrayConstructor : SmallVariableConstructor
      {
         public override DataType DataType => DataType.Array;

         public override void Parse(Stream stream, Codec codec)
         {
            byte size = stream.ReadUnsignedByte();
            byte count = stream.ReadUnsignedByte();

            byte[] bytes = stream.ReadBytes(size - 1);

            ParseArray(codec, bytes, count);
         }
      }

      private class ArrayConstructor : VariableConstructor
      {
         public override DataType DataType => DataType.Array;

         public override void Parse(Stream stream, Codec codec)
         {
            int size = stream.ReadInt();
            int count = stream.ReadInt();

            byte[] bytes = stream.ReadBytes(size - 4);

            ParseArray(codec, bytes, count);
         }
      }

      #endregion
   }
}