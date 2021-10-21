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
using Apache.Qpid.Proton.Client.Utilities;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Tracks available known capabilities for the connection to allow the client
   /// to know what features are supported on the current connection.
   /// </summary>
   public sealed class ClientConnectionCapabilities
   {
      private bool anonymousRelaySupported;
      private bool delayedDeliverySupported;

      /// <summary>
      /// Returns true if the remote indicated that it supports anonymous relay links.
      /// </summary>
      public bool AnonymousRelaySupported => anonymousRelaySupported;

      /// <summary>
      /// Returns true if the remote indicated that it supports delivery delay annotations from client messages.
      /// </summary>
      public bool DeliveryDelaySupported => delayedDeliverySupported;

      internal ClientConnectionCapabilities DetermineCapabilities(Engine.IConnection connection)
      {
         Symbol[] desired = connection.DesiredCapabilities;
         Symbol[] offered = connection.RemoteOfferedCapabilities;

         ICollection<Symbol> offeredSymbols = offered != null ? new List<Symbol>(offered) : new List<Symbol>();
         ICollection<Symbol> desiredSymbols = desired != null ? new List<Symbol>(desired) : new List<Symbol>();

         anonymousRelaySupported = CheckAnonymousRelaySupported(desiredSymbols, offeredSymbols);
         delayedDeliverySupported = CheckDeliveryRelaySupported(desiredSymbols, offeredSymbols);

         return this;
      }

      private bool CheckAnonymousRelaySupported(ICollection<Symbol> desired, ICollection<Symbol> offered)
      {
         return offered.Contains(ClientConstants.ANONYMOUS_RELAY);
      }

      private bool CheckDeliveryRelaySupported(ICollection<Symbol> desired, ICollection<Symbol> offered)
      {
         return offered.Contains(ClientConstants.DELAYED_DELIVERY);
      }
   }
}