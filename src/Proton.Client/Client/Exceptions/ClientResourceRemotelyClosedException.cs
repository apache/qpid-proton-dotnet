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

namespace Apache.Qpid.Proton.Client.Exceptions
{
   /// <summary>
   /// Root exception type for cases of remote closure or client created resources
   /// other than the Client Connection} which will throw exceptions rooted from the
   /// ClientConnectionRemotelyClosedException to indicate a fatal connection level
   /// error that requires a new connection to be created.
   /// </summary>
   public class ClientResourceRemotelyClosedException : ClientIllegalStateException
   {
      private readonly IErrorCondition errorCondition;

      /// <summary>
      /// Creates an instance of this exception with the given message
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      public ClientResourceRemotelyClosedException(string message) : base(message)
      {
      }

      /// <summary>
      /// Creates an instance of this exception with the given message and
      /// linked causal exception.
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      /// <param name="innerException">The exception that caused this error</param>
      public ClientResourceRemotelyClosedException(string message, Exception innerException) : base(message, innerException)
      {
      }

      /// <summary>
      /// Creates an instance of this exception with the given message
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      public ClientResourceRemotelyClosedException(string message, IErrorCondition errorCondition) : base(message)
      {
         this.errorCondition = errorCondition;
      }

      /// <summary>
      /// Creates an instance of this exception with the given message and
      /// linked causal exception.
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      /// <param name="innerException">The exception that caused this error</param>
      public ClientResourceRemotelyClosedException(string message, Exception innerException, IErrorCondition errorCondition) : base(message, innerException)
      {
         this.errorCondition = errorCondition;
      }

      /// <summary>
      /// Return the error provided by the remote if any.
      /// </summary>
      public IErrorCondition Error => errorCondition;

   }
}