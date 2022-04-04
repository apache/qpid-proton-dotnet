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
   public sealed class UnsignedIntegerElement : AtomicElement
   {
      private readonly uint value;

      public UnsignedIntegerElement(IElement parent, IElement prev, uint value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return ComputeSize();
      }

      public override object Value => value;

      public uint UIntValue => value;

      public override DataType DataType => DataType.UInt;

      public override uint Encode(Stream stream)
      {
         uint size = ComputeSize();

         if (!stream.IsWritable())
         {
            return 0;
         }

         switch (size)
         {
            case 1:
               if (IsElementOfArray())
               {
                  stream.WriteByte((byte)value);
               }
               else
               {
                  stream.WriteByte(((byte)EncodingCodes.UInt0));
               }
               break;
            case 2:
               stream.WriteByte((byte)EncodingCodes.SmallUInt);
               stream.WriteByte((byte)value);
               break;
            case 4: // Array Element
               stream.WriteUnsignedInt(value);
               break;
            case 5:
               stream.WriteByte((byte)EncodingCodes.UInt);
               stream.WriteUnsignedInt(value);
               break;
         }

         return size;
      }

      private uint ComputeSize()
      {
         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;
            if (parent.ConstructorType == ConstructorType.Tiny)
            {
               if (value == 0)
               {
                  return 0;
               }
               else
               {
                  parent.ConstructorType = ConstructorType.Small;
               }
            }

            if (parent.ConstructorType == ConstructorType.Small)
            {
               if (0 <= value && value <= 255)
               {
                  return 1;
               }
               else
               {
                  parent.ConstructorType = ConstructorType.Large;
               }
            }

            return 4;
         }
         else
         {
            return 0 == value ? 1u : (1 <= value && value <= 255) ? 2u : 5u;
         }
      }
   }
}