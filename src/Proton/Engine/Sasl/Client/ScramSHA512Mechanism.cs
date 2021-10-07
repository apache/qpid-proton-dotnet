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
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Implements the SASL Scram SHA512 authentication Mechanism.
   /// </summary>
   public sealed class ScramSHA512Mechanism : AbstractScramSHAMechanism
   {
      public static readonly Symbol SCRAM_SHA_512 = Symbol.Lookup("SCRAM-SHA-512");

      public ScramSHA512Mechanism() : this(Guid.NewGuid().ToString())
      {
      }

      public ScramSHA512Mechanism(string clientNonce) : base(clientNonce)
      {
      }

      public override Symbol Name => SCRAM_SHA_512;

      protected override HashAlgorithm CreateHashAlgorithm()
      {
         return SHA512.Create();
      }

      protected override HMAC CreateHmac(byte[] keyBytes)
      {
         return new HMACSHA512(keyBytes);
      }
   }
}