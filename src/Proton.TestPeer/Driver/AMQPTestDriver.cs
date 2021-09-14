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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Test.Driver.Actions;
using Apache.Qpid.Proton.Test.Driver.Codec.Primitives;
using Apache.Qpid.Proton.Test.Driver.Codec.Security;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// The AMQP Test driver internal frame processing a script handler class.
   /// </summary>
   public sealed class AMQPTestDriver
   {
      private readonly Mutex mutex = new Mutex();
      private readonly String driverName;
      private readonly FrameDecoder frameParser;
      private readonly FrameEncoder frameEncoder;

      private Open localOpen;
      private Open remoteOpen;

      private readonly DriverSessions sessions;

      private readonly Action<byte[]> frameConsumer;
      private readonly Action<Exception> assertionConsumer;
      private readonly Func<TaskFactory> taskFactorySupplier;

      private volatile Exception failureCause;

      private uint advertisedIdleTimeout = 0;

      private volatile uint emptyFrameCount;

      private volatile uint performativeCount;

      private volatile uint saslPerformativeCount;

      private uint inboundMaxFrameSize = UInt32.MaxValue;
      private uint outboundMaxFrameSize = UInt32.MaxValue;

      /// <summary>
      /// Holds the expectations for processing of data from the peer under test.
      /// Uses a thread safe queue to avoid contention on adding script entries
      /// and processing incoming data (although you should probably not do that).
      /// </summary>
      private readonly Queue<IScriptedElement> script = new Queue<IScriptedElement>();

      public AMQPTestDriver(string name, Action<byte[]> frameConsumer, Func<TaskFactory> scheduler) :
         this(name, frameConsumer, null, scheduler)
      {
      }

      public AMQPTestDriver(string name, Action<byte[]> frameConsumer, Action<Exception> assertConsumer, Func<TaskFactory> scheduler)
      {
         this.sessions = new DriverSessions(this);
         this.frameConsumer = frameConsumer;
         this.taskFactorySupplier = scheduler;
         this.assertionConsumer = assertConsumer;

         this.frameEncoder = new FrameEncoder(this);
         this.frameParser = new FrameDecoder(this);
      }

      internal DriverSessions Sessions => sessions;

      public string Name => driverName;

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
         if (taskFactorySupplier == null)
         {
            throw new InvalidOperationException("This driver cannot schedule delayed events, no scheduler available");
         }

         TaskFactory factory = taskFactorySupplier.Invoke();

         if (factory == null)
         {
            throw new InvalidOperationException("This driver cannot schedule delayed events, no scheduler available");
         }

         // TODO : Schedule not just run
         factory.StartNew(() =>
         {
            // TODO : LOG.trace("{} running delayed action: {}", driverName, action);
            action.Perform(this);
         });
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
         if (this.failureCause == null)
         {
            if (ex is AssertionError)
            {
               // TODO LOG.trace("{} sending failure assertion due to: ", driverName, ex);
               this.failureCause = (AssertionError)ex;
            }
            else
            {
               // TODO LOG.trace("{} sending failure assertion due to: ", driverName, ex);
               this.failureCause = new AssertionError(ex.Message, ex);
            }

            SearchForScriptCompletionAndTrigger();

            if (assertionConsumer != null)
            {
               assertionConsumer.Invoke(failureCause);
            }
         }
      }

      /// <summary>
      /// Throw an exception from processing incoming data which should be handled by the peer under test.
      /// </summary>
      /// <param name="message">The message used to create the assertion error</param>
      public void SignalFailure(String message)
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
         // TODO : LOG.trace("{} processing new inbound buffer of size: {}", driverName, buffer.readableBytes());

         try
         {
            // Process off all encoded frames from this buffer one at a time.
            while (input.Position < input.Length && failureCause == null)
            {
               // TODO : LOG.trace("{} ingesting {} bytes.", driverName, buffer.readableBytes());
               frameParser.Ingest(input);
               // TODO : LOG.trace("{} ingestion completed cycle, remaining bytes in buffer: {}", driverName, buffer.readableBytes());
            }
         }
         catch (Exception e)
         {
            SignalFailure(e);
         }
      }

      #region Trigger sends of Header, AMQP and SASL frames to connected peers

      internal void SendHeader(AMQPHeader header)
      {
         // TODO : LOG.trace("{} Sending AMQP Header: {}", driverName, header);
         try
         {
            frameConsumer.Invoke(header.ToArray());
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not consumed due to error.", ex));
         }
      }

      internal void SendAMQPFrame(ushort channel, IDescribedType performative, byte[] payload)
      {
         // TODO : LOG.trace("{} Sending performative: {}", driverName, performative);

         if (performative is PerformativeDescribedType)
         {
            switch (((PerformativeDescribedType)performative).Type)
            {
               case PerformativeType.Open:
                  localOpen = (Open)performative;
                  break;
            }
         }

         try
         {
            MemoryStream stream = new MemoryStream();
            frameEncoder.HandleWrite(stream, performative, channel, payload, null);
            // TODO : LOG.trace("{} Writing out buffer {} to consumer: {}", driverName, buffer, frameConsumer);
            frameConsumer.Invoke(stream.ToArray());
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not written due to error.", ex));
         }
      }

      internal void SendSaslFrame<T>(ushort channel, T performative) where T : IDescribedType
      {
         // When the outcome of SASL is written the decoder should revert to initial state
         // as the only valid next incoming value is an AMQP header.
         if (performative is SaslOutcome)
         {
            frameParser.ResetToExpectingHeader();
         }

         // TODO : LOG.trace("{} Sending sasl performative: {}", driverName, performative);

         try
         {
            MemoryStream stream = new MemoryStream();
            frameEncoder.HandleWrite(stream, performative, channel);
            frameConsumer.Invoke(stream.ToArray());
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not written due to error.", ex));
         }
      }

      public void SendEmptyFrame(ushort channel)
      {
         MemoryStream stream = new MemoryStream();
         frameEncoder.HandleWrite(stream, null, channel, null, null);

         try
         {
            frameConsumer.Invoke(stream.ToArray());
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Frame was not consumed due to error.", ex));
         }
      }

      internal void SendBytes(byte[] buffer)
      {
         // TODO : LOG.trace("{} Sending bytes from ProtonBuffer: {}", driverName, buffer);
         try
         {
            frameConsumer.Invoke(buffer);
         }
         catch (Exception ex)
         {
            SignalFailure(new AssertionError("Buffer was not consumed due to error.", ex));
         }
      }

      #endregion

      #region Handlers for frame events

      internal void HandleConnectedEstablished()
      {
         mutex.WaitOne();
         try
         {
            IScriptedElement peekNext = script.Peek();
            if (peekNext.ScriptedType == ScriptEntryType.Action)
            {
               ProcessScript(peekNext);
            }
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      internal void HandleHeader(AMQPHeader header)
      {
         mutex.WaitOne();
         try
         {
            IScriptedElement scriptEntry = script.Dequeue();

            if (scriptEntry == null)
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
                     // TODO LOG.warn(t.getMessage());
                     SignalFailure(t);
                     throw t;
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

      internal void HandleSaslPerformative(uint frameSize, SaslDescribedType sasl, ushort channel, byte[] payload)
      {
         mutex.WaitOne();
         try
         {
            IScriptedElement scriptEntry = script.Dequeue();

            if (scriptEntry == null)
            {
               SignalFailure(new AssertionError("Received SASL performative when not expecting any input."));
            }
            else if (scriptEntry is ScriptedExpectation expectation)
            {
               try
               {
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
                     // TODO LOG.warn(t.getMessage());
                     SignalFailure(unexpected);
                     throw unexpected;
                  }
               }
               catch (Exception assertion)
               {
                  // TODO LOG.warn(assertion.getMessage());
                  SignalFailure(assertion);
                  throw assertion;
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

      internal void HandlePerformative(uint frameSize, PerformativeDescribedType amqp, ushort channel, byte[] payload)
      {
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

         mutex.WaitOne();
         try
         {
            IScriptedElement scriptEntry = script.Dequeue();

            if (scriptEntry == null)
            {
               SignalFailure(new AssertionError("Received AMQP performative when not expecting any input."));
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
                     // TODO LOG.warn(t.getMessage());
                     SignalFailure(unexpected);
                     throw unexpected;
                  }
               }
               catch (Exception assertion)
               {
                  // TODO LOG.warn(assertion.getMessage());
                  SignalFailure(assertion);
                  throw assertion;
               }
            }
            else
            {
               SignalFailure(new AssertionError(
                  "Received AMQP performative when not expecting to perform some action or other script item."));
            }

            ProcessScript(scriptEntry);
         }
         finally
         {
            mutex.ReleaseMutex();
         }
      }

      internal void HandleHeartbeat(uint frameSize, ushort channel)
      {
         emptyFrameCount++;
         HandlePerformative(frameSize, Heartbeat.INSTANCE, channel, null);
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
            if (element is ScriptCompleteAction)
            {
               ScriptCompleteAction completed = (ScriptCompleteAction)element;
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

         IScriptedElement peekNext = script.Peek();
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

            peekNext = script.Peek();
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