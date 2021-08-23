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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Proton Engine handler that routes incoming and outgoing performatives
   /// </summary>
   public sealed class ProtonPerformativeHandler : IEngineHandler, IHeaderHandler<IEngineHandlerContext>, IPerformativeHandler<IEngineHandlerContext>
   {
      private ProtonEngine engine;
      private ProtonConnection connection;
      private ProtonEngineConfiguration configuration;

      #region Handle the pipeline events

      public void HandlerAdded(IEngineHandlerContext context)
      {
         engine = (ProtonEngine)context.Engine;
         connection = (ProtonConnection)engine.Connection;
         configuration = (ProtonEngineConfiguration)engine.Configuration;

         ((ProtonEngineHandlerContext)context).InterestMask = ProtonEngineHandlerContext.HANDLER_READS;
      }

      public void EngineFailed(IEngineHandlerContext context, EngineFailedException failure)
      {
         // In case external source injects failure we grab it and propagate after the
         // appropriate changes to our engine state.
         if (!engine.IsFailed)
         {
            engine.EngineFailed(failure.InnerException);
         }
      }

      public void HandleRead(IEngineHandlerContext context, HeaderEnvelope envelope)
      {
         envelope.Invoke(this, context);
      }

      public void HandleRead(IEngineHandlerContext context, IncomingAmqpEnvelope envelope)
      {
         try
         {
            envelope.Invoke(this, context);
         }
         finally
         {
            envelope.Release();
         }
      }

      #endregion

      // Here we can spy on incoming performatives and update engine state relative to
      // those prior to sending along notifications to other handlers or to the connection.
      //
      // We currently can't spy on outbound performatives but we could in future by splitting these
      // into inner classes for inbound and outbound and handle the write to invoke the outbound
      // handlers.

      #region Header Handler implementation

      public void HandleAMQPHeader(AmqpHeader header, IEngineHandlerContext context)
      {
         // Recompute max frame size now based on engine max frame size in case sasl was enabled.
         configuration.RecomputeEffectiveFrameSizeLimits();

         // Let the Connection know we have a header so it can emit any pending work.
         header.Invoke(connection, engine);
      }

      public void HandleSASLHeader(AmqpHeader header, IEngineHandlerContext context)
      {
         // Respond with Raw AMQP Header and then fail the engine.
         context.FireWrite(HeaderEnvelope.AMQP_HEADER_ENVELOPE);

         throw new ProtocolViolationException("Received SASL Header but no SASL support configured");
      }

      #endregion

      #region Performative Handler implementation

      public void HandleOpen(Open open, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         if (channel != 0)
         {
            throw new ProtocolViolationException("Open not sent on channel zero");
         }

         connection.HandleOpen(open, payload, channel, engine);

         // Recompute max frame size now based on what remote told us.
         configuration.RecomputeEffectiveFrameSizeLimits();
      }

      public void HandleBegin(Begin begin, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleBegin(begin, payload, channel, engine);
      }

      public void HandleAttach(Attach attach, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleAttach(attach, payload, channel, engine);
      }

      public void HandleFlow(Flow flow, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleFlow(flow, payload, channel, engine);
      }

      public void HandleTransfer(Transfer transfer, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleTransfer(transfer, payload, channel, engine);
      }

      public void HandleDisposition(Disposition disposition, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleDisposition(disposition, payload, channel, engine);
      }

      public void HandleDetach(Detach detach, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleDetach(detach, payload, channel, engine);
      }

      public void HandleEnd(End end, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleEnd(end, payload, channel, engine);
      }

      public void HandleClose(Close close, IProtonBuffer payload, ushort channel, IEngineHandlerContext context)
      {
         connection.HandleClose(close, payload, channel, engine);
      }

      #endregion
   }
}