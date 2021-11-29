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
   /// Exception that is thrown when a "Task" or "Action" that is submitted to
   /// an executor implementation is rejected for some reason such as capacity
   /// limits being hit or the executor being shut down.
   /// </summary>
   public sealed class RejectedExecutionException : Exception
   {
      /// <summary>
      /// Create a basic exception with no additional error information
      /// </summary>
      public RejectedExecutionException()
      {
      }

      /// <summary>
      /// Create a rejected execution exception with the provided message.
      /// </summary>
      /// <param name="message">The message to convey with this exception</param>
      public RejectedExecutionException(string message) : base(message)
      {
      }

      /// <summary>
      /// Create a rejected execution exception with the provided message and an
      /// inner exception which will be convery along with this exception instance.
      /// </summary>
      /// <param name="message">The message to convey with this exception</param>
      /// <param name="innerException">A wrapped causal exception </param>
      public RejectedExecutionException(string message, Exception innerException) : base(message, innerException)
      {
      }
   }
}