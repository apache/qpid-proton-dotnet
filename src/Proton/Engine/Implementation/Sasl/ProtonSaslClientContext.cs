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
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Security;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation.Sasl
{
   /// <summary>
   /// SASL Context for SASL client instances which handles the client side
   /// management of the SASL exchange.
   /// </summary>
   public sealed class ProtonSaslClientContext : ProtonSaslContext, ISaslClientContext
   {
      private ISaslClientAuthenticator authenticator = ProtonDefaultSaslClientAuthenticator.Instance;

      private readonly ClientHeaderReadContext clientHeaderReadContext;
      private readonly ClientHeaderWriteContext clientHeaderWriteContext;
      private readonly ClientSaslReadContext clientSaslReadContext;
      private readonly ClientSaslWriteContext clientSaslWriteContext;

      private bool headerWritten;
      private bool headerReceived;
      private bool mechanismsReceived;
      private bool mechanismChosen;
      private bool responseRequired;

      private HeaderEnvelope pausedAMQPHeader;

      public ProtonSaslClientContext(ProtonSaslHandler handler) : base(handler)
      {
         clientHeaderReadContext = new ClientHeaderReadContext(this);
         clientHeaderWriteContext = new ClientHeaderWriteContext(this);
         clientSaslReadContext = new ClientSaslReadContext(this);
         clientSaslWriteContext = new ClientSaslWriteContext(this);
      }

      public ISaslClientAuthenticator Authenticator
      {
         get => authenticator;
         set => authenticator = value;
      }

      public override SaslContextRole Role => SaslContextRole.Client;

      public ISaslClientContext SendSASLHeader()
      {
         saslHandler.Engine.Pipeline.FireWrite(HeaderEnvelope.SASL_HEADER_ENVELOPE);
         return this;
      }

      public ISaslClientContext SendChosenMechanism(Symbol mechanism, string host, IProtonBuffer initialResponse)
      {
         if (mechanism == null)
         {
            throw new ArgumentNullException(nameof(mechanism), "Client must choose a mechanism");
         }

         SaslInit saslInit = new SaslInit(mechanism, initialResponse, host);
         saslHandler.Engine.Pipeline.FireWrite(new SaslEnvelope(saslInit));
         return this;
      }

      public ISaslClientContext SendResponse(IProtonBuffer response)
      {
         if (response == null)
         {
            throw new ArgumentNullException(nameof(response), "Response buffer cannot be null if send response is called.");
         }

         saslHandler.Engine.Pipeline.FireWrite(new SaslEnvelope(new SaslResponse(response)));
         return this;
      }

      public ISaslClientContext SaslFailure(SaslException failure)
      {
         if (!IsDone)
         {
            Done(SaslAuthOutcome.SaslPermError);
            saslHandler.Engine.EngineFailed(failure);
         }

         return this;
      }

      internal override ProtonSaslContext HandleContextInitialization(ProtonEngine engine)
      {
         Authenticator.Initialize(this);
         return this;
      }

      #region Internal API to access read and write contexts

      internal override IHeaderHandler<IEngineHandlerContext> HeaderReadContext => clientHeaderReadContext;

      internal override IHeaderHandler<IEngineHandlerContext> HeaderWriteContext => clientHeaderWriteContext;

      internal override ISaslPerformativeHandler<IEngineHandlerContext> SaslReadContext => clientSaslReadContext;

      internal override ISaslPerformativeHandler<IEngineHandlerContext> SaslWriteContext => clientSaslWriteContext;

      #endregion

      #region Read and Write context classes

      private sealed class ClientHeaderReadContext : IHeaderHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslClientContext client;

         public ClientHeaderReadContext(ProtonSaslClientContext client)
         {
            this.client = client;
         }

         public void HandleAMQPHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            client.state = EngineSaslState.AuthenticationFailed;
            context.FireWrite(HeaderEnvelope.SASL_HEADER_ENVELOPE);
            throw new ProtocolViolationException("Remote does not support SASL authentication.");
         }

         public void HandleSASLHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            if (!client.headerReceived)
            {
               client.headerReceived = true;
               client.state = EngineSaslState.Authenticating;
               if (!client.headerWritten)
               {
                  context.FireWrite(HeaderEnvelope.SASL_HEADER_ENVELOPE);
                  client.headerWritten = true;
               }
            }
            else
            {
               throw new ProtocolViolationException("Remote server sent illegal additional SASL headers.");
            }
         }
      }

      private sealed class ClientHeaderWriteContext : IHeaderHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslClientContext client;

         public ClientHeaderWriteContext(ProtonSaslClientContext client)
         {
            this.client = client;
         }

         public void HandleAMQPHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            // Hold until outcome is known, if success then forward along to start negotiation.
            // Send a SASL header instead so that SASL negotiations can commence with the remote.
            client.pausedAMQPHeader = HeaderEnvelope.AMQP_HEADER_ENVELOPE;
            HandleSASLHeader(AmqpHeader.GetSASLHeader(), context);
         }

         public void HandleSASLHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            if (!client.headerWritten)
            {
               client.headerWritten = true;
               context.FireWrite(HeaderEnvelope.SASL_HEADER_ENVELOPE);
            }
            else
            {
               throw new ProtocolViolationException("SASL Header already sent to the remote SASL server");
            }
         }
      }

      private sealed class ClientSaslReadContext : ISaslPerformativeHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslClientContext client;

         public ClientSaslReadContext(ProtonSaslClientContext client)
         {
            this.client = client;
         }

         public void HandleMechanisms(SaslMechanisms saslMechanisms, IEngineHandlerContext context)
         {
            if (!client.mechanismsReceived)
            {
               client.serverMechanisms = (Symbol[])saslMechanisms.Mechanisms?.Clone();
               client.mechanismsReceived = true;
               client.authenticator.HandleSaslMechanisms(client, client.ServerMechanisms);
            }
            else
            {
               throw new ProtocolViolationException("Remote sent illegal additional SASL Mechanisms frame.");
            }
         }

         public void HandleInit(SaslInit saslInit, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Init Frame received at SASL Client.");
         }

         public void HandleChallenge(SaslChallenge saslChallenge, IEngineHandlerContext context)
         {
            if (client.mechanismsReceived)
            {
               client.responseRequired = true;
               client.authenticator.HandleSaslChallenge(client, saslChallenge.Challenge);
            }
            else
            {
               throw new ProtocolViolationException("Remote sent unexpected SASL Challenge frame.");
            }
         }

         public void HandleResponse(SaslResponse saslResponse, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Response Frame received at SASL Client.");
         }

         public void HandleOutcome(SaslOutcome saslOutcome, IEngineHandlerContext context)
         {
            client.Done(saslOutcome.Code.ToSaslAuthOutcome());

            SaslException saslFailure = null;
            switch (saslOutcome.Code)
            {
               case SaslCode.Auth:
                  saslFailure = new AuthenticationException("SASL exchange failed to authenticate client");
                  break;
               case SaslCode.Ok:
                  break;
               case SaslCode.Sys:
                  saslFailure = new SaslSystemException("SASL handshake failed due to a system error", true);
                  break;
               case SaslCode.SysTemp:
                  saslFailure = new SaslSystemException("SASL handshake failed due to a transient system error", false);
                  break;
               case SaslCode.SysPerm:
                  saslFailure = new SaslSystemException("SASL handshake failed due to a permanent system error", true);
                  break;
               default:
                  saslFailure = new SaslException("SASL handshake failed due to an unknown error");
                  break;
            }

            try
            {
               client.authenticator.HandleSaslOutcome(client, client.Outcome, saslOutcome.AdditionalData);
            }
            catch (Exception error)
            {
               if (saslFailure == null)
               {
                  saslFailure = new SaslException("Client threw unknown error while processing the outcome", error);
               }
            }

            // Request that the SASL handler be removed from the chain now that we are done with the SASL
            // exchange, the engine driver will remain in place holding the state for later examination.
            context.Engine.Pipeline.Remove(client.saslHandler);

            if (saslFailure == null)
            {
               if (client.pausedAMQPHeader != null)
               {
                  context.FireWrite(client.pausedAMQPHeader);
               }
            }
            else
            {
               context.Engine.EngineFailed(saslFailure);
            }
         }
      }

      private sealed class ClientSaslWriteContext : ISaslPerformativeHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslClientContext client;

         public ClientSaslWriteContext(ProtonSaslClientContext client)
         {
            this.client = client;
         }

         public void HandleMechanisms(SaslMechanisms saslMechanisms, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Mechanisms Frame written from SASL Client.");
         }

         public void HandleInit(SaslInit saslInit, IEngineHandlerContext context)
         {
            if (!client.mechanismChosen)
            {
               client.chosenMechanism = saslInit.Mechanism;
               client.hostname = saslInit.Hostname;
               client.mechanismChosen = true;
               context.FireWrite(new SaslEnvelope(saslInit));
            }
            else
            {
               throw new ProtocolViolationException("SASL Init already sent to the remote SASL server");
            }
         }

         public void HandleChallenge(SaslChallenge saslChallenge, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Challenge Frame written from SASL Client.");
         }

         public void HandleResponse(SaslResponse saslResponse, IEngineHandlerContext context)
         {
            if (client.responseRequired)
            {
               client.responseRequired = false;
               context.FireWrite(new SaslEnvelope(saslResponse));
            }
            else
            {
               throw new ProtocolViolationException("SASL Response is not currently expected by remote server");
            }
         }

         public void HandleOutcome(SaslOutcome saslOutcome, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Outcome Frame written from SASL Client.");
         }
      }

      #endregion
   }
}