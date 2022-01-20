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
using System.Collections.ObjectModel;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Proton transaction controller abstraction that provides the transaction services
   /// for a Sender link that transmits the transaction Declare and Discharge commads
   /// which control the lifetime and outcome of a running transaction.
   /// </summary>
   public sealed class ProtonTransactionController : ProtonEndpoint<ITransactionController>, ITransactionController
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ProtonTransactionController>();

      private static readonly IProtonBuffer ENCODED_DECLARE;

      static ProtonTransactionController()
      {
         IEncoder declareEncoder = CodecFactory.Encoder;
         IEncoderState state = CodecFactory.Encoder.NewEncoderState();

         ENCODED_DECLARE = ProtonByteBufferAllocator.Instance.Allocate();

         try
         {
            declareEncoder.WriteObject(ENCODED_DECLARE, state, new AmqpValue(new Declare()));
         }
         finally
         {
            state.Reset();
         }
      }

      private readonly ProtonSender sender;
      private readonly IEncoder commandEncoder = CodecFactory.Encoder;
      private readonly IProtonBuffer encoding = ProtonByteBufferAllocator.Instance.Allocate(128, 128);

      private readonly ISet<ITransaction<ITransactionController>> transactions =
         new HashSet<ITransaction<ITransactionController>>();

      private Action<ITransaction<ITransactionController>> declaredEventHandler;
      private Action<ITransaction<ITransactionController>> declareFailureEventHandler;
      private Action<ITransaction<ITransactionController>> dischargedEventHandler;
      private Action<ITransaction<ITransactionController>> dischargeFailureEventHandler;

      private Action<ITransactionController> parentEndpointClosedEventHandler;

      private List<Action<ITransactionController>> capacityObservers = new List<Action<ITransactionController>>();

      public ProtonTransactionController(ProtonSender sender) : base(sender.ProtonEngine)
      {
         this.sender = sender;
         this.sender.DeliveryTagGenerator = ProtonDeliveryTagTypes.Pooled.NewTagGenerator();
         this.sender.DeliveryStateUpdatedHandler(HandleDeliveryRemotelyUpdated)
                    .CreditStateUpdateHandler(HandleLinkCreditUpdated)
                    .OpenHandler(HandleSenderLinkOpened)
                    .CloseHandler(HandleSenderLinkClosed)
                    .ParentEndpointClosedHandler(HandleParentEndpointClosed)
                    .LocalOpenHandler(HandleSenderLinkLocallyOpened)
                    .LocalCloseHandler(HandleSenderLinkLocallyClosed)
                    .EngineShutdownHandler(HandleEngineShutdown);
      }

      #region Transaction Controller public API

      public IConnection Connection => sender.Connection;

      public ISession Session => sender.Session;

      public bool HasCapacity => sender.IsSendable;

      public override bool IsLocallyOpen => sender.IsLocallyOpen;

      public override bool IsLocallyClosed => sender.IsLocallyClosed;

      public override bool IsRemotelyOpen => sender.IsRemotelyOpen;

      public override bool IsRemotelyClosed => sender.IsRemotelyClosedOrDetached;

      public override Symbol[] OfferedCapabilities
      {
         get => sender.OfferedCapabilities;
         set => sender.OfferedCapabilities = value;
      }

      public override Symbol[] RemoteOfferedCapabilities => sender.RemoteOfferedCapabilities;

      public override Symbol[] DesiredCapabilities
      {
         get => sender.DesiredCapabilities;
         set => sender.DesiredCapabilities = value;
      }

      public override Symbol[] RemoteDesiredCapabilities => sender.RemoteDesiredCapabilities;

      public override IReadOnlyDictionary<Symbol, object> Properties
      {
         get => sender.Properties;
         set => sender.Properties = value;
      }

      public override IReadOnlyDictionary<Symbol, object> RemoteProperties => sender.RemoteProperties;

      public override ErrorCondition RemoteErrorCondition => sender.RemoteErrorCondition;

      public Source Source
      {
         get => sender.Source;
         set => sender.Source = value;
      }

      public Source RemoteSource
      {
         get => sender.RemoteSource;
      }

      public Coordinator Coordinator
      {
         get => sender.Coordinator;
         set => sender.Coordinator = value;
      }

      public Coordinator RemoteCoordinator
      {
         get => (Coordinator)sender.RemoteTerminus;
      }

      public IEnumerable<ITransaction<ITransactionController>> Transactions =>
         new ReadOnlyCollection<ITransaction<ITransactionController>>(
            new List<ITransaction<ITransactionController>>(transactions));

      public override ITransactionController Open()
      {
         sender.Open();
         return this;
      }

      public override ITransactionController Close()
      {
         sender.Close();
         return this;
      }

      public ITransaction<ITransactionController> NewTransaction()
      {
         ProtonControllerTransaction txn = new ProtonControllerTransaction(this);
         transactions.Add(txn);

         return txn;
      }

      public ITransaction<ITransactionController> Declare()
      {
         if (!sender.IsSendable)
         {
            throw new InvalidOperationException("Cannot Declare due to current capacity restrictions.");
         }

         ProtonControllerTransaction transaction = (ProtonControllerTransaction)NewTransaction();

         Declare(transaction);

         return transaction;
      }

      public ITransactionController Declare(ITransaction<ITransactionController> transaction)
      {
         if (!sender.IsSendable)
         {
            throw new InvalidOperationException("Cannot Declare due to current capacity restrictions.");
         }

         if (transaction.State != TransactionState.Idle)
         {
            throw new InvalidOperationException("Cannot declare a transaction that has already been used previously");
         }

         if (transaction.Parent != this)
         {
            throw new ArgumentException("Cannot declare a transaction that was created by another controller.");
         }

         ProtonControllerTransaction protonTransaction = (ProtonControllerTransaction)transaction;

         protonTransaction.State = TransactionState.Declaring;

         IOutgoingDelivery command = sender.Next();

         command.LinkedResource = protonTransaction;

         try
         {
            command.WriteBytes(ENCODED_DECLARE);
         }
         finally
         {
            ENCODED_DECLARE.ReadOffset = 0;
         }

         return this;
      }

      public ITransactionController Discharge(ITransaction<ITransactionController> transaction, bool failed)
      {
         if (transaction.State != TransactionState.Declared)
         {
            throw new InvalidOperationException("Cannot discharge a transaction that is not currently actively declared.");
         }

         if (transaction.Parent != this)
         {
            throw new ArgumentException("Cannot discharge a transaction that was created by another controller.");
         }

         if (!sender.IsSendable)
         {
            throw new InvalidOperationException("Cannot discharge transaction due to current capacity restrictions.");
         }

         ProtonTransaction<ITransactionController> protonTxn = (ProtonTransaction<ITransactionController>)transaction;

         protonTxn.State = TransactionState.Discharging;
         protonTxn.DischargeState = failed ? DischargeState.Rollback : DischargeState.Commit;

         Discharge discharge = new Discharge();
         discharge.Fail = failed;
         discharge.TxnId = transaction.TxnId;

         commandEncoder.WriteObject(encoding.Reset(), commandEncoder.CachedEncoderState, new AmqpValue(discharge));

         IOutgoingDelivery command = sender.Next();
         command.MessageFormat = 0;
         command.LinkedResource = transaction;
         command.WriteBytes(encoding);

         return this;
      }

      #endregion

      #region Transaction Controller Event Handler API

      public ITransactionController AddCapacityAvailableHandler(Action<ITransactionController> handler)
      {
         if (HasCapacity)
         {
            handler.Invoke(this);
         }
         else
         {
            capacityObservers.Add(handler);
         }

         return this;
      }

      public ITransactionController DeclaredHandler(Action<ITransaction<ITransactionController>> handler)
      {
         declaredEventHandler = handler;
         return this;
      }

      public ITransactionController DeclareFailedHandler(Action<ITransaction<ITransactionController>> handler)
      {
         declareFailureEventHandler = handler;
         return this;
      }

      public ITransactionController DischargedHandler(Action<ITransaction<ITransactionController>> handler)
      {
         dischargedEventHandler = handler;
         return this;
      }

      public ITransactionController DischargeFailedHandler(Action<ITransaction<ITransactionController>> handler)
      {
         dischargeFailureEventHandler = handler;
         return this;
      }

      public ITransactionController ParentEndpointClosedHandler(Action<ITransactionController> handler)
      {
         parentEndpointClosedEventHandler = handler;
         return this;
      }

      private void FireDeclaredEvent(ProtonControllerTransaction transaction)
      {
         if (declaredEventHandler != null)
         {
            declaredEventHandler.Invoke(transaction);
         }
         else
         {
            LOG.Debug("Transaction {0} declared successfully but no handler registered to signal result", transaction);
         }
      }

      private void FireDeclareFailureEvent(ProtonControllerTransaction transaction)
      {
         if (declareFailureEventHandler != null)
         {
            declareFailureEventHandler.Invoke(transaction);
         }
         else
         {
            LOG.Debug("Transaction {0} declare failed but no handler registered to signal result", transaction);
         }
      }

      private void FireDischargedEvent(ProtonControllerTransaction transaction)
      {
         if (dischargedEventHandler != null)
         {
            dischargedEventHandler.Invoke(transaction);
         }
         else
         {
            LOG.Debug("Transaction {0} discharged successfully but no handler registered to signal result", transaction);
         }
      }

      private void FireDischargeFailureEvent(ProtonControllerTransaction transaction)
      {
         if (dischargeFailureEventHandler != null)
         {
            dischargeFailureEventHandler.Invoke(transaction);
         }
         else
         {
            LOG.Debug("Transaction {0} discharge failed but no handler registered to signal result", transaction);
         }
      }

      #endregion

      #region Internal transaction controller API

      internal override ITransactionController Self() => this;

      #endregion

      #region Sender link event handlers

      private void HandleSenderLinkLocallyOpened(ISender sender)
      {
         FireLocalOpen();
      }

      private void HandleSenderLinkLocallyClosed(ISender sender)
      {
         FireLocalClose();
      }

      private void HandleSenderLinkOpened(ISender sender)
      {
         FireRemoteOpen();
      }

      private void HandleSenderLinkClosed(ISender sender)
      {
         FireRemoteClose();
      }

      private void HandleParentEndpointClosed(ISender sender)
      {
         parentEndpointClosedEventHandler?.Invoke(this);
      }

      private void HandleEngineShutdown(IEngine engine)
      {
         FireEngineShutdown();
      }

      private void HandleLinkCreditUpdated(ISender sender)
      {
         if (sender.IsSendable && capacityObservers.Count > 0)
         {
            List<Action<ITransactionController>> copyOf = new List<Action<ITransactionController>>(capacityObservers);
            foreach (Action<ITransactionController> handler in copyOf)
            {
               if (HasCapacity)
               {
                  try
                  {
                     handler.Invoke(this);
                  }
                  finally
                  {
                     capacityObservers.Remove(handler);
                  }
               }
               else
               {
                  break;
               }
            }
         }

         if (sender.IsDraining)
         {
            sender.Drained();
         }
      }

      private void HandleDeliveryRemotelyUpdated(IOutgoingDelivery delivery)
      {
         ProtonControllerTransaction transaction = (ProtonControllerTransaction)delivery.LinkedResource;

         IDeliveryState state = delivery.RemoteState;
         TransactionState transactionState = transaction.State;

         try
         {
            switch (state.Type)
            {
               case DeliveryStateType.Declared:
                  Declared declared = (Declared)state;
                  transaction.State = TransactionState.Declared;
                  transaction.TxnId = declared.TxnId;
                  FireDeclaredEvent(transaction);
                  break;
               case DeliveryStateType.Accepted:
                  transaction.State = TransactionState.Discharged;
                  transactions.Remove(transaction);
                  FireDischargedEvent(transaction);
                  break;
               default:
                  if (state.Type == DeliveryStateType.Rejected)
                  {
                     Rejected rejected = (Rejected)state;
                     transaction.Error = rejected.Error;
                  }

                  transactions.Remove(transaction);

                  if (transactionState == TransactionState.Declaring)
                  {
                     transaction.State = TransactionState.DelcareFailed;
                     FireDeclareFailureEvent(transaction);
                  }
                  else
                  {
                     transaction.State = TransactionState.DischargeFailed;
                     FireDischargeFailureEvent(transaction);
                  }

                  break;
            }
         }
         finally
         {
            delivery.Settle();
         }
      }

      #endregion

      #region Custom Transaction controller transaction implementation

      private sealed class ProtonControllerTransaction : ProtonTransaction<ITransactionController>, ITransaction<ITransactionController>
      {
         private readonly ProtonTransactionController controller;

         public ProtonControllerTransaction(ProtonTransactionController controller)
         {
            this.controller = controller;
         }

         public override ProtonTransactionController Parent => controller;

      }

      #endregion
   }
}