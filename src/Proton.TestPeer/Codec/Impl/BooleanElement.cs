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

namespace Apache.Qpid.Proton.Test.Driver.Codec
{
   public sealed class BooleanElement : AtomicElement
   {
      private readonly bool value;

      public BooleanElement(IElement parent, IElement prev, bool value) : base(parent, prev)
      {
         this.value = value;
      }

      /// <summary>
      /// in non-array parent then there is a single byte encoding, in an array
      /// there is a 1-byte encoding but no constructor
      /// </summary>
      public override int Size => 1;

      public override object Value => value;

      public override DataType DataType => DataType.Bool;

      public override int Encode(BinaryWriter writer)
      {
         if (writer.IsWritable())
         {
            if (IsElementOfArray())
            {
               writer.Write(value ? (byte)1 : (byte)0);
            }
            else
            {
               writer.Write(value ? ((byte)EncodingCodes.BooleanTrue) : ((byte)EncodingCodes.BooleanFalse));
            }

            return 1;
         }
         else
         {
            return 0;
         }
      }
   }
}