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
   public sealed class LongElement : AtomicElement
   {
      private readonly long value;

      public LongElement(IElement parent, IElement prev, long value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint GetSize()
      {
         return ComputeSize();
      }

      public override object Value => value;

      public long LongValue => value;

      public override DataType DataType => DataType.Long;

      public override uint Encode(Stream stream)
      {
         uint size = ComputeSize();

         if (!stream.IsWritable())
         {
            return 0;
         }

         switch (size)
         {
            case 1: // Array Element
               stream.WriteByte((byte)value);
               break;
            case 2:
               stream.WriteByte((byte)EncodingCodes.SmallLong);
               stream.WriteByte((byte)value);
               break;
            case 8: // Array Element
               stream.WriteLong(value);
               break;
            case 9:
               stream.WriteByte((byte)EncodingCodes.Long);
               stream.WriteLong(value);
               break;
         }

         return size;
      }

      private uint ComputeSize()
      {
         if (IsElementOfArray())
         {
            ArrayElement parent = (ArrayElement)Parent;

            if (parent.ConstructorType == ConstructorType.Small)
            {
               if (value is >= (-128L) and <= 127L)
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
            return (value is >= (-128L) and <= 127L) ? 2u : 9u;
         }
      }
   }
}