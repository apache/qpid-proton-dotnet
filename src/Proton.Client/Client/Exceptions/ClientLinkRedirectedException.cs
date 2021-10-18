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

using Apache.Qpid.Proton.Client.Impl;

namespace Apache.Qpid.Proton.Client.Exceptions
{
   /// <summary>
   /// Root exception type for cases of remote closure or client created Sender or Receiver.
   /// </summary>
   public class ClientLinkRedirectedException : ClientLinkRemotelyClosedException
   {
      private readonly ClientRedirect redirect;

      /// <summary>
      /// Creates an instance of this exception with the given message and provided
      /// redirect information along with an error condition sent from the remote..
      /// </summary>
      /// <param name="message">The message that describes the error</param>
      /// <param name="innerException">The exception that caused this error</param>
      public ClientLinkRedirectedException(string message, ClientRedirect redirect, IErrorCondition errorCondition) : base(message, errorCondition)
      {
         this.redirect = redirect;
      }

      /// <summary>
      /// Access the supplied host value in the redirection information.
      /// </summary>
      public string Hostname => redirect.Hostname;

      /// <summary>
      /// Access the supplied DNS host value or IP Address in the redirection information.
      /// </summary>
      public string NetworkHostname => redirect.NetworkHostname;

      /// <summary>
      /// Access the supplied remote port in the redirection information.
      /// </summary>
      public int Port => redirect.Port;

      /// <summary>
      /// Access the supplied scheme that the remote indicated the redirect connection should use.
      /// </summary>
      public string Scheme => redirect.Scheme;

      /// <summary>
      /// Access the supplied path that the remote indicated the redirect connection should use.
      /// </summary>
      public string Path => redirect.Path;

      /// <summary>
      /// Access the supplied address that the remote indicated the redirect link should use.
      /// </summary>
      public string Address => redirect.Address;

   }
}