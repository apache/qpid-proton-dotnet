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

using System.Security.Principal;
using Apache.Qpid.Proton.Buffer;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   public abstract class MechanismTestBase
   {
      protected static readonly string HOST = "localhost";
      protected static readonly string USERNAME = "user";
      protected static readonly string PASSWORD = "pencil";

      protected static readonly IProtonBuffer TEST_BUFFER;

      static MechanismTestBase()
      {
         TEST_BUFFER = ProtonByteBufferAllocator.Instance.Allocate(10, 10);
         TEST_BUFFER.WriteOffset = 10;
      }

      protected ISaslCredentialsProvider Credentials()
      {
         return new UserCredentialsProvider(USERNAME, PASSWORD, HOST, true);
      }

      protected ISaslCredentialsProvider Credentials(string user, string password)
      {
         return new UserCredentialsProvider(user, password, null, false);
      }

      protected ISaslCredentialsProvider Credentials(string user, string password, bool principal)
      {
         return new UserCredentialsProvider(user, password, null, principal);
      }

      protected ISaslCredentialsProvider EmptyCredentials()
      {
         return new UserCredentialsProvider(null, null, null, false);
      }

      private sealed class UserCredentialsProvider : ISaslCredentialsProvider
      {
         private readonly string username;
         private readonly string password;
         private readonly string host;
         private readonly bool principal;

         public UserCredentialsProvider(string username, string password, string host, bool principal)
         {
            this.username = username;
            this.password = password;
            this.host = host;
            this.principal = principal;
         }

         public string VHost => host;

         public string Username => username;

         public string Password => password;

         public IPrincipal LocalPrincipal
         {
            get
            {
               if (principal)
               {
                  return new DummyPrincipal();
               }
               else
               {
                  return null;
               }
            }
         }

         private class DummyPrincipal : IPrincipal
         {
            public IIdentity Identity => throw new System.NotImplementedException();

            public bool IsInRole(string role)
            {
               throw new System.NotImplementedException();
            }
         }
      }
   }
}