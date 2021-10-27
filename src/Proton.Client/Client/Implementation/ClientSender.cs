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
   /// <summary>
   /// TODO
   /// </summary>
   public sealed class ClientSender : ISender
   {
      private readonly AtomicBoolean closed = new AtomicBoolean();
      private ClientException failureCause;

      private readonly SenderOptions options;
      private readonly ClientSession session;
      private readonly string senderId;
      private readonly bool sendsSettled;
      private Engine.ISender protonSender;
      private Action<ISender> senderRemotelyClosedHandler;

      private volatile ISource remoteSource;
      private volatile ITarget remoteTarget;

      internal ClientSender(ClientSession session, SenderOptions options, string senderId, Engine.ISender protonSender)
      {
         this.options = new SenderOptions(options);
         this.session = session;
         this.senderId = senderId;
         this.protonSender = protonSender;
         this.protonSender.LinkedResource = this;
         this.sendsSettled = protonSender.SenderSettleMode == Types.Transport.SenderSettleMode.Settled;
      }

      public IClient Client => session.Client;

      public IConnection Connection => session.Connection;

      public ISession Session => session;

      public Task<ISender> OpenTask => throw new NotImplementedException();

      public string Address
      {
         get
         {
            if (IsDynamic)
            {
               WaitForOpenToComplete();
               return (protonSender.RemoteTerminus as ITarget)?.Address;
            }
            else
            {
               return protonSender.Target?.Address;
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
            return ClientConversionSupport.ToStringKeyedMap(protonSender.RemoteProperties);
         }
      }

      public IReadOnlyCollection<string> OfferedCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSender.OfferedCapabilities);
         }
      }

      public IReadOnlyCollection<string> DesiredCapabilities
      {
         get
         {
            WaitForOpenToComplete();
            return ClientConversionSupport.ToStringArray(protonSender.DesiredCapabilities);
         }
      }

      public void Close(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public Task<ISender> CloseAsync(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public void Detach(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public Task<ISender> DetachAsync(IErrorCondition error = null)
      {
         throw new NotImplementedException();
      }

      public void Dispose()
      {
         throw new NotImplementedException();
      }

      public ITracker Send<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         throw new NotImplementedException();
      }

      public ITracker TrySend<T>(IMessage<T> message, IDictionary<string, object> deliveryAnnotations = null)
      {
         throw new NotImplementedException();
      }

      #region Internal Sender API

      internal string SenderId => senderId;

      internal bool IsClosed => closed;

      internal bool IsDynamic => protonSender.Target?.Dynamic ?? false;

      internal SenderOptions Options => options;

      internal void Disposition(IOutgoingDelivery delivery, Types.Transport.IDeliveryState state, bool settled)
      {
         CheckClosedOrFailed();
         // TODO
         //   executor.execute(() -> {
         //       delivery.disposition(state, settled);
         //   });
      }

      #endregion

      #region Private Receiver Implementation

      private void CheckClosedOrFailed()
      {
         if (IsClosed)
         {
            throw new ClientIllegalStateException("The Sender was explicitly closed", failureCause);
         }
         else if (failureCause != null)
         {
            throw failureCause;
         }
      }

      private void WaitForOpenToComplete()
      {
         // TODO
      }

      #endregion
   }
}