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
   /// <summary>
   /// A local transaction based context for AMQP transactional sessions.
   /// </summary>
   internal sealed class ClientLocalTransactionContext : IClientTransactionContext
   {
      private readonly ClientSession session;

      // TODO tie into proton transaction contexts

      public ClientLocalTransactionContext(ClientSession session)
      {
         this.session = session;
      }

      public bool IsInTransaction => false;

      public bool IsRollbackOnly => false;

      public IClientTransactionContext Begin(TaskCompletionSource<ISession> beginFuture)
      {
         throw new NotImplementedException();
      }

      public IClientTransactionContext Commit(TaskCompletionSource<ISession> commitFuture, bool startNew)
      {
         throw new NotImplementedException();
      }

      public IClientTransactionContext Rollback(TaskCompletionSource<ISession> rollbackFuture, bool startNew)
      {
         throw new NotImplementedException();
      }

      public IClientTransactionContext Send(ClientOutgoingEnvelope envelope, Types.Transport.IDeliveryState state, bool settled)
      {
         envelope.SendPayload(state, settled);
         return this;
      }

      public IClientTransactionContext Disposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settled)
      {
         delivery.Disposition(state, settled);
         return this;
      }
   }
}