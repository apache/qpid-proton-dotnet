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
using Apache.Qpid.Proton.Types.Security;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Frame object that carries an AMQP Performative.
   /// </summary>
   public sealed class SaslEnvelope : PerformativeEnvelope<ISaslPerformative>
   {
      public static readonly byte SaslFrameType = (byte)1;

      /// <summary>
      /// Creates a new SASL Envelope with the given performative as the body.
      /// </summary>
      /// <param name="performative">The performative to carry</param>
      internal SaslEnvelope(ISaslPerformative performative) : this(performative, null)
      {
      }

      /// <summary>
      /// Creates a new SASL Envelope with the given performative as the body and the
      /// given proton buffer as the payload.
      /// </summary>
      /// <param name="performative">The performative to carry</param>
      internal SaslEnvelope(ISaslPerformative performative, IProtonBuffer payload) : base(SaslFrameType)
      {
         Initialize(performative, 0, payload);
      }

      /// <summary>
      /// Invoke the correct event point in the SASL performative handler based on the
      /// type of SASL performative carried in the body and pass along the provided
      /// context object to the handler.
      /// </summary>
      /// <typeparam name="T">The type of context that will be provided to the invocation</typeparam>
      /// <param name="handler">The handle to invoke an event on.</param>
      /// <param name="context">The context to pass to the event invocation</param>
      public void Invoke<T>(ISaslPerformativeHandler<T> handler, T context)
      {
         Body.Invoke(handler, context);
      }
   }
}
