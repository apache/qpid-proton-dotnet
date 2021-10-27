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
using System.IO;

namespace Apache.Qpid.Proton.Client
{
   /// <summary>
   /// A specialized delivery type that is returned from the stream receiver
   /// which can be used to read incoming large messages that are streamed via
   /// multiple incoming AMQP transfer frames.
   /// </summary>
   public interface IStreamDelivery : IDelivery
   {
      /// <inheritdoc cref="IDelivery.Receiver"/>
      new IStreamReceiver Receiver { get; }

      /// <inheritdoc cref="IDelivery.Message"/>
      new IStreamReceiverMessage Message();

      /// <inheritdoc cref="IDelivery.Accept"/>
      new IStreamDelivery Accept();

      /// <inheritdoc cref="IDelivery.Release"/>
      new IStreamDelivery Release();

      /// <inheritdoc cref="IDelivery.Reject(string, string)"/>
      new IStreamDelivery Reject(string condition, string description);

      /// <inheritdoc cref="IDelivery.Modified(bool, bool)"/>
      new IStreamDelivery Modified(bool deliveryFailed, bool undeliverableHere);

      /// <inheritdoc cref="IDelivery.Disposition(IDeliveryState, bool)"/>
      new IStreamDelivery Disposition(IDeliveryState state, bool settled);

      /// <inheritdoc cref="IDelivery.Settle"/>
      new IStreamDelivery Settle();

      #region Defaults methods for hidden IDelivery methods

      IReceiver IDelivery.Receiver => this.Receiver;

      IDelivery IDelivery.Accept()
      {
         return this.Accept();
      }

      IDelivery IDelivery.Disposition(IDeliveryState state, bool settled)
      {
         return this.Disposition(state, settled);
      }

      IMessage<object> IDelivery.Message()
      {
         // TODO
         throw new InvalidOperationException("Cannot conert to base IMessage from stream receiver version");
      }

      IDelivery IDelivery.Modified(bool deliveryFailed, bool undeliverableHere)
      {
         return this.Modified(deliveryFailed, undeliverableHere);
      }

      IDelivery IDelivery.Reject(string condition, string description)
      {
         return this.Reject(condition, description);
      }

      IDelivery IDelivery.Release()
      {
         return this.Release();
      }

      IDelivery IDelivery.Settle()
      {
         return this.Settle();
      }

      #endregion

   }
}