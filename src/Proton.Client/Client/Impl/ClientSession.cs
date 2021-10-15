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

namespace Apache.Qpid.Proton.Client.Impl
{
   // TODO
   public class ClientSession : ISession
   {
      private readonly SessionOptions options;
      private readonly ClientConnection connection;
      private readonly string sessionId;

      private bool disposedValue;
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

      public IDictionary<string, object> Properties => throw new NotImplementedException();

      public ICollection<string> OfferedCapabilities => throw new NotImplementedException();

      public ICollection<string> DesiredCapabilities => throw new NotImplementedException();

      public ISession BeginTransaction()
      {
         throw new NotImplementedException();
      }

      public void Close()
      {
         throw new NotImplementedException();
      }

      public void Close(IErrorCondition error)
      {
         throw new NotImplementedException();
      }

      public Task<ISession> CloseAsync()
      {
         throw new NotImplementedException();
      }

      public Task<ISession> CloseAsync(IErrorCondition error)
      {
         throw new NotImplementedException();
      }

      public ISession CommitTransaction()
      {
         throw new NotImplementedException();
      }

      public ISender OpenAnonymousSender()
      {
         throw new NotImplementedException();
      }

      public ISender OpenAnonymousSender(SenderOptions options)
      {
         throw new NotImplementedException();
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName)
      {
         throw new NotImplementedException();
      }

      public IReceiver OpenDurableReceiver(string address, string subscriptionName, ReceiverOptions options)
      {
         throw new NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver()
      {
         throw new NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver(ReceiverOptions options)
      {
         throw new NotImplementedException();
      }

      public IReceiver OpenDynamicReceiver(IDictionary<string, object> dynamicNodeProperties, ReceiverOptions options)
      {
         throw new NotImplementedException();
      }

      public IReceiver OpenReceiver(string address)
      {
         throw new NotImplementedException();
      }

      public IReceiver OpenReceiver(string address, ReceiverOptions options)
      {
         throw new NotImplementedException();
      }

      public ISender OpenSender(string address)
      {
         throw new NotImplementedException();
      }

      public ISender OpenSender(string address, SenderOptions options)
      {
         throw new NotImplementedException();
      }

      public ISession RollbackTransaction()
      {
         throw new NotImplementedException();
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!disposedValue)
         {
            if (disposing)
            {
               // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
         }
      }

      // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      // ~ClientSession()
      // {
      //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      //     Dispose(disposing: false);
      // }

      public void Dispose()
      {
         // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
         Dispose(disposing: true);
         GC.SuppressFinalize(this);
      }
   }
}