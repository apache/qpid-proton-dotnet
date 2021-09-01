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

      internal ConstructorType ConstructorType { get; set; }

      public bool IsDescribed { get; init; }

      public override IElement Child { get => first; set => first = value; }

      public override bool CanEnter => true;

      public override uint Size => ComputeSize();

      public override object Value => ExtrapolateValue();

      public Array ArrayValue => ExtrapolateValue();

      public override DataType DataType => DataType.Array;

      public DataType ArrayType { get; init; }

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
         // TODO:
         throw new NotImplementedException();
      }

      public override uint Encode(BinaryWriter writer)
      {
         // TODO:
         throw new NotImplementedException();
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
               bodySize += element.Size;
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
            // TODO
            // object descriptor = first == null ? null : first.Value;
            // IElement element = first == null ? null : first.Next;
            // int i = 0;
            // while (element != null)
            // {
            //    rVal[i++] = new DescribedTypeImpl(descriptor, element.Value);
            //    element = element.Next;
            // }
            return rVal;
         }
         else if (ArrayType == DataType.Symbol)
         {
            Symbol[] rVal = new Symbol[Count];
            // TODO
            // SymbolElement element = (SymbolElement)first;
            // int i = 0;
            // while (element != null)
            // {
            //    rVal[i++] = element.Value;
            //    element = (SymbolElement)element.Next;
            // }
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