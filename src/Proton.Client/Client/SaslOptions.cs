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

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Options that control how the SASL authentication process is performed.
   /// </summary>
   public class SaslOptions : ICloneable
   {
      /// <summary>
      /// Default for SASL use on any connection created using these options.
      /// </summary>
      public static readonly bool DEFAULT_SASL_ENABLED = true;

      private readonly HashSet<string> allowedMechanisms = new HashSet<string>();

      /// <summary>
      /// Creates a default SASL options instance.
      /// </summary>
      public SaslOptions() : base()
      {
      }

      /// <summary>
      /// Create a new SASL options instance whose settings are copied from the instance provided.
      /// </summary>
      /// <param name="other">The connection options instance to copy</param>
      public SaslOptions(SaslOptions other) : base()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Configures if a connection will enable its SASL layer for authentication which by
      /// default all connections will do.  This should only be disabled if the remote peer
      /// is known not to be using a SASL authentication layer.
      /// </summary>
      public bool SaslEnabled { get; set; } = DEFAULT_SASL_ENABLED;

      /// <summary>
      /// Returns a copy of the set of all allowed SASL mechanisms for the connections created
      /// using these options. By default there are no restrictions and all client supported SASL
      /// mechanisms will be considered however in some cases certain mechanisms are not enabled
      /// unless the user manually specifies them at the exclusion of all other mechanisms.
      /// </summary>
      public ISet<string> AllowedMechanisms
      {
         get { return new HashSet<string>(allowedMechanisms); }
      }

      /// <summary>
      /// Adds the given SASL mechanism to the set of those allowed during connection authentication.
      /// </summary>
      /// <param name="mechanism">The mechanism to add to the allowed set</param>
      public void AddAllowedMechanism(string mechanism)
      {
         this.allowedMechanisms.Add(mechanism);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return CopyInto(new SaslOptions());
      }

      internal SaslOptions CopyInto(SaslOptions other)
      {
         other.SaslEnabled = SaslEnabled;
         other.allowedMechanisms.Clear();
         foreach (string allowed in allowedMechanisms)
         {
            other.AddAllowedMechanism(allowed);
         }

         return other;
      }
   }
}