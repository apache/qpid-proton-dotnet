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

      internal bool IsAnonymous => protonSender.Target.Address == null;

      internal bool IsSendingSettled => sendsSettled;

      internal SenderOptions Options => options;

      internal ClientSender Open()
      {
         protonSender.LocalOpenHandler(HandleLocalOpen)
                     .LocalCloseHandler(HandleLocalCloseOrDetach)
                     .LocalDetachHandler(HandleLocalCloseOrDetach)
                     .OpenHandler(HandleRemoteOpen)
                     .CloseHandler(HandleRemoteCloseOrDetach)
                     .DetachHandler(HandleRemoteCloseOrDetach)
                     .ParentEndpointClosedHandler(HandleParentEndpointClosed)
                     .CreditStateUpdateHandler(HandleCreditStateUpdated)
                     .EngineShutdownHandler(HandleEngineShutdown)
                     .Open();

         return this;
      }

      internal void Disposition(IOutgoingDelivery delivery, Types.Transport.IDeliveryState state, bool settled)
      {
         CheckClosedOrFailed();
         // TODO
         //   executor.execute(() -> {
         //       delivery.disposition(state, settled);
         //   });
      }

      void HandleUpdateAnonymousRelayNotSupported()
      {
         if (IsAnonymous && protonSender.LinkState == LinkState.Idle)
         {
            ImmediateLinkShutdown(new ClientUnsupportedOperationException(
               "Anonymous relay support not available from this connection"));
         }
      }

      #endregion

      #region Private Receiver Implementation

      private ITracker CreateTracker(IOutgoingDelivery delivery)
      {
         return new ClientTracker(this, delivery);
      }

      private ITracker CreateNoOpTracker()
      {
         return new ClientNoOpTracker(this);
      }

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

      private void ImmediateLinkShutdown(ClientException failureCause)
      {
         if (this.failureCause == null)
         {
            this.failureCause = failureCause;
         }

         try
         {
            if (protonSender.IsRemotelyDetached)
            {
               protonSender.Detach();
            }
            else
            {
               protonSender.Close();
            }
         }
         catch (Exception)
         {
            // Ignore
         }

         FailPendingUnsettledAndBlockedSends(
            failureCause ?? new ClientResourceRemotelyClosedException("The sender link has closed"));

         // TODO
         // if (failureCause != null)
         // {
         //    openFuture.failed(failureCause);
         // }
         // else
         // {
         //    openFuture.complete(this);
         // }

         // closeFuture.complete(this);
      }

      private void FailPendingUnsettledAndBlockedSends(ClientException cause)
      {
         // Cancel all settlement futures for in-flight sends passing an appropriate error to the future
         foreach (IOutgoingDelivery delivery in protonSender.Unsettled)
         {
            try
            {
               ClientTracker tracker = delivery.LinkedResource as ClientTracker;
               // TODO tracker.SettlementTask.Failed(cause);
            }
            catch (Exception)
            {
            }
         }

         // TODO
         // Cancel all blocked sends passing an appropriate error to the future
         //   blocked.removeIf((held) -> {
         //       held.failed(cause);
         //       return true;
         //   });
      }

      #endregion

      #region Proton Sender lifecycle envent handlers

      private void HandleLocalOpen(Engine.ISender sender)
      {
         throw new NotImplementedException();
      }

      private void HandleLocalCloseOrDetach(Engine.ISender sender)
      {
         throw new NotImplementedException();
      }

      private void HandleRemoteOpen(Engine.ISender sender)
      {
         throw new NotImplementedException();
      }

      private void HandleRemoteCloseOrDetach(Engine.ISender sender)
      {
         if (sender.IsLocallyOpen)
         {
            try
            {
               senderRemotelyClosedHandler.Invoke(this);
            }
            catch (Exception) { }

            ImmediateLinkShutdown(ClientExceptionSupport.ConvertToLinkClosedException(
                sender.RemoteErrorCondition, "Sender remotely closed without explanation from the remote"));
         }
         else
         {
            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleParentEndpointClosed(Engine.ISender sender)
      {
         // Don't react if engine was shutdown and parent closed as a result instead wait to get the
         // shutdown notification and respond to that change.
         if (sender.Engine.IsRunning)
         {
            ClientException failureCause;

            if (sender.Connection.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(sender.Connection.RemoteErrorCondition);
            }
            else if (sender.Session.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToSessionClosedException(sender.Session.RemoteErrorCondition);
            }
            else if (sender.Engine.FailureCause != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(sender.Engine.FailureCause);
            }
            else if (!IsClosed)
            {
               failureCause = new ClientResourceRemotelyClosedException("Remote closed without a specific error condition");
            }
            else
            {
               failureCause = null;
            }

            ImmediateLinkShutdown(failureCause);
         }
      }

      private void HandleCreditStateUpdated(Engine.ISender sender)
      {
         throw new NotImplementedException();
      }

      private void HandleEngineShutdown(Engine.IEngine engine)
      {
         if (!IsDynamic && !session.ProtonSession.Engine.IsShutdown)
         {
            protonSender.LocalCloseHandler(null);
            protonSender.LocalDetachHandler(null);
            protonSender.Close();
            if (protonSender.HasUnsettled)
            {
               FailPendingUnsettledAndBlockedSends(
                   new ClientConnectionRemotelyClosedException("Connection failed and send result is unknown"));
            }
            protonSender = ClientSenderBuilder.RecreateSender(session, protonSender, options);
            protonSender.LinkedResource = this;

            Open();
         }
         else
         {
            Engine.IConnection connection = engine.Connection;
            ClientException failureCause;

            if (connection.RemoteErrorCondition != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(connection.RemoteErrorCondition);
            }
            else if (engine.FailureCause != null)
            {
               failureCause = ClientExceptionSupport.ConvertToConnectionClosedException(engine.FailureCause);
            }
            else if (!IsClosed)
            {
               failureCause = new ClientConnectionRemotelyClosedException("Remote closed without a specific error condition");
            }
            else
            {
               failureCause = null;
            }

            ImmediateLinkShutdown(failureCause);
         }
      }

      #endregion
   }
}