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

namespace Apache.Qpid.Proton.Engine.Exceptions
{
   /// <summary>
   /// Base exception types that backs most exceptions that are generated from the Proton engine
   /// during it's operation.
   /// </summary>
   public class ProtonException : Exception
   {
      /// <summary>
      /// Creates a default version of this exception type.
      /// </summary>
      public ProtonException() : base()
      {
      }

      /// <summary>
      /// Create a new instance with the given message that describes the specifics of the error.
      /// </summary>
      /// <param name="message">Description of the error</param>
      public ProtonException(string message) : base(message)
      {
      }

      /// <summary>
      /// Create a new instance with the given message that describes the specifics of the error.
      /// </summary>
      /// <param name="message">Description of the error</param>
      /// <param name="cause">The exception that causes this error</param>
      public ProtonException(string message, Exception cause) : base(message, cause)
      {
      }

      /// <summary>
      /// Create a new instance with the given exception that describes the specifics of the error.
      /// </summary>
      /// <param name="cause">The exception that causes this error</param>
      public ProtonException(Exception cause) : base(cause.Message, cause)
      {
      }
   }
}