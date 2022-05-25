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

using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Frame object that carries an AMQP Performative.
   /// </summary>
   public class IncomingAmqpEnvelope : PerformativeEnvelope<IPerformative>
   {
      public static readonly byte AmqpFrameType = 0;

      private readonly AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> pool;

      /// <summary>
      /// Creates a new empty incoming performative envelope.
      /// </summary>
      internal IncomingAmqpEnvelope() : base(AmqpFrameType)
      {
      }

      /// <summary>
      ///
      /// </summary>
      /// <param name="pool">The envelope pool to use when obtaining new envelopes</param>
      internal IncomingAmqpEnvelope(AmqpPerformativeEnvelopePool<IncomingAmqpEnvelope> pool) : base(AmqpFrameType)
      {
         this.pool = pool;
      }

      /// <summary>
      /// Used to release a Frame that was taken from a Frame pool in order
      /// to make it available for the next input operations.  Once called the
      /// contents of the Frame are invalid and cannot be used again inside the
      /// same context.
      /// </summary>
      public void Release()
      {
         Initialize(null, ushort.MaxValue, null);

         if (pool != null)
         {
            pool.Release(this);
         }
      }

      /// <summary>
      /// Invoke the correct performative handler event based on the body of this
      /// AMQP performative.
      /// </summary>
      /// <typeparam name="T">The type of context that will be provided to the invocation</typeparam>
      /// <param name="handler">The handle to invoke an event on.</param>
      /// <param name="context">The context to pass to the event invocation</param>
      public virtual void Invoke<T>(IPerformativeHandler<T> handler, T context)
      {
         Body.Invoke(handler, Payload, Channel, context);
      }
   }
}