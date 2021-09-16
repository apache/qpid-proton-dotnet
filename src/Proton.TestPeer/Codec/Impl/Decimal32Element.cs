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
   public sealed class Decimal32Element : AtomicElement
   {
      private readonly Decimal32 value;

      public Decimal32Element(IElement parent, IElement prev, Decimal32 value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return IsElementOfArray() ? 4u : 5u;
      }

      public override object Value => value;

      public Decimal32 DecimalValue => value;

      public override DataType DataType => DataType.Decimal32;

      public override uint Encode(Stream stream)
      {
         uint size = GetSize();

         if (stream.IsWritable())
         {
            if (!IsElementOfArray())
            {
               stream.WriteByte(((byte)EncodingCodes.Decimal32));
            }

            stream.WriteUnsignedInt(value.Bits);

            return size;
         }
         else
         {
            return 0u;
         }
      }
   }
}