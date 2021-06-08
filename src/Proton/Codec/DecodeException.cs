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

namespace Apache.Qpid.Proton.Codec
{
   public class DecodeException : Exception
   {
      /// <summary>
      /// Creates a default DecodeException.
      /// </summary>
      public DecodeException() : base()
      {         
      }

      /// <summary>
      /// Create a new DecodeException instance with the given message that describes the
      /// specifics of the decoding error.
      /// </summary>
      /// <param name="message">Description of the decoding error</param>
      public DecodeException(string message) : base(message)
      {
      }

      /// <summary>
      /// Create a new DecodeException instance with the given message that describes the
      /// specifics of the decoding error.
      /// </summary>
      /// <param name="message">Description of the decoding error</param>
      /// <param name="cause">The exception that causes this error</param>
      public DecodeException(string message, Exception cause) : base(message, cause)
      {
      }
   }
}