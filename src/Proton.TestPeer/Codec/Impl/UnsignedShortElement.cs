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
   public sealed class UnsignedShortElement : AtomicElement
   {
      private readonly ushort value;

      public UnsignedShortElement(IElement parent, IElement prev, ushort value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return IsElementOfArray() ? 2u : 3u;
      }

      public override object Value => value;

      public ushort UShortValue => value;

      public override DataType DataType => DataType.UShort;

      public override uint Encode(Stream stream)
      {
         if (IsElementOfArray())
         {
            if (stream.IsWritable())
            {
               stream.WriteUnsignedShort(value);
               return 2;
            }
         }
         else
         {
            if (stream.IsWritable())
            {
               stream.WriteByte(((byte)EncodingCodes.UShort));
               stream.WriteUnsignedShort(value);
               return 3;
            }
         }

         return 0;
      }
   }
}