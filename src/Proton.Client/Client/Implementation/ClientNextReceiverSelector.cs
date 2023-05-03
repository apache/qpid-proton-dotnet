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
using System.Linq;
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;
using Apache.Qpid.Proton.Utilities;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientNextReceiverSelector
   {
      private static readonly string LAST_RETURNED_STATE_KEY = "Last_Returned_State";

      private readonly ArrayDeque<TaskCompletionSource<IReceiver>> pending = new();
      private readonly Random random = new();

      private readonly ClientSession session;

      internal ClientNextReceiverSelector(ClientSession session)
      {
         this.session = session;

         HandleReconnect(); // Same processing works for initialization
      }

      public void NextReceiver(TaskCompletionSource<IReceiver> request, NextReceiverPolicy policy, TimeSpan timeout)
      {
         Statics.RequireNonNull(policy, "The next receiver selection policy cannot be null");

         ClientReceiver result = null;

         switch (policy)
         {
            case NextReceiverPolicy.Random:
               result = SelectRandomReceiver();
               break;
            case NextReceiverPolicy.RoundRobin:
               result = SelectNextAvailable();
               break;
            case NextReceiverPolicy.FirstAvailable:
               result = SelectFirstAvailable();
               break;
            case NextReceiverPolicy.LargestBacklog:
               result = SelectLargestBacklog();
               break;
            case NextReceiverPolicy.SmallestBacklog:
               result = SelectSmallestBacklog();
               break;
            default:
               request.TrySetException(new ClientException("Next receiver called with invalid or unknown policy:" + policy));
               break;
         }

         if (result == null)
         {
            pending.Enqueue(request);
            if (timeout > TimeSpan.Zero && timeout < TimeSpan.MaxValue)
            {
               session.Schedule(() =>
               {
                  pending.Remove(request);
                  request.TrySetResult(null);
               }, timeout);
            }
         }
         else
         {
            // Track last returned to update state for Round Robin next receiver dispatch
            // this effectively ties all policies together in updating the next result from
            // a call that requests the round robin fairness policy.
            session.ProtonSession.Attachments[LAST_RETURNED_STATE_KEY] = result;

            request.TrySetResult(result);
         }
      }

      public void HandleReconnect()
      {
         session.ProtonSession.DeliveryReadHandler(DeliveryReadHandler);
      }

      public void HandleShutdown()
      {
         ClientException cause;

         if (session.IsClosed)
         {
            cause = new ClientIllegalStateException("The Session was explicitly closed", session.FailureCause);
         }
         else if (session.FailureCause != null)
         {
            cause = session.FailureCause;
         }
         else
         {
            cause = new ClientIllegalStateException("The session was closed without a specific error being provided");
         }

         foreach (TaskCompletionSource<IReceiver> request in pending)
         {
            request.TrySetException(cause);
         }

         pending.Clear();
      }

      private void DeliveryReadHandler(IIncomingDelivery delivery)
      {
         // When a new delivery arrives that is completed
         if (!pending.IsEmpty && !delivery.IsPartial && !delivery.IsAborted)
         {
            // We only handle next receiver events for normal client receivers and
            // not for stream receiver types etc.
            if (delivery.Receiver.LinkedResource is ClientReceiver receiver)
            {
               // Track last returned to update state for Round Robin next receiver dispatch
               delivery.Receiver.Session.Attachments[LAST_RETURNED_STATE_KEY] = receiver;

               pending.Dequeue().TrySetResult(receiver);
            }
         }
      }

      private ClientReceiver SelectRandomReceiver()
      {
         IEnumerable<Engine.IReceiver> receivers = session.ProtonSession.Receivers.
            Where(r => r.LinkedResource is ClientReceiver receiver && receiver.GetQueuedDeliveries() > 0);

         Engine.IReceiver receiver = receivers.ElementAtOrDefault(random.Next(0, receivers.Count()));

         return receiver?.LinkedResource as ClientReceiver;
      }

      private ClientReceiver SelectNextAvailable()
      {
         ClientReceiver lastReceiver = session.ProtonSession.Attachments.Get<ClientReceiver>(LAST_RETURNED_STATE_KEY, null);
         ClientReceiver result = null;

         if (lastReceiver != null && !lastReceiver.ProtonReceiver.IsLocallyClosedOrDetached)
         {
            bool foundLast = false;
            foreach (Engine.IReceiver protonReceiver in session.ProtonSession.Receivers)
            {
               if (protonReceiver.LinkedResource is ClientReceiver candidate)
               {
                  if (foundLast)
                  {
                     if (candidate.GetQueuedDeliveries() > 0)
                     {
                        result = candidate;
                     }
                  }
                  else
                  {
                     foundLast = candidate == lastReceiver;
                  }
               }
            }
         }
         else
         {
            session.ProtonSession.Attachments[LAST_RETURNED_STATE_KEY] = null;
         }

         return result ?? SelectFirstAvailable();
      }

      private ClientReceiver SelectFirstAvailable()
      {
         Engine.IReceiver receiver =
            session.ProtonSession.Receivers.Where(
               r => r.LinkedResource is ClientReceiver receiver && receiver.GetQueuedDeliveries() > 0).FirstOrDefault();

         return (ClientReceiver)receiver?.LinkedResource;
      }

      private ClientReceiver SelectLargestBacklog()
      {
         IEnumerable<Engine.IReceiver> receivers = session.ProtonSession.Receivers.
            Where(r => r.LinkedResource is ClientReceiver receiver && receiver.GetQueuedDeliveries() > 0);

         ClientReceiver result = null;

         foreach (Engine.IReceiver receiver in receivers)
         {
            ClientReceiver candidate = (ClientReceiver)receiver.LinkedResource;

            if (result == null || result.GetQueuedDeliveries() < candidate.GetQueuedDeliveries())
            {
               result = candidate;
            }
         }

         return result;
      }

      private ClientReceiver SelectSmallestBacklog()
      {
         IEnumerable<Engine.IReceiver> receivers = session.ProtonSession.Receivers.
            Where(r => r.LinkedResource is ClientReceiver receiver && receiver.GetQueuedDeliveries() > 0);

         ClientReceiver result = null;

         foreach (Engine.IReceiver receiver in receivers)
         {
            ClientReceiver candidate = (ClientReceiver)receiver.LinkedResource;

            if (result == null || result.GetQueuedDeliveries() > candidate.GetQueuedDeliveries())
            {
               result = candidate;
            }
         }

         return result;
      }
   }
}