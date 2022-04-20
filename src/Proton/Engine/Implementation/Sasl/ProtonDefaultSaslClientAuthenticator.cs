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
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Engine.Sasl;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Engine.Implementation.Sasl
{
   /// <summary>
   /// A Proton default SASL client authenticator which only supports remote
   /// SASL exchanges where the ANONYMOUS mechanism is an published option.
   /// </summary>
   public sealed class ProtonDefaultSaslClientAuthenticator : ISaslClientAuthenticator
   {
      public static readonly ProtonDefaultSaslClientAuthenticator Instance = new();

      private readonly Symbol ANONYMOUS = Symbol.Lookup("ANONYMOUS");

      public void HandleSaslChallenge(ISaslClientContext context, IProtonBuffer challenge)
      {
         throw new NotImplementedException();
      }

      public void HandleSaslMechanisms(ISaslClientContext context, Symbol[] mechanisms)
      {

         if (mechanisms != null && Array.Find(mechanisms, element => ANONYMOUS.Equals(element)) != null)
         {
            context.SendChosenMechanism(ANONYMOUS, null, ProtonByteBufferAllocator.Instance.Allocate(0, 0));
         }
         else
         {
            ProtonSaslContext sasl = (ProtonSaslContext)context;
            context.SaslFailure(new MechanismMismatchException(
                "Proton default SASL handler only supports ANONYMOUS exchanges", StringUtils.ToStringArray(mechanisms)));
            sasl.Done(SaslAuthOutcome.SaslAuthFailed);
         }
      }

      public void HandleSaslOutcome(ISaslClientContext context, SaslAuthOutcome outcome, IProtonBuffer additional)
      {
         throw new NotImplementedException();
      }
   }
}