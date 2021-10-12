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
   /// Listener for SASL frame arrival to facilitate relevant handling for the SASL
   /// authentication of the server side of the SASL exchange.
   /// <see href="http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-security-v1.0-os.html#doc-idp51040">
   /// See the AMQP specification SASL negotiation process overview for related detail</see>
   /// </summary>
   public interface ISaslServerAuthenticator
   {
      /// <summary>
      /// Called before SASL authentication begins to give the server code a clear point to
      /// initialize all the server side expectations.
      /// </summary>
      /// <remarks>
      /// The server should use this event to configure the server mechanisms and other server
      /// authentication properties. In the event that the server cannot perform the negotiation
      /// due to some configuration issue it should call the failure method of the sasl server
      /// context provided.
      /// </remarks>
      /// <param name="context">The server context handling the SASL exchange</param>
      void Initialize(ISaslServerContext context) { }

      /// <summary>
      /// Called when the SASL header has been received and the server is now ready to send the
      /// configured SASL mechanisms. The handler should respond be calling the mechanisms send
      /// API of the provided server context immediately or later using the same thread that
      /// invoked this event handler.
      /// </summary>
      /// <remarks>
      /// In the event that the server cannot perform the negotiation due to some configuration
      /// or other internal issue it should call the failure method of the sasl server context
      /// provided here.
      /// </remarks>
      /// <param name="context">The server context handling the SASL exchange</param>
      /// <param name="header"></param>
      void HandleSaslHeader(ISaslServerContext context, AmqpHeader header);

      /// <summary>
      /// Called when a SASL init frame has arrived from the client indicating the chosen SASL
      /// mechanism and the initial response data if any. Based on the chosen mechanism the server
      /// handler should provide additional challenges or complete the SASL negotiation by sending
      /// an outcome to the client. The handler can either respond immediately or it should response
      /// using the same thread that invoked this handler.
      /// </summary>
      /// <remarks>
      /// In the event that the server cannot perform the negotiation due to some configuration
      /// or other internal issue it should call the failure method of the sasl server context
      /// provided here.
      /// </remarks>
      /// <param name="context">The server context handling the SASL exchange</param>
      /// <param name="mechanism"></param>
      /// <param name="initResponse"></param>
      void HandleSaslInit(ISaslServerContext context, Symbol mechanism, IProtonBuffer initResponse);

      /// <summary>
      /// Called when a SASL response frame has arrived from the client. The server should process
      /// the response and either offer additional challenges or complete the SASL negotiations based
      /// on the mechanics of the chosen SASL mechanism. The server handler should either respond
      /// immediately or should respond from the same thread that the response handler was invoked from.
      /// </summary>
      /// <remarks>
      /// In the event that the server cannot perform the negotiation due to some configuration
      /// or other internal issue it should call the failure method of the sasl server context
      /// provided here.
      /// </remarks>
      /// <param name="context">The server context handling the SASL exchange</param>
      /// <param name="response"></param>
      void HandleSaslResponse(ISaslServerContext context, IProtonBuffer response);

   }
}