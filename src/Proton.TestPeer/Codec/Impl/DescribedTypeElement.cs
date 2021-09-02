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
   public sealed class DescribedTypeElement : AtomicElement
   {
      private IElement first;

      internal DescribedTypeElement(IElement parent, IElement prev) : base(parent, prev)
      {
      }

      public override uint GetSize()
      {
         uint count = 0;
         uint size = 0;
         IElement elt = first;
         while (elt != null)
         {
            count++;
            size += elt.GetSize();
            elt = elt.Next;
         }

         if (IsElementOfArray())
         {
            throw new ArgumentException("Cannot add described type members to an array");
         }
         else if (count > 2)
         {
            throw new ArgumentException("Too many elements in described type");
         }
         else if (count == 0)
         {
            size = 3;
         }
         else if (count == 1)
         {
            size += 2;
         }
         else
         {
            size += 1;
         }

         return size;
      }

      public override object Value => DescribedValue;

      public IDescribedType DescribedValue
      {
         get
         {
            object descriptor = first == null ? null : first.Value;
            IElement second = first == null ? null : first.Next;
            object described = second == null ? null : second.Value;
            return DescribedTypeRegistry.LookupDescribedType(descriptor, described);
         }
      }

      public override DataType DataType => DataType.Described;

      public override bool CanEnter => true;

      public override IElement Child
      {
         get => first;
         set => first = value;
      }

      public override IElement CheckChild(IElement element)
      {
         if (element.Prev != first)
         {
            throw new ArgumentException("Described Type may only have two elements");
         }
         return element;
      }

      public override IElement AddChild(IElement element)
      {
         first = element;
         return element;
      }

      public override uint Encode(BinaryWriter writer)
      {
         uint encodedSize = GetSize();

         if (!writer.IsWritable())
         {
            return 0;
         }
         else
         {
            writer.Write((byte)0);
            if (first == null)
            {
               writer.Write((byte)0x40);
               writer.Write((byte)0x40);
            }
            else
            {
               first.Encode(writer);
               if (first.Next == null)
               {
                  writer.Write((byte)0x40);
               }
               else
               {
                  first.Next.Encode(writer);
               }
            }
         }

         return encodedSize;
      }

      internal override string StartSymbol()
      {
         return "(";
      }

      internal override string StopSymbol()
      {
         return ")";
      }
   }
}