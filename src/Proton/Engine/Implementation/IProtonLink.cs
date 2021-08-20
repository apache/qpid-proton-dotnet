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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// This is a mark interface that can be used to identify and track vai collections
   /// proton link objects (sender, receiver, coordinator) where the normal objects
   /// cannot be tracked due to the mix of generic types used to differentiate the
   /// actual objects and provide specific interface implementations.
   /// </summary>
   internal interface IProtonLink
   {
      uint Handle { get; }

      string Name { get; }

      bool IsSender { get; }

      bool IsReceiver { get; }

      Role Role { get; }

      LinkState LocalState { get; }

      LinkState RemoteState { get; }

      IProtonLink TrySyncLocalStateWithRemote();

      IProtonLink DecorateOutgoingFlow(in Flow flow);

      #region Handlers for remote incoming AMQP performatives

      IProtonLink RemoteAttach(in Attach attach);

      IProtonLink RemoteDetach(in Detach detach);

      IProtonLink RemoteFlow(in Flow flow);

      IProtonLink RemoteTransfer(in Transfer transfer, in IProtonBuffer payload, out ProtonIncomingDelivery delivery);

      IProtonLink RemoteDisposition(in Disposition disposition, in ProtonIncomingDelivery delivery);

      IProtonLink RemoteDisposition(in Disposition disposition, in ProtonOutgoingDelivery delivery);

      #endregion

      #region Handlers for state changes in parent resources

      IProtonLink HandleConnectionRemotelyClosed(in ProtonConnection connection);

      IProtonLink HandleConnectionLocallyClosed(in ProtonConnection connection);

      IProtonLink HandleSessionLocallyClosed(in ProtonSession protonSession);

      IProtonLink HandleSessionRemotelyClosed(in ProtonSession protonSession);

      IProtonLink HandleEngineShutdown(in ProtonEngine engine);

      IProtonLink HandleSessionCreditStateUpdate(in ProtonSessionOutgoingWindow window);

      IProtonLink HandleSessionCreditStateUpdate(in ProtonSessionIncomingWindow window);

      #endregion
   }
}
