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

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Implements the mechanics of managing a single AMQP session associated
   /// with the provided connection instance.
   /// </summary>
   public sealed class ProtonSession : ProtonEndpoint<ISession>, ISession
   {
      public ProtonSession(ProtonConnection connection, ushort channel) : base(connection.ProtonEngine)
      {
      }

      public override bool IsLocallyOpen => throw new NotImplementedException();

      public override bool IsLocallyClosed => throw new NotImplementedException();

      public override bool IsRemotelyOpen => throw new NotImplementedException();

      public override bool IsRemotelyClosed => throw new NotImplementedException();

      public override Symbol[] OfferedCapabilities { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
      public override Symbol[] DesiredCapabilities { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public override Symbol[] RemoteOfferedCapabilities => throw new NotImplementedException();

      public override Symbol[] RemoteDesiredCapabilities => throw new NotImplementedException();

      public override IReadOnlyDictionary<Symbol, object> Properties { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public override IReadOnlyDictionary<Symbol, object> RemoteProperties => throw new NotImplementedException();

      public IConnection Connection => throw new NotImplementedException();

      public SessionState State => throw new NotImplementedException();

      public SessionState RemoteState => throw new NotImplementedException();

      public IEnumerator<IReceiver> Receivers => throw new NotImplementedException();

      public IEnumerator<ISender> Senders => throw new NotImplementedException();

      public uint IncomingCapacity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public uint RemoteIncomingCapacity => throw new NotImplementedException();

      public uint OutgoingCapacity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public uint RemoteOutgoingCapacity => throw new NotImplementedException();

      public uint HandleMax { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public uint RemoteHandleMax => throw new NotImplementedException();

      public override ISession Close()
      {
         throw new NotImplementedException();
      }

      public ITransactionController Coordinator(string name)
      {
         throw new NotImplementedException();
      }

      public override ISession Open()
      {
         throw new NotImplementedException();
      }

      public IReceiver Receiver(string name)
      {
         throw new NotImplementedException();
      }

      public ISession ReceiverOpenHandler(Action<IReceiver> handler)
      {
         throw new NotImplementedException();
      }

      public ISender Sender(string name)
      {
         throw new NotImplementedException();
      }

      public ISession SenderOpenHandler(Action<ISender> handler)
      {
         throw new NotImplementedException();
      }

      public ISession TransactionManagerOpenedHandler(Action<ITransactionManager> handler)
      {
         throw new NotImplementedException();
      }

      #region Internal Proton Session APIs

      internal override ISession Self()
      {
         return this;
      }

      internal void HandleConnectionLocallyClosed(ProtonConnection protonConnection)
      {
         // TODO allLinks().forEach(link->link.handleConnectionLocallyClosed(connection));
      }

      internal void HandleConnectionRemotelyClosed(ProtonConnection protonConnection)
      {
         // TODO allLinks().forEach(link->link.handleConnectionRemotelyClosed(connection));
      }

      internal void HandleEngineShutdown(ProtonEngine protonEngine)
      {
         throw new NotImplementedException();
      }

      internal void TrySyncLocalStateWithRemote()
      {
         switch (State)
         {
            case SessionState.Idle:
               return;
            case SessionState.Active:
               // TODO CheckIfBeginShouldBeSent();
               break;
            case SessionState.Closed:
               // TODO CheckIfBeginShouldBeSent();
               // TODO CheckIfEndShouldBeSent();
               break;
            default:
               throw new InvalidOperationException("Session is in unknown state and cannot proceed");
         }
      }

      #endregion

      #region Private Proton Session methods

      #endregion
   }
}