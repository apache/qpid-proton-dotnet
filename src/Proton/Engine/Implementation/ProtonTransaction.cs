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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Base class for ITransaction types in the Proton engine which provides the
   /// basic API implementation that all transactions will expose.
   /// </summary>
   /// <typeparam name="T"></typeparam>
   public abstract class ProtonTransaction<T> : ITransaction<T> where T : IEndpoint<T>
   {
      private TransactionState state = TransactionState.Idle;
      private DischargeState dischargeState = DischargeState.None;
      private ErrorCondition condition;
      private IProtonBuffer txnId;

      private ProtonAttachments attachments;
      private Object linkedResource;

      public TransactionState State
      {
         get => state;
         set => state = value;
      }

      public DischargeState DischargeState
      {
         get => dischargeState;
         internal set => dischargeState = value;
      }

      public ErrorCondition Error
      {
         get => condition;
         internal set => condition = value;
      }

      public IAttachments Attachments => attachments ??= new ProtonAttachments();

      public object LinkedResource
      {
         get => linkedResource;
         set => linkedResource = value;
      }

      public IProtonBuffer TxnId
      {
         get => txnId?.Copy();
         internal set => txnId = value;
      }

      public bool IsDeclared => state == TransactionState.Declared;

      public bool IsDischarged => state == TransactionState.Discharged;

      public bool IsFailed => state > TransactionState.Discharged;

      public abstract T Parent { get; }

   }
}