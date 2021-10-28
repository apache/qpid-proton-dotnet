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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Client.Threading;

namespace Apache.Qpid.Proton.Client.Implementation
{
   // TODO
   public class ClientSession : ISession
   {
      private readonly SessionOptions options;
      private readonly ClientConnection connection;
      private readonly string sessionId;
      private readonly AtomicBoolean closed = new AtomicBoolean();

      private Engine.ISession protonSession;
      private Exception failureCause;

      public ClientSession(ClientConnection connection, SessionOptions options, string sessionId, Engine.ISession session)
      {
         this.options = new SessionOptions(options);
         this.connection = connection;
         this.protonSession = session;
         this.sessionId = sessionId;
      }

      public IClient Client => throw new NotImplementedException();

      public IConnection Connection => throw new NotImplementedException();

      public Task<ISession> OpenTask => throw new NotImplementedException();

      public IReadOnlyDictionary<string, object> Properties => throw new NotImplementedException();

      public IReadOnlyCollection<string> OfferedCapabilities => throw new NotImplementedException();

      public IReadOnlyCollection<string> DesiredCapabilities => throw new NotImplementedException();

      public ISession BeginTransaction()
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         try
         {
            Close();
         }
         catch (Exception)
         {
            // TODO Log something helpful
         }
         finally
         {
            System.GC.SuppressFinalize(this);
         }
      }

      public void Close(IErrorCondition error = null)
      {
         if (closed.CompareAndSet(false, true))
         {

         }

         throw new NotImplementedException();
      }

      public Task<ISession> CloseAsync(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public ISession CommitTransaction()
      {
         throw new NotImplementedException();
      }

      public virtual ISender OpenAnonymousSender(SenderOptions options = null)
      {
         throw new NotImplementedException();
      }

      public virtual IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options = null)
      {
         throw new NotImplementedException();
      }

      public virtual IReceiver OpenDynamicReceiver(ReceiverOptions options = null, IDictionary<string, object> dynamicNodeProperties = null)
      {
         throw new NotImplementedException();
      }

      public virtual IReceiver OpenReceiver(string address, ReceiverOptions options = null)
      {
         throw new NotImplementedException();
      }

      public virtual ISender OpenSender(string address, SenderOptions options = null)
      {
         throw new NotImplementedException();
      }

      public ISession RollbackTransaction()
      {
         throw new NotImplementedException();
      }

      protected void CheckClosedOrFailed()
      {
         if (IsClosed())
         {
            throw new ClientIllegalStateException("The Session was explicitly closed", failureCause);
         }
         else if (failureCause != null)
         {
            throw failureCause;
         }
      }

      #region Internal client session API

      internal string SessionId => sessionId;

      internal Engine.ISession ProtonSession => protonSession;

      internal bool IsClosed()
      {
         return closed;
      }

      internal SessionOptions Options => options;

      internal ClientSession Open()
      {
         // TODO
         return this;
      }

      #endregion
   }
}