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
   /// An extension of the basic executor service API that allows for the
   /// scheduling of tasks either with a given delay or tasks that must
   /// repeat periodically.
   /// </summary>
   public interface IScheduledExecutorService : IExecutorService
   {
      /// <summary>
      /// Schedules the future execution of the given action and returns a Task
      /// that can be used to track the outcome.
      /// </summary>
      /// <param name="action">The action to be performed after the given delay</param>
      /// <param name="delay">The delay before the given action should be performed</param>
      /// <returns>A task that can be used to track the outcome</returns>
      Task ScheduleAsync(Action action, TimeSpan delay);

      /// <summary>
      /// Schedules the future execution of the given action and returns a Task
      /// that can be used to track the outcome.
      /// </summary>
      /// <param name="action">The action to be performed after the given delay</param>
      /// <param name="delay">The delay before the given action should be performed</param>
      /// <param name="token">A cancellation token that can be used to try to cancel the scheduled action</param>
      /// <returns>A task that can be used to track the outcome</returns>
      Task ScheduleAsync(Action action, TimeSpan delay, CancellationToken token);

      /// <summary>
      /// Schedules the future execution of the given action and returns a Task
      /// that can be used to track the outcome.  The task will be repeated at a fixed
      /// rate specified by the caller.
      /// </summary>
      /// <remarks>
      /// The time between the completion of one iteration of the action and the next with
      /// this API is based on the given delay minus the time it takes for the task to complete.
      /// Executions of the task may bunch up if the time to complete the task is longer than
      /// the delay, however the execution of one iteration of the action and the next will
      /// never overlap.
      /// </remarks>
      /// <param name="action">The action to be performed</param>
      /// <param name="delay">The initial delay before the first execution of the action</param>
      /// <param name="period">The period that define the repeat rate of the action</param>
      /// <returns>A task that can be used to track the outcome</returns>
      Task ScheduleAtFixedRateAsync(Action action, TimeSpan delay, TimeSpan period);

      /// <summary>
      /// Schedules the future execution of the given action and returns a Task
      /// that can be used to track the outcome.  The task will be repeated at a fixed
      /// rate specified by the caller.
      /// </summary>
      /// <remarks>
      /// The time between the completion of one iteration of the action and the next with
      /// this API is based on the given delay minus the time it takes for the task to complete.
      /// Executions of the task may bunch up if the time to complete the task is longer than
      /// the delay, however the execution of one iteration of the action and the next will
      /// never overlap.
      /// </remarks>
      /// <param name="action">The action to be performed</param>
      /// <param name="delay">The initial delay before the first execution of the action</param>
      /// <param name="period">The period that define the repeat rate of the action</param>
      /// <param name="token">A cancellation token that can be used to try to cancel the scheduled action</param>
      /// <returns>A task that can be used to track the outcome</returns>
      Task ScheduleAtFixedRateAsync(Action action, TimeSpan delay, TimeSpan period, CancellationToken token);

      /// <summary>
      /// Schedules the future execution of the given action and returns a Task
      /// that can be used to track the outcome.  The task will be repeated at a fixed
      /// delay specified by the caller.
      /// </summary>
      /// <remarks>
      /// The time between the completion of one iteration of the action and the next with
      /// this API is fixed and executions do not bunch or overlap.
      /// </remarks>
      /// <param name="action">The action to be performed</param>
      /// <param name="delay">The initial delay before the first execution of the action</param>
      /// <param name="period">The period that define the repeat rate of the action</param>
      /// <returns>A task that can be used to track the outcome</returns>
      Task ScheduleAtFixedDelayAsync(Action action, TimeSpan delay, TimeSpan period);

      /// <summary>
      /// Schedules the future execution of the given action and returns a Task
      /// that can be used to track the outcome.  The task will be repeated at a fixed
      /// delay specified by the caller.
      /// </summary>
      /// <remarks>
      /// The time between the completion of one iteration of the action and the next with
      /// this API is fixed and executions do not bunch or overlap.
      /// </remarks>
      /// <param name="action">The action to be performed</param>
      /// <param name="delay">The initial delay before the first execution of the action</param>
      /// <param name="period">The period that define the repeat rate of the action</param>
      /// <param name="token">A cancellation token that can be used to try to cancel the scheduled action</param>
      /// <returns>A task that can be used to track the outcome</returns>
      Task ScheduleAtFixedDelayAsync(Action action, TimeSpan delay, TimeSpan period, CancellationToken token);

   }
}