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
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Messaging
{
   public sealed class Data : IDescribedType
   {
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000075UL;
      public static readonly Symbol DESCRIPTOR_SYMBOL = new Symbol("amqp:data:binary");

      private Binary described;

      public Data(Binary described)
      {
         if (described == null)
         {
            throw new ArgumentNullException("provided Binary must not be null");
         }

         this.described = described;
      }

      public object Descriptor => DESCRIPTOR_SYMBOL;

      public object Described => described;

      public override string ToString()
      {
         return "Data: [ " + Described + " ]";
      }
   }
}