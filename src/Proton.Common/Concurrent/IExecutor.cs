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

namespace Apache.Qpid.Proton.Common.Concurrent
{
   /// <summary>
   /// An executor of submitted tasks that decouples the mechanism of running the
   /// submitted task from the caller for the means by which tasks are submitted
   /// for execution.
   /// </summary>
   public interface IExecutor
   {
      /// <summary>
      /// Execute some action at a future time based on the implementation of the executor.
      /// The execution can occur on a new thread or it might be using a thread pool and in
      /// some implementation might execute on the callers thread.
      /// </summary>
      /// <param name="action">The action to be performed</param>
      /// <exception cref="ArgumentNullException">If the provided action is null</exception>
      /// <exception cref="RejectedExecutionException">If the action is rejected</exception>
      void Execute(Action action);

   }
}