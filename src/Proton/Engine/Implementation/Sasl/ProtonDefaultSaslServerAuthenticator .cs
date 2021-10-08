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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation.Sasl
{
   /// <summary>
   /// Proton default SASL server authenticator which fails any incoming
   /// authentication requests. A misconfigured engine will not inadvertently
   /// allow for SASL authentication to occur but instead reject all attempts
   /// with a SASL auth failure.
   /// </summary>
   public sealed class ProtonDefaultSaslServerAuthenticator : ISaslServerAuthenticator
   {
      public static readonly ProtonDefaultSaslServerAuthenticator Instance = new ProtonDefaultSaslServerAuthenticator();

      private static readonly Symbol[] PLAIN = { Symbol.Lookup("PLAIN") };

      public void HandleSaslHeader(ISaslServerContext context, AmqpHeader header)
      {
         context.SendMechanisms(PLAIN);
      }

      public void HandleSaslInit(ISaslServerContext context, Symbol mechanism, IProtonBuffer initResponse)
      {
         context.SendOutcome(SaslAuthOutcome.SaslAuthFailed, null);
      }

      public void HandleSaslResponse(ISaslServerContext context, IProtonBuffer response)
      {
         throw new ProtocolViolationException("SASL Response arrived when no challenge was issued or supported.");
      }
   }
}