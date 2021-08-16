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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Exceptions;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine.Implementation
{
   /// <summary>
   /// Creates instance of delivery tags that have an empty byte buffer body
   /// </summary>
   public sealed class ProtonEngine : IEngine
   {
      private static readonly IProtonBuffer EMPTY_FRAME_BUFFER =
         ProtonByteBufferAllocator.Instance.Wrap(new byte[] { 0x00, 0x00, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00 });

      private readonly ProtonEnginePipeline pipeline;
      private readonly ProtonEnginePipelineProxy pipelineProxy;
      private readonly ProtonEngineConfiguration configuration;
      // TODO
      // private readonly AmqpPerformativeEnvelopePool<OutgoingAMQPEnvelope> framePool = AMQPPerformativeEnvelopePool.outgoingEnvelopePool();

      private readonly ProtonConnection connection;

      private IEngineSaslDriver saslDriver = new ProtonEngineNoOpSaslDriver();

      private EngineState state = EngineState.Idle;
      private Exception failureCause;
      private uint inputSequence;
      private uint outputSequence;

      // Idle Timeout Check data
      private Task nextIdleTimeoutCheck;
      private TaskScheduler idleTimeoutExecutor;
      private int lastInputSequence;
      private int lastOutputSequence;
      private long localIdleDeadline = 0;
      private long remoteIdleDeadline = 0;

      // Engine event points
      private Action<IProtonBuffer, Action> outputHandler;
      private Action<IEngine> engineShutdownHandler;
      private Action<IEngine> engineFailureHandler = (engine) =>
      {
         // TODO : LOG.warn("Engine encountered error and will become inoperable: ", engine.failureCause());
      };

      public ProtonEngine() : base()
      {
         connection = new ProtonConnection(this);
         pipeline = new ProtonEnginePipeline(this);
         pipelineProxy = new ProtonEnginePipelineProxy(pipeline);
         configuration = new ProtonEngineConfiguration(this);
      }

      /// <summary>
      /// Allows for registration of a custom {@link EngineSaslDriver} that will convey SASL
      /// state and configuration for this engine.
      /// </summary>
      /// <param name="saslDriver">The SASL driver that this engine should use.</param>
      /// <exception cref="EngineStateException">If the engine has already been shutdown or failed</exception>
      /// <exception cref="EngineStartedException">If the engine has already been started</exception>
      public void RegisterSaslDriver(IEngineSaslDriver saslDriver)
      {
         CheckShutdownOrFailed("Cannot register a SASL driver on an Engine that is shutdown or failed.");

         if (state > EngineState.Starting)
         {
            throw new EngineStartedException("Cannot alter SASL driver after Engine has been started.");
         }

         this.saslDriver = saslDriver;
      }

      public IConnection Connection => connection;

      public bool IsFailed => failureCause != null;

      public bool IsRunning => state == EngineState.Started;

      public bool IsShutdown => state >= EngineState.Shutdown;

      public Exception FailureCause => failureCause;

      public EngineState EngineState => state;

      public IEngineConfiguration Configuration => configuration;

      public IEnginePipeline Pipeline => pipelineProxy;

      public IEngineSaslDriver SaslDriver => saslDriver;

      public EngineStateException EngineFailed(Exception cause)
      {
         throw new NotImplementedException();
      }

      public IEngine ErrorHandler(Action<IEngine> handler)
      {
         throw new NotImplementedException();
      }

      public IEngine Ingest(IProtonBuffer input)
      {
         throw new NotImplementedException();
      }

      public IEngine OutputHandler(Action<IProtonBuffer, Action> handler)
      {
         throw new NotImplementedException();
      }

      public IEngine Shutdown()
      {
         throw new NotImplementedException();
      }

      public IEngine ShutdownHandler(Action<IEngine> handler)
      {
         throw new NotImplementedException();
      }

      public IEngine Start()
      {
         throw new NotImplementedException();
      }

      public long Tick(long current)
      {
         throw new NotImplementedException();
      }

      public IEngine TickAuto(TaskScheduler scheduler)
      {
         throw new NotImplementedException();
      }

      #region Internal Engine APIs

      ProtonEngine FireWrite(HeaderEnvelope frame)
      {
         pipeline.FireWrite(frame);
         return this;
      }

      ProtonEngine FireWrite(OutgoingAmqpEnvelope frame)
      {
         pipeline.FireWrite(frame);
         return this;
      }

      // TODO : FramePool
      // ProtonEngine FireWrite(IPerformative performative, int channel)
      // {
      //    pipeline.FireWrite(framePool.take(performative, channel, null));
      //    return this;
      // }

      // ProtonEngine FireWrite(IPerformative performative, int channel, IProtonBuffer payload)
      // {
      //    pipeline.FireWrite(framePool.take(performative, channel, payload));
      //    return this;
      // }

      // OutgoingAmqpEnvelope wrap(IPerformative performative, int channel, IProtonBuffer payload)
      // {
      //    return framePool.take(performative, channel, payload);
      // }

      internal void CheckEngineNotStarted(string message)
      {
         if (state == EngineState.Idle)
         {
            throw new EngineNotStartedException(message);
         }
      }

      internal void CheckFailed(string message)
      {
         if (state == EngineState.Failed)
         {
            throw ProtonExceptionSupport.CreateFailedException(message, failureCause);
         }
      }

      internal void CheckShutdownOrFailed(string message)
      {
         if (state > EngineState.Started)
         {
            if (IsFailed)
            {
               throw ProtonExceptionSupport.CreateFailedException(message, failureCause);
            }
            else
            {
               throw new EngineShutdownException(message);
            }
         }
      }

      internal void DispatchWriteToEventHandler(IProtonBuffer buffer, Action ioComplete)
      {
         if (outputHandler != null)
         {
            outputSequence++;
            try
            {
               outputHandler.Invoke(buffer, ioComplete);
            }
            catch (Exception error)
            {
               throw EngineFailed(error);
            }
         }
         else
         {
            throw EngineFailed(new InvalidOperationException("No output handler configured for the Engine to use"));
         }
      }

      #endregion
   }
}