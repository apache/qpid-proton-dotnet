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

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Common base for SASL mechanism types that provides basic service to ease
   /// the creation of SASL mechanisms.
   /// </summary>
   public abstract class AbstractScramSHAMechanism : AbstractMechanism
   {
      private static readonly byte[] INT_1 = new byte[] { 0, 0, 0, 1 };
      private static readonly string GS2_HEADER = "n,,";

      private readonly string clientNonce;
      private readonly string digestName;
      private readonly string hmacName;

      private string serverNonce;
      private byte[] salt;
      private int iterationCount;
      private string clientFirstMessageBare;

      private byte[] serverSignature;

      private enum State
      {
         INITIAL,
         CLIENT_FIRST_SENT,
         CLIENT_PROOF_SENT,
         COMPLETE
      }

      private State state = State.INITIAL;

      protected AbstractScramSHAMechanism(string digestName, string hmacName, string clientNonce)
      {
         this.digestName = digestName;
         this.hmacName = hmacName;
         this.clientNonce = clientNonce;
      }

      public override bool IsApplicable(ISaslCredentialsProvider credentials)
      {
         return !string.IsNullOrEmpty(credentials.Username) &&
                !string.IsNullOrEmpty(credentials.Password);
      }

      public override IProtonBuffer GetInitialResponse(ISaslCredentialsProvider credentials)
      {
         if (state != State.INITIAL)
         {
            throw new SaslException("Request for initial response not expected in state " + state);
         }

         StringBuilder buf = new StringBuilder("n=");
         buf.Append(EscapeUsername(DoSaslPrep(credentials.Username)));
         buf.Append(",r=");
         buf.Append(clientNonce);
         clientFirstMessageBare = buf.ToString();
         state = State.CLIENT_FIRST_SENT;

         byte[] data = Encoding.ASCII.GetBytes(GS2_HEADER + clientFirstMessageBare);

         return ProtonByteBufferAllocator.Instance.Wrap(data);
      }

      public override IProtonBuffer GetChallengeResponse(ISaslCredentialsProvider credentials, IProtonBuffer challenge)
      {
         throw new NotImplementedException();
      }

      /// <summary>
      /// Derived SHA based SASL Mechanisms should create an HMAC that encapsulates
      /// the algorithm they represent and initialize it using the provided bytes.
      /// </summary>
      /// <param name="keyBytes">The algorithm key to use during initialization</param>
      /// <returns>The correct HMAC for the mechanism</returns>
      protected abstract HMAC CreateHmac(byte[] keyBytes);

      public override void VerifyCompletion()
      {
         base.VerifyCompletion();

         if (state != State.COMPLETE)
         {
            throw new SaslException(string.Format(
                "SASL exchange was not fully completed.  Expected state {0} but actual state {1}", State.COMPLETE, state));
         }
      }

      private string DoSaslPrep(string name)
      {
         // TODO - a real implementation of SaslPrep [rfc4013]

         try
         {
            Encoding.GetEncoding(Encoding.ASCII.CodePage,
                                 EncoderFallback.ExceptionFallback,
                                 DecoderFallback.ExceptionFallback).GetBytes(name);
            return name;
         }
         catch (EncoderFallbackException)
         {
            throw new SaslException("Can only encode names and passwords which are restricted to ASCII characters");
         }
      }

      private string EscapeUsername(string name)
      {
         name = name.Replace("=", "=3D");
         name = name.Replace(",", "=2C");
         return name;
      }
   }
}