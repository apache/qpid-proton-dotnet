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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public sealed class BinaryElement : AtomicElement
   {
      private readonly Binary value;

      public BinaryElement(IElement parent, IElement prev, Binary value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return ComputeSize();
      }

      public override object Value => value;

      public Binary BinaryValue => value;

      public override DataType DataType => DataType.Binary;

      public override uint Encode(BinaryWriter writer)
      {
         uint size = GetSize();
         if (writer.IsWritable())
         {
            return 0;
         }

         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;

            if (parent.ConstructorType == ConstructorType.Small)
            {
               writer.Write((byte)value.Length);
            }
            else
            {
               writer.Write(value.Length);
            }
         }
         else if (value.Length <= 255)
         {
            writer.Write(((byte)EncodingCodes.VBin8));
            writer.Write((byte)value.Length);
         }
         else
         {
            writer.Write(((byte)EncodingCodes.VBin32));
            writer.Write(value.Length);
         }

         writer.Write(value.Array, 0, value.Length);

         return size;
      }

      private uint ComputeSize()
      {
         uint length = (uint)value.Length;

         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;

            if (parent.ConstructorType == ConstructorType.Small)
            {
               if (length > 255)
               {
                  parent.ConstructorType = ConstructorType.Large;
                  return 4 + length;
               }
               else
               {
                  return 1 + length;
               }
            }
            else
            {
               return 4 + length;
            }
         }
         else
         {
            if (length > 255)
            {
               return 5 + length;
            }
            else
            {
               return 2 + length;
            }
         }
      }
   }
}