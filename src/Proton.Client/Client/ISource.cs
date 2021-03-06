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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Represents the remote Source instance for a sender or receiver link
   /// </summary>
   public interface ISource
   {
      /// <summary>
      /// The address value of the remote source node.
      /// </summary>
      string Address { get; }

      /// <summary>
      /// The durability mode assigned to the remote source node.
      /// </summary>
      DurabilityMode DurabilityMode { get; }

      /// <summary>
      /// The expiry timeout assigned to the remote source node.
      /// </summary>
      uint Timeout { get; }

      /// <summary>
      /// The expiry policy assigned to the remote source node.
      /// </summary>
      ExpiryPolicy ExpiryPolicy { get; }

      /// <summary>
      /// Indicates if the remote source node was created dynamically.
      /// </summary>
      bool Dynamic { get; }

      /// <summary>
      /// The node properties assigned to a dynamically created source node.
      /// </summary>
      IReadOnlyDictionary<string, object> DynamicNodeProperties { get; }

      /// <summary>
      /// The distribution mode of the remote source node.
      /// </summary>
      DistributionMode DistributionMode { get; }

      /// <summary>
      /// The collection of filters that are configured on the source node.
      /// </summary>
      IReadOnlyDictionary<string, object> Filters { get; }

      /// <summary>
      /// The default outcome that is assigned on the remote source node.
      /// </summary>
      IDeliveryState DefaultOutcome { get; }

      /// <summary>
      /// The set of supported outcomes on the remote source node.
      /// </summary>
      IReadOnlyCollection<DeliveryStateType> Outcomes { get; }

      /// <summary>
      /// The set of capabilities assigned on the remote source node.
      /// </summary>
      IReadOnlyCollection<string> Capabilities { get; }

   }
}