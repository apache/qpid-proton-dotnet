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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public class MapElement : AbstractElement
   {
      private IElement first;

      internal MapElement(IElement parent, IElement prev) : base(parent, prev)
      {
      }

      public override uint GetSize()
      {
         return ComputeSize();
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
            return count;
         }
      }

      public override object Value => MapValue;

      public IDictionary MapValue
      {
         get
         {
            IDictionary map = new Hashtable();
            IElement elt = first;
            while (elt != null)
            {
               object key = elt.Value;
               object value;
               elt = elt.Next;
               if (elt != null)
               {
                  value = elt.Value;
                  elt = elt.Next;
               }
               else
               {
                  value = null;
               }
               map.Add(key, value);
            }

            return map;
         }
      }

      public override DataType DataType => DataType.Map;

      public override IElement Child
      {
         get => first;
         set => first = value;
      }

      public override bool CanEnter => true;

      public override IElement AddChild(IElement element)
      {
         return first = element;
      }

      public override IElement CheckChild(IElement element)
      {
         return element;
      }

      public override uint Encode(Stream stream)
      {
         uint encodedSize = ComputeSize();

         uint count = 0;
         uint size = 0;
         IElement elt = first;
         while (elt != null)
         {
            count++;
            size += elt.GetSize();
            elt = elt.Next;
         }

         if (!stream.IsWritable())
         {
            return 0;
         }
         else
         {
            if (IsElementOfArray())
            {
               switch (((ArrayElement)Parent).ConstructorType)
               {
                  case ConstructorType.Small:
                     stream.WriteByte((byte)(size + 1));
                     stream.WriteByte((byte)count);
                     break;
                  case ConstructorType.Large:
                     stream.WriteUnsignedInt(size + 4);
                     stream.WriteUnsignedInt(count);
                     break;
                  case ConstructorType.Tiny:
                     break;
                  default:
                     break;
               }
            }
            else
            {
               if (size <= 254 && count <= 255)
               {
                  stream.WriteByte((byte)0xc1);
                  stream.WriteByte((byte)(size + 1));
                  stream.WriteByte((byte)count);
               }
               else
               {
                  stream.WriteByte((byte)0xd1);
                  stream.WriteUnsignedInt(size + 4);
                  stream.WriteUnsignedInt(count);
               }
            }

            elt = first;
            while (elt != null)
            {
               elt.Encode(stream);
               elt = elt.Next;
            }

            return encodedSize;
         }
      }

      internal override string StartSymbol()
      {
         return "{";
      }

      internal override string StopSymbol()
      {
         return "}";
      }

      private uint ComputeSize()
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
            ArrayElement parent = (ArrayElement)Parent;

            if (parent.ConstructorType == ConstructorType.Small)
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
            if (count <= 255 && size <= 254)
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