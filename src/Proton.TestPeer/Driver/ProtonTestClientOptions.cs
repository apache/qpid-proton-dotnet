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
   public sealed class ProtonTestClientOptions : ProtonNetworkPeerOptions
   {
      /// <summary>
      /// A Collection of certificates used to select from when performing
      /// the TLS handshake with the remote.
      /// </summary>
      public X509CertificateCollection ClientCertificates { get; set; }

      /// <summary>
      /// Configures the certificate used when the server requests client
      /// authentication via a client certificate.
      /// </summary>
      public string ClientCertificatePath
      {
         get { return CertificatePath; }
         set { CertificatePath = value; }
      }

      /// <summary>
      /// Configures the certificate password used to unlock the client
      /// certificate when doing client authentication.
      /// </summary>
      public string ClientCertificatePassword
      {
         get { return CertificatePassword; }
         set { CertificatePassword = value; }
      }
   }
}