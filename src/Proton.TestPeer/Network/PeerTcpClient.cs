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
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Apache.Qpid.Proton.Test.Driver.Utilities;
using Microsoft.Extensions.Logging;

namespace Apache.Qpid.Proton.Test.Driver.Network
{
   public sealed class PeerTcpClient
   {
      private readonly ILoggerFactory loggerFactory;
      private readonly ILogger<PeerTcpClient> logger;
      private readonly ProtonTestClientOptions options;

      private string address;

      /// <summary>
      /// Create a new peer Tcp client instance that can be used to connect to a remote.
      /// </summary>
      public PeerTcpClient(ProtonTestClientOptions options, in ILoggerFactory loggerFactory)
      {
         this.loggerFactory = loggerFactory;
         this.logger = loggerFactory?.CreateLogger<PeerTcpClient>();
         this.options = options;
      }

      public PeerTcpTransport Connect(string address, int port)
      {
         this.address = address;

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

         Stream ioStream = new NetworkStream(clientSocket);
         if (options.SslEnabled)
         {
            ioStream = AuthenticateAsClient(ioStream);
         }

         return new PeerTcpTransport(loggerFactory, PeerTransportRole.Client, clientSocket, ioStream);
      }

      private Stream AuthenticateAsClient(Stream ioStream)
      {
         SslStream sslStream = new SslStream(ioStream,
                                             false,
                                             RemoteCertificateValidationCallback,
                                             LocalCertificateSelectionCallback);

         sslStream.AuthenticateAsClient(address, options.ClientCertificates, options.CheckForCertificateRevocation);

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

         if (remoteCertificateNotAvailable &&
             !options.AllowedSslPolicyErrorsOverride.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
         {
            logger.LogTrace("Server certificate authentication failed due lack of provided certificate: {0}", sslPolicyErrors);
            validated = false;
         }

         if (remoteCertificateChainErrors &&
             !options.AllowedSslPolicyErrorsOverride.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
         {
            logger.LogTrace("Server certificate authentication failed due certificate chain error: {0}", sslPolicyErrors);
            validated = false;
         }

         if (remoteCertificateNameMismatch && options.VerifyHost &&
             !options.AllowedSslPolicyErrorsOverride.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
         {
            logger.LogTrace("Server certificate authentication failed due remote certificate name mismatch: {0}", sslPolicyErrors);
            validated = false;
         }

         if (!validated)
         {
            logger.LogDebug("Server authentication had SSL policy error(s): {0}", sslPolicyErrors);
         }

         return validated;
      }

      public static X509Certificate LocalCertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
      {
         X509Certificate result = null;

         if (acceptableIssuers != null && acceptableIssuers.Length > 0 &&
             localCertificates != null && localCertificates.Count > 0)
         {
            foreach (X509Certificate certificate in localCertificates)
            {
               string issuer = certificate.Issuer;
               if (Array.IndexOf(acceptableIssuers, issuer) != -1)
               {
                  result = certificate;
                  break;
               }
            }
         }

         if (result == null && localCertificates != null && localCertificates.Count > 0)
         {
            result = localCertificates[0];
         }

         return result;
      }
   }
}