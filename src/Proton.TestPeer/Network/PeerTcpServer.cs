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
using Microsoft.Extensions.Logging;
using Apache.Qpid.Proton.Test.Driver.Utilities;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Apache.Qpid.Proton.Test.Driver.Network
{
   /// <summary>
   /// Simple TCP server to accept a single incoming connection to the test peer and
   /// send a notification on the event loop when the connection is made.
   /// </summary>
   public sealed class PeerTcpServer
   {
      private readonly Socket serverListener;
      private readonly AtomicBoolean closed = new();

      private Action<PeerTcpTransport> clientConnectedHandler;
      private Action<PeerTcpServer, Exception> serverFailedHandler;

      private readonly ILoggerFactory loggerFactory;
      private readonly ILogger<PeerTcpServer> logger;
      private readonly ProtonTestServerOptions options;

      private string listenAddress;
      private int listenPort;
      private IPEndPoint listenEndpoint;

      public PeerTcpServer(ProtonTestServerOptions options, in ILoggerFactory loggerFactory)
      {
         this.loggerFactory = loggerFactory;
         this.logger = loggerFactory?.CreateLogger<PeerTcpServer>();
         this.options = options;

         IPEndPoint endpoint = new(IPAddress.Loopback, 0);
         serverListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
         serverListener.Bind(endpoint);
      }

      public IPEndPoint ListeningOn => listenEndpoint;

      public string ListeningOnAddress => listenAddress;

      public int ListeningOnPort => listenPort;

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

         logger.LogInformation("Peer TCP Server listen started on endpoint: {0}", serverListener.LocalEndPoint);

         this.listenEndpoint = ((IPEndPoint)serverListener.LocalEndPoint);
         this.listenAddress = listenEndpoint.Address.ToString();
         this.listenPort = listenEndpoint.Port;

         try
         {
            serverListener.BeginAccept(new AsyncCallback(NewTcpClientConnection), this);
         }
         catch (Exception ex)
         {
            logger.LogWarning(ex, "Peer TCP Server failed on begin accept : {0}", ex.Message);
         }

         return ((IPEndPoint)serverListener.LocalEndPoint).Port;
      }

      public void Stop()
      {
         if (closed.CompareAndSet(false, true))
         {
            logger.LogInformation("Peer TCP Server closed endpoint: {0}", serverListener.LocalEndPoint);
            try
            {
               serverListener.Shutdown(SocketShutdown.Both);
            }
            catch (Exception)
            {
            }

            try
            {
               serverListener.Close(1);
            }
            catch (Exception)
            {
            }
         }
      }

      public PeerTcpServer ClientConnectedHandler(Action<PeerTcpTransport> clientConnectedHandler)
      {
         this.clientConnectedHandler = clientConnectedHandler;
         return this;
      }

      public PeerTcpServer ServerFailedHandler(Action<PeerTcpServer, Exception> serverFailedHandler)
      {
         this.serverFailedHandler = serverFailedHandler;
         return this;
      }

      private Stream AuthenticateAsSslServer(Stream ioStream)
      {
         SslStream sslStream = new(ioStream, true, RemoteCertificateValidationCallback, null);

         sslStream.AuthenticateAsServer(options.ServerCertificate,
                                        options.NeedClientAuth,
                                        options.SslProtocol,
                                        options.CheckForCertificateRevocation);

         return sslStream;
      }

      private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
      {
         if (sslPolicyErrors == SslPolicyErrors.None)
         {
            return true;
         }

         bool validated = true;

         bool remoteCertificateNotAvailable = sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable);
         bool remoteCertificateNameMismatch = sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch);
         bool remoteCertificateChainErrors = sslPolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors);

         if (remoteCertificateNotAvailable && options.NeedClientAuth &&
               !options.AllowedSslPolicyErrorsOverride.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
         {
            logger.LogTrace("Client certificate authentication failed due lack of provided certificate: {0}", sslPolicyErrors);
            validated = false;
         }

         if (remoteCertificateChainErrors &&
               !options.AllowedSslPolicyErrorsOverride.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
         {
            logger.LogTrace("Client certificate authentication failed due certificate chain error: {0}", sslPolicyErrors);
            validated = false;
         }

         if (remoteCertificateNameMismatch && options.VerifyHost &&
               !options.AllowedSslPolicyErrorsOverride.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
         {
            logger.LogTrace("Client certificate authentication failed due remote certificate name mismatch: {0}", sslPolicyErrors);
            validated = false;
         }

         if (!validated)
         {
            logger.LogDebug("Client certificate authentication had SSL policy error(s): {0}", sslPolicyErrors);
         }

         return validated;
      }

      private static void NewTcpClientConnection(IAsyncResult result)
      {
         PeerTcpServer server = (PeerTcpServer)result.AsyncState;

         try
         {
            Socket client = server.serverListener.EndAccept(result);

            server.logger.LogInformation("Peer Tcp Server accepted new connection: {0}", client.RemoteEndPoint);

            client.SendBufferSize = server.options.SendBufferSize;
            client.ReceiveBufferSize = server.options.ReceiveBufferSize;
            client.NoDelay = server.options.TcpNoDelay;
            client.LingerState = new LingerOption(server.options.SoLinger > 0, (int)server.options.SoLinger);
            client.SendTimeout = (int)server.options.SendTimeout;
            client.ReceiveTimeout = (int)server.options.ReceiveTimeout;

            Stream ioStream = new NetworkStream(client);

            if (server.options.SslEnabled)
            {
               try
               {
                  ioStream = server.AuthenticateAsSslServer(ioStream);
               }
               catch (Exception ex)
               {
                  server.logger.LogWarning(ex, "Server SSL Authentication failed: {0}", ex.Message);
                  client.Close();
                  throw;
               }
            }

            // Signal that the client has connected and is ready for scripted action.
            server.clientConnectedHandler(
               new PeerTcpTransport(server.loggerFactory, PeerTransportRole.Server, client, ioStream));
         }
         catch (SocketException sockEx)
         {
            if (!server.closed)
            {
               server.logger.LogWarning(sockEx, "Server accept failed: {0}, SocketErrorCode:{1}",
                                        sockEx.Message, sockEx.SocketErrorCode);
               try
               {
                  server.serverFailedHandler(server, sockEx);
               }
               catch (Exception)
               { }
            }
         }
         catch (Exception ex)
         {
            if (!server.closed)
            {
               server.logger.LogWarning(ex, "Server accept failed: {0}", ex.Message);
               try
               {
                  server.serverFailedHandler(server, ex);
               }
               catch (Exception)
               { }
            }
         }
         finally
         {
            try
            {
               server.Stop(); // Only accept one connection.
            }
            catch (Exception)
            { }
         }
      }
   }
}