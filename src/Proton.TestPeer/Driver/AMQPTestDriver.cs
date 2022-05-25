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
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// The AMQP Test driver internal frame processing a script handler class.
   /// </summary>
   public class AMQPTestDriver : IFrameHandler
   {
      private readonly Mutex mutex = new();
      private readonly string driverName;
      private readonly FrameDecoder frameParser;
      private readonly FrameEncoder frameEncoder;
      private readonly DriverTaskScheduler scheduler;

      private Open localOpen;
      private Open remoteOpen;

      private readonly DriverSessions sessions;

      private readonly Action<Stream> frameConsumer;
      private readonly Action<Exception> assertionConsumer;

      private volatile Exception failureCause;

      private uint advertisedIdleTimeout = 0;

      private volatile uint emptyFrameCount;

      private volatile uint performativeCount;

      private volatile uint saslPerformativeCount;

      private uint inboundMaxFrameSize = uint.MaxValue;
      private uint outboundMaxFrameSize = uint.MaxValue;

      private readonly ILoggerFactory loggerFactory;
      private readonly ILogger<AMQPTestDriver> logger;

      /// <summary>
      /// Holds the expectations for processing of data from the peer under test.
      /// Uses a thread safe queue to avoid contention on adding script entries
      /// and processing incoming data (although you should probably not do that).
      /// </summary>
      private readonly Queue<IScriptedElement> script = new();

      public AMQPTestDriver(string name, Action<Stream> frameConsumer) :
         this(name, frameConsumer, null, NullLoggerFactory.Instance)
      {
      }

      public AMQPTestDriver(string name, Action<Stream> frameConsumer, ILoggerFactory logFactory) :
         this(name, frameConsumer, null, logFactory)
      {
      }

      public AMQPTestDriver(string name, Action<Stream> frameConsumer, Action<Exception> assertConsumer, ILoggerFactory logFactory)
      {
         this.sessions = new DriverSessions(this);
         this.frameConsumer = frameConsumer;
         this.assertionConsumer = assertConsumer;
         this.driverName = name;
         this.loggerFactory = logFactory ?? NullLoggerFactory.Instance;
         this.logger = loggerFactory.CreateLogger<AMQPTestDriver>();

         this.scheduler = new DriverTaskScheduler(this);
         this.frameEncoder = new FrameEncoder(this);
         this.frameParser = new FrameDecoder(this);
      }

      internal DriverSessions Sessions => sessions;

      public string Name => driverName;

      public ILoggerFactory LoggerFactory
      {
         get => loggerFactory;
      }

      public uint InboundMaxFrameSize
      {
         get => inboundMaxFrameSize;
         set => inboundMaxFrameSize = value;
      }

      public uint OutboundMaxFrameSize
      {
         get => outboundMaxFrameSize;
         set => outboundMaxFrameSize = value;
      }

      public uint AdvertisedIdleTimeout
      {
         get => advertisedIdleTimeout;
         set => advertisedIdleTimeout = value;
      }

      public uint EmptyFrameCount => emptyFrameCount;

      public uint PerformativeCount => performativeCount;

      public uint SaslPerformativeCount => saslPerformativeCount;

      public Open RemoteOpen => remoteOpen;

      public Open LocalOpen => localOpen;

      internal void AfterDelay(long delay, ScriptedAction action)
      {
         scheduler.Schedule(action, delay);
      }

      internal void AddScriptedElement(IScriptedElement element)
      {
         CheckFailed();
         mutex.WaitOne();
         try
         {
            CheckFailed();
            script.Enqueue(element);
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      /// <summary>
      /// Throw an exception from processing incoming data which should be handled by the peer under test.
      /// </summary>
      /// <param name="ex">The exception that triggered this call</param>
      public void SignalFailure(Exception ex)
      {
         mutex.WaitOne();
         try
         {
            if (this.failureCause == null)
            {
               if (ex is AssertionError error)
               {
                  logger.LogTrace("{0} sending failure assertion due to: {1}", driverName, ex.Message);
                  this.failureCause = error;
               }
               else
               {
                  logger.LogTrace("{0} sending failure assertion due to: {1}", driverName, ex.Message);
                  this.failureCause = new AssertionError(ex.Message, ex);
               }

               SearchForScriptCompletionAndTrigger();

               if (assertionConsumer != null)
               {
                  assertionConsumer.Invoke(failureCause);
               }
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      /// <summary>
      /// Throw an exception from processing incoming data which should be handled by the peer under test.
      /// </summary>
      /// <param name="message">The message used to create the assertion error</param>
      public void SignalFailure(string message)
      {
         SignalFailure(new AssertionError(message));
      }

      /// <summary>
      /// Provides an entry point for test code to inject bytes into the test driver which are
      /// decoded into AMQP or SASL performatives to drive the test interactions.
      /// </summary>
      /// <param name="input">Stream of bytes to decode</param>
      public void Ingest(Stream input)
      {
         logger.LogTrace("{0} processing new inbound buffer of size: {1}", driverName, input.Length - input.Position);

         try
         {
            // Process off all encoded frames from this buffer one at a time.
            while (input.Position < input.Length && failureCause == null)
            {
               logger.LogTrace("{0} ingesting {1} bytes.", driverName, input.Length - input.Position);
               frameParser.Ingest(input);
               logger.LogTrace("{0} ingestion completed cycle, remaining bytes in buffer: {1}", driverName, input.Length - input.Position);
            }
         }
         catch (Exception e)
         {
            SignalFailure(e);
         }
      }

      #region Trigger sends of Header, AMQP and SASL frames to connected peers

      internal virtual void SendHeader(AMQPHeader header)
      {
         logger.LogTrace("{0} Sending AMQP Header: {1}", driverName, header);
         mutex.WaitOne();
         try
         {
            frameConsumer.Invoke(new MemoryStream(header.ToArray()));
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not consumed due to error.", ex));
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      internal virtual void SendAMQPFrame(ushort channel, IDescribedType performative, byte[] payload)
      {
         logger.LogTrace("{0} Sending performative: {1}", driverName, performative);

         mutex.WaitOne();
         try
         {
            if (performative is PerformativeDescribedType type)
            {
               switch (type.Type)
               {
                  case PerformativeType.Open:
                     localOpen = (Open)performative;
                     break;
               }
            }

            MemoryStream stream = new();
            frameEncoder.HandleWrite(stream, performative, channel, payload, null);
            logger.LogTrace("{0} Writing out buffer of size:{1} to consumer: {2}", driverName, stream.Length, frameConsumer);
            frameConsumer.Invoke(stream);
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not written due to error.", ex));
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      internal virtual void SendSaslFrame<T>(ushort channel, T performative) where T : IDescribedType
      {
         mutex.WaitOne();
         try
         {
            // When the outcome of SASL is written the decoder should revert to initial state
            // as the only valid next incoming value is an AMQP header.
            if (performative is SaslOutcome)
            {
               frameParser.ResetToExpectingHeader();
            }

            logger.LogTrace("{0} Sending sasl performative: {1}", driverName, performative);
            MemoryStream stream = new();
            frameEncoder.HandleWrite(stream, performative, channel);
            frameConsumer.Invoke(stream);
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not written due to error.", ex));
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      public virtual void SendEmptyFrame(ushort channel)
      {
         mutex.WaitOne();
         try
         {
            MemoryStream stream = new();
            frameEncoder.HandleWrite(stream, null, channel, null, null);
            frameConsumer.Invoke(stream);
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not consumed due to error.", ex));
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      internal virtual void SendBytes(byte[] buffer)
      {
         logger.LogTrace("{0} Sending bytes from ProtonBuffer: {1}", driverName, buffer);
         mutex.WaitOne();
         try
         {
            frameConsumer.Invoke(new MemoryStream(buffer));
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Buffer was not consumed due to error.", ex));
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      #endregion

      #region Handlers for frame events

      internal void HandleConnectedEstablished()
      {
         mutex.WaitOne();
         try
         {
            if (script.TryPeek(out IScriptedElement peekNext))
            {
               if (peekNext.ScriptedType == ScriptEntryType.Action)
               {
                  ProcessScript(peekNext);
               }
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      public void HandleHeader(AMQPHeader header)
      {
         mutex.WaitOne();
         try
         {

            if (!script.TryDequeue(out IScriptedElement scriptEntry))
            {
               SignalFailure(new AssertionError("Received header when not expecting any input."));
            }
            else if (scriptEntry is ScriptedExpectation expectation)
            {
               try
               {
                  header.Invoke(expectation, this);
               }
               catch (Exception t)
               {
                  if (expectation.IsOptional)
                  {
                     HandleHeader(header);
                  }
                  else
                  {
                     logger.LogWarning(t.Message);
                     SignalFailure(t);
                     throw;
                  }
               }
            }
            else
            {
               SignalFailure(new AssertionError("Received header when not expecting to perform some action or other script item."));
            }

            ProcessScript(scriptEntry);
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      public void HandleSaslPerformative(uint frameSize, SaslDescribedType sasl, ushort channel, byte[] payload)
      {
         mutex.WaitOne();
         try
         {
            saslPerformativeCount++;

            if (!script.TryDequeue(out IScriptedElement scriptEntry))
            {
               SignalFailure(new AssertionError("Received SASL performative when not expecting any input."));
            }
            else if (scriptEntry is ScriptedExpectation expectation)
            {
               try
               {
                  // When the outcome of SASL is read the decoder should revert to initial state
                  // as the only valid next incoming value is an AMQP header.
                  if (sasl is SaslOutcome)
                  {
                     frameParser.ResetToExpectingHeader();
                  }

                  sasl.Invoke(expectation, frameSize, payload, channel, this);
               }
               catch (UnexpectedPerformativeError unexpected)
               {
                  if (expectation.IsOptional)
                  {
                     HandleSaslPerformative(frameSize, sasl, channel, payload);
                  }
                  else
                  {
                     logger.LogWarning(unexpected.Message);
                     SignalFailure(unexpected);
                     throw;
                  }
               }
               catch (Exception assertion)
               {
                  logger.LogWarning(assertion.Message);
                  SignalFailure(assertion);
                  throw;
               }
            }
            else
            {
               SignalFailure(new AssertionError(
                  "Received SASL performative when not expecting to perform some action or other script item."));
            }

            ProcessScript(scriptEntry);
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      public void HandlePerformative(uint frameSize, PerformativeDescribedType amqp, ushort channel, byte[] payload)
      {
         mutex.WaitOne();

         switch (amqp.Type)
         {
            case PerformativeType.Heartbeat:
               break;
            case PerformativeType.Open:
               remoteOpen = (Open)amqp;
               performativeCount++;
               break;
            default:
               performativeCount++;
               break;
         }

         try
         {
            if (!script.TryDequeue(out IScriptedElement scriptEntry))
            {
               SignalFailure(new AssertionError(
                  "Received AMQP performative when not expecting any input: " + amqp?.GetType().Name));
            }
            else if (scriptEntry is ScriptedExpectation expectation)
            {
               try
               {
                  amqp.Invoke(expectation, frameSize, payload, channel, this);
               }
               catch (UnexpectedPerformativeError unexpected)
               {
                  if (expectation.IsOptional)
                  {
                     HandlePerformative(frameSize, amqp, channel, payload);
                  }
                  else
                  {
                     logger.LogWarning(unexpected.Message);
                     SignalFailure(unexpected);
                     throw;
                  }
               }
               catch (Exception assertion)
               {
                  logger.LogWarning(assertion.Message);
                  SignalFailure(assertion);
                  throw;
               }
            }
            else
            {
               SignalFailure(new AssertionError(
                  "Received AMQP performative when not expecting to perform some action or other script item: " +
                  amqp?.GetType().Name));
            }

            ProcessScript(scriptEntry);
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      public void HandleHeartbeat(uint frameSize, ushort channel)
      {
         mutex.WaitOne();
         try
         {
            emptyFrameCount++;
            HandlePerformative(frameSize, Heartbeat.INSTANCE, channel, null);
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      #endregion

      #region Wait methods for tests to block until script completed

      /// <summary>
      /// Waits indefinitely for the scripted expectations and actions to be performed.
      /// If the script execution encounters an error this method will throw an
      /// AssertionError that describes the error.
      /// </summary>
      public void WaitForScriptToComplete()
      {
         CheckFailed();

         ScriptCompleteAction possibleWait = null;

         mutex.WaitOne();
         try
         {
            CheckFailed();
            if (script.Count > 0)
            {
               logger.LogDebug("{0} Test driver queueing script completion action, behind {1} entries", Name, script.Count);
               possibleWait = new ScriptCompleteAction(this).Queue();
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }

         if (possibleWait != null)
         {
            try
            {
               possibleWait.Await();
            }
            catch (Exception)
            {
               SignalFailure("Interrupted while waiting for script to complete");
            }
         }

         CheckFailed();
      }

      /**
       * Waits indefinitely for the scripted expectations and actions to be performed.  If the script
       * execution encounters an error this method will not throw an {@link AssertionError} that describes
       * the error but simply ignore it and return.
       */
      public void WaitForScriptToCompleteIgnoreErrors()
      {
         ScriptCompleteAction possibleWait = null;

         mutex.WaitOne();
         try
         {
            if (script.Count > 0)
            {
               possibleWait = new ScriptCompleteAction(this).Queue();
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }

         if (possibleWait != null)
         {
            try
            {
               possibleWait.Await();
            }
            catch (Exception)
            {
               SignalFailure("Interrupted while waiting for script to complete");
            }
         }
      }

      /// <summary>
      /// Waits for the given amount of time for the scripted expectations and actions to be performed.
      /// If the script execution encounters an error this method will throw an AssertionError that
      /// describes the error.
      /// </summary>
      /// <param name="timeout">Time in milliseconds to wait for the script to complete</param>
      public void WaitForScriptToComplete(long timeout)
      {
         WaitForScriptToComplete(TimeSpan.FromMilliseconds(timeout));
      }

      /// <summary>
      /// Waits for the given amount of time for the scripted expectations and actions to be performed.
      /// If the script execution encounters an error this method will throw an AssertionError that
      /// describes the error.
      /// </summary>
      /// <param name="timeout">The time to wait for the scripted events to occur</param>
      public void WaitForScriptToComplete(TimeSpan timeout)
      {
         CheckFailed();

         ScriptCompleteAction possibleWait = null;

         mutex.WaitOne();
         try
         {
            CheckFailed();
            if (script.Count > 0)
            {
               possibleWait = new ScriptCompleteAction(this).Queue();
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }

         if (possibleWait != null)
         {
            try
            {
               possibleWait.Await(timeout);
            }
            catch (Exception)
            {
               SignalFailure("Interrupted while waiting for script to complete");
            }
         }

         CheckFailed();
      }

      #endregion

      #region Internal Test Driver Utilities

      private void SearchForScriptCompletionAndTrigger()
      {
         foreach (IScriptedElement element in script)
         {
            if (element is ScriptCompleteAction completed)
            {
               completed.Perform(this);
            }
         }
      }

      private void ProcessScript(IScriptedElement current)
      {
         if (current is ScriptedExpectation expectation)
         {
            while (expectation.PerformAfterwards() != null && failureCause == null)
            {
               expectation.PerformAfterwards().Perform(this);
            }
         }

         script.TryPeek(out IScriptedElement peekNext);

         do
         {
            if (peekNext is ScriptedAction action)
            {
               script.Dequeue();
               action.Perform(this);
            }
            else
            {
               return;
            }

            script.TryPeek(out peekNext);
         }
         while (peekNext != null && failureCause == null);
      }

      private void CheckFailed()
      {
         if (failureCause != null)
         {
            throw failureCause;
         }
      }

      #endregion
   }
}