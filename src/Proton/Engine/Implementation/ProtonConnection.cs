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

      public ConnectionState ConnectionState => localState;

      public override IConnection Open()
      {
         throw new NotImplementedException();
      }

      public override IConnection Close()
      {
         throw new NotImplementedException();
      }

      public IConnection Negotiate()
      {
         throw new NotImplementedException();
      }

      public IConnection Negotiate(in Action<AmqpHeader> remoteAMQPHeaderHandler)
      {
         throw new NotImplementedException();
      }

      public string ContainerId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public string RemoteContainerId => throw new NotImplementedException();

      public string Hostname { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public string RemoteHostname => throw new NotImplementedException();

      public ushort ChannelMax { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public ushort RemoteChannelMax => throw new NotImplementedException();

      public uint MaxFrameSize { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public ushort RemoteMaxFrameSize => throw new NotImplementedException();

      public uint IdleTimeout { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public ushort RemoteIdleTimeout => throw new NotImplementedException();

      public IEnumerator<ISession> Sessions => throw new NotImplementedException();

      public override bool IsLocallyOpen => throw new NotImplementedException();

      public override bool IsLocallyClosed => throw new NotImplementedException();

      public override bool IsRemotelyOpen => throw new NotImplementedException();

      public override bool IsRemotelyClosed => throw new NotImplementedException();

      public override Symbol[] OfferedCapabilities { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public override Symbol[] DesiredCapabilities { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public override Symbol[] RemoteOfferedCapabilities => throw new NotImplementedException();

      public override Symbol[] RemoteDesiredCapabilities => throw new NotImplementedException();

      public override IDictionary<Symbol, object> Properties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public override IDictionary<Symbol, object> RemoteProperties => throw new NotImplementedException();

      public override bool Equals(object obj)
      {
         return base.Equals(obj);
      }

      public override int GetHashCode()
      {
         return base.GetHashCode();
      }

      public IConnection ReceiverOpenedHandler(Action<IReceiver> handler)
      {
         throw new NotImplementedException();
      }

      public IConnection SenderOpenedHandler(Action<ISender> handler)
      {
         throw new NotImplementedException();
      }

      public ISession Session()
      {
         throw new NotImplementedException();
      }

      public IConnection SessionOpenedHandler(Action<ISession> handler)
      {
         throw new NotImplementedException();
      }

      public long Tick(long current)
      {
         throw new NotImplementedException();
      }

      public IConnection TickAuto(in TaskScheduler scheduler)
      {
         throw new NotImplementedException();
      }

      public override string ToString()
      {
         return base.ToString();
      }

      public IConnection TransactionManagerOpenedHandler(Action<ITransactionManager> handler)
      {
         throw new NotImplementedException();
      }

      internal override IConnection Self()
      {
         return this;
      }
   }
}