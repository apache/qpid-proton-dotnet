/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Apache.Qpid.Proton.Client.Concurrent
{
   /// <summary>
   /// Single threaded event processing loop interface. Implementations
   /// accept queue'd actions to be processed within the event loop in
   /// serial fashion.
   /// </summary>
   public interface IEventLoop
   {
      /// <summary>
      /// Returns if the code currently executing is operating within the context
      /// of the event loop thread or not.
      /// </summary>
      bool InEventLoop { get; }

      /// <summary>
      /// Execute some action at a future time in order of submission.  The event
      /// loop implementation must guarantee that events never execute concurrently
      /// or out of order.
      /// </summary>
      /// <param name="action">The action to be performed</param>
      /// <exception cref="ArgumentNullException">If the provided action is null</exception>
      /// <exception cref="RejectedExecutionException">If the action is rejected</exception>
      void Execute(Action action);

      /// <summary>
      /// Returns true if the service has been shutdown.
      /// </summary>
      bool IsShutdown { get; }

      /// <summary>
      /// Returns true if a service that was shutdown has completed all shutdown operations.
      /// </summary>
      bool IsTerminated { get; }

      /// <summary>
      /// Allows a caller to wait the given time span for the service to completely shut
      /// down and terminate all service operations.  If the wait return before the service
      /// has fully shutdown it will return false.
      /// </summary>
      /// <param name="waitTime">The time to wait for complete shutdown</param>
      /// <returns>true if the service fully shut down, otherwise returns false.</returns>
      bool WaitForTermination(TimeSpan waitTime);

      /// <summary>
      /// Requests an orderly shut down of the service whereby previously submitted tasks
      /// will still be allowed to execute but any new tasks will be rejected.
      /// </summary>
      void Shutdown();

   }
}
