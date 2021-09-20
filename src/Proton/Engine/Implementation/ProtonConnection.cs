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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;
using Microsoft.Extensions.Caching.Memory;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Implements the mechanics of managing a single AMQP connection associated
   /// with the provided engine instance.
   /// </summary>
   public sealed class ProtonConnection : ProtonEndpoint<IConnection>, IConnection, IHeaderHandler<ProtonEngine>, IPerformativeHandler<ProtonEngine>
   {
      private readonly Open localOpen = new Open();
      private Open remoteOpen;
      private AmqpHeader remoteHeader;

      private IDictionary<ushort, ProtonSession> localSessions = new Dictionary<ushort, ProtonSession>();
      private IDictionary<ushort, ProtonSession> remoteSessions = new Dictionary<ushort, ProtonSession>();

      // These would be sessions that were begun and ended before the remote ever
      // responded with a matching being and end.  The remote is required to complete
      // these before answering a new begin sequence on the same local channel.
      private MemoryCache zombieSessions = new MemoryCache(new MemoryCacheOptions());

      private ConnectionState localState = ConnectionState.Idle;
      private ConnectionState remoteState = ConnectionState.Idle;

      private bool headerSent;
      private bool localOpenSent;
      private bool localCloseSent;

      private Action<AmqpHeader> remoteHeaderHandler;
      private Action<ISession> remoteSessionOpenEventHandler;
      private Action<ISender> remoteSenderOpenEventHandler;
      private Action<IReceiver> remoteReceiverOpenEventHandler;
      private Action<ITransactionManager> remoteTxnManagerOpenEventHandler;

      public ProtonConnection(ProtonEngine engine) : base(engine)
      {
         // This configures the default for the client which could later be made configurable
         // by adding an option in EngineConfiguration but for now this is forced set here.
         this.localOpen.MaxFrameSize = ProtonConstants.DefaultMaxAmqpFrameSize;
      }

      public override IConnection Open()
      {
         if (ConnectionState == ConnectionState.Idle)
         {
            engine.CheckShutdownOrFailed("Cannot open a connection when Engine is shutdown or failed.");
            localState = ConnectionState.Active;
            try
            {
               SyncLocalStateWithRemote();
            }
            finally
            {
               FireLocalOpen();
            }
         }

         return this;
      }

      public override IConnection Close()
      {
         if (ConnectionState == ConnectionState.Active)
         {
            localState = ConnectionState.Closed;
            try
            {
               engine.CheckFailed("Connection close called while engine .");
               SyncLocalStateWithRemote();
            }
            finally
            {
               foreach (ProtonSession session in AllSessions())
               {
                  session.HandleConnectionLocallyClosed(this);
               }
               FireLocalClose();
            }
         }

         return this;
      }

      public IConnection Negotiate()
      {
         return Negotiate((header) =>
         {
            // TODO : LOG.trace("Negotiation completed with remote returning AMQP Header: {}", header);
         });
      }

      public IConnection Negotiate(in Action<AmqpHeader> remoteAMQPHeaderHandler)
      {
         if (remoteAMQPHeaderHandler == null)
         {
            throw new ArgumentNullException("Provided AMQP Header received handler cannot be null");
         }

         CheckConnectionClosed("Cannot start header negotiation on a closed connection");

         if (remoteHeader != null)
         {
            remoteAMQPHeaderHandler.Invoke(remoteHeader);
         }
         else
         {
            remoteHeaderHandler = remoteAMQPHeaderHandler;
         }

         SyncLocalStateWithRemote();

         return this;
      }

      public long Tick(long current)
      {
         CheckConnectionClosed("Cannot call tick on an already closed Connection");
         return engine.Tick(current);
      }

      public IConnection TickAuto(in TaskFactory taskFactory)
      {
         CheckConnectionClosed("Cannot call tickAuto on an already closed Connection");
         engine.TickAuto(taskFactory);
         return this;
      }

      public ISession Session()
      {
         CheckConnectionClosed("Cannot create a Session from a Connection that is already closed");

         ushort localChannel = FindFreeLocalChannel();
         ProtonSession newSession = new ProtonSession(this, localChannel);
         localSessions.Add(localChannel, newSession);

         return newSession;
      }

      public ConnectionState ConnectionState => localState;

      public ConnectionState RemoteConnectionState => remoteState;

      public override bool IsLocallyOpen => ConnectionState == ConnectionState.Active;

      public override bool IsLocallyClosed => ConnectionState == ConnectionState.Closed;

      public override bool IsRemotelyOpen => RemoteConnectionState == ConnectionState.Active;

      public override bool IsRemotelyClosed => RemoteConnectionState == ConnectionState.Closed;

      public string ContainerId
      {
         get => localOpen.ContainerId;
         set
         {
            CheckNotOpened("Cannot set Container Id on already opened Connection");
            localOpen.ContainerId = value;
         }
      }

      public string RemoteContainerId => remoteOpen?.ContainerId;

      public string Hostname
      {
         get => localOpen.Hostname;
         set
         {
            CheckNotOpened("Cannot set Host name on already opened Connection");
            localOpen.Hostname = value;
         }
      }

      public string RemoteHostname => remoteOpen?.Hostname;

      public ushort ChannelMax
      {
         get => localOpen.ChannelMax;
         set
         {
            CheckNotOpened("Cannot set Channel Max on already opened Connection");
            localOpen.ChannelMax = value;
         }
      }

      public ushort RemoteChannelMax => remoteOpen?.ChannelMax ?? 0;

      public uint MaxFrameSize
      {
         get => localOpen.MaxFrameSize;
         set
         {
            CheckNotOpened("Cannot set Max Frame Size on already opened Connection");

            // We are specifically limiting max frame size to 2GB here as our buffers implementations
            // cannot handle anything larger so we must protect them from larger frames.
            if (value > int.MaxValue)
            {
               throw new ArgumentOutOfRangeException(string.Format(
                   "Given max frame size value {0} larger than this implementations limit of {1}",
                   value, int.MaxValue));
            }

            localOpen.MaxFrameSize = value;
         }
      }

      public uint RemoteMaxFrameSize => remoteOpen?.MaxFrameSize ?? ProtonConstants.MinMaxAmqpFrameSize;

      public uint IdleTimeout
      {
         get => localOpen.IdleTimeout;
         set
         {
            CheckNotOpened("Cannot set Idle Timeout on already opened Connection");
            localOpen.IdleTimeout = value;
         }
      }

      public uint RemoteIdleTimeout => remoteOpen?.IdleTimeout ?? 0;

      public override Symbol[] OfferedCapabilities
      {
         get => (Symbol[])(localOpen.OfferedCapabilities?.Clone());
         set
         {
            CheckNotOpened("Cannot set offered capabilities on already opened Connection");
            localOpen.OfferedCapabilities = (Symbol[])(value?.Clone());
         }
      }

      public override Symbol[] RemoteOfferedCapabilities => (Symbol[])(remoteOpen?.OfferedCapabilities?.Clone());

      public override Symbol[] DesiredCapabilities
      {
         get => (Symbol[])(localOpen.DesiredCapabilities?.Clone());
         set
         {
            CheckNotOpened("Cannot set desired capabilities on already opened Connection");
            localOpen.DesiredCapabilities = (Symbol[])(value?.Clone());
         }
      }

      public override Symbol[] RemoteDesiredCapabilities => (Symbol[])(remoteOpen?.DesiredCapabilities?.Clone());

      public override IReadOnlyDictionary<Symbol, object> Properties
      {
         get
         {
            if (localOpen.Properties != null)
            {
               return new Dictionary<Symbol, object>(localOpen.Properties);
            }
            else
            {
               return null;
            }
         }
         set
         {
            CheckNotOpened("Cannot set Properties on already opened Connection");
            if (value != null && value.Count > 0)
            {
               localOpen.Properties = new Dictionary<Symbol, object>(value);
            }
         }
      }

      public override IReadOnlyDictionary<Symbol, object> RemoteProperties
      {
         get
         {
            if (remoteOpen != null && remoteOpen.Properties != null)
            {
               return new Dictionary<Symbol, object>(remoteOpen.Properties);
            }
            else
            {
               return null;
            }
         }
      }

      public ICollection<ISession> Sessions
      {
         get
         {
            ISet<ISession> result;

            if (localSessions.Count == 0 && remoteSessions.Count == 0)
            {
               result = new HashSet<ISession>();
            }
            else
            {
               result = new HashSet<ISession>(localSessions.Values);
               foreach (ProtonSession session in remoteSessions.Values)
               {
                  result.Add(session);
               }
            }

            return result;
         }
      }

      public IConnection SessionOpenedHandler(Action<ISession> handler)
      {
         this.remoteSessionOpenEventHandler = handler;
         return this;
      }

      public IConnection ReceiverOpenedHandler(Action<IReceiver> handler)
      {
         this.remoteReceiverOpenEventHandler = handler;
         return this;
      }

      public IConnection SenderOpenedHandler(Action<ISender> handler)
      {
         this.remoteSenderOpenEventHandler = handler;
         return this;
      }

      public IConnection TransactionManagerOpenedHandler(Action<ITransactionManager> handler)
      {
         this.remoteTxnManagerOpenEventHandler = handler;
         return this;
      }

      #region Event Handlers for AMQP Performatives

      public void HandleAMQPHeader(AmqpHeader header, ProtonEngine context)
      {
         remoteHeader = header;

         if (remoteHeaderHandler != null)
         {
            remoteHeaderHandler.Invoke(remoteHeader);
            remoteHeaderHandler = null;
         }

         SyncLocalStateWithRemote();
      }

      public void HandleSASLHeader(AmqpHeader header, ProtonEngine context)
      {
         context.EngineFailed(new ProtocolViolationException("Received unexpected SASL Header"));
      }

      public void HandleOpen(Open open, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         if (remoteOpen != null)
         {
            context.EngineFailed(new ProtocolViolationException("Received second Open for Connection from remote"));
            return;
         }

         remoteState = ConnectionState.Active;
         remoteOpen = open;

         FireRemoteOpen();
      }

      public void HandleClose(Close close, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         remoteState = ConnectionState.Closed;
         RemoteCondition = close.Error?.Copy();

         foreach (ProtonSession session in AllSessions())
         {
            session.HandleConnectionRemotelyClosed(this);
         }

         FireRemoteClose();
      }

      public void HandleBegin(Begin begin, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         ProtonSession session = null;

         if (channel > localOpen.ChannelMax)
         {
            ErrorCondition = new ErrorCondition(ConnectionError.FRAMING_ERROR, "Channel Max Exceeded for session Begin");
            Close();
         }
         else if (remoteSessions.ContainsKey(channel))
         {
            context.EngineFailed(new ProtocolViolationException("Received second begin for Session from remote"));
         }
         else
         {
            // If there is a remote channel then this is an answer to a local open of a session, otherwise
            // the remote is requesting a new session and we need to create one and signal that a remote
            // session was opened.
            if (begin.HasRemoteChannel())
            {
               ushort localSessionChannel = begin.RemoteChannel;
               if (!localSessions.TryGetValue(localSessionChannel, out session))
               {
                  // If there is a session that was begun and ended before remote responded we
                  // expect that this exchange refers to that session and proceed as though the
                  // remote is going to begin and end it now (as it should).  The alternative is
                  // that the remote is doing something not compliant with the specification and
                  // we fail the engine to indicate this.
                  if (zombieSessions.TryGetValue(localSessionChannel, out session))
                  {
                     if (session != null)
                     {
                        // The session will now get tracked as a remote session and the next
                        // end will take care of normal remote session cleanup.
                        zombieSessions.Remove(localSessionChannel);
                     }
                     else
                     {
                        // The session was reclaimed by GC and we retain the fact that it was
                        // here so that the end that should be following doesn't result in an
                        // engine failure.
                        return;
                     }
                  }
                  else
                  {
                     ErrorCondition = new ErrorCondition(AmqpError.PRECONDITION_FAILED, "No matching session found for remote channel given");
                     Close();
                     engine.EngineFailed(new ProtocolViolationException("Received uncorrelated channel on Begin from remote: " + localSessionChannel));
                     return;
                  }
               }
            }
            else
            {
               session = (ProtonSession)Session();
            }

            remoteSessions.Add(channel, session);

            // Let the session handle the remote Begin now.
            session.RemoteBegin(begin, channel);

            // If the session was initiated remotely then we signal the creation to the any registered
            // remote session event handler
            if (session.State == SessionState.Idle)
            {
               remoteSessionOpenEventHandler?.Invoke(session);
            }
         }
      }

      public void HandleEnd(End end, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         ProtonSession session;

         if (remoteSessions.TryGetValue(channel, out session))
         {
            remoteSessions.Remove(channel);
            session.RemoteEnd(end, channel);
         }
         else
         {
            // Check that we don't have a lingering session that was opened and closed locally for
            // which the remote is finally getting round to ending but we lost the session instance
            // due to it being cleaned up by GC,
            if (zombieSessions.TryGetValue(channel, out session))
            {
               engine.EngineFailed(new ProtocolViolationException("Received uncorrelated channel on End from remote: " + channel));
            }
            else
            {
               zombieSessions.Remove(channel);
            }
         }
      }

      public void HandleAttach(Attach attach, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         ProtonSession session;

         if (remoteSessions.TryGetValue(channel, out session))
         {
            session.RemoteAttach(attach, channel);
         }
         else
         {
            engine.EngineFailed(new ProtocolViolationException("Received uncorrelated channel on Attach from remote: " + channel));
         }
      }

      public void HandleDetach(Detach detach, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         ProtonSession session;

         if (remoteSessions.TryGetValue(channel, out session))
         {
            session.RemoteDetach(detach, channel);
         }
         else
         {
            engine.EngineFailed(new ProtocolViolationException("Received uncorrelated channel on Detach from remote: " + channel));
         }
      }

      public void HandleFlow(Flow flow, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         ProtonSession session;

         if (remoteSessions.TryGetValue(channel, out session))
         {
            session.RemoteFlow(flow, channel);
         }
         else
         {
            engine.EngineFailed(new ProtocolViolationException("Received uncorrelated channel on Flow from remote: " + channel));
         }
      }

      public void HandleTransfer(Transfer transfer, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         ProtonSession session;

         if (remoteSessions.TryGetValue(channel, out session))
         {
            session.RemoteTransfer(transfer, payload, channel);
         }
         else
         {
            engine.EngineFailed(new ProtocolViolationException("Received uncorrelated channel on Transfer from remote: " + channel));
         }
      }

      public void HandleDisposition(Disposition disposition, IProtonBuffer payload, ushort channel, ProtonEngine context)
      {
         ProtonSession session;

         if (remoteSessions.TryGetValue(channel, out session))
         {
            session.RemoteDisposition(disposition, channel);
         }
         else
         {
            engine.EngineFailed(new ProtocolViolationException("Received uncorrelated channel on Disposition from remote: " + channel));
         }
      }

      #endregion

      #region Internal and Private Connection APIs

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

      internal void HandleEngineStarted(ProtonEngine protonEngine)
      {
         SyncLocalStateWithRemote();
      }

      internal void HandleEngineShutdown(ProtonEngine protonEngine)
      {
         try
         {
            FireEngineShutdown();
         }
         catch (Exception) { }

         foreach (ProtonSession session in AllSessions())
         {
            session.HandleEngineShutdown(protonEngine);
         }
      }

      internal void HandleEngineFailed(ProtonEngine protonEngine, Exception cause)
      {
         if (localOpenSent && !localCloseSent)
         {
            localCloseSent = true;

            try
            {
               if (ErrorCondition == null)
               {
                  ErrorCondition = ErrorConditionFromFailureCause(cause);
               }

               Close forcedClose = new Close();
               forcedClose.Error = ErrorCondition;

               engine.FireWrite(forcedClose, 0);
            }
            catch (Exception) { }
         }
      }

      internal override IConnection Self()
      {
         return this;
      }

      private void CheckNotOpened(String errorMessage)
      {
         if (localState > ConnectionState.Idle)
         {
            throw new InvalidOperationException(errorMessage);
         }
      }

      private void CheckConnectionClosed(String errorMessage)
      {
         if (IsLocallyClosed || IsRemotelyClosed)
         {
            throw new InvalidOperationException(errorMessage);
         }
      }

      private ISet<ProtonSession> AllSessions()
      {
         ISet<ProtonSession> result;

         if (localSessions.Count == 0 && remoteSessions.Count == 0)
         {
            result = new HashSet<ProtonSession>();
         }
         else
         {
            result = new HashSet<ProtonSession>(localSessions.Values);
            foreach (ProtonSession session in remoteSessions.Values)
            {
               result.Add(session);
            }
         }

         return result;
      }

      private void SyncLocalStateWithRemote()
      {
         if (engine.IsWritable)
         {
            // When the engine state changes or we have read an incoming AMQP header etc we need to check
            // if we have pending work to send and do so
            if (headerSent)
            {
               ConnectionState state = ConnectionState;

               // Once an incoming header arrives we can emit our open if locally opened and also send close if
               // that is what our state is already.
               if (state != ConnectionState.Idle && remoteHeader != null)
               {
                  bool resourceSyncNeeded = false;

                  if (!localOpenSent && !engine.IsShutdown)
                  {
                     engine.FireWrite(localOpen, 0);
                     engine.RecomputeEffectiveFrameSizeLimits();
                     localOpenSent = true;
                     resourceSyncNeeded = true;
                  }

                  if (IsLocallyClosed && !localCloseSent && !engine.IsShutdown)
                  {
                     Close localClose = new Close();
                     localClose.Error = ErrorCondition;
                     engine.FireWrite(localClose, 0);
                     localCloseSent = true;
                     resourceSyncNeeded = false;  // Session resources can't write anything now
                  }

                  if (resourceSyncNeeded)
                  {
                     foreach (ProtonSession session in AllSessions())
                     {
                        session.TrySyncLocalStateWithRemote();
                     }
                  }
               }
            }
            else if (remoteHeader != null || ConnectionState == ConnectionState.Active || remoteHeaderHandler != null)
            {
               headerSent = true;
               engine.FireWrite(HeaderEnvelope.AMQP_HEADER_ENVELOPE);
            }
         }
      }

      private ErrorCondition ErrorConditionFromFailureCause(Exception cause)
      {
         Symbol condition;
         string description = cause.Message;

         if (cause is ProtocolViolationException error)
         {
            condition = error.ErrorCondition;
         }
         else
         {
            condition = AmqpError.INTERNAL_ERROR;
         }

         return new ErrorCondition(condition, description);
      }

      private ushort FindFreeLocalChannel()
      {
         for (ushort i = 0; i <= localOpen.ChannelMax; ++i)
         {
            object result;

            if (!localSessions.ContainsKey(i) && !zombieSessions.TryGetValue(i, out result))
            {
               return i;
            }
         }

         // We didn't find one that isn't free and also not awaiting remote begin / end
         // so just use an overlap as it should complete in order unless the remote has
         // completely ignored the specification and or gone of the rails.
         for (ushort i = 0; i <= localOpen.ChannelMax; ++i)
         {
            if (!localSessions.ContainsKey(i))
            {
               return i;
            }
         }

         throw new InvalidOperationException("no local channel available for allocation");
      }

      internal void FreeLocalChannel(ushort localChannel)
      {
         ProtonSession session = localSessions[localChannel];

         localSessions.Remove(localChannel);

         if (session.RemoteState == SessionState.Idle)
         {
            // The remote hasn't answered our begin yet so we need to hold onto this information
            // and process the eventual begin that must be provided per specification.
            zombieSessions.CreateEntry(localChannel).Value = session;
         }
      }

      internal bool WasHeaderSent => this.headerSent;

      internal bool WasLocalOpenSent => this.localOpenSent;

      internal bool WasLocalCloseSent => this.localCloseSent;

      #endregion
   }
}