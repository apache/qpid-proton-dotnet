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
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl
{
   /// <summary>
   /// Delegate that performs custom initialization work when the client context
   /// has first been initialized.
   /// </summary>
   /// <param name="context">The SASL client context that is requesting init</param>
   public delegate void SaslClientContextInitHandler(ISaslClientContext context);

   /// <summary>
   /// Delegate that performs the selection of the SASL mechanim the client will
   /// use to perform authentication with the remote SASL server.
   /// </summary>
   /// <param name="context">The SASL client context that received the server mechanisms</param>
   /// <param name="mechanisms">The server offered SASL mechanisms</param>
   public delegate void SaslMechanismsHandler(ISaslClientContext context, Symbol[] mechanisms);

   /// <summary>
   /// Delegate that performs the response handling for any incoming SASL server
   /// challenges.
   /// </summary>
   /// <param name="context">The SASL client context that received the server challenge</param>
   /// <param name="challenge">The server provided challenge bytes</param>
   public delegate void SaslChallengeHandler(ISaslClientContext context, IProtonBuffer challenge);

   /// <summary>
   /// Delegate that reacts to the final SASL server outcome event and triggers
   /// client level state changes based on the outcome.
   /// </summary>
   /// <param name="context">The SASL client context that received the server outcome</param>
   /// <param name="outcome">The SASL outcome that was provided by the server</param>
   /// <param name="additionalData">Optional additional data provided by the server.</param>
   public delegate void SaslOutcomeHandler(ISaslClientContext context, SaslOutcome outcome, IProtonBuffer additionalData);

   /// <summary>
   /// Root context of a SASL authentication API which provides common elements
   /// used in both clients and servers.
   /// </summary>
   public interface ISaslClientContext : ISaslContext
   {
      /// <summary>
      /// Sends the AMQP Header indicating the desire for SASL negotiations to be commenced on
      /// this connection. The hosting application my wish to start SASL negotiations prior to
      /// opening a connection in order to validation authentication state out of band if the
      /// normal open process.
      /// </summary>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext SendSASLHeader();

      /// <summary>
      /// Sends a response to the SASL server indicating the chosen mechanism for this
      /// client and the host name that this client is identifying itself as.
      /// </summary>
      /// <param name="mechanism">The server mechanism that the client has selected</param>
      /// <param name="host">The name of the host</param>
      /// <param name="initialResponse">The binary response to transmit</param>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext SendChosenMechanism(Symbol mechanism, string host, IProtonBuffer initialResponse);

      /// <summary>
      /// Sends a response to a server side challenge that comprises the challenge / response
      /// exchange for the chosen SASL mechanism.
      /// </summary>
      /// <param name="response">The response bytes to transmit</param>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext SendResponse(IProtonBuffer response);

      /// <summary>
      /// Allows the client implementation to fail the SASL negotiation process due to some
      /// unrecoverable error.  Failing the process will signal the engine that the SASL process
      /// has failed and place the engine in a failed state as well as notify the registered error
      /// handler for the protocol engine.
      /// </summary>
      /// <param name="failure">The exception that defines the cause of the failure</param>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext SaslFailure(SaslException failure);

      #region SASL Client Events

      /// <summary>
      /// Called to give the application code a clear point to initialize all the client side
      /// expectations for the Authentication exchange.
      /// <para/>
      /// The application should use this event to configure the client mechanisms and other client
      /// authentication properties.
      /// </summary>
      /// <remarks>
      /// In the event that the client implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext InitializationHandler(SaslClientContextInitHandler handler);

      /// <summary>
      /// Called when a SASL mechanisms frame has arrived and its effect applied, indicating
      /// the offered mechanisms sent by the 'server' peer. The client should respond to the
      /// mechanisms event by selecting one from the offered list and calling the send method
      /// for the chosen value immediately or later using the same thread that triggered this
      /// event.
      /// </summary>
      /// <remarks>
      /// In the event that the client implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext MechanismsHandler(SaslMechanismsHandler handler);

      /// <summary>
      /// Called when a SASL challenge frame has arrived and its effect applied, indicating the
      /// challenge sent by the 'server' peer. The client should respond to the mechanisms event
      /// by selecting one from the offered list and calling the send response method immediately
      /// or later using the same thread that triggered this event.
      /// </summary>
      /// <remarks>
      /// In the event that the client implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext ChallengeHandler(SaslChallengeHandler handler);

      /// <summary>
      /// Called when a SASL outcome frame has arrived and its effect applied, indicating the outcome and
      /// any success additional data sent by the 'server' peer. The client can consider the SASL negotiations
      /// complete following this event. The client should respond appropriately to the outcome whose state
      /// can indicate that negotiations have failed and the server has not authenticated the client.
      /// </summary>
      /// <remarks>
      /// In the event that the client implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL client context instance.</returns>
      ISaslClientContext OutcomeHandler(SaslOutcomeHandler handler);

      #endregion

   }
}