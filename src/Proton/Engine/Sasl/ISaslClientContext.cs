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
   /// Root context of a SASL authentication API which provides common elements
   /// used in both clients and servers.
   /// </summary>
   public interface ISaslClientContext : ISaslContext
   {
      /// <summary>
      /// Configures the SASL Authenticator instance that will process the incoming SASL
      /// exchange and provide authentication information and response to challenge
      /// requests from the remote.
      /// </summary>
      ISaslClientAuthenticator Authenticator { get; set; }

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

   }
}