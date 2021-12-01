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

namespace Apache.Qpid.Proton.Test.Driver.Network
{
   /// <summary>
   /// Simple TCP server to accept a single incoming connection to the test peer and
   /// send a notification on the event loop when the connection is made.
   /// </summary>
   public sealed class PeerTcpServer
   {
      private Socket serverListener;
      private EventLoop eventLoop;

      private Action<PeerTcpClient> clientConnectedHandler;
      private Action<PeerTcpServer> serverFailedHandler;

      public PeerTcpServer(EventLoop eventLoop)
      {
         if (eventLoop == null)
         {
            throw new ArgumentNullException("Peer listener requires an event loop");
         }

         IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
         serverListener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
         serverListener.Bind(endpoint);

         this.eventLoop = eventLoop;
      }

      public int Start()
      {
         if (clientConnectedHandler == null)
         {
            throw new ArgumentNullException("Cannot start unless client connected handler set.");
         }

         if (serverFailedHandler == null)
         {
            throw new ArgumentNullException("Cannot start unless server failed handler set.");
         }

         serverListener.Listen(1);
         serverListener.BeginAccept(new AsyncCallback(NewTcpClientConnection), this);

         return ((IPEndPoint)serverListener.LocalEndPoint).Port;
      }

      public void Stop()
      {
         serverListener.Close();
      }

      public PeerTcpServer ClientConnectedHandler(Action<PeerTcpClient> clientConnectedHandler)
      {
         this.clientConnectedHandler = clientConnectedHandler;
         return this;
      }

      public PeerTcpServer ServerFailedHandler(Action<PeerTcpServer> serverFailedHandler)
      {
         this.serverFailedHandler = serverFailedHandler;
         return this;
      }

      private static void NewTcpClientConnection(IAsyncResult result)
      {
         PeerTcpServer server = (PeerTcpServer)result.AsyncState;

         try
         {
            Socket client = server.serverListener.EndAccept(result);

            // Signal that the client has connected and is ready for scripted action.
            server.eventLoop.Execute(() =>
               server.clientConnectedHandler(new PeerTcpClient(server.eventLoop, client)));
         }
         catch (Exception)
         {
            server.eventLoop.Execute(() => server.serverFailedHandler(server));
         }
         finally
         {
            server.Stop(); // Only accept one connection.
         }
      }
   }
}