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

using System.Net.Security;
using System.Security.Authentication;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// Options that control the SSL level transport configuration.
   /// </summary>
   public class SslOptions
   {
      public static readonly bool DEFAULT_ENABLED_CERT_REVOCATION_CHECKS = false;
      public static readonly bool DEFAULT_VERIFY_HOST = true;
      public static readonly int DEFAULT_SSL_PORT = 5671;

      /// <summary>
      /// Creates a default SSL options instance.
      /// </summary>
      public SslOptions() : base()
      {
      }

      /// <summary>
      /// Create a target options instance that copies the configuration from the given instance.
      /// </summary>
      /// <param name="other">The target options instance to copy</param>
      public SslOptions(SslOptions other) : this()
      {
         other.CopyInto(this);
      }

      /// <summary>
      /// Clone this options instance, changes to the cloned options are not reflected
      /// in this options instance.
      /// </summary>
      /// <returns>A deep copy of this options instance.</returns>
      public object Clone()
      {
         return MemberwiseClone();
      }

      internal SslOptions CopyInto(SslOptions other)
      {
         other.DefaultSslPort = DefaultSslPort;
         other.SslEnabled = SslEnabled;
         other.VerifyHost = VerifyHost;
         other.EnableCertificateRevocationChecks = EnableCertificateRevocationChecks;
         other.CommonNameOverride = CommonNameOverride;
         other.RemoteValidationCallbackOverride = RemoteValidationCallbackOverride;
         other.LocalCertificateSelectionOverride = LocalCertificateSelectionOverride;
         other.TlsVersionOverride = TlsVersionOverride;

         return this;
      }

      /// <summary>
      /// Controls if SSL is enabled for the connection these options are applied to.
      /// </summary>
      public bool SslEnabled { get; set; }

      /// <summary>
      /// Returns the configured default SSL port which if not set otherwise is 5671
      /// </summary>
      public int DefaultSslPort { get; set; } = DEFAULT_SSL_PORT;

      /// <summary>
      /// Controls if the default verification mechanism will allow host name mismatch
      /// in the servers SN or Common Name field or if a mismatch fails the verification process.
      /// By default the client uses the host used in the connection address to validate the
      /// server name.
      /// </summary>
      public bool VerifyHost { get; set; } = DEFAULT_VERIFY_HOST;

      /// <summary>
      /// Configure the value used to validate the common name (server name) provided in the
      /// servers certificate instead of using the value provided in the connection address.
      /// This option is only used when the verify host option is enabled.
      /// </summary>
      public string CommonNameOverride { get; set; } = null;

      /// <summary>
      /// Controls if the client will enable the system's certificate revocation checking
      /// feature (default is disabled).
      /// </summary>
      public bool EnableCertificateRevocationChecks { get; set; } = DEFAULT_ENABLED_CERT_REVOCATION_CHECKS;

      /// <summary>
      /// Allows the user to override the TLS version that the client will request from the O/S
      /// when performing the TLS handshake.  By default the client will let the system choose the
      /// best TLS version, however the user may wish to enforce a specifc value.
      /// </summary>
      public SslProtocols TlsVersionOverride { get; set; } = SslProtocols.None;

      /// <summary>
      /// Allows the user to provide an optional remote certificate validation callback which
      /// can be used by advanced users who want to customize the validation step of the TLS
      /// handshake process instead of relying on the built in mechanism.
      /// </summary>
      public RemoteCertificateValidationCallback RemoteValidationCallbackOverride { get; set; } = null;

      /// <summary>
      /// Allows the user to provide an optional local certificate selection callback which
      /// can be used by advanced users who want to customize the selection step when choosing
      /// the client certificate to provide to the remote during the TLS handshake.
      /// </summary>
      public LocalCertificateSelectionCallback LocalCertificateSelectionOverride { get; set; } = null;

   }
}