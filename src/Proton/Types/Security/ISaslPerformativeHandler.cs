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

namespace Apache.Qpid.Proton.Types.Security
{
   /// <summary>
   /// Handler Interface that can be used to implement a visitor pattern
   /// of processing the SASL exchange process.
   /// </summary>
   /// <typeparam name="E">The type of the context used in the processing</typeparam>
   public interface ISaslPerformativeHandler<E>
   {
      void HandleMechanisms(SaslMechanisms saslMechanisms, E context) { }

      void HandleInit(SaslInit saslInit, E context) { }

      void HandleChallenge(SaslChallenge saslChallenge, E context) { }

      void HandleResponse(SaslResponse saslResponse, E context) { }

      void HandleOutcome(SaslOutcome saslOutcome, E context) { }
   }
}