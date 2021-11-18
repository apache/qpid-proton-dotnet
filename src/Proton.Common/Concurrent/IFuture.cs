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

namespace Apache.Qpid.Proton.Common.Concurrent
{
   /// <summary>
   /// Provides an opaque future type that can wrap both a Task and
   /// a cancellation token along with the mechanics to attempt task
   /// cancellation and Task complation
   /// </summary>
   public interface IFuture
   {
      /// <summary>
      /// Returns a task that can be waited on which allows for futures
      /// to be handed out to users outside the common API without exposing
      /// the management details of the future type.
      /// </summary>
      Task Task { get; }

      /// <summary>
      /// Returns true if the future has been completed under any of the
      /// routes to completion such as success, failure or cancellation.
      /// </summary>
      bool IsComplete { get; }

      /// <summary>
      /// For a completed future returns true if the outcome was successful.
      /// </summary>
      bool IsSuccess { get; }

      /// <summary>
      /// For a completed future returns true if the outcome was failure.
      /// </summary>
      bool IsFaulted { get; }

      /// <summary>
      /// For a completed future returns true if the outcome was cancellation.
      /// </summary>
      bool IsCanceled { get; }

      /// <summary>
      /// Try to set the future into an failed state with the provided Exception
      /// as the cause of the failure, if the future has already completed then
      /// this method returns false.
      /// </summary>
      /// <param name="exception">The cause of the failure</param>
      /// <returns>true if the future was failed before being completed</returns>
      bool TrySetException(Exception exception);

      /// <summary>
      /// Try to set the future into an failed state with the provided Exception
      /// as the cause of the failure, if the future has already completed then
      /// this method throws an invalid operation exception.
      /// </summary>
      /// <param name="exception">The cause of the failure</param>
      /// <exception cref="InvalidOperationException">If the future is already completed</exception>
      void SetException(Exception exception);

      /// <summary>
      /// Try to cancel the operation represented by this future if not already
      /// completed.  If the future has already been completed (successfully or not)
      /// this method return false.
      /// </summary>
      /// <returns>true if the future was cancelled before being completed</returns>
      bool TryCancel();

      /// <summary>
      /// Try to cancel the operation represented by this future if not already
      /// completed.  If the future has already been completed (successfully or not)
      /// this method throws and invalid operation exception.
      /// </summary>
      /// <exception cref="InvalidOperationException">If the future is already completed</exception>
      void Cancel();

   }
}