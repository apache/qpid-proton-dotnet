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
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   ///  Transaction Manager endpoint that implements the mechanics of handling the declaration
   /// of and the requested discharge of AMQP transactions.  Typically an AMQP server instance
   /// will host the transaction management services that are used by client resources to declare
   /// and discharge transaction and handle the associated of deliveries that are enlisted in
   /// active transactions.
   /// </summary>
   public interface ITransactionManager : IEndpoint<ITransactionManager>
   {
      /// <summary>
      /// Adds the given amount of credit for the transaction manager which allows
      /// the remote transaction controller to send declare and discharge requests to
      /// this manager. The remote transaction controller cannot send any requests
      /// to start or complete a transaction without having credit to do so which
      /// implies that the transaction manager owner must grant credit as part of
      /// its normal processing.
      /// </summary>
      /// <param name="credit">The credit to add to the current link credit</param>
      /// <returns>This transaction manager instance.</returns>
      ITransactionManager AddCredit(uint credit);

      /// <summary>
      /// Retrieves the current amount of link credit that is assigned to the remote
      /// for sending transaction work requests.
      /// </summary>
      uint Credit { get; }

      /// <summary>
      /// Access the Source value to assign to the local end of this transaction manager link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Source Source { get; set; }

      /// <summary>
      /// Access the Coordinator value to assign to the local end of this transaction manager link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Coordinator Coordinator { get; set; }

      /// <summary>
      /// Gets the Source value to assign to the remote end of this transaction manager link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Source RemoteSource { get; set; }

      /// <summary>
      /// Gets the Coordinator value to assign to the remote end of this transaction manager link.
      /// <para/>
      /// Must be called during link setup, i.e. before calling the open method.
      /// </summary>
      Coordinator RemoteCoordinator { get; set; }

      /// <summary>
      /// Respond to a previous declare request from the remote transaction controller
      /// indicating that the requested transaction has been successfully declared and
      /// that deliveries can now be enlisted in that transaction.
      /// </summary>
      /// <param name="transaction">The transaction instance that is being declared</param>
      /// <param name="txnId">The new transaction id to assign to the transaction</param>
      /// <returns></returns>
      ITransactionManager Declared(ITransaction<ITransactionManager> transaction, byte[] txnId);

      /// <summary>
      /// Respond to a previous declare request from the remote transaction controller
      /// indicating that the requested transaction has been successfully declared and
      /// that deliveries can now be enlisted in that transaction.
      /// </summary>
      /// <param name="transaction">The transaction instance that is being declared</param>
      /// <param name="txnId">The new transaction id to assign to the transaction</param>
      /// <returns>This transaction manager instance</returns>
      ITransactionManager Declared(ITransaction<ITransactionManager> transaction, IProtonBuffer txnId);

      /// <summary>
      /// Respond to a previous declare request from the remote transaction controller
      /// indicating that the requested transaction declaration has failed and is not active.
      /// </summary>
      /// <param name="transaction">The transaction that failed to be declared</param>
      /// <param name="condition">The error condition that indicates the cause of the failure</param>
      /// <returns>This transaction manager instance</returns>
      ITransactionManager DeclareFailed(ITransaction<ITransactionManager> transaction, ErrorCondition condition);

      /// <summary>
      /// Respond to a previous discharge request from the remote transaction controller
      /// indicating that the discharge completed on the transaction identified by given
      /// transaction Id has now been retired.
      /// </summary>
      /// <param name="transaction">The transaction that was dischareged</param>
      /// <returns>This transaction manager instance</returns>
      ITransactionManager Discharged(ITransaction<ITransactionManager> transaction);

      /// <summary>
      /// Respond to a previous discharge request from the remote transaction controller
      /// indicating that the discharge resulted in an error and the transaction must be
      /// considered rolled back.
      /// </summary>
      /// <param name="transaction">The transaction that failed to be discharged</param>
      /// <param name="condition">The error condition that indicates the cause of the failure</param>
      /// <returns>This transaction manager instance</returns>
      ITransactionManager DischargeFailed(ITransaction<ITransactionManager> transaction, ErrorCondition condition);

      /// <summary>
      /// Called when the remote transaction controller end of the link has requested a new transaction
      /// be declared using the information provided in the given declare instance.
      /// </summary>
      /// <param name="handler">The delegate that will process the event</param>
      /// <returns>This transaction manager instance</returns>
      ITransactionManager DeclareHandler(Action<ITransaction<ITransactionManager>> handler);

      /// <summary>
      /// Called when the remote transaction controller end of the link has requested a current
      /// transaction be discharged using the information provided in the given discharge instance.
      /// </summary>
      /// <param name="handler">The delegate that will process the event</param>
      /// <returns>This transaction manager instance</returns>
      ITransactionManager DischargeHandler(Action<ITransaction<ITransactionManager>> handler);

      /// <summary>
      /// Sets a Action delegate for when the parent Session or Connection of this link
      /// is locally closed.
      /// <para/>
      /// Typically used by clients for logging or other state update event processing.
      /// Clients should not perform any blocking calls within this context.  It is an error
      /// for the handler to throw an exception and the outcome of doing so is undefined.
      /// </summary>
      /// <param name="handler">The delegate that will handle this event</param>
      /// <returns>This transaction manager instance</returns>
      ITransactionController ParentEndpointClosedHandler(Action<ITransactionController> handler);

   }
}