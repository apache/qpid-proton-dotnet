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
using System.Text;

namespace Apache.Qpid.Proton.Test.Driver.Codec
{
   public sealed class StringElement : AtomicElement
   {
      private readonly string value;

      public StringElement(IElement parent, IElement prev, string value) : base(parent, prev)
      {
         this.value = value;
      }

      public override int Size => ComputeSize(new UTF8Encoding().GetBytes(value).Length);

      public override object Value => value;

      public override DataType DataType => DataType.String;

      public override int Encode(BinaryWriter writer)
      {
         byte[] bytes = new UTF8Encoding().GetBytes(value);
         int length = bytes.Length;

         int size = ComputeSize(length);

         if (writer.MaxWritableBytes() < size)
         {
            return 0;
         }
         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement) Parent;

            if (parent.ConstructorType == ConstructorType.Small)
            {
               writer.Write((byte)length);
            }
            else
            {
               writer.Write(length);
            }
         }
         else if (length <= 255)
         {
            writer.Write(((byte)EncodingCodes.Str8));
            writer.Write((byte)length);
         }
         else
         {
            writer.Write(((byte)EncodingCodes.Str32));
            writer.Write(length);
         }

         writer.Write(bytes);
         return size;
      }

      private int ComputeSize(int length)
      {
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