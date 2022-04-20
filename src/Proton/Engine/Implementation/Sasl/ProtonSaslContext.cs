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

using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Security;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation.Sasl
{
   /// <summary>
   /// Engine handler that manages the SASL authentication process that occurs
   /// either on the client or server end of the SASL exchange.
   /// </summary>
   public abstract class ProtonSaslContext : ISaslContext
   {
      protected readonly ProtonSaslHandler saslHandler;

      private ProtonAttachments attachments;

      // Client negotiations tracking.
      protected Symbol[] serverMechanisms;
      protected Symbol chosenMechanism;
      protected string hostname;
      protected EngineSaslState state = EngineSaslState.Idle;
      protected SaslAuthOutcome outcome;

      private bool done;

      internal ProtonSaslContext(ProtonSaslHandler handler)
      {
         this.saslHandler = handler;
      }

      public IAttachments Attachments => attachments ??= new ProtonAttachments();

      public bool IsServer => Role == SaslContextRole.Server;

      public bool IsClient => Role == SaslContextRole.Client;

      public bool IsDone => done;

      public SaslAuthOutcome Outcome => outcome;

      public EngineSaslState State => state;

      public Symbol[] ServerMechanisms => (Symbol[])(serverMechanisms?.Clone());

      public Symbol ChosenMechanism => chosenMechanism;

      public string Hostname => hostname;

      #region Abstract context API that the subclasses need to implement

      public abstract SaslContextRole Role { get; }

      internal abstract ProtonSaslContext HandleContextInitialization(ProtonEngine engine);

      #endregion

      #region Internal SASL layer API

      internal ProtonSaslHandler Handler => Handler;

      internal ProtonSaslContext Done(SaslAuthOutcome outcome)
      {
         this.done = true;
         this.outcome = outcome;
         this.state = outcome == SaslAuthOutcome.SaslOk ? EngineSaslState.Authenticated : EngineSaslState.AuthenticationFailed;

         return this;
      }

      internal abstract IHeaderHandler<IEngineHandlerContext> HeaderReadContext { get; }

      internal abstract IHeaderHandler<IEngineHandlerContext> HeaderWriteContext { get; }

      internal abstract ISaslPerformativeHandler<IEngineHandlerContext> SaslReadContext { get; }

      internal abstract ISaslPerformativeHandler<IEngineHandlerContext> SaslWriteContext { get; }

      #endregion
   }
}