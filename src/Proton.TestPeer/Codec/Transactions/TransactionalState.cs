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
using System.Collections;
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Codec.Transactions
{
   public enum TransactionalStateField
   {
      TxnId,
      Outcome
   }

   public sealed class TransactionalState : ListDescribedType, IDeliveryState
   {
      public static readonly Symbol DESCRIPTOR_SYMBOL = new("amqp:transactional-state:list");
      public static readonly ulong DESCRIPTOR_CODE = 0x0000000000000034ul;

      public TransactionalState() : base(Enum.GetNames(typeof(TransactionalStateField)).Length)
      {
      }

      public TransactionalState(object described) : base(Enum.GetNames(typeof(TransactionalStateField)).Length, (IList)described)
      {
      }

      public TransactionalState(IList described) : base(Enum.GetNames(typeof(TransactionalStateField)).Length, described)
      {
      }

      public override object Descriptor => DESCRIPTOR_SYMBOL;

      public DeliveryStateType Type => DeliveryStateType.Transactional;

      public Binary TxnId
      {
         get => (Binary)List[((int)TransactionalStateField.TxnId)];
         set => List[((int)TransactionalStateField.TxnId)] = value;
      }

      public IOutcome Outcome
      {
         get => (IOutcome)List[((int)TransactionalStateField.Outcome)];
         set => List[((int)TransactionalStateField.Outcome)] = value;
      }

      public override string ToString()
      {
         return "TransactionalState{" + "txnId=" + TxnId + ", outcome=" + Outcome + '}';
      }
   }
}
