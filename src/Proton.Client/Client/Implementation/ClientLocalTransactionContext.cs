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
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.Concurrent;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// A local transaction based context for AMQP transactional sessions.
   /// </summary>
   internal sealed class ClientLocalTransactionContext : IClientTransactionContext
   {
      private static readonly Symbol[] SUPPORTED_OUTCOMES = new Symbol[] { Accepted.DescriptorSymbol,
                                                                           Rejected.DescriptorSymbol,
                                                                           Released.DescriptorSymbol,
                                                                           Modified.DescriptorSymbol };

      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientLocalTransactionContext>();

      private readonly string DECLARE_FUTURE_NAME = "Declare:Future";
      private readonly string DISCHARGE_FUTURE_NAME = "Discharge:Future";
      private readonly string START_TRANSACTION_MARKER = "Transaction:Start";

      private readonly AtomicInteger coordinatorCounter = new();
      private readonly ClientSession session;

      private ITransaction<ITransactionController> currentTxn;
      private ITransactionController txnController;

      private TransactionalState cachedSenderOutcome;
      private TransactionalState cachedReceiverOutcome;

      public ClientLocalTransactionContext(ClientSession session)
      {
         this.session = session;
      }

      public bool IsInTransaction => currentTxn?.State == TransactionState.Declared;

      public bool IsRollbackOnly => IsInTransaction && txnController.IsLocallyClosed;

      public IClientTransactionContext Begin(TaskCompletionSource<ISession> beginFuture)
      {
         CheckCanBeginNewTransaction();
         BeginNewTransaction(beginFuture);
         return this;
      }

      public IClientTransactionContext Commit(TaskCompletionSource<ISession> commitFuture, bool startNew)
      {
         CheckCanCommitTransaction();

         if (txnController.IsLocallyOpen)
         {
            currentTxn.Attachments.Set(DISCHARGE_FUTURE_NAME, commitFuture);
            currentTxn.Attachments.Set(START_TRANSACTION_MARKER, startNew);

            if (session.Options.RequestTimeout > 0)
            {
               session.ScheduleRequestTimeout(commitFuture, session.Options.RequestTimeout, () =>
               {
                  try
                  {
                     LOG.Trace("Commit of transaction timed out and the controller will be closed");
                     txnController.ErrorCondition = new Types.Transport.ErrorCondition("Timed out waiting for transaction to be committed");
                     txnController.Close();
                  }
                  catch (Exception)
                  {
                  }

                  return new ClientTransactionRolledBackException("Timed out waiting for Transaction commit to complete");
               });
            }

            txnController.AddCapacityAvailableHandler(controller =>
            {
               try
               {
                  txnController.Discharge(currentTxn, false);
               }
               catch (EngineFailedException efe)
               {
                  commitFuture.TrySetException(ClientExceptionSupport.CreateOrPassthroughFatal(efe));
               }
            });
         }
         else
         {
            currentTxn = null;
            // The coordinator link closed which amount to a roll back of the declared
            // transaction so we just complete the request as a failure.
            commitFuture.TrySetException(CreateRolledBackErrorFromClosedCoordinator());
         }

         return this;
      }

      public IClientTransactionContext Rollback(TaskCompletionSource<ISession> rollbackFuture, bool startNew)
      {
         CheckCanRollbackTransaction();

         if (txnController.IsLocallyOpen)
         {
            currentTxn.Attachments.Set(DISCHARGE_FUTURE_NAME, rollbackFuture);
            currentTxn.Attachments.Set(START_TRANSACTION_MARKER, startNew);

            if (session.Options.RequestTimeout > 0)
            {
               session.ScheduleRequestTimeout(rollbackFuture, session.Options.RequestTimeout, () =>
               {
                  try
                  {
                     LOG.Trace("Rollback of transaction timed out and the controller will be closed");
                     txnController.ErrorCondition = new Types.Transport.ErrorCondition("Timed out waiting for transaction to be rolled back");
                     txnController.Close();
                  }
                  catch (Exception)
                  {
                  }

                  return new ClientOperationTimedOutException("Timed out waiting for Transaction rollback to complete");
               });
            }

            txnController.AddCapacityAvailableHandler(controller =>
            {
               try
               {
                  txnController.Discharge(currentTxn, true);
               }
               catch (EngineFailedException)
               {
                  // The engine has failed and the connection will be closed so the transaction
                  // is implicitly rolled back on the remote.
                  rollbackFuture.TrySetResult(session);
               }
               catch (Exception error)
               {
                  // Some internal error has occurred and should be communicated as this is not
                  // expected under normal circumstances.
                  rollbackFuture.TrySetException(ClientExceptionSupport.CreateOrPassthroughFatal(error));
               }
            });
         }
         else
         {
            currentTxn = null;
            // Coordinator was closed after transaction was declared which amounts
            // to a roll back of the transaction so we let this complete as normal.
            rollbackFuture.TrySetResult(session);
         }

         return this;
      }

      public IClientTransactionContext Send(ISendable sendable, Types.Transport.IDeliveryState state, bool settled)
      {
         if (IsInTransaction)
         {
            if (IsRollbackOnly)
            {
               sendable.Discard();
            }
            else if (state == null)
            {
               sendable.Send(cachedSenderOutcome ??= new TransactionalState(currentTxn.TxnId), settled);
            }
            else
            {
               sendable.Send(new TransactionalState(currentTxn.TxnId, (IOutcome)state), settled);
            }
         }
         else
         {
            sendable.Send(state, settled);
         }

         return this;
      }

      public IClientTransactionContext Disposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settled)
      {
         if (IsInTransaction)
         {
            Types.Transport.IDeliveryState txnOutcome;

            if (state is Accepted)
            {
               txnOutcome = (cachedReceiverOutcome ??= new TransactionalState(currentTxn.TxnId, Accepted.Instance));
            }
            else
            {
               txnOutcome = new TransactionalState(currentTxn.TxnId, (IOutcome)state);
            }

            delivery.Disposition(txnOutcome, true);
         }
         else
         {
            delivery.Disposition(state, settled);
         }

         return this;
      }

      #region Private Local transaction context API

      private void BeginNewTransaction(TaskCompletionSource<ISession> beginFuture)
      {
         ITransactionController txnController = GetOrCreateNewTxnController();

         currentTxn = txnController.NewTransaction();
         currentTxn.LinkedResource = this;
         currentTxn.Attachments.Set(DECLARE_FUTURE_NAME, beginFuture);

         cachedReceiverOutcome = null;
         cachedSenderOutcome = null;

         if (session.Options.RequestTimeout > 0)
         {
            session.ScheduleRequestTimeout(beginFuture, session.Options.RequestTimeout, () =>
            {
               try
               {
                  LOG.Trace("Begin of new transaction timed out and the controller will be closed");
                  txnController.ErrorCondition = new Types.Transport.ErrorCondition("Timed out waiting for transaction to be declared");
                  txnController.Close();
               }
               catch (Exception)
               {
               }

               return new ClientTransactionDeclarationException("Timed out waiting for Transaction declaration to complete");
            });
         }

         txnController.AddCapacityAvailableHandler(controller =>
         {
            try
            {
               txnController.Declare(currentTxn);
            }
            catch (EngineFailedException efe)
            {
               beginFuture.TrySetException(ClientExceptionSupport.CreateOrPassthroughFatal(efe));
            }
         });
      }

      private ITransactionController GetOrCreateNewTxnController()
      {
         if (txnController == null || txnController.IsLocallyClosed)
         {
            Coordinator coordinator = new()
            {
               Capabilities = new Symbol[] { TxnCapability.LOCAL_TXN }
            };

            Source source = new()
            {
               Outcomes = (Symbol[])SUPPORTED_OUTCOMES.Clone()
            };

            ITransactionController controller = session.ProtonSession.Coordinator(NextCoordinatorId());
            controller.Source = source;
            controller.Coordinator = coordinator;
            controller.DeclaredHandler(HandleTransactionDeclared)
                      .DeclareFailedHandler(HandleTransactionDeclareFailed)
                      .DischargedHandler(HandleTransactionDischarged)
                      .DischargeFailedHandler(HandleTransactionDischargeFailed)
                      .OpenHandler(HandleCoordinatorOpen)
                      .CloseHandler(HandleCoordinatorClose)
                      .LocalCloseHandler(HandleCoordinatorLocalClose)
                      .ParentEndpointClosedHandler(HandleParentEndpointClosed)
                      .EngineShutdownHandler(HandleEngineShutdown)
                      .Open();

            this.txnController = controller;
         }

         return txnController;
      }

      private string NextCoordinatorId()
      {
         return session.SessionId + ":" + coordinatorCounter.IncrementAndGet();
      }

      private ClientTransactionRolledBackException CreateRolledBackErrorFromClosedCoordinator()
      {
         ClientException cause = ClientExceptionSupport.ConvertToNonFatalException(txnController.RemoteErrorCondition);

         if (cause is not ClientTransactionRolledBackException)
         {
            cause = new ClientTransactionRolledBackException(cause.Message, cause);
         }

         return (ClientTransactionRolledBackException)cause;
      }

      private ClientTransactionDeclarationException CreateDeclarationErrorFromClosedCoordinator()
      {
         ClientException cause = ClientExceptionSupport.ConvertToNonFatalException(txnController.RemoteErrorCondition);

         if (cause is not ClientTransactionDeclarationException)
         {
            cause = new ClientTransactionDeclarationException(cause.Message, cause);
         }

         return (ClientTransactionDeclarationException)cause;
      }

      private void CheckCanBeginNewTransaction()
      {
         if (currentTxn != null)
         {
            switch (currentTxn.State)
            {
               case TransactionState.Discharged:
               case TransactionState.DischargeFailed:
               case TransactionState.DeclareFailed:
                  break;
               case TransactionState.Declaring:
                  throw new ClientIllegalStateException("A transaction is already in the process of being started");
               case TransactionState.Declared:
                  throw new ClientIllegalStateException("A transaction is already active in this Session");
               case TransactionState.Discharging:
                  throw new ClientIllegalStateException("A transaction is still being retired and a new one cannot yet be started");
               default:
                  throw new ClientIllegalStateException("Cannot begin a new transaction until the existing transaction completes");
            }
         }
      }

      private void CheckCanCommitTransaction()
      {
         if (currentTxn == null)
         {
            throw new ClientTransactionNotActiveException("Commit called with no active transaction");
         }
         else
         {
            switch (currentTxn.State)
            {
               case TransactionState.Discharged:
                  throw new ClientTransactionNotActiveException("Commit called with no active transaction");
               case TransactionState.Declaring:
                  throw new ClientIllegalStateException("Commit called before transaction declare completed.");
               case TransactionState.Discharging:
                  throw new ClientIllegalStateException("Commit called before transaction discharge completed.");
               case TransactionState.DeclareFailed:
                  throw new ClientTransactionNotActiveException("Commit called on a transaction that has failed due to an error during declare.");
               case TransactionState.DischargeFailed:
                  throw new ClientTransactionNotActiveException("Commit called on a transaction that has failed due to an error during discharge.");
               case TransactionState.Idle:
                  throw new ClientTransactionNotActiveException("Commit called on a transaction that has not yet been declared");
               default:
                  break;
            }
         }
      }

      private void CheckCanRollbackTransaction()
      {
         if (currentTxn == null)
         {
            throw new ClientTransactionNotActiveException("Rollback called with no active transaction");
         }
         else
         {
            switch (currentTxn.State)
            {
               case TransactionState.Discharged:
                  throw new ClientTransactionNotActiveException("Rollback called with no active transaction");
               case TransactionState.Declaring:
                  throw new ClientIllegalStateException("Rollback called before transaction declare completed.");
               case TransactionState.Discharging:
                  throw new ClientIllegalStateException("Rollback called before transaction discharge completed.");
               case TransactionState.DeclareFailed:
                  throw new ClientTransactionNotActiveException("Rollback called on a transaction that has failed due to an error during declare.");
               case TransactionState.DischargeFailed:
                  throw new ClientTransactionNotActiveException("Rollback called on a transaction that has failed due to an error during discharge.");
               case TransactionState.Idle:
                  throw new ClientTransactionNotActiveException("Rollback called on a transaction that has not yet been declared");
               default:
                  break;
            }
         }
      }

      #endregion

      #region Transaction controller event handlers

      private void HandleTransactionDeclared(ITransaction<ITransactionController> transaction)
      {
         TaskCompletionSource<ISession> future =
            transaction.Attachments.Get<TaskCompletionSource<ISession>>(DECLARE_FUTURE_NAME, null);
         LOG.Trace("Declare of transaction:{0} completed", transaction);

         if (future.Task.IsCompletedSuccessfully || future.Task.IsCanceled)
         {
            // The original declare operation cancelled the future likely due to timeout
            // which means this transaction will never be completed at a higher level so we
            // must discharge it now to ensure the remote can clean up associated resources.
            try
            {
               Rollback(new TaskCompletionSource<ISession>(), false);
            }
            catch (Exception) { }
         }
         else
         {
            future?.TrySetResult(session);
         }
      }

      private void HandleTransactionDeclareFailed(ITransaction<ITransactionController> transaction)
      {
         TaskCompletionSource<ISession> future =
            transaction.Attachments.Get<TaskCompletionSource<ISession>>(DECLARE_FUTURE_NAME, null);
         LOG.Trace("Declare of transaction:{0} failed", transaction);
         ClientException cause = ClientExceptionSupport.ConvertToNonFatalException(transaction.Error);
         future?.TrySetException(new ClientTransactionDeclarationException(cause.Message, cause));
      }

      private void HandleTransactionDischarged(ITransaction<ITransactionController> transaction)
      {
         TaskCompletionSource<ISession> future =
            transaction.Attachments.Get<TaskCompletionSource<ISession>>(DISCHARGE_FUTURE_NAME, null);
         LOG.Trace("Discharge of transaction:{0} completed", transaction);
         future?.TrySetResult(session);

         if (transaction.Attachments.Get(START_TRANSACTION_MARKER, false))
         {
            BeginNewTransaction(future);
         }
      }

      private void HandleTransactionDischargeFailed(ITransaction<ITransactionController> transaction)
      {
         TaskCompletionSource<ISession> future =
            transaction.Attachments.Get<TaskCompletionSource<ISession>>(DISCHARGE_FUTURE_NAME, null);
         LOG.Trace("Discharge of transaction:{0} failed", transaction);
         ClientException cause = ClientExceptionSupport.ConvertToNonFatalException(transaction.Error);
         future?.TrySetException(new ClientTransactionRolledBackException(cause.Message, cause));
      }

      private void HandleCoordinatorOpen(ITransactionController controller)
      {
         // If remote doesn't set a remote Coordinator then a close is incoming.
         if (controller.RemoteCoordinator != null)
         {
            this.txnController = controller;
         }
      }

      private void HandleCoordinatorClose(ITransactionController controller)
      {
         txnController?.Close();
      }

      private void HandleCoordinatorLocalClose(ITransactionController controller)
      {
         /// Ensure that the controller is disconnected from this context's event points
         /// so that if any events happen after this they don't affect any newly created
         /// resources in this context.
         controller.DeclaredHandler(null)
                   .DeclareFailedHandler(null)
                   .DischargedHandler(null)
                   .DischargeFailedHandler(null)
                   .OpenHandler(null)
                   .CloseHandler(null)
                   .LocalCloseHandler(null)
                   .ParentEndpointClosedHandler(null)
                   .EngineShutdownHandler(null);

         if (currentTxn != null)
         {
            TaskCompletionSource<ISession> future;

            switch (currentTxn.State)
            {
               case TransactionState.Idle:
               case TransactionState.Declaring:
                  future = currentTxn.Attachments.Get<TaskCompletionSource<ISession>>(DECLARE_FUTURE_NAME, null);
                  future?.TrySetException(CreateDeclarationErrorFromClosedCoordinator());
                  currentTxn = null;
                  break;
               case TransactionState.Discharging:
                  future = currentTxn.Attachments.Get<TaskCompletionSource<ISession>>(DISCHARGE_FUTURE_NAME, null);
                  if (currentTxn.DischargeState == DischargeState.Commit)
                  {
                     future?.TrySetException(CreateRolledBackErrorFromClosedCoordinator());
                  }
                  else
                  {
                     future?.TrySetResult(session);
                  }
                  currentTxn = null;
                  break;
               default:
                  break;
            }
         }
      }

      private void HandleParentEndpointClosed(ITransactionController txnController)
      {
         txnController?.Close();
      }

      private void HandleEngineShutdown(IEngine engine)
      {
         txnController?.Close();
      }

      #endregion
   }
}