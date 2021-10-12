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
using Apache.Qpid.Proton.Engine.Sasl;

namespace Apache.Qpid.Proton.Engine.Implementation.Sasl
{
   /// <summary>
   /// Engine SASL Driver implementation which handles the configuration
   /// and initial engine setup which provides the handling of SASL exchanges
   /// between clients and server.
   /// </summary>
   public sealed class ProtonEngineSaslDriver : IEngineSaslDriver
   {
      /// <summary>
      /// Default max frame size value used by this engine SASL driver if not otherwise configured.
      /// </summary>
      public readonly static uint DEFAULT_MAX_SASL_FRAME_SIZE = 4096;

      /// <summary>
      /// The specification define lower bound for SASL frame size.
      /// </summary>
      private readonly static uint MIN_MAX_SASL_FRAME_SIZE = 512;

      private readonly ProtonSaslHandler handler;
      private readonly ProtonEngine engine;

      private uint maxFrameSize = DEFAULT_MAX_SASL_FRAME_SIZE;
      private ProtonSaslContext context;

      internal ProtonEngineSaslDriver(ProtonEngine engine, ProtonSaslHandler handler)
      {
         this.handler = handler;
         this.engine = engine;
      }

      public EngineSaslState SaslState => context?.State ?? EngineSaslState.Idle;

      public SaslAuthOutcome? SaslOutcome => context?.Outcome;

      public uint MaxFrameSize
      {
         get => maxFrameSize;
         set
         {
            if (SaslState == EngineSaslState.Idle)
            {
               if (value < MIN_MAX_SASL_FRAME_SIZE)
               {
                  throw new ArgumentOutOfRangeException("Cannot set a max frame size lower than: " + MIN_MAX_SASL_FRAME_SIZE);
               }
               else if (value > Int32.MaxValue)
               {
                  throw new ArgumentOutOfRangeException("Cannot set a max frame size larger than: " + Int32.MaxValue);
               }
               else
               {
                  this.maxFrameSize = value;
               }
            }
            else
            {
               throw new InvalidOperationException("Cannot configure max SASL frame size after SASL negotiations have started");
            }
         }
      }

      public ISaslClientContext Client()
      {
         if (context?.IsServer ?? false)
         {
            throw new InvalidOperationException("Engine SASL Context already operating in server mode");
         }
         if (engine.EngineState > EngineState.Started)
         {
            throw new InvalidOperationException("Engine is already shutdown or failed, cannot create client context.");
         }

         if (context == null)
         {
            context = new ProtonSaslClientContext(handler);
            // If already started we initialize here to ensure that it gets done
            if (engine.EngineState == EngineState.Started)
            {
               context.HandleContextInitialization(engine);
            }
         }

         return (ProtonSaslClientContext)context;
      }

      public ISaslServerContext Server()
      {
         if (context?.IsClient ?? false)
         {
            throw new InvalidOperationException("Engine SASL Context already operating in client mode");
         }
         if (engine.EngineState > EngineState.Started)
         {
            throw new InvalidOperationException("Engine is already shutdown or failed, cannot create server context.");
         }

         if (context == null)
         {
            context = new ProtonSaslServerContext(handler);
            // If already started we initialize here to ensure that it gets done
            if (engine.EngineState == EngineState.Started)
            {
               context.HandleContextInitialization(engine);
            }
         }

         return (ProtonSaslServerContext)context;
      }

      #region Internal engine sasl driver APIs

      internal void HandleEngineStarting(ProtonEngine engine)
      {
         context?.HandleContextInitialization(engine);
      }

      internal ProtonSaslContext SaslContext => context;

      #endregion
   }
}