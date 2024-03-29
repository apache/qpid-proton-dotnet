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
using Apache.Qpid.Proton.Client.Implementation;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Options used to configure the source when a terminus is being created.
   /// </summary>
   public class SourceOptions : TerminusOptions, ICloneable
   {
      public static readonly IDeliveryState DefaultReceiverOutcome = new ClientModified(true, false);

      private static readonly DeliveryStateType[] DefaultSupportedOutcomes = new DeliveryStateType[] {
         DeliveryStateType.Accepted, DeliveryStateType.Rejected, DeliveryStateType.Released, DeliveryStateType.Modified
      };

      /// <summary>
      /// Creates a default SourceOptions instance.
      /// </summary>
      public SourceOptions() : base()
      {
      }

      /// <summary>
      /// Creates a copy of the given source options
      /// </summary>
      /// <param name="other">The other source options instance to copy</param>
      public SourceOptions(SourceOptions other) : base()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// The distribution mode to set for the source configuration.
      /// </summary>
      public DistributionMode? DistributionMode { get; set; }

      /// <summary>
      /// The filters that are assigned to the source configuration.
      /// </summary>
      public IDictionary<string, object> Filters { get; set; }

      /// <summary>
      /// The default outcome to assign to the source configuration.
      /// </summary>
      public IDeliveryState DefaultOutcome { get; set; }

      /// <summary>
      /// The supported outcomes that are assigned to the source configuration.
      /// </summary>
      public IEnumerable<DeliveryStateType> Outcomes { get; set; } = DefaultSupportedOutcomes;

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new SourceOptions());
      }

      internal SourceOptions CopyInto(SourceOptions other)
      {
         base.CopyInto(other);

         other.DistributionMode = DistributionMode;
         other.Filters = Filters;
         other.DefaultOutcome = DefaultOutcome;

         if (Outcomes != null)
         {
            other.Outcomes = new List<DeliveryStateType>(Outcomes);
         }

         return this;
      }
   }
}