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
using System.Collections.ObjectModel;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Logging;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   internal enum LinkOperabilityState
   {
      Ok,
      LinkRemotelyDetached,
      LinkLocallyDetached,
      LinkRemotelyClosed,
      LinkLocallyClosed,
      SessionRemotelyClosed,
      SessionLocallyClosed,
      ConnectionRemotelyClosed,
      ConnectionLocallyClosed,
      EngineShutdown
   }

   /// <summary>
   /// Common base for Sender and Receiver links which provides services that both
   /// endpoints require that are the same between them.
   /// </summary>
   public abstract class ProtonLink<T> : ProtonEndpoint<T>, ILink<T>, IProtonLink where T : ILink<T>
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ProtonLink<T>>();

      protected readonly ProtonConnection connection;
      protected readonly ProtonSession session;

      protected readonly Attach localAttach = new();
      protected Attach remoteAttach;

      private bool localAttachSent;
      private bool localDetachSent;

      private readonly ProtonLinkCreditState creditState;

      private LinkOperabilityState operability = LinkOperabilityState.Ok;
      private LinkState localState = LinkState.Idle;
      private LinkState remoteState = LinkState.Idle;

      private Action<T> localDetachHandler;
      private Action<T> remoteDetachHandler;
      private Action<T> parentEndpointClosedHandler;
      private Action<T> linkCreditStateUpdateHandler;

      protected ProtonLink(ProtonSession session, string name, ProtonLinkCreditState creditState) : base(session.ProtonEngine)
      {
         this.session = session;
         this.connection = (ProtonConnection)session.Connection;
         this.creditState = creditState;
         this.localAttach.Name = name;
         this.localAttach.Role = Role;
      }

      public override T Open()
      {
         if (LinkState == LinkState.Idle)
         {
            CheckLinkOperable("Cannot open Link");
            localState = LinkState.Active;
            uint localHandle = session.FindFreeLocalHandle(this);
            localAttach.Handle = localHandle;
            TransitionedToLocallyOpened();
            try
            {
               TrySyncLocalStateWithRemote();
            }
            finally
            {
               FireLocalOpen();
            }
         }

         return Self();
      }

      public T Detach()
      {
         if (localState == LinkState.Active)
         {
            localState = LinkState.Detached;
            if (operability < LinkOperabilityState.LinkLocallyDetached)
            {
               operability = LinkOperabilityState.LinkLocallyDetached;
            }
            CreditState.ClearCredit();
            TransitionedToLocallyDetached();
            try
            {
               engine.CheckFailed("Closed called on already failed connection");
               TrySyncLocalStateWithRemote();
            }
            finally
            {
               FireLocalDetach();
            }
         }

         return Self();
      }

      public override T Close()
      {
         if (localState == LinkState.Active)
         {
            localState = LinkState.Closed;
            if (operability < LinkOperabilityState.LinkLocallyClosed)
            {
               operability = LinkOperabilityState.LinkLocallyClosed;
            }
            CreditState.ClearCredit();
            TransitionedToLocallyClosed();
            try
            {
               engine.CheckFailed("Detached called on already failed connection");
               TrySyncLocalStateWithRemote();
            }
            finally
            {
               FireLocalClose();
            }
         }

         return Self();
      }

      #region Accessors for common link Attributes

      public virtual IConnection Connection => connection;

      public virtual ISession Session => session;

      public bool IsSender => Role == Role.Sender;

      public bool IsReceiver => Role == Role.Receiver;

      public uint Handle => localAttach.Handle;

      public string Name => localAttach.Name;

      public LinkState LinkState => localState;

      public LinkState RemoteState => remoteState;

      public SenderSettleMode SenderSettleMode
      {
         get => localAttach.SenderSettleMode;
         set
         {
            CheckNotOpened("Cannot set Sender settlement mode on already opened Link");
            localAttach.SenderSettleMode = value;
         }
      }

      public SenderSettleMode RemoteSenderSettleMode =>
         remoteAttach?.SenderSettleMode ?? throw new InvalidOperationException("Remote Attach not yet received");

      public ReceiverSettleMode ReceiverSettleMode
      {
         get => localAttach.ReceiverSettleMode;
         set
         {
            CheckNotOpened("Cannot set Receiver settlement mode already opened Link");
            localAttach.ReceiverSettleMode = value;
         }
      }

      public ReceiverSettleMode RemoteReceiverSettleMode =>
         remoteAttach?.ReceiverSettleMode ?? throw new InvalidOperationException("Remote Attach not yet received");

      public Source Source
      {
         get => localAttach.Source;
         set
         {
            CheckNotOpened("Cannot set Source on already opened Link");
            localAttach.Source = value;
         }
      }

      public Source RemoteSource => remoteAttach?.Source?.Copy();

      public ITerminus Terminus
      {
         get => localAttach.Target;
         set
         {
            CheckNotOpened("Cannot set Target on already opened Link");
            localAttach.Target = value;
         }
      }

      public Coordinator Coordinator
      {
         get => (Coordinator)localAttach.Target;
         set
         {
            CheckNotOpened("Cannot set Coordinator on already opened Link");
            localAttach.Target = value;
         }
      }

      public Target Target
      {
         get => (Target)localAttach.Target;
         set
         {
            CheckNotOpened("Cannot set Target on already opened Link");
            localAttach.Target = value;
         }
      }

      public ITerminus RemoteTerminus => remoteAttach?.Target?.Copy();

      public override Symbol[] OfferedCapabilities
      {
         get => (Symbol[])(localAttach.OfferedCapabilities?.Clone());
         set
         {
            CheckNotOpened("Cannot set Offered Capabilities on already opened Link");
            localAttach.OfferedCapabilities = (Symbol[])(value?.Clone());
         }
      }

      public override Symbol[] DesiredCapabilities
      {
         get => (Symbol[])(localAttach.DesiredCapabilities?.Clone());
         set
         {
            CheckNotOpened("Cannot set Desired Capabilities on already opened Link");
            localAttach.DesiredCapabilities = (Symbol[])(value?.Clone());
         }
      }

      public override Symbol[] RemoteOfferedCapabilities
      {
         get => (Symbol[])(remoteAttach?.OfferedCapabilities?.Clone());
      }

      public override Symbol[] RemoteDesiredCapabilities
      {
         get => (Symbol[])(remoteAttach?.DesiredCapabilities?.Clone());
      }

      public override IReadOnlyDictionary<Symbol, object> Properties
      {
         get
         {
            if (localAttach.Properties != null)
            {
               return new ReadOnlyDictionary<Symbol, object>(localAttach.Properties);
            }
            else
            {
               return null;
            }
         }
         set
         {
            CheckNotOpened("Cannot set Properties on already opened Link");

            if (value != null)
            {
               localAttach.Properties = new Dictionary<Symbol, object>(value);
            }
            else
            {
               localAttach.Properties = null;
            }
         }
      }

      public override IReadOnlyDictionary<Symbol, object> RemoteProperties
      {
         get
         {
            if (remoteAttach.Properties != null)
            {
               return new ReadOnlyDictionary<Symbol, object>(remoteAttach.Properties);
            }
            else
            {
               return null;
            }
         }
      }

      public ulong MaxMessageSize
      {
         get => localAttach.MaxMessageSize;
         set
         {
            CheckNotOpened("Cannot set Max Message Size on already opened Link");
            localAttach.MaxMessageSize = value;
         }
      }

      public override bool IsLocallyOpen => LinkState == LinkState.Active;

      public override bool IsLocallyClosed => LinkState == LinkState.Closed;

      public override bool IsRemotelyOpen => RemoteState == LinkState.Active;

      public override bool IsRemotelyClosed => RemoteState == LinkState.Closed;

      public bool IsLocallyDetached => LinkState == LinkState.Detached;

      public bool IsLocallyClosedOrDetached => LinkState > LinkState.Active;

      public bool IsRemotelyDetached => RemoteState == LinkState.Detached;

      public bool IsRemotelyClosedOrDetached => RemoteState > LinkState.Active;

      public T DetachHandler(Action<T> handler)
      {
         this.remoteDetachHandler = handler;
         return Self();
      }

      internal Action<T> DetachHandler()
      {
         return remoteDetachHandler;
      }

      internal T FireRemoteDetach()
      {
         if (remoteDetachHandler != null)
         {
            remoteDetachHandler.Invoke(Self());
         }
         else
         {
            FireRemoteClose();
         }

         return Self();
      }

      public T LocalDetachHandler(Action<T> handler)
      {
         this.localDetachHandler = handler;
         return Self();
      }

      internal Action<T> LocalDetachHandler()
      {
         return localDetachHandler;
      }

      internal T FireLocalDetach()
      {
         if (localDetachHandler != null)
         {
            localDetachHandler.Invoke(Self());
         }
         else
         {
            FireLocalClose();
         }

         return Self();
      }

      public T ParentEndpointClosedHandler(Action<T> handler)
      {
         this.parentEndpointClosedHandler = handler;
         return Self();
      }

      internal Action<T> ParentEndpointClosedHandler()
      {
         return parentEndpointClosedHandler;
      }

      internal T FireParentEndpointClosed()
      {
         if (parentEndpointClosedHandler != null && IsLocallyOpen)
         {
            parentEndpointClosedHandler.Invoke(Self());
         }

         return Self();
      }

      public T CreditStateUpdateHandler(Action<T> handler)
      {
         this.linkCreditStateUpdateHandler = handler;
         return Self();
      }

      internal Action<T> CreditStateUpdateHandler()
      {
         return linkCreditStateUpdateHandler;
      }

      internal T FireCreditStateUpdated()
      {
         linkCreditStateUpdateHandler?.Invoke(Self());

         return Self();
      }

      #endregion

      #region Abstract link APIs that the subclass must implement.

      public abstract uint Credit { get; }

      public abstract bool IsDraining { get; }

      public abstract Role Role { get; }

      #endregion

      #region  Abstract methods required for specialization of the link type

      protected abstract void HandleRemoteAttach(Attach attach);

      protected abstract void HandleRemoteDetach(Detach detach);

      protected abstract void HandleRemoteFlow(Flow flow);

      protected abstract void HandleRemoteDisposition(Disposition disposition, ProtonOutgoingDelivery delivery);

      protected abstract void HandleRemoteDisposition(Disposition disposition, ProtonIncomingDelivery delivery);

      protected abstract void HandleRemoteTransfer(Transfer transfer, IProtonBuffer payload, out ProtonIncomingDelivery delivery);

      protected abstract void HandleDecorateOfOutgoingFlow(Flow flow);

      protected abstract void HandleSessionCreditStateUpdates(in ProtonSessionOutgoingWindow window);

      protected abstract void HandleSessionCreditStateUpdates(in ProtonSessionIncomingWindow window);

      #endregion

      #region Link state change handlers that must be overridden by specific link implementations

      protected virtual void TransitionedToLocallyOpened()
      {
      }

      protected virtual void TransitionedToLocallyDetached()
      {
      }

      protected virtual void TransitionedToLocallyClosed()
      {
      }

      protected virtual void TransitionToRemotelyOpenedState()
      {
      }

      protected virtual void TransitionToRemotelyDetached()
      {
      }

      protected virtual void TransitionToRemotelyClosed()
      {
      }

      protected virtual void TransitionToParentLocallyClosed()
      {
      }

      protected virtual void TransitionToParentRemotelyClosed()
      {
      }

      #endregion

      #region Utility APIs provided for use here and in the subclasses

      protected ProtonLinkCreditState CreditState => creditState;

      protected bool WasLocalAttachSent => localAttachSent;

      protected bool WasLocalDetachSent => localDetachSent;

      protected void CheckLinkOperable(string failurePrefix)
      {
         switch (operability)
         {
            case LinkOperabilityState.Ok:
               break;
            case LinkOperabilityState.EngineShutdown:
               throw new EngineShutdownException(failurePrefix + ": " + operability.ToString());
            default:
               throw new InvalidOperationException(failurePrefix + ": " + operability.ToString());
         }
      }

      protected bool AreDeliveriesStillActive()
      {
         return operability switch
         {
            LinkOperabilityState.Ok or LinkOperabilityState.LinkRemotelyDetached or LinkOperabilityState.LinkLocallyDetached => true,
            _ => false,
         };
      }

      protected void CheckNotOpened(string errorMessage)
      {
         if (localState > LinkState.Idle)
         {
            throw new InvalidOperationException(errorMessage);
         }
      }

      protected void CheckNotClosed(string errorMessage)
      {
         if (localState > LinkState.Active)
         {
            throw new InvalidOperationException(errorMessage);
         }
      }

      protected void TrySyncLocalStateWithRemote()
      {
         switch (localState)
         {
            case LinkState.Idle:
               break;
            case LinkState.Active:
               TrySendLocalAttach();
               break;
            case LinkState.Closed:
            case LinkState.Detached:
               TrySendLocalAttach();
               TrySendLocalDetach(IsLocallyClosed);
               break;
         }
      }

      private void TrySendLocalAttach()
      {
         if (!WasLocalAttachSent)
         {
            if ((session.IsLocallyOpen && session.WasLocalBeginSent) &&
                (connection.IsLocallyOpen && connection.WasLocalOpenSent))
            {

               engine.FireWrite(localAttach, session.LocalChannel);
               localAttachSent = true;

               if (IsLocallyOpen && IsReceiver && CreditState.HasCredit)
               {
                  session.WriteFlow(this);
               }
            }
         }
      }

      private void TrySendLocalDetach(bool closed)
      {
         if (!WasLocalDetachSent)
         {
            if (session.IsLocallyOpen && session.WasLocalBeginSent &&
                connection.IsLocallyOpen && connection.WasLocalOpenSent && !engine.IsShutdown)
            {

               Detach detach = new()
               {
                  Handle = localAttach.Handle,
                  Closed = closed,
                  Error = ErrorCondition
               };

               engine.FireWrite(detach, session.LocalChannel);
               session.FreeLink(this);
               localDetachSent = true;
            }
         }
      }

      #endregion

      #region IProtonLink internal API explicit implementations

      uint IProtonLink.Handle => localAttach.Handle;

      string IProtonLink.Name => localAttach.Name;

      bool IProtonLink.IsSender => Role.IsSender();

      bool IProtonLink.IsReceiver => Role.IsReceiver();

      Role IProtonLink.Role => Role;

      LinkState IProtonLink.LocalState => localState;

      LinkState IProtonLink.RemoteState => remoteState;

      IProtonLink IProtonLink.TrySyncLocalStateWithRemote()
      {
         TrySyncLocalStateWithRemote();
         return this;
      }

      IProtonLink IProtonLink.DecorateOutgoingFlow(in Flow flow)
      {
         HandleDecorateOfOutgoingFlow(flow);
         return this;
      }

      IProtonLink IProtonLink.RemoteAttach(in Attach attach)
      {
         LOG.Trace("Link:{0} Received remote Attach:{1}", this, attach);

         remoteAttach = attach;
         remoteState = LinkState.Active;
         HandleRemoteAttach(attach);
         TransitionToRemotelyOpenedState();
         FireRemoteOpen();

         return this;
      }

      IProtonLink IProtonLink.RemoteDetach(in Detach detach)
      {
         LOG.Trace("Link:{0} Received remote Detach:{1}", this, detach);
         RemoteErrorCondition = detach.Error;
         if (IsSender)
         {
            CreditState.ClearCredit();
         }

         HandleRemoteDetach(detach);

         if (detach.Closed)
         {
            remoteState = LinkState.Closed;
            operability = LinkOperabilityState.LinkRemotelyClosed;
            TransitionToRemotelyClosed();
            FireRemoteClose();
         }
         else
         {
            remoteState = LinkState.Detached;
            operability = LinkOperabilityState.LinkRemotelyDetached;
            TransitionToRemotelyDetached();
            FireRemoteDetach();
         }

         return this;
      }

      IProtonLink IProtonLink.RemoteFlow(in Flow flow)
      {
         LOG.Trace("Link:{0} Received new Flow:{1}", this, flow);
         HandleRemoteFlow(flow);
         return this;
      }

      IProtonLink IProtonLink.RemoteTransfer(in Transfer transfer, in IProtonBuffer payload, out ProtonIncomingDelivery delivery)
      {
         LOG.Trace("Link:{0} Received new Transfer:{1}", this, transfer);
         HandleRemoteTransfer(transfer, payload, out delivery);
         return this;
      }

      IProtonLink IProtonLink.RemoteDisposition(in Disposition disposition, in ProtonIncomingDelivery delivery)
      {
         LOG.Trace("Link:{0} Received remote disposition:{1} for sent delivery:{2}", this, disposition, delivery);
         HandleRemoteDisposition(disposition, delivery);
         return this;
      }

      IProtonLink IProtonLink.RemoteDisposition(in Disposition disposition, in ProtonOutgoingDelivery delivery)
      {
         LOG.Trace("Link:{0} Received remote disposition:{1} for received delivery:{2}", this, disposition, delivery);
         HandleRemoteDisposition(disposition, delivery);
         return this;
      }

      IProtonLink IProtonLink.HandleConnectionRemotelyClosed(in ProtonConnection connection)
      {
         if (IsSender)
         {
            CreditState.ClearCredit();
         }

         if (operability < LinkOperabilityState.ConnectionRemotelyClosed)
         {
            operability = LinkOperabilityState.ConnectionRemotelyClosed;
            TransitionToParentRemotelyClosed();
         }

         return this;
      }

      IProtonLink IProtonLink.HandleConnectionLocallyClosed(in ProtonConnection connection)
      {
         if (IsSender)
         {
            CreditState.ClearCredit();
         }

         if (operability < LinkOperabilityState.ConnectionLocallyClosed)
         {
            operability = LinkOperabilityState.ConnectionLocallyClosed;
            TransitionToParentLocallyClosed();
            FireParentEndpointClosed();
         }

         return this;
      }

      IProtonLink IProtonLink.HandleSessionLocallyClosed(in ProtonSession protonSession)
      {
         if (IsSender)
         {
            CreditState.ClearCredit();
         }

         if (operability < LinkOperabilityState.SessionLocallyClosed)
         {
            operability = LinkOperabilityState.SessionLocallyClosed;
            TransitionToParentLocallyClosed();
            FireParentEndpointClosed();
         }

         return this;
      }

      IProtonLink IProtonLink.HandleSessionRemotelyClosed(in ProtonSession protonSession)
      {
         if (IsSender)
         {
            CreditState.ClearCredit();
         }

         if (operability < LinkOperabilityState.SessionRemotelyClosed)
         {
            operability = LinkOperabilityState.SessionRemotelyClosed;
            TransitionToParentRemotelyClosed();
         }

         return this;
      }

      IProtonLink IProtonLink.HandleEngineShutdown(in ProtonEngine engine)
      {
         if (IsSender)
         {
            CreditState.ClearCredit();
         }

         if (operability < LinkOperabilityState.EngineShutdown)
         {
            operability = LinkOperabilityState.EngineShutdown;
         }

         try
         {
            FireEngineShutdown();
         }
         catch (Exception)
         {
         }

         return this;
      }

      IProtonLink IProtonLink.HandleSessionCreditStateUpdate(in ProtonSessionOutgoingWindow window)
      {
         HandleSessionCreditStateUpdates(window);
         return this;
      }

      IProtonLink IProtonLink.HandleSessionCreditStateUpdate(in ProtonSessionIncomingWindow window)
      {
         HandleSessionCreditStateUpdates(window);
         return this;
      }

      #endregion
   }
}