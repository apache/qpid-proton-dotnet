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
using System.Net;
using System.Net.Sockets;
using Apache.Qpid.Proton.Test.Driver.Utilities;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver.Network
{
   public sealed class PeerTcpClient
   {
      private readonly ILoggerFactory loggerFactory;
      private readonly ILogger<PeerTcpClient> logger;
      private readonly ProtonTestClientOptions options;

      /// <summary>
      /// Create a new peer Tcp client instance that can be used to connect to a remote.
      /// </summary>
      public PeerTcpClient(ProtonTestClientOptions options, in ILoggerFactory loggerFactory)
      {
         this.loggerFactory = loggerFactory;
         this.logger = loggerFactory?.CreateLogger<PeerTcpClient>();
         this.options = options;
      }

      public PeerTcpTransport Connect(IPEndPoint endpoint)
      {
         Statics.RequireNonNull(endpoint, "Cannot connect when the end point given is null");

         Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

         try
         {
            clientSocket.Connect(endpoint);

            // Configure socket options from configuration options
            clientSocket.SendBufferSize = options.SendBufferSize;
            clientSocket.ReceiveBufferSize = options.ReceiveBufferSize;
            clientSocket.NoDelay = options.TcpNoDelay;
            clientSocket.LingerState = new LingerOption(options.SoLinger > 0, (int)options.SoLinger);
            clientSocket.SendTimeout = (int)options.SendTimeout;
            clientSocket.ReceiveTimeout = (int)options.ReceiveTimeout;
         }
         catch (Exception)
         {
            try
            {
               clientSocket.Close();
            }
            catch (Exception)
            {
            }
            throw;
         }

         // TODO - SSL client authentication

         return new PeerTcpTransport(loggerFactory, PeerTransportRole.Client, clientSocket, new NetworkStream(clientSocket));
      }

      public PeerTcpTransport Connect(string address, int port)
      {
         IPHostEntry entry = Dns.GetHostEntry(address);
         foreach (IPAddress ipAddress in entry.AddressList)
         {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
               return Connect(new IPEndPoint(ipAddress, port));
            }
         }

         throw new InvalidOperationException("Could not resolve the address into an IPV4 IP Address");
      }
   }
}