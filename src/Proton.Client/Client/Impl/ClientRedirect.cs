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
using System.Collections.Generic;
using System.IO;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Client.Impl
{
   /// <summary>
   /// Support class that houses the information and API needed to process
   /// redirection information sent from a remote.
   /// </summary>
   public sealed class ClientRedirect
   {
      private readonly Dictionary<Symbol, object> redirect;

      internal ClientRedirect(IDictionary<Symbol, object> redirect)
      {
         this.redirect = new Dictionary<Symbol, object>(redirect);
      }

      public ClientRedirect Validate()
      {
         object networkHost = null;

         if (!redirect.TryGetValue(ClientConstants.NETWORK_HOST, out networkHost))
         {
            throw new IOException("Redirection information not set, missing network host.");
         }

         if (networkHost is not string)
         {
            throw new IOException("Redirection information not correctly set, unknown network host type.");
         }

         int networkPort;

         try
         {

            networkPort = Int32.Parse(redirect[ClientConstants.PORT].ToString());
         }
         catch (Exception)
         {
            throw new IOException("Redirection information contained invalid port.");
         }

         // TODO LOG.trace("Redirect issued host and port as follows: {}:{}", networkHost, networkPort);

         return this;
      }

      /// <summary>
      /// Access the dictionary with remote supplied redirection information
      /// </summary>
      public IReadOnlyDictionary<Symbol, object> RedirectMap => redirect;

      /// <summary>
      /// Access the supplied host value in the redirection information.
      /// </summary>
      public string Hostname => redirect[ClientConstants.OPEN_HOSTNAME].ToString();

      /// <summary>
      /// Access the supplied DNS host value or IP Address in the redirection information.
      /// </summary>
      public string NetworkHostname => redirect[ClientConstants.NETWORK_HOST].ToString();

      /// <summary>
      /// Access the supplied remote port in the redirection information.
      /// </summary>
      public int Port => Int32.Parse(redirect[ClientConstants.PORT].ToString());

      /// <summary>
      /// Access the supplied scheme that the remote indicated the redirect connection should use.
      /// </summary>
      public string Scheme => redirect[ClientConstants.SCHEME].ToString();

      /// <summary>
      /// Access the supplied path that the remote indicated the redirect connection should use.
      /// </summary>
      public string Path => redirect[ClientConstants.PATH].ToString();

      /// <summary>
      /// Access the supplied address that the remote indicated the redirected link should use.
      /// </summary>
      public string Address => redirect[ClientConstants.ADDRESS].ToString();

      public override string ToString()
      {
         try
         {
            return string.Format("Redirect Host:{0}, Port:{1}", NetworkHostname, Port);
         }
         catch (Exception)
         {
            return "<Invalid-Redirect-Value>";
         }
      }
   }
}