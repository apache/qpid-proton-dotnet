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
using System.Linq;
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

      protected AbstractScramSHAMechanism(string clientNonce)
      {
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

         StringBuilder buf = new("n=");
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
         byte[] response;

         switch (state)
         {
            case State.CLIENT_FIRST_SENT:
               response = CalculateClientProof(credentials, challenge);
               state = State.CLIENT_PROOF_SENT;
               break;
            case State.CLIENT_PROOF_SENT:
               EvaluateOutcome(challenge);
               response = Array.Empty<byte>();
               state = State.COMPLETE;
               break;
            default:
               throw new SaslException("No challenge expected in state " + state);
         }

         return ProtonByteBufferAllocator.Instance.Wrap(response);
      }

      private byte[] CalculateClientProof(ISaslCredentialsProvider credentials, IProtonBuffer challenge)
      {
         try
         {
            string serverFirstMessage = challenge.ToString(Encoding.ASCII);
            string[] parts = serverFirstMessage.Split(",");
            if (parts.Length < 3)
            {
               throw new SaslException("Server challenge '" + serverFirstMessage + "' cannot be parsed");
            }
            else if (parts[0].StartsWith("m="))
            {
               throw new SaslException("Server requires mandatory extension which is not supported: " + parts[0]);
            }
            else if (!parts[0].StartsWith("r="))
            {
               throw new SaslException("Server challenge '" + serverFirstMessage + "' cannot be parsed, cannot find nonce");
            }

            string nonce = parts[0].Substring(2);
            if (!nonce.StartsWith(clientNonce))
            {
               throw new SaslException("Server challenge did not use correct client nonce");
            }
            serverNonce = nonce;
            if (!parts[1].StartsWith("s="))
            {
               throw new SaslException("Server challenge '" + serverFirstMessage + "' cannot be parsed, cannot find salt");
            }

            string base64Salt = parts[1].Substring(2);
            salt = Convert.FromBase64String(base64Salt);
            if (!parts[2].StartsWith("i="))
            {
               throw new SaslException("Server challenge '" + serverFirstMessage + "' cannot be parsed, cannot find iteration count");
            }

            string iterCountString = parts[2].Substring(2);
            iterationCount = Convert.ToInt32(iterCountString);
            if (iterationCount <= 0)
            {
               throw new SaslException("Iteration count " + iterationCount + " is not a positive integer");
            }

            byte[] passwordBytes = Encoding.UTF8.GetBytes(DoSaslPrep(new string(credentials.Password)));
            byte[] saltedPassword = GenerateSaltedPassword(passwordBytes);

            string clientFinalMessageWithoutProof =
                    "c=" + Convert.ToBase64String(Encoding.ASCII.GetBytes(GS2_HEADER)) + ",r=" + serverNonce;

            string authMessage = clientFirstMessageBare
                    + "," + serverFirstMessage + "," + clientFinalMessageWithoutProof;

            byte[] clientKey = ComputeHmac(saltedPassword, "Client Key");
            byte[] storedKey = CreateHashAlgorithm().ComputeHash(clientKey);

            byte[] clientSignature = ComputeHmac(storedKey, authMessage);

            byte[] clientProof = (byte[])clientKey.Clone();
            for (int i = 0; i < clientProof.Length; i++)
            {
               clientProof[i] ^= clientSignature[i];
            }

            byte[] serverKey = ComputeHmac(saltedPassword, "Server Key");
            serverSignature = ComputeHmac(serverKey, authMessage);

            string finalMessageWithProof = clientFinalMessageWithoutProof
                    + ",p=" + Convert.ToBase64String(clientProof);

            return Encoding.ASCII.GetBytes(finalMessageWithProof);
         }
         catch (SaslException)
         {
            throw;
         }
         catch (Exception e)
         {
            throw new SaslException(e.Message, e);
         }
      }

      private void EvaluateOutcome(IProtonBuffer challenge)
      {
         string serverFinalMessage = challenge.ToString(Encoding.ASCII);
         string[] parts = serverFinalMessage.Split(",");

         if (!parts[0].StartsWith("v="))
         {
            throw new SaslException("Server final message did not contain verifier");
         }

         byte[] localServerSignature;
         try
         {
            localServerSignature = Convert.FromBase64String(parts[0].Substring(2));
         }
         catch (Exception ex)
         {
            // Possible that the encoding is faulty which results in same action of
            // denying the server outcome.
            throw new SaslException(ex.Message, ex);
         }

         if (!Enumerable.SequenceEqual(serverSignature, localServerSignature))
         {
            throw new SaslException("Server signature did not match");
         }
      }

      private byte[] GenerateSaltedPassword(byte[] passwordBytes)
      {
         HMAC mac = CreateHmac(passwordBytes);

         byte[] aggregated = new byte[salt.Length + INT_1.Length];

         Array.Copy(salt, aggregated, salt.Length);
         Array.Copy(INT_1, 0, aggregated, salt.Length, INT_1.Length);

         byte[] initial = mac.ComputeHash(aggregated);
         byte[] previous = null;

         for (int i = 1; i < iterationCount; i++)
         {
            previous = mac.ComputeHash(previous ?? initial);
            for (int x = 0; x < initial.Length; x++)
            {
               initial[x] ^= previous[x];
            }
         }

         return initial;
      }

      /// <summary>
      /// Derived SHA based SASL Mechanisms should create an HMAC that encapsulates
      /// the algorithm they represent and initialize it using the provided bytes.
      /// </summary>
      /// <param name="keyBytes">The algorithm key to use during initialization</param>
      /// <returns>The correct HMAC for the mechanism</returns>
      protected abstract HMAC CreateHmac(byte[] keyBytes);

      /// <summary>
      /// Dervied SHA based message digest algorithm used by this SASL mechanism
      /// </summary>
      /// <returns>A new hash algorithm that perfrom the digest required by this SASL mechanism</returns>
      protected abstract HashAlgorithm CreateHashAlgorithm();

      public override void VerifyCompletion()
      {
         base.VerifyCompletion();

         if (state != State.COMPLETE)
         {
            throw new SaslException(string.Format(
                "SASL exchange was not fully completed.  Expected state {0} but actual state {1}", State.COMPLETE, state));
         }
      }

      private byte[] ComputeHmac(byte[] key, string @string)
      {
         return CreateHmac(key).ComputeHash(Encoding.ASCII.GetBytes(@string));
      }

      private static string DoSaslPrep(string name)
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

      private static string EscapeUsername(string name)
      {
         name = name.Replace("=", "=3D");
         name = name.Replace(",", "=2C");
         return name;
      }
   }
}