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

using System.Collections.Generic;
using System.Security.Principal;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Interface for a supplier of login credentials used by the SASL Authenticator to
   /// select and configure the client SASL mechanism.
   /// </summary>
   public interface ISaslCredentialsProvider
   {
      /// <summary>
      /// Gets the virtual host value the use when performing SASL authentication.
      /// </summary>
      string VHost { get; }

      /// <summary>
      /// Gets the user name value the use when performing SASL authentication.
      /// </summary>
      string Username { get; }

      /// <summary>
      /// Gets the password value the use when performing SASL authentication.
      /// </summary>
      string Password { get; }

      /// <summary>
      /// The local principal value to use when performing SASL authentication.
      /// </summary>
      IPrincipal LocalPrincipal { get; }

      /// <summary>
      /// Gets a collection of options the use when performing SASL authentication.
      /// </summary>
      IDictionary<string, object> Options => new Dictionary<string, object>();

   }
}