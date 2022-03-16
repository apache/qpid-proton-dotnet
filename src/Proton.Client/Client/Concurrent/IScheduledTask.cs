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
using System.Threading.Tasks;

namespace Apache.Qpid.Proton.Client.Concurrent
{
   /// <summary>
   /// Represents a scheduled item of work that will be run on the
   /// event loop either once or on a repeating basis. The task allows
   /// for easy cancelation and for asynchronous awaits via the return
   /// completion Task.
   /// </summary>
   public interface IScheduledTask
   {
      /// <summary>
      /// Request to cancel the scheduled task if possible, returns true
      /// if the task was canceled before it ran, false otherwise.  If the
      /// Task is a periodic variant then all future executions are cancelled
      /// as well.
      /// </summary>
      /// <returns>true if cancelled, false otherwise</returns>
      bool Cancel();

      /// <summary>
      /// Returns the delay time in as high of precision as possible for
      /// the time the task executes and the next iteration if this task
      /// is periodic.
      /// </summary>
      long Delay { get; }

      /// <summary>
      /// Returns the next time to run in as high of precision value as
      /// possible in the current environment.
      /// </summary>
      long Deadline { get; }

      /// <summary>
      /// A Task that can be awaited for scheduled work that is not periodic
      /// in nature.
      /// </summary>
      Task CompletionTask { get; }

   }
}