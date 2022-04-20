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

namespace Apache.Qpid.Proton.Client.Transport
{
   public static class SslOptionsExtensions
   {
      /// <summary>
      /// Selects the server name to provide to the SSL handshake layer based on
      /// either the connected server host name or a configurable server name
      /// override in the SslOptions.
      /// </summary>
      /// <param name="options"></param>
      /// <param name="remoteHost"></param>
      /// <returns>The server name to use when performing the handshake</returns>
      public static string SelectServerName(this SslOptions options, string remoteHost)
      {
         if (string.IsNullOrEmpty(options.ServerNameOverride))
         {
            return remoteHost;
         }
         else
         {
            return options.ServerNameOverride;
         }
      }
   }
}