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

namespace Apache.Qpid.Proton.Client.Exceptions
{
   /// <summary>
   /// Thrown when a message send operation times out either waiting for credit or
   /// waiting for some other resource to be ready to allow a send to trigger.
   /// </summary>
   public class ClientSendTimedOutException : ClientOperationTimedOutException
   {
      /// <summary>
      /// Creates an instance of this exception with the given message
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      public ClientSendTimedOutException(string message) : base(message)
      {
      }
   }
}