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

using System.Security.Cryptography.X509Certificates;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Provides options for the proton TCP test client
   /// </summary>
   public sealed class ProtonTestServerOptions : ProtonNetworkPeerOptions
   {
      private static readonly int SERVER_CHOOSES_PORT = 0;

      public static readonly bool DEFAULT_NEEDS_CLIENT_AUTH = false;
      public static readonly int DEFAULT_SERVER_PORT = SERVER_CHOOSES_PORT;

      private X509Certificate serverCertificate;

      /// <summary>
      /// The port that the test peer server will listen on for an incoming
      /// connection from a client. If a value of zero is given the server
      /// will find a free port and listen there, to get the active port
      /// the user would need to start the server and then request the port
      /// from the running server.
      /// </summary>
      public int ServerPort { get; set; } = DEFAULT_SERVER_PORT;

      /// <summary>
      /// When true the server requires that the client provides a known trusted
      /// certificate in order to authenticate.
      /// </summary>
      public bool NeedClientAuth { get; set; } = DEFAULT_NEEDS_CLIENT_AUTH;

      /// <summary>
      /// Returns the configured server certificate which has either been set
      /// programmatically, or is loaded from a configured path and accessed
      /// using the configured password.
      /// </summary>
      public X509Certificate ServerCertificate
      {
         get
         {
            X509Certificate certificate = serverCertificate;

            if (serverCertificate == null && !string.IsNullOrEmpty(CertificatePath))
            {
               certificate = new X509Certificate(CertificatePath, CertificatePassword);
            }

            return certificate;
         }

         set { serverCertificate = value; }
      }

      /// <summary>
      /// Configures the certificate used when the server sends its certificate
      /// to the connecting client.
      /// </summary>
      public string ServerCertificatePath
      {
         get { return CertificatePath; }
         set { CertificatePath = value; }
      }

      /// <summary>
      /// Configures the certificate password used to unlock the server
      /// certificate when performing the TLS handshake with the client.
      /// </summary>
      public string ServerCertificatePassword
      {
         get { return CertificatePassword; }
         set { CertificatePassword = value; }
      }
   }
}