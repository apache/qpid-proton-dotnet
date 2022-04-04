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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Handles SASL traffic from the proton engine and drives the authentication process
   /// for a client connection.
   /// </summary>
   public class SaslAuthenticator : ISaslClientAuthenticator
   {
      private readonly SaslMechanismSelector selector;
      private readonly ISaslCredentialsProvider credentials;

      private Action<SaslAuthOutcome, IProtonBuffer> saslCompleteHandler;
      private IMechanism chosenMechanism;

      /// <summary>
      /// Creates a new SASL Authenticator initialized with the given credentials provider instance.
      /// Because no Mechanism selector is given the full set of supported SASL mechanisms will be
      /// chosen from when attempting to match one to the server offered SASL mechanisms.
      /// </summary>
      /// <param name="credentials">Credentials to use for authentication</param>
      /// <exception cref="ArgumentNullException">If any of the params are null</exception>
      public SaslAuthenticator(ISaslCredentialsProvider credentials) : this(new SaslMechanismSelector(), credentials)
      {
      }

      /// <summary>
      /// Creates a new client SASL Authenticator with the given Mechanism and client credentials
      /// provider instances. The configured Mechanism selector is used when attempting to match
      /// a SASL Mechanism with the server offered set of supported SASL mechanisms.
      /// </summary>
      /// <param name="selector"></param>
      /// <param name="credentials">Credentials to use for authentication</param>
      /// <exception cref="ArgumentNullException">If any of the params are null</exception>
      public SaslAuthenticator(SaslMechanismSelector selector, ISaslCredentialsProvider credentials)
      {
         this.credentials = credentials ?? throw new ArgumentNullException(nameof(credentials),
            "A SASL Credentials provider implementation is required");
         this.selector = selector ?? throw new ArgumentNullException(nameof(selector),
            "A SASL Mechanism selector implementation is required");
      }

      /// <summary>
      /// Sets a completion handler that will be notified once the SASL exchange has completed.
      /// The notification includes the SaslOutcome value which indicates if authentication succeeded
      /// or failed.
      /// </summary>
      /// <param name="saslCompleteEventHandler">Handler for the SASL complete event</param>
      /// <returns>This authenticator instance.</returns>
      public SaslAuthenticator SaslComplete(Action<SaslAuthOutcome> saslCompleteEventHandler)
      {
         this.saslCompleteHandler = (outcome, _) => saslCompleteEventHandler?.Invoke(outcome);
         return this;
      }

      /// <summary>
      /// Sets a completion handler that will be notified once the SASL exchange has completed.
      /// The notification includes the SaslOutcome value which indicates if authentication succeeded
      /// or failed. This handler would also receive a buffer with any additional data sent from
      /// the remote.
      /// </summary>
      /// <param name="saslCompleteEventHandler">Handler for the SASL complete event</param>
      /// <returns>This authenticator instance.</returns>
      public SaslAuthenticator SaslComplete(Action<SaslAuthOutcome, IProtonBuffer> saslCompleteEventHandler)
      {
         this.saslCompleteHandler = saslCompleteEventHandler;
         return this;
      }

      #region SASL Authentication exchange handlers

      public void HandleSaslMechanisms(ISaslClientContext context, Symbol[] mechanisms)
      {
         chosenMechanism = selector.Select(mechanisms, credentials);

         if (chosenMechanism == null)
         {
            context.SaslFailure(new SaslException(
                "Could not find a suitable SASL Mechanism. No supported mechanism, or none usable with " +
                "the available credentials. Server offered: " + StringUtils.ToStringSet(mechanisms)));
            return;
         }

         // TODO LOG.debug("SASL Negotiations proceeding using selected mechanisms: {}", chosenMechanism);

         IProtonBuffer initialResponse = null;
         try
         {
            initialResponse = chosenMechanism.GetInitialResponse(credentials);
         }
         catch (SaslException se)
         {
            context.SaslFailure(se);
         }
         catch (Exception unknown)
         {
            context.SaslFailure(new SaslException("Unknown error while fetching initial response", unknown));
         }

         context.SendChosenMechanism(chosenMechanism.Name, credentials.VHost, initialResponse);
      }

      public void HandleSaslChallenge(ISaslClientContext context, IProtonBuffer challenge)
      {
         IProtonBuffer response = null;
         try
         {
            response = chosenMechanism.GetChallengeResponse(credentials, challenge);
         }
         catch (SaslException se)
         {
            context.SaslFailure(se);
         }
         catch (Exception unknown)
         {
            context.SaslFailure(new SaslException("Unknown error while fetching challenge response", unknown));
         }

         context.SendResponse(response);
      }

      public void HandleSaslOutcome(ISaslClientContext context, SaslAuthOutcome outcome, IProtonBuffer additional)
      {
         try
         {
            chosenMechanism.VerifyCompletion();
            saslCompleteHandler?.Invoke(outcome, additional?.Copy());
         }
         catch (SaslException se)
         {
            context.SaslFailure(se);
         }
         catch (Exception unknown)
         {
            context.SaslFailure(new SaslException("Unknown error while verifying SASL negotiations completion", unknown));
         }
      }

      #endregion
   }
}