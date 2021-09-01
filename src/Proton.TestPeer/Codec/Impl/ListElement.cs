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

using System.Collections;
using System.IO;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public class ListElement : AbstractElement
   {
      private IElement first;

      internal ListElement(IElement parent, IElement prev) : base(parent, prev)
      {
      }

      public override uint Size => ComputeSize();

      public override object Value => ListValue;

      public IList ListValue
      {
         get
         {
            IList list = new ArrayList();
            IElement elt = first;
            while (elt != null)
            {
               list.Add(elt.Value);
               elt = elt.Next;
            }

            return list;
         }
      }

      public override DataType DataType => DataType.List;

      public override IElement Child
      {
         get => first;
         set => first = value;
      }

      public override bool CanEnter => true;

      public uint Count
      {
         get
         {
            uint count = 0;
            IElement element = first;

            while (element != null)
            {
               count++;
               element = element.Next;
            }

            return count;
         }
      }

      public override IElement AddChild(IElement element)
      {
         return first = element;
      }

      public override IElement CheckChild(IElement element)
      {
         return element;
      }

      public override uint Encode(BinaryWriter writer)
      {
         uint encodedSize = ComputeSize();

         uint count = 0;
         uint size = 0;
         IElement elt = first;
         while (elt != null)
         {
            count++;
            size += elt.Size;
            elt = elt.Next;
         }

         if (encodedSize > writer.MaxWritableBytes())
         {
            return 0;
         }
         else
         {
            if (IsElementOfArray())
            {
               switch (((ArrayElement)Parent).ConstructorType)
               {
                  case ConstructorType.Tiny:
                     break;
                  case ConstructorType.Small:
                     writer.Write((byte)(size + 1));
                     writer.Write((byte)count);
                     break;
                  case ConstructorType.Large:
                     writer.Write((size + 4));
                     writer.Write(count);
                     break;
               }
            }
            else
            {
               if (count == 0)
               {
                  writer.Write((byte)0x45);
               }
               else if (size <= 254 && count <= 255)
               {
                  writer.Write((byte)0xc0);
                  writer.Write((byte)(size + 1));
                  writer.Write((byte)count);
               }
               else
               {
                  writer.Write((byte)0xd0);
                  writer.Write((size + 4));
                  writer.Write(count);
               }

            }

            elt = first;
            while (elt != null)
            {
               elt.Encode(writer);
               elt = elt.Next;
            }

            return encodedSize;
         }
      }

      internal override string StartSymbol()
      {
         return "[";
      }

      internal override string StopSymbol()
      {
         return "]";
      }

      private uint ComputeSize()
      {
         uint count = 0;
         uint size = 0;
         IElement elt = first;
         while (elt != null)
         {
            count++;
            size += elt.Size;
            elt = elt.Next;
         }
         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;
            if (parent.ConstructorType == ConstructorType.Tiny)
            {
               if (count != 0)
               {
                  parent.ConstructorType = ConstructorType.Small;
                  size += 2;
               }
            }
            else if (parent.ConstructorType == ConstructorType.Small)
            {
               if (count > 255 || size > 254)
               {
                  parent.ConstructorType = ConstructorType.Large;
                  size += 8;
               }
               else
               {
                  size += 2;
               }
            }
            else
            {
               size += 8;
            }

         }
         else
         {
            if (count == 0)
            {
               size = 1;
            }
            else if (count <= 255 && size <= 254)
            {
               size += 3;
            }
            else
            {
               size += 9;
            }
         }

         return size;
      }
   }
}