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
   public sealed class CharElement : AtomicElement
   {
      private readonly char value;

      public CharElement(IElement parent, IElement prev, char value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint Size => IsElementOfArray() ? 4u : 5u;

      public override object Value => value;

      public override DataType DataType => DataType.Char;

      public override uint Encode(BinaryWriter writer)
      {
         uint size = Size;

         if (size <= writer.MaxWritableBytes())
         {
            if (!IsElementOfArray())
            {
               writer.Write(((byte)EncodingCodes.Char));
            }

            writer.Write((int)value);

            return size;
         }
         else
         {
            return 0u;
         }
      }
   }
}