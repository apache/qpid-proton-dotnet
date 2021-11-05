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
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   // TODO
   public sealed class ClientStreamReceiver : IStreamReceiver
   {
      private readonly StreamReceiverOptions options;
      private readonly ClientSession session;
      private readonly string receiverId;
      private readonly AtomicBoolean closed = new AtomicBoolean();
      private readonly TaskCompletionSource<IReceiver> openFuture = new TaskCompletionSource<IReceiver>();
      private readonly TaskCompletionSource<IStreamReceiver> closeFuture = new TaskCompletionSource<IStreamReceiver>();

      private Engine.IReceiver protonReceiver;
      private ClientException failureCause;
      private volatile ISource remoteSource;
      private volatile ITarget remoteTarget;

      internal ClientStreamReceiver(ClientSession session, StreamReceiverOptions options, string receiverId, Engine.IReceiver receiver)
      {
         this.options = options;
         this.session = session;
         this.receiverId = receiverId;
         this.protonReceiver = receiver;
         this.protonReceiver.LinkedResource = this;

         if (options.CreditWindow > 0)
         {
            protonReceiver.AddCredit(options.CreditWindow);
         }
      }

      public IClient Client => session.Client;

      public IConnection Connection => session.Connection;

      public ISession Session => session;

      public Task<IReceiver> OpenTask => openFuture.Task;

      public string Address
      {
         get
         {
            if (IsDynamic)
            {
               WaitForOpenToComplete();
               return protonReceiver.RemoteSource?.Address;
            }
            else
            {
               return protonReceiver.Source?.Address;
            }
         }
      }

      public ISource Source
      {
         get
         {
            WaitForOpenToComplete();
            return remoteSource;
         }
      }

      public ITarget Target
      {
         get
         {
            WaitForOpenToComplete();
            return remoteTarget;
         }
      }

      public IReadOnlyDictionary<string, object> Properties
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringKeyedMap(protonReceiver.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonReceiver.OfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonReceiver.DesiredCapabilities);
         }
      }

      public int QueuedDeliveries
      {
         get
         {
            WaitForOpenToComplete();
            throw new NotImplementedException();
         }
      }

      public IStreamReceiver AddCredit(int credit)
      {
         throw new NotImplementedException();
      }

      public void Close(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> CloseAsync(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public void Detach(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> DetachAsync(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public Task<IReceiver> Drain()
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery Receive()
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery Receive(TimeSpan timeout)
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery TryReceive()
      {
         throw new NotImplementedException();
      }

      #region Internal Receiver API

      internal ClientStreamReceiver Open()
      {
         // TODO

         return this;
      }

      internal void Disposition(IIncomingDelivery delivery, Types.Transport.IDeliveryState state, bool settle)
      {
         // TODO CheckClosedOrFailed();
         // asyncApplyDisposition(delivery, state, settle);
      }

      internal String ReceiverId => receiverId;

      internal bool IsClosed => closed;

      internal bool IsDynamic => protonReceiver.Source?.Dynamic ?? false;

      internal Exception FailureCause => failureCause;

      internal StreamReceiverOptions ReceiverOptions => options;

      #endregion

      #region Private Receiver Implementation

      private void WaitForOpenToComplete()
      {
         if (!openFuture.Task.IsCompleted || openFuture.Task.IsFaulted)
         {
            try
            {
               openFuture.Task.Wait();
            }
            catch (Exception e)
            {
               throw failureCause ?? ClientExceptionSupport.CreateNonFatalOrPassthrough(e);
            }
         }
      }

      #endregion
   }
}