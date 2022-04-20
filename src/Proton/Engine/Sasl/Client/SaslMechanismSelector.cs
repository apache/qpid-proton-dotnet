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
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Client side mechanism used to select a matching mechanism from the server offered list
   /// of mechanisms. The client configures the list of allowed Mechanism names and when the
   /// server mechanisms are offered mechanism is chosen from the allowed set. If the client
   /// does not configure any mechanisms then the selector chooses from all supported SASL
   /// Mechanism types implemented in the Proton library.
   /// </summary>
   public class SaslMechanismSelector
   {
      private readonly ISet<Symbol> allowedMechanisms;

      /// <summary>
      /// Creates a selector instance that chooses from all available SASL mechanisms
      /// </summary>
      public SaslMechanismSelector() : this(new HashSet<Symbol>())
      {
      }

      /// <summary>
      /// Creates a selector instance from the given enumeration of SASL mechanism names.
      /// </summary>
      /// <param name="allowed">An enumeration of SASL mechanism names</param>
      public SaslMechanismSelector(IEnumerable<string> allowed)
      {
         this.allowedMechanisms = allowed != null ? StringUtils.ToSymbolSet(allowed) : new HashSet<Symbol>();
      }

      /// <summary>
      /// Creates a selector instance from the given set of allowed SASL mechanisms names.
      /// </summary>
      /// <param name="allowed">A set of allowed SASL mechanism names</param>
      public SaslMechanismSelector(ISet<Symbol> allowed)
      {
         this.allowedMechanisms = allowed ?? new HashSet<Symbol>();
      }

      /// <summary>
      /// Gets a read-only view of the configured allowed mechanisms.
      /// </summary>
      public IReadOnlyCollection<Symbol> AllowedMechanisms => new HashSet<Symbol>(allowedMechanisms);

      /// <summary>
      /// Given a list of SASL mechanism names select a match from the supported types using the
      /// configured allowed list and the given credentials.
      /// </summary>
      /// <param name="serverMechanisms">The list of mechanisms that the server reports it supports</param>
      /// <param name="credentials">The credentials that have been provided to the SASL layer</param>
      /// <returns></returns>
      public IMechanism Select(Symbol[] serverMechanisms, ISaslCredentialsProvider credentials)
      {
         HashSet<Symbol> candidates = new HashSet<Symbol>(serverMechanisms);

         if (allowedMechanisms.Count > 0)
         {
            candidates.RemoveWhere((candidate) => !allowedMechanisms.Contains(candidate));
         }

         foreach (Symbol match in candidates)
         {
            //LOG.trace("Attempting to match offered mechanism {} with supported and configured mechanisms", match);

            try
            {
               IMechanism mechanism = CreateMechanism(match, credentials);
               if (mechanism == null)
               {
                  //LOG.debug("Skipping {} mechanism as no implementation could be created to support it", match);
                  continue;
               }

               if (!IsApplicable(mechanism, credentials))
               {
                  //LOG.trace("Skipping {} mechanism as it is not applicable", mechanism);
               }
               else
               {
                  return mechanism;
               }
            }
            catch (Exception)
            {
               //LOG.warn("Caught exception while trying to create SASL mechanism {}: {}", match, error.getMessage());
            }
         }

         return null;
      }

      /// <summary>
      /// Using the given Mechanism} name and the provided credentials create and configure a
      /// Mechanism for evaluation by the selector.
      /// </summary>
      /// <param name="name">The name of the SASL mechanism to create</param>
      /// <param name="credentials">The credentials provided to the selector</param>
      /// <returns>A newly created mechanism matching the given name</returns>
      protected IMechanism CreateMechanism(Symbol name, ISaslCredentialsProvider credentials)
      {
         return SaslMechanisms.Lookup(name).CreateMechanism();
      }

      /// <summary>
      /// Tests a given Mechanism instance to determine if it is applicable given the selector
      /// configuration and the provided credentials.
      /// </summary>
      /// <param name="candidate">A candidate mechanism which is being tested</param>
      /// <param name="credentials">The credentials that were provided to the selector</param>
      /// <returns></returns>
      /// <exception cref="ArgumentNullException">If a null candidate is provided</exception>
      protected bool IsApplicable(IMechanism candidate, ISaslCredentialsProvider credentials)
      {
         if (candidate == null)
         {
            throw new ArgumentNullException(nameof(candidate), "Candidate Mechanism to validate must not be null");
         }

         // If a match is found we still may skip it if the credentials given do not match with
         // what is needed in order for it to operate.  We also need to check that when working
         // from a wide open mechanism selection range we check is the Mechanism supports use in
         // a default configuration with no pre-configuration.

         if (!candidate.IsApplicable(credentials))
         {
            //LOG.debug("Skipping {} mechanism because the available credentials are not sufficient", candidate.getName());
            return false;
         }

         if (allowedMechanisms.Count == 0)
         {
            return candidate.IsEnabledByDefault();
         }
         else
         {
            return allowedMechanisms.Contains(candidate.Name);
         }
      }
   }
}