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
   /// Represents a non-fatal exception that occurs from a Client connection or one
   /// of its resources.  These error types can typically be recovered from without a
   /// full tear down and rebuild of the connection.  One example might be a failure
   /// to commit a transaction due to a forced roll back on the remote side of the
   /// connection.
   /// </summary>
   public class ClientException : Exception
   {
      /// <summary>
      /// Creates an instance of this exception with the given message
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      public ClientException(string message) : base(message)
      {
      }

      /// <summary>
      /// Creates an instance of this exception with the given message and
      /// linked causal exception.
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      /// <param name="innerException">The exception that caused this error</param>
      public ClientException(string message, Exception innerException) : base(message, innerException)
      {
      }
   }
}