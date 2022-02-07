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

using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Tracking object used to manage the life-cycle of a send of message payload
   /// to the remote which can be stalled either for link or session credit limits.
   /// The envelope carries sufficient information to write payload bytes as credit
   /// is available. The tracking envelope can also accumulate state such as aborted
   /// status for deliveries that are streaming or have stalled waiting on sufficient
   /// credit to be fully sent.
   /// </summary>
   internal sealed class ClientOutgoingEnvelope
   {
      private readonly IProtonBuffer payload;
      private readonly TaskCompletionSource<ITracker> request;
      private readonly ClientAbstractSender sender;
      private readonly uint messageFormat;
      private readonly bool complete;

      // TODO private ScheduledFuture<?> sendTimeout;
      private IOutgoingDelivery delivery;

      /// <summary>
      /// Create a new In-flight Send instance for a complete message send. No further
      /// sends can occur after the send completes however if the send cannot be completed
      /// due to session or link credit issues the send will be requeued at the sender for
      /// retry when the credit is updated by the remote.
      /// </summary>
      /// <param name="sender">The originating sender of the wrapped message payload</param>
      /// <param name="messageFormat">The AMQP message format to encode the transfer</param>
      /// <param name="payload">The encoded message bytes</param>
      /// <param name="request">The request that is linked to this send event</param>
      public ClientOutgoingEnvelope(ClientAbstractSender sender, uint messageFormat, IProtonBuffer payload, TaskCompletionSource<ITracker> request)
      {
         this.messageFormat = messageFormat;
         this.payload = payload;
         this.request = request;
         this.sender = sender;
         this.complete = true;
      }

      /// <summary>
      /// Create a new In-flight Send instance for a complete message send. No further
      /// sends can occur after the send completes however if the send cannot be completed
      /// due to session or link credit issues the send will be requeued at the sender for
      /// retry when the credit is updated by the remote.
      /// </summary>
      /// <param name="sender">The originating sender of the wrapped message payload</param>
      /// <param name="delivery">The proton delivery that is linked to this outgoing write</param>
      /// <param name="messageFormat">The AMQP message format to encode the transfer</param>
      /// <param name="payload">The encoded message bytes</param>
      /// <param name="complete">Is the delivery considered complete as of this transmission</param>
      /// <param name="request">The request that is linked to this send event</param>
      public ClientOutgoingEnvelope(ClientAbstractSender sender, IOutgoingDelivery delivery, uint messageFormat, IProtonBuffer payload, bool complete, TaskCompletionSource<ITracker> request)
      {
         this.messageFormat = messageFormat;
         this.delivery = delivery;
         this.payload = payload;
         this.request = request;
         this.sender = sender;
         this.complete = complete;
      }

      /// <summary>
      /// Indicates if the delivery contained within this envelope is aborted. This does
      /// not transmit the actual aborted status to the remote, the sender must transmit
      /// the contents of the envelope in order to convery the abort.
      /// </summary>
      public bool Aborted { get; set; } = false;

      /// <summary>
      /// The client sender instance that this envelope is linked to
      /// </summary>
      public ClientAbstractSender Sender => sender;

      /// <summary>
      /// The encoded message payload this envelope wraps.
      /// </summary>
      public IProtonBuffer Payload => payload;

      /// <summary>
      /// Returns the proton outgoing delivery object that is contained in this envelope
      /// which can be null if the payload to be sent has not yet had a transmit attempt
      /// due to lack of any credit on the link or in the session window.
      /// </summary>
      public IOutgoingDelivery Delivery => delivery;

      /// <summary>
      /// Performs a send of some or all of the message payload on this outgoing delivery
      /// or possibly an abort if the delivery has already begun streaming and has since
      /// been tagged as aborted.
      /// </summary>
      /// <param name="state">The delivery state to apply</param>
      /// <param name="settled">The settlement value to apply</param>
      public ClientOutgoingEnvelope Transmit(Types.Transport.IDeliveryState state, bool settled)
      {
         if (delivery == null)
         {
            delivery = sender.ProtonSender.Next();
            delivery.LinkedResource = sender.CreateTracker(delivery);
         }

         if (delivery.TransferCount == 0)
         {
            delivery.MessageFormat = messageFormat;
            delivery.Disposition(state, settled);
         }

         // We must check if the delivery was fully written and then complete the send operation otherwise
         // if the session capacity limited the amount of payload data we need to hold the completion until
         // the session capacity is refilled and we can fully write the remaining message payload. This
         // area could use some enhancement to allow control of write and flush when dealing with delivery
         // modes that have low assurance versus those that are strict.
         if (Aborted)
         {
            delivery.Abort();
            Succeeded();
         }
         else
         {
            delivery.StreamBytes(payload, complete);
            if (payload != null && payload.IsReadable)
            {
               sender.AddToHeadOfBlockedQueue(this);
            }
            else
            {
               Succeeded();
            }
         }

         return this;
      }

      public ClientOutgoingEnvelope Discard()
      {
         // TODO
         // if (sendTimeout != null)
         // {
         //    sendTimeout.cancel(true);
         //    sendTimeout = null;
         // }

         if (delivery != null)
         {
            ClientTrackable tracker = (ClientTrackable)delivery.LinkedResource;
            if (tracker != null)
            {
               tracker.CompleteSettlementTask();
            }
            request.TrySetResult(tracker);
         }
         else
         {
            request.TrySetResult(sender.CreateNoOpTracker());
         }

         return this;
      }

      public ClientOutgoingEnvelope Failed(ClientException exception)
      {
         // TODO
         // if (sendTimeout != null)
         // {
         //    sendTimeout.cancel(true);
         // }

         request.TrySetException(exception);

         return this;
      }

      public ClientOutgoingEnvelope Succeeded()
      {
         // TODO
         // if (sendTimeout != null)
         // {
         //    sendTimeout.cancel(true);
         // }

         request.TrySetResult((ITracker)delivery.LinkedResource);

         return this;
      }

      public ClientException CreateSendTimedOutException()
      {
         return new ClientSendTimedOutException("Timed out waiting for credit to send");
      }
   }
}