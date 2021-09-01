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
   public sealed class ShortElement : AtomicElement
   {
      private short value;

      public ShortElement(IElement parent, IElement prev, short value) : base(parent, prev)
      {
         this.value = value;
      }

      public override uint Size => IsElementOfArray() ? 2u : 3u;

      public override object Value => value;

      public short ShortValue => value;

      public override DataType DataType => DataType.Short;

      public override uint Encode(BinaryWriter writer)
      {
         if (IsElementOfArray())
         {
            if (writer.MaxWritableBytes() >= 2)
            {
               writer.Write(value);
               return 2u;
            }
         }
         else
         {
            if (writer.MaxWritableBytes() >= 3)
            {
               writer.Write(((byte)EncodingCodes.Short));
               writer.Write(value);
               return 3u;
            }
         }

         return 0u;
      }
   }
}