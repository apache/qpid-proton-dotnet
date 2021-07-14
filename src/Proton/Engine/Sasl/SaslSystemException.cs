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

namespace Apache.Qpid.Proton.Engine.Sasl
{
   /// <summary>
   /// Indicates that a SASL handshake has failed with a 'sys", 'sys-perm',
   /// or 'sys-temp' outcome code as defined by:
   ///
   /// <see cref="http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-security-v1.0-os.html#type-sasl-code"/>
   ///
   /// Version 1.0, Section 5.3.3.6
   /// </summary>
   public class SaslSystemException : SaslException
   {
      /// <summary>
      /// Creates a default version of this exception type.
      /// </summary>
      public SaslSystemException(bool permanent) : base()
      {
         Permanent = permanent;
      }

      /// <summary>
      /// Create a new instance with the given message that describes the specifics of the error.
      /// </summary>
      /// <param name="message">Description of the error</param>
      public SaslSystemException(string message, bool permanent) : base(message)
      {
         Permanent = permanent;
      }

      /// <summary>
      /// Checks if the condition that caused this exception is of a permanent nature.
      /// </summary>
      public bool Permanent { get; }

   }
}