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
   /// SASL Server operating context used by an {@link Engine} that has been
   /// configured as a SASL server or that has receiver an AMQP header thereby
   /// forcing it into becoming the server side of the SASL exchange.
   /// </summary>
   public interface ISaslServerContext : ISaslContext
   {
      /// <summary>
      /// Configures the SASL authenticator which will be used to drive the SASL
      /// authentication process on the server side.
      /// </summary>
      ISaslServerAuthenticator Authenticator { get; set; }

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

   }
}