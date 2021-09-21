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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Implements the mechanics of managing a single AMQP session associated
   /// with the provided connection instance.
   /// </summary>
   public sealed class ProtonSession : ProtonEndpoint<ISession>, ISession
   {
      private readonly Begin localBegin = new Begin();
      private Begin remoteBegin;

      private ushort localChannel;

      private readonly ProtonSessionOutgoingWindow outgoingWindow;
      private readonly ProtonSessionIncomingWindow incomingWindow;

      private readonly IDictionary<string, ProtonSender> senderByNameMap = new Dictionary<string, ProtonSender>();
      private readonly IDictionary<string, ProtonReceiver> receiverByNameMap = new Dictionary<string, ProtonReceiver>();

      private readonly SplayedDictionary<uint, IProtonLink> localLinks = new SplayedDictionary<uint, IProtonLink>();
      private readonly SplayedDictionary<uint, IProtonLink> remoteLinks = new SplayedDictionary<uint, IProtonLink>();

      private readonly Flow cachedFlow = new Flow();

      private readonly ProtonConnection connection;

      private SessionState localState = SessionState.Idle;
      private SessionState remoteState = SessionState.Idle;

      private bool localBeginSent;
      private bool localEndSent;

      // No default for these handlers, Connection will process these if not set here.
      private Action<ISender> remoteSenderOpenEventHandler;
      private Action<IReceiver> remoteReceiverOpenEventHandler;
      private Action<ITransactionManager> remoteTxnManagerOpenEventHandler;

      public ProtonSession(ProtonConnection connection, ushort channel) : base(connection.ProtonEngine)
      {
         this.connection = connection;
         this.localChannel = channel;

         this.outgoingWindow = new ProtonSessionOutgoingWindow(this);
         this.incomingWindow = new ProtonSessionIncomingWindow(this);
      }

      public override ProtonSession Open()
      {
         if (localState == SessionState.Idle)
         {
            CheckConnectionClosed();
            engine.CheckShutdownOrFailed("Cannot open a session when Engine is shutdown or failed.");

            localState = SessionState.Active;
            incomingWindow.ConfigureOutbound(localBegin);
            outgoingWindow.ConfigureOutbound(localBegin);
            try
            {
               TrySyncLocalStateWithRemote();
            }
            finally
            {
               FireLocalOpen();
            }
         }

         return this;
      }

      public override ProtonSession Close()
      {
         if (localState == SessionState.Active)
         {
            localState = SessionState.Closed;
            try
            {
               engine.CheckFailed("Session close called but engine is in a failed state.");
               TrySyncLocalStateWithRemote();
            }
            finally
            {
               foreach (IProtonLink link in AllLinks())
               {
                  link.HandleSessionLocallyClosed(this);
               }
               FireLocalClose();
            }
         }

         return this;
      }

      #region Accessors for Session state data

      internal override ISession Self()
      {
         return this;
      }

      public IConnection Connection => connection;

      public ushort LocalChannel => localChannel;

      public ushort RemoteChannel => remoteBegin?.RemoteChannel ?? 0;

      public SessionState State => localState;

      public SessionState RemoteState => remoteState;

      public override bool IsLocallyOpen => localState == SessionState.Active;

      public override bool IsLocallyClosed => localState == SessionState.Closed;

      public override bool IsRemotelyOpen => remoteState == SessionState.Active;

      public override bool IsRemotelyClosed => remoteState == SessionState.Closed;

      public uint IncomingCapacity
      {
         get => incomingWindow.IncomingCapacity;
         set => incomingWindow.IncomingCapacity = value;
      }

      public uint RemainingIncomingCapacity => incomingWindow.RemainingIncomingCapacity;

      public uint OutgoingCapacity
      {
         get => outgoingWindow.OutgoingCapacity;
         set => outgoingWindow.OutgoingCapacity = value;
      }

      public uint RemainingOutgoingCapacity => outgoingWindow.RemainingOutgoingCapacity;

      public uint HandleMax
      {
         get => localBegin.HandleMax;
         set
         {
            CheckNotOpened("Cannot set handle max on already opened Session");
            localBegin.HandleMax = value;
         }
      }

      public uint RemoteHandleMax => remoteBegin?.HandleMax ?? 0;

      public override Symbol[] OfferedCapabilities
      {
         get => (Symbol[])(localBegin.OfferedCapabilities?.Clone());
         set
         {
            CheckNotOpened("Cannot set Offered Capabilities on already opened Link");
            localBegin.OfferedCapabilities = (Symbol[])(value?.Clone());
         }
      }

      public override Symbol[] DesiredCapabilities
      {
         get => (Symbol[])(localBegin.DesiredCapabilities?.Clone());
         set
         {
            CheckNotOpened("Cannot set Desired Capabilities on already opened Link");
            localBegin.DesiredCapabilities = (Symbol[])(value?.Clone());
         }
      }

      public override Symbol[] RemoteOfferedCapabilities
      {
         get => (Symbol[])(remoteBegin?.OfferedCapabilities?.Clone());
      }

      public override Symbol[] RemoteDesiredCapabilities
      {
         get => (Symbol[])(remoteBegin?.DesiredCapabilities?.Clone());
      }

      public override IReadOnlyDictionary<Symbol, object> Properties
      {
         get
         {
            if (localBegin.Properties != null)
            {
               return new ReadOnlyDictionary<Symbol, object>(localBegin.Properties);
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
               localBegin.Properties = new Dictionary<Symbol, object>(value);
            }
            else
            {
               localBegin.Properties = null;
            }
         }
      }

      public override IReadOnlyDictionary<Symbol, object> RemoteProperties
      {
         get
         {
            if (remoteBegin.Properties != null)
            {
               return new ReadOnlyDictionary<Symbol, object>(remoteBegin.Properties);
            }
            else
            {
               return null;
            }
         }
      }

      #endregion

      #region ISession API implementations

      public IEnumerable<IReceiver> Receivers => throw new NotImplementedException();

      public IEnumerable<ISender> Senders => throw new NotImplementedException();

      public ISender Sender(string name)
      {
         CheckSessionClosed("Cannot create new Sender from closed Session");

         ProtonSender sender = null;

         if (!senderByNameMap.TryGetValue(name, out sender))
         {
            sender = new ProtonSender(this, name, new ProtonLinkCreditState());
            senderByNameMap.Add(name, sender);
         }

         return sender;
      }

      public IReceiver Receiver(string name)
      {
         CheckSessionClosed("Cannot create new Receiver from closed Session");

         ProtonReceiver receiver = null;

         if (!receiverByNameMap.TryGetValue(name, out receiver))
         {
            receiver = new ProtonReceiver(this, name, new ProtonLinkCreditState());
            receiverByNameMap.Add(name, receiver);
         }

         return receiver;
      }

      public ITransactionController Coordinator(string name)
      {
         CheckSessionClosed("Cannot create new TransactionController from closed Session");

         ProtonSender sender = null;

         if (!senderByNameMap.TryGetValue(name, out sender))
         {
            sender = new ProtonSender(this, name, new ProtonLinkCreditState());
            senderByNameMap.Add(name, sender);
         }

         return new ProtonTransactionController(sender);
      }

      public ISession ReceiverOpenHandler(Action<IReceiver> handler)
      {
         remoteReceiverOpenEventHandler = handler;
         return this;
      }

      public ISession SenderOpenHandler(Action<ISender> handler)
      {
         remoteSenderOpenEventHandler = handler;
         return this;
      }

      public ISession TransactionManagerOpenedHandler(Action<ITransactionManager> handler)
      {
         remoteTxnManagerOpenEventHandler = handler;
         return this;
      }

      #endregion

      #region Handlers for remote AMQP Performatives

      internal void RemoteBegin(Begin begin, ushort channel)
      {
         remoteBegin = begin;
         localBegin.RemoteChannel = channel;
         remoteState = SessionState.Active;
         incomingWindow.HandleBegin(begin);
         outgoingWindow.HandleBegin(begin);

         if (IsLocallyOpen)
         {
            FireRemoteOpen();
         }
      }

      internal void RemoteEnd(End end, ushort channel)
      {
         foreach (IProtonLink link in AllLinks())
         {
            link.HandleSessionRemotelyClosed(this);
         }

         RemoteErrorCondition = end.Error;
         remoteState = SessionState.Closed;

         FireRemoteClose();
      }

      internal void RemoteAttach(Attach attach, ushort channel)
      {
         if (ValidateHandleMaxCompliance(attach))
         {
            if (remoteLinks.ContainsKey(attach.Handle))
            {
               ErrorCondition = new ErrorCondition(
                  SessionError.HANDLE_IN_USE, "Attach received with handle that is already in use");
               Close();
               return;
            }

            if (!attach.HasInitialDeliveryCount() && attach.Role.IsSender())
            {
               throw new ProtocolViolationException("Sending peer attach had no initial delivery count");
            }

            IProtonLink link = FindMatchingPendingLinkOpen(attach);
            if (link == null)
            {
               link = attach.Role.IsReceiver() ? (IProtonLink)Sender(attach.Name) : (IProtonLink)Receiver(attach.Name);
            }

            remoteLinks.Add(attach.Handle, link);

            link.RemoteAttach(attach);
         }
      }

      internal void RemoteDetach(Detach detach, ushort channel)
      {
         IProtonLink link;
         if (remoteLinks.TryGetValue(detach.Handle, out link))
         {
            remoteLinks.Remove(detach.Handle);
         }
         else
         {
            engine.EngineFailed(new ProtocolViolationException(
                "Received uncorrelated handle on Detach from remote: " + channel));
            return;
         }

         // Ensure that tracked links get cleared at some point as we don't currently have the concept
         // of link free APIs to put this onto the user to manage.
         if (link.LocalState.IsClosedOrDetached())
         {
            if (link.IsReceiver)
            {
               receiverByNameMap.Remove(link.Name);
            }
            else
            {
               senderByNameMap.Remove(link.Name);
            }
         }

         link.RemoteDetach(detach);
      }

      internal void RemoteFlow(Flow flow, ushort channel)
      {
         bool previousSessionWritable = outgoingWindow.IsSendable;

         // Session level flow processing.
         incomingWindow.HandleFlow(flow);
         outgoingWindow.HandleFlow(flow);

         if (flow.HasHandle())
         {
            IProtonLink link;

            if (!remoteLinks.TryGetValue(flow.Handle, out link))
            {
               engine.EngineFailed(new ProtocolViolationException(
                   "Received uncorrelated handle on Flow from remote: " + channel));
               return;
            }

            link.RemoteFlow(flow);
         }
         else
         {
            HandleSessionOnlyFlow(flow, previousSessionWritable);
         }
      }

      internal void RemoteTransfer(Transfer transfer, IProtonBuffer payload, ushort channel)
      {
         IProtonLink link;
         if (!remoteLinks.TryGetValue(transfer.Handle, out link))
         {
            engine.EngineFailed(new ProtocolViolationException(
                "Received uncorrelated handle on Transfer from remote: " + channel));
         }
         else if (!link.RemoteState.IsOpen())
         {
            engine.EngineFailed(new ProtocolViolationException("Received Transfer for detached Receiver: " + link));
         }
         else
         {
            incomingWindow.HandleTransfer(link, transfer, payload);
         }
      }

      internal void RemoteDisposition(Disposition disposition, ushort channel)
      {
         if (disposition.Role.IsReceiver())
         {
            outgoingWindow.HandleDisposition(disposition);
         }
         else
         {
            incomingWindow.HandleDisposition(disposition);
         }
      }

      #endregion

      #region Internal Proton Session APIs

      internal bool HasReceiverOpenEventHandler => remoteReceiverOpenEventHandler != null;

      internal bool HasSenderOpenEventHandler => remoteSenderOpenEventHandler != null;

      internal bool HasTransactionManagerOpenHandler => remoteTxnManagerOpenEventHandler != null;

      internal void FireRemoteReceiverOpened(IReceiver receiver)
      {
         remoteReceiverOpenEventHandler?.Invoke(receiver);
      }

      internal void FireRemoteSenderOpened(ISender sender)
      {
         remoteSenderOpenEventHandler?.Invoke(sender);
      }

      internal void FireRemoteTransactionManagerOpened(ITransactionManager manager)
      {
         remoteTxnManagerOpenEventHandler?.Invoke(manager);
      }

      internal void HandleConnectionLocallyClosed(ProtonConnection protonConnection)
      {
         foreach (IProtonLink link in AllLinks())
         {
            link.HandleConnectionLocallyClosed(connection);
         }
      }

      internal void HandleConnectionRemotelyClosed(ProtonConnection protonConnection)
      {
         foreach (IProtonLink link in AllLinks())
         {
            link.HandleConnectionRemotelyClosed(connection);
         }
      }

      internal void HandleEngineShutdown(ProtonEngine protonEngine)
      {
         try
         {
            FireEngineShutdown();
         }
         catch (Exception)
         {
         }

         foreach (IProtonLink link in AllLinks())
         {
            link.HandleEngineShutdown(protonEngine);
         }
      }

      internal ProtonSessionOutgoingWindow OutgoingWindow => outgoingWindow;

      internal ProtonSessionIncomingWindow IncomingWindow => incomingWindow;

      internal bool WasLocalBeginSent => localBeginSent;

      internal bool WasLocalEndSent => localEndSent;

      internal void FreeLink(IProtonLink linkToFree)
      {
         FreeLocalHandle(linkToFree.Handle);

         if (linkToFree.RemoteState > LinkState.Active)
         {
            if (linkToFree.IsReceiver)
            {
               receiverByNameMap.Remove(linkToFree.Name);
            }
            else
            {
               senderByNameMap.Remove(linkToFree.Name);
            }
         }
      }

      internal uint FindFreeLocalHandle(IProtonLink link)
      {
         for (uint i = 0; i <= localBegin.HandleMax; ++i)
         {
            if (!localLinks.ContainsKey(i))
            {
               localLinks.Add((int)i, link);
               return i;
            }
         }

         throw new InvalidOperationException("no local handle available for allocation");
      }

      internal void WriteFlow(IProtonLink link)
      {
         cachedFlow.Reset();

         // (AmqpSpec:Section 2.7.4) This value must not be set if the remote begin has not been received.
         if (remoteBegin != null)
         {
            cachedFlow.NextIncomingId = IncomingWindow.NextIncomingId;
         }

         cachedFlow.NextOutgoingId = OutgoingWindow.NextOutgoingId;
         cachedFlow.IncomingWindow = IncomingWindow.IncomingWindow;
         cachedFlow.OutgoingWindow = OutgoingWindow.OutgoingWindow;

         if (link != null)
         {
            link.DecorateOutgoingFlow(cachedFlow);
         }

         engine.FireWrite(cachedFlow, localChannel);
      }

      internal void TrySyncLocalStateWithRemote()
      {
         switch (localState)
         {
            case SessionState.Idle:
               return;
            case SessionState.Active:
               CheckIfBeginShouldBeSent();
               break;
            case SessionState.Closed:
               CheckIfBeginShouldBeSent();
               CheckIfEndShouldBeSent();
               break;
         }
      }

      #endregion

      #region Private Proton Session implementation details

      private void CheckNotOpened(string errorMessage)
      {
         if (localState > SessionState.Idle)
         {
            throw new InvalidOperationException(errorMessage);
         }
      }

      private void CheckConnectionClosed()
      {
         if (connection.ConnectionState == ConnectionState.Closed ||
             connection.RemoteConnectionState == ConnectionState.Closed)
         {
            throw new InvalidOperationException("Cannot open a Session from a Connection that is already closed");
         }
      }

      private void CheckSessionClosed(string errorMessage)
      {
         if (IsLocallyClosed || IsRemotelyClosed)
         {
            throw new InvalidOperationException(errorMessage);
         }
      }

      private void CheckIfBeginShouldBeSent()
      {
         if (!WasLocalBeginSent)
         {
            if (connection.IsLocallyOpen && connection.WasLocalOpenSent)
            {
               FireSessionBegin();
            }
         }
      }

      private void CheckIfEndShouldBeSent()
      {
         if (!WasLocalEndSent)
         {
            if (connection.IsLocallyOpen && connection.WasLocalOpenSent && !engine.IsShutdown)
            {
               FireSessionEnd();
            }
         }
      }

      private IProtonLink FindMatchingPendingLinkOpen(Attach remoteAttach)
      {
         foreach (IProtonLink link in senderByNameMap.Values)
         {
            if (link.Name.Equals(remoteAttach.Name) &&
                link.RemoteState == LinkState.Idle &&
                link.Role != remoteAttach.Role)
            {

               return link;
            }
         }

         foreach (IProtonLink link in receiverByNameMap.Values)
         {
            if (link.Name.Equals(remoteAttach.Name) &&
                link.RemoteState == LinkState.Idle &&
                link.Role != remoteAttach.Role)
            {

               return link;
            }
         }

         return null;
      }

      private bool ValidateHandleMaxCompliance(Attach remoteAttach)
      {
         uint remoteHandle = remoteAttach.Handle;
         if (localBegin.HandleMax < remoteHandle)
         {
            // The handle-max value is the highest handle value that can be used on the session. A peer MUST
            // NOT attempt to attach a link using a handle value outside the range that its partner can handle.
            // A peer that receives a handle outside the supported range MUST close the connection with the
            // framing-error error-code.
            ErrorCondition condition = new ErrorCondition(ConnectionError.FRAMING_ERROR, "Session handle-max exceeded");
            connection.ErrorCondition = condition;
            connection.Close();

            return false;
         }

         return true;
      }

      private void FireSessionBegin()
      {
         engine.FireWrite(localBegin, localChannel);
         localBeginSent = true;
         foreach (IProtonLink link in AllLinks())
         {
            link.TrySyncLocalStateWithRemote();
         }
      }

      private void FireSessionEnd()
      {
         End end = new End();
         end.Error = ErrorCondition;

         engine.FireWrite(end, localChannel);
         localEndSent = true;
         connection.FreeLocalChannel(localChannel);
      }

      private void FreeLocalHandle(uint localHandle)
      {
         localLinks.Remove(localHandle);
      }

      private ISet<IProtonLink> AllLinks()
      {
         ISet<IProtonLink> result;

         if (localLinks.Count == 0 && remoteLinks.Count == 0)
         {
            result = new HashSet<IProtonLink>();
         }
         else
         {
            result = new HashSet<IProtonLink>(localLinks.Values);
            foreach (IProtonLink link in remoteLinks.Values)
            {
               result.Add(link);
            }
         }

         return result;
      }

      private void HandleSessionOnlyFlow(Flow flow, bool previousSessionWritable)
      {
         if (previousSessionWritable != outgoingWindow.IsSendable)
         {
            IList<IProtonLink> senders = new List<IProtonLink>(senderByNameMap.Values);

            foreach (IProtonLink sender in senders)
            {
               sender.HandleSessionCreditStateUpdate(outgoingWindow);

               if (previousSessionWritable == outgoingWindow.IsSendable)
               {
                  break;
               }
            }
         }

         if (flow.Echo)
         {
            // Auto respond to session level echo requests as there's not an event point at the
            // moment that would otherwise allow a response.
            WriteFlow(null);
         }
      }

      #endregion
   }
}