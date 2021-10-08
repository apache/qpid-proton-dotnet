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
using System.Security.Principal;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// SASL Credentials Provider instance that accepts delegate methods which
   /// can provide the credentials upon request.
   /// </summary>
   public sealed class DelegatedSaslCredentialsProvider : ISaslCredentialsProvider
   {
      private Func<string> vhostSupplier;
      private Func<string> usernameSupplier;
      private Func<string> passwordSupplier;
      private Func<IPrincipal> principalSupplier;

      public DelegatedSaslCredentialsProvider VHostSupplier(Func<string> supplier)
      {
         this.vhostSupplier = supplier;
         return this;
      }

      public DelegatedSaslCredentialsProvider UsernameSupplier(Func<string> supplier)
      {
         this.usernameSupplier = supplier;
         return this;
      }
      public DelegatedSaslCredentialsProvider PasswordSupplier(Func<string> supplier)
      {
         this.passwordSupplier = supplier;
         return this;
      }
      public DelegatedSaslCredentialsProvider PasswordSupplier(Func<IPrincipal> supplier)
      {
         this.principalSupplier = supplier;
         return this;
      }

      #region Delegated credentials access API

      public string VHost => vhostSupplier?.Invoke() ?? null;

      public string Username => usernameSupplier?.Invoke() ?? null;

      public string Password => passwordSupplier?.Invoke() ?? null;

      public IPrincipal LocalPrincipal => principalSupplier?.Invoke() ?? null;

      #endregion
   }
}