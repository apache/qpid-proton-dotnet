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

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Defines an AMQP Protocol Engine interface that should be used to implement
   /// an AMQP Engine.
   /// </summary>
   public interface IEngine
   {
      /// <summary>
      /// Checks if the engine is in the running state and has not failed or been
      /// shutdown yet. Will return false until start is called on the engine.
      /// </summary>
      /// <returns>true if the engine is currently running.</returns>
      bool IsRunning { get; }

      /// <summary>
      /// Returns true if the engine is accepting input from the ingestion entry points.
      /// <para>
      /// When false any attempts to write more data into the engine will result in an
      /// error being returned from the write operation. An engine that has not been
      /// started or that has been failed or shutdown will report as not writable.
      /// </summary>
      bool IsWritable { get; }

      /// <summary>
      /// Checks if the engine has been shutdown which is a terminal state
      /// after which no future engine state changes can occur.
      /// </summary>
      /// <returns></returns>
      bool IsShutdown { get; }

      /// <summary>
      /// Checks if the engine has entered a failed state either by a call to the
      /// engine failed method or by an exception having been thrown and caught.
      /// An engine that reports failed will stop after a call to shutdown.
      /// </summary>
      /// <returns>true if the engine is in a failed state</returns>
      bool IsFailed { get; }

      /// <summary>
      /// Provides an Exception that has information regarding the cause of an
      /// engine entering the failed state.
      /// </summary>
      Exception FailureCause { get; }

      /// <summary>
      /// Provides the current engine operating state.
      /// </summary>
      EngineState EngineState { get; }

      /// <summary>
      /// Provides access to the configuration object associated with this engine.
      /// </summary>
      IEngineConfiguration Configuration { get; }

      /// <summary>
      /// Provides access to the engine pipeline instance associated with this engine.
      /// </summary>
      IEnginePipeline Pipeline { get; }

      /// <summary>
      /// Provides access to the SASL driver that is assigned to this engine.
      /// </summary>
      IEngineSaslDriver SaslDriver { get; }

      /// <summary>
      /// Gets the Connection instance that is associated with this Engine instance.
      /// It is valid for an engine implementation to not return a Connection instance
      /// prior to the engine having been started although it is recommended that one
      /// be available immediately to prevent confusion.
      /// </summary>
      IConnection Connection { get; }

      /// <summary>
      /// Starts the engine and returns the Connection instance that is bound to this
      /// Engine. A non-started Engine will not allow ingestion of any inbound data and
      /// a Connection linked to the engine that was obtained from the engine cannot
      /// produce any outbound data.
      /// </summary>
      /// <returns>The Connection that is linked to the engine instance</returns>
      /// <exception cref="EngineStateException">If the engine state is already failed or shutdown</exception>
      IConnection Start();

      /// <summary>
      /// Shutdown the engine preventing any future outbound or inbound processing.
      /// </summary>
      /// <remarks>
      /// When the engine is shut down any resources, Connection, Session or Link instances
      /// that have an engine shutdown event handler registered will be notified and should
      /// react by locally closing that resource if they wish to ensure that the resource's
      /// local close event handler gets signaled if that resource is not already locally
      /// closed.
      /// </remarks>
      /// <returns>This Engine instance</returns>
      IEngine Shutdown();

      /// <summary>
      /// Transition the Engine to a failed state if not already closed or closing.
      /// </summary>
      /// <remarks>
      /// If called when the engine has not failed the engine will be transitioned to the
      /// failed state and the method will return an appropriate EngineFailedException that
      /// wraps the given cause.  If called after the engine was shutdown the method returns
      /// an EngineShutdownException indicating that the engine was already shutdown.
      /// Repeated calls to this method while the engine is in the failed state must not
      /// alter the original failure error or elicit new engine failed event notifications.
      /// </remarks>
      /// <param name="cause">The exception that led to the Engine being failed</param>
      /// <returns>The exception that caused the engine to be transitioned to the failed state</returns>
      EngineStateException EngineFailed(Exception cause);

      /// <summary>
      /// Provide data input for this Engine from some external source. If the engine is not
      /// writable when this method is called an EngineNotWritableException will be thrown
      /// unless the reason for the not writable state is due to engine failure or the engine
      /// already having been shut down in which case the appropriate EngineStateException
      /// will be thrown to indicate the reason.
      /// </summary>
      /// <param name="input">The binary data to ingest into the engine</param>
      /// <returns>This Engine instance</returns>
      /// <exception cref="EngineNotWritableException">If the engine is not currently accepting input</exception>
      /// <exception cref="EngineStateException">If the engine state is already failed or shutdown</exception>
      IEngine Ingest(IProtonBuffer input);

      /// <summary>
      /// Prompt the engine to perform idle-timeout/heartbeat handling, and return an absolute
      /// deadline in milliseconds that tick must again be called by/at, based on the provided
      /// current time in milliseconds, to ensure the periodic work is carried out as necessary.
      /// It is an error to call this method if the connection has not been opened.
      /// <para/>
      /// A returned deadline of 0 indicates there is no periodic work necessitating tick be called,
      /// e.g. because neither peer has defined an idle-timeout value.
      /// <para/>
      /// The provided milliseconds time values should be derived from a monotonic source such as
      /// a system tick counter to prevent wall clock changes leading to erroneous behavior. Note
      /// that for some monotonic time sources deadline could be a different sign than the originally
      /// given value, and so (if non-zero) the returned deadline should have the current time
      /// originally provided subtracted from it in order to establish a relative time delay to the
      /// next deadline.
      /// </summary>
      /// <param name="current">The current system tick count</param>
      /// <returns>the absolute deadline in milliseconds to next call tick by/at, or 0 if there is none</returns>
      /// <exception cref="InvalidOperationException">If the engine has already been set to auto tick</exception>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      long Tick(long current);

      /// <summary>
      /// Allows the engine to manage idle timeout processing by providing it the single threaded
      /// task scheduler where all transport work is done which ensures singled threaded access
      /// while removing the need for the client library or server application to manage calls to
      /// the tick processing methods.
      /// </summary>
      /// <param name="scheduler">The single threaded scheduler where are engine work is queued</param>
      /// <returns>This engine instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IEngine TickAuto(TaskScheduler scheduler);

      /// <summary>
      /// Sets a Action instance that will be notified when data from the engine is ready to
      /// be written to some output sink (socket etc). In the event of an error writing the data
      /// the handler should throw an error or if performed asynchronously the Engine should be
      /// marked failed via a call to the engine failed API.
      /// </summary>
      /// <remarks>
      /// This method allows for a handler to be registered that doesn't not need to invoke an
      /// output complete handler when done writing but does assume that any writes are complete
      /// once the handler returns.  If the provided handler does any sort of queuing of writes or
      /// otherwise does not immediately complete this could lead to out of memory or other errors
      /// as the engine will not be able to apply any write backpressure,
      /// </remarks>
      /// <param name="handler">The delegate that will be invoked when engine output is available</param>
      /// <returns>This engine instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IEngine OutputHandler(Action<IProtonBuffer> handler)
      {
         this.OutputHandler((buffer, action) =>
         {
            handler.Invoke(buffer);
            if (action != null)
            {
               action.Invoke();
            }
         });

         return this;
      }

      /// <summary>
      /// Sets a Action instance that will be notified when data from the engine is ready to
      /// be written to some output sink (socket etc).  The Action value provided to the handler
      /// (if non-null) should be invoked once the I/O operation has completely successfully. In
      /// the event of an error writing the data the handler should throw an error or if performed
      /// asynchronously the Engine should be marked failed via a call to the engine failed API.
      /// </summary>
      /// <param name="handler">The delegate that will be invoked when engine output is available</param>
      /// <returns>This engine instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IEngine OutputHandler(Action<IProtonBuffer, Action> handler);

      /// <summary>
      /// Sets a handler instance that will be notified when the engine encounters a fatal error.
      /// </summary>
      /// <param name="handler">The handler that will be invoked on an engine error state</param>
      /// <returns>This engine instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IEngine ErrorHandler(Action<IEngine> handler);

      /// <summary>
      /// Sets a handler instance that will be notified when the engine is shut down via a call to
      /// the engine shutdown method.
      /// </summary>
      /// <param name="handler">The handler that will be signalled on engine shutdown</param>
      /// <returns>This engine instance</returns>
      /// <exception cref="EngineStateException">If the engine has failed or was shutdown</exception>
      IEngine ShutdownHandler(Action<IEngine> handler);

   }
}