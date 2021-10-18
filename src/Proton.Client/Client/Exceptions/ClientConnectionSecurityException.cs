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
   /// Connection level Security Exception used to indicate a security violation has occurred.
   /// </summary>
   public class ClientConnectionSecurityException : ClientConnectionRemotelyClosedException
   {
      /// <summary>
      /// Creates a new connection security exception.
      /// </summary>
      /// <param name="message">The message that describes the reason for the security error.</param>
      public ClientConnectionSecurityException(string message) : base(message)
      {
      }

      /// <summary>
      /// Creates a new connection security exception.
      /// </summary>
      /// <param name="message">The message that describes the reason for the security error.</param>
      /// <param name="innerException">An exception that further defines the reason for the security error.</param>
      public ClientConnectionSecurityException(string message, Exception innerException) : base(message, innerException)
      {
      }

      /// <summary>
      /// Creates a new connection security exception.
      /// </summary>
      /// <param name="message">The message that describes the reason for the security error.</param>
      /// <param name="errorCondition">An ErrorCondition that provides additional information about the error.</param>
      public ClientConnectionSecurityException(string message, IErrorCondition errorCondition) : base(message, errorCondition)
      {
      }

      /// <summary>
      /// Creates a new connection security exception.
      /// </summary>
      /// <param name="message">The message that describes the reason for the security error.</param>
      /// <param name="innerException">An exception that further defines the reason for the security error.</param>
      /// <param name="errorCondition">An ErrorCondition that provides additional information about the error.</param>
      public ClientConnectionSecurityException(string message, Exception innerException, IErrorCondition errorCondition) : base(message, innerException, errorCondition)
      {
      }
   }
}