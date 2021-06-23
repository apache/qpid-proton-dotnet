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

namespace Apache.Qpid.Proton.Types.Transactions
{
   public sealed class Declare : ICloneable
   {
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000031UL;
      public static readonly Symbol DESCRIPTOR_SYMBOL = Symbol.Lookup("amqp:declare:list");

      public Declare() : base() { }

      public Declare(Declare other) : this()
      {
         GlobalTxnId = (GlobalTxnId)(other.GlobalTxnId?.Clone());
      }

      public GlobalTxnId GlobalTxnId { get; set; }

      public object Clone()
      {
         return new Declare(this);
      }

      public override string ToString()
      {
         return "Declare{" + "globalTxnId=" + GlobalTxnId + '}';
      }
   }
}