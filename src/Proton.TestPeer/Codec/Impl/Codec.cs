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
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public static class CodecFactory
   {
      public static ICodec Create()
      {
         return new Codec();
      }
   }

   public sealed class Codec : ICodec
   {
      private IElement first;
      private IElement current;
      private IElement parent;

      public uint Count => first?.GetSize() ?? 0u;

      public DataType DataType => current?.DataType ?? DataType.None;

      public long EncodedSize
      {
         get
         {
            uint size = 0;
            IElement element = first;
            while (element != null)
            {
               size += element.GetSize();
               element = element.Next;
            }
            return size;
         }
      }

      public bool IsArrayDescribed
      {
         get
         {
            if (current is ArrayElement)
            {
               return ((ArrayElement)current).IsDescribed;
            }
            throw new InvalidOperationException("Current value not array");
         }
      }

      public bool IsDescribed => current?.DataType == DataType.Described;

      public bool IsNull => current?.DataType == DataType.Null;

      public void Clear()
      {
         first = null;
         current = null;
         parent = null;
      }

      public void Rewind()
      {
         current = null;
         parent = null;
      }

      public DataType Next
      {
         get
         {
            IElement next = current == null ? (parent == null ? first : parent.Child) : current.Next;

            if (next != null)
            {
               current = next;
            }
            return next == null ? DataType.None : next.DataType;
         }
      }

      public DataType Prev
      {
         get
         {
            IElement prev = current == null ? null : current.Prev;

            current = prev;
            return prev == null ? DataType.None : prev.DataType;
         }
      }

      public bool Enter()
      {
         if (current != null && current.CanEnter)
         {
            parent = current;
            current = null;
            return true;
         }

         return false;
      }

      public bool Exit()
      {
         if (parent != null)
         {
            IElement oldParent = this.parent;
            current = oldParent;
            parent = current.Parent;
            return true;

         }
         return false;
      }

      public long Decode(Stream stream)
      {
        return TypeDecoder.Decode(stream, this);
      }

      public long Encode(Stream stream)
      {
         long position = stream.Position;
         IElement elt = first;
         uint size = 0;

         while (elt != null)
         {
            uint eltSize = elt.GetSize();
            if (stream.CanWrite)
            {
               size += elt.Encode(stream);
            }
            else
            {
               size += eltSize;
            }

            elt = elt.Next;
         }

         return size;
      }

      public string Format()
      {
         StringBuilder sb = new StringBuilder();
         IElement element = this.first;
         bool first = true;
         while (element != null)
         {
            if (first)
            {
               first = false;
            }
            else
            {
               sb.Append(", ");
            }
            element.Render(sb);
            element = element.Next;
         }

         return sb.ToString();
      }

      public void Free()
      {
         first = null;
         current = null;
      }

      public uint GetArray()
      {
         if (current is ArrayElement)
         {
            return ((ArrayElement)current).Count;
         }
         throw new InvalidOperationException("Current value not array");
      }

      public DataType GetArrayType()
      {
         if (current is ArrayElement)
         {
            return ((ArrayElement)current).ArrayType;
         }
         throw new InvalidOperationException("Current value not array");
      }

      public Binary GetBinary()
      {
         if (current is BinaryElement)
         {
            return ((BinaryElement)current).BinaryValue;
         }
         throw new InvalidOperationException("Current value not a Binary");
      }

      public bool GetBoolean()
      {
         if (current is BooleanElement)
         {
            return ((BooleanElement)current).BooleanValue;
         }
         throw new InvalidOperationException("Current value not boolean");
      }

      public sbyte GetByte()
      {
         if (current is ByteElement)
         {
            return ((ByteElement)current).SByteValue;
         }
         throw new InvalidOperationException("Current value not signed byte");
      }

      public int GetChar()
      {
         if (current is CharElement)
         {
            return ((CharElement)current).CharValue;
         }
         throw new InvalidOperationException("Current value not character");
      }

      public Decimal128 GetDecimal128()
      {
         if (current is Decimal128Element)
         {
            return ((Decimal128Element)current).DecimalValue;
         }
         throw new InvalidOperationException("Current value not Decimal128 value");
      }

      public Decimal32 GetDecimal32()
      {
         if (current is Decimal32Element)
         {
            return ((Decimal32Element)current).DecimalValue;
         }
         throw new InvalidOperationException("Current value not Decimal32 value");
      }

      public Decimal64 GetDecimal64()
      {
         if (current is Decimal64Element)
         {
            return ((Decimal64Element)current).DecimalValue;
         }
         throw new InvalidOperationException("Current value not Decimal64 value");
      }

      public IDescribedType GetDescribedType()
      {
         if (current is DescribedTypeElement)
         {
            return ((DescribedTypeElement)current).DescribedValue;
         }
         throw new InvalidOperationException("Current value not described type");
      }

      public double GetDouble()
      {
         if (current is DoubleElement)
         {
            return ((DoubleElement)current).DoubleValue;
         }

         throw new InvalidOperationException("Current value not a double");
      }

      public float GetFloat()
      {
         if (current is FloatElement)
         {
            return ((FloatElement)current).FloatValue;
         }

         throw new InvalidOperationException("Current value not a float");
      }

      public int GetInt()
      {
         if (current is IntegerElement)
         {
            return ((IntegerElement)current).IntegerValue;
         }

         throw new InvalidOperationException("Current value not a int");
      }

      public uint GetList()
      {
         if (current is ListElement)
         {
            return ((ListElement)current).Count;
         }

         throw new InvalidOperationException("Current value not list");
      }

      public long GetLong()
      {
         if (current is LongElement)
         {
            return ((LongElement)current).LongValue;
         }

         throw new InvalidOperationException("Current value not a long");
      }

      public uint GetMap()
      {
         if (current is MapElement)
         {
            return ((MapElement)current).Count;
         }

         throw new InvalidOperationException("Current value not map");
      }

      public object GetObject()
      {
         return current == null ? null : current.Value;
      }

      public Array GetPrimitiveArray()
      {
         if (current is ArrayElement)
         {
            return ((ArrayElement)current).ArrayValue;
         }

         throw new InvalidOperationException("Current value not array");
      }

      public IList GetPrimitiveList()
      {
         if (current is ListElement)
         {
            return ((ListElement)current).ListValue;
         }

         throw new InvalidOperationException("Current value not list");
      }

      public IDictionary GetPrimitiveMap()
      {
         if (current is MapElement)
         {
            return ((MapElement)current).MapValue;
         }

         throw new InvalidOperationException("Current value not map");
      }

      public short GetShort()
      {
         if (current is ShortElement)
         {
            return ((ShortElement)current).ShortValue;
         }

         throw new InvalidOperationException("Current value not short");
      }

      public string GetString()
      {
         if (current is StringElement)
         {
            return ((StringElement)current).StringValue;
         }

         throw new InvalidOperationException("Current value not a string");
      }

      public Symbol GetSymbol()
      {
         if (current is SymbolElement)
         {
            return ((SymbolElement)current).SymbolValue;
         }

         throw new InvalidOperationException("Current value not a symbol");
      }

      public DateTime GetTimestamp()
      {
         if (current is TimestampElement)
         {
            return ((TimestampElement)current).TimeValue;
         }

         throw new InvalidOperationException("Current value not a time stamp");
      }

      public byte GetUnsignedByte()
      {
         if (current is UnsignedByteElement)
         {
            return ((UnsignedByteElement)current).ByteValue;
         }
         throw new InvalidOperationException("Current value not unsigned byte");
      }

      public uint GetUnsignedInteger()
      {
         if (current is UnsignedIntegerElement)
         {
            return ((UnsignedIntegerElement)current).UIntValue;
         }
         throw new InvalidOperationException("Current value not unsigned integer");
      }

      public ulong GetUnsignedLong()
      {
         if (current is UnsignedLongElement)
         {
            return ((UnsignedLongElement)current).ULongValue;
         }
         throw new InvalidOperationException("Current value not unsigned long");
      }

      public ushort GetUnsignedShort()
      {
         if (current is UnsignedShortElement)
         {
            return ((UnsignedShortElement)current).UShortValue;
         }
         throw new InvalidOperationException("Current value not unsigned short");
      }

      public Guid GetUUID()
      {
         if (current is UuidElement)
         {
            return ((UuidElement)current).GuidValue;
         }
         throw new InvalidOperationException("Current value not a UUID");
      }

      public void PutArray(bool described, DataType type)
      {
         PutElement(new ArrayElement(parent, current, described, type));
      }

      public void PutBinary(Span<byte> bytes)
      {
         PutElement(new BinaryElement(parent, current, new Binary(bytes.ToArray())));
      }

      public void PutBinary(byte[] bytes)
      {
         PutElement(new BinaryElement(parent, current, new Binary(bytes)));
      }

      public void PutBinary(Binary binary)
      {
         PutElement(new BinaryElement(parent, current, binary));
      }

      public void PutBoolean(bool b)
      {
         PutElement(new BooleanElement(parent, current, b));
      }

      public void PutByte(sbyte b)
      {
         PutElement(new ByteElement(parent, current, b));
      }

      public void PutChar(char c)
      {
         PutElement(new CharElement(parent, current, c));
      }

      public void PutDecimal32(Decimal32 d)
      {
         PutElement(new Decimal32Element(parent, current, d));
      }

      public void PutDecimal64(Decimal64 d)
      {
         PutElement(new Decimal64Element(parent, current, d));
      }

      public void PutDecimal128(Decimal128 d)
      {
         PutElement(new Decimal128Element(parent, current, d));
      }

      public void PutDescribed()
      {
         PutElement(new DescribedTypeElement(parent, current));
      }

      public void PutDescribedType(IDescribedType dt)
      {
         PutElement(new DescribedTypeElement(parent, current));
         Enter();
         PutObject(dt.Descriptor);
         PutObject(dt.Described);
         Exit();
      }

      public void PutDouble(double d)
      {
         PutElement(new DoubleElement(parent, current, d));
      }

      public void PutFloat(float f)
      {
         PutElement(new FloatElement(parent, current, f));
      }

      public void PutInt(int i)
      {
         PutElement(new IntegerElement(parent, current, i));
      }

      public void PutList()
      {
         PutElement(new ListElement(parent, current));
      }

      public void PutLong(long l)
      {
         PutElement(new LongElement(parent, current, l));
      }

      public void PutMap()
      {
         PutElement(new MapElement(parent, current));
      }

      public void PutNull()
      {
         PutElement(new NullElement(parent, current));
      }

      public void PutPrimitiveList(IList list)
      {
         PutList();
         Enter();
         IEnumerator enumerator = list.GetEnumerator();
         while (enumerator.MoveNext())
         {
            PutObject(enumerator.Current);
         }
         Exit();
      }

      public void PutPrimitiveMap(IDictionary map)
      {
         PutMap();
         Enter();
         IDictionaryEnumerator enumerator = map.GetEnumerator();
         while (enumerator.MoveNext())
         {
            PutObject(enumerator.Key);
            PutObject(enumerator.Value);
         }
         Exit();
      }

      public void PutShort(short s)
      {
         PutElement(new ShortElement(parent, current, s));
      }

      public void PutString(string str)
      {
         PutElement(new StringElement(parent, current, str));
      }

      public void PutSymbol(Symbol symbol)
      {
         PutElement(new SymbolElement(parent, current, symbol));
      }

      public void PutTimestamp(DateTime t)
      {
         PutElement(new TimestampElement(parent, current, new DateTimeOffset(t).Ticks));
      }

      public void PutUnsignedByte(byte ub)
      {
         PutElement(new UnsignedByteElement(parent, current, ub));
      }

      public void PutUnsignedInteger(uint ui)
      {
         PutElement(new UnsignedIntegerElement(parent, current, ui));
      }

      public void PutUnsignedLong(ulong ul)
      {
         PutElement(new UnsignedLongElement(parent, current, ul));
      }

      public void PutUnsignedShort(ushort us)
      {
         PutElement(new UnsignedShortElement(parent, current, us));
      }

      public void PutUUID(Guid u)
      {
         PutElement(new UuidElement(parent, current, u));
      }

      public void PutObject(object o)
      {
         if (o == null)
         {
            PutNull();
         }
         else if (o is bool)
         {
            PutBoolean((bool)o);
         }
         else if (o is byte)
         {
            PutUnsignedByte((byte)o);
         }
         else if (o is sbyte)
         {
            PutByte((sbyte)o);
         }
         else if (o is ushort)
         {
            PutUnsignedShort((ushort)o);
         }
         else if (o is short)
         {
            PutShort((short)o);
         }
         else if (o is uint)
         {
            PutUnsignedInteger((uint)o);
         }
         else if (o is int)
         {
            PutInt((int)o);
         }
         else if (o is char)
         {
            PutChar((char)o);
         }
         else if (o is ulong)
         {
            PutUnsignedLong((ulong)o);
         }
         else if (o is long)
         {
            PutLong((long)o);
         }
         else if (o is DateTime)
         {
            PutTimestamp((DateTime)o);
         }
         else if (o is float)
         {
            PutFloat((float)o);
         }
         else if (o is Double)
         {
            PutDouble((Double)o);
         }
         else if (o is Decimal32)
         {
            PutDecimal32((Decimal32)o);
         }
         else if (o is Decimal64)
         {
            PutDecimal64((Decimal64)o);
         }
         else if (o is Decimal128)
         {
            PutDecimal128((Decimal128)o);
         }
         else if (o is Guid)
         {
            PutUUID((Guid)o);
         }
         else if (o is Binary)
         {
            PutBinary((Binary)o);
         }
         else if (o is String)
         {
            PutString((String)o);
         }
         else if (o is Symbol)
         {
            PutSymbol((Symbol)o);
         }
         else if (o is IDescribedType)
         {
            PutDescribedType((IDescribedType)o);
         }
         else if (o is Symbol[])
         {
            PutArray(false, DataType.Symbol);
            Enter();
            foreach (Symbol s in (Symbol[])o)
            {
               PutSymbol(s);
            }
            Exit();
         }
         else if (o is Object[])
         {
            throw new ArgumentException("Unsupported array type");
         }
         else if (o is IList)
         {
            PutPrimitiveList((IList)o);
         }
         else if (o is IDictionary)
         {
            PutPrimitiveMap((IDictionary)o);
         }
         else
         {
            throw new ArgumentException("Unknown type " + o.GetType().Name);
         }
      }

      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         Render(sb, first);
         return String.Format("Data[current={0:X}, parent={1:X}]{{2}}",
                  RuntimeHelpers.GetHashCode(current),
                  RuntimeHelpers.GetHashCode(parent), sb);
      }

      #region Private Codec implementation

      private void Render(StringBuilder sb, IElement el)
      {
         if (el == null)
         {
            return;
         }

         sb.Append("    ").Append(el).Append("\n");
         if (el.CanEnter)
         {
            Render(sb, el.Child);
         }
         Render(sb, el.Next);
      }

      private void PutElement(IElement element)
      {
         if (first == null)
         {
            first = element;
         }
         else
         {
            if (current == null)
            {
               if (parent == null)
               {
                  first = first.ReplaceWith(element);
                  element = first;
               }
               else
               {
                  element = parent.AddChild(element);
               }
            }
            else
            {
               if (parent != null)
               {
                  element = parent.CheckChild(element);
               }
               current.Next = element;
            }
         }

         current = element;
      }

      #endregion
   }
}