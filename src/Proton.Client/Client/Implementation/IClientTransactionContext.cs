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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   internal interface ISendable
   {
      /// <summary>
      /// Performs the actual send of delivery data which might be enlisted in a transaction
      /// or may simply be a passthrough based on the context and its state. The sender need
      /// not be aware of this though as the context will provide a delivery state that is
      /// appropriate for this send.
      /// </summary>
      /// <param name="state">The state to apply to the send</param>
      /// <param name="settled">If the send should be marked as settled or not</param>
      void Send(Types.Transport.IDeliveryState state, bool settled);

      /// <summary>
      /// If the context that is overseeing this send is in a failed state it can request that
      /// the send be discarded without notification to the sender that it failed, this occurs
      /// most often in an in-doubt transaction context where all work will be dropped once the
      /// user attempt to retire the transaction.
      /// </summary>
      void Discard();

   }

   /// <summary>
   /// Base for a Transaction Context used in client session instances
   /// to mask from the senders and receivers the work of deciding transaction
   /// specific behaviors.
   /// </summary>
   internal interface IClientTransactionContext
   {
      /// <summary>
      /// Begin a new transaction if one is not already in play.
      /// </summary>
      /// <param name="beginFuture">The Task that awaits the result of starting the new transaction</param>
      /// <returns>This transaction context instance</returns>
      /// <exception cref="ClientIllegalStateException">If an error occurs due to the transaction state</exception>
      IClientTransactionContext Begin(TaskCompletionSource<ISession> beginFuture);

      /// <summary>
      /// Commits the current transaction if one is active and is not failed into a roll-back only state.
      /// </summary>
      /// <param name="commitFuture">The Task that awaits the result of committing the new transaction</param>
      /// <param name="startNew">Should the context immediately initiate a new transaction</param>
      /// <returns>This transaction context instance</returns>
      /// <exception cref="ClientIllegalStateException">If an error occurs due to the transaction state</exception>
      IClientTransactionContext Commit(TaskCompletionSource<ISession> commitFuture, bool startNew);

      /// <summary>
      /// Rolls back the current transaction if one is active.
      /// </summary>
      /// <param name="rollbackFuture">The future that awaits the result of rolling back the new transaction</param>
      /// <param name="startNew">Should the context immediately initiate a new transaction</param>
      /// <returns>This transaction context instance</returns>
      /// <exception cref="ClientIllegalStateException">If an error occurs due to the transaction state</exception>
      IClientTransactionContext Rollback(TaskCompletionSource<ISession> rollbackFuture, bool startNew);

      /// <summary>
      /// Returns true if the context is hosting an active transaction
      /// </summary>
      bool IsInTransaction { get; }

      /// <summary>
      /// Returns true if there is an active transaction but its state is failed an will roll-back
      /// </summary>
      bool IsRollbackOnly { get; }

      /// <summary>
      /// Enlist the given outgoing envelope into this transaction if one is active and not already
      /// in a roll-back only state. If the transaction is failed the context should discard the
      /// envelope which should appear to the caller as if the send was successful.
      /// </summary>
      /// <param name="sendable">The sendable instance that is passing through the TXN context</param>
      /// <param name="state">The delivery state that was requested by the sender</param>
      /// <param name="settled">The settlement state that was requested by the sender</param>
      /// <returns>This transaction context instance</returns>
      IClientTransactionContext Send(ISendable sendable, Types.Transport.IDeliveryState state, bool settled);

      /// <summary>
      /// Apply a disposition to the given delivery wrapping it with a TransactionalState outcome
      /// if there is an active transaction.  If there is no active transaction than the context will apply
      /// the disposition as requested but if there is an active transaction then the disposition must be
      /// wrapped in a TransactionalState and settlement should always enforced by the client.
      /// </summary>
      /// <param name="delivery">The delivery that is having a disposition applied</param>
      /// <param name="state">The delivery state that was requested by the receiver</param>
      /// <param name="settled">The settlement state that was requested by the receiver</param>
      /// <returns>This transaction context instance</returns>
      IClientTransactionContext Disposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settled);

   }
}