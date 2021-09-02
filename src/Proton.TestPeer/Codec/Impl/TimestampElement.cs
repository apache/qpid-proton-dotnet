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

namespace Apache.Qpid.Proton.Test.Driver.Codec.Impl
{
   public sealed class TimestampElement : AtomicElement
   {
      private readonly long value;

      public TimestampElement(IElement parent, IElement prev, long value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return IsElementOfArray() ? 8u : 9u;
      }

      public override object Value => value;

      public DateTime TimeValue => new DateTime(value, DateTimeKind.Utc);

      public override DataType DataType => DataType.Timestamp;

      public override uint Encode(BinaryWriter writer)
      {
         uint size = GetSize();

         if (!writer.IsWritable())
         {
            return 0;
         }

         if (!IsElementOfArray())
         {
            writer.Write(((byte)EncodingCodes.Timestamp));
         }

         writer.Write(value);

         return size;
      }
   }
}
