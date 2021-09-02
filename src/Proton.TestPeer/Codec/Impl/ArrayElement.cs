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
   public class ArrayElement : AbstractElement
   {
      private IElement first;
      private DataType arrayType;
      private bool described;
      private ConstructorType constructorType;

      internal ArrayElement(IElement parent, IElement prev, bool described, DataType type) : base(parent, prev)
      {
         if (type == DataType.Described)
         {
            throw new ArgumentException("Array type cannot be DESCRIBED");
         }

         IsDescribed = described;
         ArrayType = type;

         switch (ArrayType)
         {
            case DataType.UInt:
            case DataType.ULong:
            case DataType.List:
               ConstructorType = ConstructorType.Tiny;
               break;
            default:
               ConstructorType = ConstructorType.Small;
               break;
         }
      }

      internal ConstructorType ConstructorType
      {
         get => constructorType;
         set => constructorType = value;
      }

      public bool IsDescribed
      {
         get => described;
         init => described = value;
      }

      public override IElement Child { get => first; set => first = value; }

      public override bool CanEnter => true;

      public override uint GetSize()
      {
         return ComputeSize();
      }

      public override object Value => ExtrapolateValue();

      public Array ArrayValue => ExtrapolateValue();

      public override DataType DataType => DataType.Array;

      public DataType ArrayType
      {
         get => arrayType;
         init => arrayType = value;
      }

      public override IElement AddChild(IElement element)
      {
         if (IsDescribed || element.DataType == ArrayType)
         {
            first = element;
            return element;
         }
         else
         {
            IElement replacement = Coerce(element);
            if (replacement != null)
            {
               first = replacement;
               return replacement;
            }
            throw new ArgumentException("Attempting to add instance of " +
                                        element.DataType + " to array of " + ArrayType);
         }
      }

      public override IElement CheckChild(IElement element)
      {
         if (element.DataType != ArrayType)
         {
            IElement replacement = Coerce(element);
            if (replacement != null)
            {
               return replacement;
            }
            throw new ArgumentException("Attempting to add instance of " +
                                        element.DataType + " to array of " + ArrayType);
         }
         return element;
      }

      private IElement Coerce(IElement element)
      {
         switch (arrayType)
         {
            case DataType.Int:
               int i;
               switch (element.DataType)
               {
                  case DataType.Byte:
                     i = ((ByteElement)element).SByteValue;
                     break;
                  case DataType.Short:
                     i = ((ShortElement)element).ShortValue;
                     break;
                  case DataType.Long:
                     i = (int)((LongElement)element).LongValue;
                     break;
                  default:
                     return null;
               }
               return new IntegerElement(element.Parent, element.Prev, i);
            case DataType.Long:
               long l;
               switch (element.DataType)
               {
                  case DataType.Byte:
                     l = ((ByteElement)element).SByteValue;
                     break;
                  case DataType.Short:
                     l = ((ShortElement)element).ShortValue;
                     break;
                  case DataType.Int:
                     l = ((IntegerElement)element).IntegerValue;
                     break;
                  default:
                     return null;
               }
               return new LongElement(element.Parent, element.Prev, l);
            case DataType.Array:
               break;
            case DataType.Binary:
               break;
            case DataType.Bool:
               break;
            case DataType.Byte:
               break;
            case DataType.Char:
               break;
            case DataType.Decimal32:
               break;
            case DataType.Decimal64:
               break;
            case DataType.Decimal128:
               break;
            case DataType.Described:
               break;
            case DataType.Double:
               break;
            case DataType.Float:
               break;
            case DataType.List:
               break;
            case DataType.Map:
               break;
            case DataType.Null:
               break;
            case DataType.Short:
               break;
            case DataType.String:
               break;
            case DataType.Symbol:
               break;
            case DataType.Timestamp:
               break;
            case DataType.UByte:
               break;
            case DataType.UInt:
               break;
            case DataType.ULong:
               break;
            case DataType.UShort:
               break;
            case DataType.Uuid:
               break;
            default:
               break;
         }

         return null;
      }

      public override uint Encode(BinaryWriter writer)
      {
         uint size = GetSize();
         uint count = Count;

         if (writer.IsWritable())
         {
            if (!IsElementOfArray())
            {
               if (size > 257 || count > 255)
               {
                  writer.Write((byte)0xf0);
                  writer.Write(size - 5);
                  writer.Write(count);
               }
               else
               {
                  writer.Write((byte)0xe0);
                  writer.Write((byte)(size - 2));
                  writer.Write((byte)count);
               }
            }
            else
            {
               ArrayElement parent = (ArrayElement)Parent;
               if (parent.ConstructorType == ConstructorType.Tiny)
               {
                  writer.Write((byte)(size - 1));
                  writer.Write((byte)count);
               }
               else
               {
                  writer.Write(size - 4);
                  writer.Write(count);
               }
            }
            IElement element = first;
            if (IsDescribed)
            {
               writer.Write((byte)0);
               if (element == null)
               {
                  writer.Write((byte)0x40);
               }
               else
               {
                  element.Encode(writer);
                  element = element.Next;
               }
            }
            switch (arrayType)
            {
               case DataType.Null:
                  writer.Write((byte)0x40);
                  break;
               case DataType.Bool:
                  writer.Write((byte)0x56);
                  break;
               case DataType.UByte:
                  writer.Write((byte)0x50);
                  break;
               case DataType.Byte:
                  writer.Write((byte)0x51);
                  break;
               case DataType.UShort:
                  writer.Write((byte)0x60);
                  break;
               case DataType.Short:
                  writer.Write((byte)0x61);
                  break;
               case DataType.UInt:
                  switch (ConstructorType)
                  {
                     case ConstructorType.Tiny:
                        writer.Write((byte)0x43);
                        break;
                     case ConstructorType.Small:
                        writer.Write((byte)0x52);
                        break;
                     case ConstructorType.Large:
                        writer.Write((byte)0x70);
                        break;
                  }
                  break;
               case DataType.Int:
                  writer.Write(ConstructorType == ConstructorType.Small ? (byte)0x54 : (byte)0x71);
                  break;
               case DataType.Char:
                  writer.Write((byte)0x73);
                  break;
               case DataType.ULong:
                  switch (ConstructorType)
                  {
                     case ConstructorType.Tiny:
                        writer.Write((byte)0x44);
                        break;
                     case ConstructorType.Small:
                        writer.Write((byte)0x53);
                        break;
                     case ConstructorType.Large:
                        writer.Write((byte)0x80);
                        break;
                  }
                  break;
               case DataType.Long:
                  writer.Write(ConstructorType == ConstructorType.Small ? (byte)0x55 : (byte)0x81);
                  break;
               case DataType.Timestamp:
                  writer.Write((byte)0x83);
                  break;
               case DataType.Float:
                  writer.Write((byte)0x72);
                  break;
               case DataType.Double:
                  writer.Write((byte)0x82);
                  break;
               case DataType.Decimal32:
                  writer.Write((byte)0x74);
                  break;
               case DataType.Decimal64:
                  writer.Write((byte)0x84);
                  break;
               case DataType.Decimal128:
                  writer.Write((byte)0x94);
                  break;
               case DataType.Uuid:
                  writer.Write((byte)0x98);
                  break;
               case DataType.Binary:
                  writer.Write(ConstructorType == ConstructorType.Small ? (byte)0xa0 : (byte)0xb0);
                  break;
               case DataType.String:
                  writer.Write(ConstructorType == ConstructorType.Small ? (byte)0xa1 : (byte)0xb1);
                  break;
               case DataType.Symbol:
                  writer.Write(ConstructorType == ConstructorType.Small ? (byte)0xa3 : (byte)0xb3);
                  break;
               case DataType.Array:
                  writer.Write(ConstructorType == ConstructorType.Small ? (byte)0xe0 : (byte)0xf0);
                  break;
               case DataType.List:
                  writer.Write(ConstructorType == ConstructorType.Tiny ? (byte)0x45 : ConstructorType == ConstructorType.Small ? (byte)0xc0 : (byte)0xd0);
                  break;
               case DataType.Map:
                  writer.Write(ConstructorType == ConstructorType.Small ? (byte)0xc1 : (byte)0xd1);
                  break;
               case DataType.Described:
                  break;
               default:
                  break;
            }
            while (element != null)
            {
               element.Encode(writer);
               element = element.Next;
            }
            return size;
         }
         else
         {
            return 0;
         }
      }

      internal override string StartSymbol()
      {
         return String.Format("{0}{1}[", IsDescribed ? "D" : "", ArrayType);
      }

      internal override string StopSymbol()
      {
         return "]";
      }

      public uint Count
      {
         get
         {
            uint count = 0;
            IElement elt = first;
            while (elt != null)
            {
               count++;
               elt = elt.Next;
            }
            if (IsDescribed && count != 0)
            {
               count--;
            }
            return count;
         }
      }

      private uint ComputeSize()
      {
         ConstructorType oldConstructorType;
         uint bodySize;
         uint count = 0;
         do
         {
            bodySize = 1; // data type constructor
            oldConstructorType = ConstructorType;
            IElement element = first;
            while (element != null)
            {
               count++;
               bodySize += element.GetSize();
               element = element.Next;
            }
         } while (oldConstructorType != ConstructorType);

         if (IsDescribed)
         {
            bodySize++; // 00 instruction
            if (count != 0)
            {
               count--;
            }
         }

         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;
            if (parent.ConstructorType == ConstructorType.Small)
            {
               if (count <= 255 && bodySize <= 254)
               {
                  bodySize += 2;
               }
               else
               {
                  parent.ConstructorType = ConstructorType.Large;
                  bodySize += 8;
               }
            }
            else
            {
               bodySize += 8;
            }
         }
         else
         {

            if (count <= 255 && bodySize <= 254)
            {
               bodySize += 3;
            }
            else
            {
               bodySize += 9;
            }

         }

         return bodySize;
      }

      private object[] ExtrapolateValue()
      {
         if (IsDescribed)
         {
            IDescribedType[] rVal = new IDescribedType[Count];
            object descriptor = first == null ? null : first.Value;
            IElement element = first == null ? null : first.Next;
            int i = 0;
            while (element != null)
            {
               rVal[i++] = new DescribedType(descriptor, element.Value);
               element = element.Next;
            }
            return rVal;
         }
         else if (ArrayType == DataType.Symbol)
         {
            Symbol[] rVal = new Symbol[Count];
            SymbolElement element = (SymbolElement)first;
            int i = 0;
            while (element != null)
            {
               rVal[i++] = element.SymbolValue;
               element = (SymbolElement)element.Next;
            }
            return rVal;
         }
         else
         {
            object[] rVal = new object[Count];
            IElement element = first;
            int i = 0;
            while (element != null)
            {
               rVal[i++] = element.Value;
               element = element.Next;
            }
            return rVal;
         }
      }
   }
}