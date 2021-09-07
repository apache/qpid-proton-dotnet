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

using Apache.Qpid.Proton.Test.Driver.Codec.Messaging;
using Apache.Qpid.Proton.Test.Driver.Codec.Transactions;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Tracks information about links that are opened be the client under test.
   /// </summary>
   public abstract class LinkTracker
   {
      private readonly SessionTracker session;

      private Attach remoteAttach;
      private Detach remoteDetach;

      private Attach localAttach;
      private Detach localDetach;

      public LinkTracker(SessionTracker session)
      {
         this.session = session;
      }

      public SessionTracker Session => session;

      public string Name => remoteAttach?.Name ?? localAttach.Name;

      public Role Role => IsSender ? Role.Sender : Role.Receiver;

      public SenderSettleMode SenderSettleMode => localAttach?.SenderSettleMode ?? SenderSettleMode.Mixed;

      public ReceiverSettleMode ReceiverSettleMode => localAttach?.ReceiverSettleMode ?? ReceiverSettleMode.Second;

      public SenderSettleMode RemoteSenderSettleMode => remoteAttach?.SenderSettleMode ?? SenderSettleMode.Mixed;

      public ReceiverSettleMode RemoteReceiverSettleMode => remoteAttach?.ReceiverSettleMode ?? ReceiverSettleMode.Second;

      public uint? Handle => localAttach?.Handle;

      public uint? RemoteHandle => remoteAttach?.Handle;

      public Source Source => localAttach?.Source;

      public Target Target => (Target)(localAttach?.Target is Target ? localAttach.Target : null);

      public Coordinator Coordinator => (Coordinator)(localAttach?.Target is Coordinator ? localAttach.Target : null);

      public Source RemoteSource => remoteAttach?.Source;

      public Target RemoteTarget => (Target)(remoteAttach?.Target is Target ? remoteAttach.Target : null);

      public Coordinator RemoteCoordinator => (Coordinator)(remoteAttach?.Target is Coordinator ? remoteAttach.Target : null);

      public bool IsRemotelyAttached => remoteAttach != null;

      public bool IsRemotelyDetached => remoteDetach != null;

      public bool IsLocallyAttached => localAttach != null;

      public bool IsLocallyDetached => localDetach != null;

      #region LinkTracker event handlers for AMQP performatives

      internal LinkTracker HandleLocalAttach(Attach localAttach)
      {
         this.localAttach = localAttach;

         return this;
      }

      public LinkTracker HandleLocalDetach(Detach localDetach)
      {
         this.localDetach = localDetach;

         return this;
      }

      public void HandlerRemoteAttach(Attach remoteAttach)
      {
         this.remoteAttach = remoteAttach;
      }

      public LinkTracker HandleRemoteDetach(Detach remoteDetach)
      {
         this.remoteDetach = remoteDetach;

         return this;
      }

      #endregion

      #region LinkTracker abstract API

      internal abstract void HandleTransfer(Transfer transfer, byte[] payload);

      internal abstract void HandleFlow(Flow flow);

      public abstract bool IsSender { get; }

      public abstract bool IsReceiver { get; }

      #endregion
   }
}