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

namespace Apache.Qpid.Proton.Common.Concurrent
{
   public interface IThreadFactory
   {
      /// <summary>
      /// Creates a new thread and configures the handler that will
      /// be called when the thread is started.
      /// </summary>
      /// <param name="threadStartedHandler">handler to call on start</param>
      /// <returns>A new thread</returns>
      Thread NewThread(Action<Object> threadStartedHandler);

      /// <summary>
      /// Creates a new thread and configures the handler that will
      /// be called when the thread is started.
      /// </summary>
      /// <param name="threadStartedHandler">handler to call on start</param>
      /// <returns>A new thread</returns>
      Thread NewThread(Action<Object> threadStartedHandler, string threadName);

   }
}