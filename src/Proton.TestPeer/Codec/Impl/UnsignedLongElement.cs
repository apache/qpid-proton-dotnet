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
   public sealed class UnsignedLongElement : AtomicElement
   {
      private readonly ulong value;

      public UnsignedLongElement(IElement parent, IElement prev, ulong value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return ComputeSize();
      }

      public override object Value => value;

      public ulong ULongValue => value;

      public override DataType DataType => DataType.ULong;

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
                  stream.WriteByte((byte)EncodingCodes.ULong0);
               }
               break;
            case 2:
               stream.WriteByte((byte)EncodingCodes.SmallULong);
               stream.WriteByte((byte)value);
               break;
            case 8: // Array Element
               stream.WriteUnsignedLong(value);
               break;
            case 9:
               stream.WriteByte((byte)EncodingCodes.ULong);
               stream.WriteUnsignedLong(value);
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
               if (value == 0ul)
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
               if (value is >= 0ul and <= 255ul)
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
            return 0ul == value ? 1u : (value is >= 1ul and <= 255ul) ? 2u : 9u;
         }
      }
   }
}