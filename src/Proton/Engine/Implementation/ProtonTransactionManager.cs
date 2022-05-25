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
using Apache.Qpid.Proton.Codec;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Proton transaction manager abstraction that provides the transaction services
   /// for a Receiver link that handles the transaction Declare and Discharge commands
   /// which control the lifetime and outcome of a running transaction.
   /// </summary>
   public sealed class ProtonTransactionManager : ProtonEndpoint<ITransactionManager>, ITransactionManager
   {
      private readonly ProtonReceiver receiver;
      private readonly IDecoder payloadDecoder;

      private Action<ITransaction<ITransactionManager>> declareEventHandler;
      private Action<ITransaction<ITransactionManager>> dischargeEventHandler;

      private Action<ITransactionManager> parentEndpointClosedEventHandler;

      private readonly IDictionary<IProtonBuffer, ProtonManagerTransaction> transactions =
         new Dictionary<IProtonBuffer, ProtonManagerTransaction>();

      public ProtonTransactionManager(ProtonReceiver receiver) : base(receiver.ProtonEngine)
      {
         this.receiver = receiver;
         this.payloadDecoder = CodecFactory.Decoder;

         this.receiver.OpenHandler(HandleReceiverLinkOpened)
                      .CloseHandler(HandleReceiverLinkClosed)
                      .LocalOpenHandler(HandleReceiverLinkLocallyOpened)
                      .LocalCloseHandler(HandleReceiverLinkLocallyClosed)
                      .ParentEndpointClosedHandler(HandleParentEndpointClosed)
                      .EngineShutdownHandler(HandleEngineShutdown)
                      .DeliveryReadHandler(HandleDeliveryRead)
                      .DeliveryStateUpdatedHandler(HandleDeliveryStateUpdate);
      }

      internal override ITransactionManager Self()
      {
         return this;
      }

      #region Transaction manager public API

      public IConnection Connection => receiver.Connection;

      public ISession Session => receiver.Session;

      public uint Credit => receiver.Credit;

      public override bool IsLocallyOpen => receiver.IsLocallyOpen;

      public override bool IsLocallyClosed => receiver.IsLocallyClosed;

      public override bool IsRemotelyOpen => receiver.IsRemotelyOpen;

      public override bool IsRemotelyClosed => receiver.IsRemotelyClosedOrDetached;

      public override ErrorCondition ErrorCondition
      {
         get => receiver.ErrorCondition;
         set => receiver.ErrorCondition = value;
      }

      public override ErrorCondition RemoteErrorCondition
      {
         get => receiver.RemoteErrorCondition;
      }

      public override Symbol[] OfferedCapabilities
      {
         get => receiver.OfferedCapabilities;
         set => receiver.OfferedCapabilities = value;
      }

      public override Symbol[] RemoteOfferedCapabilities => receiver.RemoteOfferedCapabilities;

      public override Symbol[] DesiredCapabilities
      {
         get => receiver.DesiredCapabilities;
         set => receiver.DesiredCapabilities = value;
      }

      public override Symbol[] RemoteDesiredCapabilities => receiver.RemoteDesiredCapabilities;

      public override IReadOnlyDictionary<Symbol, object> Properties
      {
         get => receiver.Properties;
         set => receiver.Properties = value;
      }

      public override IReadOnlyDictionary<Symbol, object> RemoteProperties => receiver.RemoteProperties;

      public Source Source
      {
         get => receiver.Source;
         set => receiver.Source = value;
      }

      public Source RemoteSource
      {
         get => receiver.RemoteSource;
      }

      public Coordinator Coordinator
      {
         get => receiver.Coordinator;
         set => receiver.Coordinator = value;
      }

      public Coordinator RemoteCoordinator
      {
         get => (Coordinator)receiver.RemoteTerminus;
      }

      public ITransactionManager AddCredit(uint credit)
      {
         receiver.AddCredit(credit);
         return this;
      }

      public override ITransactionManager Open()
      {
         receiver.Open();
         return this;
      }

      public override ITransactionManager Close()
      {
         receiver.Close();
         return this;
      }

      public ITransactionManager Declared(ITransaction<ITransactionManager> transaction, byte[] txnId)
      {
         return Declared(transaction, ProtonByteBufferAllocator.Instance.Wrap(txnId));
      }

      public ITransactionManager Declared(ITransaction<ITransactionManager> transaction, IProtonBuffer txnId)
      {
         ProtonManagerTransaction txn = (ProtonManagerTransaction)transaction;

         if (txn.Parent != this)
         {
            throw new ArgumentException("Cannot complete declaration of a transaction from another transaction manager.");
         }

         if (txnId == null || !txnId.IsReadable)
         {
            throw new ArgumentException("Cannot declare a transaction without a transaction Id");
         }

         txn.State = TransactionState.Declared;
         txn.TxnId = txnId;

         // Start tracking this transaction as active.
         transactions.Add(txnId, txn);

         Declared declaration = new()
         {
            TxnId = txnId
         };

         txn.Declare.Disposition(declaration, true);

         return this;
      }

      public ITransactionManager Discharged(ITransaction<ITransactionManager> transaction)
      {
         ProtonManagerTransaction txn = (ProtonManagerTransaction)transaction;

         if (txn.Parent != this)
         {
            throw new ArgumentException("Cannot complete discharge of a transaction from another transaction manager.");
         }

         // Before sending the disposition remove if from tracking in case the write fails.
         transactions.Remove(txn.TxnId);

         txn.State = TransactionState.Discharged;
         txn.Discharge.Disposition(Accepted.Instance, true);

         return this;
      }

      public ITransactionManager DeclareFailed(ITransaction<ITransactionManager> transaction, ErrorCondition condition)
      {
         ProtonManagerTransaction txn = (ProtonManagerTransaction)transaction;

         if (txn.Parent != this)
         {
            throw new ArgumentException("Cannot fail a declared transaction from another transaction manager.");
         }

         Rejected rejected = new()
         {
            Error = condition
         };

         txn.State = TransactionState.DeclareFailed;
         txn.Declare.Disposition(rejected, true);

         return this;
      }

      public ITransactionManager DischargeFailed(ITransaction<ITransactionManager> transaction, ErrorCondition condition)
      {
         ProtonManagerTransaction txn = (ProtonManagerTransaction)transaction;

         if (txn.Parent != this)
         {
            throw new ArgumentException("Cannot fail a discharge of a transaction from another transaction manager.");
         }

         transactions.Remove(txn.TxnId);

         // TODO: We should be closing the link if the remote did not report that it supports the
         //       rejected outcome although most don't regardless of what they actually do support.

         Rejected rejected = new()
         {
            Error = condition
         };

         txn.State = TransactionState.DischargeFailed;
         txn.Discharge.Disposition(rejected, true);

         return this;
      }

      #endregion

      #region Transaction manager event handler API

      public ITransactionManager DeclareHandler(Action<ITransaction<ITransactionManager>> handler)
      {
         declareEventHandler = handler;
         return this;
      }

      public ITransactionManager DischargeHandler(Action<ITransaction<ITransactionManager>> handler)
      {
         dischargeEventHandler = handler;
         return this;
      }

      public ITransactionManager ParentEndpointClosedHandler(Action<ITransactionManager> handler)
      {
         parentEndpointClosedEventHandler = handler;
         return this;
      }

      private void FireDeclare(ProtonManagerTransaction transaction)
      {
         declareEventHandler?.Invoke(transaction);
      }

      private void FireDischarge(ProtonManagerTransaction transaction)
      {
         dischargeEventHandler?.Invoke(transaction);
      }

      private void FireParentEndpointClosed()
      {
         parentEndpointClosedEventHandler?.Invoke(this);
      }

      #endregion

      #region Handlers for events from the underlying link

      private void HandleReceiverLinkLocallyOpened(IReceiver receiver)
      {
         FireLocalOpen();
      }

      private void HandleReceiverLinkLocallyClosed(IReceiver receiver)
      {
         FireLocalClose();
      }

      private void HandleReceiverLinkOpened(IReceiver receiver)
      {
         FireRemoteOpen();
      }

      private void HandleReceiverLinkClosed(IReceiver receiver)
      {
         FireRemoteClose();
      }

      private void HandleEngineShutdown(IEngine engine)
      {
         FireEngineShutdown();
      }

      private void HandleParentEndpointClosed(IReceiver receiver)
      {
         FireParentEndpointClosed();
      }

      private void HandleDeliveryRead(IIncomingDelivery delivery)
      {
         if (delivery.IsAborted)
         {
            delivery.Settle();
         }
         else if (!delivery.IsPartial)
         {
            IProtonBuffer payload = delivery.ReadAll();

            AmqpValue container = (AmqpValue)payloadDecoder.ReadObject(payload, payloadDecoder.CachedDecoderState);

            if (container.Value is Declare)
            {
               ProtonManagerTransaction transaction = new(this)
               {
                  Declare = delivery,
                  State = TransactionState.Declaring
               };

               FireDeclare(transaction);
            }
            else if (container.Value is Discharge discharge)
            {
               IProtonBuffer txnId = discharge.TxnId;

               if (transactions.TryGetValue(txnId, out ProtonManagerTransaction transaction))
               {
                  transaction.State = TransactionState.Discharging;
                  transaction.DischargeState = discharge.Fail ? DischargeState.Rollback : DischargeState.Commit;
                  transaction.Discharge = delivery;

                  FireDischarge(transaction);
               }
               else
               {
                  // TODO: If the remote did not indicate it supports reject we should really close the link.
                  ErrorCondition rejection = new(
                     TransactionError.UNKNOWN_ID, "Transaction Manager is not tracking the given transaction ID.");
                  delivery.Disposition(new Rejected(rejection), true);
               }
            }
            else
            {
               throw new ProtocolViolationException("TXN Coordinator expects Declare and Discharge Delivery payloads only");
            }
         }
      }

      private void HandleDeliveryStateUpdate(IIncomingDelivery delivery)
      {
         // Nothing to do yet
      }

      #endregion

      #region Internal Transaction Manager Transaction type

      private sealed class ProtonManagerTransaction : ProtonTransaction<ITransactionManager>, ITransaction<ITransactionManager>
      {
         private readonly ProtonTransactionManager manager;

         private IIncomingDelivery declare;
         private IIncomingDelivery discharge;

         public ProtonManagerTransaction(ProtonTransactionManager manager)
         {
            this.manager = manager;
         }

         public override ProtonTransactionManager Parent => manager;

         public IIncomingDelivery Declare
         {
            get => declare;
            set => declare = value;
         }

         public IIncomingDelivery Discharge
         {
            get => discharge;
            set => discharge = value;
         }
      }

      #endregion
   }
}