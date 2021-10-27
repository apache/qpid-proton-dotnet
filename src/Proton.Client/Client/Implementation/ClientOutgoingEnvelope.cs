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
      private readonly Task<ITracker> request;
      private readonly ClientSender sender;
      private readonly bool complete;
      private readonly int messageFormat;

      private bool aborted;
      // TODO private ScheduledFuture<?> sendTimeout;
      private IOutgoingDelivery delivery;

      /// <summary>
      /// Performs a send of some or all of the message payload on this outgoing delivery
      /// or possibly an abort if the delivery has already begun streaming and has since
      /// been tagged as aborted.
      /// </summary>
      /// <param name="state">The delivery state to apply</param>
      /// <param name="settled">The settlement value to apply</param>
      public void SendPayload(Types.Transport.IDeliveryState state, bool settled)
      {
         // TODO
      }
   }
}