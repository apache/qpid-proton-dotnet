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
using System.Security.Cryptography;
using System.Text;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Implementation of the SASL CRAM-MD5 mechanism.
   /// </summary>
   public sealed class CramMD5Mechanism : AbstractMechanism
   {
      public static readonly Symbol CRAM_MD5 = Symbol.Lookup("CRAM-MD5");

      private bool sentResponse;

      public override Symbol Name => CRAM_MD5;

      public override bool IsApplicable(ISaslCredentialsProvider credentials)
      {
         return !string.IsNullOrEmpty(credentials.Username) &&
                !string.IsNullOrEmpty(credentials.Password);
      }

      public override IProtonBuffer GetInitialResponse(ISaslCredentialsProvider credentialsProvider)
      {
         return null;
      }

      public override IProtonBuffer GetChallengeResponse(ISaslCredentialsProvider credentials, IProtonBuffer challenge)
      {
         if (!sentResponse && challenge != null && challenge.ReadableBytes != 0)
         {
            byte[] challengeBytes = new byte[challenge.ReadableBytes];
            challenge.WriteBytes(challengeBytes);

            HMACMD5 mac = new HMACMD5(Encoding.ASCII.GetBytes(credentials.Password));

            byte[] result = mac.ComputeHash(challengeBytes);

            StringBuilder hash = new StringBuilder(credentials.Username);
            hash.Append(' ');
            for (int i = 0; i < result.Length; i++)
            {
               hash.AppendFormat("{0:x2}", result[i]);
            }

            sentResponse = true;

            return ProtonByteBufferAllocator.Instance.Wrap(Encoding.ASCII.GetBytes(hash.ToString()));
         }
         else
         {
            return EMPTY;
         }
      }

      public override void VerifyCompletion()
      {
         base.VerifyCompletion();

         if (!sentResponse)
         {
            throw new SaslException("SASL exchange was not fully completed.");
         }
      }
   }
}