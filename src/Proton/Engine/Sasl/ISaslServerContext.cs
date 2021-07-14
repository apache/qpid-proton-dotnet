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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Sasl
{
   /// <summary>
   /// Delegate that performs custom initialization work when the server context
   /// has first been initialized.
   /// </summary>
   /// <param name="context">The SASL server context that is requesting init</param>
   public delegate void SaslServerContextInitHandler(ISaslServerContext context);

   /// <summary>
   /// Delegate that handler a SASL or AMQP Header received from the remote client.
   /// </summary>
   /// <param name="context">The SASL server context that received the header</param>
   /// <param name="header">The AMQP or SASL header received from the client</param>
   public delegate void SaslHeaderHandler(ISaslServerContext context, AmqpHeader header);

   /// <summary>
   /// Delegate that handles the initial mechanism selection made by a remote client
   /// once the server has sent its supported SASL mechanisms.
   /// </summary>
   /// <param name="context">The SASL server context that received the chosen mechanism </param>
   /// <param name="mechanism">The SASL mechanism name that the remote client has chosen</param>
   /// <param name="initResponse">The initial response bytes sent by the client</param>
   public delegate void SaslInitHandler(ISaslServerContext context, Symbol mechanism, IProtonBuffer initResponse);

   /// <summary>
   /// Delegate that handle the client sent challenge response bytes and either completes
   /// the SASL authentication process or produces additional challenges.
   /// </summary>
   /// <param name="context">The SASL server context that received the response</param>
   /// <param name="response">The response bytes that were sent by the client</param>
   public delegate void SaslResponseHandler(ISaslServerContext context, IProtonBuffer response);

   /// <summary>
   /// SASL Server operating context used by an {@link Engine} that has been
   /// configured as a SASL server or that has receiver an AMQP header thereby
   /// forcing it into becoming the server side of the SASL exchange.
   /// </summary>
   public interface ISaslServerContext : ISaslContext
   {
      /// <summary>
      /// Sends the set of supported mechanisms to the SASL client from which it must
      /// choose and return one mechanism which will then be the basis for the SASL
      /// authentication negotiation.
      /// </summary>
      /// <param name="mechanisms">The SASL mechanisms to offer to the client</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext SendMechanisms(Symbol[] mechanisms);

      /// <summary>
      /// Sends the SASL challenge defined by the SASL mechanism that is in use during
      /// this SASL negotiation.  The challenge is an opaque binary that is provided to
      /// the server by the security mechanism.
      /// </summary>
      /// <param name="challenge">The challenge bytes to send to the client</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext SendChallenge(IProtonBuffer challenge);

      /// <summary>
      /// Sends a response to a server side challenge that comprises the challenge / response
      /// exchange for the chosen SASL mechanism.
      /// </summary>
      /// <param name="outcome">The SASL Authentication outcome to send</param>
      /// <param name="additional">Optional additional data to send</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext SendOutcome(SaslOutcome outcome, IProtonBuffer additional);

      /// <summary>
      /// Allows the server implementation to fail the SASL negotiation process due to some
      /// unrecoverable error.  Failing the process will signal the {@link Engine} that the SASL
      /// process has failed and place the engine in a failed state as well as notify the registered
      /// error handler for the {@link Engine}.
      /// </summary>
      /// <param name="failure">The exception that indicates the reason for the server failure</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext SaslFailure(SaslException failure);

      #region SASL Server Events

      /// <summary>
      /// Called to give the application code a clear point to initialize all the server side
      /// expectations for the Authentication exchange.
      /// <para/>
      /// The application should use this event to configure the server mechanisms and other server
      /// authentication properties.
      /// </summary>
      /// <remarks>
      /// In the event that the server implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext InitializationHandler(SaslClientContextInitHandler handler);

      /// <summary>
      /// Called when the SASL header has been received and the server is now ready to send
      /// the configured SASL mechanisms. The handler should respond be calling the send
      /// of server mechanisms method immediately or later using the same thread that invoked
      /// this event handler.
      /// </summary>
      /// <remarks>
      /// In the event that the server implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext HeaderHandler(SaslHeaderHandler handler);

      /// <summary>
      /// Called when a SASL init frame has arrived from the client indicating the chosen SASL
      /// mechanism and the initial response data if any. Based on the chosen mechanism the server
      /// handler should provide additional challenges or complete the SASL negotiation by sending
      /// an outcome to the client. The handler can either respond immediately or it should response
      /// using the same thread that invoked this handler.
      /// </summary>
      /// <remarks>
      /// In the event that the server implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext AuthenticationInitHandler(SaslInitHandler handler);

      /// <summary>
      /// Called when a SASL response frame has arrived from the client. The server should process
      /// the response and either offer additional challenges or complete the SASL negotiations based
      /// on the mechanics of the chosen SASL mechanism. The server handler should either respond
      /// immediately or should respond from the same thread that the response handler was invoked from.
      /// </summary>
      /// <remarks>
      /// In the event that the server implementation cannot proceed with SASL authentication it
      /// should call the SASL failed API  to signal the engine that it should transition to a
      /// failed state.
      /// </remarks>
      /// <param name="handler">The handler for this event.</param>
      /// <returns>This SASL server context instance.</returns>
      ISaslServerContext ResponseHandler(SaslResponseHandler handler);

      #endregion
   }
}