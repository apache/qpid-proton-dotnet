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
   /// Connection level SASL Security Exception used to indicate a security violation has occurred.
   /// </summary>
   public class ClientConnectionSecuritySaslException : ClientConnectionSecurityException
   {
      private readonly bool temporary;

      /// <summary>
      /// Create a new instance of the connection SASL security exception
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      public ClientConnectionSecuritySaslException(string message) : this(message, false)
      {
      }

      /// <summary>
      /// Create a new instance of the connection SASL security exception
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      /// <param name="innerException">The exception that initiated this error</param>
      public ClientConnectionSecuritySaslException(string message, Exception innerException) : this(message, innerException, false)
      {
      }

      /// <summary>
      /// Create a new instance of the connection SASL security exception
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      /// <param name="temporary">Boolean that indicates if the error is a temporary (true) or permanent error (false)</param>
      public ClientConnectionSecuritySaslException(string message, bool temporary) : base(message)
      {
         this.temporary = temporary;
      }

      /// <summary>
      /// Create a new instance of the connection SASL security exception
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      /// <param name="innerException">The exception that initiated this error</param>
      /// <param name="temporary">Boolean that indicates if the error is a temporary (true) or permanent error (false)</param>
      public ClientConnectionSecuritySaslException(string message, Exception innerException, bool temporary) : base(message, innerException)
      {
         this.temporary = temporary;
      }
   }
}