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
using System.Text;
using System.Text.RegularExpressions;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Implementation of the SASL XOAUTH2 mechanism.
   /// </summary>
   /// <remarks>
   /// User name and Password values are sent without being encrypted.
   /// </remarks>
   public sealed class XOauth2Mechanism : AbstractMechanism
   {
      public static readonly Symbol XOAUTH2 = Symbol.Lookup("XOAUTH2");

      private readonly Regex ACCESS_TOKEN_PATTERN = new Regex("^[\\x20-\\x7F]+$", RegexOptions.Compiled);

      private string additionalFailureInformation;

      public override Symbol Name => XOAUTH2;

      public override bool IsApplicable(ISaslCredentialsProvider credentials)
      {
         if (!string.IsNullOrEmpty(credentials.Username) &&
             !string.IsNullOrEmpty(credentials.Password))
         {
            return ACCESS_TOKEN_PATTERN.IsMatch(credentials.Password);
         }

         return false;
      }

      public override IProtonBuffer GetInitialResponse(ISaslCredentialsProvider credentials)
      {
         string username = credentials.Username;
         string password = credentials.Password;

         if (username == null)
         {
            username = "";
         }

         if (password == null)
         {
            password = "";
         }

         byte[] usernameBytes = Encoding.UTF8.GetBytes(credentials.Username);
         byte[] passwordBytes = Encoding.UTF8.GetBytes(credentials.Password);
         byte[] userPrefix = Encoding.ASCII.GetBytes("user=");
         byte[] authPrefix = Encoding.ASCII.GetBytes("auth=Bearer ");
         byte[] data = new byte[usernameBytes.Length + passwordBytes.Length + 20];

         Array.Copy(userPrefix, 0, data, 0, 5);
         Array.Copy(usernameBytes, 0, data, 5, usernameBytes.Length);

         data[5 + usernameBytes.Length] = 1;

         Array.Copy(authPrefix, 0, data, 6 + usernameBytes.Length, 12);
         Array.Copy(passwordBytes, 0, data, 18 + usernameBytes.Length, passwordBytes.Length);

         data[data.Length - 2] = 1;
         data[data.Length - 1] = 1;

         return ProtonByteBufferAllocator.Instance.Wrap(data);
      }

      public override IProtonBuffer GetChallengeResponse(ISaslCredentialsProvider credentials, IProtonBuffer challenge)
      {
         if (challenge != null && challenge.ReadableBytes > 0 && additionalFailureInformation == null)
         {
            additionalFailureInformation = challenge.ToString(Encoding.UTF8);
         }

         return EMPTY;
      }
   }
}