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
using System.Threading.Tasks;
using Apache.Qpid.Proton.Client.Exceptions;
using Apache.Qpid.Proton.Engine;

namespace Apache.Qpid.Proton.Client.Implementation
{
   public sealed class ClientStreamTracker : IStreamTracker
   {
      private readonly ClientStreamSender sender;
      private readonly IOutgoingDelivery delivery;
      private readonly TaskCompletionSource<IStreamTracker> remoteSettlementFuture = new TaskCompletionSource<IStreamTracker>();

      private volatile bool remotelySettled;
      private volatile IDeliveryState remoteDeliveryState;

      internal ClientStreamTracker(ClientStreamSender sender, IOutgoingDelivery delivery)
      {
         this.sender = sender;
         this.delivery = delivery;
         this.delivery.DeliveryStateUpdatedHandler(ProcessDeliveryUpdated);
      }

      internal Engine.IOutgoingDelivery ProtonDelivery => delivery;

      public IStreamSender Sender => sender;

      public bool Settled => delivery.IsSettled;

      public IDeliveryState State => delivery.State?.ToClientDeliveryState();

      public bool RemoteSettled => remotelySettled;

      public IDeliveryState RemoteState => remoteDeliveryState;

      public Task<ITracker> SettlementTask => throw new NotImplementedException();

      public IStreamTracker Disposition(IDeliveryState state, bool settle)
      {
         try
         {
            sender.Disposition(delivery, state?.AsProtonType(), settle);
         }
         finally
         {
            if (settle)
            {
               remoteSettlementFuture.SetResult(this);
            }
         }

         return this;
      }

      public IStreamTracker Settle()
      {
         try
         {
            sender.Disposition(delivery, null, true);
         }
         finally
         {
            remoteSettlementFuture.SetResult(this);
         }

         return this;
      }

      public IStreamTracker AwaitAccepted()
      {
         try
         {
            if (Settled && !RemoteSettled)
            {
               return this;
            }
            else
            {
               remoteSettlementFuture.Task.Wait();

               if (RemoteState != null && RemoteState.IsAccepted)
               {
                  return this;
               }
               else
               {
                  throw new ClientDeliveryStateException("Remote did not accept the sent message", RemoteState);
               }
            }
         }
         catch (Exception exe)
         {
            throw ClientExceptionSupport.CreateNonFatalOrPassthrough(exe);
         }
      }

      public IStreamTracker AwaitAccepted(TimeSpan timeout)
      {
         try
         {
            if (Settled && !RemoteSettled)
            {
               return this;
            }
            else
            {
               if (remoteSettlementFuture.Task.Wait(timeout))
               {
                  if (RemoteState != null && RemoteState.IsAccepted)
                  {
                     return this;
                  }
                  else
                  {
                     throw new ClientDeliveryStateException("Remote did not accept the sent message", RemoteState);
                  }
               }
               else
               {
                  throw new ClientOperationTimedOutException("Timed out waiting for remote Accepted outcome");
               }
            }
         }
         catch (Exception exe)
         {
            throw ClientExceptionSupport.CreateNonFatalOrPassthrough(exe);
         }
      }

      public IStreamTracker AwaitSettlement()
      {
         try
         {
            if (Settled)
            {
               return this;
            }

            return remoteSettlementFuture.Task.Result;
         }
         catch (Exception exe)
         {
            throw ClientExceptionSupport.CreateNonFatalOrPassthrough(exe);
         }
      }

      public IStreamTracker AwaitSettlement(TimeSpan timeout)
      {
         try
         {
            if (Settled)
            {
               return this;
            }
            else if (!remoteSettlementFuture.Task.Wait(timeout))
            {
               throw new ClientOperationTimedOutException("Timed out waiting for remote settlement");
            }

            return this;
         }
         catch (Exception exe)
         {
            throw ClientExceptionSupport.CreateNonFatalOrPassthrough(exe);
         }
      }

      #region Private tracker APIs

      private void ProcessDeliveryUpdated(IOutgoingDelivery delivery)
      {
         remotelySettled = delivery.IsRemotelySettled;
         remoteDeliveryState = delivery.RemoteState?.ToClientDeliveryState();

         if (delivery.IsRemotelySettled)
         {
            remoteSettlementFuture.SetResult(this);
         }

         if (sender.Options.AutoSettle && delivery.IsRemotelySettled)
         {
            delivery.Settle();
         }
      }

      #endregion
   }
}