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
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Implementation of engine configuration options along with Proton specific
   /// internal configuration and state update handling based on the current
   /// configuration and remote interactions.
   /// </summary>
   public sealed class ProtonEngineConfiguration : IEngineConfiguration
   {
      private static readonly IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ProtonEngine>();

      private readonly ProtonEngine engine;

      private IProtonBufferAllocator allocator = ProtonByteBufferAllocator.Instance;

      private uint effectiveMaxInboundFrameSize = ProtonConstants.MinMaxAmqpFrameSize;
      private uint effectiveMaxOutboundFrameSize = ProtonConstants.MinMaxAmqpFrameSize;

      public ProtonEngineConfiguration(ProtonEngine engine) : base()
      {
         this.engine = engine;
      }

      public IProtonBufferAllocator BufferAllocator
      {
         get => allocator;
         set => allocator = value ?? throw new ArgumentNullException(nameof(value), "Cannot assign a null allocator");
      }

      public bool TraceFrames
      {
         get
         {
            if (engine.Pipeline.Find(ProtonConstants.FrameLoggingHandler) is ProtonFrameLoggingHandler loggingHandler)
            {
               return loggingHandler.TraceFrames;
            }
            else
            {
               return false;
            }
         }
         set
         {
            if (engine.Pipeline.Find(ProtonConstants.FrameLoggingHandler) is ProtonFrameLoggingHandler loggingHandler)
            {
               loggingHandler.TraceFrames = value;
            }
            else
            {
               LOG.Debug("Engine not configured with a frame logging handler: cannot apply traceFrames={0}", value);
            }
         }
      }

      public uint OutboundMaxFrameSize => effectiveMaxOutboundFrameSize;

      public uint InboundMaxFrameSize => effectiveMaxInboundFrameSize;

      #region Internal Engine API

      internal void RecomputeEffectiveFrameSizeLimits()
      {
         // Based on engine state compute what the max in and out frame size should
         // be at this time.  Considerations to take into account are SASL state and
         // remote values once set.

         if (engine.SaslDriver.SaslState < EngineSaslState.Authenticating)
         {
            effectiveMaxInboundFrameSize = engine.SaslDriver.MaxFrameSize;
            effectiveMaxOutboundFrameSize = engine.SaslDriver.MaxFrameSize;
         }
         else
         {
            uint localMaxFrameSize = engine.Connection.MaxFrameSize;
            uint remoteMaxFrameSize = engine.Connection.RemoteMaxFrameSize;

            // Safety check ensure no implementation try to sneak by a lower bound than
            // the specification defined min-max AMQP Frame size;
            localMaxFrameSize = Math.Max(localMaxFrameSize, ProtonConstants.MinMaxAmqpFrameSize);
            remoteMaxFrameSize = Math.Max(remoteMaxFrameSize, ProtonConstants.MinMaxAmqpFrameSize);

            // We limit outbound max frame size to our own set max frame size unless the remote has actually
            // requested something smaller as opposed to just using a default like 2GB or something similarly
            // large which we could never support in practice.
            uint intermediateMaxOutboundFrameSize = Math.Min(localMaxFrameSize, remoteMaxFrameSize);

            effectiveMaxInboundFrameSize = engine.Connection.MaxFrameSize;

            effectiveMaxOutboundFrameSize = Math.Min(int.MaxValue, intermediateMaxOutboundFrameSize);
         }
      }

      #endregion
   }
}