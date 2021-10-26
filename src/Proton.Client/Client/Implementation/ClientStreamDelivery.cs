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
using System.IO;
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public class ClientStreamDelivery : IStreamDelivery
   {
      private readonly ClientStreamReceiver receiver;
      private readonly IIncomingDelivery protonDelivery;

      private ClientStreamReceiverMessage message;
      // TODO private RawDeliveryInputStream rawInputStream;

      internal ClientStreamDelivery(ClientStreamReceiver receiver, IIncomingDelivery protonDelivery)
      {
         this.receiver = receiver;
         this.protonDelivery = protonDelivery;
         this.protonDelivery.LinkedResource = this;

         // Capture inbound events and route to an active stream or message
         protonDelivery.DeliveryReadHandler(HandleDeliveryRead)
                       .DeliveryAbortedHandler(HandleDeliveryAborted);
      }

      public IStreamReceiver Receiver => receiver;

      public uint MessageFormat => throw new NotImplementedException();

      public Stream RawInputStream => throw new NotImplementedException();

      public IReadOnlyDictionary<string, object> Annotations => throw new NotImplementedException();

      public bool Settled => throw new NotImplementedException();

      public IDeliveryState State => throw new NotImplementedException();

      public bool RemoteSettled => throw new NotImplementedException();

      public IDeliveryState RemoteState => throw new NotImplementedException();

      public IStreamDelivery Accept()
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery Disposition(IDeliveryState state, bool settled)
      {
         throw new NotImplementedException();
      }

      public IStreamReceiverMessage Message()
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery Modified(bool deliveryFailed, bool undeliverableHere)
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery Reject(string condition, string description)
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery Release()
      {
         throw new NotImplementedException();
      }

      public IStreamDelivery Settle()
      {
         throw new NotImplementedException();
      }

      #region Hidden methods from IDelivery implemented here

      IReceiver IDelivery.Receiver => receiver;

      IDelivery IDelivery.Accept()
      {
         throw new NotImplementedException();
      }

      IDelivery IDelivery.Disposition(IDeliveryState state, bool settled)
      {
         throw new NotImplementedException();
      }

      IMessage<object> IDelivery.Message()
      {
         throw new NotImplementedException();
      }

      IDelivery IDelivery.Modified(bool deliveryFailed, bool undeliverableHere)
      {
         throw new NotImplementedException();
      }

      IDelivery IDelivery.Reject(string condition, string description)
      {
         throw new NotImplementedException();
      }

      IDelivery IDelivery.Release()
      {
         throw new NotImplementedException();
      }

      IDelivery IDelivery.Settle()
      {
         throw new NotImplementedException();
      }

      #endregion

      #region Internal Stream Delivery API

      internal IIncomingDelivery ProtonDelivery => protonDelivery;

      #endregion

      #region private stream delivery implementation

      private void HandleDeliveryRead(IIncomingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      private void HandleDeliveryAborted(IIncomingDelivery delivery)
      {
         throw new NotImplementedException();
      }

      #endregion
   }
}