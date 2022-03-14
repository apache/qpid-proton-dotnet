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

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Buffer;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transport;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Client inbound delivery API that wraps the proton resources and
   /// provides API to operate on them.
   /// </summary>
   public class ClientDelivery : IDelivery
   {
      private readonly ClientReceiver receiver;
      private readonly IIncomingDelivery delivery;
      private readonly IProtonBuffer payload;

      private DeliveryAnnotations deliveryAnnotations;
      private ClientMessage<object> cachedMessage;
      private Stream rawInputStream;

      internal ClientDelivery(ClientReceiver receiver, IIncomingDelivery delivery)
      {
         this.receiver = receiver;
         this.delivery = delivery;
         this.delivery.LinkedResource = this;
         this.payload = delivery.ReadAll();
      }

      public IReceiver Receiver => receiver;

      public uint MessageFormat => delivery.MessageFormat;

      public bool Settled => delivery.IsSettled;

      public IDeliveryState State => delivery.State?.ToClientDeliveryState();

      public bool RemoteSettled => delivery.IsRemotelySettled;

      public IDeliveryState RemoteState => delivery.RemoteState?.ToClientDeliveryState();

      public IReadOnlyDictionary<string, object> Annotations
      {
         get
         {
            Message(); // Ensure decode has occurred.

            return ClientConversionSupport.ToStringKeyedMap(deliveryAnnotations?.Value);
         }
      }

      public Stream RawInputStream
      {
         get
         {
            if (cachedMessage != null)
            {
               throw new ClientIllegalStateException("Cannot access Delivery InputStream API after requesting an Message");
            }

            if (rawInputStream == null)
            {
               rawInputStream = new ProtonBufferInputStream(payload);
            }

            return rawInputStream;
         }
      }

      public IMessage<object> Message()
      {
         if (rawInputStream != null)
         {
            throw new ClientIllegalStateException("Cannot access Delivery Annotations API after requesting an InputStream");
         }

         IMessage<object> message = (IMessage<object>)cachedMessage;
         if (message == null && payload.IsReadable)
         {
            message = (IMessage<object>)(cachedMessage = ClientMessageSupport.DecodeMessage(payload, SetDeliveryAnnotations));
         }

         return message;
      }

      public IDelivery Disposition(IDeliveryState state, bool settled)
      {
         return receiver.DispositionAsync(this, state?.AsProtonType(), settled).GetAwaiter().GetResult();
      }

      public IDelivery Settle()
      {
         return receiver.DispositionAsync(this, null, true).GetAwaiter().GetResult();
      }

      public IDelivery Accept()
      {
         return receiver.DispositionAsync(this, Accepted.Instance, true).GetAwaiter().GetResult();
      }

      public IDelivery Modified(bool deliveryFailed, bool undeliverableHere)
      {
         return receiver.DispositionAsync(this, new Modified(deliveryFailed, undeliverableHere), true).GetAwaiter().GetResult();
      }

      public IDelivery Reject(string condition, string description)
      {
         return receiver.DispositionAsync(this, new Rejected(new ErrorCondition(condition, description)), true).GetAwaiter().GetResult();
      }

      public IDelivery Release()
      {
         return receiver.DispositionAsync(this, Released.Instance, true).GetAwaiter().GetResult();
      }

      public Task<IDelivery> AcceptAsync()
      {
         return receiver.DispositionAsync(this, Accepted.Instance, true);
      }

      public Task<IDelivery> ReleaseAsync()
      {
         return receiver.DispositionAsync(this, Released.Instance, true);
      }

      public Task<IDelivery> RejectAsync(string condition, string description)
      {
         return receiver.DispositionAsync(this, new Rejected(new ErrorCondition(condition, description)), true);
      }

      public Task<IDelivery> ModifiedAsync(bool deliveryFailed, bool undeliverableHere)
      {
         return receiver.DispositionAsync(this, new Modified(deliveryFailed, undeliverableHere), true);
      }

      public Task<IDelivery> DispositionAsync(IDeliveryState state, bool settled)
      {
         return receiver.DispositionAsync(this, state?.AsProtonType(), settled);
      }

      public Task<IDelivery> SettleAsync()
      {
         return receiver.DispositionAsync(this, null, true);
      }

      #region Internal API for client objects

      internal IIncomingDelivery ProtonDelivery => delivery;

      internal void SetDeliveryAnnotations(DeliveryAnnotations annotations)
      {
         this.deliveryAnnotations = annotations;
      }

      #endregion
   }
}