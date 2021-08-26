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

using System.IO;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public sealed class IntegerElement : AtomicElement
   {
      private int value;

      public IntegerElement(IElement parent, IElement prev, int value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint Size => ComputeSize();

      public override object Value => value;

      public override DataType DataType => DataType.Int;

      public override uint Encode(BinaryWriter writer)
      {
         uint size = ComputeSize();

         if (size <= writer.MaxWritableBytes())
         {
            switch (size)
            {
               case 1: // Array Element
                  writer.Write((byte)value);
                  break;
               case 2:
                  writer.Write(((byte)EncodingCodes.SmallInt));
                  writer.Write((byte)value);
                  break;
               case 4: // Array Element
                  writer.Write(value);
                  break;
               case 5:
                  writer.Write(((byte)EncodingCodes.Int));
                  writer.Write(value);
                  break;
            }

            return size;
         }
         return 0u;
      }

      private uint ComputeSize()
      {
         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;
            if (parent.ConstructorType == ConstructorType.Small)
            {
               if (-128 <= value && value <= 127)
               {
                  return 1;
               }
               else
               {
                  parent.ConstructorType = ConstructorType.Large;
                  return 4;
               }
            }
            else
            {
               return 4;
            }
         }
         else
         {
            return (-128 <= value && value <= 127) ? 2u : 5u;
         }
      }
   }
}