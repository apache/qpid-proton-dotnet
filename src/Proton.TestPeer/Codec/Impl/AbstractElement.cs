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
using System.Text;
using System.Runtime.CompilerServices;
using System.IO;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public abstract class AbstractElement : IElement
   {
      public AbstractElement(IElement parent, IElement prev)
      {
         Parent = parent;
         Prev = prev;
      }

      public abstract uint GetSize();

      public abstract object Value { get; }

      public abstract DataType DataType { get; }

      public IElement Next { get; set; }
      public IElement Prev { get; set; }
      public IElement Parent { get; set; }

      public abstract IElement Child { get; set; }

      public abstract IElement AddChild(IElement element);

      public abstract bool CanEnter { get; }

      public abstract IElement CheckChild(IElement element);

      public abstract uint Encode(Stream stream);

      public void Render(StringBuilder sb)
      {
         if (CanEnter)
         {
            sb.Append(StartSymbol());
            IElement element = Child;
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
            sb.Append(StopSymbol());
         }
         else
         {
            sb.Append(DataType).Append(' ').Append(Value);
         }
      }

      public IElement ReplaceWith(IElement elt)
      {
         if (Parent != null)
         {
            elt = Parent.CheckChild(elt);
         }

         elt.Prev = Prev;
         elt.Next = Next;
         elt.Parent = Parent;

         if (Prev != null)
         {
            Prev.Next = elt;
         }
         if (Next != null)
         {
            Next.Prev = elt;
         }

         if (Parent != null && Parent.Child == this)
         {
            Parent.Child = elt;
         }

         return elt;
      }

      public override String ToString()
      {
         return String.Format("{0}[{1:X}][parent={2:X}, prev={3:X}, next={4:X}]", this.GetType().Name,
                              RuntimeHelpers.GetHashCode(this),
                              RuntimeHelpers.GetHashCode(Parent),
                              RuntimeHelpers.GetHashCode(Prev),
                              RuntimeHelpers.GetHashCode(Next));
      }

      internal abstract string StartSymbol();

      internal abstract string StopSymbol();

      protected virtual bool IsElementOfArray()
      {
         return Parent is ArrayElement element && !(element.IsDescribed && this == Parent.Child);
      }
   }
}