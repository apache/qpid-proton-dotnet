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
   public sealed class ProtonSaslServerContext : ProtonSaslContext, ISaslServerContext
   {
      private ISaslServerAuthenticator authenticator = ProtonDefaultSaslServerAuthenticator.Instance;

      private readonly ServerHeaderReadContext headerReadContext;
      private readonly ServerHeaderWriteContext headerWriteContext;
      private readonly ServerSaslReadContext saslReadContext;
      private readonly ServerSaslWriteContext saslWriteContext;

      // Work state trackers
      private bool headerWritten;
      private bool headerReceived;
      private bool mechanismsSent;
      private bool mechanismChosen;
      private bool responseRequired;

      internal ProtonSaslServerContext(ProtonSaslHandler handler) : base(handler)
      {
         this.headerReadContext = new ServerHeaderReadContext(this);
         this.headerWriteContext = new ServerHeaderWriteContext(this);
         this.saslReadContext = new ServerSaslReadContext(this);
         this.saslWriteContext = new ServerSaslWriteContext(this);
      }

      public ISaslServerAuthenticator Authenticator
      {
         get => authenticator;
         set => authenticator = value;
      }

      public override SaslContextRole Role => SaslContextRole.Server;

      public ISaslServerContext SendMechanisms(Symbol[] mechanisms)
      {
         if (mechanisms == null)
         {
            throw new ArgumentNullException("Server mechanisms array cannot be null");
         }

         saslHandler.Engine.Pipeline.FireWrite(new SaslEnvelope(new SaslMechanisms(mechanisms)));
         return this;
      }

      public ISaslServerContext SendChallenge(IProtonBuffer challenge)
      {
         if (challenge == null)
         {
            throw new ArgumentNullException("Server challenge array cannot be null");
         }

         saslHandler.Engine.Pipeline.FireWrite(new SaslEnvelope(new SaslChallenge(challenge)));
         return this;
      }

      public ISaslServerContext SendOutcome(SaslAuthOutcome outcome, IProtonBuffer additional)
      {
         saslHandler.Engine.Pipeline.FireWrite(new SaslEnvelope(new SaslOutcome(outcome.ToSaslCode(), additional)));
         return this;
      }

      public ISaslServerContext SaslFailure(SaslException failure)
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

      #region API for the SASL Handler to access the read and write contexts

      internal override IHeaderHandler<IEngineHandlerContext> HeaderReadContext => headerReadContext;

      internal override IHeaderHandler<IEngineHandlerContext> HeaderWriteContext => headerWriteContext;

      internal override ISaslPerformativeHandler<IEngineHandlerContext> SaslReadContext => saslReadContext;

      internal override ISaslPerformativeHandler<IEngineHandlerContext> SaslWriteContext => saslWriteContext;

      #endregion

      #region Read and Write contexts presented to the sasl handler

      private sealed class ServerHeaderReadContext : IHeaderHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslServerContext server;

         public ServerHeaderReadContext(ProtonSaslServerContext server)
         {
            this.server = server;
         }

         public void HandleAMQPHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            // Raw AMQP Header shouldn't arrive before the SASL negotiations are done.
            context.FireWrite(HeaderEnvelope.SASL_HEADER_ENVELOPE);
            throw new ProtocolViolationException("Unexpected AMQP Header before SASL Authentication completed.");
         }

         public void HandleSASLHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            if (server.headerReceived)
            {
               throw new ProtocolViolationException("Unexpected second SASL Header read before SASL Authentication completed.");
            }
            else
            {
               server.headerReceived = true;
            }

            if (!server.headerWritten)
            {
               context.FireWrite(HeaderEnvelope.SASL_HEADER_ENVELOPE);
               server.headerWritten = true;
               server.state = EngineSaslState.Authenticating;
            }

            server.authenticator.HandleSaslHeader(server, header);
         }
      }

      private sealed class ServerHeaderWriteContext : IHeaderHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslServerContext server;

         public ServerHeaderWriteContext(ProtonSaslServerContext server)
         {
            this.server = server;
         }

         public void HandleAMQPHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected AMQP Header write before SASL Authentication completed.");
         }

         public void HandleSASLHeader(AmqpHeader header, IEngineHandlerContext context)
         {
            if (server.headerWritten)
            {
               throw new ProtocolViolationException("Unexpected SASL write following a previous header send.");
            }

            server.headerWritten = true;
            context.FireWrite(HeaderEnvelope.SASL_HEADER_ENVELOPE);
         }
      }

      private sealed class ServerSaslReadContext : ISaslPerformativeHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslServerContext server;

         public ServerSaslReadContext(ProtonSaslServerContext server)
         {
            this.server = server;
         }

         public void HandleMechanisms(SaslMechanisms saslMechanisms, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Mechanisms Frame received at SASL Server.");
         }

         public void HandleInit(SaslInit saslInit, IEngineHandlerContext context)
         {
            if (server.mechanismChosen)
            {
               throw new ProtocolViolationException("SASL Handler received second SASL Init");
            }

            server.hostname = saslInit.Hostname;
            server.chosenMechanism = saslInit.Mechanism;
            server.mechanismChosen = true;

            server.authenticator.HandleSaslInit(server, server.chosenMechanism, saslInit.InitialResponse);
         }

         public void HandleChallenge(SaslChallenge saslChallenge, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Challenge Frame received at SASL Server.");
         }

         public void HandleResponse(SaslResponse saslResponse, IEngineHandlerContext context)
         {
            if (server.responseRequired)
            {
               server.authenticator.HandleSaslResponse(server, saslResponse.Response);
            }
            else
            {
               throw new ProtocolViolationException("SASL Response received when none was expected");
            }
         }

         public void HandleOutcome(SaslOutcome saslOutcome, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Outcome Frame received at SASL Server.");
         }
      }

      private sealed class ServerSaslWriteContext : ISaslPerformativeHandler<IEngineHandlerContext>
      {
         private readonly ProtonSaslServerContext server;

         public ServerSaslWriteContext(ProtonSaslServerContext server)
         {
            this.server = server;
         }

         public void handleMechanisms(SaslMechanisms saslMechanisms, IEngineHandlerContext context)
         {
            if (!server.mechanismsSent)
            {
               context.FireWrite(new SaslEnvelope(saslMechanisms));
               server.serverMechanisms = (Symbol[])saslMechanisms.Mechanisms?.Clone();
               server.mechanismsSent = true;
            }
            else
            {
               throw new ProtocolViolationException("SASL Mechanisms already sent to client");
            }
         }

         public void handleInit(SaslInit saslInit, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Init Frame write attempted on SASL Server.");
         }

         public void handleChallenge(SaslChallenge saslChallenge, IEngineHandlerContext context)
         {
            if (server.headerWritten && server.mechanismsSent && !server.responseRequired)
            {
               context.FireWrite(new SaslEnvelope(saslChallenge));
               server.responseRequired = true;
            }
            else
            {
               throw new ProtocolViolationException("SASL Challenge sent when state does not allow it");
            }
         }

         public void handleResponse(SaslResponse saslResponse, IEngineHandlerContext context)
         {
            throw new ProtocolViolationException("Unexpected SASL Response Frame write attempted on SASL Server.");
         }

         public void handleOutcome(SaslOutcome saslOutcome, IEngineHandlerContext context)
         {
            if (server.headerWritten && server.mechanismsSent && !server.responseRequired)
            {
               server.Done(saslOutcome.Code.ToSaslAuthOutcome());
               context.FireWrite(new SaslEnvelope(saslOutcome));
               // Request that the SASL handler be removed from the chain now that we are done with the SASL
               // exchange, the engine driver will remain in place holding the state for later examination.
               context.Engine.Pipeline.Remove(server.saslHandler);
            }
            else
            {
               throw new ProtocolViolationException("SASL Outcome sent when state does not allow it");
            }
         }

         #endregion
      }
   }
}