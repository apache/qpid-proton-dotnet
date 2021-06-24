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
      bool IsRunning();

      /// <summary>
      /// Checks if the engine has been shutdown which is a terminal state
      /// after which no future engine state changes can occur.
      /// </summary>
      /// <returns></returns>
      bool IsShutdown();

      /// <summary>
      /// Checks if the engine has entered a failed state either by a call to the
      /// engine failed method or by an exception having been thrown and caught.
      /// An engine that reports failed will stop after a call to shutdown.
      /// </summary>
      /// <returns>true if the engine is in a failed state</returns>
      bool IsFailed();

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
      /// <returns>This Engine instance</returns>
      /// <exception cref="EngineStateException">If the engine state is already failed or shutdown</exception>
      IEngine Start();

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

   }
}