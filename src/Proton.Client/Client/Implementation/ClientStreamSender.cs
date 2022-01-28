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
using System.Collections.Generic;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Logging;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Implements the stream sender using a stateful current outgoing message that prevents
   /// any sends other than to the current message until the current is completed.
   /// </summary>
   public sealed class ClientStreamSender : ClientAbstractSender, IStreamSender
   {
      private static IProtonLogger LOG = ProtonLoggerFactory.GetLogger<ClientStreamSender>();

      private readonly StreamSenderOptions options;

      internal ClientStreamSender(ClientSession session, StreamSenderOptions options, string senderId, Engine.ISender protonSender)
         : base(session, senderId, protonSender)
      {
         this.options = new StreamSenderOptions(options);
      }

      internal override StreamSenderOptions Options => options;

      public IStreamSenderMessage BeginMessage(IDictionary<string, object> deliveryAnnotations = null)
      {
         CheckClosedOrFailed();
         DeliveryAnnotations annotations = null;
         TaskCompletionSource<IStreamSenderMessage> request = new TaskCompletionSource<IStreamSenderMessage>();

         if (deliveryAnnotations != null)
         {
            annotations = new DeliveryAnnotations(ClientConversionSupport.ToSymbolKeyedMap(deliveryAnnotations));
         }

         ClientSession.Execute(() =>
         {
            if (ProtonSender.Current != null)
            {
               request.TrySetException(new ClientIllegalStateException(
                   "Cannot initiate a new streaming send until the previous one is complete"));
            }
            else
            {
               // Grab the next delivery and hold for stream writes, no other sends
               // can occur while we hold the delivery.
               IOutgoingDelivery streamDelivery = ProtonSender.Next();
               ClientStreamTracker streamTracker = new ClientStreamTracker(this, streamDelivery);

               streamDelivery.LinkedResource = streamTracker;

               request.TrySetResult(new ClientStreamSenderMessage(this, streamTracker, annotations));
            }
         });

         return ClientSession.Request(this, request).Task.GetAwaiter().GetResult();
      }

      protected override ITracker DoSendMessage<T>(IAdvancedMessage<T> message, IDictionary<string, object> deliveryAnnotations, bool waitForCredit)
      {
         throw new NotImplementedException();
      }

      internal override ITracker CreateTracker(IOutgoingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      internal void Abort(IOutgoingDelivery protonDelivery, ClientStreamTracker tracker)
      {
         throw new NotImplementedException();
      }

      internal void SendMessage(ClientStreamSenderMessage clientStreamSenderMessage, IAdvancedMessage<object> streamMessagePacket)
      {
         throw new NotImplementedException();
      }

      internal void Complete(IOutgoingDelivery protonDelivery, ClientStreamTracker tracker)
      {
         throw new NotImplementedException();
      }
   }
}