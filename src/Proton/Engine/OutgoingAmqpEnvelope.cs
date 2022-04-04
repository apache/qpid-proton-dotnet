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
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Engine
{
   /// <summary>
   /// Frame object that carries an AMQP Performative.
   /// </summary>
   public sealed class OutgoingAmqpEnvelope : PerformativeEnvelope<IPerformative>
   {
      public static readonly byte AmqpFrameType = (byte)0;

      private readonly AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> pool;

      private Action<IPerformative> payloadToLargeHandler = DefaultPayloadToLargeHandler;
      private Action frameWriteCompleteHandler;

      /// <summary>
      /// Creates a new empty incoming performative envelope.
      /// </summary>
      internal OutgoingAmqpEnvelope() : base(AmqpFrameType)
      {
      }

      /// <summary>
      /// Creates a new empty incoming performative envelope backed by the provided pool.
      /// </summary>
      internal OutgoingAmqpEnvelope(AmqpPerformativeEnvelopePool<OutgoingAmqpEnvelope> pool) : base(AmqpFrameType)
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

         payloadToLargeHandler = DefaultPayloadToLargeHandler;
         frameWriteCompleteHandler = null;

         if (pool != null)
         {
            pool.Release(this);
         }
      }

      /// <summary>
      /// Configures a handler to be invoked if the payload that is being transmitted
      /// with this performative is to large to allow encoding the frame within the
      /// maximum configured AMQP frame size limit.
      /// </summary>
      public Action<IPerformative> PayloadToLargeHandler
      {
         set
         {
            if (value == null)
            {
               this.payloadToLargeHandler = DefaultPayloadToLargeHandler;
            }
            else
            {
               this.payloadToLargeHandler = value;
            }
         }
      }

      /// <summary>
      /// Called when the encoder determines that the encoding of the Performative
      /// plus any payload value is to large for a single AMQP frame. The configured
      /// handler should update the performative in preparation for encoding as a
      /// split framed AMQP transfer.
      /// </summary>
      /// <returns>This outgoing performative envelope instance.</returns>
      public OutgoingAmqpEnvelope HandlePayloadToLarge()
      {
         this.payloadToLargeHandler.Invoke(Body);
         return this;
      }

      /// <summary>
      /// Configures a handler to be invoked when a write operation that was handed off
      /// to the I/O layer has completed indicated that a single frame portion of the
      /// payload has been fully written.
      /// </summary>
      public Action FrameWriteCompletionHandler { set => this.frameWriteCompleteHandler = value; }

      /// <summary>
      /// Called by the encoder when the write of a frame that comprises the transfer of the
      /// AMQP performative plus any assigned payload has completed. If the transfer comprises
      /// multiple frame writes this handler should be invoked as each frame is successfully
      /// written by the IO layer.
      /// </summary>
      /// <returns>This outgoing performative envelope instance.</returns>
      public OutgoingAmqpEnvelope HandleOutgoingFrameWriteComplete()
      {
         if (frameWriteCompleteHandler != null)
         {
            frameWriteCompleteHandler.Invoke();
         }

         Release();

         return this;
      }

      /// <summary>
      /// Invoke the correct performative handler event based on the body of this
      /// AMQP performative.
      /// </summary>
      /// <typeparam name="T">The type of context that will be provided to the invocation</typeparam>
      /// <param name="handler">The handle to invoke an event on.</param>
      /// <param name="context">The context to pass to the event invocation</param>
      public void Invoke<T>(IPerformativeHandler<T> handler, T context)
      {
         Body.Invoke(handler, Payload, Channel, context);
      }

      private static void DefaultPayloadToLargeHandler(IPerformative performative)
      {
         throw new ArgumentException(string.Format(
             "Cannot transmit performative {0} with payload larger than max frame size limit", performative));
      }
   }
}