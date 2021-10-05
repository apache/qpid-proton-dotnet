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

using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Types;

namespace Apache.Qpid.Proton.Engine.Sasl.Client
{
   /// <summary>
   /// Common base for SASL mechanism types that provides basic service to ease
   /// the creation of SASL mechanisms.
   /// </summary>
   public abstract class AbstractMechanism : IMechanism
   {
      protected static readonly IProtonBuffer EMPTY = ProtonByteBufferAllocator.Instance.Allocate(0, 0);

      public abstract Symbol Name { get; }

      public abstract bool IsApplicable(ISaslCredentialsProvider credentials);

      public virtual IProtonBuffer GetChallengeResponse(ISaslCredentialsProvider credentials, IProtonBuffer challenge)
      {
         return EMPTY;
      }

      public virtual IProtonBuffer GetInitialResponse(ISaslCredentialsProvider credentialsProvider)
      {
         return EMPTY;
      }

      public virtual bool IsEnabledByDefault()
      {
         return true;
      }

      public virtual void VerifyCompletion()
      {
         // By default this implementation assumes no constraints on completion.
      }

      public override string ToString()
      {
         return "SASL-" + Name;
      }
   }
}