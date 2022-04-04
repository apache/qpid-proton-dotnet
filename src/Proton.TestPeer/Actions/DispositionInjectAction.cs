/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed With
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance With
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
using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver.Actions
{
   /// <summary>
   /// Action type used to inject the AMQP Performative into a test script to
   /// drive the AMQP connection lifecycle.
   /// </summary>
   public class DispositionInjectAction : AbstractPerformativeInjectAction<Disposition>
   {
      private readonly Disposition disposition = new Disposition();
      private readonly DeliveryStateBuilder stateBuilder;

      public DispositionInjectAction(AMQPTestDriver driver) : base(driver)
      {
         this.stateBuilder = new DeliveryStateBuilder(this);
      }

      public override Disposition Performative => disposition;

      public DispositionInjectAction FromSender()
      {
         disposition.Role = Role.Sender;
         return this;
      }

      public DispositionInjectAction FromReceiver()
      {
         disposition.Role = Role.Receiver;
         return this;
      }

      public DispositionInjectAction WithRole(bool role)
      {
         disposition.Role = RoleExtension.Lookup(role);
         return this;
      }

      public DispositionInjectAction WithRole(Role role)
      {
         disposition.Role = role;
         return this;
      }

      public DispositionInjectAction WithFirst(uint first)
      {
         disposition.First = first;
         return this;
      }

      public DispositionInjectAction WithLast(uint last)
      {
         disposition.Last = last;
         return this;
      }

      public DispositionInjectAction WithSettled(bool settled)
      {
         disposition.Settled = settled;
         return this;
      }

      public DeliveryStateBuilder WithState()
      {
         return stateBuilder;
      }

      public DispositionInjectAction WithState(IDeliveryState state)
      {
         disposition.State = state;
         return this;
      }

      public DispositionInjectAction WithBatchable(bool batchable)
      {
         disposition.Batchable = batchable;
         return this;
      }

      #region Builders for Delivery State types

      public sealed class DeliveryStateBuilder
      {
         private readonly DispositionInjectAction action;

         public DeliveryStateBuilder(DispositionInjectAction action)
         {
            this.action = action;
         }

         public DispositionInjectAction Accepted()
         {
            return action.WithState(new Accepted());
         }

         public DispositionInjectAction Released()
         {
            return action.WithState(new Released());
         }

         public DispositionInjectAction Rejected()
         {
            return action.WithState(new Rejected());
         }

         public DispositionInjectAction Rejected(String condition, String description)
         {
            Rejected rejected = new Rejected
            {
               Error = new ErrorCondition(new Symbol(condition), description)
            };

            return action.WithState(rejected);
         }

         public DispositionInjectAction Modified()
         {
            return action.WithState(new Modified());
         }

         public DispositionInjectAction Modified(bool failed)
         {
            Modified modified = new Modified
            {
               DeliveryFailed = failed
            };

            return action.WithState(modified);
         }

         public DispositionInjectAction Modified(bool failed, bool undeliverableHere)
         {
            Modified modified = new Modified
            {
               DeliveryFailed = failed,
               UndeliverableHere = undeliverableHere
            };

            return action.WithState(modified);
         }

         public TransactionalStateBuilder Transactional()
         {
            TransactionalStateBuilder builder = new TransactionalStateBuilder(action);
            action.WithState(builder.State());
            return builder;
         }
      }

      public sealed class TransactionalStateBuilder
      {
         private readonly DispositionInjectAction action;
         private readonly TransactionalState state = new TransactionalState();

         public TransactionalStateBuilder(DispositionInjectAction action)
         {
            this.action = action;
         }

         public TransactionalState State()
         {
            return state;
         }

         public DispositionInjectAction Also()
         {
            return action;
         }

         public DispositionInjectAction And()
         {
            return action;
         }

         public TransactionalStateBuilder WithTxnId(byte[] txnId)
         {
            state.TxnId = new Binary(txnId);
            return this;
         }

         public TransactionalStateBuilder WithTxnId(Binary txnId)
         {
            state.TxnId = txnId;
            return this;
         }

         public TransactionalStateBuilder WithOutcome(IOutcome outcome)
         {
            state.Outcome = outcome;
            return this;
         }

         public TransactionalStateBuilder WithAccepted()
         {
            return WithOutcome(new Accepted());
         }

         public TransactionalStateBuilder WithReleased()
         {
            return WithOutcome(new Released());
         }

         public TransactionalStateBuilder WithRejected()
         {
            return WithOutcome(new Rejected());
         }

         public TransactionalStateBuilder WithRejected(String condition, String description)
         {
            Rejected rejected = new Rejected
            {
               Error = new ErrorCondition(new Symbol(condition), description)
            };

            return WithOutcome(rejected);
         }

         public TransactionalStateBuilder WithModified()
         {
            return WithOutcome(new Modified());
         }

         public TransactionalStateBuilder WithModified(bool failed)
         {
            Modified modified = new Modified
            {
               DeliveryFailed = failed
            };

            return WithOutcome(modified);
         }

         public TransactionalStateBuilder WithModified(bool failed, bool undeliverableHere)
         {
            Modified modified = new Modified
            {
               DeliveryFailed = failed,
               UndeliverableHere = undeliverableHere
            };

            return WithOutcome(modified);
         }
      }

      #endregion
   }
}