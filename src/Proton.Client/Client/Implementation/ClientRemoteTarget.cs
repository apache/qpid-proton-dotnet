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

using System.Collections.Generic;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Client source wrapper that exposes a readonly view of a proton
   /// Source instance.
   /// </summary>
   internal sealed class ClientRemoteTarget : ITarget
   {
      private readonly Target remoteTarget;

      private IReadOnlyDictionary<string, object> cachedDynamicNodeProperties;
      private IReadOnlyCollection<string> cachedCapabilities;

      public ClientRemoteTarget(Target remoteTarget)
      {
         this.remoteTarget = remoteTarget;
      }

      public string Address => remoteTarget.Address;

      public DurabilityMode DurabilityMode
      {
         get
         {
            switch (remoteTarget.Durable)
            {
               case TerminusDurability.None:
                  return DurabilityMode.None;
               case TerminusDurability.Configuration:
                  return DurabilityMode.Configuration;
               case TerminusDurability.UnsettledState:
                  return DurabilityMode.UnsettledState;
               default:
                  return DurabilityMode.None;
            }
         }
      }

      public uint Timeout => remoteTarget.Timeout;

      public ExpiryPolicy ExpiryPolicy
      {
         get
         {
            switch (remoteTarget.ExpiryPolicy)
            {
               case TerminusExpiryPolicy.Never:
                  return ExpiryPolicy.Never;
               case TerminusExpiryPolicy.LinkDetach:
                  return ExpiryPolicy.LinkClose;
               case TerminusExpiryPolicy.SessionEnd:
                  return ExpiryPolicy.SessionClose;
               case TerminusExpiryPolicy.ConnectionClose:
                  return ExpiryPolicy.ConnectionClose;
               default:
                  return ExpiryPolicy.Never;
            }
         }
      }

      public bool Dynamic => remoteTarget.Dynamic;

      public IReadOnlyDictionary<string, object> DynamicNodeProperties
      {
         get
         {
            if (cachedDynamicNodeProperties == null)
            {
               cachedDynamicNodeProperties = ClientConversionSupport.ToStringKeyedMap(remoteTarget.DynamicNodeProperties);
            }

            return cachedDynamicNodeProperties;
         }
      }

      public IReadOnlyCollection<string> Capabilities
      {
         get
         {
            if (cachedCapabilities == null)
            {
               cachedCapabilities = ClientConversionSupport.ToStringArray(remoteTarget.Capabilities);
            }

            return cachedCapabilities;
         }
      }
   }
}