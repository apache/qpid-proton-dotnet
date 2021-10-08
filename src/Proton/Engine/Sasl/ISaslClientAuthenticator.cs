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
   /// Listener for SASL frame arrival to facilitate relevant handling for the SASL
   /// authentication of the client side of the SASL exchange.
   /// <see href="http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-security-v1.0-os.html#doc-idp51040">
   /// See the AMQP specification SASL negotiation process overview for related detail</see>
   /// </summary>
   public interface ISaslClientAuthenticator
   {
      /// <summary>
      /// Called before SASL authentication begins to give the application code a clear point to
      /// initialize all the client side expectations.
      /// </summary>
      /// <remarks>
      /// The application should use this event to configure the client mechanisms and other client
      /// authentication properties. In the event that the client cannot perform the negotiation
      /// due to some configuration issue it should call the failure method of the sasl client
      /// context provided.
      /// </remarks>
      /// <param name="context">The client context handling the SASL exchange</param>
      void Initialize(ISaslClientContext context) { }

      /// <summary>
      /// Called when a SASL mechanisms frame has arrived and its effect applied, indicating
      /// the offered mechanisms sent by the 'server' peer.  The client should respond to the
      /// mechanisms event by selecting one from the offered list and use the provided client
      /// context to send the chosen mechanism back to the remote server.  The caller should
      /// ensure that the call to send the chosen mechanism occurs in the same thread as that
      /// of this handler call.
      /// </summary>
      /// <remarks>
      /// In the event that the client cannot perform the negotiation due to some configuration
      /// or other internal issue it should call the failure method of the sasl client context
      /// provided here.
      /// </remarks>
      /// <param name="context">The client context handling the SASL exchange</param>
      /// <param name="mechanisms"></param>
      void HandleSaslMechanisms(ISaslClientContext context, Symbol[] mechanisms);

      /// <summary>
      /// Called when a SASL challenge frame has arrived and its effect applied, indicating the
      /// challenge sent by the 'server' peer. The client should respond to the mechanisms event
      /// by creating a response buffer and sending it using the context API either immediately
      /// or later but using the same thread context as this event arrived in.
      /// </summary>
      /// <remarks>
      /// In the event that the client cannot perform the negotiation due to some configuration
      /// or other internal issue it should call the failure method of the sasl client context
      /// provided here.
      /// </remarks>
      /// <param name="context">The client context handling the SASL exchange</param>
      /// <param name="challenge">The challenge bytes sent from the server</param>
      void HandleSaslChallenge(ISaslClientContext context, IProtonBuffer challenge);

      /// <summary>
      /// Called when a SASL outcome frame has arrived and its effect applied, indicating the outcome
      /// and any success additional data sent by the 'server' peer.  The client can consider the SASL
      /// negotiations complete following this event.  The client should respond appropriately to the
      /// outcome whose state can indicate that negotiations have failed and the server has not
      /// authenticated the client.
      /// </summary>
      /// <remarks>
      /// In the event that the client cannot perform the negotiation due to some configuration
      /// or other internal issue it should call the failure method of the sasl client context
      /// provided here.
      /// </remarks>
      /// <param name="context">The client context handling the SASL exchange</param>
      /// <param name="outcome">The SASL outcome provided by the server peer</param>
      /// <param name="additional">Optional additional data provided by the server</param>
      void HandleSaslOutcome(ISaslClientContext context, SaslAuthOutcome outcome, IProtonBuffer additional);

   }
}