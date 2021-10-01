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
using System.Collections.Generic;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Types.Transactions
{
   public sealed class Declared : IDeliveryState, IOutcome, ICloneable
   {
      public static readonly ulong DescriptorCode = 0x0000000000000033UL;
      public static readonly Symbol DescriptorSymbol = Symbol.Lookup("amqp:declared:list");

      private IProtonBuffer txnId;

      public Declared() : base() { }

      public Declared(Declared other) : this()
      {
         this.txnId = other.TxnId?.Copy();
      }

      public IProtonBuffer TxnId
      {
         get => txnId;
         set
         {
            if (value == null)
            {
               throw new ArgumentNullException("Transaction ID is mandatory and cannot be set to null");
            }

            txnId = value;
         }
      }

      public DeliveryStateType Type => DeliveryStateType.Declared;

      public object Clone()
      {
         return new Declared(this);
      }

      public override bool Equals(object obj)
      {
         return obj is Declared declared &&
                EqualityComparer<IProtonBuffer>.Default.Equals(txnId, declared.txnId) &&
                Type == declared.Type;
      }

      public bool Equals(IDeliveryState other)
      {
         return other is Declared declared &&
                EqualityComparer<IProtonBuffer>.Default.Equals(txnId, declared.txnId) &&
                Type == declared.Type;
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public override string ToString()
      {
         return "Declared{" + "txnId=" + StringUtils.ToQuotedString(TxnId) + '}';
      }
   }
}