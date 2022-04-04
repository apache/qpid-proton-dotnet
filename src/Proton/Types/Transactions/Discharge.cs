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
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Types.Transactions
{
   public sealed class Discharge : ICloneable
   {
      public static readonly ulong DescriptorCode = 0x0000000000000032UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:discharge:list");

      private IProtonBuffer txnId;

      public Discharge() : base() { }

      public Discharge(Discharge other) : this()
      {
         TxnId = other.TxnId?.Copy();
         Fail = other.Fail;
      }

      public IProtonBuffer TxnId
      {
         get => txnId;
         set
         {
            txnId = value ?? throw new ArgumentNullException(nameof(txnId), "The TXN Id value is mandatory and cannot be set to null");
         }
      }

      public bool Fail { get; set; }

      public object Clone()
      {
         return new Discharge(this);
      }

      public override string ToString()
      {
         return "Discharge{" + "txnId=" + TxnId + ", fail=" + Fail + '}';
      }
   }
}