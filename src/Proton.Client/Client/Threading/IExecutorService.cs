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
using System.Threading;
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client.Threading
{
   /// <summary>
   /// Extension of the basic executor API which provides API to manage the
   /// lifetime and accessability of the service.  An executor service can be
   /// shutdown which leads to tasks being rejected and provides additional
   /// API for submitting and managing tasks that have been submitted.
   /// </summary>
   public interface IExecutorService : IExecutor
   {
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

      /// <summary>
      /// Submits a task for asynchronous execution and returns a Task type that will
      /// provide the result of the action or indicate an error if the task failed
      /// for some reason. The behavior of the task execution is governed by the
      /// implementation of the executor service.
      /// </summary>
      /// <typeparam name="T">The type of result the executed action provides</typeparam>
      /// <param name="action">The action to execute</param>
      /// <returns>A Task instance that provides the result of the action</returns>
      Task<T> SubmitAsync<T>(Func<T> action);

      /// <summary>
      /// Submits a task for asynchronous execution and returns a Task type that will
      /// provide the result of the action or indicate an error if the task failed
      /// for some reason. The behavior of the task execution is governed by the
      /// implementation of the executor service.
      /// </summary>
      /// <typeparam name="T">The type of result the executed action provides</typeparam>
      /// <param name="action">The action to execute</param>
      /// <param name="token">The cancellation token that can be used to attempt to cancel the task</param>
      /// <returns>A Task instance that provides the result of the action</returns>
      Task<T> SubmitAsync<T>(Func<T> action, CancellationToken token);

   }
}