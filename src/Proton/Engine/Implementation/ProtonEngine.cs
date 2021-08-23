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
using Apache.Qpid.Proton.Types;
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
      private readonly AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> framePool =
         AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope>.OutgoingEnvelopePool();

      private readonly ProtonConnection connection;

      private IEngineSaslDriver saslDriver = new ProtonEngineNoOpSaslDriver();

      private EngineState state = EngineState.Idle;
      private bool writable;
      private Exception failureCause;
      private uint inputSequence;
      private uint outputSequence;

      // Idle Timeout Check data
      private Task nextIdleTimeoutCheck;
      private TaskScheduler idleTimeoutExecutor;
      private long lastInputSequence;
      private long lastOutputSequence;
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

      public bool IsWritable => writable;

      public Exception FailureCause => failureCause;

      public EngineState EngineState => state;

      public IEngineConfiguration Configuration => configuration;

      public IEnginePipeline Pipeline => pipelineProxy;

      public IEngineSaslDriver SaslDriver => saslDriver;

      public IConnection Start()
      {
         CheckShutdownOrFailed("Cannot start an Engine that has already been shutdown or has failed.");

         if (state == EngineState.Idle)
         {
            state = EngineState.Starting;
            try
            {
               pipeline.FireEngineStarting();
               state = EngineState.Started;
               writable = true;
               connection.HandleEngineStarted(this);
            }
            catch (Exception error)
            {
               throw EngineFailed(error);
            }
         }

         return connection;
      }

      public IEngine Shutdown()
      {
         if (state < EngineState.ShuttingDown)
         {
            state = EngineState.Shutdown;
            writable = false;

            if (nextIdleTimeoutCheck != null)
            {
               // TODO : LOG.trace("Canceling scheduled Idle Timeout Check");
               // TODO : Cancellation Token -> nextIdleTimeoutCheck.Cancel(false);
               nextIdleTimeoutCheck = null;
            }

            try
            {
               pipeline.FireEngineStateChanged();
            }
            catch (Exception) { }

            try
            {
               connection.HandleEngineShutdown(this);
            }
            catch (Exception)
            {
            }
            finally
            {
               if (engineShutdownHandler != null)
               {
                  engineShutdownHandler.Invoke(this);
               }
            }
         }

         return this;
      }

      public long Tick(long currentTime)
      {
         CheckShutdownOrFailed("Cannot tick an Engine that has been shutdown or failed.");

         if (connection.ConnectionState != ConnectionState.Active)
         {
            throw new InvalidOperationException("Cannot tick on a Connection that is not opened or an engine that has been shut down.");
         }

         if (idleTimeoutExecutor != null)
         {
            throw new InvalidOperationException("Automatic ticking previously initiated.");
         }

         PerformReadCheck(currentTime);
         PerformWriteCheck(currentTime);

         return NextTickDeadline(localIdleDeadline, remoteIdleDeadline);
      }

      public IEngine TickAuto(TaskScheduler scheduler)
      {
         CheckShutdownOrFailed("Cannot start auto tick on an Engine that has been shutdown or failed");

         if (scheduler == null)
         {
            throw new ArgumentNullException("The provided Task Scheduler cannot be null");
         }

         if (connection.ConnectionState != ConnectionState.Active)
         {
            throw new InvalidOperationException("Cannot tick on a Connection that is not opened.");
         }

         if (idleTimeoutExecutor != null)
         {
            throw new InvalidOperationException("Automatic ticking previously initiated.");
         }

         // TODO - As an additional feature of this method we could allow for calling before connection is
         //        opened such that it starts ticking either on open local and also checks as a response to
         //        remote open which seems might be needed anyway, see notes in IdleTimeoutCheck class.

         // Immediate run of the idle timeout check logic will decide afterwards when / if we should
         // reschedule the idle timeout processing.
         // TODO : LOG.trace("Auto Idle Timeout Check being initiated");
         idleTimeoutExecutor = scheduler;
         // TODO : idleTimeoutExecutor.TryExecuteTask(new IdleTimeoutCheck());

         return this;
      }

      public IEngine Ingest(IProtonBuffer input)
      {
         CheckShutdownOrFailed("Cannot ingest data into an Engine that has been shutdown or failed");

         if (!IsWritable)
         {
            throw new EngineNotWritableException("Engine is currently not accepting new input");
         }

         try
         {
            long startIndex = input.ReadOffset;
            pipeline.FireRead(input);
            if (input.ReadOffset != startIndex)
            {
               inputSequence++;
            }
         }
         catch (Exception error)
         {
            throw EngineFailed(error);
         }

         return this;
      }

      public EngineStateException EngineFailed(Exception cause)
      {
         EngineStateException failure;

         if (state < EngineState.ShuttingDown && state != EngineState.Failed)
         {
            state = EngineState.Failed;
            failureCause = cause;
            writable = false;

            if (nextIdleTimeoutCheck != null)
            {
               // TODO : LOG.trace("Canceling scheduled Idle Timeout Check");
               // TODO : nextIdleTimeoutCheck.cancel(false);
               nextIdleTimeoutCheck = null;
            }

            failure = ProtonExceptionSupport.CreateFailedException(cause);

            try
            {
               pipeline.FireFailed((EngineFailedException)failure);
            }
            catch (Exception)
            {
            }

            try
            {
               connection.HandleEngineFailed(this, cause);
            }
            catch (Exception)
            {
            }

            engineFailureHandler?.Invoke(this);
         }
         else
         {
            if (IsFailed)
            {
               failure = ProtonExceptionSupport.CreateFailedException(cause);
            }
            else
            {
               failure = new EngineShutdownException("Engine has transitioned to shutdown state");
            }
         }

         return failure;
      }

      public IEngine ErrorHandler(Action<IEngine> handler)
      {
         this.engineFailureHandler = handler;
         return this;
      }

      public IEngine OutputHandler(Action<IProtonBuffer, Action> handler)
      {
         this.outputHandler = handler;
         return this;
      }

      public IEngine ShutdownHandler(Action<IEngine> handler)
      {
         this.engineShutdownHandler = handler;
         return this;
      }

      #region Internal Engine APIs

      internal ProtonEngine FireWrite(HeaderEnvelope frame)
      {
         pipeline.FireWrite(frame);
         return this;
      }

      internal ProtonEngine FireWrite(OutgoingAmqpEnvelope frame)
      {
         pipeline.FireWrite(frame);
         return this;
      }

      internal ProtonEngine FireWrite(IPerformative performative, ushort channel)
      {
         pipeline.FireWrite(framePool.Take(performative, channel, null));
         return this;
      }

      internal ProtonEngine FireWrite(IPerformative performative, ushort channel, IProtonBuffer payload)
      {
         pipeline.FireWrite(framePool.Take(performative, channel, payload));
         return this;
      }

      internal OutgoingAmqpEnvelope Wrap(IPerformative performative, ushort channel, IProtonBuffer payload)
      {
         return framePool.Take(performative, channel, payload);
      }

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

      internal void RecomputeEffectiveFrameSizeLimits()
      {
         configuration.RecomputeEffectiveFrameSizeLimits();
      }

      internal uint OutboundMaxFrameSize => configuration.OutboundMaxFrameSize;

      internal uint IndboundMaxFrameSize => configuration.InboundMaxFrameSize;

      #endregion

      #region Idle timeout processing

      private const long MIN_IDLE_CHECK_INTERVAL = 1000;
      private const long MAX_IDLE_CHECK_INTERVAL = 10000;

      private void PerformReadCheck(long currentTime)
      {
         long localIdleTimeout = connection.IdleTimeout;

         if (localIdleTimeout > 0)
         {
            if (localIdleDeadline == 0 || lastInputSequence != inputSequence)
            {
               localIdleDeadline = ComputeDeadline(currentTime, localIdleTimeout);
               lastInputSequence = inputSequence;
            }
            else if (localIdleDeadline - currentTime <= 0)
            {
               if (connection.ConnectionState != ConnectionState.Closed)
               {
                  ErrorCondition condition = new ErrorCondition(
                      Symbol.Lookup("amqp:resource-limit-exceeded"), "local-idle-timeout expired");
                  connection.ErrorCondition = condition;
                  connection.Close();
                  EngineFailed(new IdleTimeoutException("Remote idle timeout detected"));
               }
               else
               {
                  localIdleDeadline = ComputeDeadline(currentTime, localIdleTimeout);
               }
            }
         }
      }

      private void PerformWriteCheck(long currentTime)
      {
         long remoteIdleTimeout = connection.RemoteIdleTimeout;

         if (remoteIdleTimeout > 0 && !connection.IsLocallyClosed)
         {
            if (remoteIdleDeadline == 0 || lastOutputSequence != outputSequence)
            {
               remoteIdleDeadline = ComputeDeadline(currentTime, remoteIdleTimeout / 2);
               lastOutputSequence = outputSequence;
            }
            else if (remoteIdleDeadline - currentTime <= 0)
            {
               remoteIdleDeadline = ComputeDeadline(currentTime, remoteIdleTimeout / 2);
               pipeline.FireWrite(EMPTY_FRAME_BUFFER.Copy(), null);
               lastOutputSequence++;
            }
         }
      }

      private long ComputeDeadline(long now, long timeout)
      {
         long deadline = now + timeout;
         // We use 0 to signal not-initialized and/or no-timeout, so in the
         // unlikely event thats to be the actual deadline, return 1 instead
         return deadline != 0 ? deadline : 1;
      }

      private static long NextTickDeadline(long localIdleDeadline, long remoteIdleDeadline)
      {
         long deadline;

         // If there is no locally set idle timeout then we just honor the remote idle timeout
         // value otherwise we need to use the lesser of the next local or remote idle timeout
         // deadline values to compute the next time a check is needed.
         if (localIdleDeadline == 0)
         {
            deadline = remoteIdleDeadline;
         }
         else if (remoteIdleDeadline == 0)
         {
            deadline = localIdleDeadline;
         }
         else
         {
            if (remoteIdleDeadline - localIdleDeadline <= 0)
            {
               deadline = remoteIdleDeadline;
            }
            else
            {
               deadline = localIdleDeadline;
            }
         }

         return deadline;
      }

      public void RunIdleTimeoutTask()
      {
         bool checkScheduled = false;

         if (connection.ConnectionState == ConnectionState.Active && !IsShutdown)
         {
            // Using nano time since it is not related to the wall clock, which may change
            long now = 0; // TODO Tick TimeUnit.NANOSECONDS.toMillis(System.nanoTime());

            try
            {
               PerformReadCheck(now);
               PerformWriteCheck(now);

               long deadline = NextTickDeadline(localIdleDeadline, remoteIdleDeadline);

               // Check methods will close down the engine and fire error so we need to check that engine
               // state is active and engine is not shutdown before scheduling again.
               if (deadline != 0 && connection.ConnectionState == ConnectionState.Active && state == EngineState.Started)
               {
                  // Run the next idle check at half the deadline to try and ensure we meet our
                  // obligation of sending our heart beat on time.
                  long delay = (deadline - now) / 2;

                  // TODO - Some computation to work out a reasonable delay that still compensates for
                  //        errors in scheduling while preventing over eagerness.
                  delay = Math.Max(MIN_IDLE_CHECK_INTERVAL, delay);
                  delay = Math.Min(MAX_IDLE_CHECK_INTERVAL, delay);

                  checkScheduled = true;
                  // TODO :LOG.trace("IdleTimeoutCheck rescheduling with delay: {}", delay);
                  // TODO :nextIdleTimeoutCheck = idleTimeoutExecutor.schedule(this, delay, TimeUnit.MILLISECONDS);
               }

               // TODO - If no local timeout but remote hasn't opened we might return zero and not
               //        schedule any ticking ?  Possible solution is to schedule after remote open
               //        arrives if nothing set to run and remote indicates it has an idle timeout.

            }
            catch (Exception)
            {
               // TODO :LOG.trace("Auto Idle Timeout Check encountered error during check: ", error);
            }
         }

         if (!checkScheduled)
         {
            nextIdleTimeoutCheck = null;
            // TODO : LOG.trace("Auto Idle Timeout Check task exiting and will not be rescheduled");
         }
      }

      #endregion
   }
}