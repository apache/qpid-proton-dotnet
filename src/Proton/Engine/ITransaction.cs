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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// A Transaction object that hold information and context for a single Transaction
   /// </summary>
   public interface ITransaction<T> where T : IEndpoint<T>
   {
      /// <summary>
      /// Provides access to the parent that owns this transaction instance.
      /// </summary>
      T Parent { get; }

      /// <summary>
      /// Access the attachments instance that allows callers to attach state data
      /// to an transaction instance.
      /// </summary>
      IAttachments Attachments { get; }

      /// <summary>
      /// Allows the endpoint to have some user defined resource linked to it
      /// which can be used to store application state data or other associated
      /// object instances with this transaction.
      /// </summary>
      object LinkedResource { get; set; }

      /// <summary>
      /// Returns the current transaction state that reflect where in its lifetime
      /// this transaction instance is.
      /// </summary>
      TransactionState State { get; }

      /// <summary>
      /// For a Transaction that has either been requested to discharge or has successfully
      /// discharged the discharge state reflects whether the transaction was to be committed
      /// or rolled back. Prior to a discharge being attempted there is no state value and
      /// this method returns a discharge state of none.
      /// </summary>
      DischargeState DischargeState { get; }

      /// <summary>
      /// Returns the transaction Id that is associated with the declared transaction. Prior
      /// to a transaction manager completing a transaction declaration this method will return
      ///  null to indicate that the transaction has not been declared yet.
      /// </summary>
      IProtonBuffer TxnId { get; }

      /// <summary>
      /// Checks if the transaction has been marked as declared by the transaction manager.
      /// </summary>
      bool IsDeclared { get; }

      /// <summary>
      /// Checks if the transaction has been marked as discharged by the transaction manager.
      /// </summary>
      bool IsDischarged { get; }

      /// <summary>
      /// The parent resource will mark the Transaction as failed is any of the operations
      /// performed on it cannot be successfully completed such as a declare operation failing
      /// to write due to an IO error.
      /// </summary>
      bool IsFailed { get; }

      /// <summary>
      /// If the declare or discharge of the transaction caused its state to become failed
      /// this method returns the error condition object that the remote used to describe
      /// the reason for the failure.
      /// </summary>
      ErrorCondition Error { get; }

   }
}