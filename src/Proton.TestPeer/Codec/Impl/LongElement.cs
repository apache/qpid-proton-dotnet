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

namespace Apache.Qpid.Proton.Test.Driver.Codec
{
   public sealed class LongElement : AtomicElement
   {
      private long value;

      public LongElement(IElement parent, IElement prev, long value) : base(parent, prev)
      {
         this.value = value;
      }

      public override int Size => ComputeSize();

      public override object Value => value;

      public override DataType DataType => DataType.Long;

      public override int Encode(BinaryWriter writer)
      {
         int size = ComputeSize();

         if (size > writer.MaxWritableBytes())
         {
            return 0;
         }

         switch (size)
         {
            case 1: // Array Element
               writer.Write((byte)value);
               break;
            case 2:
               writer.Write(((byte)EncodingCodes.SmallLong));
               writer.Write((byte)value);
               break;
            case 8: // Array Element
               writer.Write(value);
               break;
            case 9:
               writer.Write(((byte)EncodingCodes.Long));
               writer.Write(value);
               break;
         }

         return size;
      }

      private int ComputeSize()
      {
         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;

            if (parent.ConstructorType == ConstructorType.Small)
            {
               if (-128L <= value && value <= 127L)
               {
                  return 1;
               }
               else
               {
                  parent.ConstructorType = ConstructorType.Large;
               }
            }

            return 8;

         }
         else
         {
            return (-128L <= value && value <= 127L) ? 2 : 9;
         }
      }
   }
}