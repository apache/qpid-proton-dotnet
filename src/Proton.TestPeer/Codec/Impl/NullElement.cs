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

namespace Apache.Qpid.Proton.Test.Driver.Codec
{
   public sealed class NullElement : AtomicElement
   {
      public NullElement(IElement parent, IElement prev) : base(parent, prev)
      {
      }

      public override int Size => IsElementOfArray() ? 0 : 1;

      public override object Value => null;

      public override DataType DataType => DataType.Null;

      public override int Encode(BinaryWriter writer)
      {
         if (writer.IsWritable() && !IsElementOfArray())
         {
            writer.Write(((byte)EncodingCodes.Null));
            return 1;
         }

         return 0;
      }
   }
}