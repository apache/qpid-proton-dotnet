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
   /// <summary>
   /// Decode exception thrown when a decode operation fails because it reached
   /// the end of available input before fully decoding the value it is currently
   /// working with.
   /// </summary>
   public class DecodeEOFException : Exception
   {
      /// <summary>
      /// Creates a default DecodeEOFException.
      /// </summary>
      public DecodeEOFException() : base()
      {
      }

      /// <summary>
      /// Create a new DecodeEOFException instance with the given message that describes the
      /// specifics of the decoding error.
      /// </summary>
      /// <param name="message">Description of the decoding error</param>
      public DecodeEOFException(string message) : base(message)
      {
      }

      /// <summary>
      /// Create a new DecodeEOFException instance with the given message that describes the
      /// specifics of the decoding error.
      /// </summary>
      /// <param name="message">Description of the decoding error</param>
      /// <param name="cause">The exception that causes this error</param>
      public DecodeEOFException(string message, Exception cause) : base(message, cause)
      {
      }
   }
}