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
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Base options class for common options that can be configured for test
   /// peer implementations that operate over a network connection.
   /// </summary>
   public abstract class ProtonNetworkPeerOptions : ICloneable
   {
      public static readonly int DEFAULT_SEND_BUFFER_SIZE = 64 * 1024;
      public static readonly int DEFAULT_RECEIVE_BUFFER_SIZE = DEFAULT_SEND_BUFFER_SIZE;
      public static readonly bool DEFAULT_TCP_NO_DELAY = true;
      public static readonly bool DEFAULT_TCP_KEEP_ALIVE = false;
      public static readonly uint DEFAULT_SO_LINGER = UInt32.MinValue;
      public static readonly uint DEFAULT_SO_TIMEOUT = 0;
      public static readonly bool DEFAULT_TRACE_BYTES = false;
      public static readonly string DEFAULT_CONTEXT_PROTOCOL = "TLS";
      public static readonly bool DEFAULT_TRUST_ALL = false;
      public static readonly bool DEFAULT_VERIFY_HOST = true;
      public static readonly bool DEFAULT_CHECK_CERT_REVOCATION = false;
      public static readonly int DEFAULT_LOCAL_PORT = 0;
      public static readonly bool DEFAULT_USE_WEBSOCKETS = false;
      public static readonly bool DEFAULT_FRAGMENT_WEBSOCKET_WRITES = false;
      public static readonly bool DEFAULT_SSL_ENABLED = false;
      public static readonly SslProtocols DEFAULT_SSL_PROTOCOLS = SslProtocols.None;

      /// <summary>
      /// Used by either client or server for TLS handshake
      /// </summary>
      private X509CertificateCollection certificateCollection;

      public bool SslEnabled { get; set; } = DEFAULT_SSL_ENABLED;

      public int SendBufferSize { get; set; } = DEFAULT_SEND_BUFFER_SIZE;

      public int ReceiveBufferSize { get; set; } = DEFAULT_RECEIVE_BUFFER_SIZE;

      public uint SendTimeout { get; set; } = DEFAULT_SO_TIMEOUT;

      public uint ReceiveTimeout { get; set; } = DEFAULT_SO_TIMEOUT;

      public uint SoLinger { get; set; } = DEFAULT_SO_LINGER;

      public bool TcpKeepAlive { get; set; } = DEFAULT_TCP_KEEP_ALIVE;

      public bool TcpNoDelay { get; set; } = DEFAULT_TCP_NO_DELAY;

      public string LocalAddress { get; set; }

      public int LocalPort { get; set; } = DEFAULT_LOCAL_PORT;

      public bool TraceBytes { get; set; } = DEFAULT_TRACE_BYTES;

      public bool UseWebSockets { get; set; } = DEFAULT_USE_WEBSOCKETS;

      public bool FragmentWebSocketWrites { get; set; } = DEFAULT_FRAGMENT_WEBSOCKET_WRITES;

      /// <summary>
      /// Should TLS host verification occur during the TLS handshake
      /// </summary>
      public bool VerifyHost { get; set; } = DEFAULT_VERIFY_HOST;

      /// <summary>
      /// Should the TLS handshake perform a check for certificate revocation.
      /// </summary>
      public bool CheckForCertificateRevocation { get; set; } = DEFAULT_CHECK_CERT_REVOCATION;

      /// <summary>
      /// Configures the SSL protocol value to provide for the TLS handshake which by
      /// default allows the underlying SSL layer to choose the best version.
      /// </summary>
      public SslProtocols SslProtocol { get; set; } = DEFAULT_SSL_PROTOCOLS;

      public virtual object Clone()
      {
         return base.MemberwiseClone();
      }

      #region Internal API used by subclasses

      /// <summary>
      /// Called from the client ond server options to present a collection
      /// to store server trusted certificates or client certificates for
      /// selection during TLS handshake.
      /// </summary>
      protected X509CertificateCollection CertificateCollection
      {
         get
         {
            X509CertificateCollection result = certificateCollection;

            if (result == null && !string.IsNullOrEmpty(CertificatePath))
            {
               result = new X509CertificateCollection();
               result.Add(new X509Certificate2(CertificatePath, CertificatePassword));
            }

            return result;
         }
         set { certificateCollection = value; }
      }

      /// <summary>
      /// Call from the client and server options to present a location for the
      /// server certificate or the client certificate
      /// </summary>
      protected string CertificatePath { get; set; }

      /// <summary>
      /// Call from the client and server options to present a password used
      /// to access the client or server certificate.
      /// </summary>
      protected string CertificatePassword { get; set; }

      #endregion
   }
}