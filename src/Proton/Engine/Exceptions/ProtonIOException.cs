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
   /// Exception type that indicates an IO error has occured that is likely unrecoverable.
   /// </summary>
   public class ProtonIOException : ProtonException
   {
      /// <summary>
      /// Creates a default version of this exception type.
      /// </summary>
      public ProtonIOException() : base()
      {
      }

      /// <summary>
      /// Create a new instance with the given message that describes the specifics of the error.
      /// </summary>
      /// <param name="message">Description of the error</param>
      public ProtonIOException(string message) : base(message)
      {
      }

      /// <summary>
      /// Create a new instance with the given message that describes the specifics of the error.
      /// </summary>
      /// <param name="message">Description of the error</param>
      /// <param name="cause">The exception that causes this error</param>
      public ProtonIOException(string message, Exception cause) : base(message, cause)
      {
      }
   }
}