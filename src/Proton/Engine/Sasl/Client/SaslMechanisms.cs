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
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   public enum SaslMechanism
   {
      External,
      ScramSHA512,
      ScramSHA256,
      ScramSHA1,
      CramMD5,
      Plain,
      XOAuth2,
      Anonymous
   }

   public static class SaslMechanismExtensions
   {
      /// <summary>
      /// Returns the symbolic name that matches the given SaslMechanism enumeration value.
      /// </summary>
      /// <param name="mechanism">The enumeration value of the Sasl mechanism</param>
      /// <returns>The symbolic name that matches the given enumeration value</returns>
      /// <exception cref="ArgumentException">If no match can be found</exception>
      public static Symbol ToSymbol(this SaslMechanism mechanism)
      {
         switch (mechanism)
         {
            case SaslMechanism.External:
               return ExternalMechanism.EXTERNAL;
            case SaslMechanism.ScramSHA512:
               return ScramSHA512Mechanism.SCRAM_SHA_512;
            case SaslMechanism.ScramSHA256:
               return ScramSHA256Mechanism.SCRAM_SHA_256;
            case SaslMechanism.ScramSHA1:
               return ScramSHA1Mechanism.SCRAM_SHA_1;
            case SaslMechanism.CramMD5:
               return CramMD5Mechanism.CRAM_MD5;
            case SaslMechanism.Plain:
               return PlainMechanism.PLAIN;
            case SaslMechanism.XOAuth2:
               return XOauth2Mechanism.XOAUTH2;
            case SaslMechanism.Anonymous:
               return AnonymousMechanism.ANONYMOUS;
         }

         throw new ArgumentOutOfRangeException("Unknown or unimplemented SASL mechanism provided.");
      }

      /// <summary>
      /// Given a SaslMechanism enumeration value return an instance of the SASL
      /// Mechanism implementation from this library.
      /// </summary>
      /// <param name="mechanism">The SASL mechanism enumeration value to create</param>
      /// <returns>The matching IMechanism type for the given mechanism enumeration value</returns>
      /// <exception cref="ArgumentException">If no match can be found</exception>
      public static IMechanism CreateMechanism(this SaslMechanism mechanism)
      {
         switch (mechanism)
         {
            case SaslMechanism.External:
               return new ExternalMechanism();
            case SaslMechanism.ScramSHA512:
               return new ScramSHA512Mechanism();
            case SaslMechanism.ScramSHA256:
               return new ScramSHA256Mechanism();
            case SaslMechanism.ScramSHA1:
               return new ScramSHA1Mechanism();
            case SaslMechanism.CramMD5:
               return new CramMD5Mechanism();
            case SaslMechanism.Plain:
               return new PlainMechanism();
            case SaslMechanism.XOAuth2:
               return new XOauth2Mechanism();
            case SaslMechanism.Anonymous:
               return new AnonymousMechanism();
         }

         throw new ArgumentOutOfRangeException("Unknown or unimplemented SASL mechanism provided.");
      }
   }

   public static class SaslMechanisms
   {
      /// <summary>
      /// Given a Symbol lookup and return the correct SaslMechanism enumeration value
      /// that matches the symbolic name.
      /// </summary>
      /// <param name="name">The mechanism symbolic name to lookup</param>
      /// <returns>The SaslMechanism enumeration value that matches the given name</returns>
      /// <exception cref="ArgumentException">If no match can be found</exception>
      public static SaslMechanism Lookup(Symbol name)
      {
         foreach (SaslMechanism mechanism in (SaslMechanism[])Enum.GetValues(typeof(SaslMechanism)))
         {
            if (mechanism.ToSymbol().Equals(name))
            {
               return mechanism;
            }
         }

         throw new ArgumentException("No Matching SASL Mechanism with name: " + name.ToString());
      }

      /// <summary>
      /// Validate a SASL Mechanism name is among those supported by this library.
      /// </summary>
      /// <param name="name">The stringified name of the SASL mechanism to check</param>
      /// <returns>true if a matching mechanism is found</returns>
      public static bool Validate(String name)
      {
         foreach (SaslMechanism mechanism in (SaslMechanism[])Enum.GetValues(typeof(SaslMechanism)))
         {
            if (mechanism.ToSymbol().ToString().Equals(name))
            {
               return true;
            }
         }

         return false;
      }
   }
}