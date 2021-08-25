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
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Transaction Controller link that implements the mechanics of declaring and discharging
   /// AMQP transactions. A transaction controller is typically used at the client side of an
   /// AMQP link to create transaction instances which the client application will enlist its
   /// incoming and outgoing deliveries into.
   /// </summary>
   public interface ITransactionController : IEndpoint<ITransactionController>
   {
      /// <summary>
      /// Provides access to the connection that owns this transaction controller endpoint.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// Provides access to the session that created this transaction controller.
      /// </summary>
      ISession Session { get; }

      /// <summary>
      /// Returns true if the transaction controller has capacity to send or buffer
      /// and transaction command to declare or discharge. If no capacity then a
      /// call to transaction controller declare or discharge would throw an exception.
      /// </summary>
      bool HasCapacity { get; }

      /// <summary>
      /// Access the Source value to assign to the local end of this transaction coordinator link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Source Source { get; set; }

      /// <summary>
      /// Access the Coordinator value to assign to the local end of this transaction coordinator link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Coordinator Coordinator { get; set; }

      /// <summary>
      /// Gets the Source value to assign to the remote end of this transaction coordinator link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Source RemoteSource { get; }

      /// <summary>
      /// Gets the Coordinator value to assign to the remote end of this transaction coordinator link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Coordinator RemoteCoordinator { get; }

      /// <summary>
      /// Returns a enumerator over the set of Transactions that are active within this
      /// ITransactionController which have not reached a terminal state meaning they have
      /// not been successfully discharged and have not failed in either the declare phase
      /// or the discharge phase. If there are no transactions active within this transaction
      /// controller this method will return an enumerator that provides no entries.
      /// </summary>
      IEnumerable<ITransaction<ITransactionController>> Transactions { get; }

      /// <summary>
      /// Creates a new Transaction instances that is returned in the idle state which can be
      /// populated with application specific attachments or assigned a linked resource prior to
      /// calling the declare method.
      /// </summary>
      /// <returns>A new transaction that can be correlated with later declare events.</returns>
      ITransaction<ITransactionController> NewTransaction();

      /// <summary>
      /// Request that the remote transaction manager declare a new transaction and respond
      /// with a new transaction Id for that transaction. Upon successful declaration of a new
      /// transaction the remote will respond and the declared event handler will be signaled.
      /// <para/>
      /// This is a convenience method that is the same as first calling new transaction method
      /// and then passing the result of that to the declare method that accepts an already
      /// created transaction instance.
      /// </summary>
      /// <returns>A new transaction that can be correlated with later declare events.</returns>
      ITransaction<ITransactionController> Declare();

      /// <summary>
      /// Request that the remote transaction manager discharge the given transaction with the
      /// specified failure state (true for failed). Upon successful discharge of the given
      /// transaction the remote will respond and the discharge event handler will be signalled.
      /// </summary>
      /// <param name="transaction">The transaction instance to associate with a declare</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController Declare(ITransaction<ITransactionController> transaction);

      /// <summary>
      /// Request that the remote transaction manager declare a new transaction and respond
      /// with a new transaction Id for that transaction. Upon successful declaration of a new
      /// transaction the remote will respond and the declared event handler will be signaled.
      /// </summary>
      /// <param name="transaction">The transaction to be dischareged</param>
      /// <param name="failed">boolean that indicates if the transaction has failed.</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController Discharge(ITransaction<ITransactionController> transaction, bool failed);

      #region Transaction Controller event points

      /// <summary>
      /// Called when the transaction manager end of the link has responded to a previous
      /// declare request and the transaction can now be used to enroll deliveries into the
      /// active transaction.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController DeclaredHandler(Action<ITransaction<ITransactionController>> handler);

      /// <summary>
      /// Called when the transaction manager end of the link has responded to a previous
      /// declare request indicating that the declaration failed and the transaction cannot
      /// be used to perform transactional work.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController DeclareFailedHandler(Action<ITransaction<ITransactionController>> handler);

      /// <summary>
      /// Called when the transaction manager end of the link has responded to a previous
      /// discharge request and the transaction and the transaction has been completed.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController DischargeHandler(Action<ITransaction<ITransactionController>> handler);

      /// <summary>
      /// Called when the transaction manager end of the link has responded to a previous
      /// discharge request and the transaction discharge failed for some reason.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController DischargeFailedHandler(Action<ITransaction<ITransactionController>> handler);

      /// <summary>
      /// Allows the caller to add an delegate that will be signaled when the underlying link for
      /// this transaction controller has been granted credit which would then allow for transaction
      /// declare or discharge commands to be sent to the remote Transactional Resource.
      /// <para/>
      /// If the controller already has credit to send then the handler will be invoked immediately
      /// otherwise it will be stored until credit becomes available. Once a handler is signaled it
      /// is no longer retained for future updates and the caller will need to register it again once
      /// more transactional work is to be completed. Because more than one handler can be added at a
      /// time the caller should check again before attempting to perform a transaction declare or a
      /// discharge as other tasks might have already consumed credit if work is done via some
      /// asynchronous mechanism.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController AddCapacityAvailableHandler(Action<ITransactionController> handler);

      /// <summary>
      /// Sets a Action delegate for when the parent Session or Connection of this link
      /// is locally closed.
      /// <para/>
      /// Typically used by clients for logging or other state update event processing.
      /// Clients should not perform any blocking calls within this context.  It is an error
      /// for the handler to throw an exception and the outcome of doing so is undefined.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This transaction controller instance</returns>
      ITransactionController ParentEndpointClosedHandler(Action<ITransactionController> handler);

      #endregion

   }
}