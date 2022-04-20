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
         return mechanism switch
         {
            SaslMechanism.External => ExternalMechanism.EXTERNAL,
            SaslMechanism.ScramSHA512 => ScramSHA512Mechanism.SCRAM_SHA_512,
            SaslMechanism.ScramSHA256 => ScramSHA256Mechanism.SCRAM_SHA_256,
            SaslMechanism.ScramSHA1 => ScramSHA1Mechanism.SCRAM_SHA_1,
            SaslMechanism.CramMD5 => CramMD5Mechanism.CRAM_MD5,
            SaslMechanism.Plain => PlainMechanism.PLAIN,
            SaslMechanism.XOAuth2 => XOauth2Mechanism.XOAUTH2,
            SaslMechanism.Anonymous => AnonymousMechanism.ANONYMOUS,
            _ => throw new ArgumentOutOfRangeException(nameof(mechanism), "Unknown or unimplemented SASL mechanism provided."),
         };
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
         return mechanism switch
         {
            SaslMechanism.External => new ExternalMechanism(),
            SaslMechanism.ScramSHA512 => new ScramSHA512Mechanism(),
            SaslMechanism.ScramSHA256 => new ScramSHA256Mechanism(),
            SaslMechanism.ScramSHA1 => new ScramSHA1Mechanism(),
            SaslMechanism.CramMD5 => new CramMD5Mechanism(),
            SaslMechanism.Plain => new PlainMechanism(),
            SaslMechanism.XOAuth2 => new XOauth2Mechanism(),
            SaslMechanism.Anonymous => new AnonymousMechanism(),
            _ => throw new ArgumentOutOfRangeException(nameof(mechanism), "Unknown or unimplemented SASL mechanism provided."),
         };
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
      public static bool Validate(string name)
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