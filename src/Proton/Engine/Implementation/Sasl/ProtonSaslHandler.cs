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
using Apache.Qpid.Proton.Engine.Exceptions;

namespace Apache.Qpid.Proton.Engine.Implementation.Sasl
{
   /// <summary>
   /// Engine handler that manages the SASL authentication process that occurs
   /// either on the client or server end of the SASL exchange.
   /// </summary>
   public sealed class ProtonSaslHandler : IEngineHandler
   {
      private IEngineHandlerContext context;
      private ProtonEngineSaslDriver driver;
      private ProtonEngine engine;
      private ProtonSaslContext saslContext;

      /// <summary>
      /// Determines if the SASL authentication process has completed.
      /// </summary>
      public bool IsDone => saslContext?.IsDone ?? false;

      public void HandlerAdded(IEngineHandlerContext context)
      {
         this.engine = (ProtonEngine)context.Engine;
         this.driver = new ProtonEngineSaslDriver(engine, this);
         this.context = context;

         engine.RegisterSaslDriver(driver);
      }

      public void HandlerRemoved(IEngineHandlerContext context)
      {
         this.driver = null;
         this.saslContext = null;
         this.engine = null;
         this.context = null;

         // If the engine wasn't started then it is okay to remove this handler otherwise
         // we would only be removed from the pipeline on completion of SASL negotiations
         // and the driver must remain to convey the outcome.
         if (context.Engine.EngineState == EngineState.Idle)
         {
            ((ProtonEngine)context.Engine).RegisterSaslDriver(ProtonEngineNoOpSaslDriver.Instance);
         }
      }

      public void EngineStarting(IEngineHandlerContext context)
      {
         driver.HandleEngineStarting(engine);
      }

      public void HandleRead(IEngineHandlerContext context, HeaderEnvelope header)
      {
         if (IsDone)
         {
            context.FireRead(header);
         }
         else
         {
            // Default to server if application has not configured one way or the other.
            saslContext = driver.SaslContext;
            if (saslContext == null)
            {
               saslContext = (ProtonSaslContext)driver.Server();
            }

            header.Invoke(saslContext.HeaderReadContext, context);
         }
      }

      public void HandleRead(IEngineHandlerContext context, SaslEnvelope frame)
      {
         if (IsDone)
         {
            throw new ProtocolViolationException("Unexpected SASL Frame: SASL processing has already completed");
         }
         else
         {
            frame.Invoke(SafeGetSaslContext().SaslReadContext, context);
         }
      }

      public void HandleRead(IEngineHandlerContext context, IncomingAmqpEnvelope frame)
      {
         if (IsDone)
         {
            context.FireRead(frame);
         }
         else
         {
            throw new ProtocolViolationException("Unexpected AMQP Frame: SASL processing not yet completed");
         }
      }

      public void HandleWrite(IEngineHandlerContext context, HeaderEnvelope frame)
      {
         if (IsDone)
         {
            context.FireWrite(frame);
         }
         else
         {
            // Default to client if application has not configured one way or the other.
            saslContext = driver.SaslContext;
            if (saslContext == null)
            {
               saslContext = (ProtonSaslContext)driver.Client();
            }

            // Delegate write to the SASL Context in use to allow for state updates.
            frame.Invoke(saslContext.HeaderWriteContext, context);
         }
      }

      public void HandleWrite(IEngineHandlerContext context, OutgoingAmqpEnvelope frame)
      {
         if (IsDone)
         {
            context.FireWrite(frame);
         }
         else
         {
            throw new ProtocolViolationException("Unexpected AMQP Performative: SASL processing not yet completed");
         }
      }

      public void HandleWrite(IEngineHandlerContext context, SaslEnvelope frame)
      {
         if (IsDone)
         {
            throw new ProtocolViolationException("Unexpected SASL Performative: SASL processing has yet completed");
         }
         else
         {
            // Delegate to the SASL Context to allow state tracking to be maintained.
            frame.Invoke(SafeGetSaslContext().SaslWriteContext, context);
         }
      }

      #region Internal Proton SASL API

      internal ProtonEngine Engine => engine;

      internal IEngineHandlerContext Context => context;

      private ProtonSaslContext SafeGetSaslContext()
      {
         return saslContext ?? throw new InvalidOperationException("Cannot process incoming SASL performative, driver not yet initialized");
      }

      #endregion
   }
}