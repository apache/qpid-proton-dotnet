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

namespace Apache.Qpid.Proton.Test.Driver.Codec
{
   public sealed class UuidElement : AtomicElement
   {
      private Guid value;

      public UuidElement(IElement parent, IElement prev, Guid value) : base(parent, prev)
      {
         this.value = value;
      }

      public override int Size => IsElementOfArray() ? 16 : 17;

      public override object Value { get => value; }

      public override DataType DataType => DataType.Uuid;

      public override int Encode(BinaryWriter writer)
      {
         int size = Size;
         if (writer.MaxWritableBytes() >= size)
         {
            if (size == 17)
            {
               writer.Write(((byte)EncodingCodes.Uuid));
            }

            writer.Write(value.ToByteArray());

            return size;
         }
         else
         {
            return 0;
         }
      }
   }
}