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
      new IStreamReceiverMessage Message<Stream>();

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

   }
}