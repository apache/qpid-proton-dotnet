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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;

namespace Apache.Qpid.Proton.Client.Impl
{
   /// <summary>
   /// Client source wrapper that exposes a readonly view of a proton
   /// Source instance.
   /// </summary>
   internal sealed class ClientRemoteSource : ISource
   {
      private readonly Source remoteSource;

      private IDeliveryState cachedDefaultOutcome;
      private DistributionMode? cachedDistributionMode;
      private IReadOnlyDictionary<string, object> cachedDynamicNodeProperties;
      private IReadOnlyDictionary<string, object> cachedFilters;
      private IReadOnlyCollection<DeliveryStateType> cachedOutcomes;
      private IReadOnlyCollection<string> cachedCapabilities;

      public ClientRemoteSource(Source remoteSource)
      {
         this.remoteSource = remoteSource;
      }

      public string Address => remoteSource.Address;

      public DurabilityMode DurabilityMode
      {
         get
         {
            switch (remoteSource.Durable)
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

      public uint Timeout => remoteSource.Timeout;

      public ExpiryPolicy ExpiryPolicy
      {
         get
         {
            switch (remoteSource.ExpiryPolicy)
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

      public bool Dynamic => remoteSource.Dynamic;

      public DistributionMode DistributionMode
      {
         get
         {
            if (!cachedDistributionMode.HasValue && remoteSource.DistributionMode != null)
            {
               switch (remoteSource.DistributionMode.ToString())
               {
                  case "MOVE":
                     return (DistributionMode)(cachedDistributionMode = Client.DistributionMode.Move);
                  case "COPY":
                     return (DistributionMode)(cachedDistributionMode = Client.DistributionMode.Copy);
                  default:
                     return (DistributionMode)(cachedDistributionMode = Client.DistributionMode.None);
               }
            }

            return (DistributionMode)cachedDistributionMode;
         }
      }

      public IDeliveryState DefaultOutcome
      {
         get
         {
            if (cachedDefaultOutcome == null)
            {
               cachedDefaultOutcome = remoteSource.DefaultOutcome?.ToClientDeliveryState();
            }

            return cachedDefaultOutcome;
         }
      }

      public IReadOnlyDictionary<string, object> Filters
      {
         get
         {
            if (cachedFilters == null)
            {
               cachedFilters = ClientConversionSupport.ToStringKeyedMap(remoteSource.Filter);
            }

            return cachedFilters;
         }
      }

      public IReadOnlyDictionary<string, object> DynamicNodeProperties
      {
         get
         {
            if (cachedDynamicNodeProperties == null)
            {
               cachedDynamicNodeProperties = ClientConversionSupport.ToStringKeyedMap(remoteSource.DynamicNodeProperties);
            }

            return cachedDynamicNodeProperties;
         }
      }

      public IReadOnlyCollection<DeliveryStateType> Outcomes
      {
         get
         {
            if (cachedOutcomes == null && remoteSource.Outcomes != null)
            {
               List<DeliveryStateType> result = new List<DeliveryStateType>(
                  System.Array.ConvertAll(remoteSource.Outcomes, (outcome) => outcome.ToDeliveryStateType()));
               cachedOutcomes = result;
            }

            return cachedOutcomes;
         }
      }

      public IReadOnlyCollection<string> Capabilities
      {
         get
         {
            if (cachedCapabilities == null)
            {
               cachedCapabilities = ClientConversionSupport.ToStringArray(remoteSource.Capabilities);
            }

            return cachedCapabilities;
         }
      }
   }
}