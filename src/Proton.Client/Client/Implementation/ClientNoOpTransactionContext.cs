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

using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// A pass-through transaction context that is used when a session is operating
   /// without any active transactional state.
   /// </summary>
   internal sealed class ClientNoOpTransactionContext : IClientTransactionContext
   {
      public bool IsInTransaction => false;

      public bool IsRollbackOnly => false;

      public IClientTransactionContext Begin(Task<ISession> beginFuture)
      {
         throw new ClientIllegalStateException("Cannot begin from a no-op transaction context");
      }

      public IClientTransactionContext Commit(Task<ISession> commitFuture, bool startNew)
      {
         throw new ClientIllegalStateException("Cannot commit from a no-op transaction context");
      }

      public IClientTransactionContext Rollback(Task<ISession> rollbackFuture, bool startNew)
      {
         throw new ClientIllegalStateException("Cannot roll back from a no-op transaction context");
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