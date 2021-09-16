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
   public sealed class Decimal128Element : AtomicElement
   {
      private readonly Decimal128 value;

      public Decimal128Element(IElement parent, IElement prev, Decimal128 value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return IsElementOfArray() ? 16u : 17u;
      }

      public override object Value => value;

      public Decimal128 DecimalValue => value;

      public override DataType DataType => DataType.Decimal128;

      public override uint Encode(Stream stream)
      {
         uint size = GetSize();

         if (stream.IsWritable())
         {
            if (!IsElementOfArray())
            {
               stream.WriteByte(((byte)EncodingCodes.Decimal128));
            }

            stream.WriteUnsignedLong(value.MostSignificantBits);
            stream.WriteUnsignedLong(value.LeastSignificantBits);

            return size;
         }
         else
         {
            return 0u;
         }
      }
   }
}