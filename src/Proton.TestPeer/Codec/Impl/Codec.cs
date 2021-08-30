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

      public uint Count => first?.Size ?? 0u;

      public DataType DataType => current?.DataType ?? DataType.None;

      public uint EncodedSize
      {
         get
         {
            uint size = 0;
            IElement element = first;
            while (element != null)
            {
               size += element.Size;
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

      public uint Decode(BinaryReader reader)
      {
         throw new NotImplementedException();
      }

      public uint Encode(BinaryWriter writer)
      {
         // TODO
         // IElement elt = first;
         // uint size = 0;
         // while (elt != null)
         // {
         //    uint eltSize = elt.Size;
         //    if (eltSize <= buffer.maxWritableBytes())
         //    {
         //       size += elt.Encode(buffer);
         //    }
         //    else
         //    {
         //       size += eltSize;
         //    }
         //    elt = elt.Next;
         // }
         // return size;

         throw new NotImplementedException();

      }

      public string Format()
      {
         throw new NotImplementedException();
      }

      public void Free()
      {
         throw new NotImplementedException();
      }

      public uint GetArray()
      {
         throw new NotImplementedException();
      }

      public DataType GetArrayType()
      {
         throw new NotImplementedException();
      }

      public Span<byte> GetBinary()
      {
         throw new NotImplementedException();
      }

      public bool GetBoolean()
      {
         throw new NotImplementedException();
      }

      public byte GetByte()
      {
         throw new NotImplementedException();
      }

      public int GetChar()
      {
         throw new NotImplementedException();
      }

      public Decimal128 GetDecimal128()
      {
         throw new NotImplementedException();
      }

      public Decimal32 GetDecimal32()
      {
         throw new NotImplementedException();
      }

      public Decimal64 GetDecimal64()
      {
         throw new NotImplementedException();
      }

      public IDescribedType GetDescribedType()
      {
         throw new NotImplementedException();
      }

      public double GetDouble()
      {
         throw new NotImplementedException();
      }

      public float GetFloat()
      {
         throw new NotImplementedException();
      }

      public int GetInt()
      {
         throw new NotImplementedException();
      }

      public uint GetList()
      {
         throw new NotImplementedException();
      }

      public long GetLong()
      {
         throw new NotImplementedException();
      }

      public uint GetMap()
      {
         throw new NotImplementedException();
      }

      public object GetObject()
      {
         throw new NotImplementedException();
      }

      public object[] GetPrimitiveArray()
      {
         throw new NotImplementedException();
      }

      public IList GetPrimitiveList()
      {
         throw new NotImplementedException();
      }

      public IDictionary GetPrimitiveMap()
      {
         throw new NotImplementedException();
      }

      public short GetShort()
      {
         throw new NotImplementedException();
      }

      public string GetString()
      {
         throw new NotImplementedException();
      }

      public Symbol GetSymbol()
      {
         throw new NotImplementedException();
      }

      public DateTime GetTimestamp()
      {
         throw new NotImplementedException();
      }

      public byte GetUnsignedByte()
      {
         throw new NotImplementedException();
      }

      public uint GetUnsignedInteger()
      {
         throw new NotImplementedException();
      }

      public ulong GetUnsignedLong()
      {
         throw new NotImplementedException();
      }

      public ushort GetUnsignedShort()
      {
         throw new NotImplementedException();
      }

      public Guid GetUUID()
      {
         throw new NotImplementedException();
      }

      public void PutArray(bool described, DataType type)
      {
         PutElement(new ArrayElement(parent, current, described, type));
      }

      public void PutBinary(Span<byte> bytes)
      {
         throw new NotImplementedException();
      }

      public void PutBinary(byte[] bytes)
      {
         throw new NotImplementedException();
      }

      public void PutBoolean(bool b)
      {
         throw new NotImplementedException();
      }

      public void PutByte(byte b)
      {
         throw new NotImplementedException();
      }

      public void PutChar(int c)
      {
         throw new NotImplementedException();
      }

      public void PutDecimal128(Decimal128 d)
      {
         throw new NotImplementedException();
      }

      public void PutDecimal32(Decimal32 d)
      {
         throw new NotImplementedException();
      }

      public void PutDecimal64(Decimal64 d)
      {
         throw new NotImplementedException();
      }

      public void PutDescribed()
      {
         PutElement(new DescribedTypeElement(parent, current));
      }

      public void PutDescribedType(IDescribedType dt)
      {
         throw new NotImplementedException();
      }

      public void PutDouble(double d)
      {
         throw new NotImplementedException();
      }

      public void PutFloat(float f)
      {
         throw new NotImplementedException();
      }

      public void PutInt(int i)
      {
         throw new NotImplementedException();
      }

      public void PutList()
      {
         PutElement(new ListElement(parent, current));
      }

      public void PutLong(long l)
      {
         throw new NotImplementedException();
      }

      public void PutMap()
      {
         PutElement(new MapElement(parent, current));
      }

      public void PutNull()
      {
         PutElement(new NullElement(parent, current));
      }

      public void PutObject(object o)
      {
         throw new NotImplementedException();
      }

      public void PutPrimitiveList(IList list)
      {
         throw new NotImplementedException();
      }

      public void PutPrimitiveMap(IDictionary map)
      {
         throw new NotImplementedException();
      }

      public void PutShort(short s)
      {
         throw new NotImplementedException();
      }

      public void PutString(string str)
      {
         throw new NotImplementedException();
      }

      public void PutSymbol(Symbol symbol)
      {
         throw new NotImplementedException();
      }

      public void PutTimestamp(DateTime t)
      {
         throw new NotImplementedException();
      }

      public void PutUnsignedByte(byte ub)
      {
         throw new NotImplementedException();
      }

      public void PutUnsignedInteger(uint ui)
      {
         throw new NotImplementedException();
      }

      public void PutUnsignedLong(ulong ul)
      {
         throw new NotImplementedException();
      }

      public void PutUnsignedShort(ushort us)
      {
         throw new NotImplementedException();
      }

      public void PutUUID(Guid u)
      {
         throw new NotImplementedException();
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